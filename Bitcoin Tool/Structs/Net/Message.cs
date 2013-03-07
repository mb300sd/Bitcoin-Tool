using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Bitcoin_Tool.Structs.Net
{
	public class Message : ISerialize
	{
		public UInt32 magic;
		public Byte[] command;
		public UInt32 length;
		public Byte[] checksum;
		public IPayload payload;

		public String strcmd { get { return Encoding.ASCII.GetString(command.TakeWhile(x => x != 0x00).ToArray()); } }

		protected Message()
		{
		}

		public Message(String command, IPayload payload, bool TestNet = false)
		{
			SHA256 sha256 = new SHA256Managed();
			Byte[] payloadBytes = payload.ToBytes();
			if (!TestNet)
				this.magic = 0xD9B4BEF9;
			else
				this.magic = 0x0709110B;
			this.command = Encoding.ASCII.GetBytes(command).Concat(new Byte[12 - command.Length]).ToArray();
			this.length = (UInt32)payloadBytes.Length;
			this.checksum = sha256.ComputeHash(sha256.ComputeHash(payloadBytes)).Take(4).ToArray();
			this.payload = payload;
		}

		public Message(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			magic = br.ReadUInt32();
			command = br.ReadBytes(12);
			length = br.ReadUInt32();
			checksum = br.ReadBytes(4);
			
			switch (strcmd)
			{
				case "addr":
					payload = Addr.FromStream(s);
					break;
				case "alert":
					payload = Alert.FromStream(s);
					break;
				case "block":
					payload = Block.FromStream(s);
					break;
				case "getaddr":
					payload = GetAddr.FromStream(s);
					break;
				case "getblocks":
					payload = GetBlocks.FromStream(s);
					break;
				case "getdata":
					payload = GetData.FromStream(s);
					break;
				case "getheaders":
					payload = GetHeaders.FromStream(s);
					break;
				case "headers":
					payload = Headers.FromStream(s);
					break;
				case "inv":
					payload = Inv.FromStream(s);
					break;
				case "ping":
					payload = Ping.FromStream(s);
					break;
				case "tx":
					payload = Tx.FromStream(s);
					break;
				case "verack":
					payload = VerAck.FromStream(s);
					break;
				case "version":
					payload = Version.FromStream(s);
					break;
				default:
					payload = UnknownPayload.FromStream(s, length, command);
					break;
			}
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write((UInt32)magic);
			bw.Write(command, 0, 12);
			bw.Write((UInt32)length);
			bw.Write(checksum, 0, 4);
			payload.Write(s);
		}

		public byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static Message FromStream(Stream s)
		{
			Message x = new Message();
			x.Read(s);
			return x;
		}
	}
}
