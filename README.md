# parallel-work-gc-benchmarks
benchmarking ways of parallel processing big spans/data while being considerate of GC



## how to use
1. open solution in visual studio 2022
2. run solution
3. wait a long time for benchmarks to run


## structure
- `Program.cs` - entrypoint
- `Benchmark.cs` - benchmark tests
- `DumbWork.cs` - helper containing input data and output verification logic
- `Data.cs` - helper containing structure of test data worked on in benchmarks
- `zz_Extensions.cs` - extension method for `Span<T>` and `Array` to make parallel easier.


## The Benchmarks

Benchmark code follows this pattern, taken from the `Serial_Default` baseline:
```cs
	/// <summary>
	/// all serial benchmarks try to change only one thing from this default baseline.  
	/// </summary>
	[Benchmark(Baseline = true)]
	public long Serial_Default()
	{
		var keys = dumbWork.keys;  //input data:  random keys in an unordered array

		//the benchmark will generate data, do two writes, and store it here
		Span<Data> output = stackalloc Data[DumbWork.DATA_LENGTH];  //DATA_LENGTH=10000

		//make a data, do a write to it, and store it
		for (var i = 0; i < keys.Length; i++)
		{
			var key = keys[i];
			var data = new Data(key);
			data.Write();
			output[i] = data;
		}
		//loop again writing once more
		for (var i = 0; i < output.Length; i++)
		{
			output[i].Write();
		}
		//verify benchmark output is correct
		return dumbWork.Verify(output);
	}
```


## Benchmark output
Below are my findings on my dev PC.  

```cmd
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1237 (20H2/October2020Update)
AMD Ryzen 7 3700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.100-rc.1.21458.32
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT


| Method                                 |        Mean |      Error |     StdDev | Ratio | RatioSD |    Gen 0 |    Gen 1 |    Gen 2 |   Allocated |
| -------------------------------------- | ----------: | ---------: | ---------: | ----: | ------: | -------: | -------: | -------: | ----------: |
| Serial_Default                         | 7,015.27 us | 129.919 us | 121.527 us | 1.000 |    0.00 |        - |        - |        - |         5 B |
| Serial_AllocOneObj                     | 7,101.17 us | 136.635 us | 127.808 us | 1.013 |    0.03 |        - |        - |        - |        28 B |
| Serial_AllocOneObjPerLoop              | 7,410.49 us | 139.494 us | 123.658 us | 1.056 |    0.03 |  23.4375 |        - |        - |   240,028 B |
| Serial_NaiveAlloc                      | 7,313.86 us |  88.510 us |  73.910 us | 1.045 |    0.02 | 242.1875 | 242.1875 | 242.1875 |   800,109 B |
| Serial_ByRef                           | 7,217.72 us | 138.276 us | 153.693 us | 1.024 |    0.03 |        - |        - |        - |         4 B |
| Serial_SpanOwner                       | 7,161.89 us | 139.117 us | 180.892 us | 1.019 |    0.02 |        - |        - |        - |         6 B |
| Serial_MemoryOwner                     | 7,141.44 us | 142.226 us | 229.669 us | 1.018 |    0.04 |        - |        - |        - |        46 B |
| Serial_SpanOwner_Array                 | 7,147.11 us | 141.356 us | 178.771 us | 1.015 |    0.04 |        - |        - |        - |         6 B |
| Serial_MemoryOwner_Array               | 7,178.14 us | 132.140 us | 103.166 us | 1.025 |    0.02 |        - |        - |        - |        46 B |
| Serial_SpanOwner_NoUsing               | 7,503.53 us | 138.883 us | 129.911 us | 1.070 |    0.02 | 328.1250 | 328.1250 | 328.1250 | 1,310,859 B |
| Serial_MemoryOwner_NoUsing             | 6,990.89 us |  86.516 us |  80.927 us | 0.997 |    0.02 |        - |        - |        - |        46 B |
| ParallelFor_NoWork                     |    48.08 us |   0.443 us |   0.393 us | 0.007 |    0.00 |   1.0986 |        - |        - |     9,187 B |
| ParallelFor_Lambda                     |   958.28 us |  16.580 us |  15.509 us | 0.137 |    0.00 |   0.9766 |        - |        - |     9,333 B |
| ParallelFor_Lambda_Fixed               |   970.85 us |  18.721 us |  22.991 us | 0.140 |    0.00 |   0.9766 |        - |        - |     9,266 B |
| ParallelFor_InstanceMethod             |   976.76 us |  16.021 us |  13.378 us | 0.140 |    0.00 |   0.9766 |        - |        - |     9,158 B |
| ParallelHelperFor_Standard             | 1,073.96 us |  20.165 us |  29.557 us | 0.152 |    0.00 |        - |        - |        - |     9,511 B |
| ParallelHelperFor_SpanExtensionLambda  | 1,086.22 us |  19.659 us |  18.389 us | 0.155 |    0.00 |        - |        - |        - |     9,626 B |
| ParallelHelperFor_ArrayExtensionLambda | 1,069.59 us |  21.079 us |  20.703 us | 0.153 |    0.00 |        - |        - |        - |     9,603 B |

// * Legends *
  Mean      : Arithmetic mean of all measurements
  Error     : Half of 99.9% confidence interval
  StdDev    : Standard deviation of all measurements
  Ratio     : Mean of the ratio distribution ([Current]/[Baseline])
  RatioSD   : Standard deviation of the ratio distribution ([Current]/[Baseline])
  Gen 0     : GC Generation 0 collects per 1000 operations
  Gen 1     : GC Generation 1 collects per 1000 operations
  Gen 2     : GC Generation 2 collects per 1000 operations
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 us      : 1 Microsecond (0.000001 sec)
```



### Thoughts...
- **Speed is good**. This isn't a benchmark on parallel speed, it's looking at allocations.  
  - That said, generally the parallel computation speed seems favorable.  Be aware that verification code is done serially even in Parallel benchmarks.
- **Parallel allocations are unavoidable**. additional object allocations are annoying (but relatively small).   
  - In the benchmarks, allocating one object per loop (benchmark `Serial_AllocOneObjPerLoop` above) allocates 240kb.  Compared to that, the 9kb emitted by the Parallel tests is basically nothing.
- **`MemoryOwner<T>` instead of `SpanOwner<T>`**.  Both are great, but the tiny extra allocation overhead of `MemoryOwner` is insignificant compared to it's flexibility (use in `async/await`, Parallel/Lambdas).  
  - Also compared to the allocation cost of running a `Parallel.For` MemoryOwner allocations are effectively free.
- **Lambdas/delegate parameter capture doesnt hurt**.  Compare the "Lambda" benchmarks such as `ParallelFor_Lambda` with others.  There doesn't seem to be any difference in allocations or execution speed.







