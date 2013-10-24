using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitcoin_Tool.RPC
{
	public class BitcoinRPCException : Exception
	{
		public BitcoinRPCError Error
		{
			get;
			private set;
		}

		public BitcoinRPCException(BitcoinRPCError rpcError)
			: base(rpcError.message)
		{
			Error = rpcError;
		}

		public BitcoinRPCException(BitcoinRPCError rpcError, Exception innerException)
			: base(rpcError.message, innerException)
		{
			Error = rpcError;
		}
	}
}
