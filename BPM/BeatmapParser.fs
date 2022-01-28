namespace BPM

open System
open BPM.Map
open FParsec

module Parser =
    type UserState = unit
    type Parser<'t> = Parser<'t, UserState>
    
    let pbool = pchar '0' <|> pchar '1' >>= fun bool ->
            match bool with
            | '0' -> preturn false
            | '1' -> preturn true
            | _ -> fail $"Failed parsing boolean value (0/1) got: {bool}"
            
    let range min max name value =
         if value >= min && value <= max then preturn value
         else fail $"Failed parsing {name}, value out of range ${min}..${max}, got: {value}"
        
    
    // Parse Section Data
    // ##################
    
    let parseSectionHeader sectionName: Parser<_> =
        between (pchar '[') (pchar ']') (pstring sectionName) .>> skipNewline
        
    let endOfSection =
        (many1 skipNewline) |>> ignore <|> eof
        
    let parseKVPair keyParser valueParser: Parser<_> =
        let whitespace = many (pchar ' ' <|> tab)
        let colon = whitespace >>. (pchar ':') .>> whitespace
        (keyParser .>> colon) >>. valueParser .>> skipNewline

    // Parse Timing Points
    // ###################
    
    let parseSampleSet set: Parser<_> =
        match set with
        | 0 -> preturn Default
        | 1 -> preturn Normal
        | 2 -> preturn Soft
        | 3 -> preturn Drum
        | _ -> fail "Invalid SampleSet"
   
    let parseTimingPoint: Parser<_> =
        let number = pint32 .>> (pchar ',')
        let sampleSet = number >>= parseSampleSet
        let timingPointType inherited = pchar (if inherited then '0' else '1') .>> pchar ','
        let effects = pint32 >>= fun effects -> preturn (effects &&& 0b00000001 = 0b00000001)
        let volume = number >>= fun volume ->
            if volume >= 0 && volume <= 100 then preturn volume
            else fail $"Volume out of bounds 0..100, got: {volume}"
            
        let timingPointData inherited = tuple3 (sampleSet .>> number) (volume .>> timingPointType inherited) effects
        
        let parseUnInherited =
            let bpm = pfloat .>> pchar ',' |>> fun beatDuration -> 60.0 * 1000.0 / beatDuration
            
            let timeSignature = number >>= fun beatsPerMeasure ->
                match beatsPerMeasure with
                | 1 -> preturn FullBeat
                | 2 -> preturn HalfBeat
                | 3 -> preturn OneThird
                | 4 -> preturn OneFourth
                | 6 -> preturn OneSixth
                | 8 -> preturn OneEighth
                | 12 -> preturn OneTwelfth
                | 16 -> preturn OneSixteenth
                | _ -> fail $"Invalid Time Signature: 1/{beatsPerMeasure}"
                
            let createTimingPoint timestamp bpm timeSignature (sampleSet, volume, kiai) = UnInheritedTimingPoint {
                timestamp = timestamp
                bpm = bpm
                timeSignature = timeSignature
                sampleSet = sampleSet
                volume = volume
                kiai = kiai
            }

            pipe4 number bpm timeSignature (timingPointData false) createTimingPoint
        
        let parseInherited =
            let sliderVelocity = pfloat .>> pchar ',' >>= fun sv -> preturn (float 1 / (float sv / (float -100)))
            
            let createTimingPoint timestamp sv (sampleSet, volume, kiai) = InheritedTimingPoint {
                timestamp = timestamp
                sliderVelocity = sv
                sampleSet = sampleSet
                volume = volume
                kiai = kiai
            }
            
            pipe3 number (sliderVelocity .>> number) (timingPointData true) createTimingPoint
        
        attempt parseInherited <|> attempt parseUnInherited
        
    let parseTimingPoints =
        parseSectionHeader "TimingPoints" >>. manyTill (parseTimingPoint .>> newline) endOfSection
    
    // Parse HitObjects
    // ################
    
    let parseHitObjectArgs objectType: Parser<_> =
        let number = pint32 .>> pchar ','
        
        let nc = number >>= fun t ->
            let hasNC = (t &&& 0b00000100) <> 0
            if t &&& objectType = objectType
            then preturn hasNC
            else fail ("Wrong HitObjectType! expected: " + (string objectType) + ", got: " + (string t))
        
        tuple5 number number number nc number
    
    let parseCircle: Parser<HitObject> =
        parseHitObjectArgs 0b00000001 .>> skipRestOfLine true
        |>> fun (x, y, time, nc, hitSounds) -> Circle {
            x = x
            y = y
            time = time
            hitSounds = hitSounds
            newCombo = nc
        }
    
    let parseSpinner: Parser<HitObject> =
        let spinnerArgs = pint32 .>> skipRestOfLine true
        
        pipe2 (parseHitObjectArgs 0b00001000) spinnerArgs (fun (_, _, startTime, nc, hitSounds) endTime -> Spinner {
            startTime = startTime
            endTime = endTime
            hitSounds = hitSounds
            newCombo = nc
        })
    
    let parseSlider: Parser<HitObject> =
        let curveType = anyOf "BCLP" .>> pchar '|' |>> fun curveType ->
            match curveType with
            | 'B' -> Bezier
            | 'C' -> Catmull
            | 'L' -> Linear
            | 'P' -> PerfectCircle
            | _ -> failwith $"Invalid Slider Curve Type! got: {curveType}"
        
        let rec mergeRedAnchors (anchors: (int * int) list) =
            let makeAnchor red (x, y) = { x = x; y = y; anchorType = if red then Red else White }
            
            match anchors with
            | a when a.Length < 2 -> anchors |> List.map (makeAnchor false)
            | a when a.Head = a.Tail.Head -> makeAnchor true anchors.Head :: mergeRedAnchors anchors.Tail.Tail
            | _ -> makeAnchor false anchors.Head :: mergeRedAnchors anchors.Tail
                    

        let curvePoint = tuple2 (pint32 .>> pchar ':') pint32
        let curvePoints = sepBy curvePoint (pchar '|') .>> pchar ',' |>> mergeRedAnchors
            
        let repeats = pint32 .>> pchar ','
        let length = pfloat
        let sliderArgs = (tuple4 curveType curvePoints repeats length) .>> skipRestOfLine true
        
        let constructSlider (x, y, time, nc, hitSounds) (curveType, curvePoints, repeats, length) = Slider {
            x = x
            y = y
            time = time
            newCombo = nc
            hitSounds = hitSounds
            curveType = curveType
            curvePoints = curvePoints
            repeats = repeats
            length = length
        }
                
        pipe2 (parseHitObjectArgs 0b00000010) sliderArgs constructSlider
    
    let parseHitObject: Parser<_> =
        [parseCircle; parseSlider; parseSpinner]
        |> List.map attempt
        |> choice
        
    let parseHitObjects: Parser<_> =
        parseSectionHeader "HitObjects" >>. manyTill parseHitObject endOfSection 
    
    // Parse Beatmap
    // #############
        
    let parseBeatmap: Parser<Beatmap> =
        let versionHeader = pstring "osu file format v" >>. pint32 .>> skipNewline .>> optional skipNewline
        
        let parseOptionalString = (followedByNewline >>% option.None) <|> (restOfLine false |>> Some)
        let parseString = parseOptionalString >>= fun string ->
            if string.IsSome then preturn string.Value
            else fail "Expected string, got newline"

        let parseValue key valueParser = parseKVPair (pstring key) valueParser
        
        let settings =
            let header = parseSectionHeader "General"
            let fileName = parseValue "AudioFilename" parseString
                
            let audioLeadIn = (parseValue "AudioLeadIn" pint32) <|> preturn 0
            let previewTime = (parseValue "PreviewTime" pint32 |>> Some) <|> preturn option.None
            
            let countdown = (parseValue "Countdown" pint32 >>= fun countdown ->
                match countdown with
                | 0 -> preturn None
                | 1 -> preturn CountdownType.Normal
                | 2 -> preturn Half
                | 3 -> preturn Double
                | _ -> fail $"Invalid Countdown mode: {countdown}") <|> preturn CountdownType.Normal
            
            let sampleSet = (parseValue "SampleSet" parseString >>= fun set ->
                match set with
                | "Normal" -> preturn Normal
                | "Soft" -> preturn Soft
                | "Drum" -> preturn Drum
                | _ -> fail $"Invalid SampleSet: {set}") <|> preturn Normal
            
            let mode = (parseValue "Mode" pint32 >>= fun mode ->
                match mode with
                | 0 -> preturn Standard
                | 1 -> preturn Taiko
                | 2 -> preturn Catch
                | 3 -> preturn Mania
                | _ -> fail $"Invalid Game mode: {mode}") <|> preturn Standard
                
            let overlayPos = (parseValue "OverlayPosition" parseString >>= fun pos ->
                match pos with
                | "NoChange" -> preturn NoChange
                | "Below" -> preturn Below
                | "Above" -> preturn Above
                | _ -> fail $"Invalid Overlay Position: {pos}") <|> preturn NoChange
            
            let stackLeniency = (parseValue "StackLeniency" pfloat >>= range 0.0 1.0 "Stack Leniency") <|> preturn 0.7
            let letterbox = (parseValue "LetterboxInBreaks" pbool) <|> preturn false
            let skinSprites = (parseValue "UseSkinSprites" pbool) <|> preturn false
            let preferredSkin = (parseValue "SkinPreference" parseString |>> Some) <|> preturn option.None
            let epilepsy = (parseValue "EpilepsyWarning" pbool) <|> preturn false
            let countdownOffset = (parseValue "CountdownOffset" pint32) <|> preturn 0
            let specialStyle = (parseValue "SpecialStyle" pbool) <|> preturn false
            let widescreenSb = (parseValue "WidescreenStoryboard" pbool) <|> preturn false
            let sampleMatchPlaybackRate = (parseValue "SampleMatchPlaybackRate" pbool) <|> preturn false
            
            let createSettings
                (file, audioLI)
                (preview, cd, sample)
                (stack, mode, letter)
                (sprites, overlay, skin)
                (epilepsy, cdOffset, _, wide, pbr) = {
                    audioFileName = file
                    audioLeadIn = audioLI
                    previewPoint = preview
                    countdown = cd
                    gameMode = mode
                    defaultSampleSet = sample
                    stackLeniency = stack
                    letterBoxDuringBreaks = letter
                    useSkinSprites = sprites
                    hitCircleOverlayPosition = overlay
                    preferredSkin = skin
                    showEpilepsyWarning = epilepsy
                    countdownOffset = cdOffset
                    widescreenStoryboard = wide
                    samplesMatchPlaybackRate = pbr
                }
            
            header >>. pipe5 (fileName .>>. audioLeadIn) (tuple3 previewTime countdown sampleSet) (tuple3 stackLeniency mode letterbox) (tuple3 skinSprites overlayPos preferredSkin) (tuple5 epilepsy countdownOffset specialStyle widescreenSb (sampleMatchPlaybackRate .>> endOfSection)) createSettings
        
        let metadata =
            let spaceSeperatedList = sepBy (many1Chars (noneOf " \n")) (pchar ' ')
            let unicodeString (romanised, unicode) = {
                romanised = romanised
                unicode = unicode
            }
            
            let header = parseSectionHeader "Metadata"
            let title = parseValue "Title" parseString .>>. parseValue "TitleUnicode" parseString |>> unicodeString
            let artist = parseValue "Artist" parseString .>>. parseValue "ArtistUnicode" parseString |>> unicodeString
            let creator = parseValue "Creator" parseString
            let diff = parseValue "Version" parseString
            let source = (parseValue "Source" parseOptionalString) <|> preturn option.None
            let tags = parseValue "Tags" spaceSeperatedList
            let mapId = parseValue "BeatmapID" pint32
            let setId = parseValue "BeatmapSetID" pint32 .>> endOfSection
            
            let createMetadata (title, artist) (creator, diff) (source, tags) (mapId, setId) = {
                songTitle = title
                artist = artist
                creator = creator
                difficultyName = diff
                songSource = source
                tags = tags
                beatmapId = mapId
                beatmapSetId = setId
            }
                        
            header >>. pipe4 (tuple2 title artist) (tuple2 creator diff) (tuple2 source tags) (tuple2 mapId setId) createMetadata
        
        let editor =
            let header = parseSectionHeader "Editor"
            let commaSeperatedList = sepBy pint32 (pchar ',')
            let bookmarks = parseValue "Bookmarks" commaSeperatedList
            let spacing = parseValue "DistanceSpacing" pfloat
            let beatDivisor = parseValue "BeatDivisor" pfloat
            let zoom = parseValue "TimelineZoom" pfloat .>> endOfSection
            
            let gridSize = parseValue "GridSize" pint32 >>= fun size ->
                match size with
                | 4 -> preturn Tiny
                | 8 -> preturn Small
                | 16 -> preturn Medium
                | 32 -> preturn Large
                | _ -> fail $"Invalid Grid Size: {size}"
                
            header >>. pipe5 bookmarks spacing beatDivisor gridSize zoom (fun bookmarks spacing beatDivisor gridSize zoom -> {
                bookmarks = bookmarks
                beatSnapDivisor = beatDivisor
                distanceSnap = spacing
                gridSize = gridSize
                timelineZoom = zoom
            })
        
        let difficulty =
            let difficultyRange = pfloat >>= range 0.0 10.0 "Difficulty Setting"
            
            let header = parseSectionHeader "Difficulty"
            let hp = parseValue "HPDrainRate" difficultyRange
            let cs = parseValue "CircleSize" difficultyRange
            let od = parseValue "OverallDifficulty" difficultyRange
            let ar = parseValue "ApproachRate" difficultyRange
            let slider = (parseValue "SliderMultiplier" pfloat) .>>. (parseValue "SliderTickRate" pfloat) .>> endOfSection
            
            header >>. pipe5 hp cs od ar slider (fun hp cs od ar (sv, tickRate) -> {
                healthDrain = hp
                circleSize = cs
                overallDifficulty = od
                approachRate = ar
                baseSliderVelocity = sv
                sliderTickRate = tickRate
            })
            
        let parseColors =
            let header = parseSectionHeader "Colours"
            let color = tuple3 (puint8 .>> pchar ',') (puint8 .>> pchar ',') puint8 |>> fun (r, g, b) -> Color(byte r, byte g, byte b)
            let combo = pstring "Combo" >>. pint32
            let sliderTrack = (parseValue "SliderTrackOverride" color |>> Some) <|> preturn option.None
            let sliderBorder = (parseValue "SliderBorder" color |>> Some) <|> preturn option.None
            
            let comboColors = many (parseKVPair combo color)
            
            header >>. pipe3 comboColors sliderTrack (sliderBorder .>> endOfSection) (fun comboColors sliderTrack sliderBorder -> {
                comboColors = comboColors
                sliderTrackColor = sliderTrack
                sliderBorderColor = sliderBorder
            })        

        let createBeatmap version (settings, editor, metadata, difficulty) timingPoints colors hitObjects = {
            fileFormatVersion = version
            settings = settings
            metadata = metadata
            editorSettings = editor
            difficultySettings = difficulty
            colorSettings = colors
            timingPoints = timingPoints
            hitObjects = hitObjects
        }
        
        // Hack until events are properly parsed and implemented
        let events = skipCharsTillString "[TimingPoints]" false 10000
        
        pipe5 versionHeader (tuple4 settings editor metadata (difficulty .>> events)) parseTimingPoints parseColors parseHitObjects createBeatmap
        
    let ParseBeatmap content: Map.Beatmap =
        match run parseBeatmap content with
        | Success(map, _, _) -> map
        | Failure(err, _, _) -> raise (Exception(err))