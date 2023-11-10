#if UNITY_IOS

using System.Runtime.InteropServices;

public static class IOSShare
{
    [DllImport("__Internal")] private static extern void showSocialSharing(string filePath);

    public static void Share(string filePath)
    {
        showSocialSharing(filePath);
    }
}

#endif