using System.Runtime.InteropServices;

public static class MediaController
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    const int KEYEVENTF_EXTENDEDKEY = 0x1;
    const int KEYEVENTF_KEYUP = 0x2;

    private static void Press(byte key)
    {
        keybd_event(key, 0, KEYEVENTF_EXTENDEDKEY, 0);
        keybd_event(key, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
    }

    public static void PlayPause() => Press(0xB3);
    public static void Next() => Press(0xB0);
    public static void Previous() => Press(0xB1);
}