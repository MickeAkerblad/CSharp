using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncaPDFprint {
	class ThreadResponse1 {
		private string message;

		public ThreadResponse1(string msg) {
			this.message = msg;
		}
		public string Message { get { return message; } }
	}

}
