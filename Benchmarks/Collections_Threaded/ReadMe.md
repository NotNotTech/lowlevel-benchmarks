



# Findings

- ConcurrentDictionary seems to have reasonable Parallel Read performance, but GC pressure is worying.
- `Interlocked.Add` is expensive if run on each element.  This is unfortunately required by default `Parallel.For` / `.ForEach` workflows.
   - Using Msft Parallel Helpers  ForRange helpers is not good either, because it emits a lot of Objects.   
- My custom ForRange implementation is much better due to no need for `Interlocked.Add` every element.  (See `_SumInterlocked` runs below)
  - further custom `ForRangeAsync` reduces object allocations to minimum.



# Output



```bash

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1237 (20H2/October2020Update)
AMD Ryzen 7 3700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.100-rc.1.21458.32
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT


|                                                  Method |        Mean |      Error |       StdDev |      Median | Ratio | RatioSD | Completed Work Items | Lock Contentions |    Gen 0 |    Gen 1 |   Gen 2 |   Allocated |
|-------------------------------------------------------- |------------:|-----------:|-------------:|------------:|------:|--------:|---------------------:|-----------------:|---------:|---------:|--------:|------------:|
|                                    Sequential_Dict_Read |   171.30 us |   0.807 us |     0.674 us |   171.14 us |  1.30 |    0.01 |                    - |                - |        - |        - |       - |           - |
|                                     Sequential_Dict_Add |   332.29 us |   4.627 us |     4.101 us |   331.45 us |  2.52 |    0.04 |                    - |                - |        - |        - |       - |           - |
|                               Sequential_Dict_AddRemove |   338.60 us |   2.349 us |     1.962 us |   338.52 us |  2.56 |    0.03 |                    - |                - |        - |        - |       - |           - |
|                                   Sequential_CDict_Read |   172.88 us |   1.163 us |     0.908 us |   173.06 us |  1.31 |    0.01 |                    - |                - |        - |        - |       - |           - |
|                                    Sequential_CDict_Add | 3,900.88 us |  76.391 us |   111.972 us | 3,913.58 us | 29.47 |    1.08 |                    - |                - | 476.5625 | 242.1875 | 70.3125 | 3,631,247 B |
|                              Sequential_CDict_AddRemove |   810.56 us |  14.044 us |    19.687 us |   803.69 us |  6.25 |    0.15 |                    - |                - | 142.5781 |        - |       - | 1,200,000 B |
|                                      Parallel_Dict_Read |   203.64 us |   3.971 us |     5.695 us |   201.19 us |  1.54 |    0.06 |              15.7869 |                - |   0.4883 |        - |       - |     4,674 B |
|                                     Parallel_CDict_Read |   205.46 us |   4.031 us |     4.642 us |   203.57 us |  1.57 |    0.03 |              15.8159 |                - |   0.4883 |        - |       - |     4,678 B |
|                                       Parallel_Dict_Add | 2,522.20 us |  75.064 us |   221.328 us | 2,517.57 us | 16.66 |    0.93 |             120.0547 |         129.0156 |   1.9531 |        - |       - |    24,243 B |
|                                      Parallel_CDict_Add | 7,220.78 us | 407.572 us | 1,188.907 us | 7,114.94 us | 48.60 |    7.65 |             101.5859 |         129.1406 | 515.6250 | 250.0000 | 70.3125 | 3,661,086 B |
|                                 Parallel_Dict_AddRemove | 2,697.53 us | 101.595 us |   299.555 us | 2,742.63 us | 17.17 |    0.61 |             128.6953 |         153.3242 |   1.9531 |        - |       - |    25,672 B |
|                                Parallel_CDict_AddRemove | 5,147.32 us | 289.550 us |   853.745 us | 5,470.61 us | 27.19 |    5.89 |             120.1328 |         119.1641 | 152.3438 |  11.7188 |       - | 1,224,516 B |
|                                    Sequential_List_Read |    40.82 us |   0.645 us |     0.572 us |    40.70 us |  0.31 |    0.01 |                    - |                - |        - |        - |       - |           - |
|                                     Sequential_List_Add |   187.32 us |   1.721 us |     1.610 us |   186.80 us |  1.42 |    0.02 |                    - |                - |        - |        - |       - |           - |
|                               Sequential_List_AddRemove |   300.85 us |   4.244 us |     3.970 us |   301.25 us |  2.29 |    0.03 |                    - |                - |        - |        - |       - |           - |
|                                      Parallel_List_Read |   148.73 us |   1.305 us |     1.090 us |   148.66 us |  1.13 |    0.01 |              15.0076 |                - |   0.4883 |        - |       - |     4,565 B |
|                                       Parallel_List_Add |   550.65 us |   7.348 us |     6.874 us |   551.85 us |  4.19 |    0.06 |              14.9795 |                - |        - |        - |       - |     4,553 B |
|                                 Parallel_List_AddRemove |   781.53 us |   5.001 us |     4.176 us |   781.56 us |  5.92 |    0.06 |              15.1924 |                - |        - |        - |       - |     4,586 B |
|                          Parallel_NoWork_InterlockedAdd |   131.51 us |   1.912 us |     1.788 us |   132.16 us |  1.00 |    0.00 |              13.0854 |           0.0002 |   0.4883 |        - |       - |     4,283 B |
|                               Parallel_NoWork_AddSerial |    35.30 us |   0.204 us |     0.191 us |    35.25 us |  0.27 |    0.00 |              13.7963 |                - |   0.5493 |        - |       - |     4,568 B |
|                   Parallel_NoWork_AddSerial_ThreadLocal |    36.65 us |   0.308 us |     0.273 us |    36.60 us |  0.28 |    0.00 |              15.6385 |                - |   0.6714 |        - |       - |     5,857 B |
|                            Parallel_List_Read_AsyncTask | 2,390.94 us |  97.198 us |   286.590 us | 2,402.72 us | 19.12 |    0.66 |              14.7031 |          17.3965 |        - |        - |       - |       618 B |
|                       Parallel_List_Read_AsyncValueTask | 1,148.81 us |   8.987 us |     7.505 us | 1,148.35 us |  8.70 |    0.08 |              15.4785 |          20.7480 |        - |        - |       - |       626 B |
|                     Sequential_List_Read_AsyncAwaitTask | 9,850.50 us | 142.482 us |   118.979 us | 9,842.07 us | 74.57 |    1.00 |           10161.2500 |           0.0156 | 203.1250 |        - |       - | 1,760,255 B |
|                       Parallel_List_Read_AsyncAwaitTask | 7,745.24 us | 265.733 us |   783.520 us | 8,189.92 us | 49.66 |    4.34 |           10000.0000 |                - | 218.7500 |  93.7500 |       - | 1,840,336 B |
|                 Parallel_List_Read_AsyncAwait_ValueTask | 8,140.87 us | 161.984 us |   409.353 us | 8,268.94 us | 61.65 |    4.20 |           10000.0000 |                - | 218.7500 |  93.7500 |       - | 1,840,252 B |
|                               Sequential_List_Read_Linq |   199.04 us |   3.489 us |     3.264 us |   198.78 us |  1.51 |    0.02 |                    - |                - |        - |        - |       - |       144 B |
|                                Parallel_List_Read_PLinq |    43.36 us |   0.449 us |     0.420 us |    43.23 us |  0.33 |    0.01 |              15.0000 |           0.0001 |   1.0986 |        - |       - |     9,322 B |
|                                             P2_RangeFor |   138.92 us |   0.563 us |     0.499 us |   138.80 us |  1.05 |    0.01 |              15.6592 |           0.0085 |   0.4883 |        - |       - |     4,851 B |
|                                        P2_RangeFor_Span |   142.48 us |   0.240 us |     0.200 us |   142.52 us |  1.08 |    0.01 |              15.7346 |           0.0059 |   0.4883 |        - |       - |     4,871 B |
|                                    P2_RangeForEachAsync |   146.68 us |   0.233 us |     0.194 us |   146.62 us |  1.11 |    0.01 |              15.6521 |           0.0007 |        - |        - |       - |       856 B |
|                          P2_RangeForEachAsync_ValueTask |   143.01 us |   0.280 us |     0.262 us |   143.06 us |  1.09 |    0.01 |              15.6475 |           0.0015 |        - |        - |       - |       864 B |
|                                     P2_RangeActionBlock |    77.18 us |   0.520 us |     0.461 us |    77.28 us |  0.59 |    0.01 |               2.0000 |           0.0042 |   0.2441 |        - |       - |     2,168 B |
|                                P2_RangeActionBlock_Task |    77.48 us |   1.527 us |     2.090 us |    76.84 us |  0.60 |    0.02 |               2.0000 |           0.0033 |   0.1221 |        - |       - |     2,024 B |
|                                           P2_RangeAsync |   151.27 us |   0.463 us |     0.433 us |   151.40 us |  1.15 |    0.02 |              16.0000 |           0.0022 |   0.2441 |        - |       - |     3,599 B |
|                                P2_RangeFor_SumInterlock |    17.49 us |   0.117 us |     0.104 us |    17.47 us |  0.13 |    0.00 |               9.8177 |           0.0000 |   0.4578 |        - |       - |     3,932 B |
|                       P2_RangeForEachAsync_SumInterlock |    25.88 us |   0.710 us |     2.092 us |    26.06 us |  0.20 |    0.01 |               7.4894 |           0.0009 |   0.0916 |        - |       - |       852 B |
|             P2_RangeForEachAsync_ValueTask_SumInterlock |    26.70 us |   0.520 us |     1.266 us |    26.99 us |  0.20 |    0.01 |               7.8080 |           0.0010 |   0.0916 |        - |       - |       861 B |
|                  P2_RangeForEachAsync_Span_SumInterlock |    25.30 us |   0.497 us |     1.037 us |    25.63 us |  0.19 |    0.01 |               8.5816 |           0.0016 |   0.0916 |        - |       - |       800 B |
|             P2_RangeForEachAsync_Span_TASK_SumInterlock |    26.26 us |   0.507 us |     0.498 us |    26.38 us |  0.20 |    0.00 |               8.1345 |           0.0010 |   0.0916 |        - |       - |       806 B |
|        P2_RangeForEachAsync_Span_ValueTask_SumInterlock |    24.34 us |   0.481 us |     0.903 us |    24.29 us |  0.18 |    0.01 |               8.3467 |           0.0007 |   0.0916 |        - |       - |       820 B |
|           P2_RangeForEachAsync_Span_SumInterlock_Helper |    26.60 us |   0.148 us |     0.132 us |    26.61 us |  0.20 |    0.00 |               8.4135 |           0.0012 |   0.0916 |        - |       - |       810 B |
| P2_RangeForEachAsync_Span_ValueTask_SumInterlock_Helper |    27.63 us |   0.533 us |     0.499 us |    27.78 us |  0.21 |    0.01 |               7.4305 |           0.0007 |   0.0916 |        - |       - |       822 B |
|                        P2_RangeActionBlock_SumInterlock |    55.53 us |   1.030 us |     0.860 us |    55.37 us |  0.42 |    0.01 |               2.0001 |           0.0019 |   0.2441 |        - |       - |     2,168 B |
|           P2_RangeActionBlock_TaskExplicit_SumInterlock |    52.71 us |   0.877 us |     0.733 us |    52.48 us |  0.40 |    0.01 |               2.0000 |           0.0046 |   0.1831 |        - |       - |     2,024 B |
|                              P2_RangeAsync_SumInterlock |    23.04 us |   0.139 us |     0.116 us |    23.10 us |  0.17 |    0.00 |              16.0002 |           0.0001 |   0.3967 |   0.0305 |       - |     3,539 B |
|                         P2_RangeAsync_Task_SumInterlock |    21.41 us |   0.089 us |     0.079 us |    21.42 us |  0.16 |    0.00 |              16.0004 |                - |   0.5188 |        - |       - |     4,409 B |
|                        P2_RangeAsync_Task_SumInterlock2 |    21.36 us |   0.090 us |     0.084 us |    21.38 us |  0.16 |    0.00 |              16.0005 |                - |   0.5188 |        - |       - |     4,408 B |

// * Legends *
  Mean                 : Arithmetic mean of all measurements
  Error                : Half of 99.9% confidence interval
  StdDev               : Standard deviation of all measurements
  Median               : Value separating the higher half of all measurements (50th percentile)
  Ratio                : Mean of the ratio distribution ([Current]/[Baseline])
  RatioSD              : Standard deviation of the ratio distribution ([Current]/[Baseline])
  Completed Work Items : The number of work items that have been processed in ThreadPool (per single operation)
  Lock Contentions     : The number of times there was contention upon trying to take a Monitor's lock (per single operation)
  Gen 0                : GC Generation 0 collects per 1000 operations
  Gen 1                : GC Generation 1 collects per 1000 operations
  Gen 2                : GC Generation 2 collects per 1000 operations
  Allocated            : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 us                 : 1 Microsecond (0.000001 sec)



```