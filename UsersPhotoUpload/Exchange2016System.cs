using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace UsersPhotoUpload {
	class Exchange2016System {
		private static string addType = "Add-Type –AssemblyName System.Windows.Forms";
		private static string exchangeSession = "$Session = New-PSSession -ConfigurationName Microsoft.Exchange " +
			"-ConnectionUri http://mscs-ex-03.budzdorov.ru/powershell/ -Authentication Kerberos";
		private static string importSession = "Import-PSSession $Session";
		private static string setUserPhoto = "Set-UserPhoto \"@userMail\" -PictureData ([System.IO.File]::ReadAllBytes(\"@imageFile\")) -Confirm:$false";
		private static string removeSession = "Remove-PSSession $Session";
		private static string messageFinal = "[System.Windows.Forms.MessageBox]::Show('Окно консоли можно закрыть', 'Загрузка в Exchange завершена', 'OK', 'Information')";

		public static void SetExchangeUsersPhoto(List<ListViewFilesItem> items) {
			string script =
				addType + Environment.NewLine +
				exchangeSession + Environment.NewLine +
				importSession;

			bool isAnythingToProcess = false;
			foreach (ListViewFilesItem item in items) {
				if (string.IsNullOrEmpty(item.Mail)) {
					item.Result += " | Exchange - у пользователя отсутствует адрес почты";
					continue;
				}

				script += Environment.NewLine + setUserPhoto.Replace("@userMail", item.Mail).Replace("@imageFile", item.IconUri);
				item.Result += " | Exchange - результат в окне консоли";
				isAnythingToProcess = true;
			}

			if (!isAnythingToProcess)
				return;

			script += 
				Environment.NewLine + removeSession +
				Environment.NewLine + messageFinal;

			Encoding encoding = Encoding.GetEncoding("windows-1251");
			string scriptFileName = Path.GetTempFileName() + ".ps1";
			File.WriteAllText(scriptFileName, script, encoding);

			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = @"cmd.exe",
				Arguments = @"/k powershell.exe -executionpolicy unrestricted -command " + scriptFileName
			};

			Process process = new Process {
				StartInfo = startInfo
			};

			process.Start();
			process.WaitForExit();

			File.Delete(scriptFileName);
		}
	}
}
