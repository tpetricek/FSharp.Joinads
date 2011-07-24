// ----------------------------------------------------------------------------
// F# Joinads Samples - Samples using joins (Samples.fs)
// (c) Tomas Petricek, 2011, Available under Apache 2.0 license.
// ----------------------------------------------------------------------------

#if INTERACRIVE
#load "Joins.fs"
#else
module Samples 
#endif

open System
open System.Threading
open FSharp.Joinads.Joins

// ----------------------------------------------------------------------------
// A buffer with two different put channels
// ----------------------------------------------------------------------------

let twoPutBuffer() =
  let putString = Channel<string>("puts")
  let putInt = Channel<int>("puti")
  let get = SyncChannel<string>("get")

  join {
    match! get, putString, putInt with
    | repl, v, ? -> return react { yield repl.Reply("Echo " + v) }
    | repl, ?, v -> return react { yield repl.Reply("Echo " + (string v)) } 
  }


  // Put 5 string messages to the putString channel
  async { for i in 1 .. 5 do 
            do! Async.Sleep(1000) 
            printfn "putting: World (#%d)" i
            putString.Call(sprintf "World (#%d)" i) }
  |> Async.Start

  // Put 5 int messages to the putInt channel
  async { do! Async.Sleep(2000)
          for i in 1 .. 5 do 
            do! Async.Sleep(1000) 
            printfn "putting: %d" (1000 + i)
            putInt.Call(1000 + i) }
  |> Async.Start

  // Repeatedly read messages by calling 'Get'
  async { while true do
            do! Async.Sleep(500)
            printfn "reading..."
            let! repl = get.AsyncCall()
            printfn "got: %s" repl }
  |> Async.Start


// ----------------------------------------------------------------------------
// One place buffer sample
// ----------------------------------------------------------------------------

let onePlaceBuffer() = 
  let put = SyncChannel<string, unit>()
  let get = SyncChannel<string>()
  let empty = Channel<unit>()
  let contains = Channel<string>()
  
  // Initially, the buffer is empty
  empty.Call(())

  join {
    match! put, empty, get, contains with 
    | (s, repl), (), ?, ? -> return react {
      yield contains.Put(s)
      yield repl.Reply() }
    | ?, ?, repl, v -> return react {
      yield repl.Reply(v)
      yield empty.Put(()) } }

  // Repeatedly try to put value into the buffer
  async { do! Async.Sleep(1000)
          for i in 0 .. 10 do
            printfn "putting: %d" i
            do! put.AsyncCall(string i)
            do! Async.Sleep(500) }
  |> Async.Start

  // Repeatedly read values from the buffer and print them
  async { while true do 
            do! Async.Sleep(250)
            let! v = get.AsyncCall()
            printfn "got: %s" v }
  |> Async.Start


// ----------------------------------------------------------------------------
// Main function 
// ----------------------------------------------------------------------------

[<EntryPoint>]
let main (args) =
  twoPutBuffer()
  Console.ReadLine()
  
  onePlaceBuffer()
  Console.ReadLine()
  
  0
