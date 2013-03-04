using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

namespace Bitcoin_Tool.Structs
{
	class Transaction : ISerialize
	{
		public UInt32 version;
		public VarInt numInputs { get { return new VarInt(inputs.Length); } }
		public TxIn[] inputs;
		public VarInt numOutputs { get { return new VarInt(outputs.Length); } }
		public TxOut[] outputs;
		public UInt32 lock_time;

		private Hash _hash = null;
		public Hash hash
		{
			get
			{
				if (_hash == null)
				{
					SHA256 sha256 = new SHA256Managed();
					using (MemoryStream ms = new MemoryStream(80))
					{
						this.Write(ms);
						_hash = sha256.ComputeHash(sha256.ComputeHash(ms.ToArray())).ToArray();
					}
				}
				return _hash;
			}
		}

		protected Transaction()
		{
		}

		public Transaction(UInt32 version, TxIn[] inputs, TxOut[] outputs, UInt32 lock_time)
		{
			this.version = version;
			this.inputs = inputs;
			this.outputs = outputs;
			this.lock_time = lock_time;
		}

		public Transaction(Byte[] b)
		{
			using (MemoryStream ms = new MemoryStream(b))
				Read(ms);
		}

		public void Read(Stream s)
		{
			BinaryReader br = new BinaryReader(s);
			version = br.ReadUInt32();
			inputs = new TxIn[VarInt.FromStream(s)];
			for (int i = 0; i < inputs.Length; i++)
				inputs[i] = TxIn.FromStream(s);
			outputs = new TxOut[VarInt.FromStream(s)];
			for (int i = 0; i < outputs.Length; i++)
				outputs[i] = TxOut.FromStream(s);
			lock_time = br.ReadUInt32();
		}

		public void Write(Stream s)
		{
			BinaryWriter bw = new BinaryWriter(s);
			bw.Write((UInt32)version);
			numInputs.Write(s);
			for (int i = 0; i < inputs.Length; i++)
				inputs[i].Write(s);
			numOutputs.Write(s);
			for (int i = 0; i < outputs.Length; i++)
				outputs[i].Write(s);
			bw.Write((UInt32)lock_time);
		}

		public Byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Write(ms);
				return ms.ToArray();
			}
		}

		public static Transaction FromStream(Stream s)
		{
			Transaction x = new Transaction();
			x.Read(s);
			return x;
		}
	}
}
