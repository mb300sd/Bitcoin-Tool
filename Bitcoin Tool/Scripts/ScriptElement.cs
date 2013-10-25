using System;

namespace Bitcoin_Tool.Scripts
{
	public struct ScriptElement
	{
		public OpCode opCode;
		public Byte[] data;
		public Boolean isData
		{
			get
			{
				return data != null;
			}
		}

		public Boolean matchesPubKey
		{
			get
			{
				return (data != null) && (33 <= data.Length && data.Length <= 120);
			}
		}

		public Boolean matchesPubKeyHash
		{
			get
			{
				return (data != null) && data.Length == (160 / 8);
			}
		}

		public Boolean matchesScriptHash
		{
			get
			{
				return (data != null) && data.Length == (160 / 8);
			}
		}

		public Boolean matchesSmallInteger
		{
			get
			{
				return (opCode == OpCode.OP_0) || ((Byte)OpCode.OP_1 <= (Byte)opCode && (Byte)opCode <= (Byte)OpCode.OP_16);
			}
		}

		public Boolean matchesSmallData
		{
			get
			{
				return (data != null) && (data.Length <= 80);
			}
		}

		public ScriptElement(OpCode opCode, Byte[] data = null)
		{
			this.opCode = opCode;
			this.data = data;
			if (opCode == OpCode.OP_0)
				this.data = new ScriptVarInt(0).ToBytes();
			else if (opCode == OpCode.OP_1NEGATE || (OpCode.OP_1 <= opCode && opCode <= OpCode.OP_16))
				this.data = new ScriptVarInt((int)opCode - ((int)OpCode.OP_1 - 1)).ToBytes();
		}

		public ScriptElement(Byte[] data)
		{
			this.data = data;
			if (0x01 <= data.Length && data.Length <= 0x4b)
				this.opCode = (OpCode)data.Length;
			else if (data.Length <= Byte.MaxValue)
				this.opCode = OpCode.OP_PUSHDATA1;
			else if (data.Length <= UInt16.MaxValue)
				this.opCode = OpCode.OP_PUSHDATA2;
			else
				this.opCode = OpCode.OP_PUSHDATA4;
		}
	}
}
