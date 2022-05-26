module PerformanceTests =
  open System
  open System.Diagnostics

  open Common

  type Checker () =
    [<Conditional ("DEBUG")>]
    static member inline check ([<InlineIfLambda>] fb) str =
      if not (fb ()) then
        printfn "Check failed: %s" str
        failwith str

  // now () returns current time in milliseconds since start
  let now : unit -> int64 =
    let sw = System.Diagnostics.Stopwatch ()
    sw.Start ()
    fun () -> sw.ElapsedMilliseconds

  // time estimates the time 'action' repeated a number of times
  let time repeat action =
    let inline cc i       = System.GC.CollectionCount i

    let v                 = action ()

    let warmup = repeat / 10

    for i in 1..warmup do
      action () |> ignore

    System.GC.Collect (2, System.GCCollectionMode.Forced, true)

    let bcc0, bcc1, bcc2  = cc 0, cc 1, cc 2
    let b                 = now ()

    for i in 1..repeat do
      action () |> ignore

    let e = now ()
    let ecc0, ecc1, ecc2  = cc 0, cc 1, cc 2

    v, (e - b), ecc0 - bcc0, ecc1 - bcc1, ecc2 - bcc2

// Key is reference type in order to not kill performance in collections that always boxes
//  the key/value
  type Key(v : int) =
    member x.Value = v

    interface IComparable with
      member x.CompareTo(o : obj) =
        match o with
        | :? Key as k -> v.CompareTo (k.Value)
        | _           -> -1

    interface IComparable<Key> with
      member x.CompareTo(o : Key) = v.CompareTo (o.Value)

    interface IEquatable<Key> with
      member x.Equals(o : Key)  = v = o.Value

    override x.Equals(o : obj)  =
      match o with
      | :? Key as k -> v = k.Value
      | _           -> false
    override x.GetHashCode()    = v.GetHashCode ()
    override x.ToString()       = sprintf "%d" v
  let makeKey i = Key i

//  type Key = int
//  let makeKey i : int = i

  let zoom_in = false

  let random      = makeRandom 19740531
#if DEBUG
  let total, inner=
      40000     , 100
#else
  let total, inner=
    if zoom_in then
      100000000 , 1000
    else
      4000000   , 100
#endif
  let outer       = total / inner
  let multiplier  = 4
  let inserts     =
    [|
      for i in 0..(inner - 1) -> random 0 (inner*multiplier) |> makeKey, string i
    |]
  let removals    = shuffle random inserts
  let lookups     = shuffle random inserts

  module TestDictionary =
    open System.Collections.Generic

    let dictAdd k v (d : Dictionary<_, _>) =
      let copy = Dictionary<_, _> d // Need to copy dictionary to preserve immutability
      copy.[k] <- v
      copy

    let dictRemove k (d : Dictionary<_, _>) =
      let copy = Dictionary<_, _> d // Need to copy dictionary to preserve immutability
      copy.Remove k |> ignore
      copy

    let inline dictContainsKey k (d : Dictionary<_, _>) =
      d.ContainsKey k

    let inline doInsert hm =
      inserts
      |> Array.fold (fun s (k, v) -> s |> dictAdd k v) hm

    let inline doRemove hm =
      removals
      |> Array.fold (fun s (k, _) -> s |> dictRemove k) hm

    let inline doLookup fa hm =
      fa
      |> Array.forall (fun (k, _) -> hm |> dictContainsKey k)

    let empty     = Dictionary<_, _> ()
    let inserted  =
      let dict = Dictionary<_, _> ()
      for k, v in inserts do
        dict.[k] <- v
      dict

    let insert () =
      let result    = doInsert empty
      Checker.check (fun () -> result.Count = inserted.Count) "Expected to be same length as testSet"

    let remove () =
      let result    = doRemove inserted
      Checker.check (fun () -> result.Count = 0) "Expected to be empty"

    let insertAndRemove () =
      let inserted  = doInsert empty
      let result    = doRemove inserted
      Checker.check (fun () -> result.Count = 0) "Expected to be empty"

    let insertAndLookup () =
      let inserted  = doInsert empty
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

    let lookupInserted () =
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

  module TestHashtable =
    open System.Collections

    let htAdd k v (ht : Hashtable) =
      let copy = Hashtable ht // Need to copy dictionary to preserve immutability
      copy.[k] <- v
      copy

    let htRemove k (ht : Hashtable) =
      let copy = Hashtable ht // Need to copy dictionary to preserve immutability
      copy.Remove k |> ignore
      copy

    let inline htContainsKey k (d : Hashtable) =
      d.ContainsKey k

    let inline doInsert hm =
      inserts
      |> Array.fold (fun s (k, v) -> s |> htAdd k v) hm

    let inline doRemove hm =
      removals
      |> Array.fold (fun s (k, _) -> s |> htRemove k) hm

    let inline doLookup fa hm =
      fa
      |> Array.forall (fun (k, _) -> hm |> htContainsKey k)

    let empty     = Hashtable ()
    let inserted  =
      let dict = Hashtable ()
      for k, v in inserts do
        dict.[k] <- v
      dict

    let insert () =
      let result    = doInsert empty
      Checker.check (fun () -> result.Count = inserted.Count) "Expected to be same length as testSet"

    let remove () =
      let result    = doRemove inserted
      Checker.check (fun () -> result.Count = 0) "Expected to be empty"

    let insertAndRemove () =
      let inserted  = doInsert empty
      let result    = doRemove inserted
      Checker.check (fun () -> result.Count = 0) "Expected to be empty"

    let insertAndLookup () =
      let inserted  = doInsert empty
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

    let lookupInserted () =
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

  module TestCsPersistentHashMap =
    open CsPersistentHashMap

    let length (phm : PersistentHashMap<_, _>) =
      let mutable l = 0
      let visitor _ _ = l <- l + 1; true
      phm.Visit (Func<_, _, _> visitor) |> ignore
      l

    let inline doInsert phm =
      inserts
      |> Array.fold (fun (s : PersistentHashMap<_, _>) (k, v) -> s.Set (k, v)) phm

    let inline doRemove phm =
      removals
      |> Array.fold (fun (s : PersistentHashMap<_, _>) (k, _) -> s.Unset k) phm

    let inline doLookup fa (phm : PersistentHashMap<_, _>) =
      fa
      |> Array.forall (fun (k, _) -> let r, _ = phm.TryFind k in r)

    let empty     = PersistentHashMap.Empty<Key, string> ()
    let inserted  = doInsert empty

    let insert () =
      let result    = doInsert empty
      Checker.check (fun () -> length result = length inserted) "Expected to be same length as testSet"

    let remove () =
      let result    = doRemove inserted
      Checker.check (fun () -> result.IsEmpty) "Expected to be empty"

    let insertAndRemove () =
      let inserted  = doInsert empty
      let result    = doRemove inserted
      Checker.check (fun () -> result.IsEmpty) "Expected to be empty"

    let insertAndLookup () =
      let inserted  = doInsert empty
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

    let lookupInserted () =
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

  module TestFsPersistentHashMap =
    open FsPersistentHashMap

    let inline doInsert phm =
      inserts
      |> Array.fold (fun s (k, v) -> PersistentHashMap.set k v s) phm

    let doRemove phm =
      removals
      |> Array.fold (fun s (k, v) -> PersistentHashMap.unset k s) phm

    let inline doLookup fa phm =
      fa
      |> Array.forall (fun (k, _) -> PersistentHashMap.containsKey k phm)

    let inserted  = doInsert PersistentHashMap.empty

    let insert () =
      let result    = doInsert PersistentHashMap.empty
      Checker.check (fun () -> PersistentHashMap.length result = PersistentHashMap.length inserted) "Expected to be same length as testSet"

    let remove () =
      let result    = doRemove inserted
      Checker.check (fun () -> PersistentHashMap.isEmpty result) "Expected to be empty"

    let insertAndRemove () =
      let inserted  = doInsert PersistentHashMap.empty
      let result    = doRemove inserted
      Checker.check (fun () -> PersistentHashMap.isEmpty result) "Expected to be empty"

    let insertAndLookup () =
      let inserted  = doInsert PersistentHashMap.empty
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

    let lookupInserted () =
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

  module TestSCI =
    open System.Collections.Immutable

    let inline doInsert hm =
      inserts
      |> Array.fold (fun (s : ImmutableDictionary<_, _>) (k, v) -> (s.Remove k).Add (k, v)) hm

    let inline doRemove hm =
      removals
      |> Array.fold (fun (s : ImmutableDictionary<_, _>) (k, _) -> s.Remove k) hm

    let inline doLookup fa (hm : ImmutableDictionary<_, _>) =
      fa
      |> Array.forall (fun (k, _) -> hm.ContainsKey k)

    let empty     = ImmutableDictionary<Key, String>.Empty;
    let inserted  = doInsert empty

    let insert () =
      let result    = doInsert empty
      Checker.check (fun () -> result.Count = inserted.Count) "Expected to be same length as testSet"

    let remove () =
      let result    = doRemove inserted
      Checker.check (fun () -> result.Count = 0) "Expected to be empty"

    let insertAndRemove () =
      let inserted  = doInsert empty
      let result    = doRemove inserted
      Checker.check (fun () -> result.Count = 0) "Expected to be empty"

    let insertAndLookup () =
      let inserted  = doInsert empty
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

    let lookupInserted () =
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

  module TestMap =
    open System.Collections.Generic

    let inline doInsert hm =
      inserts
      |> Array.fold (fun s (k, v) -> s |> Map.add k v) hm

    let inline doRemove hm =
      removals
      |> Array.fold (fun s (k, _) -> s |> Map.remove k) hm

    let inline doLookup fa hm =
      fa
      |> Array.forall (fun (k, _) -> hm |> Map.containsKey k)

    let empty     = Map.empty

    let inserted  = doInsert empty

    let insert () =
      let result    = doInsert empty
      Checker.check (fun () -> result.Count = inserted.Count) "Expected to be same length as testSet"

    let remove () =
      let result    = doRemove inserted
      Checker.check (fun () -> result.Count = 0) "Expected to be empty"

    let insertAndRemove () =
      let inserted  = doInsert empty
      let result    = doRemove inserted
      Checker.check (fun () -> result.Count = 0) "Expected to be empty"

    let insertAndLookup () =
      let inserted  = doInsert empty
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

    let lookupInserted () =
      let result    = doLookup lookups inserted
      Checker.check (fun () -> result) "Expected true for all"

  let testCases =
    if zoom_in then
      [|
        "Lookup"  , "Dictionary (Copy on Write)"    , TestDictionary.lookupInserted
        "Lookup"  , "Hashtable (Copy on Write)"     , TestHashtable.lookupInserted
        "Lookup"  , "Persistent Hash Map (C#)"      , TestCsPersistentHashMap.lookupInserted
        "Lookup"  , "Persistent Hash Map (F#)"      , TestFsPersistentHashMap.lookupInserted
        "Lookup"  , "System.Collections.Immutable"  , TestSCI.lookupInserted
      |]
    else
      [|
        "Lookup"  , "Dictionary (Copy on Write)"    , TestDictionary.lookupInserted
        "Insert"  , "Dictionary (Copy on Write)"    , TestDictionary.insert
        "Remove"  , "Dictionary (Copy on Write)"    , TestDictionary.remove
        "Lookup"  , "Hashtable (Copy on Write)"     , TestHashtable.lookupInserted
        "Insert"  , "Hashtable (Copy on Write)"     , TestHashtable.insert
        "Remove"  , "Hashtable (Copy on Write)"     , TestHashtable.remove
        "Lookup"  , "Persistent Hash Map (C#)"      , TestCsPersistentHashMap.lookupInserted
        "Insert"  , "Persistent Hash Map (C#)"      , TestCsPersistentHashMap.insert
        "Remove"  , "Persistent Hash Map (C#)"      , TestCsPersistentHashMap.remove
        "Lookup"  , "Persistent Hash Map (F#)"      , TestFsPersistentHashMap.lookupInserted
        "Insert"  , "Persistent Hash Map (F#)"      , TestFsPersistentHashMap.insert
        "Remove"  , "Persistent Hash Map (F#)"      , TestFsPersistentHashMap.remove
        "Lookup"  , "FSharp.Collections.Map"        , TestMap.lookupInserted
        "Insert"  , "FSharp.Collections.Map"        , TestMap.insert
        "Remove"  , "FSharp.Collections.Map"        , TestMap.remove
        "Lookup"  , "System.Collections.Immutable"  , TestSCI.lookupInserted
        "Insert"  , "System.Collections.Immutable"  , TestSCI.insert
        "Remove"  , "System.Collections.Immutable"  , TestSCI.remove
      |]

  let run () =
    use tw = new System.IO.StreamWriter "performance_results.csv"
    let line  l = tw.WriteLine (l : string)
    let linef f = FSharp.Core.Printf.kprintf line f
    line "Type,Name,TimeInMs,CC0,CC1,CC2"
    printfn "Total: %d, outer: %d, inner: %d" total outer inner
    for tp, nm, a in testCases do
      printfn "Running test case: %s - %s..." tp nm
      let _, tm, cc0, cc1, cc2 = time outer a
      printfn "...It took %d ms, CC: (%d, %d, %d)" tm cc0 cc1 cc2
      linef "%s,%s,%d,%d,%d,%d" tp nm tm cc0 cc1 cc2

[<EntryPoint>]
let main argv =
  PerformanceTests.run ()
  0