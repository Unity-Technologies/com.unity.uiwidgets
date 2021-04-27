# UIWidgets 2.0 （preview）


## 介绍

UIWidgets是Unity编辑器的一个插件包，可帮助开发人员通过Unity引擎来创建、调试和部署高效的跨平台应用。

UIWidgets主要来自[Flutter](https://github.com/flutter/flutter)。但UIWidgets通过使用强大的Unity引擎为开发人员提供了许多新功能，显著地改进他们开发的应用性能和工作流程。

UIWidgets 2.0是UIWidgets的最新版本，它主要着力于UI绘制相关的整体性能优化。经测试，UIWidgets 2.0在iPhone 6等部分机型上的帧率相对1.0版本可以取得10%左右的提升。如果因为各种原因您还需要使用UIWidgets 1.0，请在Releases中下载对应的包或者使用uiwidgets_1.0分支。

#### 效率
通过使用最新的Unity渲染SDK，UIWidgets应用可以非常快速地运行并且大多数时间保持大于60fps的速度。

#### 跨平台
与任何其他Unity项目一样，UIWidgets应用可以直接部署在各种平台上，包括PC和移动设备等。

#### 多媒体支持
除了基本的2D UI之外，开发人员还能够将3D模型，音频，粒子系统添加到UIWidgets应用中。

#### 开发者友好
开发者可以使用许多高级工具，如CPU/GPU Profiling和FPS Profiling，直接在Unity Editor中调试UIWidgets应用。

## 示例

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

### 基于UIWidgets的项目

#### Unity Connect App
Unity Connect App是使用UIWidgets开发的一个移动App产品，您随时可以在Android (https://connect.unity.com/connectApp/download)
以及iOS (Searching for "Unity Connect" in App Store)端下载到它最新的版本. 本项目的所有代码均开源@https://github.com/UnityTech/ConnectAppCN.

#### Unity中文官方文档
Unity的线上中文官方文档由UIWidgets开发，您可以点击以下网址 https://connect.unity.com/doc 来访问它的全部内容。该项目目前已开源，所有代码可以在
https://github.com/UnityTech/DocCN 查看。

## 使用要求

#### Unity

安装 **Unity 2019.1.14f1c1** 及其更高版本。 你可以从[https://unity3d.com/get-unity/download](https://unity3d.com/get-unity/download)下载最新的Unity。

#### UIWidgets包

访问我们的Github存储库 [https://github.com/Unity-Technologies/com.unity.uiwidgets](https://github.com/Unity-Technologies/com.unity.uiwidgets)下载最新的UIWidgets包。

将下载的包文件夹移动到Unity项目的Package文件夹中。

通常，你可以在控制台（或终端）应用程序中输入下面的代码来完成这个操作：

   ```none
    cd <YourPackagePath>
    git clone https://github.com/Unity-Technologies/com.unity.uiwidgets.git com.unity.uiwidgets
   ```
在unity的PackageManager中，选择添加添加local file。选中```/com.unity.uiwidgets```下的```package.json```。

#### 运行环境

UIWidgets目前暂时只支持MacOS（Metal），iOS（Metal），Android（Armv7，OpenGLes）以及 Windows（Direct3D11）。我们后续会针对更广泛的运行环境进行适配，敬请期待。

## 入门指南

#### 一、 概观
在本教程中，我们将创建一个非常简单的UIWidgets应用。 该应用只包含文本标签和按钮。 文本标签将计算按钮上的点击次数。

首先，请打开或创建Unity项目并使用Unity编辑器打开它。

#### 二、 场景构建

UIWidgets应用通常构建在Unity UI Canvas上。 请按照以下步骤在Unity中创建一个
UI Canvas。
1. 选择 File > New Scene来创建一个新场景。
2. 选择 GameObject > UI > Canvas 在场景中创建UI Canvas。
3. 右键单击Canvas并选择UI > Panel，将面板（即面板1）添加到UI Canvas中。 然后删除面板中的 **Image** 组件。

#### 三、创建小部件

UIWidgets应用是用**C＃脚本**来编写的。 请按照以下步骤创建应用程序并在Unity编辑器中播放。
1. 创建一个新C＃脚本，命名为“UIWidgetsExample.cs”，并将以下代码粘贴到其中。

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

2. 保存此脚本，并将其附加到Panel 1中作为其组件。
3. 在Unity编辑器中，点击Play按钮来启动应用。

#### 四、构建应用程序

最后，你可以按以下步骤将UIWidgets应用构建成适用于任何特定平台的应用程序包。
1. 选择**File** > **Build Settings...**打开Build Settings面板。
2. 选择目标平台，点击Build。 之后Unity编辑器将自动组装所有相关资源并生成最终的应用程序包。

#### 五、如何加载图像？
1. 将你的图像文件，如image1.png，放在StreamingAssets文件夹中。
2. 使用Image.file("image1.png")加载图像。


UIWidgets也支持Gif！
1. 假设你有一个loading1.gif文件，复制到StreamingAssets文件夹。
2. 使用Image.file("loading1.gif")加载gif图像。

#### 六、在安卓上显示状态栏
当一个Unity项目运行在Android设备上时，状态栏是默认隐藏且无法在编辑内进行调整的。
如果您希望在您的UIWidgets App中显示状态栏，您可以取消```Start in fullscreen``` 与 ```record outside safe area```, 确认```UIWidgetsAndroidConfiguration```下```showStatusBar```为 ```true```

#### 七、图片导入设置
请将图片放入StreamingAssets下，而后用```Image.file```读取


## 调试UIWidgets应用程序

在编辑器菜单栏选择```UIWidgets->EnableDebug```

如果想在runtime选择是否debug，请修改文件```com.unity.uiwidgets/com.unity.uiwidgets/Runtime/foundation/debug.cs```中的```static bool debugEnableAtRuntimeInternal```

## 使用Window Scope保护外部调用
如果您在调试时遇到 `AssertionError: Window.instance is null` 或者在调用 `Window.instance` 时得到空指针, 那么您需要
使用以下方式来保护您的调用，使之可以在正确的Isolate上执行回调逻辑：
```csharp
using(Isolate.getScope(the isolate of your App)) {
    // code dealing with UIWidgets,
    // e.g. setState(() => {....})
}
```

通常，在您使用外部事件，例如来自UIWidgets之外的输入事件、网络传输回调事件时需要做这样的处理。具体的您可以参考我们的 HttpRequestSample 这个样例中的写法。需要注意的是，一般情况下您在UIWidgets框架的内部调用 (如 `Widget.build, State.initState...`)中不需要额外采取上述保护措施。

## 学习

#### 教程

包括开发组在内的广大开发者为UIWidgets提供了许多可供学习的样例和教程，你可以根据你的需求进行学习：
- UIWidgets官方示例。目前所有官方使用的示例项目均维护在一个独立的Github仓库（ https://github.com/Unity-Technologies/com.unity.uiwidgets ）中。
具体的，你可以在Sample项目的**Scene**子文件夹中浏览所有示例UI场景。
此外，你还可以点击主菜单上的新增的UIWidgetsTests选项卡，并在下拉菜单中选择一个EditorWindow UI示例来运行。
- UIWidgets凉鞋系列教程。你可以在凉鞋老师整理的Github仓库（ https://github.com/liangxiegame/awesome-uiwidgets ）中学习UIWidgets的基本用法
以及许多有趣的小Demo。
- ConnectApp开源项目。这是一个完整的线上、开源、完全基于UIWidgets的第一方App项目。其中包含了大量产品级的UIWidgets工程实践细节，
如果你想深入了解UIWidgets并且使用它构建线上项目，请访问项目Github仓库了解更多（ https://github.com/UnityTech/ConnectAppCN ）。

#### Wiki

目前开发团队仍在改进UIWidgets Wiki。 由于UIWidgets主要来源于Flutter，你也可以参考Flutter Wiki中与UIWidgets API对应部分的详细描述。
同时，你可以加入我们的讨论组( https://connect.unity.com/g/uiwidgets )。

#### 常问问题解答

| 问题     | 回答  |
| :-----------------------------------------------| ---------------------: |
| 我可以使用UIWidgets创建独立应用吗？     | 可以  |
| 我可以使用UIWidgets构建游戏UI吗？   | 可以    |
| 我可以使用UIWidgets开发Unity编辑器插件吗？ | 可以 |
| UIWidgets是UGUI / NGUI的扩展吗？ | 不是 |
| UIWidgets只是Flutter的副本吗？ | 不是 |
| 我可以通过简单的拖放操作来创建带有UIWidgets的UI吗？ | 不可以 |
| 我是否需要付费使用UIWidgets？ | 不需要 |
| 有推荐的适用于UIWidgets的IDE吗？ | Rider, VSCode(Open .sln) |

## 如何贡献
请查看[CONTRIBUTING.md](CONTRIBUTING.md)
