using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Bitcoin_Tool.DataConverters;
using Bitcoin_Tool.Scripts;
using Bitcoin_Tool.Structs;
using Bitcoin_Tool.Structs.Net;
using Bitcoin_Tool.Util;

using Version = Bitcoin_Tool.Structs.Net.Version;

namespace Bitcoin_Tool
{
	class Program
	{
		static void Main(string[] args)
		{
			TcpClient tcpClient = new TcpClient("10.1.1.40", 8333);
			NetworkStream ns = tcpClient.GetStream();

			NetAddr localaddr = new NetAddr(Services.NODE_NETWORK,
				((IPEndPoint)tcpClient.Client.LocalEndPoint).Address, (UInt16)((IPEndPoint)tcpClient.Client.LocalEndPoint).Port);
			NetAddr remaddr = new NetAddr(Services.NODE_NETWORK,
				((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address, (UInt16)((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port);
			
			new Message("version", Version.Default(remaddr, localaddr, 0)).Write(ns);

			Tx tx = new Tx(HexString.ToByteArray("0100000001b924de36d37e9b46171036c2380dd3f86c33de7868b95f295c79d057abb876c9000000006c493046022100d35d290d515b2310a81850b14522f3965982ad059b08b7d6646372c9f6f98642022100d2d0fa06f8afa2a9df7bf7e06f901f08fc40882f486b292c217c96706912f23a012103b5091600a12d971b056170a682ea596b6680dd8a784f2960b1bfdc891155db44ffffffff0100ae4c2d000000001976a91443e86640aa84e2a53597e79109e912f8f9ebda1188ac00000000"));

			Message mtx = new Message("tx", tx);

			mtx.Write(ns);

			while (tcpClient.Client.Connected)
			{
				Message msg = Message.FromStream(ns);
				ISerialize p = msg.payload;
				Console.WriteLine(msg.payload);
				switch (msg.strcmd)
				{
					case "addr":
						break;
					case "alert":
						break;
					case "block":
						break;
					case "getaddr":
						{
						//	Message m = new Message("addr", new Addr());
							break;
						}
					case "getblocks":
						break;
					case "getdata":
						break;
					case "getheaders":
						break;
					case "headers":
						break;
					case "inv":
						{
							Inv inv = (Inv)msg.payload;
							new Message("getdata", new GetData(inv.inventory)).Write(ns);
							break;
						}
					case "ping":
						break;
					case "tx":
						break;
					case "verack":
						break;
					case "version":
						{
							Message m = new Message("verack", new VerAck());
							m.Write(ns);
							break;
						}
					default:
						break;
				}
			}

			/*
			string line;

			Transaction txUnspent = new Transaction(HexString.ToByteArray("0100000001f165b1179cb8ed7057540375884214287cf1592f2325080364f0bd8ef51d638b010000006a47304402207cf76bb48434e70ff0ea86a949ec4191fff0c31fdbbf6441c28293a09cb6841d022039906ff8b98761aa0d113e975c7af277d091a3cb9aaceae07dffe91f13fd65830121031adaac125ad58a48d1289bd44624d569b3ace8f9c59e02557e8ad00787dc7e06ffffffff0100ae4c2d00000000232102004066b81e658b2535b951c309edc4807a5260d9f1387e5dfda2d91355e562efac00000000"));

			Address addr = new Address("1QJqE3KiyFBSCP12kTybW1ZRSeuJZTUwBY");

			TxIn txIn = new TxIn(txUnspent.hash, 0, new Byte[0]);
			TxOut txOut = new TxOut(760000000, ScriptTemplate.PayToAddress(addr).ToBytes());

			Transaction tx = new Transaction(1, new TxIn[] { txIn }, new TxOut[] { txOut }, 0);

			PrivateKey pk = PrivateKey.FromWIF("L1F95LNfoyGozceafTC1Pex8296YeF3EbsfETs22E83uhqVF5PBy");

			tx.inputs[0].Sign(tx, txUnspent.outputs[0], pk);

			Console.WriteLine(HexString.FromByteArray(tx.ToBytes()));

			Script t = new Script(tx.inputs[0].scriptSig, txUnspent.outputs[0].scriptPubKey);
			Console.WriteLine(t.Evaluate(tx, 0));
			
			/****/
			/*
			int blockFileNum = 0;

			FileStream fs = new FileStream(@"C:\Users\Administrator\AppData\Roaming\Bitcoin\blocks\blk" + blockFileNum.ToString("D5") + ".dat", FileMode.Open);
			List<Block_Disk> blocks = new List<Block_Disk>();
			List<Byte[]> blockHashes = new List<byte[]>();
			List<Address> addrs = new List<Address>();
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
					catch (FileNotFoundException)
					{
						break;
					}
					continue;
				}
				foreach (Transaction t in b.txns)
					foreach (TxOut txo in t.outputs)
					{
						addrs.Add(Address.FromScript(txo.script));
						//Console.WriteLine(Address.FromScript(txo.script));
						//Console.ReadLine();
					}
				//blockHashes.Add(b.prev_block);
				//Console.WriteLine(HexString.FromByteArray(b.hash));
				//blocks.Add(b);
			}
			Console.WriteLine(HexString.FromByteArray(blockHashes[blockHashes.Count() - 1]));
			/**/
			Console.ReadLine();
		}
	}
}
