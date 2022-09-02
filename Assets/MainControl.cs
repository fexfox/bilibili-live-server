using CreativeVeinStudio.Simple_Pool_Manager;
using CreativeVeinStudio.Simple_Pool_Manager.Interfaces;
using Liluo.BiliBiliLive;
using System;
using TMPro;
using UnityEngine;

public class MainControl : MonoBehaviour
{
    public Transform DanmuContainer;
    public Transform SCContainer;
    public TMP_Text RoomViewer;
    public TMP_Text WatchCount;
    public TMP_Text RoomState;
    public TMP_Text Gift;
    public TMP_Text Interact;
    public TMP_InputField RoomID;
    public bool showAvatar;
    private IPoolActions _spManager;
    private void Awake()
    {
        _spManager = FindObjectOfType<SpManager>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }
    IBiliBiliLiveRequest req;

    public async void Init()
    {
        // 创建一个直播间监听对象
        req = await BiliBiliLive.Connect(int.Parse(RoomID.text));
        req.OnDanmuCallBack += Req_OnDanmuCallBack;
        req.OnWatchCountChange += Req_OnWatchCountChange;
        req.OnUserCountChange += Req_OnUserCountChange;
        req.OnLiveStateChange += Req_OnLiveStateChange;
        req.OnInteractCallBack += Req_OnInteractCallBack;
        req.OnGiftCallBack += Req_OnGiftCallBack;
        req.OnGuardCallBack += Req_OnGuardCallBack;
        req.OnSuperChatCallBack += Req_OnSuperChatCallBack;
        req.OnWelcomeCallBack += Req_OnWelcomeCallBack;
    }

    private void Req_OnWelcomeCallBack(BiliBiliLiveWelcomeData obj)
    {
        throw new System.NotImplementedException();
    }

    private void Req_OnSuperChatCallBack(BiliBiliLiveSuperChatData obj)
    {
        Debug.Log("sc");
        var sc = _spManager.GetRandomPoolItem("SC", false);
        if (!sc) return;
        sc.transform.SetParent(SCContainer);
        var data = sc.GetComponent<SCManager>();
        data.setSC(obj.username, obj.price.ToString(), obj.keepTime, obj.content);
        sc.SetActive(true);
    }

    private void Req_OnGuardCallBack(BiliBiliLiveGuardData obj)
    {
        switch (obj.guardLevel)
        {
            case 1:
                Gift.text += $"{Environment.NewLine}恭喜<color=#FFB3B3>{obj.username}</color>成功上船！出任<color=#FF425E>{obj.guardName}</color>";
                break;
            case 2:
                Gift.text += $"{Environment.NewLine}恭喜<color=#FFB3B3>{obj.username}</color>成功上船！出任<color=#D397E9>{obj.guardName}</color>";
                break;
            case 3:
                Gift.text += $"{Environment.NewLine}恭喜<color=#FFB3B3>{obj.username}</color>成功上船！出任<color=#61B1FF>{obj.guardName}</color>";
                break;
            default:

                break;
        }


    }

    private void Req_OnGiftCallBack(BiliBiliLiveGiftData obj)
    {
        Gift.text += $"{Environment.NewLine}感谢<color=#FFB3B3>{obj.username}</color>送出的{obj.num}个<color=#FF5800>{obj.giftName}</color>";
    }

    private void Req_OnInteractCallBack(BiliBiliLiveInteractData obj)
    {
        switch (obj.interactType)
        {
            case InteractTypeEnum.Enter:
                var guardName = obj.guardLevel == 3 ? " <color=#61B1FF>舰长</color>" : obj.guardLevel == 2 ? " <color=#D397E9>提督</color>" : obj.guardLevel == 1 ? " <color=#FF425E>总督</color>" : "";
                Interact.text += $"{Environment.NewLine}欢迎{guardName} <color=#FFB3B3>{obj.username}</color>进入直播间";
                Debug.Log("enter");
                break;
            case InteractTypeEnum.Follow:
                Interact.text += $"{Environment.NewLine}感谢<color=#FFB3B3>{obj.username}</color>的关注";
                break;
            case InteractTypeEnum.Share:
                Interact.text += $"{Environment.NewLine}感谢<color=#FFB3B3>{obj.username}</color>的分享";
                break;
            case InteractTypeEnum.SpecialFollow:
                Interact.text += $"{Environment.NewLine}感谢<color=#FFB3B3>{obj.username}</color>的<color=#FF5800>特别关注</color>";
                break;
            case InteractTypeEnum.MutualFollow:
                Interact.text += $"{Environment.NewLine}已经和<color=#FFB3B3>{obj.username}</color><color=#FF5800>互相关注</color>啦！";
                break;
            default:
                break;
        }
    }

    private void Req_OnLiveStateChange(int obj)
    {
        switch (obj)
        {
            case -1:
                RoomState.text = "未开播";
                RoomState.color = Color.red;
                break;
            case 0:
                RoomState.text = "准备中";
                RoomState.color = Color.yellow;
                break;
            case 1:
                RoomState.text = "直播中";
                RoomState.color = Color.green;
                break;
        }
    }

    private void Req_OnUserCountChange(int obj)
    {
        RoomViewer.text = obj.ToString();
    }

    private void Req_OnWatchCountChange(int obj)
    {
        WatchCount.text = obj.ToString();
    }

    private async void Req_OnDanmuCallBack(BiliBiliLiveDanmuData obj)
    {

        var danmu = _spManager.GetRandomPoolItem("Danmu", false);
        if (!danmu) return;
        danmu.transform.SetParent(DanmuContainer);
        var data = danmu.GetComponent<DanmuManager>();
        if (showAvatar)
            data.avatar.sprite = await BiliBiliLive.GetHeadSprite(obj.userId);
        else
            data.HideAvatar();
        var level = obj.medalLevel;
        if (level > 0)
            data.level.text = level.ToString();
        data.username.text = obj.username;
        data.content.text = obj.content;
        data.setGuardLevel(obj.guardLevel);
        danmu.SetActive(true);
        //Debug.Log($"{ obj.userId},{obj.username},{obj.vip},{obj.guardLevel},{obj.content}");
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnDestroy()
    {
        Disconnect();
    }
    public void Disconnect()
    {
        // 释放监听对象
        req?.DisConnect();
        req = null;
        RoomViewer.text = "未知";
        WatchCount.text = "未知";
        RoomState.text = "未知";
        RoomState.color = Color.white;
    }
}
