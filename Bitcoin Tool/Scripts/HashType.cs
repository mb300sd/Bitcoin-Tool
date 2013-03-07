namespace Bitcoin_Tool.Scripts
{
	public enum HashType : byte
	{
		SIGHASH_ALL = 0x01,
		SIGHASH_NONE = 0x02,
		SIGHASH_SINGLE = 0x03,
		SIGHASH_ANYONECANPAY = 0x80
	}
}
