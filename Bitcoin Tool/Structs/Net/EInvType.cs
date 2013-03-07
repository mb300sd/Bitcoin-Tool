using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitcoin_Tool.Structs.Net
{
	public enum InvType : uint
	{
		ERROR = 0,
		MSG_TX = 1,
		MSG_BLOCK = 2
	}
}
