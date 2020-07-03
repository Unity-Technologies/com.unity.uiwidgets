# UIWidgets Samples


## 介绍
本项目包含了UIWidgets的所有官方开发样例，用以帮助开发者快速入门基于UIWidgets的UI开发。

UIWidgets是一个新的、开源、独立的Unity UI界面框架。在本项目中您将可以找到许多基于UIWidgets开发的界面样例，它们
涵盖了该框架所提供的主要功能。您可以随意运行和测试这些样例，您也可以自行改造这些样例来制作您自己的UI作品。


## 使用要求

#### Unity

安装 **Unity 2018.4.10f1(LTS)** 或 **Unity 2019.1.14f1** 及其更高版本。 你可以从[https://unity3d.com/get-unity/download](https://unity3d.com/get-unity/download)下载最新的Unity。

#### UIWidgets包

访问UIWidgets的Github存储库 [https://github.com/UnityTech/UIWidgets](https://github.com/UnityTech/UIWidgets)下载最新的UIWidgets包。

将下载的包文件夹移动到Unity项目的Package文件夹中。

通常，你可以在控制台（或终端）应用程序中输入下面的代码来完成这个操作：

   ```none
    cd <YourProjectPath>/Packages
    git clone https://github.com/UnityTech/UIWidgets.git com.unity.uiwidgets
   ```

#### 安装
使用Unity建立一个空的新项目并将下载好的UIWidgets包移动到其Package文件夹中。接下来将本项目下载到
您项目的**Asset**目录下即可。


## 使用指南

#### 运行时UI界面
在**Scenes**目录下包含了所有可用的运行时UI界面对应的场景文件。您可以打开任何一个场景来测试对应的UI界面。
特别的，在**UIWidgetsGallery**场景中您可以运行UIWidgets Gallery范例，该范例主要移植自flutter Gallery，
它涵盖了UIWidgets的大部分功能点，您可以通过它来了解UIWidgets整体能力。

其次，在**UIWidgetsTheatre**场景中提供了一个可切换的UI展示Panel，在这里您可以从一系列精心挑选出来的样例界面中
选择并展示您所感兴趣的部分。

最后，上述样例界面的具体实现文件可以参考以下目录：
* **MaterialSample** 包含了部分Material风格组件的样例代码
* **ReduxSample** 包含了Redux组件相关的样例代码
* **UIWidgetGallery** 包含了UIWidgets Gallery场景相关的代码
* **UIWidgetsSample** 包含了部分基础Widget组件的样例代码
* **UIWidgetsTheatre** 包含了UIWidgetsTheatre场景相关的代码

#### EditorWindow UI界面
UIWidgets也可以被用来制作Unity中的EditorWindow。请点击菜单栏中的新的**UIWidgetsTests**选项卡，并
在下拉菜单中选择打开您感兴趣的选项来显示对应的UI界面。

所有EditorWindow样例相关的代码均位于**Editor**文件夹中。
