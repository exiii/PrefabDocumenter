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
            FileDialog.OpenSaveFolderDialog(out var path);
            TargetFolderPath.Text = path;
        }

        private async void CreateTreeFile(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TargetFolderPath.Text) == false)
            {
                MessageBox.Show(Properties.Resources.IncorrectTargetFolderPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = FileDialog.SaveFileDialog(xmlCommonFilter, out var path);
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

            var result = FileDialog.OpenFileDialog(xmlCommonFilter, out var path);
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
            var result = FileDialog.OpenFileDialog(xmlCommonFilter, out var path);
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

            var result = FileDialog.SaveFileDialog(xmlCommonFilter, out var path);
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

            var result = FileDialog.OpenFileDialog(xmlCommonFilter, out var path);
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

            var result = FileDialog.SaveFileDialog(dbCommonFilter, out var path);
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
        internal static bool OpenFileDialog(CommonFileDialogFilter filter, out string FileName)
        {
            var dialog = new CommonOpenFileDialog();

            dialog.Filters.Add(filter);

            var result = dialog.ShowDialog();

            switch (result)
            {
                case CommonFileDialogResult.None:
                    MessageBox.Show(Properties.Resources.IncorrectFile, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    FileName = "";
                    return false;
                case CommonFileDialogResult.Cancel:
                    FileName = "";
                    return false;
            }

            FileName = dialog.FileName;
            return true;
        }

        internal static bool SaveFileDialog(CommonFileDialogFilter filter, out string FileName)
        {
            var dialog = new CommonSaveFileDialog();

            dialog.Filters.Add(filter);
            dialog.AlwaysAppendDefaultExtension = true;
            dialog.DefaultExtension = filter.Extensions.First();

            var result = dialog.ShowDialog();

            switch (result)
            {
                case CommonFileDialogResult.None:
                    MessageBox.Show(Properties.Resources.IncorrectPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    FileName = "";
                    return false;
                case CommonFileDialogResult.Cancel:
                    FileName = "";
                    return false;
            }

            FileName = dialog.FileName;
            return true;
        }

        internal static bool OpenSaveFolderDialog(out string FileName)
        {
            var dialog = new CommonOpenFileDialog(Properties.Resources.SaveFolderSelectDialogTitle) {
                IsFolderPicker = true
            };

            var result = dialog.ShowDialog();

            switch (result)
            {
                case CommonFileDialogResult.None:
                    MessageBox.Show(Properties.Resources.IncorrectPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    FileName = "";
                    return false;
                case CommonFileDialogResult.Cancel:
                    FileName = "";
                    return false;
            }

            FileName = dialog.FileName;
            return true;
        }
    }
}
