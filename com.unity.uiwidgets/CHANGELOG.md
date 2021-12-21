# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.0.4-preview.1] - 2021-12-21

### Fixes
- Android IL2CPP bbuild crash issue [\#194](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/194)
- Fix glContext issue when showing multiple UIWidgets panels [\#209](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/209)
- Fix the issue that hover doesn't work [\#215](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/215)
- Fix crash caused by shadow [\#225](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/225)
- Fix stackoverflow issue caused by static methods [\#246](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/246)
- Fix crash caused by missing Lottie file [\#263](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/263)
- Fix color calculation issue [\#267](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/267)

### New Features
- Add support for Raycastable panel [\#214](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/214)
- Add support for Android arm64 build [\#221](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/221)
- Add support for Editor drag&drop support [\#268](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/268)

## [2.0.3-preview.1] - 2021-07-02

### Fixes
- Touch input on mobiles [\#187](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/187)
- Crash when font file not found [\#183](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/183)
- Incorrect renderView init transform [\#182](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/182)

### New Features
- Incremental GC [\#184](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/184)


## [2.0.2-preview.1] - 2021-05-31

Major changes are as follows:

### Fixes
- Android return button support [\#168](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/168)

### New Features
- Add engine compilation scripts [\#170](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/170)
- On demand rendering support [\#166](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/166)
- [Breaking Changes] Use CreateExternalTexture API to share textures on Windows [\#153](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/153)


## [2.0.1-preview.1] - 2021-05-18

#### In this release we mainly focus on the optimization and stabilization of the framework. 
#### We also upgrade UIWidgets to version 1.17.5, mainly derived from flutter 1.17.5.

### New Features
- Upgrade UIWidgets to 2.0 with breaking changes from 1.5.4 [\#145](https://github.com/Unity-Technologies/com.unity.uiwidgets/pull/145)

## [1.5.4-preview.1] - 2019-08-30

#### In this release we mainly focus on the optimization and stabilization of the framework. 
#### We also upgrade UIWidgets to version 1.5.4, mainly derived from flutter 1.5.4.

### New Features
- Optimize the GC performance of the rendering system [\#247](https://github.com/UnityTech/UIWidgets/pull/247)
- Optimize the rendering performance of shadows [\#257](https://github.com/UnityTech/UIWidgets/pull/257)
- Leverage Compute Buffer to optimize GPU-CPU communication [\#272](https://github.com/UnityTech/UIWidgets/pull/272)
- Cupertino Theme Supported [\#287](https://github.com/UnityTech/UIWidgets/pull/287)
- Support Unity Editor Drag&Drop mouse event [\#253](https://github.com/UnityTech/UIWidgets/pull/253)
- Implement geometric shapes anti-alias draw [\#262](https://github.com/UnityTech/UIWidgets/pull/262)
- Optimize paragraph layout [\#254](https://github.com/UnityTech/UIWidgets/pull/254)
- Support emoji display and edit [\#231](https://github.com/UnityTech/UIWidgets/pull/231)

## [1.0.0-preview] - 2019-03-01

### This is the first release of *Unity Package UIWidgets*.

*just the first release*