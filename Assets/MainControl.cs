using CreativeVeinStudio.Simple_Pool_Manager;
using CreativeVeinStudio.Simple_Pool_Manager.Interfaces;
using Liluo.BiliBiliLive;
using TMPro;
using UnityEngine;

public class MainControl : MonoBehaviour
{
    public GameObject DanmuPrefab;
    public Transform Container;
    public TMP_Text RoomViewer;
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
        req.OnRoomViewer += Req_OnRoomViewer;
    }

    private void Req_OnRoomViewer(int obj)
    {
        RoomViewer.text = obj.ToString();
    }

    private async void Req_OnDanmuCallBack(BiliBiliLiveDanmuData obj)
    {
            
        var danmu = _spManager.GetRandomPoolItem("Danmu",false);
        if (!danmu) return;
        danmu.transform.SetParent(Container);
        var data = danmu.GetComponent<DanmuManager>();
        if (showAvatar)
        { data.avatar.sprite = await BiliBiliLive.GetHeadSprite(obj.userId); }
        else
        {
            data.HideAvatar();
        }
        data.username.text = obj.username;
        data.content.text = obj.content;
        data.setGuardLevel(obj.guardLevel);
        danmu.SetActive(true);
        Debug.Log($"{ obj.userId},{obj.username},{obj.vip},{obj.guardLevel},{obj.content}");
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
    }
}
