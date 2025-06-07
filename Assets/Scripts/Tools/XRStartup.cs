using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class XRStartup : MonoBehaviour
{
    IEnumerator Start()
    {
        XRGeneralSettings.Instance.Manager.InitializeLoaderSync();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null) {
            Debug.LogError("XR Loader 初始化失败！");
            yield break;
        }

        XRGeneralSettings.Instance.Manager.StartSubsystems();
        Debug.Log("XR 初始化成功！");
    }
}
