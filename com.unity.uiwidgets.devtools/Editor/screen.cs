using System;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.DevTools
{
  public class ScreenUtils
  {
    public static bool shouldShowScreen(Screen screen) {
      if (globals.offlineMode) {
        return screen.worksOffline;
      }
      if (screen.requiresLibrary != null) {
        if (!globals.serviceManager.isServiceAvailable ||
            !globals.serviceManager.isolateManager.selectedIsolateAvailable.isCompleted ||
            !globals.serviceManager.libraryUriAvailableNow(screen.requiresLibrary)) {
          return false;
        }
      }
      if (screen.requiresDartVm) {
        if (!globals.serviceManager.isServiceAvailable ||
            !globals.serviceManager.connectedApp.isRunningOnDartVM) {
          return false;
        }
      }
      if (screen.requiresDebugBuild) {
        if (!globals.serviceManager.isServiceAvailable || 
            globals.serviceManager.connectedApp.isProfileBuildNow) {
          return false;
        }
      }
      if (screen.requiresVmDeveloperMode) {
        if (!globals.preferences.vmDeveloperModeEnabled.value) {
          return false;
        }
      }
      return true;
    }
  }
  
  
    public abstract class Screen {
      public Screen(
        string screenId,
        string title = null,
        IconData icon = null,
        Key tabKey = null,
        string requiresLibrary = null,
        bool requiresDartVm = false,
        bool requiresDebugBuild = false,
        bool requiresVmDeveloperMode = false,
        bool worksOffline = false
      )
      {
        this.screenId = screenId;
        this.title = title;
        this.icon = icon;
        this.tabKey = tabKey;
        this.requiresLibrary = requiresLibrary;
        this.requiresDartVm = requiresDartVm;
        this.requiresDebugBuild = requiresDebugBuild;
        this.requiresVmDeveloperMode = requiresVmDeveloperMode;
        this.worksOffline = worksOffline;
      }

      public Screen(
        string id = null,
        string requiresLibrary = null,
        bool requiresDartVm = false,
        bool requiresDebugBuild = false,
        bool requiresVmDeveloperMode = false,
        bool worksOffline = false,
        string title = null,
        IconData icon = null,
        Key tabKey = null
      )
      {
        this.screenId = id;
        this.requiresLibrary = requiresLibrary;
        this.requiresDartVm = requiresDartVm;
        this.requiresDebugBuild = requiresDebugBuild;
        this.requiresVmDeveloperMode = requiresVmDeveloperMode;
        this.worksOffline = worksOffline;
        this.title = title;
        this.icon = icon;
        this.tabKey = tabKey;
      }

  public readonly string screenId;
  
  public readonly string title;

  public readonly IconData icon;
  
  public readonly Key tabKey;
  
  public readonly string requiresLibrary;
  
  public readonly bool requiresDartVm;

  public readonly bool requiresDebugBuild;

  public readonly bool requiresVmDeveloperMode;

  public readonly bool worksOffline;

  // ValueListenable<bool> showIsolateSelector
  // {
  //   get
  //   {
  //     return FixedValueListenable<bool>(false);
  //   }
  // }
  

  string docPageId
  {
    get
    {
      return null;
    }
  }

  int badgeCount
  {
    get
    {
      return 0;
    }
  }

  Widget buildTab(BuildContext context) {
    return new ValueListenableBuilder<int>(
      valueListenable:
      globals.serviceManager.errorBadgeManager.errorCountNotifier(screenId),
      builder: (context2, count, _) => {
        var tab = new Tab(
          key: tabKey,
          child: new Row(
            children: new List<Widget>{
              new Icon(icon, size: CommonThemeUtils.defaultIconSize),
              new Padding(
                padding: EdgeInsets.only(left: CommonThemeUtils.denseSpacing),
                child: new Text(title)
              ),
            }
          )
        );

        if (count > 0) {
          var painter = new TextPainter(
            text: new TextSpan(
              text: title
            ),
            textDirection: TextDirection.ltr
          );
          var titleWidth = painter.width;

          return new LayoutBuilder(
            builder: (context3, constraints) =>{
              return new Stack(
                children: new List<Widget>{
                  new CustomPaint(
                    size: new Size(CommonThemeUtils.defaultIconSize + CommonThemeUtils.denseSpacing + titleWidth, 0),
                    painter: new BadgePainter(number: count)
                  ),
                  tab,
                }
              );
            }
          );
        }

        return tab;
      }
    );
  }

  public abstract Widget build(BuildContext context);
  
  Widget buildStatus(BuildContext context, TextTheme textTheme) {
    return null;
  }
}

// mixin OfflineScreenMixin<T extends StatefulWidget, U> on State<T> {
//   bool loadingOfflineData
//   {
//     get
//     {
//       return _loadingOfflineData;
//     }
//   }
//
// bool _loadingOfflineData = false;
//
//   bool shouldLoadOfflineData();
//
//   FutureOr processOfflineData(U offlineData);
//
//   Future loadOfflineData(U offlineData) {
//     setState(() => {
//       _loadingOfflineData = true;
//     });
//     processOfflineData(offlineData).then(() =>
//     {
//       setState(() => {
//         _loadingOfflineData = false;
//       });
//     });
//   }
// }
    


public class BadgePainter : CustomPainter {
  public BadgePainter(int? number = null)
  {
    this.number = number;
  }

  public readonly int? number;
  
  public void paint(Canvas canvas, Size size)
  {
    Paint paint = new Paint();
    paint.color = CommonThemeUtils.devtoolsError;
    paint.style = PaintingStyle.fill;

    TextPainter countPainter = new TextPainter(
      text: new TextSpan(
        text: $"{number}",
        style: new TextStyle(
          color: Colors.white,
          fontWeight: FontWeight.bold
        )
      ),
      textDirection: TextDirection.ltr
    );
    countPainter.layout();

    var badgeWidth = Mathf.Max(
      CommonThemeUtils.defaultIconSize,
      countPainter.width + CommonThemeUtils.denseSpacing
    );
    canvas.drawOval(
      Rect.fromLTWH(size.width, 0, badgeWidth, CommonThemeUtils.defaultIconSize),
      paint
    );

    countPainter.paint(
      canvas,
      new Offset(size.width + (badgeWidth - countPainter.width) / 2, 0)
    );
  }
  
  public bool shouldRepaint(CustomPainter oldDelegate) {
    if (oldDelegate is BadgePainter) {
      return number != ((BadgePainter)oldDelegate).number;
    }
    return true;
  }

  public bool? hitTest(Offset position)
  {
    throw new NotImplementedException();
  }

  public void addListener(VoidCallback listener)
  {
    throw new NotImplementedException();
  }

  public void removeListener(VoidCallback listener)
  {
    throw new NotImplementedException();
  }
}

}