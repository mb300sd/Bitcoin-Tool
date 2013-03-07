using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	public class Block : Structs.Block, IPayload
	{
		protected Block()
		{
		}

		public Block(Byte[] b)
			: base(b)
		{
		}

		public static new Block FromStream(Stream s)
		{
			Block x = new Block();
			x.Read(s);
			return x;
		}
	}
}
