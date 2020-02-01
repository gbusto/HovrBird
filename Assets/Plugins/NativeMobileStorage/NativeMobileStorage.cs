using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class NativeMobileStorage
{
    /*
     * Declaring external functions for the iOS native plugin
     */
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    extern static private string _getApplicationDirectory();
#endif


    /*
     * C# plugin functions
     */
    static public string GetApplicationDirectory()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return _getApplicationDirectory();
#elif UNITY_ANDROID && !UNITY_EDITOR
        return AndroidGetApplicationDirectory();
#endif

        return "/tmp/xyz.ashgames.hovrbird";
    }

    static private string AndroidGetApplicationDirectory()
    {
        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject plugin = new AndroidJavaObject("xyz.ashgames.nativemobilestorageplugin.NativeMobileStorage");
        object[] parameters = new object[1];
        parameters[0] = unityActivity;
        string directory = plugin.Call<string>("GetApplicationDirectory", parameters);
        return directory;
    }
}
