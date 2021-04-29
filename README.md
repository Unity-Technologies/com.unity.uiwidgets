# UIWidgets 2.0 (preview)
[中文](README-ZH.md)


## Introduction

UIWidgets is a plugin package for Unity Editor which helps developers to create, debug and deploy efficient,
cross-platform Apps using the Unity Engine.

UIWidgets is mainly derived from [Flutter](https://github.com/flutter/flutter). However, taking advantage of
the powerful Unity Engine, it offers developers many new features to improve their Apps
as well as the develop workflow significantly.

As the latest version, UIWidgets 2.0 aims to optmize the overall performance of the package. Specifically, a performance gain around 10% is observed on  mobile devices like iPhone 6 after upgrading to UIWidgets 2.0. However, if you still want to use the original UIWidgets 1.0, please download the archived packages from Releases or switch your working branch to uiwidgets_1.0.

#### Efficiency
Using the latest Unity rendering SDKs, a UIWidgets App can run very fast and keep >60fps in most times.


#### Cross-Platform
A UIWidgets App can be deployed on all kinds of platforms including PCs and mobile devices directly, like
any other Unity projects.

#### Multimedia Support
Except for basic 2D UIs, developers are also able to include 3D Models, audios, particle-systems to their UIWidgets Apps.


#### Developer-Friendly
A UIWidgets App can be debug in the Unity Editor directly with many advanced tools like
CPU/GPU Profiling, FPS Profiling.


## Example

<div style="text-align: center"><table><tr>
<td style="text-align: center">
  <img src="https://connect-prd-cdn.unity.com/20190323/p/images/2a27606f-a2cc-4c9f-9e34-bb39ae64d06c_uiwidgets1.gif" width="200"/>
</td>
<td style="text-align: center">
  <img src="https://connect-prd-cdn.unity.com/20190323/p/images/097a7c53-19b3-4e0a-ad27-8ec02506905d_uiwidgets2.gif" width="200" />
</td>
<td style="text-align: center">
  <img src="https://connect-prd-cdn.unity.com/20190323/p/images/1f03c1d0-758c-4dde-b3a9-2f5f7216b7d9_uiwidgets3.gif" width="200"/>
</td>
<td style="text-align: center">
  <img src="https://connect-prd-cdn.unity.com/20190323/p/images/a8884fbd-9e7c-4bd7-af46-0947e01d01fd_uiwidgets4.gif" width="200"/>
</td>
</tr></table></div>

### Projects using UIWidgets

#### Unity Connect App
The Unity Connect App is created using UIWidgets and available for both Android (https://unity.cn/connectApp/download)
and iOS (Searching for "Unity Connect" in App Store). This project is open-sourced @https://github.com/UnityTech/ConnectAppCN.

#### Unity Chinese Doc
The official website of Unity Chinese Documentation (https://connect.unity.com/doc) is powered by UIWidgets and
open-sourced @https://github.com/UnityTech/DocCN.

## Requirements

#### Unity

Install **Unity 2019.4.26f1c1** and above. You can download the latest Unity on https://unity3d.com/get-unity/download.

#### UIWidgets Package
Visit our Github repository https://github.com/Unity-Technologies/com.unity.uiwidgets
 to download the latest UIWidgets package.

Move the downloaded package folder into the **Package** folder of your Unity project.

Generally, you can make it using a console (or terminal) application by just a few commands as below:

   ```none
    cd <YourPackagePath>
    git clone https://github.com/Unity-Technologies/com.unity.uiwidgets.git com.unity.uiwidgets
   ```

In PackageManger of unity, select add local file. select ```package.json``` under ```/com.unity.uiwidgets```

#### Runtime Environment

Currently UIWidgets only supports MacOS(Metal), iOS(Metal), Android(Armv7, OpenGLes) and Windows(Direct3D11). More devices will be supported in the future.

## Getting Start

#### i. Overview
In this tutorial, we will create a very simple UIWidgets App as the kick-starter. The app contains
only a text label and a button. The text label will count the times of clicks upon the button.

First of all, please open or create a Unity Project and open it with Unity Editor.

#### ii. Scene Build
A UIWidgets App is usually built upon a Unity UI Canvas. Please follow the steps to create a
UI Canvas in Unity.
1. Create a new Scene by "File -> New Scene";
1. Create a UI Canvas in the scene by "GameObject -> UI -> Canvas";
1. Add a Panel (i.e., **Panel 1**) to the UI Canvas by right click on the Canvas and select "UI -> Panel". Then remove the
**Image** Component from the Panel.

#### iii. Create Widget
A UIWidgets App is written in **C# Scripts**. Please follow the steps to create an App and play it
in Unity Editor.

1. Create a new C# Script named "UIWidgetsExample.cs" and paste the following codes into it.
   ```csharp
    using System.Collections.Generic;
    using uiwidgets;
    using Unity.UIWidgets.cupertino;
    using Unity.UIWidgets.engine;
    using Unity.UIWidgets.ui;
    using Unity.UIWidgets.widgets;
    using Text = Unity.UIWidgets.widgets.Text;
    using ui_ = Unity.UIWidgets.widgets.ui_;
    using TextStyle = Unity.UIWidgets.painting.TextStyle;

    namespace UIWidgetsSample
    {
        public class CountDemo : UIWidgetsPanel
        {
            protected void OnEnable()
            {
                // if you want to use your own font or font icons.
                    // AddFont("Material Icons", new List<string> {"MaterialIcons-Regular.ttf"}, new List<int> {0});
                base.OnEnable();
            }

            protected override void main()
            {
                ui_.runApp(new MyApp());
            }

            class MyApp : StatelessWidget
            {
                public override Widget build(BuildContext context)
                {
                    return new CupertinoApp(
                        home: new CounterApp()
                    );
                }
            }
        }

        internal class CounterApp : StatefulWidget
        {
            public override State createState()
            {
                return new CountDemoState();
            }
        }

        internal class CountDemoState : State<CounterApp>
        {
            private int count = 0;

            public override Widget build(BuildContext context)
            {
                return new Container(
                    color: Color.fromARGB(255, 255, 0, 0),
                    child: new Column(children: new List<Widget>()
                        {
                            new Text($"count: {count}", style: new TextStyle(color: Color.fromARGB(255, 0 ,0 ,255))),
                            new CupertinoButton(
                                onPressed: () =>
                                {
                                    setState(() =>
                                    {
                                        count++;
                                    });
                                },
                                child: new Container(
                                    color: Color.fromARGB(255,0 , 255, 0),
                                    width: 100,
                                    height: 40
                                )
                            ),
                        }
                    )
                );
            }
        }
    }
   ```

1. Save this script and attach it to **Panel 1** as its component.
1. Press the "Play" Button to start the App in Unity Editor.

#### iv. Build App
Finally, the UIWidgets App can be built to packages for any specific platform by the following steps.
1. Open the Build Settings Panel by "File -> Build Settings..."
1. Choose a target platform and click "Build". Then the Unity Editor will automatically assemble
all relevant resources and generate the final App package.

#### How to load images?
1. Put your images files in StreamingAssets folder. e.g. image1.png.
2. Use Image.file("image1.png") to load the image.

UIWidgets supports Gif as well!
1. Put your gif files in StreamingAssets folder. e.g. loading1.gif.
2. Use Image.file("loading1.gif") to load the gif images.

#### Show Status Bar on Android
Status bar is always hidden by default when an Unity project is running on an Android device.
If you
want to show the status bar in your App, you can disable```Start in fullscreen``` and ```record outside safe area```, make sure ```showStatusBar``` is ```true``` under ```UIWidgetsAndroidConfiguration```

#### Image Import Setting
Please put images under StreamingAssets folder, a and loading it using ```Image.file```.


## Debug UIWidgets Application

In Unity editor, you can switch debug/release mode by “UIWidgets->EnableDebug”.

If you want to change different mode in runtime, please modify the file “com.unity.uiwidgets/com.unity.uiwidgets/Runtime/foundation/debug.cs” by making “static bool debugEnableAtRuntimeInternal” to true or false. Note that the value is set to false by default.

## Using Window Scope
If you see the error `AssertionError: Window.instance is null` or null pointer error of `Window.instance`,
it means the code is not running in the window scope. In this case, you can enclose your code
with window scope as below:
```csharp
using(Isolate.getScope(the isolate of your App)) {
    // code dealing with UIWidgets,
    // e.g. setState(() => {....})
}
```

This is needed if the code is in methods
not invoked by UIWidgets. For example, if the code is in `completed` callback of `UnityWebRequest`,
you need to enclose them with window scope.
Please see our HttpRequestSample for detail.
For callback/event handler methods from UIWidgets (e.g `Widget.build, State.initState...`), you don't need do
it yourself, since the framework ensure it's in window scope.

## Learn

#### Samples
You can find many UIWidgets sample projects on Github, which cover different aspects and provide you
learning materials in various levels:
* UIWidgetsSamples (https://github.com/Unity-Technologies/com.unity.uiwidgets). These samples are developed by the dev team in order to illustrates all the features of 
UIWidgets. 
you can find all the sample scenes under the **Scene** folder.
You can also try UIWidgets-based Editor windows by clicking the new **UIWidgetsTests** tab on the main menu
and open one of the dropdown samples.
* awesome-UIWidgets by Liangxie (https://github.com/liangxiegame/awesome-uiwidgets). This Repo contains 
lots of UIWidget demo apps and third-party applications. 
* ConnectApp (https://github.com/UnityTech/ConnectAppCN). This is an online, open-source UIWidget-based App developed 
by the dev team. If you are making your own App with UIWidgets, this project will provides you with 
many best practice cases.


#### Wiki
The develop team is still working on the UIWidgets Wiki. However, since UIWidgets is mainly derived from Flutter,
 you can refer to Flutter Wiki to access detailed descriptions of UIWidgets APIs
 from those of their Flutter counterparts.
Meanwhile, you can join the discussion channel at (https://connect.unity.com/g/uiwidgets)

#### FAQ

| Question     | Answer  |
| :-----------------------------------------------| ---------------------: |
| Can I create standalone App using UIWidgets?     | **Yes**  |
| Can I use UIWidgets to build game UIs?   | **Yes**    |
| Can I develop Unity Editor plugins using UIWidgets?  | **Yes** |
| Is UIWidgets a extension of UGUI/NGUI? | **No** |
| Is UIWidgets just a copy of Flutter? | **No** |
| Can I create UI with UIWidgets by simply drag&drop? | **No** |
| Do I have to pay for using UIWidgets? | **No** |
| Any IDE recommendation for UIWidgets? | **Rider, VSCode(Open .sln)** |

## How to Contribute

Check [CONTRIBUTING.md](CONTRIBUTING.md)
