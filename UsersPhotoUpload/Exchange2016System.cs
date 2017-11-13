using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using System.Diagnostics;
using System.IO;

namespace UsersPhotoUpload {
	class Exchange2016System {
		private static string executionPolicy = "Set-ExecutionPolicy Unrestricted -Scope CurrentUser";
		private static string credential = "$UserCredential = Get-Credential";
		private static string exchangeSession = "$Session = New-PSSession -ConfigurationName Microsoft.Exchange " +
			"-ConnectionUri http://mscs-ex-03.budzdorov.ru/powershell/ -Authentication Kerberos -Credential $UserCredential";
		private static string importSession = "Import-PSSession $Session";
		private static string setUserPhoto = "Set-UserPhoto \"@userMail\" -PictureData ([System.IO.File]::ReadAllBytes(\"@imageFile\")) -Confirm:$false";
			
		public static void SetExchangeUsersPhoto(List<ListViewFilesItem> items) {
			string script =
				executionPolicy + Environment.NewLine +
				credential + Environment.NewLine +
				exchangeSession + Environment.NewLine +
				importSession;

			bool isAnythingToProcess = false;
			foreach (ListViewFilesItem item in items) {
				if (string.IsNullOrEmpty(item.Mail)) {
					item.Result += " | Отсутствует учетная запись Exchange";
					continue;
				}

				script += Environment.NewLine + setUserPhoto.Replace("@userMail", item.Mail).Replace("@imageFile", item.IconUri);
				item.Result += " | Изображение для Exchange обновлено";
				isAnythingToProcess = true;
			}

			if (!isAnythingToProcess)
				return;

			Encoding encoding = Encoding.GetEncoding("windows-1251");
			string scriptFileName = Path.GetTempFileName() + ".ps1";
			File.WriteAllText(scriptFileName, script, encoding);
			
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = @"cmd.exe";
			startInfo.Arguments = @"/k powershell.exe " + scriptFileName;

			Process process = new Process();
			process.StartInfo = startInfo;
			process.Start();

			process.WaitForExit();

			File.Delete(scriptFileName);
		}
	}
}
