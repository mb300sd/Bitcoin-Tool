using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Bitcoin_Tool.DataConverters;
using Bitcoin_Tool.Structs;
using Bitcoin_Tool.Crypto;

namespace Bitcoin_Tool.Scripts
{
	public class Script
	{
		private enum IfElseExec
		{
			RUN_IF,
			RUN_ELSE,
			SKIP_IF,
			SKIP_ELSE
		}

		private Stack<Byte[]> stack = new Stack<Byte[]>();
		private Stack<Byte[]> altstack = new Stack<Byte[]>();

		private RIPEMD160 ripemd160 = new RIPEMD160Managed();
		private SHA1 sha1 = new SHA1Managed();
		private SHA256 sha256 = new SHA256Managed();

		public List<ScriptElement> elements = new List<ScriptElement>();

		public Script()
		{
		}

		public Script(Byte[] script)
		{
			using (MemoryStream ms = new MemoryStream(script))
			using (BinaryReader br = new BinaryReader(ms))
			{
				while (ms.Position < ms.Length)
					elements.Add(readElement(br));
			}
		}

		public Script(Byte[] scriptSig, Byte[] scriptPubKey)
		{
			using (MemoryStream ms = new MemoryStream(scriptSig))
			using (BinaryReader br = new BinaryReader(ms))
			{
				while (ms.Position < ms.Length)
					elements.Add(readElement(br));
			}
			elements.Add(new ScriptElement(OpCode.OP_CODESEPARATOR));
			using (MemoryStream ms = new MemoryStream(scriptPubKey))
			using (BinaryReader br = new BinaryReader(ms))
			{
				while (ms.Position < ms.Length)
					elements.Add(readElement(br));
			}
		}

		public Script(Script scriptSig, Script scriptPubKey)
		{
			elements = scriptSig.elements
				.Concat(new ScriptElement[] {new ScriptElement(OpCode.OP_CODESEPARATOR)})
				.Concat(scriptPubKey.elements)
				.ToList();
		}

		public Script(IEnumerable<ScriptElement> elements)
		{
			this.elements = elements.ToList();
		}

		private static ScriptElement readElement(BinaryReader br)
		{
			OpCode opCode = (OpCode)br.ReadByte();
			switch (opCode)
			{
				case OpCode.OP_0:
					{
						return new ScriptElement(opCode, new Byte[] { 0x00 });
					}
				case OpCode.OP_PUSHDATA1:
					{
						int length = br.ReadByte();
						return new ScriptElement(opCode, br.ReadBytes(length));
					}
				case OpCode.OP_PUSHDATA2:
					{
						int length = br.ReadUInt16();
						return new ScriptElement(opCode, br.ReadBytes(length));
					}
				case OpCode.OP_PUSHDATA4:
					{
						int length = (int)br.ReadUInt32();
						return new ScriptElement(opCode, br.ReadBytes(length));
					}
				case OpCode.OP_1NEGATE:
				case OpCode.OP_1:
				case OpCode.OP_2:
				case OpCode.OP_3:
				case OpCode.OP_4:
				case OpCode.OP_5:
				case OpCode.OP_6:
				case OpCode.OP_7:
				case OpCode.OP_8:
				case OpCode.OP_9:
				case OpCode.OP_10:
				case OpCode.OP_11:
				case OpCode.OP_12:
				case OpCode.OP_13:
				case OpCode.OP_14:
				case OpCode.OP_15:
				case OpCode.OP_16:
					{
						return new ScriptElement(opCode, new ScriptVarInt((int)opCode - ((int)OpCode.OP_1 - 1)).ToBytes());
					}
				default:
					{
						if (0x01 <= (Byte)opCode && (Byte)opCode <= 0x4b)
						{
							return new ScriptElement(opCode, br.ReadBytes((int)opCode));
						}
						else
						{
							return new ScriptElement(opCode);
						}
					}
			}
		}

		public bool Evaluate(Transaction tx = null, UInt32 txInIndex = 0)
		{
			Stack<IfElseExec> ifExec = new Stack<IfElseExec>();
			int lastCodeSeperator = 0;

			stack.Clear();
			altstack.Clear();

			for (int index = 0; index < elements.Count; index++)
			{
				ScriptElement se = elements[index];
				if (ifExec.Count > 0)
					if ((ifExec.First() == IfElseExec.SKIP_IF && !(se.opCode == OpCode.OP_ELSE || se.opCode == OpCode.OP_ENDIF)) ||
						(ifExec.First() == IfElseExec.SKIP_ELSE && !(se.opCode == OpCode.OP_ENDIF)))
						continue;

				if (se.isData)
				{
					stack.Push(se.data);
					continue;
				}

				switch (se.opCode)
				{
					case OpCode.OP_RESERVED:
						{
							return false;
						}
					case OpCode.OP_NOP:
						{
							// NOP, Dump stack for debug
							/*
							stack.ToList().ForEach(x => Console.WriteLine(HexString.FromByteArray(x)));
							Console.WriteLine("---------------------------");
							*/
							break;
						}
					case OpCode.OP_VER:
						{
							return false;
						}
					case OpCode.OP_IF:
					case OpCode.OP_NOTIF:
						{
							if (stack.Count < 1)
								return false;
							Boolean stackIsZero = new ScriptVarInt(stack.Pop()).value.IsZero;
							if ((se.opCode == OpCode.OP_IF && !stackIsZero) || (se.opCode == OpCode.OP_NOTIF && stackIsZero))
							{
								// Run to else or endif
								ifExec.Push(IfElseExec.RUN_IF);
							}
							else
							{
								// Skip to else or endif
								ifExec.Push(IfElseExec.SKIP_IF);
							}
							break;
						}
					case OpCode.OP_VERIF:
					case OpCode.OP_VERNOTIF:
						{
							return false;
						}
					case OpCode.OP_ELSE:
						{
							if (ifExec.Count == 0)
								// No preceding if statement!
								return false;
							if (ifExec.First() == IfElseExec.RUN_IF)
							{
								// If ran, skip else
								ifExec.Push(IfElseExec.SKIP_ELSE);
							}
							else if (ifExec.First() == IfElseExec.SKIP_IF)
							{
								// If did not run, run else
								ifExec.Push(IfElseExec.RUN_ELSE);
							}
							break;
						}
					case OpCode.OP_ENDIF:
						{
							if (ifExec.Count == 0)
								// No preceding if statement!
								return false;
							if (ifExec.First() == IfElseExec.RUN_IF ||
								ifExec.First() == IfElseExec.SKIP_IF)
							{
								ifExec.Pop();
							}
							else if (ifExec.First() == IfElseExec.RUN_ELSE ||
							   ifExec.First() == IfElseExec.SKIP_ELSE)
							{
								ifExec.Pop();
								ifExec.Pop();
							}
							break;
						}
					case OpCode.OP_VERIFY:
						{
							if (stack.Count < 1 || new ScriptVarInt(stack.First()).value.IsZero)
								return false;
							stack.Pop();
							break;
						}
					case OpCode.OP_RETURN:
						{
							return false;
						}
					case OpCode.OP_TOALTSTACK:
						{
							if (stack.Count < 1)
								return false;
							altstack.Push(stack.Pop());
							break;
						}
					case OpCode.OP_FROMALTSTACK:
						{
							if (altstack.Count < 1)
								return false;
							stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_2DROP:
						{
							if (stack.Count < 2)
								return false;
							stack.Pop();
							stack.Pop();
							break;
						}
					case OpCode.OP_2DUP:
						{
							if (stack.Count < 2)
								return false;
							stack.Push(stack.Skip(1).First());
							stack.Push(stack.Skip(1).First());
							break;
						}
					case OpCode.OP_3DUP:
						{
							if (stack.Count < 3)
								return false;
							stack.Push(stack.Skip(2).First());
							stack.Push(stack.Skip(2).First());
							stack.Push(stack.Skip(2).First());
							break;
						}
					case OpCode.OP_2OVER:
						{
							if (stack.Count < 4)
								return false;
							stack.Push(stack.Skip(3).First());
							stack.Push(stack.Skip(3).First());
							break;
						}
					case OpCode.OP_2ROT:
						{
							if (stack.Count < 6)
								return false;
							altstack.Push(stack.Skip(4).First());
							altstack.Push(stack.Skip(5).First());
							altstack.Push(stack.Pop());
							altstack.Push(stack.Pop());
							altstack.Push(stack.Pop());
							altstack.Push(stack.Pop());
							stack.Pop();
							stack.Pop();
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_2SWAP:
						{
							if (stack.Count < 4)
								return false;
							altstack.Push(stack.Skip(2).First());
							altstack.Push(stack.Skip(3).First());
							altstack.Push(stack.Pop());
							altstack.Push(stack.Pop());
							stack.Pop();
							stack.Pop();
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_IFDUP:
						{
							if (stack.Count < 1)
								return false;
							if (!new ScriptVarInt(stack.First()).value.IsZero)
								stack.Push(stack.First());
							break;
						}
					case OpCode.OP_DEPTH:
						{
							stack.Push(new ScriptVarInt((UInt64)stack.Count).ToBytes());
							break;
						}
					case OpCode.OP_DROP:
						{
							if (stack.Count < 1)
								return false;
							stack.Pop();
							break;
						}
					case OpCode.OP_DUP:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(stack.First());
							break;
						}
					case OpCode.OP_NIP:
						{
							if (stack.Count < 2)
								return false;
							altstack.Push(stack.Pop());
							stack.Pop();
							stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_OVER:
						{
							if (stack.Count < 2)
								return false;
							stack.Push(stack.Skip(1).First());
							break;
						}
					case OpCode.OP_PICK:
						{
							if (stack.Count < 2)
								return false;
							int pick = new ScriptVarInt(stack.Pop()).intValue;
							if (pick < 0 || stack.Count <= pick)
								return false;
							stack.Push(stack.Skip(pick).First());
							break;
						}
					case OpCode.OP_ROLL:
						{
							if (stack.Count < 2)
								return false;
							int roll = new ScriptVarInt(stack.Pop()).intValue;
							if (roll < 0 || stack.Count <= roll)
								return false;
							altstack.Push(stack.Skip(roll).First());
							for (int i = 0; i < roll; i++)
								altstack.Push(stack.Pop());
							stack.Pop();
							for (int i = 0; i < roll + 1; i++)
								stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_ROT:
						{
							if (stack.Count < 3)
								return false;
							altstack.Push(stack.Skip(2).First());
							altstack.Push(stack.Pop());
							altstack.Push(stack.Pop());
							stack.Pop();
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_SWAP:
						{
							if (stack.Count < 2)
								return false;
							altstack.Push(stack.Skip(1).First());
							altstack.Push(stack.Pop());
							stack.Pop();
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_TUCK:
						{
							if (stack.Count < 2)
								return false;
							altstack.Push(stack.Pop());
							altstack.Push(stack.Pop());
							stack.Push(altstack.Skip(1).First());
							stack.Push(altstack.Pop());
							stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_CAT:
						{
							if (stack.Count < 2)
								return false;
							altstack.Push(stack.Skip(1).First().Concat(stack.First()).ToArray());
							stack.Pop();
							stack.Pop();
							stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_SUBSTR:
						{
							if (stack.Count < 3)
								return false;
							altstack.Push(stack.Skip(2).First()
								.Skip(new ScriptVarInt(stack.Skip(1).First()).intValue)
								.Take(new ScriptVarInt(stack.First()).intValue).ToArray());
							stack.Pop();
							stack.Pop();
							stack.Pop();
							stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_LEFT:
						{
							if (stack.Count < 2)
								return false;
							altstack.Push(stack.Skip(1).First()
								.Take(new ScriptVarInt(stack.First()).intValue).ToArray());
							stack.Pop();
							stack.Pop();
							stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_RIGHT:
						{
							if (stack.Count < 2)
								return false;
							int count = new ScriptVarInt(stack.First()).intValue;
							altstack.Push(stack.Skip(1).First()
								.Skip(stack.Skip(1).First().Length - count).ToArray());
							stack.Pop();
							stack.Pop();
							stack.Push(altstack.Pop());
							break;
						}
					case OpCode.OP_SIZE:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(new ScriptVarInt((UInt64)stack.First().Length).ToBytes());
							break;
						}
					case OpCode.OP_INVERT:
						{
							if (stack.Count < 1)
								return false;
							Byte[] data = stack.Pop();
							for (int i = 0; i < data.Length; i++)
								data[i] ^= 0xFF;
							stack.Push(data);
							break;
						}
					case OpCode.OP_AND:
					case OpCode.OP_OR:
					case OpCode.OP_XOR:
						{
							if (stack.Count < 2)
								return false;
							Byte[] data = stack.Pop();
							Byte[] data2 = stack.Pop();
							int length = new int[] { data.Length, data2.Length }.Max();
							data = data.Concat(new Byte[length - data.Length]).ToArray();
							data2 = data2.Concat(new Byte[length - data2.Length]).ToArray();
							switch (se.opCode)
							{
								case OpCode.OP_AND:
									{
										for (int i = 0; i < length; i++)
											data[i] &= data2[i];
										break;
									}
								case OpCode.OP_OR:
									{
										for (int i = 0; i < length; i++)
											data[i] |= data2[i];
										break;
									}
								case OpCode.OP_XOR:
									{
										for (int i = 0; i < length; i++)
											data[i] ^= data2[i];
										break;
									}
							}
							stack.Push(data);
							break;
						}
					case OpCode.OP_EQUAL:
					case OpCode.OP_EQUALVERIFY:
						{
							if (stack.Count < 2)
								return false;
							if (stack.Pop().SequenceEqual(stack.Pop()))
								stack.Push(new Byte[] { 0x01 });
							else
								stack.Push(new Byte[] { 0x00 });

							if (se.opCode == OpCode.OP_EQUALVERIFY)
							{
								if (stack.First().Single() == 0x00)
									return false;
								stack.Pop();
							}
							break;
						}
					case OpCode.OP_RESERVED1:
					case OpCode.OP_RESERVED2:
						{
							return false;
						}
					case OpCode.OP_1ADD:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(((ScriptVarInt)BigInteger.Add(new ScriptVarInt(stack.Pop()), 1)).ToBytes());
							break;
						}
					case OpCode.OP_1SUB:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(((ScriptVarInt)BigInteger.Subtract(new ScriptVarInt(stack.Pop()), 1)).ToBytes());
							break;
						}
					case OpCode.OP_2MUL:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(((ScriptVarInt)BigInteger.Multiply(new ScriptVarInt(stack.Pop()), 2)).ToBytes());
							break;
						}
					case OpCode.OP_2DIV:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(((ScriptVarInt)BigInteger.Divide(new ScriptVarInt(stack.Pop()), 2)).ToBytes());
							break;
						}
					case OpCode.OP_NEGATE:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(((ScriptVarInt)BigInteger.Negate(new ScriptVarInt(stack.Pop()))).ToBytes());
							break;
						}
					case OpCode.OP_ABS:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(((ScriptVarInt)BigInteger.Abs(new ScriptVarInt(stack.Pop()))).ToBytes());
							break;
						}
					case OpCode.OP_NOT:
						{
							if (stack.Count < 1)
								return false;
							if (new ScriptVarInt(stack.Pop()).value.IsZero)
								stack.Push(new Byte[] { 0x01 });
							else
								stack.Push(new Byte[] { 0x00 });
							break;
						}
					case OpCode.OP_0NOTEQUAL:
						{
							if (stack.Count < 1)
								return false;
							if (new ScriptVarInt(stack.Pop()).value.IsZero)
								stack.Push(new Byte[] { 0x00 });
							else
								stack.Push(new Byte[] { 0x01 });
							break;
						}
					case OpCode.OP_ADD:
					case OpCode.OP_SUB:
					case OpCode.OP_MUL:
					case OpCode.OP_DIV:
					case OpCode.OP_MOD:
					case OpCode.OP_LSHIFT:
					case OpCode.OP_RSHIFT:
						{
							if (stack.Count < 2)
								return false;
							ScriptVarInt b = new ScriptVarInt(stack.Pop());
							ScriptVarInt a = new ScriptVarInt(stack.Pop());
							switch (se.opCode)
							{
								case OpCode.OP_ADD:
									a = (BigInteger)a + b;
									break;
								case OpCode.OP_SUB:
									a = (BigInteger)a - b;
									break;
								case OpCode.OP_MUL:
									a = (BigInteger)a * b;
									break;
								case OpCode.OP_DIV:
									a = (BigInteger)a / b;
									break;
								case OpCode.OP_MOD:
									a = (BigInteger)a % b;
									break;
								case OpCode.OP_LSHIFT:
									a = (BigInteger)a << b.intValue;
									break;
								case OpCode.OP_RSHIFT:
									a = (BigInteger)a >> b.intValue;
									break;
							}
							stack.Push(a.ToBytes());
							break;
						}
					case OpCode.OP_BOOLAND:
						{
							if (stack.Count < 2)
								return false;
							if (!new ScriptVarInt(stack.Pop()).value.IsZero && !new ScriptVarInt(stack.Pop()).value.IsZero)
								stack.Push(new Byte[] { 0x01 });
							else
								stack.Push(new Byte[] { 0x00 });
							break;
						}
					case OpCode.OP_BOOLOR:
						{
							if (stack.Count < 2)
								return false;
							if (!new ScriptVarInt(stack.Pop()).value.IsZero || !new ScriptVarInt(stack.Pop()).value.IsZero)
								stack.Push(new Byte[] { 0x01 });
							else
								stack.Push(new Byte[] { 0x00 });
							break;
						}
					case OpCode.OP_NUMEQUAL:
					case OpCode.OP_NUMEQUALVERIFY:
						{
							if (stack.Count < 2)
								return false;
							if (new ScriptVarInt(stack.Pop()).value == new ScriptVarInt(stack.Pop()).value)
								stack.Push(new Byte[] { 0x01 });
							else
								stack.Push(new Byte[] { 0x00 });
							if (se.opCode == OpCode.OP_NUMEQUALVERIFY)
							{
								if (stack.First().Single() == 0x00)
									return false;
								stack.Pop();
							}
							break;
						}
					case OpCode.OP_NUMNOTEQUAL:
						{
							if (stack.Count < 2)
								return false;
							if (new ScriptVarInt(stack.Pop()).value != new ScriptVarInt(stack.Pop()).value)
								stack.Push(new Byte[] { 0x01 });
							else
								stack.Push(new Byte[] { 0x00 });
							break;
						}
					case OpCode.OP_LESSTHAN:
						{
							if (stack.Count < 2)
								return false;
							ScriptVarInt b = new ScriptVarInt(stack.Pop());
							ScriptVarInt a = new ScriptVarInt(stack.Pop());
							if ((BigInteger)a < b)
								stack.Push(new Byte[] { 0x01 });
							else
								stack.Push(new Byte[] { 0x00 });
							break;
						}
					case OpCode.OP_GREATERTHAN:
						{
							if (stack.Count < 2)
								return false;
							ScriptVarInt b = new ScriptVarInt(stack.Pop());
							ScriptVarInt a = new ScriptVarInt(stack.Pop());
							if ((BigInteger)a > b)
								stack.Push(new Byte[] { 0x01 });
							else
								stack.Push(new Byte[] { 0x00 });
							break;
						}
					case OpCode.OP_LESSTHANOREQUAL:
						{
							if (stack.Count < 2)
								return false;
							ScriptVarInt b = new ScriptVarInt(stack.Pop());
							ScriptVarInt a = new ScriptVarInt(stack.Pop());
							if ((BigInteger)a <= b)
								stack.Push(new Byte[] { 0x01 });
							else
								stack.Push(new Byte[] { 0x00 });
							break;
						}
					case OpCode.OP_GREATERTHANOREQUAL:
						{
							if (stack.Count < 2)
								return false;
							ScriptVarInt b = new ScriptVarInt(stack.Pop());
							ScriptVarInt a = new ScriptVarInt(stack.Pop());
							if ((BigInteger)a >= b)
								stack.Push(new Byte[] { 0x01 });
							else
								stack.Push(new Byte[] { 0x00 });
							break;
						}
					case OpCode.OP_MIN:
						{
							if (stack.Count < 2)
								return false;
							Byte[] b = stack.Pop();
							Byte[] a = stack.Pop();
							if (new ScriptVarInt(a).value < new ScriptVarInt(b).value)
								stack.Push(a);
							else
								stack.Push(b);
							break;
						}
					case OpCode.OP_MAX:
						{
							if (stack.Count < 2)
								return false;
							Byte[] b = stack.Pop();
							Byte[] a = stack.Pop();
							if (new ScriptVarInt(a).value > new ScriptVarInt(b).value)
								stack.Push(a);
							else
								stack.Push(b);
							break;
						}
					case OpCode.OP_WITHIN:
						{
							if (stack.Count < 3)
								return false;
							ScriptVarInt max = new ScriptVarInt(stack.Pop());
							ScriptVarInt min = new ScriptVarInt(stack.Pop());
							ScriptVarInt x = new ScriptVarInt(stack.Pop());
							if ((BigInteger)min <= x && (BigInteger)x <= max)
								stack.Push(new Byte[] { 0x01 });
							else
								stack.Push(new Byte[] { 0x00 });
							break;
						}
					case OpCode.OP_RIPEMD160:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(ripemd160.ComputeHash(stack.Pop()));
							break;
						}
					case OpCode.OP_SHA1:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(sha1.ComputeHash(stack.Pop()));
							break;
						}
					case OpCode.OP_SHA256:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(sha256.ComputeHash(stack.Pop()));
							break;
						}
					case OpCode.OP_HASH160:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(ripemd160.ComputeHash(sha256.ComputeHash(stack.Pop())));
							break;
						}
					case OpCode.OP_HASH256:
						{
							if (stack.Count < 1)
								return false;
							stack.Push(sha256.ComputeHash(sha256.ComputeHash(stack.Pop())));
							break;
						}
					case OpCode.OP_CODESEPARATOR:
						{
							lastCodeSeperator = index;
							break;
						}
					case OpCode.OP_CHECKSIG:
					case OpCode.OP_CHECKSIGVERIFY:
						{
							if (tx == null)
								throw new ArgumentNullException("Transaction is required if script contains OP_CHECKSIG[VERIFY]");
							if (stack.Count < 2)
								return false;

							PublicKey pubKey = new PublicKey(stack.Pop());
							Byte[] sig = stack.Pop();

							Script subScript = new Script(elements.Skip(lastCodeSeperator)
								.Where(x => x.opCode != OpCode.OP_CODESEPARATOR).ToArray());

							stack.Push(new ScriptVarInt(CheckSig(sig, pubKey, txInIndex, subScript, tx) ? 1 : 0).ToBytes());

							if (se.opCode == OpCode.OP_CHECKSIGVERIFY)
							{
								if (stack.First().Single() == 0x00)
									return false;
								stack.Pop();
							}
							break;
						}
					case OpCode.OP_CHECKMULTISIG:
					case OpCode.OP_CHECKMULTISIGVERIFY:
						{
							if (tx == null)
								throw new ArgumentNullException("Transaction is required if script contains OP_CHECKMULTISIG[VERIFY]");
							if (stack.Count < 2)
								return false;

							PublicKey[] pubKeys = new PublicKey[new ScriptVarInt(stack.Pop()).intValue];

							if (stack.Count < pubKeys.Length + 1)
								return false;

							for (int i = 0; i < pubKeys.Length; i++)
							{
								pubKeys[i] = new PublicKey(stack.Pop());
							}

							Byte[][] sigs = new Byte[new ScriptVarInt(stack.Pop()).intValue][];

							if (stack.Count < sigs.Length)
								return false;

							for (int i = 0; i < sigs.Length; i++)
							{
								sigs[i] = stack.Pop();
							}

							// Remove extra unused value from stack
							stack.Pop();

							Script subScript = new Script(elements.Skip(lastCodeSeperator)
								.Where(x => x.opCode != OpCode.OP_CODESEPARATOR).ToArray());

							int validSigs = 0;
							foreach (Byte[] sig in sigs)
							{
								foreach (PublicKey pubKey in pubKeys)
								{
									if (CheckSig(sig, pubKey, txInIndex, subScript, tx))
									{
										pubKeys = pubKeys.Where(x => x != pubKey).ToArray();
										validSigs++;
										break;
									}
								}
							}

							stack.Push(new ScriptVarInt((validSigs >= sigs.Length) ? 1 : 0).ToBytes());

							if (se.opCode == OpCode.OP_CHECKMULTISIGVERIFY)
							{
								if (stack.First().Single() == 0x00)
									return false;
								stack.Pop();
							}
							break;
						}
					case OpCode.OP_NOP1:
					case OpCode.OP_NOP2:
					case OpCode.OP_NOP3:
					case OpCode.OP_NOP4:
					case OpCode.OP_NOP5:
					case OpCode.OP_NOP6:
					case OpCode.OP_NOP7:
					case OpCode.OP_NOP8:
					case OpCode.OP_NOP9:
					case OpCode.OP_NOP10:
						{
							break;
						}
					case OpCode.OP_SMALLINTEGER:
					case OpCode.OP_PUBKEYS:
					case OpCode.OP_PUBKEYHASH:
					case OpCode.OP_PUBKEY:
					case OpCode.OP_INVALIDOPCODE:
					default:
						{
							return false;
						}

				}
			}
			if (ifExec.Count > 0)
				// Missing endif
				return false;
			if (stack.Count > 0 && new ScriptVarInt(stack.First()).value.IsZero)
				return false;
			return true;
		}

		private Boolean CheckSig(Byte[] sig, PublicKey pubKey, UInt32 txInIndex, Script subScript, Transaction tx)
		{
			HashType hashType = (HashType)sig.Last();
			sig = sig.Take(sig.Length - 1).ToArray();

			Transaction txCopy = tx.CopyForSigning(txInIndex, subScript, hashType);

			Byte[] txHash = txCopy.ToBytes().Concat(new Byte[] { (Byte)hashType, 0x00, 0x00, 0x00 }).ToArray();
			txHash = sha256.ComputeHash(sha256.ComputeHash(txHash));

			return pubKey.VerifySignature(txHash, sig);
		}

		public Byte[] ToBytes()
		{
			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(ms))
			{
				foreach (ScriptElement se in elements)
				{
					if (!se.isData)
					{
						bw.Write((Byte)se.opCode);
					}
					else
					{
						if (se.data.Length == 1 && se.data[0].Equals((Byte)se.opCode) &&
							(se.opCode == OpCode.OP_NEGATE || (OpCode.OP_1 <= se.opCode && se.opCode <= OpCode.OP_16)))
						{
							bw.Write((Byte)se.opCode);
						}
						else if ((Byte)se.opCode == se.data.Length && 0x01 <= se.data.Length && se.data.Length <= 0x4b)
						{
							bw.Write((Byte)se.opCode);
							bw.Write(se.data, 0, (Byte)se.opCode);
						}
						else if (se.opCode == OpCode.OP_PUSHDATA1 && se.data.Length <= Byte.MaxValue)
						{
							bw.Write((Byte)se.opCode);
							bw.Write((Byte)se.data.Length);
							bw.Write(se.data, 0, se.data.Length);
						}
						else if (se.opCode == OpCode.OP_PUSHDATA2 && se.data.Length <= UInt16.MaxValue)
						{
							bw.Write((Byte)se.opCode);
							bw.Write((UInt16)se.data.Length);
							bw.Write(se.data, 0, se.data.Length);
						}
						else if (se.opCode == OpCode.OP_PUSHDATA4)
						{
							bw.Write((Byte)se.opCode);
							bw.Write((UInt32)se.data.Length);
							bw.Write(se.data, 0, se.data.Length);
						}
						else
						{
							throw new InvalidDataException("Data invalid for opcode used");
						}
					}
				}
				return ms.ToArray();
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (ScriptElement se in elements)
			{
				if (se.isData)
				{
					sb.AppendFormat("{0} ", HexString.FromByteArray(se.data));
				}
				else
				{
					sb.AppendFormat("{0} ", se.opCode.ToString());
				}
			}
			return sb.Remove(sb.Length - 1, 1).ToString();
		}
	}
}
