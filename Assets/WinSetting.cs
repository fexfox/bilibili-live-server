using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.IO;

/// <summary>
/// һ����ѡ��������ʽ
/// </summary>
public enum enumWinStyle
{
    /// <summary>
    /// �ö�
    /// </summary>
    WinTop,
    /// <summary>
    /// �ö�����͸��
    /// </summary>
    WinTopApha,
    /// <summary>
    /// �ö�͸�����ҿ��Դ�͸
    /// </summary>
    WinTopAphaPenetrate
}
public class WinSetting : MonoBehaviour
{

    #region Win��������
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

    [DllImport("user32.dll")]
    static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, int bAlpha, int dwFlags);

    [DllImport("Dwmapi.dll")]
    static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    private const int WS_POPUP = 0x800000;
    private const int GWL_EXSTYLE = -20;
    private const int GWL_STYLE = -16;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_BORDER = 0x00800000;
    private const int WS_CAPTION = 0x00C00000;
    private const int SWP_SHOWWINDOW = 0x0040;
    private const int LWA_COLORKEY = 0x00000001;
    private const int LWA_ALPHA = 0x00000002;
    private const int WS_EX_TRANSPARENT = 0x20;
    //
    private const int ULW_COLORKEY = 0x00000001;
    private const int ULW_ALPHA = 0x00000002;
    private const int ULW_OPAQUE = 0x00000004;
    private const int ULW_EX_NORESIZE = 0x00000008;
    #endregion
    //
    public string strProduct;//��Ŀ����
    public enumWinStyle WinStyle = enumWinStyle.WinTop;//������ʽ
    //
    public int ResWidth;//���ڿ��
    public int ResHeight;//���ڸ߶�
    //
    public int currentX;//�������Ͻ�����x
    public int currentY;//�������Ͻ�����y
    //
    private bool isApha;//�Ƿ�͸��
    private bool isAphaPenetrate;//�Ƿ�Ҫ��͸����
    // Use this for initialization
    void Awake()
    {
        Screen.fullScreen = false;
#if UNITY_EDITOR
        print("�༭ģʽ�����Ĵ���");
#else
        switch (WinStyle)
        {
            case enumWinStyle.WinTop:
                isApha = false;
                isAphaPenetrate = false;
                break;
            case enumWinStyle.WinTopApha:
                isApha = true;
                isAphaPenetrate = false;
                break;
            case enumWinStyle.WinTopAphaPenetrate:
                isApha = true;
                isAphaPenetrate = true;
                break;
        }
        //
        IntPtr hwnd = FindWindow(null, strProduct);
        //
        if (isApha)
        {
            //ȥ�߿���͸��
            SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED);
            int intExTemp = GetWindowLong(hwnd, GWL_EXSTYLE);
            if (isAphaPenetrate)//�Ƿ�͸����͸����
            {
                SetWindowLong(hwnd, GWL_EXSTYLE, intExTemp | WS_EX_TRANSPARENT | WS_EX_LAYERED);
            }
            //
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_BORDER & ~WS_CAPTION);
            SetWindowPos(hwnd, -1, currentX, currentY, ResWidth, ResHeight, SWP_SHOWWINDOW);
            var margins = new MARGINS() { cxLeftWidth = -1 };
            //
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
        else
        {
            //����ȥ�߿�
            SetWindowLong(hwnd, GWL_STYLE, WS_POPUP);
            SetWindowPos(hwnd, -1, currentX, currentY, ResWidth, ResHeight, SWP_SHOWWINDOW);
        }
#endif
    }
}