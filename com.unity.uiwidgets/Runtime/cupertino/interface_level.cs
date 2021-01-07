using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.cupertino {
    public enum CupertinoUserInterfaceLevelData {
        baselayer,
        elevatedlayer,
        
    }
    public class CupertinoUserInterfaceLevel : InheritedWidget {
       
        public CupertinoUserInterfaceLevel(
            Key key = null,
            CupertinoUserInterfaceLevelData data = default,
            Widget child = null
        ) : base(key: key, child: child) 
        {
            D.assert(data != null);
            _data = data;
        }
        public CupertinoUserInterfaceLevelData _data;

        public static CupertinoUserInterfaceLevelData? of(BuildContext context, bool nullOk = false ) {
            D.assert(context != null);
            D.assert(nullOk != null);
            CupertinoUserInterfaceLevel query = context.dependOnInheritedWidgetOfExactType<CupertinoUserInterfaceLevel>(null);
            if (query != null)
              return query._data;
            //if (nullOk)
            //  return ;
            throw new UIWidgetsError(
              "CupertinoUserInterfaceLevel.of() called with a context that does not contain a CupertinoUserInterfaceLevel.\n" +
              "No CupertinoUserInterfaceLevel ancestor could be found starting from the context that was passed "+
              "to CupertinoUserInterfaceLevel.of(). This can happen because you do not have a WidgetsApp or "+
              "MaterialApp widget (those widgets introduce a CupertinoUserInterfaceLevel), or it can happen "+
              "if the context you use comes from a widget above those widgets.\n"+
              "The context used was:\n"+
              context.ToString()
            );
        }

          public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<CupertinoUserInterfaceLevelData>("user interface level", _data));
          }

          public override bool updateShouldNotify(InheritedWidget oldWidget) {
              //throw new System.NotImplementedException();              
              //oldWidget._data != _data;
              /// ????? 
              if (oldWidget.GetType() == typeof(CupertinoUserInterfaceLevel)) {
                  return updateShouldNotify(oldWidget);
              }

              return false;
          }
          public bool updateShouldNotify(CupertinoUserInterfaceLevel oldWidget) => oldWidget._data != _data;

    }

}

    