using System;
using System.IO;
using Bitcoin_Tool.Structs;

namespace Bitcoin_Tool.Util
{
	public class BlockFileReader
	{
		public int blockFileNum { get; private set; }
			FileStream fs;

			public BlockFileReader()
			{
				blockFileNum = 0;
				fs = new FileStream(@"C:\Users\Administrator\AppData\Roaming\Bitcoin\blocks\blk" + blockFileNum.ToString("D5") + ".dat", FileMode.Open);
			}

			public BlockFileReader(string file)
			{
				blockFileNum = 0;
				fs = new FileStream(file, FileMode.Open);
			}

			public Block_Disk getNext()
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
	}
}
