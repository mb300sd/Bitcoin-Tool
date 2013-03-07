using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	public class GetHeaders : IPayload
	{
		public UInt32 version;
		public VarInt hash_count { get { return new VarInt(block_locator_hashes.Length); } }
		public Byte[][] block_locator_hashes;
		public Byte[] hash_stop;

		protected GetHeaders()
		{
		}

		public GetHeaders(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			version = br.ReadUInt32();
			block_locator_hashes = new Byte[VarInt.FromStream(s)][];
			for (int i = 0; i < block_locator_hashes.Length; i++)
				block_locator_hashes[i] = br.ReadBytes(32);
			hash_stop = br.ReadBytes(32);
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write((UInt32)version);
			hash_count.Write(s);
			for (int i = 0; i < block_locator_hashes.Length; i++)
				bw.Write(block_locator_hashes[i], 0, 32);
			bw.Write(hash_stop, 0, 32);
		}

		public byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static GetHeaders FromStream(Stream s)
		{
			GetHeaders x = new GetHeaders();
			x.Read(s);
			return x;
		}
	}
}
