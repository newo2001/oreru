namespace BeatmapParser

module Map =
    type Color = byte * byte * byte
    
    type CountdownType = None | Normal | Half | Double
    
    type SampleSet = Default | Normal | Soft | Drum
    
    type GameMode = Standard | Taiko | Catch | Mania
    
    type OverlayPosition = NoChange | Below | Above
    
    type SliderCurveType = Bezier | Catmull | Linear | PerfectCircle
    
    type TimeSignature = FullBeat | HalfBeat | OneThird | OneFourth | OneSixth | OneEighth | OneTwelfth | OneSixteenth
    
    type GridSize = Tiny | Small | Medium | Large
    
    type UnicodeString = {
        romanised: string
        unicode: string
    }
    
    type BeatmapSettings = {
        audioFileName: string
        audioLeadIn: int
        previewPoint: option<int>
        countdown: CountdownType
        defaultSampleSet: SampleSet
        stackLeniency: float
        gameMode: GameMode
        letterBoxDuringBreaks: bool
        useSkinSprites: bool
        hitCircleOverlayPosition: OverlayPosition
        preferredSkin: option<string>
        showEpilepsyWarning: bool
        countdownOffset: int
        widescreenStoryboard: bool
        samplesMatchPlaybackRate: bool
    }
    
    type BeatmapMetadata = {
        songTitle: UnicodeString
        artist: UnicodeString
        creator: string
        difficultyName: string
        songSource: string option
        tags: string list
        beatmapId: int
        beatmapSetId: int
    }
    
    type EditorSettings = {
        distanceSnap: float
        beatSnapDivisor: float
        gridSize: GridSize
        timelineZoom: float
        bookmarks: int list
    }
    
    type ColorSettings = {
        comboColors: Color list
        sliderTrackColor: option<Color>
        sliderBorderColor: option<Color>
    }
    
    type DifficultySettings = {
        healthDrain: float
        circleSize: float
        overallDifficulty: float
        approachRate: float
        baseSliderVelocity: float
        sliderTickRate: float
    }
    
    type UnInheritedTimingPoint = {
        timestamp: int
        bpm: float
        timeSignature: TimeSignature
        sampleSet: SampleSet
        volume: int
        kiai: bool
    }
    
    type InheritedTimingPoint = {
        timestamp: int
        sliderVelocity: float
        sampleSet: SampleSet
        volume: int
        kiai: bool
    }
    
    type TimingPoint =
        | InheritedTimingPoint of InheritedTimingPoint
        | UnInheritedTimingPoint of UnInheritedTimingPoint
        
    type HitCircle = {
        x: int
        y: int
        time: int
        hitSounds: int
        newCombo: bool
    }
    
    type AnchorType = Red | White
    
    type AnchorPoint = {
        x: int
        y: int
        anchorType: AnchorType
    }
    
    type Slider = {
        x: int
        y: int
        time: int
        length: float
        hitSounds: int
        newCombo: bool
        repeats: int
        curveType: SliderCurveType
        curvePoints: AnchorPoint list
    }
    
    type Spinner = {
       startTime: int
       endTime: int
       hitSounds: int
       newCombo: bool
    }
    
    type HitObject =
        | Circle of HitCircle
        | Slider of Slider
        | Spinner of Spinner
    
    type Beatmap = {
        fileFormatVersion: int
        settings: BeatmapSettings
        editorSettings: EditorSettings
        metadata: BeatmapMetadata
        difficultySettings: DifficultySettings
        timingPoints: TimingPoint list
        colorSettings: ColorSettings
        hitObjects: HitObject list
    }