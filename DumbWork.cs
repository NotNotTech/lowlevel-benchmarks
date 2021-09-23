using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parallel_work_gc_benchmark;

/// <summary>
/// helper to obtain benchmark input and verify output.
/// Use via the INSTANCE static.
/// </summary>
public class DumbWork
{
	public static DumbWork INSTANCE = new();


	public const int DATA_LENGTH = 10000;


	/// <summary>
	/// array of random keys used for benchmark input
	/// </summary>
	public long[] keys = new long[DATA_LENGTH];

	/// <summary>
	/// simple checksum to ensure that all kys are present in output
	/// </summary>
	private long _cachedKeySum = 0;


	/// <summary>
	/// all benchmark algos are expected to create a span that matches this verification.  to see how output is formed, see the DumbWork.ctor() method;
	/// </summary>
	public long Verify(Span<Data> output)
	{
		long keysSum = 0;
		if (output.Length != DATA_LENGTH)
		{
			throw new ApplicationException("benchmark output does not match: wrong output.Length");
		}
		for (var i = 0; i < output.Length; i++)
		{
			if (output[i].writeCount != 2)
			{
				throw new ApplicationException("benchmark output does not match: writeCounts should all equal 2");
			}
			keysSum += output[i].key;
		}
		if (keysSum != _cachedKeySum)
		{
			throw new ApplicationException("benchmark output does not match: sum of keys should match our stored _keysSum");
		}
		return keysSum;
	}

	private DumbWork()
	{
		//generate keys
		{
			var rand = new Random();
			var hashSet = new HashSet<long>();
			while (hashSet.Count < DATA_LENGTH)
			{
				var key = rand.NextInt64();
				if (hashSet.Add(key))
				{
					_cachedKeySum += key;
				}
			}
			keys = hashSet.ToArray();
		}


		//algo that all benchmarks should follow to produce matching output
		{
			Span<Data> output = new Data[DumbWork.DATA_LENGTH];

			for (var i = 0; i < keys.Length; i++)
			{
				var key = keys[i];
				var data = new Data(key);
				data.Write();
				output[i] = data;
			}
			for (var i = 0; i < output.Length; i++)
			{
				output[i].Write();
			}
		}
	}









}





