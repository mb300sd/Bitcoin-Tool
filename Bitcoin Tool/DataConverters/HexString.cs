using System;
using System.Linq;
using System.Text;

namespace Bitcoin_Tool.DataConverters
{
	public static class HexString
	{
		public static Byte[] ToByteArray(String s)
		{
			if (s.Length % 2 != 0)
				throw new ArgumentException();
			return Enumerable.Range(0, s.Length / 2)
							 .Select(x => Byte.Parse(s.Substring(2 * x, 2), System.Globalization.NumberStyles.HexNumber))
							 .ToArray();
		}

		public static Byte[] ToByteArrayReversed(String s)
		{
			if (s.Length % 2 != 0)
				throw new ArgumentException();
			return Enumerable.Range(0, s.Length / 2)
							 .Select(x => Byte.Parse(s.Substring(2 * x, 2), System.Globalization.NumberStyles.HexNumber))
							 .Reverse()
							 .ToArray();
		}

		public static String FromByteArray(Byte[] b)
		{
			StringBuilder sb = new StringBuilder(b.Length * 2);
			foreach (Byte _b in b)
			{
				sb.Append(_b.ToString("x2"));
			}
			return sb.ToString();
		}

		public static String FromByteArrayReversed(Byte[] b) {
			StringBuilder sb = new StringBuilder(b.Length * 2);
			foreach (Byte _b in b.Reverse())
			{
				sb.Append(_b.ToString("x2"));
			}
			return sb.ToString();
		}

	}
}
