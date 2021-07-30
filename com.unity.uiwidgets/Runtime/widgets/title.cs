using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.service;
namespace Unity.UIWidgets.widgets {
    class Title : StatelessWidget {
        public Title(
            Color color = null,
            Key key = null,
            string title = "",
            Widget child = null
        ) : base(key: key) {
            D.assert(title != null);
            //in flutter, the background color is not allowed to be transparent because there is nothing behind the UI. But in UIWidgets it is 
            //possible to put a unity scene under the ui panel. Therefore we can discard the assertion on "color.alpha == 0xFF" here 
            D.assert(color != null);
            this.color = color;
            this.child = child;
            this.title = title;
        }

        public readonly string title;
        public readonly Color color;
        public readonly Widget child;
        public override Widget build(BuildContext context) {
            /*SystemChrome.setApplicationSwitcherDescription(
                new ApplicationSwitcherDescription(
                    label: title,
                    primaryColor: (int)color.value
                )
            );*/
            return child;
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("title", title, defaultValue: ""));
            properties.add(new ColorProperty("color", color, defaultValue: null));
        }
    }

}