using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersPhotoUpload {
	class LoyaltySurveySystem {
		public static void CopyFileToLoyaltySurvey(ListViewFilesItem item) {
			if (string.IsNullOrEmpty(item.DistinguishedName)) {
				item.Result += " | LoyaltySurvey - учетная запись пользователя не выбрана";
				return;
			}

			if (!item.Dimension.Equals("500 x 500")) {
				item.Result += " | LoyaltySurvey - размер изображения не соответствует требуемому (500 x 500)";
				return;
			}

			string rootFolder = Properties.Settings.Default.FolderRoot;

			Dictionary<string, string> folders = new Dictionary<string, string>() {
				{ "kzkk", Properties.Settings.Default.FolderKzkk },
				{ "kdtt", Properties.Settings.Default.FolderKdtt },
				{ "mssu", Properties.Settings.Default.FolderMssu },
				{ "mspo", Properties.Settings.Default.FolderMspo },
				{ "mskm", Properties.Settings.Default.FolderMskm },
				{ "splp", Properties.Settings.Default.FolderSplp },
				{ "sctrk", Properties.Settings.Default.FolderSctrk },
				{ "mskv", Properties.Settings.Default.FolderMskv },
				{ "ufkk", Properties.Settings.Default.FolderUfkk }
			};

			foreach (KeyValuePair<string, string> folder in folders) {
				if (!item.DistinguishedName.ToLower().Contains(folder.Key))
					continue;

				string destinationFile = rootFolder + folder.Value + "\\" + item.AdAccountName + ".jpg";
				try {
					File.Copy(item.IconUri, destinationFile, true);
				} catch (Exception e) {
					item.Result += " | LoyaltySurvey - не удалось скопировать файл: " + destinationFile + " - " + e.Message;
					return;
				}

				item.Result += " | LoyaltySurvey - ok, имя файла: " + destinationFile;
				return;
			}

			item.Result += " | LoyaltySurvey - не удалось найти путь сохранения для подразделения";
		}
	}
}
