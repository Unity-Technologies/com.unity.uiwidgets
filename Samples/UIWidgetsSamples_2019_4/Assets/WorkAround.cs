using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UIWidgetsSample;
using Unity.UIWidgets.ui;
using UnityEngine;

public class WorkAround : MonoBehaviour
{
    
    [DllImport(NativeBindings.dllName)]
    static extern  System.IntPtr GetUnityContextEventFunc();

    public CountDemo demo;
    
    // Start is called before the first frame update
    void Start()
    {
        GL.IssuePluginEvent(GetUnityContextEventFunc(), 1);
        // demo = GetComponent<CountDemo>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!demo.enabled)
        {
            demo.enabled = true;
            this.enabled = false;
        }
    }
}
