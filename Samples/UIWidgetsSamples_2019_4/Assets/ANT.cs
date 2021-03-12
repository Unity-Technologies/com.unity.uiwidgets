using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = UnityEngine.Color;

public class ANT : MonoBehaviour
{
    [DllImport(NativeBindings.dllName)]
    private static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h);

    [DllImport(NativeBindings.dllName)]
    private static extern void draw_xxx();

    // Start is called before the first frame update
    void Start()
    {
        CreateTextureAndPassToPlugin();
    }

    private void CreateTextureAndPassToPlugin()
    {
        // Create a texture
        Texture2D tex = new Texture2D(20, 20, TextureFormat.ARGB32, false);
        // Set point filtering just so we can see the pixels clearly
        tex.filterMode = FilterMode.Point;
        // Call Apply() so it's actually uploaded to the GPU
        tex.Apply();
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                tex.SetPixel(i, j, Color.magenta);
            }
        }

        tex.Apply();

        // Set texture onto our material
        GetComponent<Renderer>().material.mainTexture = tex;

#if !UNITY_EDITOR
        // Pass texture pointer to the plugin
        SetTextureFromUnity (tex.GetNativeTexturePtr(), tex.width, tex.height);
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if !UNITY_EDITOR
        var tex = (Texture2D)GetComponent<Renderer>().material.mainTexture;
        Debug.Log("update");

        draw_xxx();
        Debug.Log("22");

        tex.Apply();
        Debug.Log("apply");

#endif
    }
}