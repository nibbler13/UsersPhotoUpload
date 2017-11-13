using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UsersPhotoUpload {
	public partial class WindowAdAccountSearch : Window {
		private ListViewFilesItem item;
		private bool isSearchResultsPresented;

		public WindowAdAccountSearch(ListViewFilesItem item) {
			InitializeComponent();

			this.item = item;
			imageUserPhoto.Source = new BitmapImage(new Uri(item.IconUri));
			textBlockUserName.Focus();
			textBlockFileName.Text = "Имя файла: " + item.FileName;
		}

		private async void ButtonSearch_Click(object sender, RoutedEventArgs e) {
			listViewAdAccounts.Items.Clear();

			string userName = textBlockUserName.Text;

			Cursor = Cursors.Wait;
			IsEnabled = false;

			List<ListViewAdAccountsItem> items = new List<ListViewAdAccountsItem>();
			await Task.Run(new Action(() => { items = ActiveDirectorySystem.SearchUsers(userName); }));

			Cursor = Cursors.Arrow;
			IsEnabled = true;

			isSearchResultsPresented = items.Count > 0;

			if (items.Count == 0)
				listViewAdAccounts.Items.Add(new ListViewAdAccountsItem { Name = "Нет элементов для отображения в этом виде" });
			else
				foreach (ListViewAdAccountsItem item in items)
					listViewAdAccounts.Items.Add(item);
		}

		private void AdAccountItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if (isSearchResultsPresented)
				ButtonSelect_Click(buttonSelect, new RoutedEventArgs());
		}

		private void ButtonSelect_Click(object sender, RoutedEventArgs e) {
			ListViewAdAccountsItem selectedItem = listViewAdAccounts.SelectedItem as ListViewAdAccountsItem;
			item.AdAccountName = selectedItem.Name;
			item.Company = selectedItem.Company;
			item.SAMAccountName = selectedItem.SAMAccountName;
			item.Mail = selectedItem.Mail;
			item.DistinguishedName = selectedItem.DistinguishedName;
			item.Result = "Учетная запись выбрана";

			DialogResult = true;

			Close();
		}

		private void textBlockUserName_TextChanged(object sender, TextChangedEventArgs e) {
			string text = textBlockUserName.Text;
			buttonSearch.IsEnabled = !(string.IsNullOrEmpty(text) && string.IsNullOrWhiteSpace(text));
		}

		private void listViewAdAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			buttonSelect.IsEnabled = (sender as ListView).SelectedItem != null && isSearchResultsPresented;
		}

		private void textBlockUserName_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter)
				ButtonSearch_Click(sender, new RoutedEventArgs());
		}
	}
}
