using System;
using System.Collections.Generic;
using System.Numerics;

namespace Bitcoin_Tool.Scripts
{
	struct ScriptVarInt
	{
		public BigInteger value;
		public Int32 intValue { get { return (Int32)value; } }

		public ScriptVarInt(BigInteger i)
		{
			this.value = i;
		}
		public ScriptVarInt(Byte[] b)
		{
			value = 0;
			for (int i = 0; i < b.Length - 1; i++)
			{
				value += b[i] << (i * 8);
			}
			value += (b[b.Length - 1] & 0x7F) << ((b.Length - 1) * 8);
			if ((b[b.Length - 1] & 0x80) == 0x80)
				value = -value;
		}
		public Byte[] ToBytes()
		{
			List<Byte> b = new List<byte>();
			BigInteger val = BigInteger.Abs(this.value);
			do
			{
				b.Add((Byte)(val & 0xFF));
				val = val >> 8;
			} while (val > 0);
			if ((b[b.Count - 1] & 0x80) == 0x80)
				b.Add(0x00);
			if (this.value < 0)
				b[b.Count - 1] |= 0x80;
			return b.ToArray();
		}
		public static implicit operator BigInteger(ScriptVarInt svi)
		{
			return svi.value;
		}
		public static implicit operator ScriptVarInt(BigInteger bi)
		{
			return new ScriptVarInt(bi);
		}
	}
}