``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1739 (21H2)
Intel Core i5-3570K CPU 3.40GHz (Ivy Bridge), 1 CPU, 4 logical and 4 physical cores
.NET SDK=6.0.300
  [Host] : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
  0_STD  : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
  1_PGO  : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
  2_NOT  : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT


```
|                    Method |   Job |                                                EnvironmentVariables |     Mean |    Error |   StdDev | Ratio | RatioSD | Allocated |
|-------------------------- |------ |-------------------------------------------------------------------- |---------:|---------:|---------:|------:|--------:|----------:|
|          DictionaryLookup | 0_STD |                                                               Empty | 22.74 μs | 0.066 μs | 0.055 μs |  1.00 |    0.00 |         - |
| ImmutableDictionaryLookup | 0_STD |                                                               Empty | 78.82 μs | 0.212 μs | 0.198 μs |  3.47 |    0.01 |         - |
|       CsPersistentHashMap | 0_STD |                                                               Empty | 37.13 μs | 0.096 μs | 0.090 μs |  1.63 |    0.01 |         - |
|       FsPersistentHashMap | 0_STD |                                                               Empty | 38.88 μs | 0.135 μs | 0.113 μs |  1.71 |    0.01 |         - |
|                           |       |                                                                     |          |          |          |       |         |           |
|          DictionaryLookup | 1_PGO | DOTNET_TieredPGO=1,DOTNET_TC_QuickJitForLoops=1,DOTNET_ReadyToRun=0 | 17.75 μs | 0.085 μs | 0.079 μs |  1.00 |    0.00 |         - |
| ImmutableDictionaryLookup | 1_PGO | DOTNET_TieredPGO=1,DOTNET_TC_QuickJitForLoops=1,DOTNET_ReadyToRun=0 | 71.80 μs | 0.188 μs | 0.176 μs |  4.05 |    0.02 |         - |
|       CsPersistentHashMap | 1_PGO | DOTNET_TieredPGO=1,DOTNET_TC_QuickJitForLoops=1,DOTNET_ReadyToRun=0 | 30.10 μs | 0.067 μs | 0.059 μs |  1.70 |    0.01 |         - |
|       FsPersistentHashMap | 1_PGO | DOTNET_TieredPGO=1,DOTNET_TC_QuickJitForLoops=1,DOTNET_ReadyToRun=0 | 29.02 μs | 0.078 μs | 0.073 μs |  1.64 |    0.01 |         - |
|                           |       |                                                                     |          |          |          |       |         |           |
|          DictionaryLookup | 2_NOT |                                          DOTNET_TieredCompilation=0 | 25.09 μs | 0.089 μs | 0.083 μs |  1.00 |    0.00 |         - |
| ImmutableDictionaryLookup | 2_NOT |                                          DOTNET_TieredCompilation=0 | 94.15 μs | 0.298 μs | 0.279 μs |  3.75 |    0.01 |         - |
|       CsPersistentHashMap | 2_NOT |                                          DOTNET_TieredCompilation=0 | 40.43 μs | 0.138 μs | 0.116 μs |  1.61 |    0.00 |         - |
|       FsPersistentHashMap | 2_NOT |                                          DOTNET_TieredCompilation=0 | 37.81 μs | 0.098 μs | 0.091 μs |  1.51 |    0.01 |         - |
