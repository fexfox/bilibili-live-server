using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitConverter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Liluo.BiliBiliLive
{
    public class BiliBiliLiveRequest : IBiliBiliLiveRequest
    {
        /// <summary>
        /// 弹幕聊天的服务器地址
        /// </summary>
        string chatHost = "broadcastlv.chat.bilibili.com";
        /// <summary>
        /// Tcp 客户端 socket
        /// </summary>
        TcpClient client;
        /// <summary>
        /// 流
        /// </summary>
        Stream netStream;
        /// <summary>
        /// 判断是否连接
        /// </summary>
        bool connected = false;

        /// <summary>
        /// 默认主机的这两个服务器
        /// </summary>
        string[] defaultHosts = new string[] { "tx-gz-live-comet-02.chat.bilibili.com", "tx-bj-live-comet-02.chat.bilibili.com", "broadcastlv.chat.bilibili.com" };
        /// <summary>
        /// Http 对象
        /// </summary>
        HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
        /// <summary>
        /// 获取 房间ID 的地址
        /// </summary>
        string CIDInfoUrl = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo?id=";
        /// <summary>
        /// 协议转换
        /// </summary>
        short protocolversion = 1;
        /// <summary>
        /// 端口号
        /// </summary>
        int chatPort = 2243;
        int defaultPort = 2243;
        private static List<Tuple<string, int>> ChatHostList = new List<Tuple<string, int>>();
        public event Action<string> OnWarning;
        public event Action<int> OnUserCountChange;
        public event Action<int> OnWatchCountChange;
        public event Action<int> OnLiveStateChange;
        public event Action<BiliBiliLiveDanmuData> OnDanmuCallBack;
        public event Action<BiliBiliLiveGiftData> OnGiftCallBack;
        public event Action<BiliBiliLiveGuardData> OnGuardCallBack;
        public event Action<BiliBiliLiveSuperChatData> OnSuperChatCallBack;
        public event Action<BiliBiliLiveWelcomeData> OnWelcomeCallBack;
        public event Action<BiliBiliLiveInteractData> OnInteractCallBack;

        private string lastserver;
        private static int lastroomid;
        private static string token = "";
        private CancellationTokenSource cancellationTokenSource;
        /// <summary>
        /// 申请异步连接  需要输入对应房间号
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task<bool> Connect(int roomID)
        {
            try { 
            if (connected)
            {
                UnityEngine.Debug.LogError("连接已存在");
                return true;
            }
            if (roomID != lastroomid || ChatHostList.Count == 0)
            {
                //token 令牌
                try
                {
                    //发起Http请求
                    var req = await httpClient.GetStringAsync(CIDInfoUrl + roomID);
                    JObject roomobj = JObject.Parse(req);
                    token = roomobj["data"]["token"] + "";
                    var serverlist = roomobj["data"]["host_list"].Value<JArray>();
                    ChatHostList = new List<Tuple<string, int>>();
                    foreach (var serverinfo in serverlist)
                    {
                        ChatHostList.Add(new Tuple<string, int>(serverinfo["host"] + "", serverinfo["port"].Value<int>()));
                    }

                    var server = ChatHostList[new Random().Next(ChatHostList.Count)];
                    chatHost = server.Item1;

                    chatPort = server.Item2;
                    if (string.IsNullOrEmpty(chatHost)) throw new Exception();
                }
                catch (WebException ex)
                {
                    chatHost = defaultHosts[UnityEngine.Random.Range(0, defaultHosts.Length)];
                    chatPort = defaultPort;
                    var errorResponse = ex.Response as HttpWebResponse;
                    if (errorResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        // 直播间不存在（HTTP 404）
                        var msg = "该直播间疑似不存在，弹幕姬只支持使用原房间号连接";
                        UnityEngine.Debug.LogError(msg);
                    }
                    else
                    {
                        // B站服务器响应错误
                        var msg = "B站服务器响应弹幕服务器地址出错，尝试使用常见地址连接";
                        UnityEngine.Debug.LogError(msg);
                    }
                    // UnityEngine.Debug.LogError($"获取弹幕服务器地址时出现错误，尝试使用默认服务器... 错误信息: {e}");
                }
                catch (Exception)
                {
                    // 其他错误（XML解析错误？）
                    chatHost = defaultHosts[new Random().Next(defaultHosts.Length)];
                    chatPort = defaultPort;
                    var msg = "获取弹幕服务器地址时出现未知错误，尝试使用常见地址连接";
                    UnityEngine.Debug.LogError(msg);
                }
            }
            else
            {
                var server = ChatHostList[new Random().Next(ChatHostList.Count)];
                chatHost = server.Item1;

                chatPort = server.Item2;
            }
            // 创建 TCP对象
            client = new TcpClient();
            // DNS解析域名 服务器IP地址
            var ipAddress = await System.Net.Dns.GetHostAddressesAsync(chatHost);
            // 随机选择一个进行连接
            await client.ConnectAsync(ipAddress[UnityEngine.Random.Range(0, ipAddress.Length)], chatPort);
            netStream = Stream.Synchronized(client.GetStream());
            cancellationTokenSource = new CancellationTokenSource();
            UnityEngine.Debug.Log("发送验证消息");
            if (await SendJoinChannel(roomID, token, cancellationTokenSource.Token))
            {
                UnityEngine.Debug.Log("成功");
                connected = true;
                // 发送心跳包
                _ = ReceiveMessageLoop(cancellationTokenSource.Token);
                lastserver = chatHost;
                lastroomid = roomID;
                // 接收消息

                return true;
            }
            UnityEngine.Debug.Log("失败");
            return false;
        }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
                return false;
            }
        }
    public void DisConnect() => _disconnect();

        async Task ReceiveMessageLoop(CancellationToken ct)
        {
            Task heartbeatLoop = null;
            var stableBuffer = new byte[16];
            var buffer = new byte[4096];
            while (this.connected)
            {
                try
                {
                    await netStream.ReadBAsync(stableBuffer, 0, 16,ct);
                    var protocol = DanmakuProtocol.FromBuffer(stableBuffer);
                    if (protocol.PacketLength < 16)
                    {
                        UnityEngine.Debug.LogError("协议失败: (L:" + protocol.PacketLength + ")");
                        continue;
                    }
                    var payloadlength = protocol.PacketLength - 16;
                    if (payloadlength == 0) continue;
                    buffer = new byte[payloadlength];
                    //继续接受 协议总长度-协议头部 长度 的字节数据
                    await netStream.ReadBAsync(buffer, 0, payloadlength,ct);
                    if (heartbeatLoop == null)
                    {
                        heartbeatLoop = this.HeartbeatLoop(cancellationTokenSource.Token);
                    }
                    if (protocol.Version == 2 && protocol.Action == 5)
                    {
                        using (var ms = new MemoryStream(buffer, 2, payloadlength - 2))
                        using (var deflate = new DeflateStream(ms, CompressionMode.Decompress))
                        {
                            var headerbuffer = new byte[16];
                            try
                            {
                                while (true)
                                {
                                    await deflate.ReadBAsync(headerbuffer, 0, 16,ct);
                                    var protocol_in = DanmakuProtocol.FromBuffer(headerbuffer);
                                    payloadlength = protocol_in.PacketLength - 16;
                                    var danmakubuffer = new byte[payloadlength];
                                    await deflate.ReadBAsync(danmakubuffer, 0, payloadlength,ct);
                                    ProcessDanmaku(protocol.Action, danmakubuffer);
                                }
                            }
                            catch (Exception e)
                            {
                                UnityEngine.Debug.LogError($"读取弹幕消息失败。错误信息: {e}");
                            }
                        }
                    }
                    else if (protocol.Version == 3 && protocol.Action == 5) // brotli?
                    {
                        using (var ms = new MemoryStream(buffer)) // Skip 0x78 0xDA

                        using (var deflate = new Brotli.BrotliStream(ms, CompressionMode.Decompress))
                        {
                            var headerbuffer = new byte[16];
                            try
                            {
                                while (true)
                                {
                                    await deflate.ReadBAsync(headerbuffer, 0, 16,ct);
                                    var protocol_in = DanmakuProtocol.FromBuffer(headerbuffer);
                                    payloadlength = protocol_in.PacketLength - 16;
                                    var danmakubuffer = new byte[payloadlength];
                                    await deflate.ReadBAsync(danmakubuffer, 0, payloadlength,ct);
                                    ProcessDanmaku(protocol.Action, danmakubuffer);
                                }

                            }
                            catch (Exception e)
                            {
                                UnityEngine.Debug.LogError($"读取弹幕消息失败。错误信息: {e}");
                            }


                        }
                    }
                    else
                    {
                        ProcessDanmaku(protocol.Action, buffer);
                    }
                }
                catch (Exception e)
                {
                    if (e is System.ObjectDisposedException)
                    {
                        UnityEngine.Debug.LogWarning("连接已释放");
                        break;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"接受消息时发生错误。错误信息: {e}");
                    }
                    _disconnect();
                }
            }
        }

        void ProcessDanmaku(int action, byte[] buffer)
        {
            switch (action)
            {
                case 3:
                    {
                        // 观众人数
                        var viewer = EndianBitConverter.BigEndian.ToInt32(buffer, 0);
                        OnUserCountChange?.Invoke(viewer);
                        break;
                    }
                case 5:
                    {
                        // 弹幕
                        var json = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        // UnityEngine.Debug.Log(obj["cmd"].ToString());

                        //using (System.IO.StreamWriter file = new System.IO.StreamWriter("log.txt", true))
                        //{
                        //    file.Write(json);//直接追加文件末尾，不换行
                        //    file.WriteLine();//直接追加文件末尾，换行 
                        //}

                        var obj = JObject.Parse(json);
                        switch (obj["cmd"].ToString())
                        {
                            case "WARNING":
                                {
                                    var commentText = obj["msg"]?.ToString();
                                    OnWarning?.Invoke(commentText);
                                    break;
                                }
                            case "LIVE":
                                OnLiveStateChange?.Invoke(1);
                                break;
                            case "PREPARING":
                                OnLiveStateChange?.Invoke(0);
                                break;
                            case "CUT_OFF":
                                {
                                    OnLiveStateChange?.Invoke(-1);
                                    break;
                                }
                            case "GIFT_TOP":
                                {
                                    //MsgType = MsgTypeEnum.GiftTop;
                                    //var alltop = obj["data"].ToList();
                                    //GiftRanking = new List<GiftRank>();
                                    //foreach (var v in alltop)
                                    //{
                                    //    GiftRanking.Add(new GiftRank()
                                    //    {
                                    //        uid = v.Value<int>("uid"),
                                    //        UserName = v.Value<string>("uname"),
                                    //        coin = v.Value<decimal>("coin")

                                    //    });
                                    //}

                                    break;
                                }
                            case "ENTRY_EFFECT":
                                {
                                    //var msg = obj["data"]["copy_writing"] + "";
                                    //var match = EntryEffRegex.Match(msg);
                                    //if (match.Success)
                                    //{
                                    //    MsgType = MsgTypeEnum.WelcomeGuard;
                                    //    UserName = match.Groups[1].Value;
                                    //    UserID = obj["data"]["uid"].ToObject<int>();
                                    //    UserGuardLevel = obj["data"]["privilege_type"].ToObject<int>();
                                    //}
                                    //else
                                    //{
                                    //    MsgType = MsgTypeEnum.Unknown;
                                    //}

                                    break;
                                }
                            case "INTERACT_WORD":
                                {
                                    BiliBiliLiveInteractData interactData = new BiliBiliLiveInteractData();
                                    interactData.username = obj["data"]["uname"].ToString();
                                    interactData.userId = obj["data"]["uid"].ToObject<int>();
                                    interactData.interactType = (InteractTypeEnum)obj["data"]["msg_type"].ToObject<int>();
                                    OnInteractCallBack?.Invoke(interactData);
                                    break;
                                }
                            case "DANMU_MSG":
                                {
                                    BiliBiliLiveDanmuData danmuData = new BiliBiliLiveDanmuData();
                                    danmuData.username = obj["info"][2][1].ToString();
                                    danmuData.content = obj["info"][1].ToString();
                                    danmuData.userId = obj["info"][2][0].Value<int>();
                                    danmuData.isAdmin = obj["info"][2][2].ToString() == "1";
                                    danmuData.vip = obj["info"][2][3].ToString() == "1";
                                    danmuData.guardLevel = obj["info"][7].ToObject<int>();
                                    OnDanmuCallBack?.Invoke(danmuData);
                                    break;
                                }
                            // 礼物
                            case "SEND_GIFT":
                                {

                                    BiliBiliLiveGiftData giftData = new BiliBiliLiveGiftData();
                                    giftData.username = obj["data"]["uname"].ToString();
                                    giftData.userId = obj["data"]["uid"].Value<int>();
                                    giftData.giftName = obj["data"]["giftName"].ToString();
                                    giftData.giftId = obj["data"]["giftId"].Value<int>();
                                    giftData.num = obj["data"]["num"].Value<int>();
                                    giftData.price = obj["data"]["price"].Value<int>();//TODO
                                    giftData.total_coin = obj["data"]["total_coin"].Value<int>();//TODO
                                    OnGiftCallBack?.Invoke(giftData);
                                    break;
                                }
                            // 上舰
                            case "GUARD_BUY":
                                {
                                    BiliBiliLiveGuardData guardData = new BiliBiliLiveGuardData();
                                    guardData.username = obj["data"]["username"].ToString();
                                    guardData.userId = obj["data"]["uid"].ToObject<int>();
                                    guardData.guardLevel = obj["data"]["guard_level"].ToObject<int>();
                                    guardData.guardName = guardData.guardLevel == 3 ? "舰长" :
                                        guardData.guardLevel == 2 ? "提督" :
                                        guardData.guardLevel == 1 ? "总督" : "";
                                    guardData.guardCount = obj["data"]["num"].ToObject<int>();
                                    OnGuardCallBack?.Invoke(guardData);
                                    break;
                                }
                            // SC
                            case "SUPER_CHAT_MESSAGE":
                            case "SUPER_CHAT_MESSAGE_JP":
                                {
                                    BiliBiliLiveSuperChatData superChatData = new BiliBiliLiveSuperChatData();
                                    superChatData.username = obj["data"]["user_info"]["uname"].ToString();
                                    superChatData.userId = obj["data"]["uid"].ToObject<int>();
                                    superChatData.content = obj["data"]["message"]?.ToString();
                                    superChatData.price = obj["data"]["price"].ToObject<decimal>();
                                    superChatData.keepTime = obj["data"]["time"].ToObject<int>();
                                    OnSuperChatCallBack?.Invoke(superChatData);
                                    break;
                                }
                            case "WELCOME":
                                {
                                    BiliBiliLiveWelcomeData welcomeData = new BiliBiliLiveWelcomeData();
                                    welcomeData.username = obj["data"]["uname"].ToString();
                                    welcomeData.userId = obj["data"]["uid"].ToObject<int>();
                                    welcomeData.isVIP = true;
                                    welcomeData.isAdmin = obj["data"]["isadmin"]?.ToString() == "1";
                                    welcomeData.guardLevel = 0;
                                    OnWelcomeCallBack?.Invoke(welcomeData);
                                    break;
                                }
                            case "WELCOME_GUARD":
                                {
                                    BiliBiliLiveWelcomeData welcomeData = new BiliBiliLiveWelcomeData();
                                    welcomeData.username = obj["data"]["username"].ToString();
                                    welcomeData.userId = obj["data"]["uid"].ToObject<int>();
                                    welcomeData.guardLevel = obj["data"]["guard_level"].ToObject<int>();
                                    OnWelcomeCallBack?.Invoke(welcomeData);
                                    break;
                                }
                            case "WATCHED_CHANGE":
                                {

                                    var count = obj["data"]["num"].ToObject<int>();
                                    OnWatchCountChange?.Invoke(count);
                                    break;
                                }
                            default:
                                if (obj["cmd"].ToString().StartsWith("DANMU_MSG"))
                                {
                                    BiliBiliLiveDanmuData danmuData = new BiliBiliLiveDanmuData();
                                    danmuData.username = obj["info"][2][1].ToString();
                                    danmuData.isAdmin = obj["info"][2][2].ToString() == "1";
                                    danmuData.content = obj["info"][1].ToString();
                                    danmuData.userId = obj["info"][2][0].Value<int>();
                                    danmuData.vip = obj["info"][2][3].ToString() == "1";
                                    danmuData.guardLevel = obj["info"][7].ToObject<int>();
                                    OnDanmuCallBack?.Invoke(danmuData);
                                    break;
                                }
                                break;
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        async Task HeartbeatLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (this.connected)
                {
                    //每30秒发送一次 心跳
                    await SendHeartbeatAsync(cancellationToken);
                    await Task.Delay(30000,cancellationToken);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"与服务器连接时发生错误。错误信息: {e}");
                _disconnect();
            }

           
        }
        private async Task SendHeartbeatAsync(CancellationToken ct)
        {
            await SendSocketDataAsync(2, "[object Object]", ct);
            UnityEngine.Debug.Log("Message Sent: Heartbeat");
        }
        void _disconnect()
        {
            if (connected)
            {
                UnityEngine.Debug.Log("断开连接");
                cancellationTokenSource.Cancel();
                connected = false;
                client.Close();
                netStream = null;
            }
        }

        Task SendSocketDataAsync(int action, string body, CancellationToken ct)
        {
            return SendSocketDataAsync(0, 16, protocolversion, action, 1, body,ct);
        }
        async Task SendSocketDataAsync(int packetlength, short magic, short ver, int action, int param, string body, CancellationToken ct)
        {
            var playload = Encoding.UTF8.GetBytes(body);
            if (packetlength == 0) packetlength = playload.Length + 16;
            var buffer = new byte[packetlength];
            using (var ms = new MemoryStream(buffer))
            {
                var b = EndianBitConverter.BigEndian.GetBytes(buffer.Length);

                await ms.WriteAsync(b, 0, 4);
                b = EndianBitConverter.BigEndian.GetBytes(magic);
                await ms.WriteAsync(b, 0, 2);
                b = EndianBitConverter.BigEndian.GetBytes(ver);
                await ms.WriteAsync(b, 0, 2);
                b = EndianBitConverter.BigEndian.GetBytes(action);
                await ms.WriteAsync(b, 0, 4);
                b = EndianBitConverter.BigEndian.GetBytes(param);
                await ms.WriteAsync(b, 0, 4);
                if (playload.Length > 0)
                {
                    await ms.WriteAsync(playload, 0, playload.Length);
                }
                await netStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        async Task<bool> SendJoinChannel(int channelId, string token, CancellationToken ct)
        {
            var packetModel = new { roomid = channelId, uid = 0, protover = 3, key = token, platform = "danmuji", type = 2 };
            var playload = JsonConvert.SerializeObject(packetModel);
            await SendSocketDataAsync(7, playload,ct);
            return true;
        }
    }

    internal struct DanmakuProtocol
    {
        /// <summary>
        /// 消息总长度 (协议头 + 数据长度)
        /// </summary>
        public int PacketLength;
        /// <summary>
        /// 消息头长度 (固定为16[sizeof(DanmakuProtocol)])
        /// </summary>
        public short HeaderLength;
        /// <summary>
        /// 消息版本号
        /// </summary>
        public short Version;
        /// <summary>
        /// 消息类型
        /// </summary>
        public int Action;
        /// <summary>
        /// 参数, 固定为1
        /// </summary>
        public int Parameter;

        internal static DanmakuProtocol FromBuffer(byte[] buffer)
        {
            if (buffer.Length < 16) { throw new ArgumentException(); }
            return new DanmakuProtocol()
            {
                PacketLength = EndianBitConverter.BigEndian.ToInt32(buffer, 0),
                HeaderLength = EndianBitConverter.BigEndian.ToInt16(buffer, 4),
                Version = EndianBitConverter.BigEndian.ToInt16(buffer, 6),
                Action = EndianBitConverter.BigEndian.ToInt32(buffer, 8),
                Parameter = EndianBitConverter.BigEndian.ToInt32(buffer, 12),
            };
        }
    }
}