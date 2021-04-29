
using UnityEngine;


public enum JoyButtonCode
{
    A,
    B,
    Jump,
}

public enum JoyButtonEvent
{
    None=0,
    BeginTouch,
    SlidingClickDir,
    Holding,
    EndTouch,
}

public enum JoyButtonDir
{
    Center = 0,
    Up,
    Down,
    Left,
    Right,
}


public enum AttackKeyEvent
{
    None = 1,
    Click,
    Holding,
    Up,
    Down,
    Left,
    Right,
    EndTouch,
}

public enum InstructionDefine
{
    None,

    DoStartMove,
    DoMoveing,
    DoClickMove,

    DoStartUp,
    DoUpMoveing,
    DoClickUpper,

    DoStartDown,
    DoDownMoveing,
    DoClickDown,

    DoMoveEnd,

    DoJump,
    AttackTouched,
    AttackHolding,
    AttackTouchOver,
}