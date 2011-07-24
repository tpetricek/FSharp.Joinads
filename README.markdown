F# Joinad Samples
=================

This repository contains various examples that use the `match!` extension for the F# 
langauge. This is a prototype implementation for research purposes. 

Join calculus
-------------

The `match!` extension can be used to encode computations based on _join calculus_.
A buffer that provides two put channels (for storing strings and integers) and a 
single get channel for retreiving values can be encoded as follows:

    let putString = Channel<string>("puts")
    let putInt = Channel<int>("puti")
    let get = SyncChannel<string>("get")

    join {
      match! get, putString, putInt with
      | repl, v, ? -> return react { yield repl.Reply("Echo " + v) }
      | repl, ?, v -> return react { yield repl.Reply("Echo " + (string v)) } 
    }

The first three lines define channels. The `get` value represents a synchronous
channel (when we call it, we need to wait for a reply). The `putInt` and `putString`
channels represent two methods for adding values to the buffer.

The join program is implemented using the `join` computation builder that defines
primitives for _merging_ channels, _projecting_ values in the channel and for
non-deterministic _choice_ between channels. These primitives return _alias_ channels
that do not actually contain values - they just query the original channels for
a value when a value is requested. 

The two clauses of the `match!` expression represent two _join patterns_. In the first
join pattern, we require a value from the `get` channel and the `putString` channel.
When the values are available, we return a reaction. The reaction is created using
`react` computation builder and it can yield operations to be performed. In this sample,
the only operation is to send a reply to the caller of the `get` channel.

The buffer can be called using F# asynchronous workflows as follows:

    // Put 5 values to 'putString' and 5 values to 'putInt'
    for i in 1 .. 5 do 
      putString.Call("Hello!")
      putInt.Call(i)

    // Repeatedly call 'get' to read the next value. This is a blocking
    // operation, so it should be done from asynchronous workflow to 
    // avoid blocking physical threads.
    async { 
      while true do
        let! repl = get.AsyncCall()
        printfn "got: %s" repl }
    |> Async.Start
