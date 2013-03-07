using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Other
{
	public struct UnspentTxOutHeader : ISerialize
	{
		public Hash txid;
		public UInt32 index;

		public UnspentTxOutHeader(Hash txid, UInt32 index)
		{
			this.txid = txid;
			this.index = index;
		}

		public UnspentTxOutHeader(Byte[] b)
		{
			this.txid = null;
			this.index = 0;
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public override bool Equals(Object x)
		{
			if (x != null && x is UnspentTxOutHeader)
				return (index == ((UnspentTxOutHeader)x).index) && txid.Equals(((UnspentTxOutHeader)x).txid);
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

		public static UnspentTxOutHeader FromStream(Stream s)
		{
			UnspentTxOutHeader x = new UnspentTxOutHeader();
			x.Read(s);
			return x;
		}
	}
}
