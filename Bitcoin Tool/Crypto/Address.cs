using System;
using System.Security.Cryptography;
using Bitcoin_Tool.Scripts;
using Bitcoin_Tool.DataConverters;
using Bitcoin_Tool.Structs;
using System.Linq;

namespace Bitcoin_Tool.Crypto
{
	public class Address
	{
		public const Byte PUBKEYHASH = 0x00;
		public const Byte SCRIPTHASH = 0x05;
		public const Byte PUBKEY = 0xFF;

		private String address = null;
		private Hash pubKeyHash = null;
		private Hash scriptHash = null;

		public Hash PubKeyHash
		{
			get
			{
				if (pubKeyHash == null && calcHash() != PUBKEYHASH)
					throw new InvalidOperationException("Address is not a public key hash.");
				return pubKeyHash;
			}
		}

		public Hash ScriptHash
		{
			get
			{
				if (pubKeyHash == null && calcHash() != SCRIPTHASH)
					throw new InvalidOperationException("Address is not a script hash.");
				return scriptHash;
			}
		}

		public Hash EitherHash
		{
			get
			{
				if (pubKeyHash == null && scriptHash == null)
					calcHash();
				if (pubKeyHash != null)
					return pubKeyHash;
				if (scriptHash != null)
					return scriptHash;
				return null;
			}
		}

		public Address(Byte[] data, Byte version = PUBKEY)
		{
			SHA256 sha256 = new SHA256Managed();
			RIPEMD160 ripemd160 = new RIPEMD160Managed();
			switch (version)
			{
				case PUBKEY:
					pubKeyHash = ripemd160.ComputeHash(sha256.ComputeHash(data));
					break;
				case PUBKEYHASH:
					pubKeyHash = data;
					break;
				case SCRIPTHASH:
					scriptHash = data;
					break;
			}
		}

		public Address(String address)
		{
			this.address = address;
		}

		public Byte calcHash()
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
				this.address = Base58CheckString.FromByteArray(scriptHash, SCRIPTHASH);
			else
				throw new InvalidOperationException("Address is not a public key or script hash!");
		}

		public static Address FromScript(Byte[] b)
		{
			Script s = new Script(b);
			if (s.IsPayToAddress())
				return new Address(s.elements[s.elements.Count - 3].data, PUBKEYHASH);
			if (s.IsPayToScriptHash())
				return new Address(s.elements[s.elements.Count - 2].data, SCRIPTHASH);
			if (s.IsPayToPublicKey())
				return new Address(s.elements[s.elements.Count - 2].data, PUBKEY);
			return null;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Address))
				return false;
			if (this.EitherHash == null || ((Address)obj).EitherHash == null)
				return false;
			return this.EitherHash.hash.SequenceEqual(((Address)obj).EitherHash.hash);
		}

		public override int GetHashCode()
		{
			return this.EitherHash.GetHashCode();
		}

		public override String ToString()
		{
			if (address == null)
				calcBase58();
			return address;
		}
	}
}
