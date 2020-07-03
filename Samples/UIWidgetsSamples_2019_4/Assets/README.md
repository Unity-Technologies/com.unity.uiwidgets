# UIWidgets Samples
[中文](README-ZH.md)

## Introduction
This project provides standard samples for UIWidgets.

UIWidgets is an open-source, standalone and novel UI framework for Unity. You can find various UIWidgets-based 
UI panels in this Repository which illustrates different features of the corresponding framework. 
Please feel free to run and test these demos. You are also encouraged to modify them to meet your own
requirements and see the outcomes.


## Requirements

#### Unity

Install **Unity 2018.4.10f1 (LTS)** or **Unity 2019.1.14f1** and above. You can download the latest Unity on https://unity3d.com/get-unity/download.

#### UIWidgets Package
Visit the Github repository https://github.com/UnityTech/UIWidgets
 to download the latest UIWidgets package.

Move the downloaded package folder into the **Package** folder of your Unity project.

Generally, you can make it using a console (or terminal) application by just a few commands as below:

   ```none
    cd <YourProjectPath>/Packages
    git clone https://github.com/UnityTech/UIWidgets.git com.unity.uiwidgets
   ```

#### Install
Create an empty project using Unity and copy the latest UIWidgets package into it. Then clone this 
Repository to the **Asset** folder of your project.


## Guide

#### Runtime UIs
In the **Scenes** folder you can find all the demo scenes which illustrate different features of UIWidgets.
More specifically, the **UIWidgetsGallery** scene contains a Gallery demo of UIWidgets. It is mainly derived from 
the flutter Gallery demo and will provides you a big picture about "What UIWidgets can do".

In the **UIWidgetsTheatre** scene you can switch between some deliberately chosen UIWidgets Panels, each 
focusing on one specific feature of UIWidgets.

The implementations of all the demo UI widgets are located in different folders. In short:
* **MaterialSample** contains sample codes of material scheme widgets
* **ReduxSample** contains samples codes for the redux framework used in UIWidgets
* **UIWidgetGallery** contains codes of the Gallery demo
* **UIWidgetsSample** contains samples codes for basic widgets
* **UIWidgetsTheatre** contains codes of the UIWidgetsTheatre

#### EditorWindow UIs
UIWidgets can also be used to create EditorWindows in Unity. 
Please click the new **UIWidgetsTests** tab on the main menu
and open one of the dropdown samples to see the corresponding EditorWindow example.

All the codes of the EditorWindow samples are located in the **Editor** folder.
