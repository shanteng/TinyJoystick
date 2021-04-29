using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CameraFollow : MonoBehaviour
{
    [Title("摄像机平滑移动的时间", "#FF4F63")]
    public float mSmoothTime = 0.01f; //

    [Title("相机Z轴固定坐标", "#FF4F63")]
    public float mConstZ = -20;

    [Title("相机Y轴固定位移", "#FF4F63")]
    public float mOffsetY = 6;

    [Title("距离为0时的Size", "#FF4F63")]
    public float minOrthographicSize = 4;

    private Camera mCamera;
    private Focus[] mFocusList;
    private Vector3 mCameraVelocity = Vector3.zero;
    private float mCameraVelocitySize = 0;
    private Vector3 mTargetPos;
    private Vector2 CurDistance = Vector2.zero;
    public float screenWHRate = 0;
    void Awake()
    {
        this.screenWHRate = (float)Screen.width / (float)Screen.height;
        this.mCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        this.FindFocusObjects();
    }

    float afterSize;
    private void FindFocusObjects()
    {
        mFocusList = GameObject.FindObjectsOfType<Focus>();
        mTargetPos = Vector3.zero;
        this.CurDistance = Vector2.zero;
        if (mFocusList != null && mFocusList.Length > 0)
        {
            Vector3 targetPos = Vector3.zero;
            int len = this.mFocusList.Length;
            for (int i = 0; i < len; ++i)
            {
                targetPos += this.mFocusList[i].transform.position;//  child.Bounds.center;
            }
            targetPos /= len;
            mTargetPos = targetPos;
            float magnitude = 0;
            Transform minPosiTran = null;
            Transform maxPosiTran = null;
            for (int i = 0; i < len; ++i)
            {
                Vector3 posi = this.mFocusList[i].transform.position;
                if (minPosiTran == null)
                {
                    minPosiTran = this.mFocusList[i].transform;
                    maxPosiTran =  this.mFocusList[i].transform;
                }
                for (int j = i + 1; j < len; ++j)
                {
                    Vector3 posj = this.mFocusList[j].transform.position;
                    float distance = (posj - posi).sqrMagnitude;
                    if (distance > magnitude)
                    {
                        minPosiTran = this.mFocusList[i].transform;
                        maxPosiTran = this.mFocusList[j].transform;
                        magnitude = distance;
                        this.CurDistance.x = Mathf.Abs(posj.x - posi.x);
                        this.CurDistance.y = Mathf.Abs(posj.y - posi.y)+this.mOffsetY;
                    }
                }
            }
        }

        float cameraHeight = this.mCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * this.mCamera.aspect;


        float afterSizeX = this.CurDistance.x / this.mCamera.aspect * 0.5f;
        float afterSizeY = this.CurDistance.y * 0.5f;
        afterSize = afterSizeX > afterSizeY ? afterSizeX : afterSizeY;

        afterSize += 1f;
        if (afterSize < this.minOrthographicSize)
            afterSize = this.minOrthographicSize;

        mTargetPos.z = this.mConstZ;
        mTargetPos.y += mOffsetY;
    }

    private void Update()
    {
        this.FindFocusObjects();
    }

    private void LateUpdate()
    {
        this.FocusCenter();
    }

    public  void FocusCenter()
    {
        this.mCamera.orthographicSize = afterSize;
        this.transform.position = mTargetPos;// Vector3.SmoothDamp(transform.position, mTargetPos, ref this.mCameraVelocity, this.mSmoothTime);
    }

    void GetCorners( )
    {
        Vector3[] corners = new Vector3[4];

        float halfFOV = (this.mCamera.orthographicSize * 0.5f) * Mathf.Deg2Rad;
        float aspect = this.mCamera.aspect;

        float height = Mathf.Tan(halfFOV);
        float width = height * aspect;

        Debug.Log("height:" + height);
        Debug.Log("width:" + width);
    }
}//end class



