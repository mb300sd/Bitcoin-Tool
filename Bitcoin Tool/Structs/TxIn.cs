using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Bitcoin_Tool.Structs
{
	class TxIn : ISerialize
	{
		public Byte[] prevOut;
		public UInt32 prevOutIndex;
		public VarInt scriptSigLen { get { return new VarInt(scriptSig.Length); } }
		public Byte[] scriptSig;
		public UInt32 sequenceNo;

		protected TxIn()
		{
		}

		public TxIn(Byte[] prevOut, UInt32 prevOutIndex, Byte[] scriptSig, UInt32 sequenceNo = 0xFFFFFFFF)
		{
			this.prevOut = prevOut;
			this.prevOutIndex = prevOutIndex;
			this.scriptSig = scriptSig;
			this.sequenceNo = sequenceNo;
		}

		public TxIn(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			prevOut = br.ReadBytes(32);
			prevOutIndex = br.ReadUInt32();
			scriptSig = br.ReadBytes(VarInt.FromStream(s).intValue);
			sequenceNo = br.ReadUInt32();
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write(prevOut, 0, 32);
			bw.Write((UInt32)prevOutIndex);
			scriptSigLen.Write(s);
			bw.Write(scriptSig, 0, scriptSig.Length);
			bw.Write((UInt32)sequenceNo);
		}

		public Byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static TxIn FromStream(Stream s)
		{
			TxIn x = new TxIn();
			x.Read(s);
			return x;
		}
	}
}
