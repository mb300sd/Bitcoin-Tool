using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	public class InvVect : ISerialize
	{
		public InvType type;
		public Byte[] hash;

		protected InvVect()
		{
		}

		public InvVect(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			type = (InvType)br.ReadUInt32();
			hash = br.ReadBytes(32);
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write((UInt32)type);
			bw.Write(hash, 0, 32);
		}

		public byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static InvVect FromStream(Stream s)
		{
			InvVect x = new InvVect();
			x.Read(s);
			return x;
		}
	}
}
