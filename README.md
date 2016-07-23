# Turing-Script
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
turing machine machine1
initial state is q0
set of final states is {q2}
in q0 reading 0 change to q0 write n move to right,
in q0 reading 1 change to q1 write n move to right,
in q1 reading 0 change to q1 write y move to right,
in q1 reading _ change to q2 write y move to right

tape t1 as 545454
tape t2 as 001010001
tape t3 as abcbbabc
tape t4 as 010000000000000

run machine1 with t1
run machine1 with t2
run machine1 with t3
run machine1 with t4
```

As one can see, 3 things can be done:
* Create Turing machines
* Create tapes
* Run some Turing machine with some tape

The only statement that produces output is `run`. The output of the later program is:
```
machine1 rejected t1 after 0 steps... Final tape:
545454

machine1 rejected t2 after 4 steps... Final tape:
nnny10001

machine1 rejected t3 after 0 steps... Final tape:
abcbbabc

machine1 accepted t4 after 16 steps... Final tape:
nnyyyyyyyyyyyyyy
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

tape <tape-name> as <tape>

run <machine-name> with <tape-name>
```

where:

* `<state>` is the name of a state. It can be any sequence of letters and numerals.
* `<char>` is a symbol. It can be a single letter, numeral or underline.
* `<tape-name>` `<machine-name>` is a identifier. It can be any sequence of letter and numerals, starting with a letter.
* `<tape>` is a tape of Turing machine. It is a sequence of symbols described above as `<char>`.

Do not forget the comma (`,`) between transitions.

The (**bad** written) context free grammar is as follow:

```
identifier          := ([A-Za-z])([A-Za-z0-9_])* 

symbol              := ([A-Za-z0-9_])+

state               := ([A-Za-z0-9])+

tape-literal        := symbol | symbol, tape-literal

direction           := 'right' | 'left'

states              := state | state, ','

machine-name        := 'turing machine', identifier

initial-state       := 'initial state is', state

final-states        := 'set of final states is {', states, '}'

transition          := 'in', state, 'reading', symbol, 'change to' state, 
'write', symbol, 'move to', direction

transitions         := transition | transition, ',\n'

turing-machine-stmt := machine-name, initial-state, final-states, transitions

tape-stmt           := 'tape', identifier, 'as', tape-literal

run-stmt            := 'run', identifier, 'with', identifier
```
