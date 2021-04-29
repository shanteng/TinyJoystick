
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateBehaviourInfo
{
    public int mNameHash;//状态机名的Hash
    public float mNormalizedTime;//运行时间
    public float mFrameRate;//帧率
    public int mTotalFrame;//总帧数
    public int mRunningFrame;//当前帧数(会不断递增)

    public void Init(AnimatorClipInfo clipInfo)
    {
        this.mFrameRate = clipInfo.clip.frameRate;
        this.mTotalFrame = Mathf.FloorToInt(clipInfo.clip.length / (1f / this.mFrameRate));
        this.mRunningFrame = 0;
        this.mNormalizedTime = 0;
    }
}


public enum FrameParam
{
    JumpReady,//准备起跳
    BeginFalling,
}

[System.Serializable]
public class StateCallBackFrame
{
    [Title("回调帧", "#FF4F63")]
    public int mFrame;
    [Title("回调参数", "#FF4F63")]
    public FrameParam mParam;
}


public class StateBehaviour : StateMachineBehaviour
{
    [Title("状态机名", "#FF4F63")]
    public string mName;
    [Title("回调帧配置", "#FF4F63")]
    public List<StateCallBackFrame> mCallBackFrames;

    protected LocalPlayerMotionHandler mLocalMotion;
    protected StateBehaviourInfo mStateInfo;
    protected Queue<StateCallBackFrame> mCallBackFrameQueue;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        this.mCallBackFrameQueue = new Queue<StateCallBackFrame>();
        for (int i = 0; i < this.mCallBackFrames.Count; ++i)
        {
            this.mCallBackFrameQueue.Enqueue(this.mCallBackFrames[i]);
        }
        this.mLocalMotion = animator.GetComponent<LocalPlayerMotionHandler>();
        this.mStateInfo = new StateBehaviourInfo();
        this.mStateInfo.mNameHash = Animator.StringToHash(this.mName);
        AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(layerIndex);
        if (clips.Length > 0)
            this.mStateInfo.Init(clips[0]);

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (this.mLocalMotion == null) return;
        this.mStateInfo.mNormalizedTime = stateInfo.normalizedTime % 1;
        this.mStateInfo.mRunningFrame = Mathf.FloorToInt(this.mStateInfo.mTotalFrame * stateInfo.normalizedTime);
        if (this.mCallBackFrameQueue.Count > 0)
        {
            StateCallBackFrame peekFrame = this.mCallBackFrameQueue.Peek();
            if (this.mStateInfo.mRunningFrame >= peekFrame.mFrame)
            {
                this.mLocalMotion.OnBehaviourCallBack(this.mStateInfo, this.mCallBackFrameQueue.Dequeue());
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (this.mLocalMotion == null) return;
    }
}//end class