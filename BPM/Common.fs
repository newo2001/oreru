namespace BPM

open FParsec

module Common =
    type UserState = unit
    type Parser<'t> = Parser<'t, UserState>

    type UnicodeString = {
        romanised: string
        unicode: string
    }

    type Color = byte * byte * byte

    type GameMode = Standard | Taiko | Catch | Mania

    type Axis = Horizontal | Vertical