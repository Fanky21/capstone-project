#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Auto-enable cleartext traffic support for localhost development
/// Script ini otomatis menambahkan define symbol untuk allow HTTP traffic
/// </summary>
[InitializeOnLoad]
public class WebViewDefines
{
    static WebViewDefines()
    {
        var buildTargetGroup = BuildTargetGroup.Android;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        
        // Enable cleartext traffic untuk localhost development
        if (!defines.Contains("UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC"))
        {
            if (!string.IsNullOrEmpty(defines) && !defines.EndsWith(";"))
            {
                defines += ";";
            }
            defines += "UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            UnityEngine.Debug.Log("[WebView] Added UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC define symbol for HTTP localhost support");
        }
    }
}
#endif
