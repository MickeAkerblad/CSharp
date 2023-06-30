using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using IncaPDFprint;
using Tools;

namespace IncaPDFPrint {
	class ExecThread {

		private System.Windows.Forms.ListView pdfList;
		private int RemoveFile = 0;
		private int BackupFile = 0;
		private string BackupPath;
		private string PDFPath;
		private string AdobePath;

		public ExecThread(System.Windows.Forms.ListView pdfList, string BackupPath, string PDFPath, string AdobePath, int BackupFile, int RemoveFile) {
			this.pdfList = pdfList;
			this.BackupFile = BackupFile;
			this.RemoveFile = RemoveFile;
			this.BackupPath = BackupPath;
			this.PDFPath = PDFPath;
			this.AdobePath = AdobePath;
		}

		public void Start() {
			Thread thread = new Thread(ExecApp);
			thread.IsBackground = true;
			thread.Start();
			return;
		}

		private void ExecApp() {

			string fileToExec = null;

			try {
				// Get the currently selected item in the ListBox.
				if (pdfList.SelectedItems.Count == 0) {
					Logger.WriteLog("(ExecThread:ExecApp) Nothing selected in listbox!");
					return;
				}

				ListViewItem item = pdfList.SelectedItems[0];

				string filename = item.SubItems[0].Text;
				string userid = item.SubItems[1].Text;
				fileToExec = string.Format(@"{0}\{1}", PDFPath, filename);

				// Use ProcessStartInfo class.
				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.CreateNoWindow = true;
				startInfo.UseShellExecute = true;
//				startInfo.FileName = "\"C:\\Program Files (x86)\\Adobe\\Acrobat Reader DC\\Reader\\AcroRd32.exe\"";
				startInfo.FileName = AdobePath; // @"C:\Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe";
				startInfo.Arguments = string.Format("/n {0}", fileToExec);

				Logger.WriteLog(string.Format("(ExecThread:ExecApp) Command: {0} {1}", startInfo.FileName, startInfo.Arguments));

				// Start the process with the info we specified.
				// Call WaitForExit and then the using-statement will close.
				using (Process exeProcess = Process.Start(startInfo)) {
					exeProcess.WaitForExit();
//					AppExitCode = exeProcess.ExitCode;
//					WriteLog(string.Format("(ExecThread:ExecApp) AppExitCode: {0}", AppExitCode));
				}


				// If configured to move file, move it
				string CurrentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
				CurrentUser = CurrentUser.ToLower();
				if (CurrentUser == "fspa\\p950mak") {
					Logger.WriteLog("(ExecThread:ExecApp) p950mak executing, wont remove or backup file!");
					return;
				}

				if (this.BackupFile == 1) {
					if (String.IsNullOrEmpty(BackupPath)) {
						Logger.WriteLog(string.Format("(ExecThread:ExecApp) No or empty directory name! Cannot move file"));
						return;
					}
					string NewFile = GetUniqueFilename((string.Format(@"{0}\{1}", BackupPath, filename)));
					File.Move(fileToExec, NewFile);
					Logger.WriteLog(string.Format("(ExecThread:ExecApp) File {0} saved as {1}.", fileToExec, NewFile));
				} else {
					// If configured to remove file, remove it
					if (this.RemoveFile == 1) {
						File.Delete(fileToExec);
						Logger.WriteLog(string.Format("(ExecThread:ExecApp) File {0} deleted.", fileToExec));
					}
				}
				pdfList.Items[item.Index].Remove();
			}
			catch (Exception ex) {
				Logger.WriteLog(string.Format("(ExecThread:ExecApp) Cannot move or delete file: {0} Exception Message: {1}", fileToExec, ex.Message));
			}
		}

		public static string GetUniqueFilename(string fullPath) {
			if (!System.IO.Path.IsPathRooted(fullPath)) {
				fullPath = System.IO.Path.GetFullPath(fullPath);
			}
				
			if (File.Exists(fullPath)) {
				String filename = System.IO.Path.GetFileName(fullPath);
				String path = fullPath.Substring(0, fullPath.Length - filename.Length);
				String filenameWOExt = System.IO.Path.GetFileNameWithoutExtension(fullPath);
				String ext = System.IO.Path.GetExtension(fullPath);
				int n = 1;
				do {
					fullPath = System.IO.Path.Combine(path, String.Format("{0} ({1}){2}", filenameWOExt, (n++), ext));
				} while (File.Exists(fullPath));
			}
			return fullPath;
		}
	}
}
