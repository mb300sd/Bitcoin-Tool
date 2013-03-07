using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitcoin_Tool.Util
{
	public static class UnixTimestamp
	{
		public static Int64 Now
		{
			get
			{
				return (Int64)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local)).TotalSeconds;
			}
		}
		
	}
}
