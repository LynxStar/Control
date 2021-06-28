Documentation coming for now check out


https://shiftlang.com

These are the rough milestones atm

* Document the target state for Shift
* Get Shift's compiler functional enough to hit enough of the language spec that something meaningful can be built in it
* Rewrite Control and Shift's compiler in Shift.

----

# Getting Started

Shift is a high level that runs on CLR (.NET Core). Ensure you have .NET Core 5 installed and build the Control.sln file.

Control and Shift will be split into separate projects when I get the mental effort. Both are currently coded in C# 9

## Control
The compiler compiler that allows rapid prototyping of new grammar
* It supports productions (forms), tokens, fragments, and discards.
* It has SDK support for taking a source and a grammar as input and traversing these stages
  * Build ruleset from grammar text   
  * Lexer to tokenize source text + ruleset to a token stream
  * Parser to convert tokenstream + ruleset to an Abstract Syntax Tree
  * Concreter to convert an AST + ruleset + concrete types to a strongly typed Syntax Tree

This functionality is supplied out of the box from Control for the mentioned stages above.

## Shift
The compiler for the Shift programming language.

* /Shift/ShiftGrammar.cs currently contains the working grammar for Shift.
* It is defined in Control's grammar language
* It leverages Control's library functionality to go from source to a strongly typed tree.
* It then uses this and compiles the code.

Check out Shift/quickTest.bat for seeing how good or bad master is.

Whenever version 0.1 is out, I'll actually give a shit about making master stable and being a good software engineer.
