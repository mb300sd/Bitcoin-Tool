using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bitcoin_Tool.Crypto;
using Bitcoin_Tool.Structs;
using Bitcoin_Tool.Structs.Other;

namespace Bitcoin_Tool.Apps
{
	class ComputeAddressBalances
	{
		public static void Main(string[] args)
		{
			Dictionary<Address, UInt64> balances = new Dictionary<Address, UInt64>();

			UnspentTxOutList utxo = new UnspentTxOutList();
			Hash lastBlockHash;
			using (FileStream fs = new FileStream(@"D:\utxo.dat", FileMode.Open))
			{
				BinaryReader br = new BinaryReader(fs);
				lastBlockHash = br.ReadBytes(32);
				utxo = UnspentTxOutList.FromStream(fs);
			}
		
			foreach (KeyValuePair<TxOutId, TxOut> txo in utxo)
			{
				Address a = Address.FromScript(txo.Value.scriptPubKey);
				if (a == null)
					continue;
				if (!balances.ContainsKey(a))
					balances.Add(a, 0);
				balances[a] += txo.Value.value;
			}

			foreach (KeyValuePair<Address, UInt64> bal in balances.AsParallel().OrderByDescending(x=>x.Value))
			{
				Console.WriteLine(bal.Key + "\t" + bal.Value);
			}
		}
	}
}
