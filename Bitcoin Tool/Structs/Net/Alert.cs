using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	public class Alert : IPayload
	{
		/*
		public class AlertData
		{
			public Int32 Version;
			public Int64 RelayUntil;
			public Int64 Expiration;
			public Int32 ID;
			public Int32 Cancel;
			
			// TODO: FIX! Not actually VarStr
			public VarStr setCancel;
			
			public Int32 MinVer;
			public Int32 MaxVer;

			// TODO: FIX! Not actually VarStr
			public VarStr setSubVer;

			public Int32 Priority;
			public VarStr Comment;
			public VarStr StatusBar;
			public VarStr Reserved;
		}
		*/

		public VarStr payload;
		public VarStr signature;

		protected Alert()
		{
		}

		public Alert(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public void Read(Stream s)
		{
			payload = VarStr.FromStream(s);
			signature = VarStr.FromStream(s);
		}

		public void Write(Stream s)
		{
			payload.Write(s);
			signature.Write(s);
		}

		public byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static Alert FromStream(Stream s)
		{
			Alert a = new Alert();
			a.Read(s);
			return a;
		}
	}
}