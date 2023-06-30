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

namespace IncaPDFprint {
	static class Logger {
		static public void WriteLog(string Message) {

			string path = @"c:\temp\IncaPDFPrint.log";

			// Create a file to write to.
			if (!File.Exists(path)) {
				// Create a file to write to.
				using (StreamWriter sw = File.CreateText(path)) {
					sw.WriteLine(string.Format("{0} (TID):{1} {2}",
							   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff", System.Globalization.DateTimeFormatInfo.InvariantInfo),
							   Thread.CurrentThread.ManagedThreadId,
							   Message));
				}
			} else {
				// This text is always added, making the file longer over time
				// if it is not deleted.
				using (StreamWriter sw = File.AppendText(path)) {
					sw.WriteLine(string.Format("{0} (TID):{1} {2}",
							   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff", System.Globalization.DateTimeFormatInfo.InvariantInfo),
							   Thread.CurrentThread.ManagedThreadId,
							   Message));
				}
			}

		}
	}
}
