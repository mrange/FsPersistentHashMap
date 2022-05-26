module Common
open System
open FsCheck

type MapAction =
  | Add     of int*string
  | Remove  of int

type ComplexType =
  | IntKey    of  int
  | StringKey of  int
  | TupleKey  of  int*string

let fsCheckConfig =
#if DEBUG
  Config.Default
#else
  { Config.Default with MaxTest = 1000; MaxFail = 1000 }
#endif

let notIdentical<'T when 'T : not struct> (f : 'T) (s : 'T) = obj.ReferenceEquals (f, s) |> not

let notIdenticalArray (f : 'T array) (s : 'T array) = 
  if f.Length = 0 && s.Length = 0 then 
    true
  else
    obj.ReferenceEquals (f, s) |> not

let popCount v =
  let rec loop c v =
    if v <> 0u then
      loop (c + 1) (v &&& (v - 1u))
    else
      c
  loop 0 v

type HalfHash(v : int) =
  member x.Value = v

  interface IComparable<HalfHash> with
    member x.CompareTo(o : HalfHash)  = v.CompareTo o.Value

  interface IEquatable<HalfHash> with
    member x.Equals(o : HalfHash)  = v = o.Value

  override x.Equals(o : obj)  =
    match o with
    | :? HalfHash as k -> v = k.Value
    | _                -> false
  override x.GetHashCode()    = (v.GetHashCode ()) >>> 16 // In order to get a fair bunch of duplicated hashes
  override x.ToString()       = sprintf "%d" v


let uniqueKey vs =
  vs
  |> FsLinq.groupBy fst
  |> FsLinq.map (fun g -> g.Key, (g |> FsLinq.map snd |> FsLinq.last))
  |> FsLinq.sortBy fst
  |> FsLinq.toArray

let makeRandom (seed : int) =
  let mutable state = int64 seed
  let m = 0x7FFFFFFFL // 2^31 - 1
  let d = 1. / float m
  let a = 48271L      // MINSTD
  let c = 0L
  fun (b : int) (e : int) ->
    state <- (a*state + c) % m
    let r = float state * d
    let v = float (e - b)*r + float b |> int
    v

let shuffle random vs =
  let a = Array.copy vs
  for i in 0..(vs.Length - 2) do
    let s =  random i vs.Length
    let t =  a.[s]
    a.[s] <- a.[i]
    a.[i] <- t
  a
