public struct BiliBiliLiveInteractData
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string username;

    /// <summary>
    /// 用户ID
    /// </summary>
    public int userId;

    /// <summary>
    ///  观众互动内容
    /// </summary>
    public InteractTypeEnum interactType;
    public int guardLevel;
}
/// <summary>
/// 观众互动内容
/// </summary>
public enum InteractTypeEnum
{
    /// <summary>
    /// 进入
    /// </summary>
    Enter = 1,

    /// <summary>
    /// 关注
    /// </summary>
    Follow = 2,

    /// <summary>
    /// 分享直播间
    /// </summary>
    Share = 3,

    /// <summary>
    /// 特别关注
    /// </summary>
    SpecialFollow = 4,

    /// <summary>
    /// 互相关注
    /// </summary>
    MutualFollow = 5,

}