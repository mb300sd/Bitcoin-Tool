using System;
using System.Linq;
using System.Security.Cryptography;

namespace Bitcoin_Tool.DataConverters
{
	static class Base58CheckString
	{
		public static String FromByteArray(Byte[] b, Byte version) {
			SHA256 sha256 = new SHA256Managed();
			b = (new Byte[] { version }).Concat(b).ToArray();
			Byte[] hash = sha256.ComputeHash(sha256.ComputeHash(b)).Take(4).ToArray();
			return Base58String.FromByteArray(b.Concat(hash).ToArray());
		}

		public static Byte[] ToByteArray(String s, out Byte version) {
			SHA256 sha256 = new SHA256Managed();
			Byte[] b = Base58String.ToByteArray(s);
			Byte[] hash = sha256.ComputeHash(sha256.ComputeHash(b.Take(b.Length - 4).ToArray()));
			if (!hash.Take(4).SequenceEqual(b.Skip(b.Length - 4).Take(4)))
				throw new ArgumentException("Invalid Base58Check String");
			version = b.First();
			return b.Skip(1).Take(b.Length - 5).ToArray();
		}

		public static Byte[] ToByteArray(String s)
		{
			Byte b;
			return ToByteArray(s, out b);
		}
	}
}
