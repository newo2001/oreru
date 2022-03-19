namespace BPM

open System

module Replay =
    type GameMode = Standard | Taiko | Catch | Mania
    
    [<Flags>]
    type Mod = 
        | NoMod = 0
        | NoFail = 1
        | Easy = 2
        | TouchDevice = 4
        | Hidden = 8
        | HardRock = 16
        | SuddenDeath = 32
        | DoubleTime = 64
        | Relax = 128
        | HalfTime = 256
        | NightCore = 512
        | FlashLight = 1024
        | AutoPlay = 2048
        | SpunOut = 4096
        | AutoPilot = 8192
        | Perfect = 16384
        | ScoreV2 = 536870912

    [<Flags>]
    type Key =
        | None = 0
        | LeftMouse = 1
        | RightMouse = 2
        | Key1 = 4
        | Key2 = 8
        | Smoke = 16

    type Frame = {
        timestamp: int64
        cursor: float*float
        keys: Key
    }

    type ReplayMetadata = {
        gameMode: GameMode
        gameVersion: int
        mapHash: string
        playerName: string
        replayHash: string
        timestamp: int64
        mods: Mod
        scoreId: int64
    }

    type ReplayStats = {
        num300: int16
        num100: int16
        num50: int16
        misses: int16
        gekis: int16
        katsus: int16
        score: int
        maxCombo: int16
        fullCombo: bool
    }

    type Replay = {
        metadata: ReplayMetadata
        stats: ReplayStats
        frames: Frame list
    }