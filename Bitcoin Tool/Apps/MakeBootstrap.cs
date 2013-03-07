using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Bitcoin_Tool.Structs;
using Bitcoin_Tool.DataConverters;

// BROKEN/IMCOMPLETE/ABANDONED
// Attempted to create bootstrap.dat containing no orphaned blocks

namespace Bitcoin_Tool.Apps
{
	class MakeBootstrap
	{
		const int MAX_ORPHAN_DEPTH = 4;

		class BlockQueue : Queue<Block_Disk>
		{
			int blockFileNum = 0;
			FileStream fs;

			public BlockQueue() : base()
			{
				fs = new FileStream(@"C:\Users\Administrator\AppData\Roaming\Bitcoin\blocks\blk" + blockFileNum.ToString("D5") + ".dat", FileMode.Open);
				fill();
			}

			private void fill() {
				while (this.Count < 5) {
					Block_Disk b = readNext();
					if (b == null)
						return;
					base.Enqueue(b);
				}
			}

			private Block_Disk readNext()
			{
				while (fs.Position < fs.Length)
				{
					Block_Disk b;
					b = Block_Disk.FromStream(fs);

					if (b.blockSize == 0)
					{
						blockFileNum++;
						try
						{
							fs.Close();
							fs = new FileStream(@"C:\Users\Administrator\AppData\Roaming\Bitcoin\blocks\blk" + blockFileNum.ToString("D5") + ".dat", FileMode.Open);
						}
						catch (FileNotFoundException)
						{
							return null;
						}
						continue;
					}

					return b;
				}

				return null;
			}

			public new void Enqueue(Block_Disk b)
			{
				throw new InvalidOperationException();
			}

			public new Block_Disk Dequeue()
			{
				fill();
				return base.Dequeue();
			}

			public new Block_Disk Peek()
			{
				fill();
				return base.Peek();
			}
		}

		public static List<Queue<Hash>> copyChain(List<Queue<Hash>> chain)
		{
			List<Queue<Hash>> altchain = new List<Queue<Hash>>();
			chain.ForEach(q =>
			{
				altchain.Add(makeQueue(q.ToList()));
			});
			return altchain;
		}

		public static Queue<Hash> makeQueue(List<Hash> l)
		{
			Queue<Hash> q = new Queue<Hash>();
			l.ForEach(h => q.Enqueue(h));
			return q;
		}

		public static void Main(string[] args)
		{
			BlockQueue q = new BlockQueue();
			
			FileStream fso = new FileStream(@"D:\bootstrap.dat", FileMode.Create);

			Dictionary<Hash, int> diskBlockIndex = new Dictionary<Hash, int>();
			Dictionary<int, Block_Disk> diskBlocks = new Dictionary<int, Block_Disk>();

			List<Queue<Hash>> chains = new List<Queue<Hash>>();
			chains.Add(new Queue<Hash>());

			Queue<Hash> bestChain = new Queue<Hash>();

			bool first = true;

			int index = 0;
			while (q.Count > 0)
			{
				Block_Disk b = q.Dequeue();
//				diskBlocks.Add(index, b);
				diskBlockIndex.Add(b.Hash, index);

				if (first)
				{
					chains.Single().Enqueue(b.Hash);
					first = false;
				}
				else
				{
					int ci = chains.FindIndex(x => x.ToList().Contains(b.prev_block));
					if (ci < 0){
						Console.WriteLine(HexString.FromByteArrayReversed(b.Hash));
						Console.WriteLine(HexString.FromByteArrayReversed(b.prev_block));
						Console.WriteLine(HexString.FromByteArrayReversed(chains[ci].Last()));
						throw new Exception("Invalid PrevBlock!");
					}
					if (chains[ci].Last().Equals(b.prev_block))
					{
						// Add to chain
						chains[ci].Enqueue(b.Hash);
					}
					else
					{
						// not last block in chain, duplicate and add
						List<Hash> l = chains[ci].ToList();
						int bi = l.FindIndex(x => x.Equals(b.prev_block));
						l.RemoveRange(bi + 1, l.Count - (bi + 1));
						l.Add(b.Hash);
						chains.Add(makeQueue(l));
					}

					int longestChain = chains.Max(x => x.Count);
					chains.RemoveAll(x => x.Count < (longestChain - MAX_ORPHAN_DEPTH));

					while (chains.Max(x => x.Count) > MAX_ORPHAN_DEPTH * 2)
					{
						bestChain.Enqueue(chains.First(x => x.Count == chains.Max(y => y.Count)).Peek());
						chains.ForEach(c => c.Dequeue());
						chains.RemoveAll(c => c.Count == 0);
					}
				}
				index++;
			}
			while (chains.Count > 0 && chains.Max(x => x.Count) > 0)
			{
				bestChain.Enqueue(chains.First(x => x.Count == chains.Max(y => y.Count)).Peek());
				chains.ForEach(c => c.Dequeue());
				chains.RemoveAll(c => c.Count == 0);
			}

			Console.ReadLine();
		}
	}
}