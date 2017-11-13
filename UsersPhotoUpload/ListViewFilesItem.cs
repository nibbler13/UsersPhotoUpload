using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UsersPhotoUpload {
	public class ListViewFilesItem : INotifyPropertyChanged {
		public string IconUri { get; set; }
		public string FileName { get; set; }
		public string Path { get; set; }
		public string Dimension { get; set; }
		public string SAMAccountName { get; set; }
		public string Mail { get; set; }
		public string DistinguishedName { get; set; }

		private string adAccountName;
		public string AdAccountName {
			get {
				return adAccountName;
			}
			set {
				if (value != adAccountName) {
					adAccountName = value;
					NotifyPropertyChanged();
				}
			}
		}

		private string result;
		public string Result {
			get {
				return result;
			}
			set {
				if (value != result) {
					result = value;
					NotifyPropertyChanged();
				}
			}
		}

		private string company;
		public string Company {
			get {
				return company;
			}
			set {
				if (value != company) {
					company = value;
					NotifyPropertyChanged();
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
