using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	public class Addr : IPayload
	{
		public struct TimeNetAddr
		{
			public UInt32 time;
			public NetAddr net_addr;
		}
		
		public VarInt count { get { return new VarInt(addr_list.Length); } }
		public TimeNetAddr[] addr_list;

		protected Addr() 
		{
		}

		public Addr(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			addr_list = new TimeNetAddr[VarInt.FromStream(s)];
			for (int i = 0; i < addr_list.Length; i++)
			{
				addr_list[i].time = br.ReadUInt32();
				addr_list[i].net_addr = NetAddr.FromStream(s);
			}
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			count.Write(s);
			for (int i = 0; i < addr_list.Length; i++)
			{
				bw.Write((UInt32)addr_list[i].time);
				addr_list[i].net_addr.Write(s);
			}
		}

		public byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static Addr FromStream(Stream s)
		{
			Addr x = new Addr();
			x.Read(s);
			return x;
		}
	}
}
