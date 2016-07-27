# Turing Script
A small language for creating Turing machines

This the interpreter of Turing Script.

One can compile it with
`$ fsharpc TuringScript.fsx --out:bin/TuringScript.exe`

One will get a `TuringScript.exe` under relative `bin` directory. Some dependencies are needed:
* FParsec.DLL
* FParsec.XML
* FParsecCS.DLL
* FParsecCS.XML

After it, one can run with 
`$ mono bin/TuringScript.exe example.txt`. 
It will load the content of `exemple.txt` and interpret as source code.

The language accepted for this interpreter is really small. A valid code is, for example:

```
// I'm creating a georgeous Turing machine.
turing machine tm1
initial state is q0
set of final states is {q0, q2}
in q0 reading 0 change to q1 write 1 move to right,
in q1 reading _ change to q2 write _ move to right,
in q2 reading 1 change to q3 write 0 move to right,
in q3 reading _ change to q0 write _ move to right

// Now I'm creating an enigmatic tape.
tape tape1 as 0_1_0_1_0_1_0_1_0_1

// I wonder if tm1 would accept tape1. Let me see.
run tm1 with tape1

// Comments *almost* everywhere:
// Can't be inside a statement.

// Turing would be proud because of this glorious machine.
turing machine foo
initial state is q0
set of final states is {q2}
in q0 reading b change to q1 write _ move to right,
in q1 reading a change to q1 write _ move to right,
in q1 reading r change to q2 write _ move to right

run foo with bar
run foo with baaaar
run foo with baaaaaaaaaaaaaaaar
run foo with baz
```

As one can see, 3 things can be done:
* Create Turing machines
* Create tapes
* Run some Turing machine with some tape

The only statement that produces output is `run`. The output of the later program is:
```
tm1 accepted tape1 after 20 steps... Final tape:
1_0_1_0_1_0_1_0_1_0_

foo accepted annonymous tape after 3 steps... Final tape:
___

foo accepted annonymous tape after 6 steps... Final tape:
______

foo accepted annonymous tape after 18 steps... Final tape:
__________________

foo rejected annonymous tape after 2 steps... Final tape:
__z
```

One can create as many Turing machines and tapes as wanted.

Another example, this time with placeholders:

```
turing machine <machine-name>
initial state is <state>
set of final states is {<state>, <state>, <state>...}
in <state> reading <char> change to <state> write <char> move to [right|left],
in <state> reading <char> change to <state> write <char> move to [right|left],
in <state> reading <char> change to <state> write <char> move to [right|left],
in <state> reading <char> change to <state> write <char> move to [right|left]

tape <tape-name> as <tape-literal>

run <machine-name> with <tape-name or tape-literal>
```

where:

* `<state>` is the name of a state. It can be any sequence of letters and numerals.
* `<char>` is a symbol. It can be a single letter, numeral or underline.
* `<tape-name>` `<machine-name>` is a identifier. It can be any sequence of letter and numerals, starting with a letter.
* `<tape-literal>` is a tape of Turing machine. It is a sequence of symbols described above as `<char>`.

Do not forget the comma (`,`) between transitions.
