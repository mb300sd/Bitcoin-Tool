using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Bitcoin_Tool.DataConverters;

namespace Bitcoin_Tool.Crypto
{
	class PrivateKey
	{
		private ECKeyPair ecKeyPair;
		public PublicKey pubKey { get; private set; }

		public PrivateKey(Byte[] privKey, Boolean compress = false)
		{
			this.ecKeyPair = new ECKeyPair(privKey, null, compress);
			this.pubKey = new PublicKey(ecKeyPair);
		}

		public PrivateKey(ECKeyPair ecKeyPair)
		{
			if (ecKeyPair.privKey == null)
				throw new ArgumentException("ECKeyPair does not contain private key.");
			this.ecKeyPair = ecKeyPair;
			this.pubKey = new PublicKey(ecKeyPair);
		}

		public Byte[] ToBytes()
		{
			return ecKeyPair.privKey;
		}

		public Byte[] Sign(Byte[] data)
		{
			return ecKeyPair.signData(data);
		}

		public static PrivateKey FromWIF(String s)
		{
			Byte[] b = Base58CheckString.ToByteArray(s);
			if (b.Length == 0x20)
				return new PrivateKey(b, false);
			else if (b.Length == 0x21)
				return new PrivateKey(b.Take(0x20).ToArray(), true);
			else
				throw new ArgumentException("Invalid WIF Private Key");
		}

		public static PrivateKey FromHex(String s, Boolean compress = false)
		{
			return new PrivateKey(HexString.ToByteArray(s), compress);
		}

		public static PrivateKey FromMiniKey(String s)
		{
			SHA256 sha256 = new SHA256Managed();
			Byte[] b = Encoding.ASCII.GetBytes(s);
			if (sha256.ComputeHash(b.Concat(new Byte[] { (Byte)'?' }).ToArray()).First() != 0x00)
				throw new ArgumentException("Invalid miniKey");
			return new PrivateKey(sha256.ComputeHash(b));
		}

		public static PrivateKey FromBase64(String s, Boolean compress = false)
		{
			return new PrivateKey(Convert.FromBase64String(s), compress);
		}

		public static implicit operator PublicKey (PrivateKey k) {
			return k.pubKey;
		}

		public static implicit operator Address(PrivateKey k)
		{
			return k.pubKey.address;
		}
	}
}
