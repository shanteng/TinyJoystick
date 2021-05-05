
using UnityEngine;


public class TriggerKeyHash
{
    public static int State = Animator.StringToHash("State");
    public static int AttackValue = Animator.StringToHash("AttackValue");
    public static int Attack = Animator.StringToHash("Attack");
    public static int Speed = Animator.StringToHash("Speed");
    public static int Jump = Animator.StringToHash("Jump");
    public static int Falling = Animator.StringToHash("Falling");
    public static int IsGrounded = Animator.StringToHash("IsGrounded");
    public static int JumpCount = Animator.StringToHash("JumpCount");
    public static int Holding = Animator.StringToHash("Holding");
}


public class StateHash
{
    public static int TouchGround = Animator.StringToHash("TouchGround");
}


public class UtilTools
{
    public static bool IsFloatSame(float a, float b, float accurate = 0.001f)
    {
        return Mathf.Abs(a - b) <= accurate;
    }
}