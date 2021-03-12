using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = UnityEngine.Color;

public class ANT : MonoBehaviour
{
#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport ("libUIWidgets_d")]
#endif
    private static extern void SetTextureFromUnity2(System.IntPtr texture, int w, int h);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport ("libUIWidgets_d")]
#endif
    private static extern void draw_xxx();

    // Start is called before the first frame update
    IEnumerator Start()
    {
        CreateTextureAndPassToPlugin();
        yield return StartCoroutine("CallPluginAtEndOfFrames");
    }

    private void CreateTextureAndPassToPlugin()
    {
        Texture2D tex = new Texture2D(256,256,TextureFormat.ARGB32,false);
        // Set point filtering just so we can see the pixels clearly
        tex.filterMode = FilterMode.Point;
        // Call Apply() so it's actually uploaded to the GPU
        tex.Apply();

        // Set texture onto our material
        GetComponent<Renderer>().material.mainTexture = tex;

#if !UNITY_EDITOR
        // Pass texture pointer to the plugin
        SetTextureFromUnity2 (tex.GetNativeTexturePtr(), tex.width, tex.height);
#endif
    }

    private IEnumerator CallPluginAtEndOfFrames()
    {
        while (true)
        {
            // Wait until all frame rendering is done
            yield return new WaitForEndOfFrame();
#if !UNITY_EDITOR
            draw_xxx();
#endif
        }
    }
}