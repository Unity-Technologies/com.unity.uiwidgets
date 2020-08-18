using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using  Unity.UIWidgets.ui2;
using Color = Unity.UIWidgets.ui2.Color;

public class testPlugin : MonoBehaviour
{
    [DllImport("libUIWidgets_d")]
    static extern IntPtr CreateTexture(int w, int h);
    [DllImport("libUIWidgets_d")]
    static extern void Draw123();
    
    [DllImport("libUIWidgets_d")]
    static extern void DrawParagraph(IntPtr paragraph);

    void testParagraph()
    {
        var renderer = GetComponent<Renderer>();
        var c = CreateTexture(1024, 1024);
        renderer.material.mainTexture =
            Texture2D.CreateExternalTexture(1024, 1024, TextureFormat.RGBA32, true, true, c);
        Draw123();
        var style = new ParagraphStyle(fontFamily: "Arial", height: 4);

        var pb = new ParagraphBuilder(style);
        var ts = new TextStyle(
            color: new Color(0xFFFF00F0), 
            decoration: TextDecoration.lineThrough,
            decorationStyle: TextDecorationStyle.doubleLine,
            fontFamily: "Arial", 
            fontSize:30, 
            height:1.5
        );
        pb.pushStyle(ts);
        pb.addText("just for test\n 中文测试 分段测试长长长长长长长长长长长长长长长长长长长长长长长长66666666");
        var p = pb.build();
        p.layout(new ParagraphConstraints(500));
        DrawParagraph(p.ptr);
    }
    // Start is called before the first frame update
    void Start()
    {
        testParagraph();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
