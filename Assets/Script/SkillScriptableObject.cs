using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/SkillScriptableObject")]
public class SkillScriptableObject : ScriptableObject
{
    public SkillConfig _config;
}


public enum SkillType
{
    Directly,//直接触发
    Charging,//蓄力攻击
}

public enum TriggerWay
{
    None,
    Grounded,
    NotGrounded,
}

[System.Serializable]
public class SkillConfig
{
    [Title("按下触发的AttackDir", "#FFFF0F")]
    public int _AttackValue;

    [Title("攻击类型", "#FFFF0F")]
    public SkillType _Type;

    [Title("攻击触发类型", "#FFFF0F")]
    public JoyButtonResponseData _keyData;

    [Title("连击唯一标识", "#FFFF0F")]
    public int _NextAttackValue;

    [Title("技能唯一标识", "#FFFF0F")]
    public float _AttackDamage = 30;

    [Title("攻击特效信息", "#FFFF0F")]
    public FxInfo _AttackFx;

    [Title("打击特效信息", "#FFFF0F")]
    public FxInfo _AttackHitFx;

    [Title("后摇开始时间", "#FFFF0F")]
    public float _EndShakeStartTime;

    [Title("位移", "#FFFF0F")]
    public FrameMove _FrameMove;

    [Title("攻击判定气泡组", "#FFFF0F")]
    public string _HitBubbleGroup = "weapon";

    [Title("蓄力所需时间", "#FFFF0F")]
    public float _ChargingSecs;

    [Title("动作时长", "#FFFF0F")]
    public float _Length;
}

[System.Serializable]
public struct FxInfo
{
    public GameObject prefab;
    public float lifetime;
}

[System.Serializable]
public class FrameMove
{
    [Title("是否需要位移", "#FFFF0F")]
    public bool mNeedMove;
    [Title("开始位移的时间", "#FFFF0F")]
    public float mStartTime;
    [Title("X位移曲线", "#FFFF0F")]
    public AnimationCurve _xCurve;
    [Title("Y位移曲线", "#FFFF0F")]
    public AnimationCurve _yCurve;
}

[System.Serializable]
public class FrameData
{
    public int _Start;
    public int _End;
    public bool InFrame(int current)
    {
        return current >= this._Start && current <= this._End;
    }
}