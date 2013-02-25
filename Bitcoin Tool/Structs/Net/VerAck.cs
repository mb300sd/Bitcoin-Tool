using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	class VerAck : EmptyPayload, IPayload
	{
		public static GetAddr FromStream(Stream s)
		{
			return new GetAddr();
		}
	}
}
