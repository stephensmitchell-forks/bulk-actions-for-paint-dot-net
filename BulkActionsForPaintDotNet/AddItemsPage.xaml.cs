using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Collections;
using System.Windows.Media.Animation;

namespace PDNBulkUpdater
{
	/// <summary>
	/// Interaction logic for AddItemsPage.xaml
	/// </summary>
	public partial class AddItemsPage : Page
	{
		UpdateContext m_ctx = new UpdateContext();

		public AddItemsPage()
		{
			InitializeComponent();

			DateTime now = DateTime.Now;
			m_txtOutputDir.Text = m_ctx.OutputDirectory;

			m_comboFileType.Items.Add("Source File Type");

			foreach(PaintDotNet.FileType fileType in m_ctx.FileTypes)
			{
				m_comboFileType.Items.Add(fileType);
			}

			m_comboFileType.SelectedIndex = 0;
            ButtonNext.IsEnabled = false;
		}

		private void OnNextButton_Click(object sender, RoutedEventArgs e)
		{
			string outDir = m_ctx.OutputDirectory;
			bool isFailed = outDir.Length == 0;

			try
			{
				outDir = Util.SanitizePath(outDir, 0);
			}
			catch(Exception)
			{
				isFailed = true;
			}

			if(isFailed)
			{
				MessageBox.Show("The output directory is not a valid path.", outDir, MessageBoxButton.OK, MessageBoxImage.Stop);
				m_txtOutputDir.Focus();
				return;
			}
			else if(InputValidator.GetHasErrors(m_txtRenameFiles))
			{
				MessageBox.Show("\'Rename Files To:\' does not contain a valid file name.", m_txtRenameFiles.Text, MessageBoxButton.OK, MessageBoxImage.Stop);
				m_txtRenameFiles.Focus();
				return;
			}

			if(!System.IO.Directory.Exists(outDir))
			{
				if(MessageBox.Show("The output directory does not exist. If you continue it will be automatically created.", m_ctx.OutputDirectory, MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel)
				{
					return;
				}
			}

			if(NavigationService.CanGoForward)
			{
				NavigationService.GoForward();
			}
			else
			{
				NavigationService.Navigate(new ImageResizePage(m_ctx));
			}
		}

		private void OnBackButton_Click(object sender, RoutedEventArgs e)
		{
			NavigationService.GoBack();
		}

		private void OnBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			using(System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog())
			{
				dlg.Description = "Please locate the output directory.";
				dlg.RootFolder = Environment.SpecialFolder.MyComputer;
				dlg.SelectedPath = m_txtOutputDir.Text;
				dlg.ShowNewFolderButton = true;

				if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					m_txtOutputDir.Text = dlg.SelectedPath;
                    m_ctx.OutputDirectory = dlg.SelectedPath;
                }
			}
		}

		private void OnAddImagesButton_Click(object sender, RoutedEventArgs e)
		{
			FileTypeCollection fileTypes = m_ctx.FileTypes;

			StringBuilder filter = new StringBuilder("All Supported Images|");

			foreach(string ext in fileTypes.AllExtensions)
			{
				filter.Append('*');
				filter.Append(ext);
				filter.Append(';');
			}

			filter.Length = filter.Length - 1;

			using(System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
			{
				dlg.AddExtension = true;
				dlg.AutoUpgradeEnabled = true;
				dlg.CheckFileExists = true;
				dlg.CheckPathExists = true;
				dlg.Filter = filter.ToString();
				dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				dlg.Multiselect = true;
				dlg.RestoreDirectory = true;
				dlg.SupportMultiDottedExtensions = true;
				dlg.Title = "Images to Update";

				if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    AddFilesToViewItemsList(dlg.FileNames);
			}
		}

        private void AddFilesToViewItemsList(string[] files)
        {
            foreach (string path in files)
                AddFilesToViewItemsList(path);
        }

        private void AddFilesToViewItemsList(string path)
        {
            FileSystemItem file;
            if (IsValidFileType(path, m_ctx.FileTypes) == false)
                return;

            if (System.IO.Directory.Exists(path))
                file = new FileSystemDirectory(path, m_ctx.FileTypes);
            else // File
                file = new FileSystemFile(path, m_ctx.FileTypes);

            if (!m_ctx.Files.Contains(file))
            {
                m_ctx.Files.Add(file);
                m_viewItems.Items.Add(file);
            }

            SelectedFilesUpdated();
        }

        private void SelectedFilesUpdated()
        {
            if (m_ctx.Files.Count > 0)
                ButtonNext.IsEnabled = true;
            else
                ButtonNext.IsEnabled = false;
        }

        private static bool IsValidFileType(string path, FileTypeCollection validFilesTypes)
        {
            if (String.IsNullOrEmpty(path))
                return false;

            if (System.IO.Directory.Exists(path))
                return true;

            for (int i = 0; i < validFilesTypes.Length; i++)
                foreach (string ext in validFilesTypes[i].Extensions)
                    if (path.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
                        return true;

            return false;
        }

        private void OnAddFolderButton_Click(object sender, RoutedEventArgs e)
		{
			using(System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog())
			{
				dlg.Description = "Please locate a folder to update.";
				dlg.RootFolder = Environment.SpecialFolder.MyComputer;
				dlg.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				dlg.ShowNewFolderButton = false;

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    AddFilesToViewItemsList(dlg.SelectedPath);
			}
		}

        private void OnViewItems_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (fileNames != null)
                {
                    bool foundValidFile = false;
                    foreach (string fileName in fileNames)
                    {
                        foundValidFile = foundValidFile || IsValidFileType(fileName, m_ctx.FileTypes);
                        if (foundValidFile == true)
                            break;
                    }

                    if (foundValidFile == true)
                        e.Effects = DragDropEffects.Copy;
                }
            }

            e.Handled = true;
        }

        private void OnViewItems_Drop(object sender, DragEventArgs e)
        {
            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (fileNames != null)
                {
                    AddFilesToViewItemsList(fileNames);
                    e.Effects = DragDropEffects.Copy;
                }
            }
            e.Handled = true;
        }

        private void OnViewItems_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Delete)
			{
				ArrayList items = new ArrayList(m_viewItems.SelectedItems);
				int selectedIndex = m_viewItems.SelectedIndex;

				foreach(FileSystemItem item in items)
				{
					m_ctx.Files.Remove(item);
					m_viewItems.Items.Remove(item);
				}

				if(selectedIndex != -1 && m_viewItems.Items.Count > 0)
				{
					m_viewItems.SelectedIndex = Math.Min(selectedIndex, m_viewItems.Items.Count - 1);
				}

                SelectedFilesUpdated();

				e.Handled = true;
			}
		}

		private void OnClearButton_Click(object sender, RoutedEventArgs e)
		{
			m_ctx.Files.Clear();
			m_viewItems.Items.Clear();

            SelectedFilesUpdated();
		}

		private void OnPath_ToolTipOpening(object sender, ToolTipEventArgs e)
		{
			TextBox box = (TextBox)sender;

			try
			{
				box.ToolTip = Util.SanitizePath(box.Text, 0);
			}
			catch(Exception)
			{
				box.ToolTip = "Invalid file name";
			}
		}

		private void OnViewItems_ToolTipOpening(object sender, ToolTipEventArgs e)
		{
			m_viewItems.ToolTip = m_viewItems.Items.Count.ToString() + " items";
		}

		private void OnValidateTxtBox_LostFocus(object sender, RoutedEventArgs e)
		{
			TextBox box = sender as TextBox;

			if(box != null)
			{
				bool hasErrors = true;

				try
				{
					Util.SanitizePath(box.Text, 0);
					hasErrors = false;
				}
				catch(Exception)
				{
				}

				InputValidator.SetHasErrors(box, hasErrors);

				if(box == m_txtOutputDir)
				{
					m_ctx.OutputDirectory = box.Text;
				}
				else if(box == m_txtRenameFiles)
				{
					m_ctx.RenameFiles = m_txtRenameFiles.Text;
				}
			}
		}

		private void OnComboFileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			m_ctx.OutputFileType = e.AddedItems[0] as PaintDotNet.FileType;
		}
    }
}
