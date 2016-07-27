module TuringScript

#r "lib/FParsec.dll"
#r "lib/FParsecCS.dll"
      
open System

module AST =    
    type direction =
        | Left
        | Right

    type machine = {
        transitions: Map<string * char, string * char * direction>;
        initialState: string;
        finalStates: Set<string>
    }

    type statement =
        | TuringMachine of string * machine
        | Tape of string * string
        | Run of string * string
        | RunLiteralTape of string * string

module TuringExecuter =
    open AST
    
    type private state = string
    
    type private symbol = char
    
    type result = 
    | Accepted 
    | Rejected

    type private context = {
        turingMachine: machine;
        currentState: state;
        cursorPosition: int;
        tape: Map<int, symbol>
    }

    let private nextState curState curSymbol transitions =
        Map.tryFind (curState, curSymbol) transitions

    let private moveCursor position moveDirection =
        match moveDirection with
        | Right -> position + 1
        | Left -> position - 1

    let private updateContext oldCtx newSymbol newState dir =
        {oldCtx with
            currentState = newState
            cursorPosition = moveCursor oldCtx.cursorPosition dir
            tape = Map.add oldCtx.cursorPosition newSymbol oldCtx.tape
        }

    let private getSymbol index (tape: Map<int, symbol>) =
        match Map.tryFind index tape with
        | Some sy -> sy
        | None -> '_'

    let private processContext (ctx: context) =
        let curSymbol = getSymbol ctx.cursorPosition ctx.tape
        let curState = ctx.currentState
        let transitions = ctx.turingMachine.transitions
        
        match nextState curState curSymbol transitions with
        | Some (newState, newSymbol, dir) -> Some (updateContext ctx newSymbol newState dir)
        | None -> None

    let rec private processUntilHalt ctx timesProcessed =
        match processContext ctx with
        | Some newCtx -> processUntilHalt newCtx (timesProcessed + 1)
        | None -> (ctx, timesProcessed)
    
    let private initialContext machine tapeStr =
        let mapper c i = (c, i)
        
        let tape = Seq.mapi mapper tapeStr
        
        {
            turingMachine = machine
            tape = Map tape
            currentState = machine.initialState
            cursorPosition = 0
        }
        
    let run machine tape =
        let initialCtx = initialContext machine tape
        let finalStates = machine.finalStates
        
        let finalCtx, timesProcessed = processUntilHalt initialCtx 0
        
        let res = 
            match finalCtx.currentState with
            | s when Seq.contains s finalStates -> Accepted
            | _ -> Rejected
        
        (res, finalCtx.tape, timesProcessed)

module Parser =
    open FParsec
    open AST

    let private ws = spaces

    let private str s = pstring s

    let private alphanumeric c = isLetter c || isDigit c
    
    let private isSymbol c = alphanumeric c || c = '_'
    
    let private pcomment =
        str "//" >>. skipManySatisfy (fun c -> c <> '\n') >>. newline
        
    let private pendline =
        many1 (pcomment <|> newline) >>. ws
    
    let private pidentifier =
        let first c = isLetter c
        
        many1Satisfy2 first alphanumeric

    let private psymbol =
        choice [
            satisfy alphanumeric
            pchar '_'
        ]

    let private pstr1 =
        (many1Satisfy isSymbol)

    let private plist p sep =
        sepBy1 p (pchar sep .>> ws)

    let private pset p sep =
        between (pchar '{') (pchar '}') (plist p sep)

    let private pdirection =
        choice [
            str "left" |>> fun s -> Left
            str "right" |>> fun s -> Right
        ]

    let private ptape =
        let s1 = str "tape " >>. pidentifier
        let s2 = str " as " >>. pstr1
        
        pipe2 s1 s2 (fun name tape -> Tape (name, tape))

    let private prun =
        let s1 = str "run " >>. pidentifier
        let s2 = str " with " >>. pstr1
        
        pipe2 s1 s2 (fun machine tape -> Run (machine, tape))

    let private pmachinename =
        str "turing machine " >>. pidentifier

    let private pinitialstate =
        str "initial state is " >>. pstr1

    let private pfinalstates =
        str "set of final states is " >>. (pset pstr1 ',')

    let private ptransition =
        let s1 = ws >>. str "in " >>. pstr1
        let s2 = str " reading " >>. psymbol
        let s3 = str " change to " >>. pstr1
        let s4 = str " write " >>. psymbol
        let s5 = str " move to " >>. pdirection
        
        pipe5 s1 s2 s3 s4 s5 (fun curSt curSy nxSt nxSy dir -> 
                                (curSt, curSy), (nxSt, nxSy, dir))

    let private ptransitionset =
        plist (attempt ptransition) ','

    let private pturingmachine =
        let s1 = pmachinename .>> newline
        let s2 = pinitialstate .>> newline
        let s3 = pfinalstates .>> newline
        let s4 = ptransitionset
        
        pipe4 s1 s2 s3 s4 (fun name inSt fnSts ts -> 
            let m = {
                initialState = inSt
                finalStates = Set fnSts
                transitions = Map ts
            }
            
            TuringMachine (name, m)
        )

    let private pstatement =
        choice [
            pturingmachine
            ptape
            prun
        ]
    
    let private pprogram =
        ws >>. pendline >>. sepBy1 (ws >>. pstatement) pendline .>> eof
    
    let parse input =
        match run pprogram input with
        | Success (r, _, _) -> r
        | Failure (msg, _, _) -> failwith msg

module Interpreter =
    open AST
    open TuringExecuter
    
    type private var =
        | TapeVar of string
        | MachineVar of machine
    
    let private executeAndPrint tapeName machineName tape machine =
        let mapToStr (tapeMap: Map<int, char>) =
            let s = Seq.map (fun (i, c) -> c) (Map.toSeq tapeMap)
            String.Concat s

        let toLower s = 
            (sprintf "%A" s).ToString().ToLower()

        let res, finalTape, count = run machine tape
        
        printf "%s %s %s after %d steps... Final tape:\n%s\n\n"
            machineName (toLower res) tapeName count (mapToStr finalTape)
    
    let private interpretRun tapeNameOrLiteral machineName vars =
        let t = Map.tryFind tapeNameOrLiteral vars
        let m = Map.find machineName vars
        
        match t, m with
        | Some (TapeVar tape), MachineVar machine ->
            let tapeName = tapeNameOrLiteral
            executeAndPrint tapeName machineName tape machine
        | _, MachineVar machine ->
            let tapeLiteral = tapeNameOrLiteral
            executeAndPrint "annonymous tape" machineName tapeLiteral machine
        | _ -> failwith "Incorrect variable type"
    
    let private interpretStatement statement vars = 
        match statement with 
        | Tape (name, body) ->
            vars |> Map.add name (TapeVar body)
        | TuringMachine (name, machine) ->
            vars |>  Map.add name (MachineVar machine)
        | Run (machineName, tapeName) ->
            interpretRun tapeName machineName vars
            vars
        
    let rec private interpretHelper statements vars =
        match statements with
        | [] -> ()
        | x::xs ->
            let newVars = interpretStatement x vars
            interpretHelper xs newVars
        
    let interpret statements =
        interpretHelper statements Map.empty

[<EntryPointAttribute>]
let main args =
    printf "\n"
    
    match args with
    | [|path|] when IO.File.Exists path ->
        let inputCode = IO.File.ReadAllText path        
        Interpreter.interpret (Parser.parse inputCode)
    | _ ->
        printf "Not a valid file path"
    
    printf "\n"
    
    0