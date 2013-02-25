using System;
using System.Security.Cryptography;
using Bitcoin_Tool.Scripts;
using Bitcoin_Tool.DataConverters;

namespace Bitcoin_Tool.Crypto
{
	class Address
	{
		const Byte PUBKEYHASH = 0x00;
		const Byte SCRIPTHASH = 0x05;

		private String address = null;
		private Byte[] pubKeyHash = null;
		private Byte[] scriptHash = null;

		public Byte[] PubKeyHash
		{
			get
			{
				if (pubKeyHash == null && calcHash() != PUBKEYHASH)
					throw new InvalidOperationException("Address is not a public key hash.");
				return pubKeyHash;
			}
		}

		public Byte[] ScriptHash
		{
			get
			{
				if (pubKeyHash == null && calcHash() != SCRIPTHASH)
					throw new InvalidOperationException("Address is not a script hash.");
				return scriptHash;
			}
		}

		public Address(Byte[] hash, Byte version = PUBKEYHASH)
		{
			SHA256 sha256 = new SHA256Managed();
			RIPEMD160 ripemd160 = new RIPEMD160Managed();
			switch (version)
			{
				case PUBKEYHASH:
					pubKeyHash = ripemd160.ComputeHash(sha256.ComputeHash(hash));
					break;
				case SCRIPTHASH:
					scriptHash = ripemd160.ComputeHash(sha256.ComputeHash(hash));
					break;
			}
		}

		public Address(String address)
		{
			this.address = address;
		}

		private Byte calcHash()
		{
			Byte version;
			Byte[] hash = Base58CheckString.ToByteArray(address, out version);
			switch (version)
			{
				case PUBKEYHASH:
					pubKeyHash = hash;
					break;
				case SCRIPTHASH:
					scriptHash = hash;
					break;
			}
			return version;
		}

		private void calcBase58()
		{
			if (pubKeyHash != null)
				this.address = Base58CheckString.FromByteArray(pubKeyHash, PUBKEYHASH);
			else if (scriptHash != null)
				this.address = Base58CheckString.FromByteArray(pubKeyHash, SCRIPTHASH);
			else
				throw new InvalidOperationException("Address is not a public key or script hash!");
		}

		public static Address FromScript(Byte[] b)
		{
			Script s = new Script(b);
			if (s.IsPayToAddress())
				return new Address(Base58CheckString.FromByteArray(s.elements[2].data, PUBKEYHASH));
			if (s.IsPayToPublicKey())
				return new Address(s.elements[0].data);
			return null;
		}

		public override String ToString()
		{
			if (address == null)
				calcBase58();
			return address;
		}
	}
}
