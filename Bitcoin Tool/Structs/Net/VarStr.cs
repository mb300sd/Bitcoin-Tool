using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	public class VarStr : ISerialize
	{
		VarInt len { get { return new VarInt(str.Length); } }
		Byte[] str;

		protected VarStr()
		{
		}

		public VarStr(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public VarStr(String s)
		{
			str = Encoding.ASCII.GetBytes(s);
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			str = br.ReadBytes(VarInt.FromStream(s).intValue);
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			len.Write(s);
			bw.Write(str, 0, str.Length);
		}

		public byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static VarStr FromStream(Stream s)
		{
			VarStr x = new VarStr();
			x.Read(s);
			return x;
		}

		public override string ToString()
		{
			return Encoding.ASCII.GetString(str);
		}
	}
}
