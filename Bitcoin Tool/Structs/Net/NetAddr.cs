using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace Bitcoin_Tool.Structs.Net
{
	public class NetAddr : ISerialize
	{
		public Services services;
		public IPAddress address;
		public UInt16 port;

		protected NetAddr()
		{
		}

		public NetAddr (Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public NetAddr(Services services, IPAddress address, UInt16 port)
		{
			this.services = services;
			this.address = address;
			this.port = port;

			if (address.GetAddressBytes().Length != 16)
				this.address = new IPAddress((new Byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF })
					.Concat(address.GetAddressBytes()).ToArray());
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			services = (Services)br.ReadUInt64();
			Byte[] address = br.ReadBytes(16);
			this.address = new IPAddress(address);
			port = br.ReadUInt16();
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write((UInt64)services);
			bw.Write(address.GetAddressBytes(), 0, 16);
			bw.Write((UInt16)port);
		}

		public byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static NetAddr FromStream(Stream s)
		{
			NetAddr x = new NetAddr();
			x.Read(s);
			return x;
		}
	}
}
