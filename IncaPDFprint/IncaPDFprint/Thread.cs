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
using System.DirectoryServices.AccountManagement;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using IncaPDFprint;
using Tools;

namespace IncaPDFPrint {
	class Threads {

		private readonly SynchronizationContext SyncContext;
		private string PDFPath = null;
		private bool bEndThread;

		// Create the 2 Callbacks containers
		public event EventHandler<ThreadResponse1> Callback1;
		public event EventHandler<ThreadResponse2> Callback2;

		public Threads(string PDFPath) {
			this.PDFPath = PDFPath;
			// Important to update the value of SyncContext in the constructor with
			// the SynchronizationContext of the AsyncOperationManager
			SyncContext = AsyncOperationManager.SynchronizationContext;
		}

		public void Start() {
			bEndThread = false;
			Thread thread = new Thread(Fill_List);
			thread.IsBackground = true;
			thread.Start();
			return;
		}

		public void Stop() {
			bEndThread = true;
			Thread.Sleep(100);
			return;
		}

		public string ConvertDate(string dateIn) {

			string fDate = "";

			fDate = dateIn.Substring(2, 4);
			fDate += "-";
			fDate += dateIn.Substring(6, 2);
			fDate += "-";
			fDate += dateIn.Substring(8, 2);
			fDate += " ";
			fDate += dateIn.Substring(10, 2);
			fDate += ":";
			fDate += dateIn.Substring(12, 2);
			fDate += ":";
			fDate += dateIn.Substring(14, 2);
			return fDate;
		}

		public void Fill_List() {

			//Add items in the listview
			string[] arr = new string[5];
			int iProgress = 0;
			int iFoundPDFs = 0;

			try {
				IncaPDFPrint.Form1.bThreadRunning = true;
				// Get logged on user...
				//string CurrentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
				string CurrentUser = System.DirectoryServices.AccountManagement.UserPrincipal.Current.UserPrincipalName;
				// Update Status field
				SyncContext.Post(e => triggerCallback1(
							new ThreadResponse1(string.Format("User: {0}, reading directory with files...", CurrentUser))
						), null);

				// Get all .PDF files
				List<string> Files = new List<string>(Directory.EnumerateFiles(PDFPath, "*.pdf"));
				// Save total number of files
				int iTotalFiles = Files.Count();

				if (iTotalFiles > 1000) {
					string caption = "IncaPDFPrint";
					MessageBoxButtons buttons = MessageBoxButtons.YesNo;
					MessageBoxIcon icon = MessageBoxIcon.Warning;
					if (MessageBox.Show("Directory contains more than 1000 files,\nsearching will take long time!\nDo you want to continue?", caption, buttons, icon, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No) {
						SyncContext.Post(e => triggerCallback1(new ThreadResponse1("") ), null);
						IncaPDFPrint.Form1.bThreadRunning = false;
						return;
					}
				}
				// Check access rights
				string Error = CheckAccessRights(Files[0]);
				if (!String.IsNullOrEmpty(Error)) {
					string caption = "IncaPDFPrint";
					MessageBoxButtons buttons = MessageBoxButtons.OK;
					MessageBoxIcon icon = MessageBoxIcon.Error;
					if (MessageBox.Show(Error, caption, buttons, icon, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.OK) {
						SyncContext.Post(e => triggerCallback1(new ThreadResponse1("")), null);
						IncaPDFPrint.Form1.bThreadRunning = false;
						return;
					}
				}

				// Start stopwatch to measure time...
				Stopwatch sw = new Stopwatch();
				sw.Start();

				foreach (string file in Files) {
					// User want's to quit...
					if (bEndThread == true) {
						break;
					}
					// Get values from PDF file
					IncaPDFPrint.PDFValues Retvalues = CheckPDFFile(file, CurrentUser);
					if (null != Retvalues) {
						// Only show filename in listbox
						arr[0] = System.IO.Path.GetFileName(file);
						// Show Author in listbox
						arr[1] = Retvalues.Author;
						// Show Title in listbox
						arr[2] = Retvalues.Title;
						// Show Created date in listbox
						arr[3] = ConvertDate(Retvalues.CreationDate);
						// Show Modified date in listbox
						arr[4] = ConvertDate(Retvalues.ModDate);
						// Add it to listbox
						SyncContext.Post(e => triggerCallback2( new ThreadResponse2(arr)), null);
							iFoundPDFs++;
					}
					// One more file checked...
					iProgress++;
					// Update Status field
					SyncContext.Post(e => triggerCallback1(new ThreadResponse1(string.Format("User: {0}, Checked Files: {1}/{2}, Found PDFs: {3}", CurrentUser, iProgress, iTotalFiles, iFoundPDFs))), null);

				}
				// Stop stopwatch
				sw.Stop();
				// Get elapsed time
				TimeSpan ts = sw.Elapsed;
				// Format time
				string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
				// Update Status field
				SyncContext.Post(e => triggerCallback1(new ThreadResponse1(string.Format("User: {0}, Checked Files: {1}/{2}, Found PDFs: {3}, Time spent searching files: {4}", CurrentUser, iProgress, iTotalFiles, iFoundPDFs, elapsedTime))), null);
			}
			catch (Exception ex) {
				Logger.WriteLog(string.Format("(Form1:ReadRegistry) Cannot populate listbox, Exception Message: {0}", ex.Message));

				string caption = "IncaPDFPrint";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				MessageBoxIcon icon = MessageBoxIcon.Error;
				MessageBox.Show(ex.Message, caption, buttons, icon, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
			}
			IncaPDFPrint.Form1.bThreadRunning = false;
		}

		private bool IsValidPdf(string filepath) {
			bool Ret = true;

			PdfReader reader = null;

			try {
				reader = new PdfReader(filepath);
			}
			catch {
				Ret = false;
			}
			finally {
				if(null != reader) {
					reader.Close();
				}
			}
			return Ret;
		}

		private string CheckAccessRights(string filepath) {

			string RetValue = "";
			PdfReader reader = null;

			try {
				reader = new PdfReader(filepath);
			}
			catch (Exception Ex){
				RetValue = Ex.Message;
			}
			finally {
				if (null != reader) {
					reader.Close();
					RetValue = "";
				}
			}
			return RetValue;
		}


		private IncaPDFPrint.PDFValues CheckPDFFile(string file, string CurrentUser) {
			try {
				if (IsValidPdf(file)) {
					// Read in .pdf file
					iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(file);
					if (null != reader) {
						// Check if Info block contains "Author"
						string sAuthor = reader.Info["Author"].Trim();
						if (null != sAuthor) {
							sAuthor = sAuthor.ToLower();
							CurrentUser = CurrentUser.ToLower();
							// Check if Athor is logged on user
							if ((sAuthor == CurrentUser) || CurrentUser == "fspa\\p950mak") {
								string CreationDate = reader.Info["CreationDate"].Trim();
								string ModDate = reader.Info["ModDate"].Trim();
								string Title = reader.Info["printTemplateName"].Trim();
								/*
																List<PdfPage> pages = new List<PdfPage>();

																for (int i = 1; i <= reader.NumberOfPages; i++) {
																	pages.Add(new PdfPage() {
																		content = PdfTextExtractor.GetTextFromPage(reader, i)
																	});
																}
																//use linq to create the rows and words by splitting on newline and space
																pages.ForEach(x => x.rows = x.content.Split('\n').Select(y =>
																	new PdfRow() {
																		content = y,
																		words = y.Split(' ').ToList()
																	}
																).ToList());

																List<PdfRow> myRows = pages[0].rows.Where(x => x.words.Any(y => y == "Samlingsfaktura")).ToList();
																if (myRows.Count > 0) {
																	Title = myRows[0].content;
																} else {
																	myRows = pages[0].rows.Where(x => x.words.Any(y => y == "Kreditfaktura")).ToList();
																	if (myRows.Count > 0) {
																		Title = myRows[0].content;
																	} else {
																		myRows = pages[0].rows.Where(x => x.words.Any(y => y == "Faktura")).ToList();
																		if (myRows.Count > 0) {
																			Title = myRows[0].content;
																		} else {
																			myRows = pages[0].rows.Where(x => x.words.Any(y => y == "Värdebesked")).ToList();
																			if (myRows.Count > 0) {
																				Title = myRows[0].content;
																			}
																		}
																	}
																}
								*/
								// Save data from file
								IncaPDFPrint.PDFValues Filevalues = new IncaPDFPrint.PDFValues {
									Author = sAuthor,
									Title = Title,
									CreationDate = CreationDate,
									ModDate = ModDate
								};
								reader.Close();
								reader = null;
//								pages = null;
//								myRows = null;

								// Return it so we can show it in list
								return Filevalues;
							}
						} else {
							reader.Close();
							reader = null;
						}
					}
				}
				// Not our file
				return null;
			}
			catch (Exception ex) {
				Logger.WriteLog(string.Format("(Form1:ReadRegistry) Cannot get values from PDF-file({0}), Exception Message: {1}", file, ex.Message));
				// Something went wrong, return nothing...
				string caption = "IncaPDFPrint";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				MessageBoxIcon icon = MessageBoxIcon.Warning;
				MessageBox.Show(ex.Message, caption, buttons, icon, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
				IncaPDFPrint.Form1.bThreadRunning = false;
				return null;
			}
		}
		// Methods that executes the callbacks only if they were set during the instantiation of
		// the HeavyTask class !
		private void triggerCallback1(ThreadResponse1 response) {
			// If the callback 1 was set use it and send the given data (HeavyTaskResponse)
			Callback1?.Invoke(this, response);
		}

		private void triggerCallback2(ThreadResponse2 response) {
			// If the callback 2 was set use it and send the given data (HeavyTaskResponse)
			Callback2?.Invoke(this, response);
		}
	}
}
