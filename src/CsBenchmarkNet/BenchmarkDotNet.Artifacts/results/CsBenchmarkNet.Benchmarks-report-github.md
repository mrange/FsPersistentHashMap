``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1739 (21H2)
Intel Core i5-3570K CPU 3.40GHz (Ivy Bridge), 1 CPU, 4 logical and 4 physical cores
.NET SDK=6.0.300
  [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
  DefaultJob : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT


```
|                    Method |     Mean |    Error |   StdDev | Ratio | Allocated |
|-------------------------- |---------:|---------:|---------:|------:|----------:|
|          DictionaryLookup | 23.23 μs | 0.050 μs | 0.047 μs |  1.00 |         - |
| ImmutableDictionaryLookup | 76.35 μs | 0.190 μs | 0.169 μs |  3.29 |         - |
|       CsPersistentHashMap | 35.09 μs | 0.048 μs | 0.045 μs |  1.51 |         - |
|       FsPersistentHashMap | 38.31 μs | 0.069 μs | 0.054 μs |  1.65 |         - |
