namespace BPM

open System
open Microsoft.FSharp.Core
open BPM.Common

module Storyboard =
    type Layer = Background | Fail | Pass | Foreground
    
    type Origin =
        | TopLeft | TopCenter | TopRight
        | CenterLeft | Center | CenterRight
        | BottomLeft | BottomCenter | BottomRight
        
    type LoopType = Once | Forever
    
    type EasingFunction =
        | Linear | EaseOut | EaseIn
        | QuadraticIn | QuadraticOut | QuadraticInOut
        | CubicIn | CubicOut | CubicInOut
        | QuarticIn | QuarticOut | QuarticInOut
        | QuinticIn | QuinticOut | QuinticInOut
        | SineIn | SineOut | SineInOut
        | ExponentialIn | ExponentialOut | ExponentialInOut
        | Circular | CircularOut | CircularInOut
        | ElasticIn | ElasticOut | ElasticHalfOut | ElasticQuarterOut | ElasticInOut
        | BackIn | BackOut | BackInOut
        | BounceIn | BounceOut | BounceInOut
    
    type FadeCommand = {
        easing: EasingFunction
        startTime: TimeSpan
        endTime: TimeSpan
        startOpacity: float
        endOpacity: float
    }
    
    type MoveCommand = {
        easing: EasingFunction
        startTime: TimeSpan
        endTime: TimeSpan
        startX: int
        startY: int
        endX: int
        endY: int
    }
    
    type MoveXCommand = {
        easing: EasingFunction
        startTime: TimeSpan
        endTime: TimeSpan
        startX: int
        endX: int
    }
    
    type MoveYCommand = {
        easing: EasingFunction
        startTime: TimeSpan
        endTime: TimeSpan
        startY: int
        endY: int
    }
    
    type ScaleCommand = {
        easing: EasingFunction
        startTime: TimeSpan
        endTime: TimeSpan
        startScale: float
        endScale: float
    }
    
    type VectorScaleCommand = {
        easing: EasingFunction
        startTime: TimeSpan
        endTime: TimeSpan
        startScaleX: float
        startScaleY: float
        endScaleX: float
        endScaleY: float
    }
    
    type RotateCommand = {
        easing: EasingFunction
        startTime: TimeSpan
        endTime: TimeSpan
        startAngle: float
        endAngle: float
    }
    
    type ColorCommand = {
        easing: EasingFunction
        startTime: TimeSpan
        endTime: TimeSpan
        startColor: Color
        endColor: Color
    }
    
    type FlipCommand = {
        easing: EasingFunction
        startTime: TimeSpan
        endTime: TimeSpan
        axis: Axis
    }
    
    type AdditiveBlendCommand = {
        easing: EasingFunction
        startTime: TimeSpan
        endTime: TimeSpan
    }
    
    type LoopCommand = {
        startTime: TimeSpan
        iterations: int
    }
    
    // TODO trigger command        
    type Command =
        | FadeCommand of FadeCommand | MoveCommand of MoveCommand
        | MoveXCommand of MoveXCommand | MoveYCommand of MoveYCommand
        | ScaleCommand of ScaleCommand | VectorScaleCommand of VectorScaleCommand
        | RotateCommand of RotateCommand | FlipCommand of FlipCommand
        | AdditiveBlendCommand of AdditiveBlendCommand | LoopCommand of LoopCommand
        | ColorCommand of ColorCommand
        
    type Sprite = {
        layer: Layer
        origin: Origin
        filePath: string
        x: int
        y: int
    }

    type Animation = {
        layer: Layer
        origin: Origin
        filePath: string
        x: int
        y: int
        frames: int
        frameDelay: int
        loop: LoopType
    }
    
    type Image = Sprite of Sprite | Animation of Animation
    
    type Effect = {
        image: Image
        commands: Command list
    }
        
    type Storyboard = {
        effects: Effect list
    }