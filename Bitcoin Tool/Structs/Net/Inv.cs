using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bitcoin_Tool.Structs.Net
{
	public class Inv : IPayload
	{
		public VarInt count { get { return new VarInt(inventory.Length); } }
		public InvVect[] inventory;

		protected Inv()
		{
		}

		public Inv(InvVect[] inventory)
		{
			this.inventory = inventory;
		}

		public Inv(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public void Read(Stream s)
		{
			inventory = new InvVect[VarInt.FromStream(s)];
			for (int i = 0; i < inventory.Length; i++)
				inventory[i] = InvVect.FromStream(s);
		}

		public void Write(Stream s)
		{
			count.Write(s);
			for (int i = 0; i < inventory.Length; i++)
				inventory[i].Write(s);
		}

		public byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static Inv FromStream(Stream s)
		{
			Inv x = new Inv();
			x.Read(s);
			return x;
		}
	}
}
