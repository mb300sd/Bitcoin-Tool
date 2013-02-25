using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Bitcoin_Tool.DataConverters
{
	static class Base58String
	{
		private const string base58chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

		public static Byte[] ToByteArray(String s)
		{
			BigInteger bi = 0;
			// Decode base58
			foreach (Char c in s)
			{
				int charVal = base58chars.IndexOf(c);
				if (charVal >= 0)
				{
					bi *= 58;
					bi += charVal;
				}
			}
			Byte[] b = bi.ToByteArray();
			// Remove 0x00 sign byte if present.
			if (b[b.Length - 1] == 0x00)
				b = b.Take(b.Length - 1).ToArray();
			// Add leading 0x00 bytes
			int num0s = s.IndexOf(s.First(c => c != '1'));
			return b.Concat(new Byte[num0s]).Reverse().ToArray();
		}

		public static String FromByteArray(Byte[] b)
		{
			StringBuilder sb = new StringBuilder();
			BigInteger bi = new BigInteger(b.Reverse().Concat(new Byte[] {0x00}).ToArray()); // concat adds sign byte
			// Calc base58 representation
			while (bi > 0)
			{
				int mod = (int)(bi % 58);
				bi /= 58;
				sb.Insert(0, base58chars[mod]);
			}
			// Add 1s for leading 0x00 bytes
			for (int i = 0; i < b.Length && b[i] == 0x00; i++)
				sb.Insert(0, '1');
			return sb.ToString();
		}
	}
}
