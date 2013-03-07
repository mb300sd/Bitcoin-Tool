using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	public class GetAddr : EmptyPayload, IPayload
	{
		public static GetAddr FromStream (Stream s) {
			return new GetAddr();
		}
	}
}
