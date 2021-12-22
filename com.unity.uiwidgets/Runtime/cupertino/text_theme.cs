using Unity.UIWidgets.foundation;
//using Unity.UIWidgets.material;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Brightness = Unity.UIWidgets.ui.Brightness;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    static class CupertinoTextThemeDataUtils {
        
        public static readonly TextStyle _kDefaultTextStyle = new TextStyle(
          inherit: false,
          fontFamily: ".SF Pro Text",
          fontSize: 17.0f,
          letterSpacing: -0.41f,
          color: CupertinoColors.label,
          decoration: TextDecoration.none
        );
        public static readonly TextStyle _kDefaultActionTextStyle = new TextStyle(
          inherit: false,
          fontFamily: ".SF Pro Text",
          fontSize: 17.0f,
          letterSpacing: -0.41f,
          color: CupertinoColors.activeBlue,
          decoration: TextDecoration.none
        );

        public static readonly TextStyle _kDefaultTabLabelTextStyle = new TextStyle(
          inherit: false,
          fontFamily: ".SF Pro Text",
          fontSize: 10.0f,
          letterSpacing: -0.24f,
          color: CupertinoColors.inactiveGray
        );

        public static readonly TextStyle _kDefaultMiddleTitleTextStyle = new TextStyle(
          inherit: false,
          fontFamily: ".SF Pro Text",
          fontSize: 17.0f,
          fontWeight: FontWeight.w600,
          letterSpacing: -0.41f,
          color: CupertinoColors.label
        );

        public static readonly TextStyle _kDefaultLargeTitleTextStyle = new TextStyle(
          inherit: false,
          fontFamily: ".SF Pro Display",
          fontSize: 34.0f,
          fontWeight: FontWeight.w700,
          letterSpacing: 0.41f,
          color: CupertinoColors.label
        );

        public static readonly TextStyle _kDefaultPickerTextStyle = new TextStyle(
          inherit: false,
          fontFamily: ".SF Pro Display",
          fontSize: 21.0f,
          fontWeight: FontWeight.w400,
          letterSpacing: -0.41f,
          color: CupertinoColors.label
        );

        public static readonly TextStyle _kDefaultDateTimePickerTextStyle = new TextStyle(
          inherit: false,
          fontFamily: ".SF Pro Display",
          fontSize: 21.0f,
          fontWeight: FontWeight.normal,
          color: CupertinoColors.label
        );
        public static TextStyle _resolveTextStyle(TextStyle style, BuildContext context, bool nullOk) {
            return style?.copyWith(
                color: CupertinoDynamicColor.resolve(style?.color, context, nullOk: nullOk),
                backgroundColor: CupertinoDynamicColor.resolve(style?.backgroundColor, context, nullOk: nullOk),
                decorationColor: CupertinoDynamicColor.resolve(style?.decorationColor, context, nullOk: nullOk)
            );
        }
    }


    public class CupertinoTextThemeData : Diagnosticable {
        public CupertinoTextThemeData(
            _TextThemeDefaultsBuilder defaults = null,
            Color primaryColor = null,
            TextStyle textStyle = null,
            TextStyle actionTextStyle = null,
            TextStyle tabLabelTextStyle = null,
            TextStyle navTitleTextStyle = null,
            TextStyle navLargeTitleTextStyle = null,
            TextStyle navActionTextStyle = null,
            TextStyle pickerTextStyle = null,
            TextStyle dateTimePickerTextStyle = null
        ) {
            _defaults = defaults ?? new _TextThemeDefaultsBuilder(CupertinoColors.label, CupertinoColors.inactiveGray);
            _primaryColor = primaryColor ?? CupertinoColors.systemBlue;
            D.assert((_navActionTextStyle != null && _actionTextStyle != null) || _primaryColor != null);
            _textStyle = textStyle;
            _actionTextStyle = actionTextStyle;
            _tabLabelTextStyle = tabLabelTextStyle;
            _navTitleTextStyle = navTitleTextStyle;
            _navLargeTitleTextStyle = navLargeTitleTextStyle;
            _navActionTextStyle = navActionTextStyle;
            _pickerTextStyle = pickerTextStyle;
            _dateTimePickerTextStyle = dateTimePickerTextStyle;
        }
        public static CupertinoTextThemeData _raw(
            _TextThemeDefaultsBuilder _defaults,
            Color primaryColor,
            TextStyle textStyle,
            TextStyle actionTextStyle ,
            TextStyle tabLabelTextStyle ,
            TextStyle navTitleTextStyle ,
            TextStyle navLargeTitleTextStyle ,
            TextStyle navActionTextStyle ,
            TextStyle pickerTextStyle,
            TextStyle dateTimePickerTextStyle 
        ) {
            var textThemeData = new CupertinoTextThemeData(
                _defaults,
                primaryColor,
                textStyle,
                actionTextStyle,
                tabLabelTextStyle,
                navTitleTextStyle,
                navLargeTitleTextStyle,
                navActionTextStyle,
                pickerTextStyle,
                dateTimePickerTextStyle
                );
            return textThemeData;

        }

        public _TextThemeDefaultsBuilder _defaults;
        readonly Color _primaryColor;
        readonly Brightness? _brightness;
        
        readonly TextStyle _textStyle; 
        public virtual TextStyle textStyle {
            get {
                return _textStyle ?? _defaults.textStyle;
            }
        }

        readonly TextStyle _actionTextStyle;

        public virtual TextStyle actionTextStyle {
            get {
                return _actionTextStyle ?? _defaults.actionTextStyle(primaryColor: _primaryColor);
                       
            }
        }

        readonly TextStyle _tabLabelTextStyle;

        public virtual TextStyle tabLabelTextStyle {
            get { return _tabLabelTextStyle ??  _defaults.tabLabelTextStyle;}
        }

        readonly TextStyle _navTitleTextStyle;

        public virtual TextStyle navTitleTextStyle {
            get {
                return _navTitleTextStyle ??  _defaults.navTitleTextStyle;
                       
            }
        }

        readonly TextStyle _navLargeTitleTextStyle;

        /// Typography of large titles in sliver navigation bars.
        public virtual TextStyle navLargeTitleTextStyle {
            get {
                return _navLargeTitleTextStyle ??  _defaults.navLargeTitleTextStyle;
            }
        }

        readonly TextStyle _navActionTextStyle;

        public virtual TextStyle navActionTextStyle {
            get {
                return _navActionTextStyle ??  _defaults.navActionTextStyle(primaryColor: _primaryColor);
            }
        }
        readonly TextStyle _pickerTextStyle;

        public virtual TextStyle pickerTextStyle {
            get {
                return _pickerTextStyle ?? _defaults.pickerTextStyle;
            }
        }

        readonly TextStyle _dateTimePickerTextStyle;

        public virtual TextStyle dateTimePickerTextStyle {
            get {
                return _dateTimePickerTextStyle ?? _defaults.dateTimePickerTextStyle;
            }
        }
        
        public CupertinoTextThemeData resolveFrom(BuildContext context,  bool nullOk = false ) {
            return new CupertinoTextThemeData(
                _defaults?.resolveFrom(context, nullOk),
                CupertinoDynamicColor.resolve(_primaryColor, context, nullOk: nullOk),
                CupertinoTextThemeDataUtils._resolveTextStyle(_textStyle, context, nullOk),
                CupertinoTextThemeDataUtils._resolveTextStyle(_actionTextStyle, context, nullOk),
                CupertinoTextThemeDataUtils._resolveTextStyle(_tabLabelTextStyle, context, nullOk),
                CupertinoTextThemeDataUtils._resolveTextStyle(_navTitleTextStyle, context, nullOk),
                CupertinoTextThemeDataUtils._resolveTextStyle(_navLargeTitleTextStyle, context, nullOk),
                CupertinoTextThemeDataUtils._resolveTextStyle(_navActionTextStyle, context, nullOk),
                CupertinoTextThemeDataUtils._resolveTextStyle(_pickerTextStyle, context, nullOk),
                CupertinoTextThemeDataUtils._resolveTextStyle(_dateTimePickerTextStyle, context, nullOk)
            );
        }


        public CupertinoTextThemeData copyWith(
            Color primaryColor = null,
            TextStyle textStyle = null,
            TextStyle actionTextStyle = null,
            TextStyle tabLabelTextStyle = null,
            TextStyle navTitleTextStyle = null,
            TextStyle navLargeTitleTextStyle = null,
            TextStyle navActionTextStyle = null,
            TextStyle pickerTextStyle = null,
            TextStyle dateTimePickerTextStyle = null
        ) {
            return new CupertinoTextThemeData(
                _defaults,
                primaryColor: primaryColor ?? _primaryColor,
                textStyle: textStyle ?? _textStyle,
                actionTextStyle: actionTextStyle ?? _actionTextStyle,
                tabLabelTextStyle: tabLabelTextStyle ?? _tabLabelTextStyle,
                navTitleTextStyle: navTitleTextStyle ?? _navTitleTextStyle,
                navLargeTitleTextStyle: navLargeTitleTextStyle ?? _navLargeTitleTextStyle,
                navActionTextStyle: navActionTextStyle ?? _navActionTextStyle,
                pickerTextStyle ?? _pickerTextStyle,
                dateTimePickerTextStyle ?? _dateTimePickerTextStyle
            );
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            CupertinoTextThemeData defaultData = new CupertinoTextThemeData();
            properties.add(new DiagnosticsProperty<TextStyle>("textStyle", textStyle, defaultValue: defaultData.textStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("actionTextStyle", actionTextStyle, defaultValue: defaultData.actionTextStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("tabLabelTextStyle", tabLabelTextStyle, defaultValue: defaultData.tabLabelTextStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("navTitleTextStyle", navTitleTextStyle, defaultValue: defaultData.navTitleTextStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("navLargeTitleTextStyle", navLargeTitleTextStyle, defaultValue: defaultData.navLargeTitleTextStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("navActionTextStyle", navActionTextStyle, defaultValue: defaultData.navActionTextStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("pickerTextStyle", pickerTextStyle, defaultValue: defaultData.pickerTextStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("dateTimePickerTextStyle", dateTimePickerTextStyle, defaultValue: defaultData.dateTimePickerTextStyle));
        }
    }
    public class _TextThemeDefaultsBuilder {
        public _TextThemeDefaultsBuilder(
            Color labelColor = null,
            Color inactiveGrayColor = null
        ) {
            this.labelColor = labelColor;
            this.inactiveGrayColor = inactiveGrayColor;
            D.assert(labelColor != null);
            D.assert(inactiveGrayColor != null);
        }

        public readonly Color labelColor;
        public readonly Color inactiveGrayColor;

        public static TextStyle _applyLabelColor(TextStyle original, Color color) {
            return original?.color == color
                ?  original
                :  original?.copyWith(color: color);
        }

        public TextStyle textStyle {
            get {
                return  _applyLabelColor(CupertinoTextThemeDataUtils._kDefaultTextStyle, labelColor);
            }
        }
       
        public TextStyle tabLabelTextStyle 
        {
            get {
                return  _applyLabelColor(CupertinoTextThemeDataUtils._kDefaultTabLabelTextStyle, inactiveGrayColor);
            }
        }
            
        public TextStyle navTitleTextStyle {
            get {
                return  _applyLabelColor(CupertinoTextThemeDataUtils._kDefaultMiddleTitleTextStyle, labelColor);
            }
        }
           
        public TextStyle navLargeTitleTextStyle {
            get {
                return _applyLabelColor(CupertinoTextThemeDataUtils._kDefaultLargeTitleTextStyle, labelColor);
            }
        }
           
        public TextStyle pickerTextStyle {
            get {
                return _applyLabelColor(CupertinoTextThemeDataUtils._kDefaultPickerTextStyle, labelColor);
            }
        }
            
        public TextStyle dateTimePickerTextStyle {
            get {
                return  _applyLabelColor(CupertinoTextThemeDataUtils._kDefaultDateTimePickerTextStyle, labelColor);
            }
        }
           

        public TextStyle actionTextStyle( Color primaryColor = null) => CupertinoTextThemeDataUtils._kDefaultActionTextStyle.copyWith(color: primaryColor);
        public TextStyle navActionTextStyle( Color primaryColor = null) => actionTextStyle(primaryColor: primaryColor);

        public _TextThemeDefaultsBuilder resolveFrom(BuildContext context, bool nullOk) {
            Color resolvedLabelColor = CupertinoDynamicColor.resolve(labelColor, context, nullOk: nullOk);
            Color resolvedInactiveGray = CupertinoDynamicColor.resolve(inactiveGrayColor, context, nullOk: nullOk);
            return resolvedLabelColor == labelColor && resolvedInactiveGray == CupertinoColors.inactiveGray
                ? this
                : new _TextThemeDefaultsBuilder(resolvedLabelColor, resolvedInactiveGray);
        }
    }

}