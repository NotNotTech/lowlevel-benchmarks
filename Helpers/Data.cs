using System.Security.Cryptography;

namespace lowlevel_benchmark;
public unsafe struct Data
{
	private static ThreadLocal<SHA512> TL_Sha512 = new ThreadLocal<SHA512>(() => SHA512.Create());

	public long key;
	public int writeCount;

	public bool isInit;

	private fixed byte _someData[64];

	public Data(long key)
	{
		this.key = key;
		writeCount = 0;
		isInit = true;
	}

	public void Write()
	{
		Span<byte> input = stackalloc byte[32];
		//Span<byte> output = stackalloc byte[64];
		fixed(byte* pSpan = _someData)
		{
			Span<byte> output = new Span<byte>(pSpan, 64); 
			var result = TL_Sha512.Value.TryComputeHash(input, output, out var bytesWritten);
			if (result && bytesWritten > 0)
			{
				writeCount++;
			}
		}
		
		
	}
}





