using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	class Headers : Structs.Block, IPayload
	{
		protected Headers()
		{
		}

		public Headers(Byte[] b)
			: base(b)
		{
		}

		public static new Headers FromStream(Stream s)
		{
			Headers x = new Headers();
			x.Read(s);
			return x;
		}
	}
}
