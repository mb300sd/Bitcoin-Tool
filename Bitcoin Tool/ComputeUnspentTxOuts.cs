using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bitcoin_Tool.Structs;
using Bitcoin_Tool.DataConverters;

namespace Bitcoin_Tool
{
	class ComputeUnspentTxOuts
	{
		struct UnspentTxOut
		{
			public Hash txid;
			public UInt32 index;

			public UnspentTxOut(Hash txid, UInt32 index)
			{
				this.txid = txid;
				this.index = index;
			}

			public override bool Equals(Object x)
			{
				if (x != null && x is UnspentTxOut)
					return (index == ((UnspentTxOut)x).index) && txid.Equals(((UnspentTxOut)x).txid);
				return false;
			}

			public override int GetHashCode()
			{
				return (int)(txid.GetHashCode() ^ index);
			}
		}

		static void Main(string[] args)
		{
			Dictionary<UnspentTxOut, TxOut> utxo = new Dictionary<UnspentTxOut, TxOut>();
			List<KeyValuePair<UnspentTxOut, TxOut>> duptxout = new List<KeyValuePair<UnspentTxOut, TxOut>>();

			int blockFileNum = 0;
			FileStream fs = new FileStream(@"C:\Users\Administrator\AppData\Roaming\Bitcoin\blocks\blk" + blockFileNum.ToString("D5") + ".dat", FileMode.Open);

			while (fs.Position < fs.Length)
			{
				Block_Disk b;
				b = Block_Disk.FromStream(fs);
				if (b.blockSize == 0)
				{
					blockFileNum++;
					try
					{
						fs = new FileStream(@"C:\Users\Administrator\AppData\Roaming\Bitcoin\blocks\blk" + blockFileNum.ToString("D5") + ".dat", FileMode.Open);
					}
					catch (FileNotFoundException) { break; }
					continue;
				}
				foreach (Transaction tx in b.txns)
				{
					foreach (TxIn txin in tx.inputs)
					{
						if (txin.prevOutIndex == 0xFFFFFFFF) //coinbase
							continue;
						UnspentTxOut u = new UnspentTxOut(txin.prevOut, txin.prevOutIndex);
						if (!utxo.Remove(u))
						{
							// Not found, in dup list?
							int i = duptxout.FindIndex(x => x.Key.Equals(u));
							if (i < 0)
							{
								// Still not found!?
								Console.WriteLine(HexString.FromByteArray(txin.prevOut));
								Console.WriteLine(HexString.FromByteArrayReversed(txin.prevOut));
							}
							else
							{
								duptxout.RemoveAt(i);
							}

						}
					}
					for (uint i = 0; i < tx.outputs.Length; i++)
					{
						UnspentTxOut u = new UnspentTxOut(tx.hash, i);
						try
						{
							utxo.Add(u, tx.outputs[i]);
						}
						catch (ArgumentException) { 
							// Duplicate!
							duptxout.Add(new KeyValuePair<UnspentTxOut,TxOut>(u, tx.outputs[i]));
						}
					}
				}
			}
		}
	}
}
