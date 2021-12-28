using System;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public enum ButtonTextTheme {
        normal,

        accent,

        primary
    }

    public enum ButtonBarLayoutBehavior {
        constrained,

        padded
    }

    public class ButtonTheme : InheritedTheme {
        public ButtonTheme(
            Key key = null,
            ButtonTextTheme textTheme = ButtonTextTheme.normal,
            ButtonBarLayoutBehavior layoutBehavior = ButtonBarLayoutBehavior.padded,
            float minWidth = 88.0f,
            float height = 36.0f,
            EdgeInsetsGeometry padding = null,
            ShapeBorder shape = null,
            bool alignedDropdown = false,
            Color buttonColor = null,
            Color disabledColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            ColorScheme colorScheme = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(minWidth >= 0.0);
            D.assert(height >= 0.0);
            data = new ButtonThemeData(
                textTheme: textTheme,
                minWidth: minWidth,
                height: height,
                padding: padding,
                shape: shape,
                alignedDropdown: alignedDropdown,
                layoutBehavior: layoutBehavior,
                buttonColor: buttonColor,
                disabledColor: disabledColor,
                focusColor: focusColor,
                hoverColor: hoverColor,
                highlightColor: highlightColor,
                splashColor: splashColor,
                colorScheme: colorScheme,
                materialTapTargetSize: materialTapTargetSize);
        }

        public ButtonTheme(
            Key key = null,
            ButtonThemeData data = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(data != null);
            this.data = data;
        }

        public static ButtonTheme fromButtonThemeData(
            Key key = null,
            ButtonThemeData data = null,
            Widget child = null) {
            return new ButtonTheme(key, data, child);
        }

        public static ButtonTheme bar(
            Key key = null,
            ButtonTextTheme textTheme = ButtonTextTheme.accent,
            float minWidth = 64.0f,
            float height = 36.0f,
            EdgeInsetsGeometry padding = null,
            ShapeBorder shape = null,
            bool alignedDropdown = false,
            Color buttonColor = null,
            Color disabledColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            ColorScheme colorScheme = null,
            Widget child = null,
            ButtonBarLayoutBehavior layoutBehavior = ButtonBarLayoutBehavior.padded
        ) {
            D.assert(minWidth >= 0.0);
            D.assert(height >= 0.0);
            return new ButtonTheme(key, new ButtonThemeData(
                textTheme: textTheme,
                minWidth: minWidth,
                height: height,
                padding: padding ?? EdgeInsets.symmetric(horizontal: 8.0f),
                shape: shape,
                alignedDropdown: alignedDropdown,
                layoutBehavior: layoutBehavior,
                buttonColor: buttonColor,
                disabledColor: disabledColor,
                focusColor: focusColor,
                hoverColor: hoverColor,
                highlightColor: highlightColor,
                splashColor: splashColor,
                colorScheme: colorScheme
            ), child);
        }


        public readonly ButtonThemeData data;

        public static ButtonThemeData of(BuildContext context) {
            ButtonTheme inheritedButtonTheme = (ButtonTheme) context.dependOnInheritedWidgetOfExactType<ButtonTheme>();
            ButtonThemeData buttonTheme = inheritedButtonTheme?.data;
            if (buttonTheme?.colorScheme == null) {
                ThemeData theme = Theme.of(context);
                buttonTheme = buttonTheme ?? theme.buttonTheme;
                if (buttonTheme.colorScheme == null) {
                    buttonTheme = buttonTheme.copyWith(
                        colorScheme: theme.buttonTheme.colorScheme ?? theme.colorScheme);
                    D.assert(buttonTheme.colorScheme != null);
                }
            }

            return buttonTheme;
        }

        public override Widget wrap(BuildContext context, Widget child) {
            ButtonTheme ancestorTheme = context.findAncestorWidgetOfExactType<ButtonTheme>();
            return ReferenceEquals(this, ancestorTheme)
                ? child
                : ButtonTheme.fromButtonThemeData(data: data, child: child);
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return data != ((ButtonTheme) oldWidget).data;
        }
    }


    public class ButtonThemeData : Diagnosticable, IEquatable<ButtonThemeData> {
        public ButtonThemeData(
            ButtonTextTheme textTheme = ButtonTextTheme.normal,
            float minWidth = 88.0f,
            float height = 36.0f,
            EdgeInsetsGeometry padding = null,
            ShapeBorder shape = null,
            ButtonBarLayoutBehavior layoutBehavior = ButtonBarLayoutBehavior.padded,
            bool alignedDropdown = false,
            Color buttonColor = null,
            Color disabledColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            ColorScheme colorScheme = null,
            MaterialTapTargetSize? materialTapTargetSize = null
        ) {
            D.assert(minWidth >= 0.0);
            D.assert(height >= 0.0);
            this.textTheme = textTheme;
            this.minWidth = minWidth;
            this.height = height;
            this.layoutBehavior = layoutBehavior;
            this.alignedDropdown = alignedDropdown;
            this.colorScheme = colorScheme;
            _buttonColor = buttonColor;
            _disabledColor = disabledColor;
            _focusColor = focusColor;
            _hoverColor = hoverColor;
            _highlightColor = highlightColor;
            _splashColor = splashColor;
            _padding = padding;
            _shape = shape;
            _materialTapTargetSize = materialTapTargetSize;
        }


        public readonly float minWidth;

        public readonly float height;

        public readonly ButtonTextTheme textTheme;

        public readonly ButtonBarLayoutBehavior layoutBehavior;

        public BoxConstraints constraints {
            get {
                return new BoxConstraints(minWidth: minWidth,
                    minHeight: height);
            }
        }

        public EdgeInsetsGeometry padding {
            get {
                if (_padding != null) {
                    return _padding;
                }

                switch (textTheme) {
                    case ButtonTextTheme.normal:
                    case ButtonTextTheme.accent:
                        return EdgeInsets.symmetric(horizontal: 16.0f);
                    case ButtonTextTheme.primary:
                        return EdgeInsets.symmetric(horizontal: 24.0f);
                }

                D.assert(false);
                return EdgeInsets.zero;
            }
        }

        readonly EdgeInsetsGeometry _padding;

        public ShapeBorder shape {
            get {
                if (_shape != null) {
                    return _shape;
                }

                switch (textTheme) {
                    case ButtonTextTheme.normal:
                    case ButtonTextTheme.accent:
                        return new RoundedRectangleBorder(
                            borderRadius: BorderRadius.all(Radius.circular(2.0f)));
                    case ButtonTextTheme.primary:
                        return new RoundedRectangleBorder(
                            borderRadius: BorderRadius.all(Radius.circular(4.0f)));
                }

                return new RoundedRectangleBorder();
            }
        }

        readonly ShapeBorder _shape;

        public readonly bool alignedDropdown;

        readonly Color _buttonColor;

        readonly Color _disabledColor;

        readonly Color _focusColor;

        readonly Color _hoverColor;

        readonly Color _highlightColor;

        readonly Color _splashColor;

        public readonly ColorScheme colorScheme;

        readonly MaterialTapTargetSize? _materialTapTargetSize;

        public Brightness getBrightness(MaterialButton button) {
            return button.colorBrightness ?? colorScheme.brightness;
        }

        public ButtonTextTheme getTextTheme(MaterialButton button) {
            return button.textTheme ?? textTheme;
        }

        public Color getDisabledTextColor(MaterialButton button) {
            if (button.textColor is IMaterialStateProperty<Color>)
                return button.textColor;
            if (button.disabledTextColor != null) {
                return button.disabledTextColor;
            }

            return colorScheme.onSurface.withOpacity(0.38f);
        }


        Color getDisabledFillColor(MaterialButton button) {
            if (button.disabledColor != null) {
                return button.disabledColor;
            }

            if (_disabledColor != null) {
                return _disabledColor;
            }

            return colorScheme.onSurface.withOpacity(0.38f);
        }


        public Color getFillColor(MaterialButton button) {
            Color fillColor = button.enabled ? button.color : button.disabledColor;
            if (fillColor != null) {
                return fillColor;
            }

            if (button is FlatButton || button is OutlineButton || button.GetType() == typeof(MaterialButton)) {
                return null;
            }


            if (button.enabled && button is RaisedButton && _buttonColor != null) {
                return _buttonColor;
            }

            switch (getTextTheme(button)) {
                case ButtonTextTheme.normal:
                case ButtonTextTheme.accent:
                    return button.enabled ? colorScheme.primary : getDisabledFillColor(button);
                case ButtonTextTheme.primary:
                    return button.enabled
                        ? _buttonColor ?? colorScheme.primary
                        : colorScheme.onSurface.withOpacity(0.12f);
            }

            D.assert(false);
            return null;
        }

        public Color getTextColor(MaterialButton button) {
            if (!button.enabled) {
                return getDisabledTextColor(button);
            }

            if (button.textColor != null) {
                return button.textColor;
            }

            switch (getTextTheme(button)) {
                case ButtonTextTheme.normal:
                    return getBrightness(button) == Brightness.dark ? Colors.white : Colors.black87;
                case ButtonTextTheme.accent:
                    return colorScheme.secondary;
                case ButtonTextTheme.primary: {
                    Color fillColor = getFillColor(button);
                    bool fillIsDark = fillColor != null
                        ? ThemeData.estimateBrightnessForColor(fillColor) == Brightness.dark
                        : getBrightness(button) == Brightness.dark;
                    if (fillIsDark) {
                        return Colors.white;
                    }

                    if (button is FlatButton || button is OutlineButton) {
                        return colorScheme.primary;
                    }

                    return Colors.black;
                }
            }

            D.assert(false);
            return null;
        }

        public Color getSplashColor(MaterialButton button) {
            if (button.splashColor != null) {
                return button.splashColor;
            }

            if (_splashColor != null && (button is RaisedButton || button is OutlineButton)) {
                return _splashColor;
            }

            if (_splashColor != null && button is FlatButton) {
                switch (getTextTheme(button)) {
                    case ButtonTextTheme.normal:
                    case ButtonTextTheme.accent:
                        return _splashColor;
                    case ButtonTextTheme.primary:
                        break;
                }
            }

            return getTextColor(button).withOpacity(0.12f);
        }

        public Color getFocusColor(MaterialButton button) {
            return button.focusColor ?? _focusColor ?? getTextColor(button).withOpacity(0.12f);
        }

        public Color getHoverColor(MaterialButton button) {
            return button.hoverColor ?? _hoverColor ?? getTextColor(button).withOpacity(0.04f);
        }

        public Color getHighlightColor(MaterialButton button) {
            if (button.highlightColor != null) {
                return button.highlightColor;
            }

            switch (getTextTheme(button)) {
                case ButtonTextTheme.normal:
                case ButtonTextTheme.accent:
                    return _highlightColor ?? getTextColor(button).withOpacity(0.16f);
                case ButtonTextTheme.primary:
                    return Colors.transparent;
            }

            D.assert(false);
            return Colors.transparent;
        }


        public float getElevation(MaterialButton button) {
            if (button.elevation != null) {
                return button.elevation ?? 0.0f;
            }

            if (button is FlatButton) {
                return 0.0f;
            }

            return 2.0f;
        }

        public float getFocusElevation(MaterialButton button) {
            if (button.focusElevation != null)
                return button.focusElevation.Value;
            if (button is FlatButton)
                return 0.0f;
            if (button is OutlineButton)
                return 0.0f;
            return 4.0f;
        }

        public float getHoverElevation(MaterialButton button) {
            if (button.hoverElevation != null)
                return button.hoverElevation.Value;
            if (button is FlatButton)
                return 0.0f;
            if (button is OutlineButton)
                return 0.0f;
            return 4.0f;
        }

        public float getHighlightElevation(MaterialButton button) {
            if (button.highlightElevation != null) {
                return button.highlightElevation ?? 0.0f;
            }

            if (button is FlatButton) {
                return 0.0f;
            }

            if (button is OutlineButton) {
                return 0.0f;
            }

            return 8.0f;
        }


        public float getDisabledElevation(MaterialButton button) {
            if (button.disabledElevation != null) {
                return button.disabledElevation ?? 0.0f;
            }

            return 0.0f;
        }


        public EdgeInsetsGeometry getPadding(MaterialButton button) {
            if (button.padding != null) {
                return button.padding;
            }

            if (button is MaterialButtonWithIconMixin) {
                return EdgeInsets.fromLTRB(12.0f, 0.0f, 16.0f, 0.0f);
            }

            if (_padding != null) {
                return _padding;
            }

            switch (getTextTheme(button)) {
                case ButtonTextTheme.normal:
                case ButtonTextTheme.accent:
                    return EdgeInsets.symmetric(horizontal: 16.0f);
                case ButtonTextTheme.primary:
                    return EdgeInsets.symmetric(horizontal: 24.0f);
            }

            D.assert(false);
            return EdgeInsets.zero;
        }

        public ShapeBorder getShape(MaterialButton button) {
            return button.shape ?? shape;
        }


        public TimeSpan getAnimationDuration(MaterialButton button) {
            return button.animationDuration ?? material_.kThemeChangeDuration;
        }

        public BoxConstraints getConstraints(MaterialButton button) {
            return constraints;
        }


        public MaterialTapTargetSize getMaterialTapTargetSize(MaterialButton button) {
            return button.materialTapTargetSize ?? _materialTapTargetSize ?? MaterialTapTargetSize.padded;
        }


        public ButtonThemeData copyWith(
            ButtonTextTheme? textTheme = null,
            ButtonBarLayoutBehavior? layoutBehavior = null,
            float? minWidth = null,
            float? height = null,
            EdgeInsetsGeometry padding = null,
            ShapeBorder shape = null,
            bool? alignedDropdown = null,
            Color buttonColor = null,
            Color disabledColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            ColorScheme colorScheme = null,
            MaterialTapTargetSize? materialTapTargetSize = null) {
            return new ButtonThemeData(
                textTheme: textTheme ?? this.textTheme,
                layoutBehavior: layoutBehavior ?? this.layoutBehavior,
                minWidth: minWidth ?? this.minWidth,
                height: height ?? this.height,
                padding: padding ?? this.padding,
                shape: shape ?? this.shape,
                alignedDropdown: alignedDropdown ?? this.alignedDropdown,
                buttonColor: buttonColor ?? _buttonColor,
                disabledColor: disabledColor ?? _disabledColor,
                focusColor: focusColor ?? _focusColor,
                hoverColor: hoverColor ?? _hoverColor,
                highlightColor: highlightColor ?? _highlightColor,
                splashColor: splashColor ?? _splashColor,
                colorScheme: colorScheme ?? this.colorScheme,
                materialTapTargetSize: materialTapTargetSize ?? _materialTapTargetSize);
        }

        public bool Equals(ButtonThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return textTheme == other.textTheme
                   && minWidth == other.minWidth
                   && height == other.height
                   && padding == other.padding
                   && shape == other.shape
                   && alignedDropdown == other.alignedDropdown
                   && _buttonColor == other._buttonColor
                   && _disabledColor == other._disabledColor
                   && _focusColor == other._focusColor
                   && _hoverColor == other._hoverColor
                   && _highlightColor == other._highlightColor
                   && _splashColor == other._splashColor
                   && colorScheme == other.colorScheme
                   && _materialTapTargetSize == other._materialTapTargetSize;
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

            return Equals((ButtonThemeData) obj);
        }

        public static bool operator ==(ButtonThemeData left, ButtonThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(ButtonThemeData left, ButtonThemeData right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = textTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ minWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ height.GetHashCode();
                hashCode = (hashCode * 397) ^ padding.GetHashCode();
                hashCode = (hashCode * 397) ^ shape.GetHashCode();
                hashCode = (hashCode * 397) ^ alignedDropdown.GetHashCode();
                hashCode = (hashCode * 397) ^ (_buttonColor != null ? _buttonColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_disabledColor != null ? _disabledColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_focusColor != null ? _focusColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_hoverColor != null ? _hoverColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_highlightColor != null ? _highlightColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_splashColor != null ? _splashColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (colorScheme != null ? colorScheme.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _materialTapTargetSize.GetHashCode();
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            ButtonThemeData defaultTheme = new ButtonThemeData();
            properties.add(new EnumProperty<ButtonTextTheme>("textTheme", textTheme,
                defaultValue: defaultTheme.textTheme));
            properties.add(new FloatProperty("minWidth", minWidth, defaultValue: defaultTheme.minWidth));
            properties.add(new FloatProperty("height", height, defaultValue: defaultTheme.height));
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding,
                defaultValue: defaultTheme.padding));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: defaultTheme.shape));
            properties.add(new FlagProperty("alignedDropdown",
                value: alignedDropdown,
                defaultValue: defaultTheme.alignedDropdown,
                ifTrue: "dropdown width matches button"
            ));
            properties.add(new ColorProperty("buttonColor", _buttonColor, defaultValue: null));
            properties.add(new ColorProperty("disabledColor", _disabledColor, defaultValue: null));
            properties.add(new ColorProperty("focusColor", _focusColor, defaultValue: null));
            properties.add(new ColorProperty("hoverColor", _hoverColor, defaultValue: null));
            properties.add(new ColorProperty("highlightColor", _highlightColor, defaultValue: null));
            properties.add(new ColorProperty("splashColor", _splashColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<ColorScheme>("colorScheme", colorScheme,
                defaultValue: defaultTheme.colorScheme));
            properties.add(new DiagnosticsProperty<MaterialTapTargetSize?>("materialTapTargetSize",
                _materialTapTargetSize, defaultValue: null));
        }
    }
}