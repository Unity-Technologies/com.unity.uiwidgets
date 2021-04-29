using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public class IconTheme : InheritedTheme {
        public IconTheme(
            Key key = null,
            IconThemeData data = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(data != null);
            D.assert(child != null);
            this.data = data;
        }

        public static Widget merge(
            Key key = null,
            IconThemeData data = null,
            Widget child = null
        ) {
            return new Builder(
                builder: (BuildContext context)=> {
                return new IconTheme(
                    key: key,
                    data: _getInheritedIconThemeData(context).merge(data),
                    child: child
                );
            }
            );
        }

        public readonly IconThemeData data;

        public static IconThemeData of(BuildContext context) {
            IconThemeData iconThemeData = _getInheritedIconThemeData(context).resolve(context);
            return iconThemeData.isConcrete
                ? iconThemeData
                : iconThemeData.copyWith(
                    size: iconThemeData.size ??  IconThemeData.fallback().size,
            color: iconThemeData.color ?? IconThemeData.fallback().color,
            opacity: iconThemeData.opacity ?? IconThemeData.fallback().opacity
                );
        }

        static IconThemeData _getInheritedIconThemeData(BuildContext context) {
            IconTheme iconTheme = (IconTheme) context.dependOnInheritedWidgetOfExactType<IconTheme>();
            
            return iconTheme?.data ?? IconThemeData.fallback();
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return data != ((IconTheme) oldWidget).data;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            data.debugFillProperties(properties);
        }

        public override Widget wrap(BuildContext context, Widget child) {
            IconTheme iconTheme = context.findAncestorWidgetOfExactType<IconTheme>();
            return this == iconTheme ? child : new IconTheme(data: data, child: child);
        }
    }
}