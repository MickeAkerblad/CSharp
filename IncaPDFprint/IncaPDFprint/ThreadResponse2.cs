using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncaPDFprint {
	class ThreadResponse2 {
		private string[] arr;
		public ThreadResponse2(string[] array) {
			this.arr = array;
		}
		public string[] Array { get { return arr; } }

	}
}
