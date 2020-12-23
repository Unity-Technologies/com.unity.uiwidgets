using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class TextTheme : Diagnosticable, IEquatable<TextTheme> {
        public TextTheme(
            TextStyle headline1 = null,
            TextStyle headline2 = null,
            TextStyle headline3 = null,
            TextStyle headline4 = null,
            TextStyle headline5 = null,
            TextStyle headline6 = null,
            TextStyle subtitle1 = null,
            TextStyle subtitle2 = null,
            TextStyle bodyText1 = null,
            TextStyle bodyText2 = null,
            
            TextStyle caption = null,
            TextStyle button = null,
            TextStyle subtitle = null,
            TextStyle overline = null,
            // deprecated 
            TextStyle display4 = null,
            TextStyle display3 = null,
            TextStyle display2 = null,
            TextStyle display1 = null,
            TextStyle headline = null,
            TextStyle title = null,
            TextStyle subhead = null,
            TextStyle body2 = null,
            TextStyle body1 = null
        ) {
            D.assert(
                (headline1 == null && headline2 == null && headline3 == null && headline4 == null &&
                 headline5 == null && headline6 == null &&
                 subtitle1 == null && subtitle2 == null &&
                 bodyText1 == null && bodyText2 == null) ||
                (display4 == null && display3 == null && display2 == null && display1 == null && headline == null &&
                 title == null &&
                 subhead == null && subtitle == null &&
                 body2 == null && body1 == null),
                () => "Cannot mix 2014 and 2018 terms in call to TextTheme() constructor.");
            headline1 = headline1 ?? display4;
            headline2 = headline2 ?? display3;
            headline3 = headline3 ?? display2;
            headline4 = headline4 ?? display1;
            headline5 = headline5 ?? headline;
            headline6 = headline6 ?? title;
            subtitle1 = subtitle1 ?? subhead;
            subtitle2 = subtitle2 ?? subtitle;
            bodyText1 = bodyText1 ?? body2;
            bodyText2 = bodyText2 ?? body1;
            this.caption = caption;
            this.button = button; 
            this.overline = overline;
        }
        
        public readonly TextStyle headline1 ;
        public readonly TextStyle headline2 ;
        public readonly TextStyle headline3 ;
        public readonly TextStyle headline4 ;
        public readonly TextStyle headline5 ;
        public readonly TextStyle headline6 ;
        public readonly TextStyle subtitle1 ;
        public readonly TextStyle subtitle2 ;
        public readonly TextStyle bodyText1 ;
        public readonly TextStyle bodyText2 ;

        public TextStyle display4 {
            get { return headline1; }
        }

        public TextStyle display3{
            get { return headline2; }
        }

        public TextStyle display2{
            get { return headline3; }
        }

        public TextStyle display1{
            get { return headline4; }
        }

        public TextStyle headline{
            get { return headline5; }
        }

        public TextStyle title{
            get { return headline6; }
        }

        public TextStyle subhead{
            get { return subtitle1; }
        }
        public TextStyle subtitle{
            get { return subtitle2;}
        }

        public  TextStyle body2{
            get { return bodyText1;}
        }

        public TextStyle body1{
            get { return bodyText2;}
        }

        public readonly TextStyle caption;

        public readonly TextStyle button;

        

        public readonly TextStyle overline;


        public TextTheme copyWith(
            TextStyle headline1 = null,
            TextStyle headline2 = null,
            TextStyle headline3 = null,
            TextStyle headline4 = null,
            TextStyle headline5 = null,
            TextStyle headline6 = null,
            TextStyle subtitle1 = null,
            TextStyle subtitle2 = null,
            TextStyle bodyText1 = null,
            TextStyle bodyText2 = null,
            
            TextStyle caption = null,
            TextStyle button = null,
            TextStyle subtitle = null,
            TextStyle overline = null,
            // deprecated 
            TextStyle display4 = null,
            TextStyle display3 = null,
            TextStyle display2 = null,
            TextStyle display1 = null,
            TextStyle headline = null,
            TextStyle title = null,
            TextStyle subhead = null,
            TextStyle body2 = null,
            TextStyle body1 = null
        ) {
            D.assert(
                (headline1 == null && headline2 == null && headline3 == null && headline4 == null && headline5 == null && headline6 == null &&
                 subtitle1 == null && subtitle2 == null &&
                 bodyText1 == null && bodyText2 == null) ||
                (display4 == null && display3 == null && display2 == null && display1 == null && headline == null && title == null &&
                 subhead == null && subtitle == null &&
                 body2 == null && body1 == null), ()=>"Cannot mix 2014 and 2018 terms in call to TextTheme.copyWith().");
            return new TextTheme(
                headline1: headline1 ?? display4 ?? this.headline1,
                headline2: headline2 ?? display3 ?? this.headline2,
                headline3: headline3 ?? display2 ?? this.headline3,
                headline4: headline4 ?? display1 ?? this.headline4,
                headline5: headline5 ?? headline ?? this.headline5,
                headline6: headline6 ?? title ?? this.headline6,
                subtitle1: subtitle1 ?? subhead ?? this.subtitle1,
                subtitle2: subtitle2 ?? subtitle ?? this.subtitle2,
                bodyText1: bodyText1 ?? body2 ?? this.bodyText1,
                bodyText2: bodyText2 ?? body1 ?? this.bodyText2,
                caption: caption ?? this.caption,
                button: button ?? this.button,
                overline: overline ?? this.overline
            );
        }

        public TextTheme merge(TextTheme other) {
            if (other == null)
                return this;
            return copyWith(
                headline1: headline1?.merge(other.headline1) ?? other.headline1,
                headline2: headline2?.merge(other.headline2) ?? other.headline2,
                headline3: headline3?.merge(other.headline3) ?? other.headline3,
                headline4: headline4?.merge(other.headline4) ?? other.headline4,
                headline5: headline5?.merge(other.headline5) ?? other.headline5,
                headline6: headline6?.merge(other.headline6) ?? other.headline6,
                subtitle1: subtitle1?.merge(other.subtitle1) ?? other.subtitle1,
                subtitle2: subtitle2?.merge(other.subtitle2) ?? other.subtitle2,
                bodyText1: bodyText1?.merge(other.bodyText1) ?? other.bodyText1,
                bodyText2: bodyText2?.merge(other.bodyText2) ?? other.bodyText2,
                caption: caption?.merge(other.caption) ?? other.caption,
                button: button?.merge(other.button) ?? other.button,
                overline: overline?.merge(other.overline) ?? other.overline
            );
        }
        public TextTheme apply(
            string fontFamily = null,
            float fontSizeFactor = 1.0f,
            float fontSizeDelta = 0.0f,
            Color displayColor = null,
            Color bodyColor = null,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null
        ) { 
            
            return new TextTheme(
                headline1: headline1?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ), 
                headline2: headline2?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ), 
                headline3: headline3?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily, 
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ), 
                headline4: headline4?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ), 
                headline5: headline5?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor, 
                    fontSizeDelta: fontSizeDelta
                ),
                headline6: headline6?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                subtitle1: subtitle1?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                    ),
                subtitle2: subtitle2?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                    ),
                bodyText1: bodyText1?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta),
                bodyText2: bodyText2?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                    ),
                caption: caption?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta),
                button: button?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                    ),
                overline: overline?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta)
                );
        }

        public static TextTheme lerp(TextTheme a, TextTheme b, float t) {
            D.assert(t != null);
            return new TextTheme(
                headline1: TextStyle.lerp(a?.headline1, b?.headline1, t),
                headline2: TextStyle.lerp(a?.headline2, b?.headline2, t),
                headline3: TextStyle.lerp(a?.headline3, b?.headline3, t),
                headline4: TextStyle.lerp(a?.headline4, b?.headline4, t),
                headline5: TextStyle.lerp(a?.headline5, b?.headline5, t),
                headline6: TextStyle.lerp(a?.headline6, b?.headline6, t),
                subtitle1: TextStyle.lerp(a?.subtitle1, b?.subtitle1, t),
                subtitle2: TextStyle.lerp(a?.subtitle2, b?.subtitle2, t),
                bodyText1: TextStyle.lerp(a?.bodyText1, b?.bodyText1, t),
                bodyText2: TextStyle.lerp(a?.bodyText2, b?.bodyText2, t),
                caption: TextStyle.lerp(a?.caption, b?.caption, t),
                button: TextStyle.lerp(a?.button, b?.button, t),
                overline: TextStyle.lerp(a?.overline, b?.overline, t)
            );
        }

        public bool Equals(TextTheme other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return  other is TextTheme
                    && headline1 == other.headline1
                    && headline2 == other.headline2
                    && headline3 == other.headline3
                    && headline4 == other.headline4
                    && headline5 == other.headline5
                    && headline6 == other.headline6
                    && subtitle1 == other.subtitle1
                    && subtitle2 == other.subtitle2
                    && bodyText1 == other.bodyText1
                    && bodyText2 == other.bodyText2
                    && caption == other.caption
                    && button == other.button
                    && overline == other.overline;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextTheme) obj);
        }

        public static bool operator ==(TextTheme left, TextTheme right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextTheme left, TextTheme right) {
            return !Equals(left, right);
        }

        int? _cachedHashCode = null;
        public override int GetHashCode() {
            if (_cachedHashCode != null) {
                return _cachedHashCode.Value;
            }
            unchecked {
                var hashCode = headline1?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ headline2?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ headline3?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ headline4?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ headline5?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ headline6?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ subtitle1?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ subtitle2?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ bodyText1?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ bodyText2?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ caption?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ button?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ overline?.GetHashCode() ?? 0;

                _cachedHashCode = hashCode;
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            TextTheme defaultTheme = Typography.material2018().black;
            properties.add(new DiagnosticsProperty<TextStyle>("headline1", headline1, defaultValue: defaultTheme.headline1));
            properties.add(new DiagnosticsProperty<TextStyle>("headline2", headline2, defaultValue: defaultTheme.headline2));
            properties.add(new DiagnosticsProperty<TextStyle>("headline3", headline3, defaultValue: defaultTheme.headline3));
            properties.add(new DiagnosticsProperty<TextStyle>("headline4", headline4, defaultValue: defaultTheme.headline4));
            properties.add(new DiagnosticsProperty<TextStyle>("headline5", headline5, defaultValue: defaultTheme.headline5));
            properties.add(new DiagnosticsProperty<TextStyle>("headline6", headline6, defaultValue: defaultTheme.headline6));
            properties.add(new DiagnosticsProperty<TextStyle>("subtitle1", subtitle1, defaultValue: defaultTheme.subtitle1));
            properties.add(new DiagnosticsProperty<TextStyle>("subtitle2", subtitle2, defaultValue: defaultTheme.subtitle2));
            properties.add(new DiagnosticsProperty<TextStyle>("bodyText1", bodyText1, defaultValue: defaultTheme.bodyText1));
            properties.add(new DiagnosticsProperty<TextStyle>("bodyText2", bodyText2, defaultValue: defaultTheme.bodyText2));
            properties.add(new DiagnosticsProperty<TextStyle>("caption", caption, defaultValue: defaultTheme.caption));
            properties.add(new DiagnosticsProperty<TextStyle>("button", button, defaultValue: defaultTheme.button));
            properties.add(new DiagnosticsProperty<TextStyle>("overline", overline, defaultValue: defaultTheme.overline));
        }
    }
}