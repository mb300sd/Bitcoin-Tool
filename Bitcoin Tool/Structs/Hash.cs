using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Bitcoin_Tool.Structs
{
	public class Hash
	{
		public Byte[] hash;

		public Hash(Byte[] b)
		{
			hash = b;
		}

		public Byte this[int i]
		{
			get { return this.hash[i]; }
			set { this.hash[i] = value; }
		}

		public static implicit operator Byte[](Hash h)
		{
			return h.hash;
		}

		public static implicit operator Hash(Byte[] b)
		{
			return new Hash(b);
		}

		public override bool Equals(Object h)
		{
			if (h != null && h is Hash)
				return this.hash.SequenceEqual(((Hash)h).hash);
			return false;
		}

		public override int GetHashCode()
		{
			return (hash[0] << 24) | (hash[1] << 16) | (hash[2] << 8) | (hash[3] << 0);
		}
	}
}
