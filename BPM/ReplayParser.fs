namespace BPM

open System
open Replay
open FParsec
open SevenZip
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

    let pbytes n: Parser<byte array> =
        parray n anyChar |>> extendedAscii.GetBytes

    let pbyte: Parser<byte> =
        pbytes 1
        |>> (fun x -> Array.head x |> byte)

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
            })
            

        //let decoder = Compression.LZMA.Decoder()
        pint >>= pbytes >>= (fun data ->
            (**let input = new IO.MemoryStream (data)
            input.Seek(0, IO.SeekOrigin.Begin) |> ignore

            let props = Array.zeroCreate<byte> 5
            input.Read props |> ignore

            let decoder = Compression.LZMA.Decoder();
            decoder.SetDecoderProperties(props);

            let mutable decodedSize = 0
            for i in 0..7 do
                decodedSize <- (decodedSize ||| (input.ReadByte() <<< (i * 8)))

            (**let mutable encodedSize = 0
            for i in 0..7 do
                encodedSize <- (encodedSize ||| (input.ReadByte() <<< (i * 8)))**)
            
            let encodedSize = (int64 data.Length) - input.Position;
            
            //let lzma = new LzmaStream(props, input, encodedSize);
            let output = new IO.MemoryStream();
            //lzma.CopyTo(output);
            decoder.Code(input, output, encodedSize, 200, null)**)

            let decoder = Compression.LZMA.Decoder()
            let inStream = new IO.MemoryStream(data)
            inStream.Seek(0, IO.SeekOrigin.Begin) |> ignore

            let properties = Array.zeroCreate<byte> 5;
            if not (inStream.Read(properties, 0, 5) = 5) then
                failwith "input .lzma is too short" ;

            decoder.SetDecoderProperties(properties);

            let mutable outSize = 0 |> int64
            let mutable i = 0
            while i < 8 do
                let v = int32 (inStream.ReadByte())
                if v < 0 then
                    i <- 8
                else
                    outSize <- outSize ||| ((v |> byte |> int64) <<< (8 * i));
                    i <- i + 1

            let compressedSize = inStream.Length - inStream.Position;

            let outStream = new IO.MemoryStream();
            decoder.Code(inStream, outStream, compressedSize, outSize, null);
            outStream.Flush();
            outStream.Position <- 0

            preturn (outStream.ToArray() |> Text.Encoding.UTF8.GetString)
            //preturn (output.ToArray() |> Text.Encoding.UTF8.GetString)
        ) >>= (fun decoded ->
            match run (sepBy parseFrame (pchar ',')) decoded with
            | Success(frames, _, _) -> preturn frames
            | Failure(_, _, _) -> failwith "Failed to decode LZMA stream"
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