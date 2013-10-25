using System;
using System.Collections.Generic;
using System.Linq;
using Bitcoin_Tool.Structs;
using Bitcoin_Tool.Scripts;
using Bitcoin_Tool.Structs.Other;

namespace Bitcoin_Tool.Scripts
{
	public static class ValidationExtensions
	{
		public static bool Validate(this Transaction tx, Dictionary<TxOutId, TxOut> prevOuts)
		{
			for (uint i = 0; i < tx.inputs.Length; i++)
			{
				TxIn txin = tx.inputs[i];
			
				Script scriptSig = new Script(txin.scriptSig);
				Script scriptPubKey = new Script(prevOuts[new TxOutId(txin.prevOut, txin.prevOutIndex)].scriptPubKey);
				
				Script s = new Script(scriptSig, scriptPubKey);

				if (!s.Evaluate(tx, i))
					return false;
				
				if (scriptPubKey.IsPayToScriptHash() &&
					scriptSig.elements.Count == 2)
				{
					Script serializedScript = new Script(scriptSig.elements[1].data);
					scriptSig.elements.RemoveAt(1);
					s = new Script(scriptSig, serializedScript);
				
					if (!s.Evaluate(tx, i))
						return false;
				}
			}
			return true;
		}
	}
}