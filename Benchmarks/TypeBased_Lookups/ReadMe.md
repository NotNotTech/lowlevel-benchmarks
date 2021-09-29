# what

benchmark meant to support dotnet runtime proposal: https://github.com/dotnet/runtime/issues/59718

for optimized instance based type lookups.

# results

```bash

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1237 (20H2/October2020Update)
AMD Ryzen 7 3700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.100-rc.1.21458.32
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT


|             Method |        Mean |     Error |    StdDev |
|------------------- |------------:|----------:|----------:|
|   GlobalLookupTest |    21.06 us |  0.086 us |  0.067 us |
| InstanceLookupTest | 1,333.12 us | 15.480 us | 14.480 us |

```