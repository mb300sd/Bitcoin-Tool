using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs
{
	interface ISerialize
	{
		void Read(Stream s);
		void Write(Stream s);
		Byte[] ToBytes();
	}
}
