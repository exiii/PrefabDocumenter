using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using PrefabDocumenter.Xml;
using PrefabDocumenter.Db;
using CenterCLR;

namespace PrefabDocumenter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private CommonFileDialogFilter xmlCommonFilter = new CommonFileDialogFilter("XML", "xml");
        private CommonFileDialogFilter htmlCommonFilter = new CommonFileDialogFilter("HTML", "html");
        private CommonFileDialogFilter dbCommonFilter = new CommonFileDialogFilter("DB", "db");

        private XElement loadFileTreeRootElement;
        private XElement loadDraftDocRootElement;

        public MainWindow()
        {
            InitializeComponent();
        }

        //<-MainWindow.xaml call functions
        private void TargetFolderPathInject(object sender, RoutedEventArgs e)
        {
            var pathOption = Optional.Return(FileDialog.Open(new CommonOpenFileDialog(Properties.Resources.SaveFolderSelectDialogTitle) { IsFolderPicker = true }));

            pathOption.Match(
                value => TargetFolderPath.Text = value,
                () => { });
        }

        private async void CreateTreeFile(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TargetFolderPath.Text) == false)
            {
                MessageBox.Show(Properties.Resources.IncorrectTargetFolderPath, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var pathOption = Optional.Return(FileDialog.Open(new CommonSaveFileDialog(), xmlCommonFilter));

            pathOption.Match(async value => {
                using (var fs = new FileStream(value, FileMode.Create))
                {
                    ToggleAllButtonEnabled(false);

                    var xDoc = await FileTreeXml.CreateXElement(TargetFolderPath.Text, FileNameRegex.Text);

                    await Task.Run(() => {
                        xDoc.Save(fs);
                    });

                    fs.Close();
                    fs.Dispose();

                    UpdateMetaFileTree(xDoc);
                    ToggleAllButtonEnabled(true);
                }
            }, 
            () => { });
        }

        private async void LoadXmlFile(object sender, RoutedEventArgs e)
        {
            var pathOption = Optional.Return(FileDialog.Open(new CommonOpenFileDialog(), xmlCommonFilter));

            pathOption.Match(async value => {
                if (!File.Exists(value))
                {
                    MessageBox.Show(Properties.Resources.IncorrectPath, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ToggleAllButtonEnabled(false);

                var xDoc = new XDocument();
                await Task.Run(() => {
                    xDoc = XDocument.Load(value);
                });

                UpdateMetaFileTree(xDoc);

                ToggleAllButtonEnabled(true);
            },
            () => { });
        }

        private async void LoadDraftDocument(object sender, RoutedEventArgs e)
        {
            var pathOption = Optional.Return(FileDialog.Open(new CommonOpenFileDialog(), xmlCommonFilter));

            pathOption.Match(async value =>
            {
                ToggleAllButtonEnabled(false);

                var xDoc = new XDocument();
                await Task.Run(() =>
                {
                    xDoc = XDocument.Load(value);
                });

                UpdateDraftDocTree(xDoc);

                ToggleAllButtonEnabled(true);
            }, 
            () => { });

        }

        private async void CreateDraftDocument(object sender, RoutedEventArgs e)
        {
            if (loadFileTreeRootElement == null)
            {
                MessageBox.Show(Properties.Resources.FileTreeXMLNotLoaded, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var pathOption = Optional.Return(FileDialog.Open(new CommonSaveFileDialog(), xmlCommonFilter));

            pathOption.Match(async value => {
                using (var fs = new FileStream(value, FileMode.Create))
                {
                    ToggleAllButtonEnabled(false);

                    var xDoc = await XmlDocument.CreateDraftDocument(loadFileTreeRootElement.DescendantsAndSelf().Where(element => element.Attribute(XmlTags.GuidAttr) != null));

                    await Task.Run(() => {
                        xDoc.Save(fs);
                    });

                    fs.Close();
                    fs.Dispose();

                    UpdateDraftDocTree(xDoc);

                    ToggleAllButtonEnabled(true);
                }
            },
            () => { });


        }

        private async void UpdateDraftDocument(object sender, RoutedEventArgs e)
        {
            if (loadFileTreeRootElement == null)
            {
                MessageBox.Show(Properties.Resources.FileTreeXMLNotLoaded, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var pathOption = Optional.Return(FileDialog.Open(new CommonOpenFileDialog(), xmlCommonFilter));

            pathOption.Match(async value => {
                ToggleAllButtonEnabled(false);

                var xDoc = new XDocument();
                await Task.Run(() => {
                    xDoc = XDocument.Load(value);
                });

                xDoc = await XmlDocument.UpdateDraftDocument(loadFileTreeRootElement.Elements(), xDoc.Elements());

                using (var fs = new FileStream(value, FileMode.Create))
                {
                    await Task.Run(() => {
                        xDoc.Save(fs);
                    });

                    fs.Close();
                    fs.Dispose();
                }

                UpdateDraftDocTree(xDoc);

                ToggleAllButtonEnabled(true);
            },
            () => { });
        }

        private async void CreateDbDocument(object sender, RoutedEventArgs e)
        {
            if (loadDraftDocRootElement == null || loadFileTreeRootElement == null)
            {
                MessageBox.Show(Properties.Resources.FileTreeAndDraftNotLoaded, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var pathOption = Optional.Return(FileDialog.Open(new CommonSaveFileDialog(), xmlCommonFilter));

            pathOption.Match(async value => {
                var sqlProvider = new SqlDbProvider<PrefabDocumentModel>(value);

                var models = await PrefabDocumentModel.CreateXmlToModel(loadDraftDocRootElement, loadFileTreeRootElement);

                sqlProvider.InitTable();

                sqlProvider.Inserts(models);
            },
            () => { });
        }

        private void UpdateMetaFileTree(XDocument xDoc)
        {
            metaFileTree.Items.Refresh();
            metaFileTree.ItemsSource = xDoc.Root.Elements();
            loadFileTreeRootElement = xDoc.Root;
        }

        private void UpdateDraftDocTree(XDocument xDoc)
        {
            draftTreeView.Items.Refresh();
            draftTreeView.ItemsSource = xDoc.Root.Elements();
            loadDraftDocRootElement = xDoc.Root;
        }

        private void ToggleAllButtonEnabled(bool isEnabled)
        {
            var buttons = MainGrid.Children.OfType<Button>();

            foreach (var button in buttons)
            {
                button.IsEnabled = isEnabled;
            }
        }
    }

    internal static class FileDialog
    {
        internal static string Open(CommonFileDialog Dialog, CommonFileDialogFilter Filter = null)
        {
            if(Filter != null)
            {
                Dialog.Filters.Add(Filter);
                //Dialog.AlwaysAppendDefaultExtension = true;
                Dialog.DefaultExtension = Filter.Extensions.First();
            }

            var result = Dialog.ShowDialog();

            switch (result)
            {
                case CommonFileDialogResult.Ok:
                    return Dialog.FileName;
            }

            return null;
        }
    }
}
