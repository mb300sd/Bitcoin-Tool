using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

namespace Bitcoin_Tool.Structs
{
	class Block : ISerialize
	{
		public UInt32 version;
		public Byte[] prev_block;
		public Byte[] merkle_root;
		public UInt32 timestamp;
		public UInt32 bits;
		public UInt32 nonce;
		public VarInt tx_count { get { return new VarInt(txns.Length); } }
		public Transaction[] txns = new Transaction[0];

		private Hash _hash = null;
		public Hash hash
		{
			get
			{
				if (_hash == null)
				{
					SHA256 sha256 = new SHA256Managed();
					using (MemoryStream ms = new MemoryStream())
					{
						this.Write(ms);
						_hash = sha256.ComputeHash(sha256.ComputeHash(ms.ToArray())).ToArray();
					}
				}
				return _hash;
			}
		}

		protected Block()
		{
		}

		public Block(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public virtual void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			version = br.ReadUInt32();
			prev_block = br.ReadBytes(32);
			merkle_root = br.ReadBytes(32);
			timestamp = br.ReadUInt32();
			bits = br.ReadUInt32();
			nonce = br.ReadUInt32();
			txns = new Transaction[VarInt.FromStream(s)];
			for (int i = 0; i < txns.Length; i++)
				txns[i] = Transaction.FromStream(s);
		}

		private void WriteHeader(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write((UInt32)version);
			bw.Write(prev_block, 0, 32);
			bw.Write(merkle_root, 0, 32);
			bw.Write((UInt32)timestamp);
			bw.Write((UInt32)bits);
			bw.Write((UInt32)nonce);
		}

		public virtual void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			WriteHeader(s);
			tx_count.Write(s);
			for (int i = 0; i < txns.Length; i++)
				txns[i].Write(s);
		}

		public virtual Byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static Block FromStream(Stream s)
		{
			Block x = new Block();
			x.Read(s);
			return x;
		}
	}
}
