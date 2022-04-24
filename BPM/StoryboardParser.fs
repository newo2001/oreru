// Based on https://osu.ppy.sh/community/forums/topics/1869?n=1, https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Commands and https://osu.ppy.sh/wiki/en/Storyboard/Scripting
module BPM.StoryboardParser

open System
open BPM
open BPM.Storyboard
open BPM.Common
open FParsec
open FParsec.Pipes

let comma: Parser<char> = pchar ','

let optionallyQuoted terminator: Parser<string> =
    let quote = pchar '"'
    optional quote >>. (manyCharsTillApply anyChar (optional quote .>> terminator)) (fun str _ -> str)

let timestamp: Parser<TimeSpan> =
    pint32 |>> fun ms -> ms |> TimeSpan.FromMilliseconds

let layer: Parser<Layer> =
    optionallyQuoted comma |>> fun layer ->
        match layer with
        | "Background" -> Background
        | "Foreground" -> Foreground
        | "Pass" -> Pass
        | "Fail" -> Fail
        | _ -> failwith $"Invalid layer: {layer}"

let parseOrigin: Parser<Origin> =
    optionallyQuoted comma |>> fun origin ->
        match origin with
        | "TopLeft" -> TopLeft
        | "TopCentre" -> TopCenter
        | "TopRight" -> TopRight
        | "CentreLeft" -> CenterLeft
        | "Centre" -> Center
        | "CentreRight" -> CenterRight
        | "BottomLeft" -> BottomLeft
        | "BottomCentre" -> BottomCenter
        | "BottomRight" -> BottomRight
        | _ -> failwith $"Invalid origin: {origin}"
        
let parseLoopType: Parser<LoopType> =
    pstring "LoopForever" <|> pstring "LoopOnce" |>> fun loop ->
        match loop with
        | "LoopForever" -> Forever
        | "LoopOnce" -> Once
        | _ -> Forever              // Default according to spec

let parseSprite: Parser<Image> =
    %% pstring "Sprite" -- comma -- +.layer -- +.parseOrigin
    -- +.optionallyQuoted comma -- +.pint32 -- comma -- +.pint32
    -- optional newline -|>
    fun layer origin path x y -> Sprite {
        layer = layer; origin = origin
        filePath = path
        x = x; y = y
    }
    
let parseAnimation: Parser<Image> =
    %% pstring "Animation" -- comma -- +.layer -- +. parseOrigin
    -- +. optionallyQuoted comma -- +.pint32 -- comma -- +.pint32 -- comma
    -- +. pint32 -- comma -- +. pint32 -- comma -- +.parseLoopType -- optional newline -|>
    fun layer origin path x y frames delay loopType -> Animation {
        layer = layer; origin = origin
        filePath = path
        x = x; y = y
        frames = frames; frameDelay = delay; loop = loopType
    }
    
let parseEasing: Parser<EasingFunction> =
    pint32 |>> fun n ->
        if n < 0 || n > 34 then
            failwith($"Invalid easing function: {n}")
        [|
            Linear; EaseOut; EaseIn
            QuadraticIn; QuadraticOut; QuadraticInOut
            CubicIn; CubicOut; CubicInOut
            QuarticIn; QuarticOut; QuarticInOut
            QuinticIn; QuinticOut; QuinticInOut
            SineIn; SineOut; SineInOut
            ExponentialIn; ExponentialOut; ExponentialInOut
            Circular; CircularOut; CircularInOut
            ElasticIn; ElasticOut; ElasticHalfOut; ElasticQuarterOut; ElasticInOut
            BackIn; BackOut; BackInOut
            BounceIn; BounceOut; BounceInOut
        |][n]
        
// TODO: support shorthand command format
let parseCommand: Parser<Command> =
    let commandBase =
        %% +.parseEasing -- comma -- +.timestamp
        -- comma -- +.timestamp -- comma -%> auto
        
    let coord = (pint32 .>> comma) .>>. pint32
    let floatPair = (pfloat .>> comma) .>>. pfloat
    let pbyte = pint32 |>> uint8
    let byteTriple = %% +.pbyte -- comma -- +.pbyte -- comma -- +.pbyte -%> auto
    
    let fade: Parser<Command> =
        %% +.commandBase -- +.floatPair -- optional newline -|>
        fun (ease, start, ending) (startOpacity, endOpacity) -> FadeCommand {
            easing = ease
            startTime = start; endTime = ending
            startOpacity = startOpacity; endOpacity = endOpacity
        }
    
    let move: Parser<Command> =
        %% +.commandBase -- +.coord -- comma -- +.coord -- optional newline -|>
        fun (ease, start, ending) (startX, startY) (endX, endY) -> MoveCommand {
            easing = ease
            startTime = start; endTime = ending
            startX = startX; startY = startY
            endX = endX; endY = endY
        }
        
    let moveX: Parser<Command> =
        %% +.commandBase -- +.coord -- optional newline -|>
        fun (ease, start, ending) (startX, endX) -> MoveXCommand {
            easing = ease
            startTime = start; endTime = ending
            startX = startX; endX = endX
        }
        
    let moveY: Parser<Command> =
        %% +.commandBase -- +.coord -- optional newline -|>
        fun (ease, start, ending) (startY, endY) -> MoveYCommand {
            easing = ease
            startTime = start; endTime = ending
            startY = startY; endY = endY
        }
    
    let scale: Parser<Command> =
        %% +.commandBase -- +.floatPair -- optional newline -|>
        fun (ease, start, ending) (startScale, endScale) -> ScaleCommand {
            easing = ease
            startTime = start; endTime = ending
            startScale = startScale; endScale = endScale
        }
    
    let vectorScale: Parser<Command> =
        %% +.commandBase -- +.floatPair -- comma -- +.floatPair -- optional newline -|>
        fun (ease, start, ending) (startScaleX, startScaleY) (endScaleX, endScaleY) -> VectorScaleCommand {
            easing = ease
            startTime = start; endTime = ending
            startScaleX = startScaleX; startScaleY = startScaleY
            endScaleX = endScaleX; endScaleY = endScaleY
        }
    
    let rotate: Parser<Command> =
        %% +.commandBase -- +.floatPair -- optional newline -|>
        fun (ease, start, ending) (startAngle, endAngle) -> RotateCommand {
            easing = ease
            startTime = start; endTime = ending
            startAngle = startAngle; endAngle = endAngle
        }
    
    // TODO apparently osu supports hex color as well 
    let color: Parser<Command> =
        %% +. commandBase -- +.byteTriple -- comma -- +.byteTriple -|>
        fun (ease, start, ending) fromColor toColor -> ColorCommand {
            easing = ease
            startTime = start; endTime = ending
            startColor = fromColor; endColor = toColor
        }
    
    let loop: Parser<Command> =
        %% +.timestamp -- comma -- +.pint32 -|> fun start loopCount -> LoopCommand {
            startTime = start; iterations = loopCount;
        }
    
    let parameter: Parser<Command> =
        commandBase .>>. anyOf "HVA" |>> fun ((ease, start, ending), p) ->
        match p with
        | p when p = 'H' || p = 'V' ->
            FlipCommand {
                easing = ease
                startTime = start; endTime = ending
                axis = if p = 'H' then Horizontal else Vertical
            }
        | 'A' ->
            AdditiveBlendCommand {
                easing = ease
                startTime = start; endTime = ending
            }
        | _ -> failwith $"Invalid parameter command: {p}"
        
    anyOf "_ " >>. manyCharsTillApply anyChar comma (fun s _ -> s)
        >>= fun command ->
            match command with
            | "F" -> fade
            | "M" -> move
            | "MX" -> moveX
            | "MY" -> moveY
            | "S" -> scale
            | "V" -> vectorScale
            | "R" -> rotate
            | "C" -> color
            | "P" -> parameter
            | "L" -> loop
            | _ -> failwith $"Invalid command type: {command}"
            
let parseEffect: Parser<Effect> =
    pipe2 (parseSprite <|> parseAnimation) (many parseCommand) (fun image commands -> {
        image = image
        commands = commands
    })
        
let parseStoryboard: Parser<Storyboard.Storyboard> =
    manyTill parseEffect eof |>> fun effects -> {
        effects = effects
    }

let ParseStoryboard content: Storyboard.Storyboard =
    match run parseStoryboard content with
    | Success(storyboard, _, _) -> storyboard
    | Failure(err, _, _) -> raise (Exception(err))