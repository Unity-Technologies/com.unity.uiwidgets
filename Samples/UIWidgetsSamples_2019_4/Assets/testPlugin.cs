using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using  Unity.UIWidgets.ui2;
using UnityEditor.Graphs;
using Color = Unity.UIWidgets.ui2.Color;

public class testPlugin : MonoBehaviour
{
    [DllImport("libUIWidgets_d")]
    static extern IntPtr CreateTexture(int w, int h);
    
    [DllImport("libUIWidgets_d")]
    static extern void DrawParagraph(IntPtr paragraph);

    void testParagraph()
    {
        var renderer = GetComponent<Renderer>();
        var c = CreateTexture(1024, 1024);
        renderer.material.mainTexture =
            Texture2D.CreateExternalTexture(1024, 1024, TextureFormat.RGBA32, true, true, c);
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
        var ts2 = new TextStyle(
            color: new Color(0xFF00FF00), 
            decoration: TextDecoration.underline,
            decorationStyle: TextDecorationStyle.dashed,
            fontFamily: "Arial", 
            fontSize:40, 
            height:1.5,
            background: new Paint()
            {
                color = new Color(0xAAFF7F00),
                
            }
            ,
            foreground:new Paint()
            {
                color = new Color(0xAAFFFF00)
            }
        );
        pb.pushStyle(ts2);
        pb.addText("test push one more style");
        pb.pop();
        pb.addText("test pop style");
        var p = pb.build();
        p.layout(new ParagraphConstraints(600));
        Debug.Log($"paragraph width: {p.width()}");
        Debug.Log($"paragraph height: {p.height()}");
        Debug.Log($"paragraph longestLine: {p.longestLine()}");
        Debug.Log($"paragraph minIntrinsicWidth: {p.minIntrinsicWidth()}");
        Debug.Log($"paragraph maxIntrinsicWidth: {p.maxIntrinsicWidth()}");
        Debug.Log($"paragraph alphabeticBaseline: {p.alphabeticBaseline()}");
        Debug.Log($"paragraph ideographicBaseline: {p.ideographicBaseline()}");
        Debug.Log($"paragraph didExceedMaxLines: {p.didExceedMaxLines()}");
        var boxes = p.getBoxesForRange(0, 20);
        foreach (var box in boxes)
        {
            Debug.Log(box.ToString());
        }
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
