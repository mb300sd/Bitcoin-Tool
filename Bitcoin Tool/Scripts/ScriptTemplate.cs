using System;
using System.Collections.Generic;
using Bitcoin_Tool.Crypto;

namespace Bitcoin_Tool.Scripts
{
	public static class ScriptTemplate
	{
		public static Script PayToAddress(Address addr)
		{
			switch (addr.Type)
			{
				case Address.PUBKEYHASH:
					return PayToPubKeyHash(addr);
				case Address.SCRIPTHASH:
					return PayToScriptHash(addr);
				default:
					throw new ArgumentException("Invalid address");
			}
		}

		public static Script PayToPubKeyHash(Address addr)
		{
			List<ScriptElement> se = new List<ScriptElement>();
			se.Add(new ScriptElement(OpCode.OP_DUP));
			se.Add(new ScriptElement(OpCode.OP_HASH160));
			se.Add(new ScriptElement(addr.PubKeyHash));
			se.Add(new ScriptElement(OpCode.OP_EQUALVERIFY));
			se.Add(new ScriptElement(OpCode.OP_CHECKSIG));
			return new Script(se.ToArray());
		}

		public static Script PayToPublicKey(PublicKey pubKey)
		{
			List<ScriptElement> se = new List<ScriptElement>();
			se.Add(new ScriptElement(pubKey.ToBytes()));
			se.Add(new ScriptElement(OpCode.OP_CHECKSIG));
			return new Script(se.ToArray());
		}

		public static Script PayToScriptHash(Address addr)
		{
			List<ScriptElement> se = new List<ScriptElement>();
			se.Add(new ScriptElement(OpCode.OP_HASH160));
			se.Add(new ScriptElement(addr.ScriptHash));
			se.Add(new ScriptElement(OpCode.OP_EQUAL));
			return new Script(se.ToArray());
		}

		public static Boolean IsPayToPubKeyHash(this Script s)
		{
			return (
				s.elements.Count >= 5 &&
				s.elements[s.elements.Count - 5].opCode == OpCode.OP_DUP &&
				s.elements[s.elements.Count - 4].opCode == OpCode.OP_HASH160 &&
				s.elements[s.elements.Count - 3].matchesPubKeyHash &&
				s.elements[s.elements.Count - 2].opCode == OpCode.OP_EQUALVERIFY &&
				s.elements[s.elements.Count - 1].opCode == OpCode.OP_CHECKSIG);
		}

		public static Boolean IsPayToPublicKey(this Script s)
		{
			return (
				s.elements.Count >= 2 &&
				s.elements[s.elements.Count - 2].matchesPubKey &&
				s.elements[s.elements.Count - 1].opCode == OpCode.OP_CHECKSIG);
		}

		public static Boolean IsPayToScriptHash(this Script s)
		{
			return (
				s.elements.Count >= 3 &&
				s.elements[s.elements.Count - 3].opCode == OpCode.OP_HASH160 &&
				s.elements[s.elements.Count - 2].matchesScriptHash &&
				s.elements[s.elements.Count - 1].opCode == OpCode.OP_EQUAL);
		}

		public static Boolean IsPayToMultiSig(this Script s)
		{
			int i = s.elements.Count;
			if (s.elements[i--].opCode != OpCode.OP_CHECKMULTISIG)
				return false;
			if (!s.elements[i--].matchesSmallInteger)
				return false;
			while (i >= 0)
			{
				if (s.elements[i].matchesPubKey)
				{
					i--;
					continue;
				}
				if (s.elements[i].matchesSmallInteger)
				{
					return true;
				}
				return false;
			}
			return false;
		}
	}
}
