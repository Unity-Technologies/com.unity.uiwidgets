using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Brightness = Unity.UIWidgets.ui.Brightness;
using TextStyle = Unity.UIWidgets.painting.TextStyle;


namespace Unity.UIWidgets.material {
    public class NavigationRailThemeData : Diagnosticable,IEquatable<NavigationRailThemeData> {
        public NavigationRailThemeData(
            Color backgroundColor = null,
            float? elevation = null,
            TextStyle unselectedLabelTextStyle = null, 
            TextStyle selectedLabelTextStyle = null,
            IconThemeData unselectedIconTheme = null,
            IconThemeData selectedIconTheme = null,
            float? groupAlignment = null,
            NavigationRailLabelType? labelType = null
            ) {
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.unselectedLabelTextStyle = unselectedLabelTextStyle;
            this.selectedLabelTextStyle = selectedLabelTextStyle;
            this.unselectedIconTheme = unselectedIconTheme;
            this.selectedIconTheme = selectedIconTheme;
            this.groupAlignment = groupAlignment;
            this.labelType = labelType;
        }

        public readonly Color backgroundColor;
        public readonly float? elevation;
        public readonly TextStyle unselectedLabelTextStyle; 
        public readonly TextStyle selectedLabelTextStyle;
        public readonly IconThemeData unselectedIconTheme;
        public readonly IconThemeData selectedIconTheme;
        public readonly float? groupAlignment;
        public readonly NavigationRailLabelType? labelType;
        
        public NavigationRailThemeData copyWith(
            Color backgroundColor = null,
            float? elevation = null,
            TextStyle unselectedLabelTextStyle = null, 
            TextStyle selectedLabelTextStyle = null,
            IconThemeData unselectedIconTheme = null,
            IconThemeData selectedIconTheme = null,
            float? groupAlignment = null,
            NavigationRailLabelType? labelType = null
            ) { 
            return new NavigationRailThemeData(
                backgroundColor: backgroundColor ?? this.backgroundColor,
                elevation: elevation ?? this.elevation,
                unselectedLabelTextStyle: unselectedLabelTextStyle ?? this.unselectedLabelTextStyle,
                selectedLabelTextStyle: selectedLabelTextStyle ?? this.selectedLabelTextStyle,
                unselectedIconTheme: unselectedIconTheme ?? this.unselectedIconTheme,
                selectedIconTheme: selectedIconTheme ?? this.selectedIconTheme,
                groupAlignment: groupAlignment ?? this.groupAlignment,
                labelType: labelType ?? this.labelType
            );
        } 
        public static NavigationRailThemeData lerp(NavigationRailThemeData a, NavigationRailThemeData b, float t) {
            D.assert(t != null);
            if (a == null && b == null)
              return null;
            return new NavigationRailThemeData(
              backgroundColor: Color.lerp(a?.backgroundColor, b?.backgroundColor, t),
              elevation: MathUtils.lerpFloat((float)a?.elevation.Value, (float)b?.elevation.Value, t),
              unselectedLabelTextStyle: TextStyle.lerp(a?.unselectedLabelTextStyle, b?.unselectedLabelTextStyle, t),
              selectedLabelTextStyle: TextStyle.lerp(a?.selectedLabelTextStyle, b?.selectedLabelTextStyle, t),
              unselectedIconTheme: IconThemeData.lerp(a?.unselectedIconTheme, b?.unselectedIconTheme, t),
              selectedIconTheme: IconThemeData.lerp(a?.selectedIconTheme, b?.selectedIconTheme, t),
              groupAlignment: MathUtils.lerpFloat((float)a?.groupAlignment.Value, (float)b?.groupAlignment.Value, t),
              labelType: t < 0.5 ? a?.labelType : b?.labelType
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) { 
            base.debugFillProperties(properties); 
            NavigationRailThemeData defaultData = new NavigationRailThemeData();
            properties.add( new ColorProperty("backgroundColor", backgroundColor, defaultValue: defaultData.backgroundColor));
            properties.add( new FloatProperty("elevation", elevation, defaultValue: defaultData.elevation));
            properties.add( new DiagnosticsProperty<TextStyle>("unselectedLabelTextStyle", unselectedLabelTextStyle, defaultValue: defaultData.unselectedLabelTextStyle));
            properties.add( new DiagnosticsProperty<TextStyle>("selectedLabelTextStyle", selectedLabelTextStyle, defaultValue: defaultData.selectedLabelTextStyle));
            properties.add( new DiagnosticsProperty<IconThemeData>("unselectedIconTheme", unselectedIconTheme, defaultValue: defaultData.unselectedIconTheme));
            properties.add( new DiagnosticsProperty<IconThemeData>("selectedIconTheme", selectedIconTheme, defaultValue: defaultData.selectedIconTheme));
            properties.add( new FloatProperty("groupAlignment", groupAlignment, defaultValue: defaultData.groupAlignment));
            properties.add( new DiagnosticsProperty<NavigationRailLabelType>("labelType", labelType.Value, defaultValue: defaultData.labelType));
        }

        public bool Equals(NavigationRailThemeData other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(backgroundColor, other.backgroundColor) 
                   && Nullable.Equals(elevation, other.elevation) 
                   && Equals(unselectedLabelTextStyle, other.unselectedLabelTextStyle) 
                   && Equals(selectedLabelTextStyle, other.selectedLabelTextStyle) 
                   && Equals(unselectedIconTheme, other.unselectedIconTheme) 
                   && Equals(selectedIconTheme, other.selectedIconTheme) 
                   && Nullable.Equals(groupAlignment, other.groupAlignment) 
                   && labelType.Equals(other.labelType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((NavigationRailThemeData) obj);
        }
        public static bool operator ==(NavigationRailThemeData left, NavigationRailThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(NavigationRailThemeData left, NavigationRailThemeData right) {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (backgroundColor != null ? backgroundColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ elevation.GetHashCode();
                hashCode = (hashCode * 397) ^ (unselectedLabelTextStyle != null ? unselectedLabelTextStyle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (selectedLabelTextStyle != null ? selectedLabelTextStyle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (unselectedIconTheme != null ? unselectedIconTheme.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (selectedIconTheme != null ? selectedIconTheme.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ groupAlignment.GetHashCode();
                hashCode = (hashCode * 397) ^ labelType.GetHashCode();
                return hashCode;
            }
        }
    }
}