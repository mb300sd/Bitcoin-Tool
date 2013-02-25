using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitcoin_Tool.Structs
{
	class UnspentTxOut : TxOut
	{
		public Byte[] txid;
		public UInt32 index;
	}
}
