using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Other
{
	public struct TxOutId : ISerialize
	{
		public Hash txid;
		public UInt32 index;

		public TxOutId(Hash txid, UInt32 index)
		{
			this.txid = txid;
			this.index = index;
		}

		public TxOutId(Byte[] b)
		{
			this.txid = null;
			this.index = 0;
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public override bool Equals(Object x)
		{
			if (x != null && x is TxOutId)
				return (index == ((TxOutId)x).index) && txid.Equals(((TxOutId)x).txid);
			return false;
		}

		public override int GetHashCode()
		{
			return (int)(txid.GetHashCode() ^ index);
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			txid = br.ReadBytes(32);
			index = br.ReadUInt32();
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write(txid, 0, 32);
			bw.Write((UInt32)index);
		}

		public Byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static TxOutId FromStream(Stream s)
		{
			TxOutId x = new TxOutId();
			x.Read(s);
			return x;
		}
	}
}
