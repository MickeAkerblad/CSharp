using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IncaPDFPrint {
	public partial class Form2 : Form {

		private string PDFPath = null;
		private string BackupPath = null;
		private int RemoveFile = 0;
		private int BackupFile = 0;

		public string myPDFPath {
			get { return PDFPath; }
			set { PDFPath = value; }
		}
		public string myBackupPath {
			get { return BackupPath; }
			set { BackupPath = value; }
		}
		public int myRemoveFile {
			get { return RemoveFile; }
			set { RemoveFile = value; }
		}
		public int myBackupFile {
			get { return BackupFile; }
			set { BackupFile = value; }
		}

		public Form2() {
			InitializeComponent();
			ReadUsersRegistry();
		}

		private void button2_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void button1_Click(object sender, EventArgs e) {
			try {

				PDFPath = textBox2.Text;
				BackupPath = textBox1.Text;

				if (checkBox1.Checked == true) {
					//checkbox is checked
					RemoveFile = 1;
				} else {
					//checkbox is not checked
					RemoveFile = 0;
				}
				if (checkBox2.Checked == true) {
					//checkbox is checked
					BackupFile = 1;
				} else {
					//checkbox is not checked
					BackupFile = 0;
				}
				if (BackupFile == 1 && String.IsNullOrEmpty(BackupPath)) {
					string caption = "Config";
					MessageBoxButtons buttons = MessageBoxButtons.OK;
					MessageBoxIcon icon = MessageBoxIcon.Error;
					MessageBox.Show("Directory can not be empty!", caption, buttons, icon);
					return;
				}

				// Save parameters in users registry hive and exit
				using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\IncaPDFprint")) {
					if (key != null) {
						key.SetValue("BackupFile", BackupFile, RegistryValueKind.DWord);
						key.SetValue("RemoveFile", RemoveFile, RegistryValueKind.DWord);
						key.SetValue("BackupPath", BackupPath, RegistryValueKind.String);
						key.SetValue("PDFPath", PDFPath, RegistryValueKind.String);
					}
				}
				this.Close();

			}
			catch (Exception ex) {
				string caption = "Config";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				MessageBoxIcon icon = MessageBoxIcon.Error;
				MessageBox.Show(ex.Message, caption, buttons, icon);
			}
		}

		private void ReadUsersRegistry() {
			try {
				RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\IncaPDFprint");
				if (key != null) {
					Object o = key.GetValue("PDFPath");
					if (o != null) {
						PDFPath = o.ToString();
					}
					textBox2.Text = PDFPath;
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
				}
				textBox1.Text = BackupPath;
				if(RemoveFile == 1) {
					checkBox1.Checked = true;
				} else {
					checkBox1.Checked = false;
				}
				if (BackupFile == 1) {
					checkBox2.Checked = true;
				} else {
					checkBox2.Checked = false;
				}
			}
			catch (Exception ex) {
				string caption = "Config";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				MessageBoxIcon icon = MessageBoxIcon.Error;
				MessageBox.Show(ex.Message, caption, buttons, icon);
			}
		}

		private void checkBox2_CheckedChanged(object sender, EventArgs e) {
			if (checkBox2.Checked == true) {
				//checkbox is checked
				checkBox1.Checked = false;
//			} else {
//				//checkbox is not checked
//				checkBox1.Checked = true;
			}
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e) {
			if (checkBox1.Checked == true) {
				//checkbox is checked
				checkBox2.Checked = false;
//			} else {
//				//checkbox is not checked
//				checkBox2.Checked = true;
			}
		}

		private void Form2_Load(object sender, EventArgs e) {

		}

		private void label1_Click(object sender, EventArgs e) {

		}

		private void textBox1_TextChanged(object sender, EventArgs e) {

		}
	}
}
