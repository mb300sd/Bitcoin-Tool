using System;
using System.IO;

namespace Bitcoin_Tool.Structs
{
	public class VarInt : ISerialize
	{
		public UInt64 value;
		public Int32 intValue { get { return (Int32)value; } }

		public VarInt() 
		{
		}

		public VarInt(Int64 i)
		{
			value = (UInt64)i;
		}

		public VarInt(UInt64 i)
		{
			value = i;
		}

		public VarInt(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
			{
				Read(ms);
			}
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
				byte b = br.ReadByte();
				if (b < 0xFD)
					value = b;
				else if (b == 0xFD)
					value = br.ReadUInt16();
				else if (b == 0xFE)
					value = br.ReadUInt32();
				else
					value = br.ReadUInt64();
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
				if (value < 0xFD)
				{
					bw.Write((Byte)value);
				}
				else if (value < UInt16.MaxValue)
				{
					bw.Write((Byte)0xFD);
					bw.Write((UInt16)value);
				}
				else if (value < UInt32.MaxValue)
				{
					bw.Write((Byte)0xFE);
					bw.Write((UInt32)value);
				}
				else
				{
					bw.Write((Byte)0xFF);
					bw.Write((UInt64)value);
			}
		}

		public Byte[] ToBytes() {
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static VarInt FromStream(Stream s)
		{
			VarInt x = new VarInt(0);
			x.Read(s);
			return x;
		}

		public static implicit operator VarInt(UInt64 i)
		{
			return new VarInt(i);
		}

		public static implicit operator UInt64(VarInt vi)
		{
			return vi.value;
		}

		public static implicit operator Int64(VarInt vi)
		{
			return (Int64)vi.value;
		}
	}
}
