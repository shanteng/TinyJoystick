
using System.Collections.Generic;
using UnityEngine;


public class JoyButtonUi : MonoBehaviour
{
    public List<UIJoyButton> mBtnList;

    public static JoyButtonUi Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void AttachToUiHandler(LocalPlayerMotionHandler uiHandle)
    {
        foreach (UIJoyButton btn in this.mBtnList)
        {
            btn.AttachToUiHandler(uiHandle);
        }
    }

    void Start()
    {
        //测试入口
        LocalPlayerMotionHandler playerHandler = GameObject.FindGameObjectWithTag("Player").GetComponent<LocalPlayerMotionHandler>();
        this.AttachToUiHandler(playerHandler);
        this.AttachToUiHandler(playerHandler);
    }
}


