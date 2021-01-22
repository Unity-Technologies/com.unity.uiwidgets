using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class MaterialBannerThemeData : Diagnosticable, IEquatable<MaterialBannerThemeData> {
        public MaterialBannerThemeData(
            Color backgroundColor = null,
            TextStyle contentTextStyle = null,
            EdgeInsetsGeometry padding = null,
            EdgeInsetsGeometry leadingPadding = null
        ) {
            this.backgroundColor = backgroundColor;
            this.contentTextStyle = contentTextStyle;
            this.padding = padding;
            this.leadingPadding = leadingPadding;
        }

        public readonly Color backgroundColor;

        public readonly TextStyle contentTextStyle;

        public readonly EdgeInsetsGeometry padding;

        public readonly EdgeInsetsGeometry leadingPadding;

        public MaterialBannerThemeData copyWith(
            Color backgroundColor = null,
            TextStyle contentTextStyle = null,
            EdgeInsetsGeometry padding = null,
            EdgeInsetsGeometry leadingPadding = null
        ) {
            return new MaterialBannerThemeData(
                backgroundColor: backgroundColor ?? this.backgroundColor,
                contentTextStyle: contentTextStyle ?? this.contentTextStyle,
                padding: padding ?? this.padding,
                leadingPadding: leadingPadding ?? this.leadingPadding
            );
        }

        public static MaterialBannerThemeData lerp(MaterialBannerThemeData a, MaterialBannerThemeData b, float t) {
            return new MaterialBannerThemeData(
                backgroundColor: Color.lerp(a?.backgroundColor, b?.backgroundColor, t),
                contentTextStyle: TextStyle.lerp(a?.contentTextStyle, b?.contentTextStyle, t),
                padding: EdgeInsetsGeometry.lerp(a?.padding, b?.padding, t),
                leadingPadding: EdgeInsetsGeometry.lerp(a?.leadingPadding, b?.leadingPadding, t)
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new ColorProperty("backgroundColor", backgroundColor, defaultValue: null));
            properties.add(
                new DiagnosticsProperty<TextStyle>("contentTextStyle", contentTextStyle, defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding, defaultValue: null));
            properties.add(
                new DiagnosticsProperty<EdgeInsetsGeometry>("leadingPadding", leadingPadding, defaultValue: null));
        }

        public static bool operator ==(MaterialBannerThemeData self, MaterialBannerThemeData other) {
            return Equals(self, other);
        }

        public static bool operator !=(MaterialBannerThemeData self, MaterialBannerThemeData other) {
            return !Equals(self, other);
        }

        public bool Equals(MaterialBannerThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(backgroundColor, other.backgroundColor) && Equals(contentTextStyle, other.contentTextStyle) &&
                   Equals(padding, other.padding) && Equals(leadingPadding, other.leadingPadding);
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

            return Equals((MaterialBannerThemeData) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (backgroundColor != null ? backgroundColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (contentTextStyle != null ? contentTextStyle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (padding != null ? padding.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (leadingPadding != null ? leadingPadding.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class MaterialBannerTheme : InheritedTheme {
        public MaterialBannerTheme(
            Key key = null,
            MaterialBannerThemeData data = null,
            Widget child = null
        ) : base(key: key, child: child) {
            this.data = data;
        }

        public readonly MaterialBannerThemeData data;

        public static MaterialBannerThemeData of(BuildContext context) {
            MaterialBannerTheme bannerTheme = context.dependOnInheritedWidgetOfExactType<MaterialBannerTheme>();
            return bannerTheme?.data ?? Theme.of(context).bannerTheme;
        }

        public override Widget wrap(BuildContext context, Widget child) {
            MaterialBannerTheme ancestorTheme = context.findAncestorWidgetOfExactType<MaterialBannerTheme>();
            return ReferenceEquals(this, ancestorTheme) ? child : new MaterialBannerTheme(data: data, child: child);
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) =>
            oldWidget is MaterialBannerTheme materialBannerTheme && data != materialBannerTheme.data;
    }
}