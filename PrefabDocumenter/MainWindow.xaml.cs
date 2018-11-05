using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using exiii;
using AngleSharp.Dom.Html;
using AngleSharp.Html;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace PrefabDocumenter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string xmlFilter = "XML(*.xml)|*.xml|全てのファイル (*.*)|*.*";
        private const string htmlFilter = "html(*.html)|*.html|全てのファイル (*.*)|*.*";
        private const string dbFilter = "db(*.db)|*.db|全てのファイル (*.*)|*.*";

        private XElement loadFileTreeRootElement;
        private XElement loadDraftDocRootElement;

        public MainWindow()
        {
            InitializeComponent();
        }

        //<-MainWindow.xaml call functions
        private void TargetFolderPathInject(object sender, RoutedEventArgs e)
        {
            TargetFolderPath.Text = FileDialog.OpenFolderDialog();
        }

        private async void CreateTreeFile(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TargetFolderPath.Text) == false)
            {
                MessageBox.Show("Target folder pathに正しいディレクトリを入力してください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = FileDialog.SaveFileDialog(xmlFilter, out var path);
            if (!result)
            {
                MessageBox.Show("正しいパスを入力してください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var fs = new FileStream(path, FileMode.Create))
            {
                toggleAllButtonEnabled(false);

                var xDoc = await FileTreeXml.CreateXElement(TargetFolderPath.Text);

                await Task.Run(() => {
                    xDoc.Save(fs);
                });

                fs.Close();
                fs.Dispose();

                updateMetaFileTree(xDoc);
                toggleAllButtonEnabled(true);
            }
        }

        private async void LoadXmlFile(object sender, RoutedEventArgs e)
        {

            var result = FileDialog.OpenFileDialog(xmlFilter, out var path);
            if (!result)
            {
                MessageBox.Show("正しいファイルを入力してください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            toggleAllButtonEnabled(false);

            var xDoc = new XDocument();
            await Task.Run(() => {
                xDoc = XDocument.Load(path);
            });

            updateMetaFileTree(xDoc);

            toggleAllButtonEnabled(true);
        }

        private async void LoadDraftDocument(object sender, RoutedEventArgs e)
        {
            var result = FileDialog.OpenFileDialog(xmlFilter, out var path);
            if (!result)
            {
                MessageBox.Show("正しいファイルを入力してください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            toggleAllButtonEnabled(false);

            var xDoc = new XDocument();
            await Task.Run(() => {
                xDoc = XDocument.Load(path); ;
            });

            updateDraftDocTree(xDoc);

            toggleAllButtonEnabled(true);
        }

        private async void CreateDraftDocument(object sender, RoutedEventArgs e)
        {
            if (loadFileTreeRootElement == null)
            {
                MessageBox.Show("File tree xmlを読み込んでください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = FileDialog.SaveFileDialog(xmlFilter, out var path);
            if (!result)
            {
                MessageBox.Show("正しいパスを入力してください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var fs = new FileStream(path, FileMode.Create))
            {
                toggleAllButtonEnabled(false);

                var xDoc = await XmlDocument.CreateDraftDocument(loadFileTreeRootElement.DescendantsAndSelf().Where(element => element.Attribute("Guid") != null));

                await Task.Run(() => {
                    xDoc.Save(fs);
                });

                fs.Close();
                fs.Dispose();

                updateDraftDocTree(xDoc);

                toggleAllButtonEnabled(true);
            }
        }

        private async void UpdateDraftDocument(object sender, RoutedEventArgs e)
        {
            if (loadFileTreeRootElement == null)
            {
                MessageBox.Show("File tree xmlを読み込んでください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = FileDialog.OpenFileDialog(xmlFilter, out var path);
            if (!result)
            {
                MessageBox.Show("正しいファイルを選択してください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            toggleAllButtonEnabled(false);

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

            updateDraftDocTree(xDoc);

            toggleAllButtonEnabled(true);
        }

        private async void CreateDbDocument(object sender, RoutedEventArgs e)
        {
            if (loadDraftDocRootElement == null || loadFileTreeRootElement == null)
            {
                MessageBox.Show("File tree xmlとDraft documentを読み込んでください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = FileDialog.SaveFileDialog(dbFilter, out var path);
            if (!result)
            {
                MessageBox.Show("正しいパスを入力してください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sqlProvider = new SqlDbProvider<PrefabDocumentModel>(path);

            var models = await PrefabDocumentModel.CreateXmlToModel(loadDraftDocRootElement, loadFileTreeRootElement);

            sqlProvider.InitTable();

            sqlProvider.Inserts(models);
        }

        private void updateMetaFileTree(XDocument xDoc)
        {
            metaFileTree.Items.Refresh();
            metaFileTree.ItemsSource = xDoc.Root.Elements();
            loadFileTreeRootElement = xDoc.Root;
        }

        private void updateDraftDocTree(XDocument xDoc)
        {
            draftTreeView.Items.Refresh();
            draftTreeView.ItemsSource = xDoc.Root.Elements();
            loadDraftDocRootElement = xDoc.Root;
        }

        private void toggleAllButtonEnabled(bool isEnabled)
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
        internal static bool OpenFileDialog(string filter, out string FileName)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Filter = filter;
            dialog.CheckFileExists = true;

            if (dialog.ShowDialog() == false)
            {
                FileName = "";
                return false;
            }

            FileName = dialog.FileName;
            return true;
        }

        internal static bool SaveFileDialog(string filter, out string FileName)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();

            dialog.Filter = filter;
            dialog.CheckPathExists = true;

            if (dialog.ShowDialog() == false)
            {
                FileName = "";
                return false;
            }

            FileName = dialog.FileName;
            return true;
        }

        internal static string OpenFolderDialog()
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog("保存フォルダ選択");
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                return "";
            }

            return dialog.FileName;
        }
    }
}
