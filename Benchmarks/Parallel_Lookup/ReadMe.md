# Description

testing random access to a data storage.   Tested storage collections are
  - Array
  - ArraySegment
  - List
  - Dictionary
  - ConcurrentDictionary

Default element count is 100,000 with each benchmark accessing each element once.

Every benchmark run accesses the data in a different order, to simulate random access of elements.


# Findings
- `List` and `ArraySegment` are similar, both slightly slower than `Array`
- `Span` is slightly faster than `Array`
- `Dictionary` and `ConcurrentDictionary` are slow.  about 30x slower than direct `Array` access.
   - if you need to access using a dictionary, use `.TryGet()` not indexers.  Indexers add another 10x cost.

# Conclusion
- Build storage using a `List', and use `System.Runtime.Interop.CollectionMarshal.AsSpan()` to do lookups
   - In the 'DumDum' engine: build storage using a List structure like `AllocSlotList<T>` and do direct access using `List._AsSpan` extension methods



# Output
```bash
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1237 (20H2/October2020Update)
AMD Ryzen 7 3700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.100-rc.1.21458.32
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT


|                       Method |        Mean |      Error |     StdDev |      Median | Ratio | RatioSD | Completed Work Items | Lock Contentions | Allocated |
|----------------------------- |------------:|-----------:|-----------:|------------:|------:|--------:|---------------------:|-----------------:|----------:|
|             Sequential_Array |   153.18 us |   2.908 us |   2.578 us |   152.78 us |  1.00 |    0.00 |                    - |                - |         - |
|              Sequential_Span |   142.37 us |   2.782 us |   3.617 us |   142.39 us |  0.93 |    0.03 |                    - |                - |         - |
|      Sequential_ArraySegment |   712.49 us |  14.072 us |  13.821 us |   712.92 us |  4.64 |    0.12 |                    - |                - |       1 B |
|              Sequential_List |   680.00 us |  12.746 us |  11.299 us |   681.09 us |  4.44 |    0.09 |                    - |                - |         - |
|        Sequential_ListUnsafe |   142.29 us |   2.837 us |   5.188 us |   141.65 us |  0.94 |    0.04 |                    - |                - |         - |
|              Sequential_Dict | 3,609.60 us |  94.073 us | 257.525 us | 3,619.48 us | 21.94 |    1.70 |                    - |                - |       2 B |
|             Sequential_CDict | 5,127.67 us | 265.993 us | 750.237 us | 4,896.34 us | 37.63 |    6.29 |                    - |                - |       2 B |
|       Sequential_Dict_TryGet | 3,126.96 us | 280.412 us | 795.482 us | 2,867.97 us | 23.42 |    6.12 |                    - |                - |       6 B |
|      Sequential_CDict_TryGet | 2,709.04 us |  86.318 us | 244.869 us | 2,635.22 us | 17.83 |    1.95 |                    - |                - |       2 B |
|        Sequential_Array_Task |   150.47 us |   2.405 us |   5.715 us |   149.34 us |  0.98 |    0.04 |                    - |                - |      72 B |
| Sequential_ArraySegment_Task |   714.50 us |  14.272 us |  31.025 us |   710.44 us |  4.73 |    0.17 |                    - |                - |      73 B |
|         Sequential_List_Task |   666.17 us |  12.321 us |  14.667 us |   664.88 us |  4.35 |    0.15 |                    - |                - |      72 B |
|         Sequential_Dict_Task | 3,233.33 us |  63.908 us | 148.117 us | 3,234.57 us | 21.28 |    0.92 |                    - |                - |      76 B |
|        Sequential_CDict_Task | 3,649.55 us |  72.021 us |  98.582 us | 3,634.32 us | 23.83 |    0.73 |                    - |                - |      74 B |
|               Parallel_Array |    93.02 us |   0.982 us |   0.871 us |    92.69 us |  0.61 |    0.01 |              12.2773 |           0.0026 |     824 B |
|                Parallel_Span |    90.33 us |   1.795 us |   1.679 us |    90.83 us |  0.59 |    0.01 |              12.1191 |           0.0017 |     824 B |
|        Parallel_ArraySegment |   175.00 us |   1.236 us |   1.096 us |   175.05 us |  1.14 |    0.03 |              15.8958 |           0.0010 |     824 B |
|                Parallel_List |   167.03 us |   1.510 us |   1.261 us |   166.54 us |  1.09 |    0.02 |              15.8506 |           0.0002 |     824 B |
|         Parallel_List_Unsafe |    88.01 us |   0.451 us |   0.400 us |    88.01 us |  0.57 |    0.01 |              11.8629 |           0.0009 |     824 B |
|                Parallel_Dict |   401.55 us |   4.950 us |   4.133 us |   400.29 us |  2.62 |    0.06 |              15.9673 |           0.0020 |     824 B |
|               Parallel_CDict |   449.48 us |   8.900 us |   7.432 us |   448.78 us |  2.93 |    0.08 |              15.9678 |           0.0005 |     824 B |
|         Parallel_Dict_TryGet |   363.83 us |   5.747 us |  10.214 us |   360.22 us |  2.40 |    0.10 |              15.9526 |           0.0005 |     825 B |
|        Parallel_CDict_TryGet |   354.97 us |   3.629 us |   3.395 us |   353.85 us |  2.32 |    0.04 |              15.9507 |           0.0005 |     824 B |


// * Legends *
  Mean                 : Arithmetic mean of all measurements
  Error                : Half of 99.9% confidence interval
  StdDev               : Standard deviation of all measurements
  Median               : Value separating the higher half of all measurements (50th percentile)
  Ratio                : Mean of the ratio distribution ([Current]/[Baseline])
  RatioSD              : Standard deviation of the ratio distribution ([Current]/[Baseline])
  Completed Work Items : The number of work items that have been processed in ThreadPool (per single operation)
  Lock Contentions     : The number of times there was contention upon trying to take a Monitor's lock (per single operation)
  Allocated            : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 us                 : 1 Microsecond (0.000001 sec)
```