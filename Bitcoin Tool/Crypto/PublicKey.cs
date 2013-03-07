using System;
using Bitcoin_Tool.DataConverters;
using System.Linq;

namespace Bitcoin_Tool.Crypto
{
	public class PublicKey
	{
		private ECKeyPair ecKeyPair;
		public Address address { get; private set; }

		public PublicKey(Byte[] pubKey)
		{
			this.ecKeyPair = new ECKeyPair(null, pubKey);
			this.address = new Address(ecKeyPair.pubKey);
		}

		public PublicKey(ECKeyPair ecKeyPair)
		{
			this.ecKeyPair = ecKeyPair;
			this.address = new Address(ecKeyPair.pubKey);
		}

		public Byte[] ToBytes()
		{
			return ecKeyPair.pubKey;
		}

		public static PublicKey FromHex(String s)
		{
			return new PublicKey(HexString.ToByteArray(s));
		}

		public Boolean VerifySignature(Byte[] data, Byte[] sig)
		{
			return ecKeyPair.verifySignature(data, sig);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is PublicKey))
				return false;
			return ((PublicKey)obj).ecKeyPair.pubKey.SequenceEqual(this.ecKeyPair.pubKey);
		}

		public override int GetHashCode()
		{
			return ecKeyPair.pubKey.GetHashCode();
		}

		public static implicit operator Address(PublicKey k)
		{
			return k.address;
		}
	}
}
