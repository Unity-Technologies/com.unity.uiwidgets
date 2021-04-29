using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public abstract class InheritedTheme : InheritedWidget {

        public InheritedTheme(
            Key key = null,
            Widget child = null
        ) : base(key: key, child: child) {
        }
        public abstract Widget wrap(BuildContext context, Widget child);
        
        public static Widget captureAll(BuildContext context, Widget child) { 
            D.assert(child != null);
            D.assert(context != null);
            List<InheritedTheme> themes = new List<InheritedTheme>();
            HashSet<Type> themeTypes = new  HashSet<Type>();
            context.visitAncestorElements((Element ancestor)=> { 
                if (ancestor is InheritedElement && ancestor.widget is InheritedTheme) { 
                    InheritedTheme theme = ancestor.widget as InheritedTheme;
                    Type themeType = theme.GetType();
                    if (!themeTypes.Contains(themeType)) {
                        themeTypes.Add(themeType);
                        themes.Add(theme);
                    }
                }
                return true;
            });

            return new _CaptureAll(themes: themes, child: child);
        }
    }
    public  class _CaptureAll : StatelessWidget {
        public _CaptureAll(
            Key key = null,
            List<InheritedTheme> themes = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(themes != null);
            D.assert(child != null);
            this.child = child;
            this.themes = themes;
        }

        public readonly List<InheritedTheme> themes;
        public readonly  Widget child;

    
        public override Widget build(BuildContext context) {
            Widget wrappedChild = child;
            foreach (InheritedTheme theme in themes)
                wrappedChild = theme.wrap(context, wrappedChild);
            return wrappedChild;
        }
    }
}