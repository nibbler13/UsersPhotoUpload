using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UsersPhotoUpload {
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class WindowMain : Window {
		public WindowMain() {
			InitializeComponent();
		}

		private void ButtonAdd_Click(object sender, RoutedEventArgs e) {
			Console.WriteLine("ButtonAdd_Click");

			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Изображения (*.jpg)|*.jpg";
			openFileDialog.CheckFileExists = true;
			openFileDialog.CheckPathExists = true;
			openFileDialog.Multiselect = true;
			openFileDialog.RestoreDirectory = true;

			if (openFileDialog.ShowDialog() == true)
				AddItemsToListView(openFileDialog.FileNames);
		}

		private void AddItemsToListView(string[] fileNames) {
			Console.WriteLine("AddItemsToListView, fileNames length: " + fileNames.Length);

			foreach (string file in fileNames) {
				if (!file.EndsWith(".jpg"))
					continue;

				string fileName = System.IO.Path.GetFileName(file);
				string filePath = System.IO.Path.GetDirectoryName(file);
				BitmapFrame bitmapFrame = BitmapFrame.Create(new Uri(file), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
				string dimension = bitmapFrame.PixelWidth + " x " + bitmapFrame.PixelHeight;
				ListViewFilesItem listVIewFilesItem = new ListViewFilesItem {
					IconUri = file,
					FileName = fileName,
					Path = filePath,
					Dimension = dimension,
					AdAccountName = "",
					Result = "",
					SAMAccountName = "",
					Company = ""};
				listViewFiles.Items.Add(listVIewFilesItem);
			}

			SetButtonsEnableState();
		}

		private void SetButtonsEnableState() {
			buttonHandle.IsEnabled = listViewFiles.Items.Count > 0;
			buttonSearchAccounts.IsEnabled = listViewFiles.Items.Count > 0;
		}

		private void ButtonDelete_Click(object sender, RoutedEventArgs e) {
			Console.WriteLine("ButtonDelete_Click");
			List<Object> itemsToRemove = new List<object>();
			foreach (object item in listViewFiles.SelectedItems)
				itemsToRemove.Add(item);
			foreach (object itemToRemove in itemsToRemove)
				listViewFiles.Items.Remove(itemToRemove);
			SetButtonsEnableState();
		}

		private async void ButtonHandle_Click(object sender, RoutedEventArgs e) {
			Console.WriteLine("ButtonHandle_Click");

			if (listViewFiles.Items.Count == 0) {
				MessageBox.Show("Необходимо выбрать файлы для обработки", "", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (checkBoxAd.IsChecked != true &&
				checkBoxExchange.IsChecked != true &&
				checkBoxLoyaltySurvey.IsChecked != true) {
				MessageBox.Show("Необходимо выбрать вариант обработки", "", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			Cursor = Cursors.Wait;
			buttonHandle.IsEnabled = false;

			List<ListViewFilesItem> items = new List<ListViewFilesItem>();

			foreach (ListViewFilesItem item in listViewFiles.Items)
				items.Add(item);

			bool loadToAd = checkBoxAd.IsChecked == true ? true : false;
			bool loadToExchange = checkBoxExchange.IsChecked == true ? true : false;
			bool loadToSurveyLoyalty = checkBoxLoyaltySurvey.IsChecked == true ? true : false;

			await Task.Run(() => { HandleFiles(items, loadToAd, loadToExchange, loadToSurveyLoyalty); });

			buttonHandle.IsEnabled = true;
			Cursor = Cursors.Arrow;

			MessageBox.Show("Информация по каждому файлу записана в столбец 'Результат'", 
				"Загрузка завершена", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void HandleFiles(List<ListViewFilesItem> items, bool loadToAd, bool loadToExchange, bool loadToSurveyLoyalty) {
			Console.WriteLine("ProcessPhotos");

			ProcessingSystem.ProcessFilesItems(
				items,
				SetProgressBarValue,
				loadToAd,
				loadToExchange,
				loadToSurveyLoyalty);
		}

		private void ButtonSelect_Click(object sender, RoutedEventArgs e) {
			Console.WriteLine("ButtonSelect_Click");
			try {
				ListViewFilesItem item = (sender as Button).DataContext as ListViewFilesItem;
				Console.WriteLine(item.FileName);
				WindowAdAccountSearch windowAdAccountSearch = new WindowAdAccountSearch(item);
				windowAdAccountSearch.ShowDialog();
			} catch (Exception exception) {
				Console.WriteLine(exception.Message);
			}
		}

		private void SetProgressBarValue(double progress) {
			Application.Current.Dispatcher.Invoke(new Action(() => {
				progressBar.Value = progress;
			}));
		}

		private async void buttonSearchAccounts_Click(object sender, RoutedEventArgs e) {
			SetApplicationState(true);

			await Task.Run(() => {
				foreach (ListViewFilesItem item in listViewFiles.Items)
					ProcessingSystem.FindSAMAccountName(item);
			});

			SetApplicationState(false);
		}

		private void SetApplicationState(bool IsBusy) {
			Cursor = IsBusy ? Cursors.Wait : Cursors.Arrow;
			this.IsEnabled = !IsBusy;
		}

		private void listViewFiles_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			buttonDelete.IsEnabled = listViewFiles.SelectedItems.Count > 0;
		}

		private void listViewFiles_Drop(object sender, DragEventArgs e) {
			Console.WriteLine("listViewFiles_Drop");
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			
			List<string> fileNames = new List<string>();

			foreach (string fileName in files) {
				FileAttributes fileAttribute = File.GetAttributes(fileName);
				if ((fileAttribute & FileAttributes.Directory) == FileAttributes.Directory) {
					try {
						string[] filesInDirectory = Directory.GetFiles(fileName, "*", System.IO.SearchOption.AllDirectories);
						fileNames.AddRange(filesInDirectory);
					} catch (Exception exception) {
						Console.WriteLine(exception.Message + " " + exception.StackTrace);
					}
				} else {
					fileNames.Add(fileName);
				}
			}
			
			AddItemsToListView(fileNames.ToArray());
		}
	}
}
