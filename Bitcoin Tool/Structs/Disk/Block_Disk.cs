using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Bitcoin_Tool.Structs
{
	public class Block_Disk : Block, ISerialize
	{
		public UInt32 magic;
		public UInt32 blockSize;

		protected Block_Disk()
		{
		}

		public Block_Disk(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public override void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			magic = br.ReadUInt32();
			blockSize = br.ReadUInt32();
			base.Read(s);
		}

		public override void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write((UInt32)magic);
			bw.Write((UInt32)blockSize);
			base.Write(s);
		}

		public override Byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public new static Block_Disk FromStream(Stream s)
		{
			Block_Disk x = new Block_Disk();
			x.Read(s);
			return x;
		}
	}
}
