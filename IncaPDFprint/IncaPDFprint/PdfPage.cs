using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncaPDFPrint {
	class PdfPage {
		public string content { get; set; }
		public List<PdfRow> rows { get; set; }
	}
}
