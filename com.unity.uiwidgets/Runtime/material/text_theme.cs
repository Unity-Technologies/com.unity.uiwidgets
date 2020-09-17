using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class TextTheme : Diagnosticable, IEquatable<TextTheme> {
        public TextTheme(
            TextStyle display4 = null,
            TextStyle display3 = null,
            TextStyle display2 = null,
            TextStyle display1 = null,
            TextStyle headline = null,
            TextStyle title = null,
            TextStyle subhead = null,
            TextStyle body2 = null,
            TextStyle body1 = null,
            TextStyle caption = null,
            TextStyle button = null,
            TextStyle subtitle = null,
            TextStyle overline = null
        ) {
            this.display4 = display4;
            this.display3 = display3;
            this.display2 = display2;
            this.display1 = display1;
            this.headline = headline;
            this.title = title;
            this.subhead = subhead;
            this.body2 = body2;
            this.body1 = body1;
            this.caption = caption;
            this.button = button;
            this.subtitle = subtitle;
            this.overline = overline;
        }


        public readonly TextStyle display4;

        public readonly TextStyle display3;

        public readonly TextStyle display2;

        public readonly TextStyle display1;

        public readonly TextStyle headline;

        public readonly TextStyle title;

        public readonly TextStyle subhead;

        public readonly TextStyle body2;

        public readonly TextStyle body1;

        public readonly TextStyle caption;

        public readonly TextStyle button;

        public readonly TextStyle subtitle;

        public readonly TextStyle overline;


        public TextTheme copyWith(
            TextStyle display4 = null,
            TextStyle display3 = null,
            TextStyle display2 = null,
            TextStyle display1 = null,
            TextStyle headline = null,
            TextStyle title = null,
            TextStyle subhead = null,
            TextStyle body2 = null,
            TextStyle body1 = null,
            TextStyle caption = null,
            TextStyle button = null,
            TextStyle subtitle = null,
            TextStyle overline = null
        ) {
            return new TextTheme(
                display4: display4 ?? this.display4,
                display3: display3 ?? this.display3,
                display2: display2 ?? this.display2,
                display1: display1 ?? this.display1,
                headline: headline ?? this.headline,
                title: title ?? this.title,
                subhead: subhead ?? this.subhead,
                body2: body2 ?? this.body2,
                body1: body1 ?? this.body1,
                caption: caption ?? this.caption,
                button: button ?? this.button,
                subtitle: subtitle ?? this.subtitle,
                overline: overline ?? this.overline
            );
        }

        public TextTheme merge(TextTheme other) {
            if (other == null) {
                return this;
            }

            return copyWith(
                display4: display4?.merge(other.display4) ?? other.display4,
                display3: display3?.merge(other.display3) ?? other.display3,
                display2: display2?.merge(other.display2) ?? other.display2,
                display1: display1?.merge(other.display1) ?? other.display1,
                headline: headline?.merge(other.headline) ?? other.headline,
                title: title?.merge(other.title) ?? other.title,
                subhead: subhead?.merge(other.subhead) ?? other.subhead,
                body2: body2?.merge(other.body2) ?? other.body2,
                body1: body1?.merge(other.body1) ?? other.body1,
                caption: caption?.merge(other.caption) ?? other.caption,
                button: button?.merge(other.button) ?? other.button,
                subtitle: subtitle?.merge(other.subtitle) ?? other.subtitle,
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
                display4: display4?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                display3: display3?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                display2: display2?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                display1: display1?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                headline: headline?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                title: title?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                subhead: subhead?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                body2: body2?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                body1: body1?.apply(
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
                    fontSizeDelta: fontSizeDelta
                ),
                button: button?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                subtitle: subtitle?.apply(
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
                    fontSizeDelta: fontSizeDelta
                )
            );
        }

        public static TextTheme lerp(TextTheme a, TextTheme b, float t) {
            return new TextTheme(
                display4: TextStyle.lerp(a?.display4, b?.display4, t),
                display3: TextStyle.lerp(a?.display3, b?.display3, t),
                display2: TextStyle.lerp(a?.display2, b?.display2, t),
                display1: TextStyle.lerp(a?.display1, b?.display1, t),
                headline: TextStyle.lerp(a?.headline, b?.headline, t),
                title: TextStyle.lerp(a?.title, b?.title, t),
                subhead: TextStyle.lerp(a?.subhead, b?.subhead, t),
                body2: TextStyle.lerp(a?.body2, b?.body2, t),
                body1: TextStyle.lerp(a?.body1, b?.body1, t),
                caption: TextStyle.lerp(a?.caption, b?.caption, t),
                button: TextStyle.lerp(a?.button, b?.button, t),
                subtitle: TextStyle.lerp(a?.subtitle, b?.subtitle, t),
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

            return display4 == other.display4
                   && display3 == other.display3
                   && display2 == other.display2
                   && display1 == other.display1
                   && headline == other.headline
                   && title == other.title
                   && subhead == other.subhead
                   && body2 == other.body2
                   && body1 == other.body1
                   && caption == other.caption
                   && button == other.button
                   && subtitle == other.subtitle
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
                var hashCode = display4?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ display3?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ display2?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ display1?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ headline?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ title?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ subhead?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ body2?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ body1?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ caption?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ button?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ subtitle?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ overline?.GetHashCode() ?? 0;

                _cachedHashCode = hashCode;
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            TextTheme defaultTheme = new Typography().black;
            properties.add(new DiagnosticsProperty<TextStyle>("display4", display4,
                defaultValue: defaultTheme.display4));
            properties.add(new DiagnosticsProperty<TextStyle>("display3", display3,
                defaultValue: defaultTheme.display3));
            properties.add(new DiagnosticsProperty<TextStyle>("display2", display2,
                defaultValue: defaultTheme.display2));
            properties.add(new DiagnosticsProperty<TextStyle>("display1", display1,
                defaultValue: defaultTheme.display1));
            properties.add(new DiagnosticsProperty<TextStyle>("headline", headline,
                defaultValue: defaultTheme.headline));
            properties.add(new DiagnosticsProperty<TextStyle>("title", title, defaultValue: defaultTheme.title));
            properties.add(
                new DiagnosticsProperty<TextStyle>("subhead", subhead, defaultValue: defaultTheme.subhead));
            properties.add(new DiagnosticsProperty<TextStyle>("body2", body2, defaultValue: defaultTheme.body2));
            properties.add(new DiagnosticsProperty<TextStyle>("body1", body1, defaultValue: defaultTheme.body1));
            properties.add(
                new DiagnosticsProperty<TextStyle>("caption", caption, defaultValue: defaultTheme.caption));
            properties.add(
                new DiagnosticsProperty<TextStyle>("button", button, defaultValue: defaultTheme.button));
            properties.add(new DiagnosticsProperty<TextStyle>("subtitle)", subtitle,
                defaultValue: defaultTheme.subtitle));
            properties.add(new DiagnosticsProperty<TextStyle>("overline", overline,
                defaultValue: defaultTheme.overline));
        }
    }
}