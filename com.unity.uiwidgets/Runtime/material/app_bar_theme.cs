using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Brightness = Unity.UIWidgets.ui.Brightness;

namespace Unity.UIWidgets.material {
    public class AppBarTheme : Diagnosticable,IEquatable<AppBarTheme> {
        public AppBarTheme(
            Brightness? brightness = null,
            Color color = null,
            float? elevation = null,
            IconThemeData iconTheme = null,
            IconThemeData actionsIconTheme = null,
            TextTheme textTheme = null
        ) {
            this.brightness = brightness;
            this.color = color;
            this.elevation = elevation;
            this.iconTheme = iconTheme;
            this.actionsIconTheme = actionsIconTheme;
            this.textTheme = textTheme;
        }

        public readonly Brightness? brightness;

        public readonly Color color;

        public readonly float? elevation;

        public readonly IconThemeData iconTheme;

        public readonly IconThemeData actionsIconTheme;

        public readonly TextTheme textTheme;

        AppBarTheme copyWith(
            Brightness? brightness = null,
            Color color = null,
            float? elevation = null,
            IconThemeData iconTheme = null,
            IconThemeData actionsIconTheme = null,
            TextTheme textTheme = null
        ) {
            return new AppBarTheme(
                brightness: brightness ?? this.brightness,
                color: color ?? this.color,
                elevation: elevation ?? this.elevation,
                iconTheme: iconTheme ?? this.iconTheme,
                actionsIconTheme: actionsIconTheme ?? this.actionsIconTheme,
                textTheme: textTheme ?? this.textTheme
            );
        }

        public static AppBarTheme of(BuildContext context) {
            return Theme.of(context).appBarTheme;
        }

        public static AppBarTheme lerp(AppBarTheme a, AppBarTheme b, float t) {
            return new AppBarTheme(
                brightness: t < 0.5f ? a?.brightness : b?.brightness,
                color: Color.lerp(a?.color, b?.color, t),
                elevation: MathUtils.lerpFloat(a?.elevation ?? 0.0f, b?.elevation ?? 0.0f, t),
                iconTheme: IconThemeData.lerp(a?.iconTheme, b?.iconTheme, t),
                actionsIconTheme: IconThemeData.lerp(a?.actionsIconTheme, b?.actionsIconTheme, t),
                textTheme: TextTheme.lerp(a?.textTheme, b?.textTheme, t)
            );
        }

        public override int GetHashCode() {
            var hashCode = brightness?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ color?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ elevation?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ iconTheme?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ actionsIconTheme?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ textTheme?.GetHashCode() ?? 0;
            return hashCode;
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

            return Equals((AppBarTheme) obj);
        }

        public static bool operator ==(AppBarTheme left, AppBarTheme right) {
            return Equals(left, right);
        }

        public static bool operator !=(AppBarTheme left, AppBarTheme right) {
            return !Equals(left, right);
        }

        public bool Equals(AppBarTheme other) {
            return other.brightness == brightness
                   && other.color == color
                   && other.elevation == elevation
                   && other.iconTheme == iconTheme
                   && other.actionsIconTheme == actionsIconTheme
                   && other.textTheme == textTheme;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Brightness?>("brightness", brightness, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("color", color, defaultValue: null));
            properties.add(new DiagnosticsProperty<float?>("elevation", elevation, defaultValue: null));
            properties.add(new DiagnosticsProperty<IconThemeData>("iconTheme", iconTheme, defaultValue: null));
            properties.add(new DiagnosticsProperty<IconThemeData>("actionsIconTheme", actionsIconTheme, defaultValue: null));
            properties.add(new DiagnosticsProperty<TextTheme>("textTheme", textTheme, defaultValue: null));
        }
    }
}