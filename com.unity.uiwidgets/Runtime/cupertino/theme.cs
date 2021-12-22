using System.Runtime.CompilerServices;
using Unity.UIWidgets.foundation;
//using Unity.UIWidgets.material;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Brightness = Unity.UIWidgets.ui.Brightness;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    class CupertinoThemeDataUtils {
        
        public static readonly _CupertinoThemeDefaults _kDefaultTheme = 
            new _CupertinoThemeDefaults(
                null,
                CupertinoColors.systemBlue,
                CupertinoColors.systemBackground,
                CupertinoDynamicColor.withBrightness(
                    color: new Color(0xF0F9F9F9),
                    darkColor: new Color(0xF01D1D1D)
                    // Values extracted from navigation bar. For toolbar or tabbar the dark color is 0xF0161616.
                ),
                CupertinoColors.systemBackground,
                new _CupertinoTextThemeDefaults(CupertinoColors.label, CupertinoColors.inactiveGray)
            );



    }

    public class CupertinoTheme : StatelessWidget {
        public CupertinoTheme(
            Key key = null,
            CupertinoThemeData data = null,
            Widget child = null
            
        ) : base(key: key) {
            D.assert(child != null);
            D.assert(data != null);
            this.data = data;
            this.child = child;
        }

        public readonly CupertinoThemeData data;

        public readonly Widget child;

        public static CupertinoThemeData of(BuildContext context) { 
            _InheritedCupertinoTheme inheritedTheme = context.dependOnInheritedWidgetOfExactType<_InheritedCupertinoTheme>(); 
            var result = (inheritedTheme?.theme?.data ?? new CupertinoThemeData()).resolveFrom(context, nullOk: true);
            return result;
        }

        public static Brightness? brightnessOf(BuildContext context, bool nullOk = false) {
            _InheritedCupertinoTheme inheritedTheme = context.dependOnInheritedWidgetOfExactType<_InheritedCupertinoTheme>();
            return (inheritedTheme?.theme?.data?.brightness) ?? MediaQuery.of(context, nullOk: nullOk)?.platformBrightness;
        }

        public override Widget build(BuildContext context) {
            return new _InheritedCupertinoTheme(
                theme: this,
                child: new IconTheme(
                    data: new CupertinoIconThemeData(color: data.primaryColor),
                    child: child
                )
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            data.debugFillProperties(properties);
        }
    }

    class _InheritedCupertinoTheme : InheritedWidget {
        public _InheritedCupertinoTheme(
            Key key = null,
            CupertinoTheme theme = null,
            Widget child = null
        ) : base(key: key, child: child) {
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
            Color scaffoldBackgroundColor = null,
            _CupertinoThemeDefaults defaults = null
        ) {
            this.brightness = brightness;// ?? Brightness.light;
            _primaryColor = primaryColor;
            _primaryContrastingColor = primaryContrastingColor;
            _textTheme = textTheme;
            _barBackgroundColor = barBackgroundColor;
            _scaffoldBackgroundColor = scaffoldBackgroundColor;
            _defaults =  defaults ?? CupertinoThemeDataUtils._kDefaultTheme;

        }

        
        public static CupertinoThemeData _rawWithDefaults(
            Brightness? brightness,
            Color primaryColor,
            Color primaryContrastingColor ,
            CupertinoTextThemeData textTheme ,
            Color barBackgroundColor ,
            Color scaffoldBackgroundColor ,
            _CupertinoThemeDefaults defaults
        ) {
            var themeData = new CupertinoThemeData(
                brightness: brightness,
                primaryColor: primaryColor,
                primaryContrastingColor: primaryContrastingColor,
                textTheme: textTheme,
                barBackgroundColor: barBackgroundColor,
                scaffoldBackgroundColor: scaffoldBackgroundColor,
                defaults);
           
            return themeData;
        }

        public  _CupertinoThemeDefaults _defaults;

        public readonly Brightness? brightness;

        public Color primaryColor {
            get {
                return _primaryColor ?? _defaults.primaryColor;
            }
        }
        readonly Color _primaryColor;

        public Color primaryContrastingColor {
            get {
                return _primaryContrastingColor ?? _defaults.primaryContrastingColor;
            }
        }
        readonly Color _primaryContrastingColor;

        public CupertinoTextThemeData textTheme {
            get {
                return _textTheme ?? _defaults.textThemeDefaults.createDefaults(primaryColor: primaryColor);
            }
        }
        readonly CupertinoTextThemeData _textTheme;

        public Color barBackgroundColor {
            get {
                return _barBackgroundColor ?? _defaults.barBackgroundColor;
            }
        }
        readonly Color _barBackgroundColor;

        public Color scaffoldBackgroundColor {
            get {
                return _scaffoldBackgroundColor ??  _defaults.scaffoldBackgroundColor;
            }
        }
        readonly Color _scaffoldBackgroundColor;

        public CupertinoThemeData noDefault() {
            return new _NoDefaultCupertinoThemeData(
                brightness,
                _primaryColor,
                _primaryContrastingColor,
                _textTheme,
                _barBackgroundColor,
                _scaffoldBackgroundColor
            );
        }

        public CupertinoThemeData resolveFrom(BuildContext context,  bool nullOk = false ) {
            Color convertColor(Color color) => CupertinoDynamicColor.resolve(color, context, nullOk: nullOk);

            return new CupertinoThemeData(
                brightness:brightness,
                primaryColor:convertColor(_primaryColor),
                primaryContrastingColor:convertColor(_primaryContrastingColor),
                textTheme:_textTheme?.resolveFrom(context, nullOk: nullOk),
                barBackgroundColor:convertColor(_barBackgroundColor),
                scaffoldBackgroundColor:convertColor(_scaffoldBackgroundColor),
                defaults:_defaults.resolveFrom(context, _textTheme == null, nullOk: nullOk)
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
            //return new CupertinoThemeData(
                return _rawWithDefaults(
                brightness: brightness ?? this.brightness,
                primaryColor: primaryColor ?? _primaryColor,
                primaryContrastingColor: primaryContrastingColor ?? _primaryContrastingColor,
                textTheme: textTheme ?? _textTheme,
                barBackgroundColor: barBackgroundColor ?? _barBackgroundColor,
                scaffoldBackgroundColor: scaffoldBackgroundColor ?? _scaffoldBackgroundColor,
                _defaults
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            CupertinoThemeData defaultData = new CupertinoThemeData();
            properties.add(
                new EnumProperty<Brightness?>("brightness", brightness, defaultValue: defaultData.brightness));
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
            textTheme.debugFillProperties(properties);
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
        ) : base(
            brightness,
            primaryColor,
            primaryContrastingColor,
            textTheme,
            barBackgroundColor,
            scaffoldBackgroundColor,
            null) {
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
        
        public new _NoDefaultCupertinoThemeData resolveFrom(BuildContext context,bool nullOk = false ) {
            Color convertColor(Color color) => CupertinoDynamicColor.resolve(color, context, nullOk: nullOk);

            return new _NoDefaultCupertinoThemeData(
                brightness,
                convertColor(primaryColor),
                convertColor(primaryContrastingColor),
                textTheme?.resolveFrom(context, nullOk: nullOk),
                convertColor(barBackgroundColor),
                convertColor(scaffoldBackgroundColor)
            );
        }

      
        public new CupertinoThemeData copyWith(
            Brightness? brightness = null,
            Color primaryColor = null,
            Color primaryContrastingColor = null,
            CupertinoTextThemeData textTheme = null,
            Color barBackgroundColor = null,
            Color scaffoldBackgroundColor = null
        ) {
            return new _NoDefaultCupertinoThemeData(
                brightness ?? this.brightness,
                primaryColor ?? this.primaryColor,
                primaryContrastingColor ?? this.primaryContrastingColor,
                textTheme ?? this.textTheme,
                barBackgroundColor ?? this.barBackgroundColor,
                scaffoldBackgroundColor ?? this.scaffoldBackgroundColor
            );
        }
    }

    public class _CupertinoThemeDefaults {
        public _CupertinoThemeDefaults(
            Brightness? brightness ,
            Color primaryColor ,
            Color primaryContrastingColor ,
            Color barBackgroundColor ,
            Color scaffoldBackgroundColor ,
            _CupertinoTextThemeDefaults textThemeDefaults 
            ) 
        {
            this.brightness = brightness;
            this.primaryColor = primaryColor;
            this.primaryContrastingColor = primaryContrastingColor;
            this.barBackgroundColor = barBackgroundColor;
            this.scaffoldBackgroundColor = scaffoldBackgroundColor;
            this.textThemeDefaults = textThemeDefaults;

        }
        public Brightness? brightness;
        public Color primaryColor;
        public Color primaryContrastingColor;
        public Color barBackgroundColor;
        public Color scaffoldBackgroundColor;
        public _CupertinoTextThemeDefaults textThemeDefaults;

        public _CupertinoThemeDefaults resolveFrom(BuildContext context, bool resolveTextTheme,  bool nullOk = false) {
            Color convertColor(Color color) => CupertinoDynamicColor.resolve(color, context, nullOk: nullOk);

            return new _CupertinoThemeDefaults(
                brightness:brightness,
                primaryColor:convertColor(primaryColor),
                primaryContrastingColor:convertColor(primaryContrastingColor),
                barBackgroundColor:convertColor(barBackgroundColor),
                scaffoldBackgroundColor:convertColor(scaffoldBackgroundColor),
                textThemeDefaults:resolveTextTheme ? textThemeDefaults?.resolveFrom(context, nullOk: nullOk) : textThemeDefaults
            );
        }
    }

  
    public class _CupertinoTextThemeDefaults {
        public _CupertinoTextThemeDefaults(
            Color labelColor = null,
            Color inactiveGray = null
        ) {
            this.labelColor = labelColor;
            this.inactiveGray = inactiveGray;
        }

        public readonly Color labelColor;
        public readonly Color inactiveGray;

        public _CupertinoTextThemeDefaults resolveFrom(BuildContext context, bool nullOk = false) {
            return new _CupertinoTextThemeDefaults(
                CupertinoDynamicColor.resolve(labelColor, context, nullOk: nullOk),
                CupertinoDynamicColor.resolve(inactiveGray, context, nullOk: nullOk)
            );
        }

        public CupertinoTextThemeData createDefaults(Color primaryColor ) {
            D.assert(primaryColor != null);
            return new _DefaultCupertinoTextThemeData(
                primaryColor: primaryColor,
                labelColor: labelColor,
                inactiveGray: inactiveGray
            );
        }
    }

    class _DefaultCupertinoTextThemeData : CupertinoTextThemeData {
        public _DefaultCupertinoTextThemeData(
            Color labelColor = null,
            Color inactiveGray = null,
            Color primaryColor = null
        ) : base(primaryColor: primaryColor) 
        {
            D.assert(labelColor != null);
            D.assert(inactiveGray != null);
            D.assert(primaryColor != null);
            
            this.labelColor = labelColor;
            this.inactiveGray = inactiveGray;

        }

        public readonly Color labelColor;
        public readonly Color inactiveGray;


        public override TextStyle textStyle {
            get {
                return base.textStyle.copyWith(color: labelColor);

            }
        }

        public override TextStyle tabLabelTextStyle {
            get {
                return base.tabLabelTextStyle.copyWith(color: inactiveGray);
            }
        }


        public override TextStyle navTitleTextStyle {
            get {
                return base.navTitleTextStyle.copyWith(color: labelColor);
            }
        }


        public override TextStyle navLargeTitleTextStyle {
            get {
                return base.navLargeTitleTextStyle.copyWith(color: labelColor);
            }
        }

    
        public override TextStyle  pickerTextStyle {
            get {
                return base.pickerTextStyle.copyWith(color: labelColor);
            }
        }

    
        public override TextStyle dateTimePickerTextStyle {
            get {
                return base.dateTimePickerTextStyle.copyWith(color: labelColor);
            }
        }
    }

    
}