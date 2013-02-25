using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	class UnknownPayload : IPayload
	{
		public readonly Byte[] command;
		public readonly UInt32 len;
		public Byte[] unknown;

		protected UnknownPayload(UInt32 len, Byte[] command)
		{
			this.command = command;
			this.len = len;
		}

		public UnknownPayload(Byte[] b, Byte[] command)
		{
			len = (UInt32)b.Length;
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			unknown = br.ReadBytes((int)len);
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write(unknown, 0, (int)len);
		}

		public byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static UnknownPayload FromStream(Stream s, UInt32 len, Byte[] command)
		{
			UnknownPayload x = new UnknownPayload(len, command);
			x.Read(s);
			return x;
		}
	}
}
