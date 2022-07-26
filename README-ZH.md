# UIWidgets 2.0 （preview）


## 介绍

UIWidgets是Unity编辑器的一个插件包，可帮助开发人员通过Unity引擎来创建、调试和部署高效的跨平台应用。

UIWidgets主要来自[Flutter](https://github.com/flutter/flutter)。但UIWidgets通过使用强大的Unity引擎为开发人员提供了许多新功能，显著地改进他们开发的应用性能和工作流程。

**UIWidgets 2.0**是UIWidgets的最新版本，它针对**中国版Unity**开发并主要着力于UI绘制相关的整体性能优化。经测试，UIWidgets 2.0在iPhone 6等部分机型上的帧率相对1.0版本可以取得10%左右的提升。

如果因为各种原因您还需要使用UIWidgets 1.0，请在Releases中下载对应的包或者使用uiwidgets_1.0分支。

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
Unity Connect App是使用**UIWidgets 2.0**开发的一个移动App产品，您随时可以在Android (https://unity.cn/connectApp/download)
以及iOS (Searching for "Unity Connect" in App Store)端下载到它最新的版本. 本项目的所有代码均开源@https://github.com/UnityTech/ConnectAppCN.

#### Unity中文官方文档
Unity的线上中文官方文档由UIWidgets 1.0开发，您可以点击以下网址 https://connect.unity.com/doc 来访问它的全部内容。该项目目前已开源，所有代码可以在
https://github.com/UnityTech/DocCN 查看。

## 使用要求

#### Unity
:warning: **注意：UIWidgets 2.0仅仅适用于中国版Unity**

UIWidgets的各个版本所需的Unity版本如下表所示。您可以从[https://unity.cn/releases](https://unity.cn/releases)下载最新的Unity。

| UIWidgets 版本     |  Unity 2019 LTS  |  Unity 2020 LTS  | 
| -----------------------------------------------| ------------------------- | ------------------------- |
| 1.5.4 及以下     | 2019.4.10f1 及以上  | N\A |
| 2.0.1   | 2019.4.26f1c1  | N\A |
| 2.0.3   | 2019.4.26f1c1 ~ 2019.4.29f1c1 | N\A |
| 2.0.4及以上 | 2019.4.26f1c1 ~ 2019.4.29f1c1 | 2020.3.24f1c2 及以上 |

#### UIWidgets包

访问我们的Github存储库 [https://github.com/Unity-Technologies/com.unity.uiwidgets](https://github.com/Unity-Technologies/com.unity.uiwidgets)下载最新的UIWidgets包。

将下载的包文件夹移动到您Unity项目的根目录下。

通常，你可以在控制台（或终端）应用程序中输入下面的代码来完成这个操作：

   ```none
    cd <YourProjectPath>
    git clone https://github.com/Unity-Technologies/com.unity.uiwidgets.git com.unity.uiwidgets
   ```

此外，因为UIWidgets 2.0中为各个平台编译的C++动态库文件较大，我们使用了github提供的**Git Large File Storage**来管理它们以
优化下载体验。为此，请您确保在下载UIWidgets前安装好该[服务](https://docs.github.com/en/repositories/working-with-files/managing-large-files/installing-git-large-file-storage)以便正确下载动态库文件。

最后，在unity的PackageManager中，选择添加添加local file。选中```/com.unity.uiwidgets```下的```package.json```。

#### 运行环境

**UIWidgets 2.0**目前暂时只支持MacOS（**Intel64**，Metal/OpenGLCore），iOS（Metal/OpenGLes），Android（**OpenGLes**）以及 Windows（Direct3D11）。我们后续会针对更广泛的运行环境进行适配，敬请期待。与之相对的，UIWidgets 1.0目前支持所有Unity导出目标平台。

## 入门指南（演示[视频](https://www.bilibili.com/video/BV1zR4y1s7HN/)）

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
        public class UIWidgetsExample : UIWidgetsPanel
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

#### 八、外接纹理显示
利用我们新增的Unity内置API``UIWidgetsExternalTextureHelper.createCompatibleExternalTexture``以及UIWidgets中的``Texture``组件，开发者可以生成一个兼容UIWidgets底层的Unity纹理并将它绑定并渲染到UIWidgets页面上。从而可以将3D模型、视频等添加到App中作为一个UI组件。

需要注意，本功能目前只支持**OpenGLCore** (Mac), **OpenGLes** (iOS&Android) 以及 **D3D11** (Windows)，且必须使用**Unity 2020.3.37f1c1**及以上版本。在我们的示例项目中有一个简单的例子 (例如``3DTest1.unity``)可供参考。

#### 九、移动设备优化
目前在默认情况下，为了保证流畅度，项目在各个平台上均会以最高的刷新频率运行。不过您可以通过在代码中设置```UIWidgetsGlobalConfiguration.EnableAutoAdjustFramerate = true```的方式来开启自动降帧的功能：该功能开启后，在UI内容不变的情况下我们将降低项目的刷新率来降低耗电。

在移动设备上UI绘制的流畅度受到GC影响较大。如有卡顿，例如滑动掉帧。可开启OnDemandGC, UIWidgets将接管并优化整体GC效果，请在代码中设置```UIWidgetsGlobalConfiguration.EnableIncrementalGC = true```,并开启```Project Setting -> Player -> Other Settings -> Use incremental GC```。

## 调试UIWidgets应用程序

在Editor模式下，编辑器菜单栏选择```UIWidgets->EnableDebug```。

在Runtime模式下，Debug/Development build会自动开启Debug，Release build则会自动关闭Debug。

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
同时，你也可以加入我们的[讨论组](https://unity.cn/plate/uiwidgets)来和大家交流使用心得。

#### 常问问题解答

1. 在打开一个UIWidgets 2.0项目后Unity编辑器崩溃了。

      请确定您使用的Unity编辑器版本兼容您使用的UIWidgets版本。例如，**UIWidgets 2.0.3**只支持以下中国版Unity版本：2019.4.26f1c1 ～ 2019.4.29f1c1。您可以在[这里](#unity)查找兼容您UIWidgets版本的Unity版本信息。

2. 在打开一个UIWidgets 2.0项目后Unity控制台报错，报错信息为**DllNotFoundException: libUIWidgets**。

      请首先检查您的UIWidgets根目录下/Runtime/Plugins中适配各个平台的C++库文件是否完整。例如，Windows平台下的libUIWidgets.dll位于*X86_64*子目录下，Mac平台下的libUIWidgets.dylib位于*osx*目录下。

      如果您发现库文件不存在或者文件大小不正常（<1MB)，请确认您已经在您电脑上安装了**Git Large File Storage**，然后在UIWidgets根目录下执行如下指令：
      ```
      git lfs pull
      ```

3. UIWidgets 2.0和UIWidgets 1.0的区别大吗？哪个更适合我的项目？

      在UIWidgets 1.0中所有渲染相关代码都由C#编写并且使用了Unity [Graphics API](https://docs.unity3d.com/ScriptReference/Graphics.html)
      进行渲染。因此它可以正确运行在任意Unity支持的平台。不过与此同时，它的运行效率较低，且渲染效果与flutter在某一些细节上并不一致。

      在UIWidgets 2.0中我们将一个flutter引擎嵌入到了C++动态库中，然后Unity通过调用这个动态库来进行渲染。因此，它的渲染结果与flutter完全一致，且性能比C#实现的渲染代码有明显提升。不过为了使flutter引擎和Unity可以正确协作，我们对flutter和Unity引擎都进行了一些修改。因此，目前UIWidgets 2.0只能够运行在包含上述修改的中国版Unity中，并且暂时只支持部分Unity的目标平台。

      由于UIWidgets 2.0在效果和效率上的优势，因此推荐大家使用。仅当您需要在UIWidgets 2.0暂时不支持的平台（如webgl）上开发时才推荐使用UIWidgets 1.0。此外，由于人力原因，目前只有UIWidgets 2.0我们会持续更新。

4. 使用Unity 2020.3LTS打包UIWidgets 2.0的项目到iOS平台后Build失败，提示无法链接到OpenGLES库函数。

      这是因为在Unity 2020.3版本中Unity导出的iOS项目默认不再包含对OpenGLES库的依赖，但UIWidgets 2.0需要依赖该库。为了解决这个问题，您需要手动用Xcode打开项目并为UnityFramework添加上对OpenGLES库的依赖。
      
## 联系我们
官方QQ群: UIWidgets (群ID: 234207153)

## 如何贡献
请查看[CONTRIBUTING.md](CONTRIBUTING.md)
