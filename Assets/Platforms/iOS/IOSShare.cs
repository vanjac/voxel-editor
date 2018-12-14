#if UNITY_IOS

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class IOSShare
{
    [DllImport("__Internal")] private static extern void showSocialSharing(string filePath);

    public static void Share(string filePath)
    {
        showSocialSharing(filePath);
    }
}

#endif