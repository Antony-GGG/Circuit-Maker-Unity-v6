using System.Runtime.InteropServices;
using UnityEngine;

public class GrandAdManager : MonoBehaviour
{
    public static GrandAdManager instance;
    [SerializeField] bool showAds = true;
    public int adsAfter;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    [DllImport("__Internal")]
    private static extern void broadcastCustom(System.IntPtr message);
    public void ShowAd(string message)
    {
        if (showAds)
        {

            var utf8StrPtr = Marshal.StringToHGlobalAnsi(message);
            broadcastCustom(utf8StrPtr);
            Marshal.FreeHGlobal(utf8StrPtr);
        }
    }
}
