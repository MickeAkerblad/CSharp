using IncaPDFprint;
using Microsoft.Win32;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tools;

namespace IncaPDFPrint {
	public partial class Form1 : Form {

		IncaPDFPrint.Threads ThreadClass = null;
		public static bool bThreadEnd = false;
		public static bool bThreadRunning = false;
		private string PDFPath;
		private string BackupPath;
		private string AdobePath;
		public int RemoveFile;
		public int BackupFile;
		public string LastSort = string.Empty;
		public int LastCol = 0;

//		private static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

		public Form1() {
			InitializeComponent();
			// Read registry values
			ReadRegistry();
			GetAdobeReaderPath();
			// Init look of listbox
			initListView();
			// Change color
//			Form1.colorListViewHeader(ref pdfList, System.Drawing.Color.LightGray, System.Drawing.Color.Black);

			// Fill columns
			Load_Columns();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			bool bHandled = false;
			// switch case is the easy way, a hash or map would be better, 
			// but more work to get set up.
			switch (keyData) {
				case Keys.F5:
					// Load values in listbox
					Load_Columns();
					bHandled = true;
					break;
			}
			return bHandled;
		}
		private void button4_Click(object sender, EventArgs e) {
			ThreadClass.Stop();
		}

		private void pdfList_ColumnClick(object sender, ColumnClickEventArgs e) {
			SortListView(e.Column);
		}

		private void button2_Click(object sender, EventArgs e) {
			// Select button clicked
			//			GetSelectedItemAndExecuteProgram();
			// Create thread
			ExecThread exeThread  = new IncaPDFPrint.ExecThread(pdfList, BackupPath, PDFPath, AdobePath, BackupFile, RemoveFile);
			// Start thread to update values in columns
			exeThread.Start();

		}
		private void button3_Click(object sender, EventArgs e) {
			Load_Columns();
		}

		private void pdfList_MouseDoubleClick(object sender, MouseEventArgs e) {
			// Double clik in listbox
			//			GetSelectedItemAndExecuteProgram();
			// Create thread
			ExecThread exeThread = new IncaPDFPrint.ExecThread(pdfList, BackupPath, PDFPath, AdobePath, BackupFile, RemoveFile);
			// Start thread to update values in columns
			exeThread.Start();
		}

		private void button1_Click(object sender, EventArgs e) {
			ThreadClass.Stop();
			// Close button clicked
			Application.Exit();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
			ThreadClass.Stop();
			// Close in menu clicked
			Application.Exit();
		}
		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			ThreadClass.Stop();
			// BIG Close clicked
			Application.Exit();
		}

		private void toolStripMenuItem2_Click(object sender, EventArgs e) {
			// Config in menu clicked
			try {
				// Create Config dialog class
				Form2 configDialog = new Form2();
				// Show Config Window
				configDialog.ShowDialog(this);
				// Save new values from Config Class
				PDFPath = configDialog.myPDFPath;
				BackupPath = configDialog.myBackupPath;
				RemoveFile = configDialog.myRemoveFile;
				BackupFile = configDialog.myBackupFile;
			}
			catch (Exception ex) {
				Logger.WriteLog(string.Format("(Form1:toolStripMenuItem2_Click) Cannot save registry values, Exception Message: {0}", ex.Message));

				string caption = "IncaPDFPrint";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				MessageBoxIcon icon = MessageBoxIcon.Error;
				MessageBox.Show(ex.Message, caption, buttons, icon);
			}
		}

		private void initListView() {
			// Add columns
			pdfList.View = View.Details;
			pdfList.GridLines = false;
			pdfList.FullRowSelect = true;
			pdfList.MultiSelect = false;;

			//Add column header
			pdfList.Columns.Add("Filename", 150);
			pdfList.Columns.Add("User", 100);
			pdfList.Columns.Add("Title", 200);
			pdfList.Columns.Add("Created", 150);
			pdfList.Columns.Add("Modified", 150);
		}

		private void Load_Columns() {

			try {
				if (bThreadRunning == true) {
					return;
				}
				bThreadEnd = false;
				bThreadRunning = false;
				// Create thread
				ThreadClass = new IncaPDFPrint.Threads(PDFPath);

				ThreadClass.Callback1 += CallbackUpdateToolstrip;
				ThreadClass.Callback2 += CallbackUpdateListbox;

				// Clear the listbox
				pdfList.Items.Clear();

				// Start thread to update values in columns
				ThreadClass.Start();
			}
			catch (Exception ex) {
				Logger.WriteLog(string.Format("(Form1:Load_Columns) Cannot save registry values, Exception Message: {0}", ex.Message));

				string caption = "IncaPDFPrint";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				MessageBoxIcon icon = MessageBoxIcon.Error;
				MessageBox.Show(ex.Message, caption, buttons, icon);
			}
		}

		public void SortListView(int Index) {
			
			DataTable TempTable = new DataTable();
			//Add column names to datatable from listview
			foreach (ColumnHeader iCol in pdfList.Columns) {
				TempTable.Columns.Add(iCol.Text);
			}
			//Create a datarow from each listviewitem and add it to the table
			foreach (ListViewItem Item in pdfList.Items) {
				DataRow iRow = TempTable.NewRow();
				// the for loop dynamically copies the data one by one instead of doing irow[i] = MyListView.Subitems[1]... so on
				for (int i = 0; i < pdfList.Columns.Count; i++) {
					if (i == 0) {
						iRow[i] = Item.Text;
					} else {
						iRow[i] = Item.SubItems[i].Text;
					}
				}
				TempTable.Rows.Add(iRow);
			}
			string SortType = string.Empty;
			//LastCol is a public int variable on the form, and LastSort is public string variable
			if (LastCol == Index) {
				if (LastSort == "ASC" || LastSort == string.Empty || LastSort == null) {
					SortType = "DESC";
					LastSort = "DESC";
				} else {
					SortType = "ASC";
					LastSort = "ASC";
				}
			} else {
				SortType = "DESC";
				LastSort = "DESC";
			}
			LastCol = Index;
			pdfList.Items.Clear();
			//Sort it based on the column text clicked and the sort type (asc or desc)
			TempTable.DefaultView.Sort = pdfList.Columns[Index].Text + " " + SortType;
			TempTable = TempTable.DefaultView.ToTable();
			//Create a listview item from the data in each row
			foreach (DataRow iRow in TempTable.Rows) {
				ListViewItem Item = new ListViewItem();
				List<string> SubItems = new List<string>();
				for (int i = 0; i < TempTable.Columns.Count; i++) {
					if (i == 0) {
						Item.Text = iRow[i].ToString();
					} else {
						SubItems.Add(iRow[i].ToString());
					}
				}
				Item.SubItems.AddRange(SubItems.ToArray());
				pdfList.Items.Add(Item);
			}
		}
		private void WriteUserRegistry() {
			Logger.WriteLog("(Form1:WriteUserRegistry) Saving config in User registry hive!");
			try {
				using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\IncaPDFprint")) {
					if (key != null) {
						key.SetValue("BackupFile", BackupFile, RegistryValueKind.DWord);
						key.SetValue("RemoveFile", RemoveFile, RegistryValueKind.DWord);
						key.SetValue("BackupPath", BackupPath, RegistryValueKind.String);
						key.SetValue("PDFPath", PDFPath, RegistryValueKind.String);
					}
				}
			}
			catch(Exception ex) {
				Logger.WriteLog(string.Format("(Form1:WriteUserRegistry) Cannot save registry values, Exception Message: {0}", ex.Message));
			}
		}

		private void ReadRegistry() {
			string caption = "IncaPDFPrint";
			MessageBoxButtons buttons = MessageBoxButtons.OK;
			MessageBoxIcon icon = MessageBoxIcon.Error;

			try {
				// Read CURRENT_USER values
				RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\IncaPDFprint", false);
				if(null == key) {
					// If CURRENT_USER values not found, try LOCAL_MACHINE
					var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
					key = hklm.OpenSubKey(@"Software\IncaPDFprint", false);
				}
				if (key != null) {
					Object o = key.GetValue("PDFPath");
					if (o != null) {
						PDFPath = o.ToString();
					}
					o = key.GetValue("BackupPath");
					if (o != null) {
						BackupPath = o.ToString();
					}
					o = key.GetValue("RemoveFile");
					if (o != null) {
						RemoveFile = (int)o;
					}
					o = key.GetValue("BackupFile");
					if (o != null) {
						BackupFile = (int)o;
					}
					key.Close();
					WriteUserRegistry();
				} else {
					MessageBox.Show("Registry is not updated!", caption, buttons, icon);
				}
			}
			catch (Exception ex) {
				Logger.WriteLog(string.Format("(Form1:ReadRegistry) Cannot read registry values, Exception Message: {0}", ex.Message));
				MessageBox.Show(ex.Message, caption, buttons, icon);
			}
		}

		private void GetAdobeReaderPath() {
			string caption = "IncaPDFPrint";
			MessageBoxButtons buttons = MessageBoxButtons.OK;
			MessageBoxIcon icon = MessageBoxIcon.Error;

			try {
				var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
				RegistryKey key = hklm.OpenSubKey(@"Software\WOW6432Node\Adobe\Acrobat Reader", false);
				if (key != null) {
					Object o = key.GetValue("Reader_2018_URI");
					if (o != null) {
						AdobePath = o.ToString();
						Logger.WriteLog(string.Format("(Form1:GetAdobeReaderPath) Got Adobe reader path from registry: {0}", AdobePath));
					} else {
						Logger.WriteLog("(Form1:GetAdobeReaderPath) Cannot read Adobe reader path from registry!");
					}
					key.Close();
				} else {
					Logger.WriteLog("(Form1:GetAdobeReaderPath) Cannot read Adobe reader path!");
					MessageBox.Show("Could not find Adobe reader regsitry value!", caption, buttons, icon);
				}
			}
			catch (Exception ex) {
				Logger.WriteLog(string.Format("(Form1:GetAdobeReaderPath) Cannot read registry values, Exception Message: {0}", ex.Message));
				MessageBox.Show(ex.Message, caption, buttons, icon);
			}
		}

		private void CallbackUpdateToolstrip(object sender, ThreadResponse1 response) {
			toolStripStatusLabel1.Text = response.Message;
			statusStrip1.Refresh();
		}

		private void CallbackUpdateListbox(object sender, ThreadResponse2 response) {

			ListViewItem itm;

			string[] myArray = response.Array;
			itm = new ListViewItem(myArray);

			pdfList.Items.Add(itm);
		}

		private void Form1_Resize(object sender, EventArgs e) {

			pdfList.Size = new Size(this.Width - 38, this.Height - 200);

			button1.Location = new Point(this.Width - 113, this.Height - 120);
			button4.Top = this.Height - 120;
			button3.Top = this.Height - 120;
			button2.Top = this.Height - 120;
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
			// Create Config dialog class
			Form3 aboutDialog = new Form3();
			// Show Config Window
			aboutDialog.ShowDialog(this);
		}

		//List view header formatters
		public static void colorListViewHeader(ref ListView list, Color backColor, Color foreColor) {
			list.OwnerDraw = true;
			list.DrawColumnHeader +=
				new DrawListViewColumnHeaderEventHandler
				(
					(sender, e) => headerDraw(sender, e, backColor, foreColor)
				);
			list.DrawItem += new DrawListViewItemEventHandler(bodyDraw);
		}

		private static void headerDraw(object sender, DrawListViewColumnHeaderEventArgs e, Color backColor, Color foreColor) {
			using (SolidBrush backBrush = new SolidBrush(backColor)) {
				e.Graphics.FillRectangle(backBrush, e.Bounds);
			}

			using (SolidBrush foreBrush = new SolidBrush(foreColor)) {
				e.Graphics.DrawString(e.Header.Text, e.Font, foreBrush, e.Bounds);
			}
		}

		private static void bodyDraw(object sender, DrawListViewItemEventArgs e) {
			e.DrawDefault = true;
		}

	}
}
