using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersPhotoUpload {
	class ActiveDirectorySystem {
		public static List<ListViewAdAccountsItem> SearchUsers(string userName) {
			List<ListViewAdAccountsItem> items = new List<ListViewAdAccountsItem>();

			using (DirectorySearcher dsSearcher = new DirectorySearcher()) {
				dsSearcher.Filter = "(&(objectClass=user)(name=" + userName + "*)(!(UserAccountControl:1.2.840.113556.1.4.803:=2)))";
				SearchResultCollection results = dsSearcher.FindAll();

				foreach (SearchResult result in results) {
					try {
						using (DirectoryEntry user = new DirectoryEntry(result.Path)) {
							string name = user.Properties["Name"].Value != null ? 
								user.Properties["Name"].Value.ToString() : String.Empty;
							string description = user.Properties["Description"].Value != null ? 
								user.Properties["Description"].Value.ToString() : String.Empty;
							string company = user.Properties["Company"].Value != null ? 
								user.Properties["Company"].Value.ToString() : String.Empty;
							string l = user.Properties["L"].Value != null ? 
								user.Properties["L"].Value.ToString() : String.Empty;
							string department = user.Properties["Department"].Value != null ? 
								user.Properties["Department"].Value.ToString() : String.Empty;
							string title = user.Properties["Title"].Value != null ? 
								user.Properties["Title"].Value.ToString() : String.Empty;
							string mail = user.Properties["Mail"].Value != null ? 
								user.Properties["Mail"].Value.ToString() : String.Empty;
							string sAMAccountName = user.Properties["SAMAccountName"].Value != null ? 
								user.Properties["SAMAccountName"].Value.ToString() : String.Empty;
							string distinguishedName = user.Properties["DistinguishedName"].Value != null ?
								user.Properties["DistinguishedName"].Value.ToString() : String.Empty;

							ListViewAdAccountsItem item = new ListViewAdAccountsItem {
								Name = name,
								Description = description,
								Company = company,
								L = l,
								Department = department,
								Title = title,
								Mail = mail,
								SAMAccountName = sAMAccountName,
								DistinguishedName = distinguishedName };

							items.Add(item);
						}
					} catch (Exception e) {
						Console.WriteLine(e.Message);
					}
				}
			}

			return items;
		}

		public static bool InsertPicture(string sAMAccountName, string fileName) {
			using (DirectorySearcher dsSearcher = new DirectorySearcher()) {
				dsSearcher.Filter = "(&(objectClass=user) (sAMAccountName=" + sAMAccountName + "))";
				SearchResult result = dsSearcher.FindOne();

				if (result == null) 
					return false;

				using (FileStream inputFile = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read)) {
					byte[] binaryData = new byte[inputFile.Length];
					int bytesRead = inputFile.Read(binaryData, 0, (int)inputFile.Length);

					using (DirectoryEntry user = new DirectoryEntry(result.Path)) {
						user.UsePropertyCache = false;
						user.Properties["jpegPhoto"].Clear();
						user.Properties["jpegPhoto"].Add(binaryData);
						user.CommitChanges();
					}

					byte[] newData = GetUserPictureBytes(sAMAccountName);

					return (newData.SequenceEqual(binaryData));
				}
			}
		}

		public static byte[] GetUserPictureBytes(string sAMAccountName) {
			using (DirectorySearcher dsSearcher = new DirectorySearcher()) {
				dsSearcher.Filter = "(&(objectClass=user) (sAMAccountName=" + sAMAccountName + "))";
				SearchResult result = dsSearcher.FindOne();

				using (DirectoryEntry user = new DirectoryEntry(result.Path))
					return user.Properties["jpegPhoto"].Value as byte[];
			}
		}
	}
}
