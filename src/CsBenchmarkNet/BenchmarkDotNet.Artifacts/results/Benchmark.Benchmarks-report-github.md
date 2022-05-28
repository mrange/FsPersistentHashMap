``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1739 (21H2)
Intel Core i5-3570K CPU 3.40GHz (Ivy Bridge), 1 CPU, 4 logical and 4 physical cores
.NET SDK=6.0.300
  [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
  DefaultJob : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT


```
|                    Method |     Mean |    Error |   StdDev | Ratio | RatioSD | Allocated |
|-------------------------- |---------:|---------:|---------:|------:|--------:|----------:|
|          DictionaryLookup | 23.83 μs | 0.075 μs | 0.070 μs |  1.00 |    0.00 |         - |
| ImmutableDictionaryLookup | 79.25 μs | 0.455 μs | 0.404 μs |  3.33 |    0.02 |         - |
|       CsPersistentHashMap | 43.96 μs | 0.467 μs | 0.437 μs |  1.84 |    0.02 |         - |
|       FsPersistentHashMap | 44.36 μs | 0.411 μs | 0.365 μs |  1.86 |    0.01 |         - |
