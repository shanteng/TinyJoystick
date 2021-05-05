
using UnityEngine;

public enum StateValue
{
    None = 0,
    Idle = 1,
    Run = 2,
    Attack = 3,
}

public class NetworkPlayer : MonoBehaviour
{
    private Animator mAnimator;

    void Awake()
    {
        this.mAnimator = this.GetComponent<Animator>();
    }

    #region 状态机
    public void TriggerIdle()
    {
        this.SetInt(TriggerKeyHash.State, (int)StateValue.Idle);
    }

    public void TriggerRun()
    {
        this.SetInt(TriggerKeyHash.State, (int)StateValue.Run);
    }


    public void TriggerAttack(int attackvalue,bool isHolding)
    {
        this.SetInt(TriggerKeyHash.State, (int)StateValue.Attack);
        this.SetInt(TriggerKeyHash.AttackValue, attackvalue);
        this.SetIsHoldingAttack(isHolding);
    }

    public void HoldingAttack()
    {
        this.SetInt(TriggerKeyHash.AttackValue, 0);
    }

    public void SetIsHoldingAttack(bool ishold)
    {
        this.SetBool(TriggerKeyHash.Holding, ishold);
    }


    public void TriggerJump(int jumpCount)
    {
        this.SetNoneState();
        this.SetBool(TriggerKeyHash.Jump,true);
        this.SetInt(TriggerKeyHash.JumpCount, jumpCount);
    }

    public void ResetJump()
    {
        this.SetBool(TriggerKeyHash.Jump, false);
        this.SetInt(TriggerKeyHash.JumpCount, 0);
    }

    public void OnReadyJump()
    {
        this.SetBool(TriggerKeyHash.Jump, false);
    }

    public void TriggerFalling()
    {
        this.SetNoneState();
        this.SetBool(TriggerKeyHash.Falling, true);
    }

    public void OnBeginFalling()
    {
        this.SetBool(TriggerKeyHash.Falling, false);
    }

    public void SetIsGrounded(bool isgrd)
    {
        this.SetBool(TriggerKeyHash.IsGrounded, isgrd);
        if (isgrd)
        {
            this.SetBool(TriggerKeyHash.Falling, false);
        }
    }


    public void SetNoneState()
    {
        this.SetInt(TriggerKeyHash.State, (int)StateValue.None);
    }

    public void SetSpeed(float spd)
    {
        this.SetFloat(TriggerKeyHash.Speed, spd);
    }

    public void SetTrigger(int keyHash)
    {
        this.mAnimator.SetTrigger(keyHash);
    }


    public void SetFloat(int keyHash, float value)
    {
        this.mAnimator.SetFloat(keyHash, value);
    }

  

    public void SetInt(int keyHash, int value)
    {
        this.mAnimator.SetInteger(keyHash, value);
    }

  

    public void SetBool(int keyHash, bool value)
    {
        this.mAnimator.SetBool(keyHash, value);
    }

  
    #endregion
}//end class
