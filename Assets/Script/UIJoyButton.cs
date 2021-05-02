using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//所有滑动列表单项的基类
public class UIJoyButton : MonoBehaviour
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
    , IPointerClickHandler
     , IPointerDownHandler
     , IPointerUpHandler
{
    public Dictionary<JoyButtonDir, Transform> mArrowList;
    public GameObject mPress;
    public RectTransform mControlTransform;

    [Title("按钮枚举", "#FF4F63")]
    public JoyButtonCode mBtnCode;

    [Title("长按判定时长", "#FF4F63")]
    public float mPressNotiInterval = 0.5f;
 
    [Title("滑动判定值", "#FF4F63")]
    public float mOffset = 30;

    [Title("是否需要滑动方向判定", "#FF4F63")]
    public bool mNeedDir = true;

    private UnityAction<JoyButtonCode,JoyButtonEvent, JoyButtonDir> mCallBackFunction;
    private JoyButtonDir mArrowDir = JoyButtonDir.Center;
    private JoyButtonEvent mHasNotifyedEvent = JoyButtonEvent.None;
    private float mTouchStartTime = 0;
    private Vector2 mTouchedAxis = Vector2.zero;
    private float mButtonRadius = 0;
    private Camera mUiCamera;

    void Awake()
    {
        this.mUiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
        this.mArrowList = new Dictionary<JoyButtonDir, Transform>();
        this.mArrowList.Add(JoyButtonDir.Left, this.transform.Find("Arrow/Left"));
        this.mArrowList.Add(JoyButtonDir.Right, this.transform.Find("Arrow/Right"));
        this.mArrowList.Add(JoyButtonDir.Up, this.transform.Find("Arrow/Up"));
        this.mArrowList.Add(JoyButtonDir.Down, this.transform.Find("Arrow/Down"));
        this.mButtonRadius = this.GetComponent<RectTransform>().sizeDelta.x / 2;
        this.ResetToInit();
    }


    public void AttachToUiHandler(LocalPlayerMotionHandler uiHandle)
    {
        this.mCallBackFunction = uiHandle.OnUiJoyButtonEvent;
    }

    private void CallBackEvent(JoyButtonEvent evt)
    {
        if (this.mCallBackFunction != null)
            this.mCallBackFunction.Invoke(mBtnCode,evt, this.mArrowDir);
    }

    private void ResetToInit()
    {
        this.mArrowDir = JoyButtonDir.Center;
        this.mHasNotifyedEvent = JoyButtonEvent.None;
        this.mTouchStartTime = 0;
        this.UpdateArrowDir();
        this.mControlTransform.anchoredPosition = Vector2.zero;
        this.mPress.SetActive(false);
    }

    public bool IsTouched => (false == UtilTools.IsFloatSame(this.mTouchStartTime, 0));


    void Update()
    {
        if (this.IsTouched)
        {
            float PressDelta = Time.time - this.mTouchStartTime;
            if (PressDelta > this.mPressNotiInterval && this.mHasNotifyedEvent == JoyButtonEvent.None)
            {
                //通知长按事件
                this.mHasNotifyedEvent = JoyButtonEvent.Holding;
                this.CallBackEvent(JoyButtonEvent.Holding);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 worldPosition;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(this.mControlTransform, eventData.position, mUiCamera, out worldPosition))
        {
            this.SetJoyStickAxis(worldPosition);
        }
        this.mPress.SetActive(true);
        this.mTouchStartTime = Time.time;
        //按下直接响应Click
        this.CallBackEvent(JoyButtonEvent.Touched);
    }

    private void SetJoyStickAxis(Vector3 worldPos)
    {
        this.mControlTransform.position = worldPos;
        this.mTouchedAxis = mControlTransform.anchoredPosition;
        if (this.mTouchedAxis.magnitude > mButtonRadius)
            this.mTouchedAxis = this.mTouchedAxis.normalized * mButtonRadius;
        mControlTransform.anchoredPosition = this.mTouchedAxis;

        float XOffset = Mathf.Abs(this.mTouchedAxis.x);
        float YOffset = Mathf.Abs(this.mTouchedAxis.y);

        float maxOffset = XOffset >= YOffset ? XOffset : YOffset;
        //float halfHalfRadius = this.mButtonRadius / 2;
        this.mArrowDir = JoyButtonDir.Center;
        if (maxOffset >= this.mOffset && this.mNeedDir)
        {
            if (XOffset >= YOffset)
            {
                this.mArrowDir = this.mTouchedAxis.x > 0 ? JoyButtonDir.Right : JoyButtonDir.Left;
            }
            else
            {
                this.mArrowDir = this.mTouchedAxis.y > 0 ? JoyButtonDir.Up : JoyButtonDir.Down;
            }
        }

        this.UpdateArrowDir();
    }

    private void UpdateArrowDir()
    {
        foreach (JoyButtonDir dir in mArrowList.Keys)
        {
            mArrowList[dir].gameObject.SetActive(dir == this.mArrowDir);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        this.EndTouch();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPosition;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(this.mControlTransform, eventData.position, mUiCamera, out worldPosition))
        {
            this.SetJoyStickAxis(worldPosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.EndTouch();
    }

    public void EndTouch()
    {
        if (false == this.IsTouched)
            return;
        if (this.mHasNotifyedEvent == JoyButtonEvent.None && this.mArrowDir != JoyButtonDir.Center)
        {
            //通知是一个滑动操作
            this.CallBackEvent(JoyButtonEvent.SlideDir);
        }
        else
        {
            //通知已经抬起了
            this.CallBackEvent(JoyButtonEvent.EndTouch);
        }

        this.ResetToInit();
    }

}


