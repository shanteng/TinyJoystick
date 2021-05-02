
using System.Collections.Generic;
using UnityEngine;

public enum XMoveDirection
{
    None = 0,
    Left = -1,//向左边
    Right = 1,//向右边
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
    public List<SkillConfig> mSkillConfigList;
    private Dictionary<int, SkillConfig> mSkillDic;
    private bool mIsButtonTouched = false;
    private List<SkillConfig> mSkillQueueList = new List<SkillConfig>();
    public XMoveDirection Dir => this._direction;
    private void Awake()
    {
        this.mPlayer = this.GetComponent<NetworkPlayer>();
        _FaceDirRotateYDic = new Dictionary<XMoveDirection, float>();
        _FaceDirRotateYDic.Add(XMoveDirection.Right, mRotateY);
        _FaceDirRotateYDic.Add(XMoveDirection.Left, mRotateY-180);

        this.mSkillDic = new Dictionary<int, SkillConfig>();
        foreach (SkillConfig cfg in this.mSkillConfigList)
        {
            this.mSkillDic.Add(cfg._AttackValue, cfg);
        }

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
    }
    #endregion


    public void CallJump()
    {
        if (this.mJumpData.CanJump())
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
        this.ProcessMoving();
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

    private void ProcessMoving()
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
        else
        {
            this.mMotion.y -= this.mGravity * deltaTime;
        }
 
        Vector3 newPosition = this.transform.position + deltaTime * this.mMotion;
        this.mPlayer.SetSpeed(SpeedRateNow);

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

    public void CallAttackTouched(JoyButtonResponseData data)
    {
        mIsButtonTouched = true;

        SkillConfig lastBuff = null;
        if (this.mSkillQueueList.Count > 0)
            lastBuff = this.mSkillQueueList[mSkillQueueList.Count - 1];

        if (lastBuff == null)
        {
            this.mMotion.x = 0;//停止横向移动
        }
    }

    public void CallAttackHolding(JoyButtonResponseData data)
    {
      
    }

    public void CallAttackEnd(JoyButtonResponseData data)
    {
        mIsButtonTouched = false;
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

        SkillConfig useConfig = null;
        foreach (SkillConfig config in this.mSkillDic.Values)
        {
            if (config._keyData.IsEqual(data))
            {
                useConfig = config;
            }
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
