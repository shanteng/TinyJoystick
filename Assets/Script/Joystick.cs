using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum JoyPos
{
    Inner =1,
    Outer = 2,
}

public enum JoyStickSection
{
    None = 0,
    Move = 1,
    Upper = 2,
    Down = 3,
}

public class JoyStickData
{
    public XMoveDirection Dir = XMoveDirection.None;
    public JoyStickSection Section = JoyStickSection.None;
    public JoyPos Pos;
    public Vector2 TouchedAxis = Vector2.zero;
    public Vector2 RateAbs = Vector2.zero;
    public float JoyStickRadius = 0;
    public float JoyStickInnerRadius = 0;
}

public class Joystick : MonoBehaviour
    , IPointerDownHandler
    , IPointerClickHandler
    , IPointerUpHandler
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
{

    private Camera _UiCamera;
    [Title("外半径", "#FF4F63")]
    public float JoyStickRadius = 0;
    [Title("内半径", "#FF4F63")]
    public float JoyStickInnerRadius = 0;
    [Title("中心区域半径", "#FF4F63")]
    public float ResponseOffset = 5f;
    [Title("上下扇区角度", "#FF4F63")]
    public float SectionDegree = 45;
    public float CurrentDegree = 0;

    [HideInInspector]
    public Vector2 _UpDegreeRange = Vector2.zero;
    [HideInInspector]
    public Vector2 _DownDegreeRange = Vector2.zero;

    public RectTransform _curLine;
    private Vector2 _lineSize = Vector2.zero;
    public Image _UpSectionCircle;
    public Image _DownSectionCircle;
    /// <summary>
    /// 中心要移动滑块的Transform组件
    /// </summary>
    public RectTransform ControlTransform;
    public RectTransform OuterTransform;
    public RectTransform InnerTransform;
    public RectTransform CenterTransform;

    private Dictionary<KeyCode, Transform> _KeyboardTransDic;
    private bool isTouched = false;
    private float TouchSecs = 0;
    private float MaxTouchSces = 0;
    private Vector2 originPosition;
    private Vector2 _touchedAxis = Vector2.zero;
    private JoyStickData _CallBackData = new JoyStickData();
    public JoyStickData Data => this._CallBackData;

    public static Joystick Instance;
    public Vector2 NormallizedTouchedAxis
    {
        get
        {
            return _touchedAxis.magnitude > JoyStickRadius ? _touchedAxis.normalized / JoyStickRadius : _touchedAxis.normalized;
        }
    }

    public bool Touched => this.isTouched;

    public delegate void JoyStickTouchBegin();
    public delegate void JoyStickTouchMove();
    public delegate void JoyStickTouchEnd();
    public delegate void JoyStickClick();

    public event JoyStickTouchBegin OnJoyStickTouchBegin;
    public event JoyStickTouchMove OnJoyStickTouchMove;
    public event JoyStickTouchEnd OnJoyStickTouchEnd;
    public event JoyStickClick OnJoyStickClick;

    private NetworkPlayer mControlPlayer;
    void Awake()
    {
        this._UiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
        this._lineSize = this._curLine.sizeDelta;
        Instance = this;
        this.SetRedius(this.JoyStickInnerRadius, this.JoyStickRadius);
        this.SetSectionDegree(this.SectionDegree);
        this.SetCenter(this.ResponseOffset);
        this.ResetPosition();
        this._KeyboardTransDic = new Dictionary<KeyCode, Transform>();
        this._KeyboardTransDic.Add(KeyCode.W, this.OuterTransform.Find("Top"));
        this._KeyboardTransDic.Add(KeyCode.S, this.OuterTransform.Find("Bottom"));
        this._KeyboardTransDic.Add(KeyCode.A, this.OuterTransform.Find("Left"));
        this._KeyboardTransDic.Add(KeyCode.D, this.OuterTransform.Find("Right"));
    }

    void Start()
    {
        //测试入口
        LocalPlayerMotionHandler playerHandler = GameObject.FindGameObjectWithTag("Player").GetComponent<LocalPlayerMotionHandler>();
        this.AttachToUiHandler(playerHandler);
        this.AttachToUiHandler(playerHandler);
    }

    public void AttachToUiHandler(LocalPlayerMotionHandler uiHandle)
    {
        this.OnJoyStickTouchBegin += uiHandle.JoyStickBegin;
        this.OnJoyStickTouchMove += uiHandle.JoyStickMove;
        this.OnJoyStickTouchEnd += uiHandle.JoyStickEnd;
        this.OnJoyStickClick += uiHandle.JoyStickClick;
    }
    
    public void SetCenter(float radius)
    {
        this.ResponseOffset = radius;
        this.CenterTransform.sizeDelta = new Vector2(this.ResponseOffset * 2, this.ResponseOffset * 2);
    }

    public void SetSectionDegree(float degree)
    {
        this.SectionDegree = degree;
        float fillValue = degree / 360;
        float halfDegree = degree / 2;
        this._UpDegreeRange.x = halfDegree;
        this._UpDegreeRange.y = 360 - halfDegree;
        this._DownDegreeRange.x = this._UpDegreeRange.y - 180;
        this._DownDegreeRange.y = this._UpDegreeRange.x + 180;

        this._UpSectionCircle.fillAmount = fillValue;
        this._DownSectionCircle.fillAmount = fillValue;

        this._UpSectionCircle.transform.localEulerAngles = new Vector3(0, 0, halfDegree);
        this._DownSectionCircle.transform.localEulerAngles = new Vector3(0, 0, halfDegree + 180);
    }

    public void SetRedius(float inner, float outer)
    {
        _CallBackData.JoyStickInnerRadius = inner;
        _CallBackData.JoyStickRadius = outer;
        _CallBackData.RateAbs = Vector2.zero;
        _CallBackData.TouchedAxis = Vector2.zero;
        _CallBackData.Section = JoyStickSection.None;
        if (Mathf.Abs(_CallBackData.TouchedAxis.x) < 0.001f)
            _CallBackData.Dir = XMoveDirection.None;
        else
            _CallBackData.Dir = _CallBackData.TouchedAxis.x > 0 ? XMoveDirection.Right : XMoveDirection.Left;

        this.JoyStickRadius = outer;
        this.JoyStickInnerRadius = inner;
        this.OuterTransform.sizeDelta = new Vector2(outer * 2, outer * 2);
        this.InnerTransform.sizeDelta = new Vector2(inner * 2, inner * 2);
        this._UpSectionCircle.GetComponent<RectTransform>().sizeDelta = new Vector2(outer * 2, outer * 2);
        this._DownSectionCircle.GetComponent<RectTransform>().sizeDelta = new Vector2(outer * 2, outer * 2);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        this.BeginTouch(eventData.position);
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
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(ControlTransform, eventData.position, this._UiCamera, out worldPosition))
        {
            this.SetJoyStickAxis(worldPosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.EndTouch();
    }


    private KeyCode _downKeyboardCode = KeyCode.None;
    public void DirKeybordDown(KeyCode code)
    {
        if (this.isTouched)
            return;
        _downKeyboardCode = code;
        Transform tran = this._KeyboardTransDic[code];
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(this._UiCamera, tran.position);
        this.BeginTouch(screenPos);
    }

    public void DirKeybordUp(KeyCode code)
    {
        if (this.isTouched == false || code != this._downKeyboardCode)
            return;
        this._downKeyboardCode = KeyCode.None;
        this.EndTouch();
    }

    public void BeginTouch(Vector3 position)
    {
        Vector3 worldPosition;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(ControlTransform, position, this._UiCamera, out worldPosition))
        {
            this.SetJoyStickAxis(worldPosition);
        }

        if (this._touchedAxis.magnitude >= this.ResponseOffset)
        {
            if(OnJoyStickTouchBegin !=null)
                OnJoyStickTouchBegin();
        }

        this.MaxTouchSces = Time.fixedDeltaTime * 10;
        this.TouchSecs = 0;
        isTouched = true;
    }

    public void EndTouch()
    {
        if (this.isTouched == false)
            return;

        if (this.TouchSecs < this.MaxTouchSces && this._CallBackData.TouchedAxis.magnitude > this.ResponseOffset)
        {
            if (OnJoyStickClick != null)
                OnJoyStickClick();
        }

        this.ResetPosition();
        if (OnJoyStickTouchEnd != null)
            OnJoyStickTouchEnd();
    }

    private void ResetPosition()
    {
        this.ControlTransform.anchoredPosition = Vector2.zero;
        SetJoyStickAxis(this.ControlTransform.position);
        this.TouchSecs = 0;
        this.isTouched = false;
    }

    private void UpdateDegree()
    {
        float degree = Mathf.Atan2(this._touchedAxis.x, this._touchedAxis.y) * Mathf.Rad2Deg;
        if (degree < 0)
            degree += 360;
        CurrentDegree = 360 - degree;
        if (CurrentDegree >= 360)
            CurrentDegree -= 360;
        this._curLine.localEulerAngles = new Vector3(0, 0, CurrentDegree);
        this._lineSize.y = this._touchedAxis.magnitude;
        this._curLine.sizeDelta = this._lineSize;

        if(this._touchedAxis.magnitude <= this.ResponseOffset)
            this._CallBackData.Section = JoyStickSection.None;
        else if ((CurrentDegree >= 0 && CurrentDegree <= this._UpDegreeRange.x) || (CurrentDegree >= this._UpDegreeRange.y))
            this._CallBackData.Section = JoyStickSection.Upper;
        else if (CurrentDegree >= this._DownDegreeRange.x && CurrentDegree <= this._DownDegreeRange.y)
            this._CallBackData.Section = JoyStickSection.Down;
        else
            this._CallBackData.Section = JoyStickSection.Move;
    }

    void Update()
    {
        if (isTouched)
        {
            this.TouchSecs += Time.deltaTime;
            if (OnJoyStickTouchMove != null)
                OnJoyStickTouchMove();
        }
    }

    private void SetJoyStickAxis(Vector3 worldPos)
    {
        this.ControlTransform.position = worldPos;
        this._touchedAxis = ControlTransform.anchoredPosition;
        if (this._touchedAxis.magnitude > JoyStickRadius)
        {
            this._touchedAxis = this._touchedAxis.normalized * JoyStickRadius;
        }

        ControlTransform.anchoredPosition = this._touchedAxis;
        //超过内径就认为到达最大Rate
        float XOffset = Mathf.Abs(this._touchedAxis.x);
        float xRateCur = XOffset / this.JoyStickInnerRadius;
        float YOffset = Mathf.Abs(this._touchedAxis.y);
        float yRateCur = YOffset / this.JoyStickInnerRadius;

        _CallBackData.RateAbs.x = Mathf.Min(1f,xRateCur);
        _CallBackData.RateAbs.y = Mathf.Min(1f, yRateCur);
        _CallBackData.TouchedAxis = this._touchedAxis;
        _CallBackData.Pos = this._touchedAxis.magnitude <= this.JoyStickInnerRadius ? JoyPos.Inner : JoyPos.Outer;
        if (Mathf.Abs(_CallBackData.TouchedAxis.x) < 0.001f)
            _CallBackData.Dir = XMoveDirection.None;
        else
            _CallBackData.Dir = _CallBackData.TouchedAxis.x > 0 ? XMoveDirection.Right : XMoveDirection.Left;

        this.UpdateDegree();
    }


}
