// ----------------------------------------------------------------------------------------------
// Copyright 2016 Mårten Rånge
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------------------

module CsPersistentHashMapProperties
open Common

open CsPersistentHashMap


module PersistentHashMap =
  let inline empty () = PersistentHashMap.Empty<_, _> ()

  let isEmpty     (phm : PersistentHashMap<_, _>) = phm.IsEmpty

  let tryFind k   (phm : PersistentHashMap<_, _>) =
    match phm.TryFind k with
    | true, v -> Some v
    | _   , _ -> None

  let set     k v (phm : PersistentHashMap<_, _>) = phm.Set (k, v)

  let unset   k   (phm : PersistentHashMap<_, _>) = phm.Unset k

  let visit   v   (phm : PersistentHashMap<_, _>) = phm.Visit v

  let toArray     (phm : PersistentHashMap<_, _>) = phm |> FsLinq.toArray

  let toSeq       (phm : PersistentHashMap<_, _>) = phm |> FsLinq.toSeq

  let inline length (m : PersistentHashMap<_, _>) : int =
    let l = ref 0
    let v = System.Func<_, _, _> (fun k v -> incr l; true)
    visit v m |> ignore
    !l

  let inline mapValues (f : 'K -> 'V -> 'U) (m : PersistentHashMap<'K, 'V>) : PersistentHashMap<'K, 'U> =
    m.MapValues f

let fromArray kvs =
  Array.fold
    (fun s (k, v) -> PersistentHashMap.set k v s)
    (PersistentHashMap.empty ())
    kvs

let toArray m phm =
  phm
  |> m
  |> FsLinq.map (fun (KeyValue (k,v)) -> k, v)
  |> FsLinq.toArray

let toSortedKeyArray m phm =
  let vs = phm |> (toArray m)
  vs |> Array.sortInPlaceBy fst
  vs

let inline checkInvariant (phm : PersistentHashMap<_, _>) = 
#if PHM_TEST_BUILD
  phm.CheckInvariant ()
#else
  true
#endif

let testLongInsert () =
#if DEBUG
  let count       = 1000
#else
  let count       = 1000000
#endif
  let multiplier  = 8
  printfn "testLongInsert: count:%d, multiplier:%d" count multiplier
  let random      = makeRandom 19740531
  let inserts     = [| for x in 1..count -> random 0 (count * multiplier) |]
  let lookups     = shuffle random inserts
  let removals    = shuffle random inserts

  let mutable phm = PersistentHashMap.empty ()

  for i in inserts do
    phm <- phm |> PersistentHashMap.set i i
    match phm |> PersistentHashMap.tryFind i with
    | Some v when v = i -> ()
    | _                 -> failwith "testLongInsert/insert/tryFind failed"

type Properties () =
(*
  TODO: Test these properties by exposing these members
  static member ``PopCount returns number of set bits`` (i : uint32) =
    let expected  = popCount i
    let actual    = PersistentHashMap.PopCount i

    expected      = actual

  static member ``CopyArray copies the array`` (vs : int []) =
    let expected  = vs
    let actual    = PersistentHashMap.CopyArray vs

    notIdentical expected actual
    && expected = actual

  static member ``CopyArrayMakeHoleLast copies the array and leaves a hole in last pos`` (vs : Empty []) (hole : Empty)=
    let expected  = Array.append vs [| hole |]
    let actual    = PersistentHashMap.CopyArrayMakeHoleLast (vs, hole)

    notIdenticalArray expected actual
    && expected = actual

  static member ``CopyArrayMakeHole copies the array and leaves a hole at pos`` (at : int) (vs : Empty []) (hole : Empty)=
    let at        = abs at % (vs.Length + 1)
    let expected  = copyArrayMakeHole at vs hole
    let actual    = PersistentHashMap.CopyArrayMakeHole (at, vs, hole)

    notIdenticalArray expected actual
    && expected = actual
*)

  static member ``PHM to* must contain all added values`` (vs : (int*string) []) =
    let expected    = uniqueKey vs
    let phm         = vs |> fromArray
    let actualSeq   = phm |> toSortedKeyArray (PersistentHashMap.toSeq)
    let actualArray = phm |> toSortedKeyArray (PersistentHashMap.toArray)

    notIdenticalArray    expected  actualSeq
    && notIdenticalArray expected  actualArray
    && notIdenticalArray actualSeq actualArray
    && checkInvariant phm
    && expected = actualSeq
    && expected = actualArray

  static member ``PHM TryFind must return all added values`` (vs : (ComplexType*ComplexType) []) =
    let unique    = uniqueKey vs
    let phm       = unique |> fromArray

    let rec loop i =
      if i < unique.Length then
        let k, v = unique.[i]
        match PersistentHashMap.tryFind k phm with
        | Some fv when fv = v -> loop (i + 1)
        | _                   -> false
      else
        true

    checkInvariant phm
    && loop 0

  static member ``PHM Unset on all added values must yield empty map`` (vs : (HalfHash*int) []) =
    let unique    = uniqueKey vs
    let phm       = unique |> fromArray

    let rec loop (phm : PersistentHashMap<_, _>) i =
      if checkInvariant phm |> not then
        None
      elif i < unique.Length then
        if phm |> PersistentHashMap.isEmpty then
          None
        else
          let k, v = unique.[i]
          loop (PersistentHashMap.unset k phm) (i + 1)
      else
        Some phm

    match loop phm 0 with
    | Some phm  -> PersistentHashMap.isEmpty phm
    | None      -> false

  static member ``PHM should behave as Map`` (vs : MapAction []) =
    let compare map (phm : PersistentHashMap<_, _>) =
      let empty =
        match map |> Map.isEmpty, phm |> PersistentHashMap.isEmpty with
        | true  , true
        | false , false -> true
        | _     , _     -> false

      let visitor k v =
        match map |> Map.tryFind k with
        | Some fv -> v = fv
        | _       -> false

      checkInvariant phm
      && (PersistentHashMap.length phm = map.Count)
      && empty
      && PersistentHashMap.visit visitor phm

    let ra = ResizeArray<int> ()

    let rec loop map (phm : PersistentHashMap<_, _>) i =
      if i < vs.Length then
        match vs.[i] with
        | Add (k, v)  ->
          ra.Add k
          let map = map |> Map.add k v
          let phm = PersistentHashMap.set k v phm
          compare map phm && loop map phm (i + 1)
        | Remove r    ->
          if ra.Count > 0 then
            let r   = abs r % ra.Count
            let k   = ra.[r]
            ra.RemoveAt r
            let map = map |> Map.remove k
            let phm = PersistentHashMap.unset k phm
            compare map phm && loop map phm (i + 1)
          else
            loop map phm (i + 1)
      else
        true

    loop Map.empty (PersistentHashMap.empty ()) 0

  static member ``PHM mapValues must contain all added and mapped values`` (vs : (int*int) []) =
    let expected    = uniqueKey vs |> Array.map (fun (k, v) -> k, int64 k + int64 v + 1L)
    let phm         = vs |> fromArray |> PersistentHashMap.mapValues (fun k v -> int64 k + int64 v + 1L)
    let actualArray = phm |> toSortedKeyArray (PersistentHashMap.toArray)

    notIdenticalArray expected  actualArray
    && checkInvariant phm
    && expected = actualArray

open FsCheck

let check () =
  Check.All<Properties> fsCheckConfig
  testLongInsert ()

