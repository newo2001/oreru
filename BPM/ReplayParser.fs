namespace BPM

open System
open System.IO
open BPM.Replay
open FParsec
open SharpCompress.Compressors.LZMA

module ReplayParser =
    type UserState = unit
    type Parser<'t> = Parser<'t, UserState>

    type PartialMetadata = {
        gameMode: GameMode
        gameVersion: int
        mapHash: string
        playerName: string
        replayHash: string
    }
    
    let extendedAscii = Text.Encoding.GetEncoding(28591)
    
    let pbyte: Parser<byte> =
        fun stream ->
            let c = stream.Read()
            if c <> EOS then Reply(byte <| c)
            else Reply(Error, messageError "Expected Any Char")
    
    let pbytes n: Parser<byte array> =
        parray n pbyte

    let pshort: Parser<int16> =
        pbytes 2
        |>> (fun x -> BitConverter.ToInt16(x, 0))

    let pint: Parser<int> =
        pbytes 4
        |>> (fun x -> BitConverter.ToInt32(x, 0))

    let plong: Parser<int64> =
        pbytes 8
        |>> (fun x -> BitConverter.ToInt64(x, 0))

    let pbool = pbyte >>= fun bool ->
        match bool with
        | 0uy -> preturn false
        | 1uy -> preturn true
        | _ -> fail $"Failed parsing boolean value (0/1) got: {bool}"
    
    let uleb128: Parser<int32> =
        let rec recurse depth: Parser<int32> =
            pbyte >>= fun x ->
                if x &&& 0b10000000uy = 0uy
                then preturn ((int32 (x &&& 0b01111111uy)) <<< (7 * depth))
                else recurse (depth + 1) >>= fun y ->
                    preturn (y ||| ((int32 (x &&& 0b01111111uy)) <<< (7 * depth)))
        recurse 0

    let osuString =
        pbyte >>= fun x ->
            if x = 11uy
            then uleb128 >>= anyString
            else pstring ""

    let parseMetadata: Parser<PartialMetadata> =
        let gameMode = pbyte |>> fun mode ->
            match mode with
            | 0uy -> Standard
            | 1uy -> Taiko
            | 2uy -> Catch
            | 3uy -> Mania
            | _ -> failwith $"Invalid gamemode! got: {mode}"

        let constructMetadata mode version mapHash username replayHash = {
            gameMode = mode
            gameVersion = version
            mapHash = mapHash
            playerName = username
            replayHash = replayHash
        }

        pipe5 gameMode pint osuString osuString osuString constructMetadata
    
    let parseStats: Parser<ReplayStats> =
        let constructStats (n300, n100, n50) (gekis, katsus) misses score (combo, fc) = {
            num300 = n300
            num100 = n100
            num50 = n50
            gekis = gekis
            katsus = katsus
            misses = misses
            score = score
            maxCombo = combo
            fullCombo = fc
        }
        
        pipe5 (tuple3 pshort pshort pshort) (tuple2 pshort pshort) pshort pint (tuple2 pshort pbool) constructStats

    let parseFrames: Parser<Frame list> =
        let pipe = pchar '|'
        let parseFrame: Parser<Frame> =
            tuple4 (pint64 .>> pipe) (pfloat .>> pipe) (pfloat .>> pipe) pint32
            |>> (fun (time, x, y, keys) -> {
                timestamp = time
                cursor = (x, y)
                keys = LanguagePrimitives.EnumOfValue keys
            }) .>> pchar ','
            
        pint >>= pbytes >>= (fun data ->
            let stream = new MemoryStream(data)
            let reader = new BinaryReader(stream)
            let props = reader.ReadBytes 5
            let decompressedSize = reader.ReadInt64()
            let compressedSize = int64 (stream.Length - stream.Position)
            let lzma = new LzmaStream(props, stream, compressedSize, decompressedSize + 1L)
            
            let out = Array.zeroCreate<byte> (int <| decompressedSize);
            lzma.Read(out, 0, int <| decompressedSize) |> ignore
            preturn (Text.Encoding.UTF8.GetString out);
        ) >>= (fun decoded ->
            match run (manyTill parseFrame eof) decoded with
            | Success(frames, _, _) ->
                let sumFrames (time, frames) (frame: Frame) = (time + frame.timestamp, {
                    timestamp = time + frame.timestamp
                    keys = frame.keys
                    cursor = frame.cursor
                }::frames)
                
                frames
                |> List.fold sumFrames (0, [])
                |> snd
                |> List.rev
                |> preturn
            | Failure(err, _, _) -> failwith ("Failed to parse replay frames: " + err) 
        )

    let parseReplay: Parser<Replay> =
        let combineMetadata (meta: PartialMetadata) time mods scoreId = {
            gameMode = meta.gameMode
            gameVersion = meta.gameVersion
            mapHash = meta.mapHash
            playerName = meta.playerName
            replayHash = meta.replayHash
            timestamp = time
            mods = mods
            scoreId = scoreId
        }
        
        let constructReplay (meta, stats) mods time frames scoreId = {
            metadata = combineMetadata meta time mods scoreId
            stats = stats
            frames = frames
        }

        pipe5 (tuple2 parseMetadata parseStats) ((pint |>> LanguagePrimitives.EnumOfValue) .>> osuString) plong parseFrames (preturn (int64 0)) constructReplay

    let ParseReplay file: Replay =
        match runParserOnFile parseReplay () file extendedAscii with
        | Success(replay, _, _) -> replay
        | Failure(err, _, _) -> raise (Exception(err))