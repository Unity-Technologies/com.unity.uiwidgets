using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Brightness = Unity.UIWidgets.ui.Brightness;

namespace Unity.UIWidgets.cupertino {
    static class CupertinoThemeDataUtils {
        public static readonly Color _kDefaultBarLightBackgroundColor = new Color(0xCCF8F8F8);
        public static readonly Color _kDefaultBarDarkBackgroundColor = new Color(0xB7212121);
    }

    public class CupertinoTheme : StatelessWidget {
        public CupertinoTheme(
            CupertinoThemeData data,
            Widget child,
            Key key = null
        ) : base(key: key) {
            D.assert(child != null);
            D.assert(data != null);
            this.data = data;
            this.child = child;
        }

        public readonly CupertinoThemeData data;

        public readonly Widget child;

        public static CupertinoThemeData of(BuildContext context) {
            _InheritedCupertinoTheme inheritedTheme =
                (_InheritedCupertinoTheme) context.inheritFromWidgetOfExactType(typeof(_InheritedCupertinoTheme));
            return inheritedTheme?.theme?.data ?? new CupertinoThemeData();
        }


        public override Widget build(BuildContext context) {
            return new _InheritedCupertinoTheme(
                theme: this,
                child: new IconTheme(
                    data: new IconThemeData(color: data.primaryColor),
                    child: child
                )
            );
        }
    }

    class _InheritedCupertinoTheme : InheritedWidget {
        public _InheritedCupertinoTheme(
            CupertinoTheme theme,
            Widget child,
            Key key = null
        )
            : base(key: key, child: child) {
            D.assert(theme != null);
            this.theme = theme;
        }

        public readonly CupertinoTheme theme;

        public override bool updateShouldNotify(InheritedWidget old) {
            return theme.data != ((_InheritedCupertinoTheme) old).theme.data;
        }
    }

    public class CupertinoThemeData : Diagnosticable {
        public CupertinoThemeData(
            Brightness? brightness = null,
            Color primaryColor = null,
            Color primaryContrastingColor = null,
            CupertinoTextThemeData textTheme = null,
            Color barBackgroundColor = null,
            Color scaffoldBackgroundColor = null
        ) {
            _brightness = brightness;
            _primaryColor = primaryColor;
            _primaryContrastingColor = primaryContrastingColor;
            _textTheme = textTheme;
            _barBackgroundColor = barBackgroundColor;
            _scaffoldBackgroundColor = scaffoldBackgroundColor;
        }

        public static CupertinoThemeData raw(
            Brightness? brightness = null,
            Color primaryColor = null,
            Color primaryContrastingColor = null,
            CupertinoTextThemeData textTheme = null,
            Color barBackgroundColor = null,
            Color scaffoldBackgroundColor = null
        ) {
            D.assert(brightness != null);
            D.assert(primaryColor != null);
            D.assert(primaryContrastingColor != null);
            D.assert(textTheme != null);
            D.assert(barBackgroundColor != null);
            D.assert(scaffoldBackgroundColor != null);
            return new CupertinoThemeData(
                brightness: brightness,
                primaryColor: primaryColor,
                primaryContrastingColor: primaryContrastingColor,
                textTheme: textTheme,
                barBackgroundColor: barBackgroundColor,
                scaffoldBackgroundColor: scaffoldBackgroundColor
            );
        }

        bool _isLight {
            get { return brightness == Brightness.light; }
        }

        public Brightness brightness {
            get { return _brightness ?? Brightness.light; }
        }

        readonly Brightness? _brightness;

        public Color primaryColor {
            get {
                return _primaryColor ??
                       (_isLight ? CupertinoColors.activeBlue : CupertinoColors.activeOrange);
            }
        }

        readonly Color _primaryColor;

        public Color primaryContrastingColor {
            get {
                return _primaryContrastingColor ??
                       (_isLight ? CupertinoColors.white : CupertinoColors.black);
            }
        }

        readonly Color _primaryContrastingColor;

        public CupertinoTextThemeData textTheme {
            get {
                return _textTheme ?? new CupertinoTextThemeData(
                           brightness: brightness,
                           primaryColor: primaryColor
                       );
            }
        }

        readonly CupertinoTextThemeData _textTheme;

        public Color barBackgroundColor {
            get {
                return _barBackgroundColor ??
                       (_isLight
                           ? CupertinoThemeDataUtils._kDefaultBarLightBackgroundColor
                           : CupertinoThemeDataUtils._kDefaultBarDarkBackgroundColor);
            }
        }

        readonly Color _barBackgroundColor;

        public Color scaffoldBackgroundColor {
            get {
                return _scaffoldBackgroundColor ??
                       (_isLight ? CupertinoColors.white : CupertinoColors.black);
            }
        }

        readonly Color _scaffoldBackgroundColor;

        public CupertinoThemeData noDefault() {
            return new _NoDefaultCupertinoThemeData(
                _brightness,
                _primaryColor,
                _primaryContrastingColor,
                _textTheme,
                _barBackgroundColor,
                _scaffoldBackgroundColor
            );
        }

        public CupertinoThemeData copyWith(
            Brightness? brightness = null,
            Color primaryColor = null,
            Color primaryContrastingColor = null,
            CupertinoTextThemeData textTheme = null,
            Color barBackgroundColor = null,
            Color scaffoldBackgroundColor = null
        ) {
            return new CupertinoThemeData(
                brightness: brightness ?? _brightness,
                primaryColor: primaryColor ?? _primaryColor,
                primaryContrastingColor: primaryContrastingColor ?? _primaryContrastingColor,
                textTheme: textTheme ?? _textTheme,
                barBackgroundColor: barBackgroundColor ?? _barBackgroundColor,
                scaffoldBackgroundColor: scaffoldBackgroundColor ?? _scaffoldBackgroundColor
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            CupertinoThemeData defaultData = new CupertinoThemeData();
            properties.add(
                new EnumProperty<Brightness>("brightness", brightness, defaultValue: defaultData.brightness));
            properties.add(new DiagnosticsProperty<Color>("primaryColor", primaryColor,
                defaultValue: defaultData.primaryColor));
            properties.add(new DiagnosticsProperty<Color>("primaryContrastingColor", primaryContrastingColor,
                defaultValue: defaultData.primaryContrastingColor));
            properties.add(
                new DiagnosticsProperty<CupertinoTextThemeData>("textTheme", textTheme,
                    defaultValue: defaultData.textTheme));
            properties.add(new DiagnosticsProperty<Color>("barBackgroundColor", barBackgroundColor,
                defaultValue: defaultData.barBackgroundColor));
            properties.add(new DiagnosticsProperty<Color>("scaffoldBackgroundColor", scaffoldBackgroundColor,
                defaultValue: defaultData.scaffoldBackgroundColor));
        }
    }

    class _NoDefaultCupertinoThemeData : CupertinoThemeData {
        public _NoDefaultCupertinoThemeData(
            Brightness? brightness,
            Color primaryColor,
            Color primaryContrastingColor,
            CupertinoTextThemeData textTheme,
            Color barBackgroundColor,
            Color scaffoldBackgroundColor
        ) {
            this.brightness = brightness;
            this.primaryColor = primaryColor;
            this.primaryContrastingColor = primaryContrastingColor;
            this.textTheme = textTheme;
            this.barBackgroundColor = barBackgroundColor;
            this.scaffoldBackgroundColor = scaffoldBackgroundColor;
        }

        public new readonly Brightness? brightness;

        public new readonly Color primaryColor;

        public new readonly Color primaryContrastingColor;

        public new readonly CupertinoTextThemeData textTheme;

        public new readonly Color barBackgroundColor;

        public new readonly Color scaffoldBackgroundColor;
    }
}