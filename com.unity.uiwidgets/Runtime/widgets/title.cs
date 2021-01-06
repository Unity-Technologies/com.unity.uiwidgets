using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.service;
namespace Unity.UIWidgets.widgets {
    class Title : StatelessWidget {
        public Title(
            Color color,
            Key key = null,
            string title = "",
            Widget child = null
        ) : base(key: key) {
            D.assert(title != null);
            D.assert(color != null && color.alpha == 0xFF);
        }

        public readonly String title;
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
            //properties.add(new ColorProperty("color", color, defaultValue: null));
        }
    }

}