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
- Build storage using a `List`, and use `System.Runtime.Interop.CollectionMarshal.AsSpan()` to do lookups
   - even if the conversion to Span is in a hotpath, it's still faster than List indexers



# Output
```bash
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1237 (20H2/October2020Update)
AMD Ryzen 7 3700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.100-rc.1.21458.32
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT


|                                                Method |        Mean |      Error |     StdDev |      Median | Ratio | RatioSD | Completed Work Items | Lock Contentions | Allocated |
|------------------------------------------------------ |------------:|-----------:|-----------:|------------:|------:|--------:|---------------------:|-----------------:|----------:|
|                                      Sequential_Array |   133.63 us |   2.260 us |   2.003 us |   133.82 us |  1.00 |    0.00 |                    - |                - |         - |
|                                Sequential_Array_Local |   135.73 us |   1.888 us |   2.825 us |   135.09 us |  1.02 |    0.03 |                    - |                - |         - |
|                                       Sequential_Span |   137.21 us |   2.731 us |   4.252 us |   136.68 us |  1.02 |    0.04 |                    - |                - |         - |
|                     Sequential_ArraySpanCastAgressive |   171.38 us |   3.419 us |   6.421 us |   171.24 us |  1.27 |    0.04 |                    - |                - |         - |
|                Sequential_ArrayUnsafeExtensionPointer |   124.58 us |   2.472 us |   4.394 us |   123.47 us |  0.93 |    0.04 |                    - |                - |         - |
|              Sequential_ArrayUnsafeExtensionAgressive |   138.74 us |   2.769 us |   7.719 us |   136.60 us |  1.02 |    0.06 |                    - |                - |         - |
|                        Sequential_Array_UnsafePointer |   126.74 us |   2.430 us |   3.407 us |   125.81 us |  0.96 |    0.03 |                    - |                - |         - |
|                         Sequential_Array_FixedPointer |   127.74 us |   2.554 us |   5.868 us |   125.55 us |  0.98 |    0.04 |                    - |                - |         - |
|                               Sequential_ArraySegment |   684.35 us |  19.403 us |  54.087 us |   670.46 us |  5.10 |    0.32 |                    - |                - |         - |
|                                       Sequential_List |   658.19 us |  19.171 us |  54.384 us |   650.76 us |  4.91 |    0.40 |                    - |                - |         - |
|                                 Sequential_ListUnsafe |   141.37 us |   3.652 us |  10.240 us |   138.07 us |  1.09 |    0.12 |                    - |                - |         - |
|                        Sequential_ListUnsafeAgressive |   232.19 us |   7.082 us |  19.507 us |   227.85 us |  1.77 |    0.14 |                    - |                - |         - |
|              Sequential_ListUnsafeAgressive_Extension |   234.55 us |   6.991 us |  19.834 us |   229.47 us |  1.90 |    0.17 |                    - |                - |         - |
|        Sequential_ListUnsafeAgressive_ExtensionInline |   227.89 us |   4.726 us |  12.938 us |   225.51 us |  1.70 |    0.12 |                    - |                - |         - |
| Sequential_ListUnsafeAgressive_ExtensionInlineOnecall |   218.61 us |   4.198 us |  11.772 us |   214.53 us |  1.70 |    0.06 |                    - |                - |         - |
|                                       Sequential_Dict | 2,613.65 us |  52.073 us | 108.695 us | 2,597.55 us | 19.79 |    0.80 |                    - |                - |       2 B |
|                                      Sequential_CDict | 3,604.48 us | 161.142 us | 470.060 us | 3,428.64 us | 25.29 |    2.74 |                    - |                - |       3 B |
|                                Sequential_Dict_TryGet | 2,110.69 us |  52.477 us | 148.013 us | 2,116.61 us | 16.15 |    0.69 |                    - |                - |       1 B |
|                               Sequential_CDict_TryGet | 2,240.59 us |  51.034 us | 142.262 us | 2,240.78 us | 16.78 |    1.05 |                    - |                - |       2 B |
|                                 Sequential_Array_Task |   140.51 us |   2.417 us |   2.143 us |   141.11 us |  1.05 |    0.02 |                    - |                - |      72 B |
|                          Sequential_ArraySegment_Task |   647.51 us |  12.841 us |  17.142 us |   645.30 us |  4.86 |    0.20 |                    - |                - |      72 B |
|                                  Sequential_List_Task |   628.43 us |  12.505 us |  25.825 us |   626.69 us |  4.71 |    0.19 |                    - |                - |      73 B |
|                                  Sequential_Dict_Task | 2,885.62 us |  68.614 us | 195.760 us | 2,860.88 us | 21.49 |    1.02 |                    - |                - |      74 B |
|                                 Sequential_CDict_Task | 3,332.12 us |  83.432 us | 235.323 us | 3,301.82 us | 27.23 |    2.19 |                    - |                - |      76 B |
|                                        Parallel_Array |    89.98 us |   0.290 us |   0.242 us |    89.96 us |  0.67 |    0.01 |              12.3413 |           0.0010 |     824 B |
|                                  Parallel_Array_Fixed |    81.76 us |   1.595 us |   1.707 us |             |  0.57 |    0.02 |              11.0405 |           0.0010 |     824 B |
|                                         Parallel_Span |    87.02 us |   1.622 us |   1.517 us |    86.46 us |  0.65 |    0.01 |              11.9420 |           0.0015 |     824 B |
|                                 Parallel_ArraySegment |   169.31 us |   1.905 us |   1.782 us |   169.78 us |  1.27 |    0.03 |              15.9355 |           0.0005 |     824 B |
|                                         Parallel_List |   163.34 us |   1.826 us |   1.619 us |   163.21 us |  1.22 |    0.02 |              15.8853 |           0.0007 |     824 B |
|                                  Parallel_List_Unsafe |    87.25 us |   1.669 us |   1.561 us |    86.54 us |  0.65 |    0.02 |              12.2083 |           0.0029 |     823 B |
|                         Parallel_List_UnsafeAgressive |   114.73 us |   2.284 us |   5.294 us |   112.91 us |  0.88 |    0.06 |              13.9624 |           0.0005 |     824 B |
|               Parallel_List_UnsafeAgressive_Extension |   104.31 us |   1.065 us |   0.944 us |   104.06 us |  0.78 |    0.01 |              13.6283 |           0.0037 |     824 B |
|         Parallel_List_UnsafeAgressive_ExtensionInline |   107.59 us |   0.882 us |   0.737 us |   107.69 us |  0.81 |    0.01 |              13.7836 |           0.0015 |     824 B |
|  Parallel_List_UnsafeAgressive_ExtensionInlineOnecall |   103.45 us |   0.832 us |   0.778 us |   103.19 us |  0.77 |    0.01 |              13.7139 |           0.0026 |     824 B |
|                                         Parallel_Dict |   380.13 us |   5.320 us |   4.976 us |   380.87 us |  2.85 |    0.05 |              15.9868 |                - |     824 B |
|                                        Parallel_CDict |   432.46 us |   7.656 us |   6.787 us |   433.99 us |  3.24 |    0.05 |              15.9902 |           0.0010 |     824 B |
|                                  Parallel_Dict_TryGet |   349.16 us |   5.481 us |   5.865 us |   347.92 us |  2.61 |    0.05 |              15.9590 |           0.0005 |     825 B |
|                                 Parallel_CDict_TryGet |   348.83 us |   6.784 us |   6.346 us |   349.77 us |  2.61 |    0.07 |              15.9014 |           0.0005 |     825 B |


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