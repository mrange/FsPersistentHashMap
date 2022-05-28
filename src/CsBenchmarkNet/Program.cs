using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System.Collections.Immutable;
using System.Globalization;

namespace CsBenchmarkNet
{
  public sealed partial class BenchmarkConfig : ManualConfig
  {
    public BenchmarkConfig()
    {
      // Use .NET 6.0 default mode:
      AddJob(Job.Default.WithId("0_STD"));

      // Use Dynamic PGO mode:
      AddJob(Job.Default.WithId("1_PGO")
        .WithEnvironmentVariables(
          new EnvironmentVariable("DOTNET_TieredPGO", "1"),
          new EnvironmentVariable("DOTNET_TC_QuickJitForLoops", "1"),
          new EnvironmentVariable("DOTNET_ReadyToRun", "0")));

      // No Tiered Jitting
      AddJob(Job.Default.WithId("2_NOT")
        .WithEnvironmentVariables(
          new EnvironmentVariable("DOTNET_TieredCompilation", "0")));
    }
  }

  [MemoryDiagnoser]
  //  [HardwareCounters(HardwareCounter.BranchMispredictions,HardwareCounter.BranchInstructions,HardwareCounter.CacheMisses)]
  //[Config(typeof(BenchmarkConfig))]
  public partial class Benchmarks
  {
    const int Size = 1000;

    readonly (string k, int v)[] _kvs;
    readonly string[] _keys;
    readonly long _sum;
    readonly Dictionary<string, int> _dictionary;
    readonly ImmutableDictionary<string, int> _immutableDictionary;
    readonly CsPersistentHashMap.PersistentHashMap<string, int> _csPersistentHashMap;
    readonly FsPersistentHashMap.PersistentHashMap<string, int> _fsPersistentHashMap;

    public Benchmarks()
    {
      var rnd = new Random(19740531);
      _kvs = Enumerable
        .Range(0, int.MaxValue)
        .Select(v =>
          {
            var k = rnd.Next().ToString(CultureInfo.InvariantCulture);
            return (k, v);
          })
        .DistinctBy(kv => kv.k)
        .Take(Size)
        .ToArray()
        ;
      _keys = _kvs.Select(kv => kv.k).ToArray();
      // Shuffle the keys so the querying happens in different than insert
      for (var i = 0; i < _keys.Length; ++i)
      {
        var swap = rnd.Next(i, _keys.Length);
        var tmp = _keys[i];
        _keys[i] = _keys[swap];
        _keys[swap] = tmp;
      }
      _sum = _kvs.Select(kv => (long)kv.v).Sum();

      var kvs = _kvs;

      {
        _dictionary = new(kvs.Length);
        foreach (var kv in kvs)
        {
          _dictionary.Add(kv.k, kv.v);
        }
      }

      {
        var builder = ImmutableDictionary.CreateBuilder<string, int>();
        foreach (var kv in kvs)
        {
          builder.Add(kv.k, kv.v);
        }
        _immutableDictionary = builder.ToImmutable();
      }

      {
        var csPersistentHashMap = global::CsPersistentHashMap.PersistentHashMap.Empty<string, int>();
        foreach (var kv in kvs)
        {
          csPersistentHashMap = csPersistentHashMap.Set(kv.k, kv.v);
        }
        _csPersistentHashMap = csPersistentHashMap;
      }

      {
        var fsPersistentHashMap = global::FsPersistentHashMap.PersistentHashMap.empty<string, int>();
        foreach (var kv in kvs)
        {
          fsPersistentHashMap = fsPersistentHashMap.Set(kv.k, kv.v);
        }
        _fsPersistentHashMap = fsPersistentHashMap;
      }
    }

    [Benchmark(Baseline = true)]
    public void DictionaryLookup()
    {
      var keys = _keys;
      var dictionary = _dictionary;
      var sum = 0L;
      foreach (var k in keys)
      {
        if (dictionary.TryGetValue(k, out var v))
        {
          sum += v;
        }
      }
      if (sum != _sum) throw new Exception("Test case is broken");
    }

    [Benchmark()]
    public void ImmutableDictionaryLookup()
    {
      var keys = _keys;
      var immutableDictionary = _immutableDictionary;
      var sum = 0L;
      foreach (var k in keys)
      {
        if (immutableDictionary.TryGetValue(k, out var v))
        {
          sum += v;
        }
      }
      if (sum != _sum) throw new Exception("Test case is broken");
    }

    [Benchmark()]
    public void CsPersistentHashMap()
    {
      var keys = _keys;
      var csPersistentHashMap = _csPersistentHashMap;
      var sum = 0L;
      foreach (var k in keys)
      {
        if (csPersistentHashMap.TryFind2(k, out var v))
        {
          sum += v;
        }
      }
      if (sum != _sum) throw new Exception("Test case is broken");
    }

    [Benchmark()]
    public void FsPersistentHashMap()
    {
      var keys = _keys;
      var fsPersistentHashMap = _fsPersistentHashMap;
      var sum = 0L;
      foreach (var k in keys)
      {
        if (fsPersistentHashMap.TryFind(k, out var v))
        {
          sum += v;
        }
      }
      if (sum != _sum) throw new Exception("Test case is broken");
    }
  }

  partial class Program
  {
    public static int Main(string[] args)
    {
      BenchmarkRunner.Run<Benchmarks>();
      return 0;
    }
  }
}

