
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FrameParamType
{
    JumpReady,//准备起跳
    BeginFalling,
    HoldingAttack,
    Attack,
}

[System.Serializable]
public class StateCallBackFrame
{
    [Title("回调开始时间", "#FF4F63")]
    public float mFrameSecs;
    [Title("回调类型", "#FF4F63")]
    public FrameParamType mType;
    [Title("回调参数", "#FF4F63")]
    public string mParam;
}

public class StateBehaviour : StateMachineBehaviour
{
    [Title("状态机名", "#FF4F63")]
    public string mName;
    [Title("回调帧配置", "#FF4F63")]
    public List<StateCallBackFrame> mCallBackFrames;

    protected LocalPlayerMotionHandler mLocalMotion;
    protected float mStateLenght = 0;
    protected Queue<StateCallBackFrame> mCallBackFrameQueue;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        this.mCallBackFrameQueue = new Queue<StateCallBackFrame>();
        for (int i = 0; i < this.mCallBackFrames.Count; ++i)
        {
            this.mCallBackFrameQueue.Enqueue(this.mCallBackFrames[i]);
        }
        this.mLocalMotion = animator.GetComponent<LocalPlayerMotionHandler>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (this.mLocalMotion == null) return;
        if (this.mCallBackFrameQueue.Count > 0)
        {
            StateCallBackFrame peekFrame = this.mCallBackFrameQueue.Peek();
            if (stateInfo.normalizedTime >= peekFrame.mFrameSecs)
            {
                this.mLocalMotion.OnBehaviourCallBack(this.mCallBackFrameQueue.Dequeue());
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (this.mLocalMotion == null) return;
    }
}//end class