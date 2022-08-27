``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19045
Intel Core i5-3570K CPU 3.40GHz (Ivy Bridge), 1 CPU, 4 logical and 4 physical cores
.NET SDK=6.0.400
  [Host] : .NET 6.0.8 (6.0.822.36306), X64 RyuJIT
  0_STD  : .NET 6.0.8 (6.0.822.36306), X64 RyuJIT
  1_PGO  : .NET 6.0.8 (6.0.822.36306), X64 RyuJIT
  2_NOT  : .NET 6.0.8 (6.0.822.36306), X64 RyuJIT


```
|                    Method |   Job |                                                EnvironmentVariables |     Mean |    Error |   StdDev | Ratio | RatioSD | Allocated |
|-------------------------- |------ |-------------------------------------------------------------------- |---------:|---------:|---------:|------:|--------:|----------:|
|          DictionaryLookup | 0_STD |                                                               Empty | 22.87 μs | 0.052 μs | 0.049 μs |  1.00 |    0.00 |         - |
| ImmutableDictionaryLookup | 0_STD |                                                               Empty | 76.50 μs | 0.300 μs | 0.266 μs |  3.34 |    0.01 |         - |
|       CsPersistentHashMap | 0_STD |                                                               Empty | 33.80 μs | 0.144 μs | 0.135 μs |  1.48 |    0.01 |         - |
|       FsPersistentHashMap | 0_STD |                                                               Empty | 40.58 μs | 0.079 μs | 0.070 μs |  1.77 |    0.00 |         - |
|                           |       |                                                                     |          |          |          |       |         |           |
|          DictionaryLookup | 1_PGO | DOTNET_TieredPGO=1,DOTNET_TC_QuickJitForLoops=1,DOTNET_ReadyToRun=0 | 17.67 μs | 0.083 μs | 0.078 μs |  1.00 |    0.00 |         - |
| ImmutableDictionaryLookup | 1_PGO | DOTNET_TieredPGO=1,DOTNET_TC_QuickJitForLoops=1,DOTNET_ReadyToRun=0 | 70.78 μs | 0.176 μs | 0.165 μs |  4.01 |    0.02 |         - |
|       CsPersistentHashMap | 1_PGO | DOTNET_TieredPGO=1,DOTNET_TC_QuickJitForLoops=1,DOTNET_ReadyToRun=0 | 33.13 μs | 0.102 μs | 0.096 μs |  1.88 |    0.01 |         - |
|       FsPersistentHashMap | 1_PGO | DOTNET_TieredPGO=1,DOTNET_TC_QuickJitForLoops=1,DOTNET_ReadyToRun=0 | 29.45 μs | 0.099 μs | 0.088 μs |  1.67 |    0.01 |         - |
|                           |       |                                                                     |          |          |          |       |         |           |
|          DictionaryLookup | 2_NOT |                                          DOTNET_TieredCompilation=0 | 26.01 μs | 0.069 μs | 0.061 μs |  1.00 |    0.00 |         - |
| ImmutableDictionaryLookup | 2_NOT |                                          DOTNET_TieredCompilation=0 | 92.83 μs | 0.240 μs | 0.224 μs |  3.57 |    0.01 |         - |
|       CsPersistentHashMap | 2_NOT |                                          DOTNET_TieredCompilation=0 | 34.42 μs | 0.040 μs | 0.031 μs |  1.32 |    0.00 |         - |
|       FsPersistentHashMap | 2_NOT |                                          DOTNET_TieredCompilation=0 | 38.71 μs | 0.085 μs | 0.080 μs |  1.49 |    0.01 |         - |
