
using UnityEngine;

[System.Serializable]
public class JoyButtonResponseData
{
    public bool mIsGrounded;
    public JoyButtonCode mCode;
    public JoyButtonEvent mEvent;
    public JoyButtonDir mDir;

    public bool IsEqual(JoyButtonResponseData data)
    {
        return data.mIsGrounded == this.mIsGrounded &&
             data.mCode == this.mCode &&
              data.mEvent == this.mEvent &&
               data.mDir == this.mDir;
    }
}

    public enum JoyButtonCode
{
    A,
    B,
    Jump,
}

public enum JoyButtonEvent
{
    None=0,
    Touched,
    SlideDir,
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