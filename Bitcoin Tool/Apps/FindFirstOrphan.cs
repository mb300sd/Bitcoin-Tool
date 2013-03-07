using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Bitcoin_Tool.Structs;
using Bitcoin_Tool.DataConverters;
using Bitcoin_Tool.Util;

namespace Bitcoin_Tool.Apps
{
	class FindFirstOrphan
	{

		public static void Main(string[] args)
		{
			BlockFileReader blocks = new BlockFileReader();

			Block_Disk b;
			Hash prevHash = new Byte[32];
			while ((b = blocks.getNext()) != null) {
				if (!b.prev_block.Equals(prevHash))
				{
					Console.WriteLine("Orphan found in blk" + blocks.blockFileNum.ToString("D5") + ".dat");
					break;
				}
				prevHash = b.Hash;
			}
			Console.WriteLine("Reached end of block database.");
			Console.ReadLine();
		}
	}
}