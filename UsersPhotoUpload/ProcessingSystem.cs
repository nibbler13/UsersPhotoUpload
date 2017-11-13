using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UsersPhotoUpload {
	class ProcessingSystem {
		public static void ProcessFilesItems(List<ListViewFilesItem> items, Action<double> setProgressBarValue, 
			bool LoadToAd, bool LoadToExchange, bool LoadToLoyaltySurvey) {
			double progressCurrent = 0;
			double handleTypesCount = Convert.ToInt32(LoadToAd) + Convert.ToInt32(LoadToExchange) + Convert.ToInt32(LoadToLoyaltySurvey);
			double progressStep = 100 / (items.Count * handleTypesCount);
			
			foreach (ListViewFilesItem item in items) {
				setProgressBarValue(progressCurrent);
				progressCurrent += progressStep;

				if (item.SAMAccountName.Equals(string.Empty))
					FindSAMAccountName(item);

				if (item.SAMAccountName.Equals(string.Empty)) {
					Application.Current.Dispatcher.Invoke(new Action(() => {
						WindowAdAccountSearch windowAdAccountSearch = new WindowAdAccountSearch(item);
						windowAdAccountSearch.ShowDialog();
					}));
				}

				if (item.SAMAccountName.Equals(string.Empty)) {
					item.Result = "Учетная запись пользователя не выбрана";
					continue;
				}

				if (LoadToAd) {
					bool adResult = ActiveDirectorySystem.InsertPicture(item.SAMAccountName, item.IconUri);
					item.Result = adResult ? "AD фото успешно обновлено" : "AD не удалось обновить фото";
				}

				if (LoadToLoyaltySurvey)
					LoyaltySurveySystem.CopyFileToLoyaltySurvey(item);
			}

			if (LoadToExchange)
				Exchange2016System.SetExchangeUsersPhoto(items);

			setProgressBarValue(100);
		}

		public static void FindSAMAccountName(ListViewFilesItem item) {
			string fileName = Path.GetFileNameWithoutExtension(item.IconUri);
			string[] fileNameParts = fileName.Replace("  ", " ").TrimStart(' ').TrimEnd(' ').Split(' ');

			List<string> possibleUserNames = new List<string>();

			if (fileNameParts.Length >= 3)
				possibleUserNames.Add(fileNameParts[0] + " " + fileNameParts[1] + " " + fileNameParts[2]);

			if (fileNameParts.Length >= 2)
				possibleUserNames.Add(fileNameParts[0] + " " + fileNameParts[1]);
			
			possibleUserNames.Add(fileNameParts[0]);

			foreach (string possibleUserName in possibleUserNames) {
				List<ListViewAdAccountsItem> items = ActiveDirectorySystem.SearchUsers(possibleUserName);
				if (items.Count == 1) {
					item.AdAccountName = items[0].Name;
					item.SAMAccountName = items[0].SAMAccountName;
					item.Mail = items[0].Mail;
					item.Company = items[0].Company;
					item.DistinguishedName = items[0].DistinguishedName;
					item.Result = "Имя пользователя найдено";

					return;
				}
			}

			item.Result = "Не удалось найти пользователя по имени файла";
		}
	}
}
