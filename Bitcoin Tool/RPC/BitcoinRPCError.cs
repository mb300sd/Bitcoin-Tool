using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitcoin_Tool.RPC
{
	public class BitcoinRPCError
	{
		public int code;
		public string message;

		public BitcoinRPCError(int code, string message)
		{
			this.code = code;
			this.message = message;
		}

		public BitcoinRPCError()
		{
		}
	}
}
