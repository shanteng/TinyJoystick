
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum XMoveDirection
{
    None = 0,
    Left = -1,//向左边
    Right = 1,//向右边
}

public class AttackingInfo
{
    public List<SkillConfig> mAttackQueue = new List<SkillConfig>();
    public SkillConfig mCurrentAttack = null;
    public float mStartTime;
    public Coroutine mAttackProcess;
    public bool mHitBegin = false;

    public AttackingInfo()
    {
        this.Reset();
    }

    public void Reset()
    {
        this.mAttackQueue.Clear();
        this.mCurrentAttack = null;
        this.mStartTime = 0;
        this.mHitBegin = false;
        mAttackProcess = null;
    }

    public bool IsAttackCharging => this.mCurrentAttack != null && this.mCurrentAttack._Type == SkillType.Charging;

    public float RunningSecs => (Time.time - this.mStartTime);

    public bool IsBeginEndShake()
    {
        //后摇开启前都判定为可伤害
        return this.mCurrentAttack != null && this.RunningSecs >= this.mCurrentAttack._EndShakeStartTime;
    }

    public bool IsAttackOver()
    {
        return (this.mCurrentAttack == null ||  this.RunningSecs >= this.mCurrentAttack._Length) && this.mAttackQueue.Count == 0;
    }

    public bool IsAttacking => this.mCurrentAttack != null || this.mAttackQueue.Count > 0;
}

[System.Serializable]
public class JumpConfig
{
    [Title("弹跳力", "#FF4F63")]
    public float mJumpSpeed = 12;
}

[System.Serializable]
public class TurningData
{
    public float duration = 0.3f;
    public float xOffset = 1.5f;
}

[System.Serializable]
public class JumpData
{
    [Title("下次起跳时间间隔", "#FF4F63")]
    public float mCallGap = 0.2f;
    [Title("多段跳跃配置", "#FF4F63")]
    public List<JumpConfig> mJumpConfigs;
    [HideInInspector]
    public int mJumpCount = 0;
    [HideInInspector]
    public float mLastCallTime = 0;

    public void Reset()
    {
        this.mJumpCount = 0;
        this.mLastCallTime = 0;
    }

    //func
    public bool CanJump()
    {
        if (this.mJumpCount >= this.mJumpConfigs.Count)
            return false;
        float gap = Time.time - this.mLastCallTime;
        if (gap < this.mCallGap)
            return false;
        return true;
    }
    
    public float CurJumpStrength
    {
        get
        {
            if (this.mJumpCount < 1)
                return 0;
            return this.mJumpConfigs[this.mJumpCount - 1].mJumpSpeed;
        }
    }
}

public class LocalPlayerMotionHandler : MonoBehaviour
{
    public float mMaxSpeedValue = 10f;
    public AnimationCurve mSpdToForceDeltaCurve;
    public float mResistanceRate = 16f;

    public AnimationCurve mSpdToForceSkyDeltaCurve;
    public float mResistanceSkyRate = 6f;

    [Title("正向旋转角度", "#FF4F63")]
    public float mRotateY = 150;

    [Title("转身Tween配置", "#FF4F63")]
    public TurningData mTurnData;

    [Title("重力", "#FF4F63")]
    public float mGravity = 20.0f;
    [Title("跳跃配置", "#FF4F63")]
    public JumpData mJumpData;
    [Title("检测地面碰撞的身体百分比", "#FF4F63")]
    public float groundCheckElevation = 0.5f;
    public float groundThreshold = 0.05f;
    public float mMaxDeltaTime = 0.017f;

    public Vector3 mMotion = Vector3.zero;

    private NetworkPlayer mPlayer;
    private bool mIsGrounded = true;
    private XMoveDirection _direction = XMoveDirection.Right;
    private Dictionary<XMoveDirection, float> _FaceDirRotateYDic;

    public List<SkillScriptableObject> mSkillConfigList;
    private AttackingInfo mAttackInfo;

    public XMoveDirection Dir => this._direction;
    private void Awake()
    {
        this.mPlayer = this.GetComponent<NetworkPlayer>();
        _FaceDirRotateYDic = new Dictionary<XMoveDirection, float>();
        _FaceDirRotateYDic.Add(XMoveDirection.Right, mRotateY);
        _FaceDirRotateYDic.Add(XMoveDirection.Left, mRotateY-180);

        this.mAttackInfo = new AttackingInfo();
      
        this.ChangeDir(XMoveDirection.Right);
    }

    public void MoveCall(InstructionDefine callType)
    {
        if (callType == InstructionDefine.DoStartMove)
        {
          
        }
        else if (callType == InstructionDefine.DoMoveing)
        {
            
        }
        else if (callType == InstructionDefine.DoMoveEnd)
        {
           
        }
    }

    #region StateBehaviour

    public void OnBehaviourCallBack(StateCallBackFrame callFrameData)
    {
        if (callFrameData.mType == FrameParamType.JumpReady)
        {
            this.mMotion.y = this.mJumpData.CurJumpStrength;
            this.mPlayer.OnReadyJump();
        }
        else if (callFrameData.mType == FrameParamType.BeginFalling)
        {
            this.mPlayer.OnBeginFalling();
        }
        else if (callFrameData.mType == FrameParamType.Attack)
        {
            this.BeginAttack();
        }
        else if (callFrameData.mType == FrameParamType.HoldingAttack)
        {
            this.BeginHoldingAttack();
        }
    }
    #endregion

    private void BeginHoldingAttack()
    {
        this.mAttackInfo.mStartTime = Time.time;
        this.mPlayer.HoldingAttack();
    }

    private void BeginAttack()
    {
        //攻击判定协程
        // this.PlayAttackEffect();
        if (mAttackInfo.mAttackProcess != null)
            StopCoroutine(mAttackInfo.mAttackProcess);

        this.mAttackInfo.mStartTime = Time.time;
        this.mAttackInfo.mHitBegin = false;
        mAttackInfo.mAttackProcess = StartCoroutine(AttackProcedural());
    }

    IEnumerator AttackProcedural()
    {
        while (!this.mAttackInfo.IsAttackOver())
        {
            //攻击判定开始
            if (this.mAttackInfo.IsBeginEndShake())
            {
                if (this.DoAttackCommand())
                    yield break;
            }
            else
            {
                if (!this.mAttackInfo.mHitBegin)
                {
                    this.mAttackInfo.mHitBegin = true;
                    //this._actor.mRig.EnableHitDetection(mAttackBubbleGroup, ProcessDamage);
                }
            }//end else
            yield return 0;
        }//end while

        this.AttackEnd();
    }

  

    public void CallJump()
    {
        if (this.mJumpData.CanJump() && this.mAttackInfo.IsAttacking == false)
        {
            this.mPlayer.TriggerJump(this.mJumpData.mJumpCount);
            this.mJumpData.mLastCallTime = Time.time;
            this.mJumpData.mJumpCount++;
        }
    }

    //public func
    public void ChangeDir(XMoveDirection dir)
    {
        _direction = dir;
        this.transform.localRotation = Quaternion.AngleAxis(this._FaceDirRotateYDic[this._direction], Vector3.up);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.CallJump();
        }

        if (this.mAttackInfo.IsAttacking == false)
            this.EvaluateHorizalMoving();
        this.ProcessingPostion();
    }

    private void EvaluateSpeedStrength(float dt)
    {
        XMoveDirection useDir = Joystick.Instance.Data.Dir;
        float targetSpeed = mMaxSpeedValue * Joystick.Instance.Data.RateAbs.x * (int)useDir;//目标速度
        float curSpeed = (this.mMotion.x);

        AnimationCurve useCurve = this.mIsGrounded ? this.mSpdToForceDeltaCurve : this.mSpdToForceSkyDeltaCurve;
        float curForce = Mathf.Abs(useCurve.Evaluate(Joystick.Instance.Data.RateAbs.x) * dt) * (int)useDir;  //牵引力

        if (UtilTools.IsFloatSame(curSpeed, targetSpeed))
        {
            this.mMotion.x = targetSpeed;
        }
        else
        {
            this.mMotion.x += curForce;
            float useResistanceRate = this.mIsGrounded ? this.mResistanceRate : this.mResistanceSkyRate;
            float curResistance = Mathf.Abs(this.mMotion.x * useResistanceRate * dt) * (int)this.Dir;      //摩擦力
            bool isDirChanged = this.mMotion.x * (int)this.Dir < 0;
            if (isDirChanged)
                curResistance = -curResistance;

            this.mMotion.x -= curResistance;

            if (this.mIsGrounded && useDir == XMoveDirection.None)
            {
                //地面松开摇杆则直接停下，无需通过摩擦力缓慢停下
                this.mMotion.x = 0;
            }
           
            if (useDir == this.Dir && Mathf.Abs(this.mMotion.x) > Mathf.Abs(targetSpeed))
            {
                this.mMotion.x = targetSpeed;
            }
            else if ((int)useDir * (int)this.Dir < 0 && Mathf.Abs(this.mMotion.x) > Mathf.Abs(targetSpeed))
            {
                this.mMotion.x = targetSpeed;
            }
            else if (useDir == XMoveDirection.None && Mathf.Abs(this.mMotion.x) < 0)
            {
                this.mMotion.x = 0;
            }
        }
    }

    private void ProcessingPostion()
    {
        float deltaTime = Time.deltaTime;
        if(this.mIsGrounded == false)
            this.mMotion.y -= this.mGravity * deltaTime;
        Vector3 newPosition = this.transform.position + deltaTime * this.mMotion;
        //着地判定
        bool PreGround = this.mIsGrounded;
        this.mIsGrounded = false;
        if (Physics.Raycast(newPosition + Vector3.up * groundCheckElevation, Vector3.down, out var hitInfo, groundCheckElevation * 2f, 1))
        {
            if (hitInfo.distance < groundCheckElevation + groundThreshold && this.mMotion.y <= 0)
            {
                this.mIsGrounded = true;
                newPosition.y += groundCheckElevation - hitInfo.distance;
            }
        }

        this.transform.position = newPosition;
        this.mPlayer.SetIsGrounded(this.mIsGrounded);


        if (PreGround && !this.mIsGrounded && UtilTools.IsFloatSame(this.mMotion.y, 0))
        {
            //跌落
            this.FallFromBorder();
        }
        else if (this.mIsGrounded && !PreGround && this.mMotion.y < 0)
        {
            this.StartTouchGround();
            this.mJumpData.Reset();
            this.mPlayer.ResetJump();
        }
    }

    private void EvaluateHorizalMoving()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime > this.mMaxDeltaTime)
            deltaTime = this.mMaxDeltaTime;

        XMoveDirection useDir = Joystick.Instance.Data.Dir;
        float SpeedRate = Mathf.Abs(this.mMotion.x) / mMaxSpeedValue;
        bool preHasSpeed = UtilTools.IsFloatSame(SpeedRate, 0) == false;
        //计算速度
        this.EvaluateSpeedStrength(deltaTime);

        if ((int)useDir * (int)this.Dir < 0 && this.mIsGrounded)
        {
            this.mMotion.x = 0;
            this.ChangeDir(useDir);
            return;
        }

        float SpeedRateNow = Mathf.Abs(this.mMotion.x) / mMaxSpeedValue;
        bool hasSpeedNow = UtilTools.IsFloatSame(SpeedRateNow, 0) == false;
        if (this.mIsGrounded)
        {
            if (hasSpeedNow && !preHasSpeed && this.mJumpData.mJumpCount == 0)
                this.mPlayer.TriggerRun();
            else if (!hasSpeedNow && this.mJumpData.mJumpCount == 0)
            {
                this.mMotion.x = 0;
                this.mPlayer.TriggerIdle();
            }
        }
        this.mPlayer.SetSpeed(SpeedRateNow);
    }

    private void FallFromBorder()
    {
        this.mJumpData.mJumpCount = 1;//掉落消耗一次跳跃
        this.mJumpData.mLastCallTime = Time.time;
        this.mPlayer.TriggerFalling();
    }

    private void StartTouchGround()
    {
        this.mPlayer.ResetJump();
        this.mMotion.y = 0;
        this.mMotion.x = 0;
    }


    public void CallAttackHolding(JoyButtonResponseData data)
    {
        if (this.mAttackInfo.mCurrentAttack == null || this.mAttackInfo.IsBeginEndShake() || this.mAttackInfo.mCurrentAttack._keyData.mCode != data.mCode)
        {
            this.CallAttackTouched(data);
        }
        else if (this.mAttackInfo.mCurrentAttack._keyData.mCode == data.mCode)
        {
            //相同指令，构造连击
            data.mEvent = JoyButtonEvent.Touched;
            SkillConfig lastBuff = this.mAttackInfo.mAttackQueue.Count > 0 ? this.mAttackInfo.mAttackQueue[mAttackInfo.mAttackQueue.Count - 1] : this.mAttackInfo.mCurrentAttack;
            while (lastBuff != null && lastBuff._NextAttackValue > 0)
            {
                this.CallAttackTouched(data);
                lastBuff = this.mAttackInfo.mAttackQueue.Count > 0 ? this.mAttackInfo.mAttackQueue[mAttackInfo.mAttackQueue.Count - 1] : this.mAttackInfo.mCurrentAttack;
            }
        }
    }

    public void CallAttackEnd(JoyButtonResponseData data)
    {
        //蓄力释放
        if (this.mAttackInfo.IsAttackCharging)
        {
            float passtime = Time.time - this.mAttackInfo.mStartTime;
            if (passtime >= this.mAttackInfo.mCurrentAttack._ChargingSecs)
            {
                this.mPlayer.TriggerAttack(this.mAttackInfo.mCurrentAttack._AttackValue,false);
            }
            else
            {
                this.mAttackInfo.Reset();
            }
        }
    }

    public void CallAttackTouched(JoyButtonResponseData data)
    {
        SkillConfig useConfig;
        SkillConfig lastBuff = this.mAttackInfo.mAttackQueue.Count > 0 ? this.mAttackInfo.mAttackQueue[mAttackInfo.mAttackQueue.Count - 1] : this.mAttackInfo.mCurrentAttack;
        bool isSameCommand = lastBuff != null && data.IsEqual(lastBuff._keyData);
        if (isSameCommand)
        {
            //判定连击
            useConfig = lastBuff._NextAttackValue > 0 ? this.DataToConfig(data, lastBuff._NextAttackValue) : null;
        }
        else
        {
            //第一步攻击
            useConfig = this.DataToConfig(data, 0);
        }

        if (useConfig == null)
            return;

        if (isSameCommand == false)
            this.mAttackInfo.mAttackQueue.Clear();
        this.mAttackInfo.mAttackQueue.Add(useConfig);

        if (this.mAttackInfo.mCurrentAttack == null || this.mAttackInfo.IsBeginEndShake())
            this.DoAttackCommand();
    }


    private bool DoAttackCommand()
    {
        if (this.mAttackInfo.mAttackQueue.Count == 0)
            return false;
        this.mAttackInfo.mCurrentAttack = this.mAttackInfo.mAttackQueue[0];
        this.mAttackInfo.mAttackQueue.RemoveAt(0);
        this.mPlayer.TriggerAttack(this.mAttackInfo.mCurrentAttack._AttackValue, this.mAttackInfo.mCurrentAttack._Type == SkillType.Charging);
        return true;
    }

    private SkillConfig DataToConfig(JoyButtonResponseData data, int attackValue)
    {
        foreach (SkillScriptableObject scp in this.mSkillConfigList)
        {
            if (scp._config._keyData.IsEqual(data))
            {
                if (attackValue == 0 || attackValue == scp._config._AttackValue)
                {
                    return scp._config;
                }
            }
        }
        return null;
    }

    private void AttackEnd()
    {
        this.mAttackInfo.Reset();
    }

    private void CallAttack(JoyButtonResponseData data)
    {
        if (data.mEvent == JoyButtonEvent.Touched ||
            data.mEvent == JoyButtonEvent.SlideDir)
        {
            this.CallAttackTouched(data);
        }
        else if (data.mEvent == JoyButtonEvent.Holding)
        {
            this.CallAttackHolding(data);
        }
        else if (data.mEvent == JoyButtonEvent.EndTouch)
        {
            this.CallAttackEnd(data);
        }
    }

    #region JoyButton
    public void OnUiJoyButtonEvent(JoyButtonCode buttonCode,JoyButtonEvent evt, JoyButtonDir dir)
    {
        JoyButtonResponseData data = new JoyButtonResponseData();
        data.mCode = buttonCode;
        data.mEvent = evt;
        data.mDir = dir;
        data.mIsGrounded = this.mIsGrounded;
        if (data.mCode == JoyButtonCode.Jump && data.mEvent == JoyButtonEvent.Touched)
        {
            this.CallJump();
        }
        else 
        {
            this.CallAttack(data);
        }
    }
    #endregion

    #region JoystickUi
    public void JoyStickBegin()
    {
        this.MoveCall(InstructionDefine.DoStartMove);
    }

    public void JoyStickMove()
    {
        this.MoveCall(InstructionDefine.DoMoveing);
    }

    public void JoyStickEnd()
    {
        this.MoveCall(InstructionDefine.DoMoveEnd);
    }

    public void JoyStickClick()
    {
        this.MoveCall(InstructionDefine.DoClickMove);
    }
    #endregion
}//end class
