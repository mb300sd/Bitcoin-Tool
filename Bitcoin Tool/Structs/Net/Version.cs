using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Bitcoin_Tool.Util;

namespace Bitcoin_Tool.Structs.Net
{
	class Version : IPayload
	{
		public static Version Default(NetAddr addr_recv, NetAddr addr_from, Int32 start_height)
		{
			Random rand = new Random();
			String user_agent = "/Bitcoin-Tool:" + 
				Assembly.GetExecutingAssembly().GetName().Version.ToString() + "/";

			Version v = new Version();
			v.version = 60001;
			v.services = Services.NODE_NETWORK;
			v.timestamp = UnixTimestamp.Now;
			v.addr_recv = addr_recv;
			v.addr_from = addr_from;
			v.nonce = (((UInt64)rand.Next()) << 32) | (UInt32)rand.Next();
			v.user_agent = new VarStr(user_agent);
			v.start_height = start_height;
			return v;
		}

		public Int32 version;
		public Services services;
		public Int64 timestamp;
		public NetAddr addr_recv;
		public NetAddr addr_from;
		public UInt64 nonce;
		public VarStr user_agent;
		public Int32 start_height;

		protected Version()
		{
		}

		public Version(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			version = br.ReadInt32();
			services = (Services)br.ReadUInt64();
			timestamp = br.ReadInt64();
			addr_recv = NetAddr.FromStream(s);
			addr_from = NetAddr.FromStream(s);
			nonce = br.ReadUInt64();
			user_agent = VarStr.FromStream(s);
			start_height = br.ReadInt32();
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write((Int32)version);
			bw.Write((UInt64)services);
			bw.Write((UInt64)timestamp);
			addr_recv.Write(s);
			addr_from.Write(s);
			bw.Write((UInt64)nonce);
			user_agent.Write(s);
			bw.Write((Int32)start_height);
		}

		public byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static Version FromStream(Stream s)
		{
			Version x = new Version();
			x.Read(s);
			return x;
		}
	}
}
