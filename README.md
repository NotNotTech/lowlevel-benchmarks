# lowlevel-benchmarks
benchmarking ways of doing lowlevel work in dotnet.



# TLDR;
1. **Don't do cheap work in parallel**.  For example, in some of these benchmarks I get a 2x speedup for 16x the cpu cost.
1. For lookups, `Span` is fastest.   Avoid Dictionary/ConcurrentDictionary in hotpaths
1. Avoid add/remove from `ConcurrentDictionary` in hotpaths
1. `Interlocked` is expensive to use in hotpaths.   do per-thread sums or write out to a seperate results span for processing back on the main thread. Using a `ForRange()` parallel work function is best, for example: `/Helpers/Extras/ForRange.cs`
1. Linq and PLinq are not that bad.  Not super great, but not that bad.
1. `MemoryOwner<T>` is your friend.

## how to use
1. open solution in visual studio 2022
2. run solution
3. pick a benchmark
4. wait a long time for benchmarks to run


## structure
- `Program.cs` - entrypoint
- `Benchmarks/*/*.cs` - benchmark tests
- `Helpers` - helpers for the benchmarking, such as:
   - `DumbWork.cs` - helper containing input data and output verification logic
   - `Data.cs` - helper containing structure of test data worked on in benchmarks
   - `zz_Extensions.cs` - extension method for `Span<T>` and `Array` to make parallel easier.


## The Benchmarks

these are the benchmarks, contained in subfolders of `/Benchmarks/`.  Look at each sub folder for a `ReadMe.md` with individual findings:

- `Collections_Threaded` checks speed/correctness of doing collection read/writes from threads
- `Parallel_Work` checks doing work on `Span<T>` from threads
- `Parallel_Lookup` checks a real-world critical path scenario, random access lookup of 100,000 entities.  Benchmark tests using different backing storage collections and Sequential vs Parallel.


