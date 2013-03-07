using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	public class Tx : Structs.Transaction, IPayload
	{
		protected Tx()
		{
		}

		public Tx(Byte[] b)
			: base(b)
		{
		}

		public static new Tx FromStream(Stream s)
		{
			Tx x = new Tx();
			x.Read(s);
			return x;
		}
	}
}
