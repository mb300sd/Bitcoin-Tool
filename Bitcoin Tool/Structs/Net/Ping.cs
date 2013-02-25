using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	class Ping : EmptyPayload, IPayload
	{
		public static Ping FromStream(Stream s)
		{
			return new Ping();
		}
	}
}