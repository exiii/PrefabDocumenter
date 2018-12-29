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
            FileDialog.Open(new CommonOpenFileDialog(Properties.Resources.SaveFolderSelectDialogTitle) { IsFolderPicker = true }, out var path);
            TargetFolderPath.Text = path;
        }

        private async void CreateTreeFile(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TargetFolderPath.Text) == false)
            {
                MessageBox.Show(Properties.Resources.IncorrectTargetFolderPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = FileDialog.Open(new CommonSaveFileDialog(), out var path, xmlCommonFilter);
            if (!result)
            {
                return;
            }

            using (var fs = new FileStream(path, FileMode.Create))
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
        }

        private async void LoadXmlFile(object sender, RoutedEventArgs e)
        {

            var result = FileDialog.Open(new CommonOpenFileDialog(), out var path, xmlCommonFilter);
            if (!result)
            {
                return;
            }

            if (File.Exists(path))
            {
                MessageBox.Show(Properties.Resources.IncorrectFile, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ToggleAllButtonEnabled(false);

            var xDoc = new XDocument();
            await Task.Run(() => {
                xDoc = XDocument.Load(path);
            });

            UpdateMetaFileTree(xDoc);

            ToggleAllButtonEnabled(true);
        }

        private async void LoadDraftDocument(object sender, RoutedEventArgs e)
        {
            var result = FileDialog.Open(new CommonOpenFileDialog(), out var path, xmlCommonFilter);
            if (!result)
            {
                return;
            }

            ToggleAllButtonEnabled(false);

            var xDoc = new XDocument();
            await Task.Run(() => {
                xDoc = XDocument.Load(path); ;
            });

            UpdateDraftDocTree(xDoc);

            ToggleAllButtonEnabled(true);
        }

        private async void CreateDraftDocument(object sender, RoutedEventArgs e)
        {
            if (loadFileTreeRootElement == null)
            {
                MessageBox.Show(Properties.Resources.FileTreeXMLNotLoad, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = FileDialog.Open(new CommonSaveFileDialog(), out var path, xmlCommonFilter);
            if (!result)
            {
                return;
            }

            using (var fs = new FileStream(path, FileMode.Create))
            {
                ToggleAllButtonEnabled(false);

                var xDoc = await XmlDocument.CreateDraftDocument(loadFileTreeRootElement.DescendantsAndSelf().Where(element => element.Attribute(XmlTags.GuidAttrTag) != null));

                await Task.Run(() => {
                    xDoc.Save(fs);
                });

                fs.Close();
                fs.Dispose();

                UpdateDraftDocTree(xDoc);

                ToggleAllButtonEnabled(true);
            }
        }

        private async void UpdateDraftDocument(object sender, RoutedEventArgs e)
        {
            if (loadFileTreeRootElement == null)
            {
                MessageBox.Show(Properties.Resources.FileTreeXMLNotLoad, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = FileDialog.Open(new CommonOpenFileDialog(), out var path, xmlCommonFilter);
            if (!result)
            {
                return;
            }

            ToggleAllButtonEnabled(false);

            var xDoc = new XDocument();
            await Task.Run(() => {
                xDoc = XDocument.Load(path);
            });

            xDoc = await XmlDocument.UpdateDraftDocument(loadFileTreeRootElement.Elements(), xDoc.Elements());

            using (var fs = new FileStream(path, FileMode.Create))
            {
                await Task.Run(() => {
                    xDoc.Save(fs);
                });

                fs.Close();
                fs.Dispose();
            }

            UpdateDraftDocTree(xDoc);

            ToggleAllButtonEnabled(true);
        }

        private async void CreateDbDocument(object sender, RoutedEventArgs e)
        {
            if (loadDraftDocRootElement == null || loadFileTreeRootElement == null)
            {
                MessageBox.Show(Properties.Resources.FileTreeAndDraftNotLoad, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = FileDialog.Open(new CommonSaveFileDialog(), out var path, dbCommonFilter);
            if (!result)
            {
                return;
            }

            var sqlProvider = new SqlDbProvider<PrefabDocumentModel>(path);

            var models = await PrefabDocumentModel.CreateXmlToModel(loadDraftDocRootElement, loadFileTreeRootElement);

            sqlProvider.InitTable();

            sqlProvider.Inserts(models);
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
        internal static bool Open(CommonFileDialog Dialog, out string FileName, CommonFileDialogFilter Filter = null)
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
                    FileName = Dialog.FileName;
                    return true;
            }

            FileName = "";
            return false;
        }
    }
}
