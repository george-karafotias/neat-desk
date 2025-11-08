using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace NeatDesk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileConfiguration fileConfiguration;
        private TextBoxLogger textBoxLogger;
        
        public MainWindow()
        {
            InitializeComponent();
            textBoxLogger = new TextBoxLogger(LogTextBox);
            fileConfiguration = new FileConfiguration();
            PopulateCategoriesComboBox(fileConfiguration.FileCategories);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a folder to organize";
                dialog.ShowNewFolderButton = true;

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    FolderPathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private async void OrganizeFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string folder = FolderPathTextBox.Text;
            CustomizeGrid.Visibility = Visibility.Collapsed;
            LogTextBox.Visibility = Visibility.Visible;
            await Task.Run(() => OrganizeFiles(folder));
        }

        private void CutomizeButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Visibility = Visibility.Collapsed;
            CustomizeGrid.Visibility = Visibility.Visible;
        }

        private void SelectCategoryNameComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string selectedCategoryName = SelectCategoryNameComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedCategoryName)) return;

            FileCategory selectedCategory = fileConfiguration.GetFileCategory(selectedCategoryName);
            if (selectedCategory == null) return;

            PopulateFileExtensions(selectedCategory, ChangeCategoryValuesTextBox);
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AddCategoryNameTextBox.Text) || AddCategoryNameTextBox.Text.Trim().Length == 0)
            {
                ShowErrorMessage("Please enter a name for the new category.");
                return;
            }

            List<string> fileExtensions = GetFileExtensions(AddCategoryValuesTextBox);
            if (fileExtensions == null || fileExtensions.Count == 0)
            {
                ShowErrorMessage("Cannot add a category that does not have any file extensions.\nMake sure that every file extension is defined on a different line.");
                return;
            }

            string categoryName = AddCategoryNameTextBox.Text.Trim();
            if (fileConfiguration.AddFileCategory(categoryName, fileExtensions))
            {
                ShowInfoMessage("The category " + categoryName + " has been added successfully.");
                PopulateCategoriesComboBox(fileConfiguration.FileCategories);
                AddCategoryNameTextBox.Clear();
                AddCategoryValuesTextBox.Clear();
            }
            else
            {
                ShowErrorMessage("Your changes cannot be saved. Please try again.");
            }
        }

        private void SaveCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedCategoryName = SelectCategoryNameComboBox.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(selectedCategoryName))
            {
                ShowErrorMessage("Please select a category first.");
                return;
            }

            if (string.IsNullOrEmpty(ChangeCategoryValuesTextBox.Text) || ChangeCategoryValuesTextBox.Text.Trim().Length == 0)
            {
                ShowErrorMessage("Cannot save a category that does not have any file extensions.");
                return;
            }

            List<string> fileExtensions = GetFileExtensions(ChangeCategoryValuesTextBox);
            if (fileExtensions == null || fileExtensions.Count == 0)
            {
                ShowErrorMessage("Cannot save a category that does not have any file extensions.\nMake sure that every file extension is defined on a different line.");
                return;
            }

            if (fileConfiguration.UpdateFileCategory(selectedCategoryName, fileExtensions))
            {
                ShowInfoMessage("Your changes have been saved successfully.");
            }
            else
            {
                ShowErrorMessage("Your changes cannot be saved. Please try again.");
            }
        }

        private void DeleteCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedCategoryName = SelectCategoryNameComboBox.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(selectedCategoryName))
            {
                ShowErrorMessage("Please select a category first.");
                return;
            }

            if (fileConfiguration.DeleteFileCategory(selectedCategoryName))
            {
                ShowInfoMessage("The category has been deleted successfully.");
                PopulateCategoriesComboBox(fileConfiguration.FileCategories);
                ChangeCategoryValuesTextBox.Clear();
            }
            else
            {
                ShowErrorMessage("The category cannot be deleted. Please try again.");
            }
        }

        private void ShowInfoMessage(string message)
        {
            System.Windows.MessageBox.Show(message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowErrorMessage(string message)
        {
            System.Windows.MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private List<string> GetFileExtensions(System.Windows.Controls.TextBox textBox)
        {
            string[] fileExtensions = textBox.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (fileExtensions != null && fileExtensions.Length > 0)
            {
                return fileExtensions.ToList();
            }
            return null;
        }

        private void OrganizeFiles(string folder)
        {
            FileOrganizer fileOrganizer = new FileOrganizer(new List<ILogger> { textBoxLogger });
            FileOrganizerResult result = fileOrganizer.OrganizeFiles(folder, fileConfiguration);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (result.Success)
                {
                    System.Windows.MessageBox.Show("The files were organized successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show(result.Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void PopulateCategoriesComboBox(List<FileCategory> categories)
        {
            SelectCategoryNameComboBox.Items.Clear();
            foreach (FileCategory category in categories)
            {
                SelectCategoryNameComboBox.Items.Add(category.Name);
            }
        }

        private void PopulateFileExtensions(FileCategory fileCategory, System.Windows.Controls.TextBox textBox)
        {
            if (fileCategory == null) return;
            
            StringBuilder selectedCategories = new StringBuilder();
            for (int i = 0; i < fileCategory.FileExtensions.Count; i++)
            {
                string fileExtension = fileCategory.FileExtensions[i];
                if (i < fileCategory.FileExtensions.Count - 1)
                {
                    selectedCategories.AppendLine(fileExtension);
                }
                else
                {
                    selectedCategories.Append(fileExtension);
                }
            }
            textBox.Text = selectedCategories.ToString();
        }
    }
}
