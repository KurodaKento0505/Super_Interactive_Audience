using UnityEngine;
using System;
using System.Runtime.InteropServices;

static public class WindowTopmost
{

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern int FindWindow(String className, String windowName);
    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern int SetWindowPos(int hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
    private static extern int GetDesktopWindow();
    [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
    private static extern bool GetWindowRect(int hWnd, out RECT rect);


    static int handle; // ウィンドウ

    const String WINDOW_NAME = "LargeSpaceSampleContents";

    const int HWND_TOPMOST = -1; // 常に最前面に表示
    const int SWP_SHOWWINDOW = 0x0040; // ウィンドウを表示

    private static int displayWidth; // ディスプレイの幅
    private static int displayHeight; // ディスプレイの高さ

    public static void MakeTopmost()
    {

#if UNITY_EDITOR

#else

        // ディスプレイサイズを取得
        int display = GetDesktopWindow();
        RECT rect = new RECT();
        GetWindowRect(display, out rect);

        displayWidth = rect.right; // ディスプレイの幅を取得
        displayHeight = rect.bottom; // ディスプレイの高さを取得

        handle = FindWindow(null, WINDOW_NAME); // 起動したウィンドウを指定

        // SetWindowLongによる変更を適用
        SetWindowPos(handle, HWND_TOPMOST, 0, 0, displayWidth, displayHeight, SWP_SHOWWINDOW);


#endif

    }

}
