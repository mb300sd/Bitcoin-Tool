using System;
using System.Collections.Generic;
using Bitcoin_Tool.Crypto;

namespace Bitcoin_Tool.Scripts
{
	public static class ScriptTemplate
	{
		public static Script PayToAddress(Address addr)
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

		public static Boolean IsPayToAddress(this Script s)
		{
			return (
				s.elements.Count >= 5 &&
				s.elements[s.elements.Count - 5].opCode == OpCode.OP_DUP &&
				s.elements[s.elements.Count - 4].opCode == OpCode.OP_HASH160 &&
				s.elements[s.elements.Count - 3].isData &&
				s.elements[s.elements.Count - 2].opCode == OpCode.OP_EQUALVERIFY &&
				s.elements[s.elements.Count - 1].opCode == OpCode.OP_CHECKSIG);
		}

		public static Boolean IsPayToPublicKey(this Script s)
		{
			return (
				s.elements.Count >= 2 &&
				s.elements[s.elements.Count - 2].isData &&
				s.elements[s.elements.Count - 1].opCode == OpCode.OP_CHECKSIG);
		}

		public static Boolean IsPayToScriptHash(this Script s)
		{
			return (
				s.elements.Count >= 3 &&
				s.elements[s.elements.Count - 3].opCode == OpCode.OP_HASH160 &&
				s.elements[s.elements.Count - 2].isData &&
				s.elements[s.elements.Count - 1].opCode == OpCode.OP_EQUAL);
		}
	}
}
