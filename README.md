# parallel-work-gc-benchmarks
benchmarking ways of doing lowlevel work in dotnet.


## how to use
1. open solution in visual studio 2022
2. run solution
3. pick a benchmark
4. wait a long time for benchmarks to run


## structure
- `Program.cs` - entrypoint
- `Benchmarks/*/*.cs` - benchmark tests
- `Helpers`
   - `DumbWork.cs` - helper containing input data and output verification logic
   - `Data.cs` - helper containing structure of test data worked on in benchmarks
   - `zz_Extensions.cs` - extension method for `Span<T>` and `Array` to make parallel easier.


## The Benchmarks

there are two benchmarks.

- `Collections_Threaded` checks speed/correctness of doing collection read/writes from threads
- `Parallel_Work` checks doing work on `Span<T>` from threads



