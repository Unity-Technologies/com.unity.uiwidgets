using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class SliderTheme : InheritedTheme {
        public SliderTheme(
            Key key = null,
            SliderThemeData data = null,
            Widget child = null)
            : base(key: key, child: child) {
            D.assert(child != null);
            D.assert(data != null);
            this.data = data;
        }

        public readonly SliderThemeData data;

        public static SliderThemeData of(BuildContext context) {
            SliderTheme inheritedTheme = context.dependOnInheritedWidgetOfExactType<SliderTheme>();
            return inheritedTheme != null ? inheritedTheme.data : Theme.of(context).sliderTheme;
        }

        public override Widget wrap(BuildContext context, Widget child) {
            SliderTheme ancestorTheme = context.findAncestorWidgetOfExactType<SliderTheme>();
            return ReferenceEquals(this, ancestorTheme) ? child : new SliderTheme(data: data, child: child);
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            SliderTheme _oldWidget = (SliderTheme) oldWidget;
            return data != _oldWidget.data;
        }
    }


    public enum ShowValueIndicator {
        onlyForDiscrete,
        onlyForContinuous,
        always,
        never
    }

    public enum Thumb {
        start,
        end
    }

    public class SliderThemeData : Diagnosticable {
        public SliderThemeData(
            float? trackHeight = null,
            Color activeTrackColor = null,
            Color inactiveTrackColor = null,
            Color disabledActiveTrackColor = null,
            Color disabledInactiveTrackColor = null,
            Color activeTickMarkColor = null,
            Color inactiveTickMarkColor = null,
            Color disabledActiveTickMarkColor = null,
            Color disabledInactiveTickMarkColor = null,
            Color thumbColor = null,
            Color overlappingShapeStrokeColor = null,
            Color disabledThumbColor = null,
            Color overlayColor = null,
            Color valueIndicatorColor = null,
            SliderComponentShape overlayShape = null,
            SliderTickMarkShape tickMarkShape = null,
            SliderComponentShape thumbShape = null,
            SliderTrackShape trackShape = null,
            SliderComponentShape valueIndicatorShape = null,
            RangeSliderTickMarkShape rangeTickMarkShape = null,
            RangeSliderThumbShape rangeThumbShape = null,
            RangeSliderTrackShape rangeTrackShape = null,
            RangeSliderValueIndicatorShape rangeValueIndicatorShape = null,
            ShowValueIndicator? showValueIndicator = null,
            TextStyle valueIndicatorTextStyle = null,
            float? minThumbSeparation = null,
            RangeThumbSelector thumbSelector = null
        ) {
            this.trackHeight = trackHeight;
            this.activeTrackColor = activeTrackColor;
            this.inactiveTrackColor = inactiveTrackColor;
            this.disabledActiveTrackColor = disabledActiveTrackColor;
            this.disabledInactiveTrackColor = disabledInactiveTrackColor;
            this.activeTickMarkColor = activeTickMarkColor;
            this.inactiveTickMarkColor = inactiveTickMarkColor;
            this.disabledActiveTickMarkColor = disabledActiveTickMarkColor;
            this.disabledInactiveTickMarkColor = disabledInactiveTickMarkColor;
            this.thumbColor = thumbColor;
            this.overlappingShapeStrokeColor = overlappingShapeStrokeColor;
            this.disabledThumbColor = disabledThumbColor;
            this.overlayColor = overlayColor;
            this.valueIndicatorColor = valueIndicatorColor;
            this.overlayShape = overlayShape;
            this.tickMarkShape = tickMarkShape;
            this.thumbShape = thumbShape;
            this.trackShape = trackShape;
            this.valueIndicatorShape = valueIndicatorShape;
            this.rangeTickMarkShape = rangeTickMarkShape;
            this.rangeThumbShape = rangeThumbShape;
            this.rangeTrackShape = rangeTrackShape;
            this.rangeValueIndicatorShape = rangeValueIndicatorShape;
            this.showValueIndicator = showValueIndicator;
            this.valueIndicatorTextStyle = valueIndicatorTextStyle;
            this.minThumbSeparation = minThumbSeparation;
            this.thumbSelector = thumbSelector;
        }

        public static SliderThemeData fromPrimaryColors(
            Color primaryColor = null,
            Color primaryColorDark = null,
            Color primaryColorLight = null,
            TextStyle valueIndicatorTextStyle = null) {
            D.assert(primaryColor != null);
            D.assert(primaryColorDark != null);
            D.assert(primaryColorLight != null);
            D.assert(valueIndicatorTextStyle != null);

            const int activeTrackAlpha = 0xff;
            const int inactiveTrackAlpha = 0x3d; // 24% opacity
            const int disabledActiveTrackAlpha = 0x52; // 32% opacity
            const int disabledInactiveTrackAlpha = 0x1f; // 12% opacity
            const int activeTickMarkAlpha = 0x8a; // 54% opacity
            const int inactiveTickMarkAlpha = 0x8a; // 54% opacity
            const int disabledActiveTickMarkAlpha = 0x1f; // 12% opacity
            const int disabledInactiveTickMarkAlpha = 0x1f; // 12% opacity
            const int thumbAlpha = 0xff;
            const int disabledThumbAlpha = 0x52; // 32% opacity
            const int overlayAlpha = 0x1f;        // 12% opacity
            const int valueIndicatorAlpha = 0xff;

            return new SliderThemeData(
                trackHeight: 2.0f,
                activeTrackColor: primaryColor.withAlpha(activeTrackAlpha),
                inactiveTrackColor: primaryColor.withAlpha(inactiveTrackAlpha),
                disabledActiveTrackColor: primaryColorDark.withAlpha(disabledActiveTrackAlpha),
                disabledInactiveTrackColor: primaryColorDark.withAlpha(disabledInactiveTrackAlpha),
                activeTickMarkColor: primaryColorLight.withAlpha(activeTickMarkAlpha),
                inactiveTickMarkColor: primaryColor.withAlpha(inactiveTickMarkAlpha),
                disabledActiveTickMarkColor: primaryColorLight.withAlpha(disabledActiveTickMarkAlpha),
                disabledInactiveTickMarkColor: primaryColorDark.withAlpha(disabledInactiveTickMarkAlpha),
                thumbColor: primaryColor.withAlpha(thumbAlpha),
                overlappingShapeStrokeColor: Colors.white,
                disabledThumbColor: primaryColorDark.withAlpha(disabledThumbAlpha),
                overlayColor: primaryColor.withAlpha(overlayAlpha),
                valueIndicatorColor: primaryColor.withAlpha(valueIndicatorAlpha),
                overlayShape: new RoundSliderOverlayShape(),
                tickMarkShape: new RoundSliderTickMarkShape(),
                thumbShape: new RoundSliderThumbShape(),
                trackShape: new RoundedRectSliderTrackShape(),
                valueIndicatorShape: new PaddleSliderValueIndicatorShape(),
                rangeTickMarkShape: new RoundRangeSliderTickMarkShape(),
                rangeThumbShape: new RoundRangeSliderThumbShape(),
                rangeTrackShape: new RoundedRectRangeSliderTrackShape(),
                rangeValueIndicatorShape: new PaddleRangeSliderValueIndicatorShape(),
                valueIndicatorTextStyle: valueIndicatorTextStyle,
                showValueIndicator: ShowValueIndicator.onlyForDiscrete
            );
        }

        public readonly float? trackHeight;

        public readonly Color activeTrackColor;

        public readonly Color inactiveTrackColor;

        public readonly Color disabledActiveTrackColor;

        public readonly Color disabledInactiveTrackColor;

        public readonly Color activeTickMarkColor;

        public readonly Color inactiveTickMarkColor;

        public readonly Color disabledActiveTickMarkColor;

        public readonly Color disabledInactiveTickMarkColor;

        public readonly Color thumbColor;

        public readonly Color overlappingShapeStrokeColor;

        public readonly Color disabledThumbColor;

        public readonly Color overlayColor;

        public readonly Color valueIndicatorColor;

        public readonly SliderTickMarkShape tickMarkShape;

        public readonly SliderComponentShape overlayShape;

        public readonly SliderComponentShape thumbShape;

        public readonly SliderTrackShape trackShape;

        public readonly SliderComponentShape valueIndicatorShape;

        public readonly RangeSliderTickMarkShape rangeTickMarkShape;

        public readonly RangeSliderThumbShape rangeThumbShape;

        public readonly RangeSliderTrackShape rangeTrackShape;

        public readonly RangeSliderValueIndicatorShape rangeValueIndicatorShape;

        public readonly ShowValueIndicator? showValueIndicator;

        public readonly TextStyle valueIndicatorTextStyle;

        public readonly float? minThumbSeparation;

        public readonly RangeThumbSelector thumbSelector;

        public SliderThemeData copyWith(
            float? trackHeight = null,
            Color activeTrackColor = null,
            Color inactiveTrackColor = null,
            Color disabledActiveTrackColor = null,
            Color disabledInactiveTrackColor = null,
            Color activeTickMarkColor = null,
            Color inactiveTickMarkColor = null,
            Color disabledActiveTickMarkColor = null,
            Color disabledInactiveTickMarkColor = null,
            Color thumbColor = null,
            Color overlappingShapeStrokeColor = null,
            Color disabledThumbColor = null,
            Color overlayColor = null,
            Color valueIndicatorColor = null,
            SliderComponentShape overlayShape = null,
            SliderTickMarkShape tickMarkShape = null,
            SliderComponentShape thumbShape = null,
            SliderTrackShape trackShape = null,
            SliderComponentShape valueIndicatorShape = null,
            RangeSliderTickMarkShape rangeTickMarkShape = null,
            RangeSliderThumbShape rangeThumbShape = null,
            RangeSliderTrackShape rangeTrackShape = null,
            RangeSliderValueIndicatorShape rangeValueIndicatorShape = null,
            ShowValueIndicator? showValueIndicator = null,
            TextStyle valueIndicatorTextStyle = null,
            float? minThumbSeparation = null,
            RangeThumbSelector thumbSelector = null
        ) {
            return new SliderThemeData(
                trackHeight: trackHeight ?? this.trackHeight,
                activeTrackColor: activeTrackColor ?? this.activeTrackColor,
                inactiveTrackColor: inactiveTrackColor ?? this.inactiveTrackColor,
                disabledActiveTrackColor: disabledActiveTrackColor ?? this.disabledActiveTrackColor,
                disabledInactiveTrackColor: disabledInactiveTrackColor ?? this.disabledInactiveTrackColor,
                activeTickMarkColor: activeTickMarkColor ?? this.activeTickMarkColor,
                inactiveTickMarkColor: inactiveTickMarkColor ?? this.inactiveTickMarkColor,
                disabledActiveTickMarkColor: disabledActiveTickMarkColor ?? this.disabledActiveTickMarkColor,
                disabledInactiveTickMarkColor: disabledInactiveTickMarkColor ?? this.disabledInactiveTickMarkColor,
                thumbColor: thumbColor ?? this.thumbColor,
                overlappingShapeStrokeColor: overlappingShapeStrokeColor ?? this.overlappingShapeStrokeColor,
                disabledThumbColor: disabledThumbColor ?? this.disabledThumbColor,
                overlayColor: overlayColor ?? this.overlayColor,
                valueIndicatorColor: valueIndicatorColor ?? this.valueIndicatorColor,
                overlayShape: overlayShape ?? this.overlayShape,
                tickMarkShape: tickMarkShape ?? this.tickMarkShape,
                thumbShape: thumbShape ?? this.thumbShape,
                trackShape: trackShape ?? this.trackShape,
                valueIndicatorShape: valueIndicatorShape ?? this.valueIndicatorShape,
                rangeTickMarkShape: rangeTickMarkShape ?? this.rangeTickMarkShape,
                rangeThumbShape: rangeThumbShape ?? this.rangeThumbShape,
                rangeTrackShape: rangeTrackShape ?? this.rangeTrackShape,
                rangeValueIndicatorShape: rangeValueIndicatorShape ?? this.rangeValueIndicatorShape,
                showValueIndicator: showValueIndicator ?? this.showValueIndicator,
                valueIndicatorTextStyle: valueIndicatorTextStyle ?? this.valueIndicatorTextStyle,
                minThumbSeparation: minThumbSeparation ?? this.minThumbSeparation,
                thumbSelector: thumbSelector ?? this.thumbSelector
            );
        }

        public static SliderThemeData lerp(SliderThemeData a, SliderThemeData b, float t) {
            D.assert(a != null);
            D.assert(b != null);
            return new SliderThemeData(
                trackHeight: MathUtils.lerpNullableFloat(a.trackHeight, b.trackHeight, t),
                activeTrackColor: Color.lerp(a.activeTrackColor, b.activeTrackColor, t),
                inactiveTrackColor: Color.lerp(a.inactiveTrackColor, b.inactiveTrackColor, t),
                disabledActiveTrackColor: Color.lerp(a.disabledActiveTrackColor, b.disabledActiveTrackColor, t),
                disabledInactiveTrackColor: Color.lerp(a.disabledInactiveTrackColor, b.disabledInactiveTrackColor, t),
                activeTickMarkColor: Color.lerp(a.activeTickMarkColor, b.activeTickMarkColor, t),
                inactiveTickMarkColor: Color.lerp(a.inactiveTickMarkColor, b.inactiveTickMarkColor, t),
                disabledActiveTickMarkColor: Color.lerp(a.disabledActiveTickMarkColor, b.disabledActiveTickMarkColor,
                    t),
                disabledInactiveTickMarkColor: Color.lerp(a.disabledInactiveTickMarkColor,
                    b.disabledInactiveTickMarkColor, t),
                thumbColor: Color.lerp(a.thumbColor, b.thumbColor, t),
                overlappingShapeStrokeColor: Color.lerp(a.overlappingShapeStrokeColor, b.overlappingShapeStrokeColor, t),
                disabledThumbColor: Color.lerp(a.disabledThumbColor, b.disabledThumbColor, t),
                overlayColor: Color.lerp(a.overlayColor, b.overlayColor, t),
                valueIndicatorColor: Color.lerp(a.valueIndicatorColor, b.valueIndicatorColor, t),
                overlayShape: t < 0.5 ? a.overlayShape : b.overlayShape,
                tickMarkShape: t < 0.5 ? a.tickMarkShape : b.tickMarkShape,
                thumbShape: t < 0.5 ? a.thumbShape : b.thumbShape,
                trackShape: t < 0.5 ? a.trackShape : b.trackShape,
                valueIndicatorShape: t < 0.5 ? a.valueIndicatorShape : b.valueIndicatorShape,
                rangeTickMarkShape: t < 0.5 ? a.rangeTickMarkShape : b.rangeTickMarkShape,
                rangeThumbShape: t < 0.5 ? a.rangeThumbShape : b.rangeThumbShape,
                rangeTrackShape: t < 0.5 ? a.rangeTrackShape : b.rangeTrackShape,
                rangeValueIndicatorShape: t < 0.5 ? a.rangeValueIndicatorShape : b.rangeValueIndicatorShape,
                showValueIndicator: t < 0.5 ? a.showValueIndicator : b.showValueIndicator,
                valueIndicatorTextStyle: TextStyle.lerp(a.valueIndicatorTextStyle, b.valueIndicatorTextStyle, t),
                minThumbSeparation: MathUtils.lerpNullableFloat(a.minThumbSeparation, b.minThumbSeparation, t),
                thumbSelector: t < 0.5 ? a.thumbSelector : b.thumbSelector
            );
        }

        public bool Equals(SliderThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(other.trackHeight, trackHeight)
                   && other.activeTrackColor == activeTrackColor
                   && other.inactiveTrackColor == inactiveTrackColor
                   && other.disabledActiveTrackColor == disabledActiveTrackColor
                   && other.disabledInactiveTrackColor == disabledInactiveTrackColor
                   && other.activeTickMarkColor == activeTickMarkColor
                   && other.inactiveTickMarkColor == inactiveTickMarkColor
                   && other.disabledActiveTickMarkColor == disabledActiveTickMarkColor
                   && other.disabledInactiveTickMarkColor == disabledInactiveTickMarkColor
                   && other.thumbColor == thumbColor
                   && other.overlappingShapeStrokeColor == overlappingShapeStrokeColor
                   && other.disabledThumbColor == disabledThumbColor
                   && other.overlayColor == overlayColor
                   && other.valueIndicatorColor == valueIndicatorColor
                   && other.overlayShape == overlayShape
                   && other.tickMarkShape == tickMarkShape
                   && other.thumbShape == thumbShape
                   && other.trackShape == trackShape
                   && other.valueIndicatorShape == valueIndicatorShape
                   && other.rangeTickMarkShape == rangeTickMarkShape
                   && other.rangeThumbShape == rangeThumbShape
                   && other.rangeTrackShape == rangeTrackShape
                   && other.rangeValueIndicatorShape == rangeValueIndicatorShape
                   && other.showValueIndicator == showValueIndicator
                   && other.valueIndicatorTextStyle == valueIndicatorTextStyle
                   && Equals(other.minThumbSeparation, minThumbSeparation)
                   && other.thumbSelector == thumbSelector;
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

            return Equals((SliderThemeData) obj);
        }

        public static bool operator ==(SliderThemeData left, SliderThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(SliderThemeData left, SliderThemeData right) {
            return !Equals(left, right);
        }

        int? _cachedHashCode = null;

        public override int GetHashCode() {
            if (_cachedHashCode != null) {
                return _cachedHashCode.Value;
            }

            unchecked {
                var hashCode = trackHeight?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ activeTrackColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ inactiveTrackColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ disabledActiveTrackColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ disabledInactiveTrackColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ activeTickMarkColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ inactiveTickMarkColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ disabledActiveTickMarkColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ disabledInactiveTickMarkColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ thumbColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ overlappingShapeStrokeColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ disabledThumbColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ overlayColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ valueIndicatorColor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ overlayShape?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ tickMarkShape?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ thumbShape?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ trackShape?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ rangeTickMarkShape?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ rangeThumbShape?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ rangeTrackShape?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ rangeValueIndicatorShape?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ valueIndicatorShape?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ showValueIndicator?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ valueIndicatorTextStyle?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ minThumbSeparation?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ thumbSelector?.GetHashCode() ?? 0;

                _cachedHashCode = hashCode;
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            ThemeData defaultTheme = ThemeData.fallback();
            SliderThemeData defaultData = fromPrimaryColors(
                primaryColor: defaultTheme.primaryColor,
                primaryColorDark: defaultTheme.primaryColorDark,
                primaryColorLight: defaultTheme.primaryColorLight,
                valueIndicatorTextStyle: defaultTheme.accentTextTheme.body2
            );
            properties.add(new DiagnosticsProperty<Color>("activeTrackColor", activeTrackColor,
                defaultValue: defaultData.activeTrackColor));
            properties.add(new DiagnosticsProperty<Color>("activeTrackColor", activeTrackColor,
                defaultValue: defaultData.activeTrackColor));
            properties.add(new DiagnosticsProperty<Color>("inactiveTrackColor", inactiveTrackColor,
                defaultValue: defaultData.inactiveTrackColor));
            properties.add(new DiagnosticsProperty<Color>("disabledActiveTrackColor", disabledActiveTrackColor,
                defaultValue: defaultData.disabledActiveTrackColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("disabledInactiveTrackColor", disabledInactiveTrackColor,
                defaultValue: defaultData.disabledInactiveTrackColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("activeTickMarkColor", activeTickMarkColor,
                defaultValue: defaultData.activeTickMarkColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("inactiveTickMarkColor", inactiveTickMarkColor,
                defaultValue: defaultData.inactiveTickMarkColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("disabledActiveTickMarkColor",
                disabledActiveTickMarkColor, defaultValue: defaultData.disabledActiveTickMarkColor,
                level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("disabledInactiveTickMarkColor",
                disabledInactiveTickMarkColor, defaultValue: defaultData.disabledInactiveTickMarkColor,
                level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("thumbColor", thumbColor,
                defaultValue: defaultData.thumbColor));
            properties.add(new ColorProperty("overlappingShapeStrokeColor", overlappingShapeStrokeColor, defaultValue: defaultData.overlappingShapeStrokeColor));

            properties.add(new DiagnosticsProperty<Color>("disabledThumbColor", disabledThumbColor,
                defaultValue: defaultData.disabledThumbColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("overlayColor", overlayColor,
                defaultValue: defaultData.overlayColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("valueIndicatorColor", valueIndicatorColor,
                defaultValue: defaultData.valueIndicatorColor));
            properties.add(new DiagnosticsProperty<SliderComponentShape>("overlayShape", overlayShape,
                defaultValue: defaultData.overlayShape, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<SliderTickMarkShape>("tickMarkShape", tickMarkShape,
                defaultValue: defaultData.tickMarkShape, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<SliderComponentShape>("thumbShape", thumbShape,
                defaultValue: defaultData.thumbShape, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<SliderTrackShape>("trackShape", trackShape,
                defaultValue: defaultData.trackShape, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<SliderComponentShape>("valueIndicatorShape",
                valueIndicatorShape, defaultValue: defaultData.valueIndicatorShape, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<RangeSliderTickMarkShape>("rangeTickMarkShape", rangeTickMarkShape, defaultValue: defaultData.rangeTickMarkShape));
            properties.add(new DiagnosticsProperty<RangeSliderThumbShape>("rangeThumbShape", rangeThumbShape, defaultValue: defaultData.rangeThumbShape));
            properties.add(new DiagnosticsProperty<RangeSliderTrackShape>("rangeTrackShape", rangeTrackShape, defaultValue: defaultData.rangeTrackShape));
            properties.add(new DiagnosticsProperty<RangeSliderValueIndicatorShape>("rangeValueIndicatorShape", rangeValueIndicatorShape, defaultValue: defaultData.rangeValueIndicatorShape));
            properties.add(new EnumProperty<ShowValueIndicator>("showValueIndicator", showValueIndicator.Value,
                defaultValue: defaultData.showValueIndicator));
            properties.add(new DiagnosticsProperty<TextStyle>("valueIndicatorTextStyle", valueIndicatorTextStyle,
                defaultValue: defaultData.valueIndicatorTextStyle));
            properties.add(new FloatProperty("minThumbSeparation", minThumbSeparation, defaultValue: defaultData.minThumbSeparation));
            properties.add(new DiagnosticsProperty<RangeThumbSelector>("thumbSelector", thumbSelector, defaultValue: defaultData.thumbSelector));
        }
    }

    public abstract class SliderComponentShape {
        public SliderComponentShape() {
        }

        public abstract Size getPreferredSize(
            bool isEnabled,
            bool isDiscrete,
            TextPainter textPainter = null);

        public abstract void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            TextDirection? textDirection = null,
            float? value = null);

        public static readonly SliderComponentShape noThumb = new _EmptySliderComponentShape();

        public static readonly SliderComponentShape noOverlay = new _EmptySliderComponentShape();
    }

    public abstract class SliderTickMarkShape {
        public SliderTickMarkShape() {
        }

        public abstract Size getPreferredSize(
            SliderThemeData sliderTheme = null,
            bool isEnabled = false);

        public abstract void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset thumbCenter = null,
            bool isEnabled = false,
            TextDirection? textDirection = null);

        public static readonly SliderTickMarkShape noTickMark = new _EmptySliderTickMarkShape();
    }

    public abstract class SliderTrackShape {
        public SliderTrackShape() {
        }

        public abstract Rect getPreferredRect(
            RenderBox parentBox = null,
            Offset offset = null,
            SliderThemeData sliderTheme = null,
            bool isEnabled = false,
            bool isDiscrete = false);

        public abstract void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset thumbCenter = null,
            bool isEnabled = false,
            bool isDiscrete = false,
            TextDirection? textDirection = null
        );
    }

    public abstract class RangeSliderThumbShape {
        public RangeSliderThumbShape() {
        }

        public abstract Size getPreferredSize(bool isEnabled, bool isDiscrete);

        public abstract void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            bool isEnabled = false,
            bool? isOnTop = null,
            TextDirection? textDirection = null,
            SliderThemeData sliderTheme = null,
            Thumb? thumb = null
        );
    }

    public abstract class RangeSliderValueIndicatorShape {
        public RangeSliderValueIndicatorShape() {
        }

        public abstract Size getPreferredSize(bool isEnabled, bool isDiscrete, TextPainter labelPainter = null);

        public virtual float getHorizontalShift(
            RenderBox parentBox = null,
            Offset center = null,
            TextPainter labelPainter = null,
            Animation<float> activationAnimation = null
        ) {
            return 0;
        }

        public abstract void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            bool? isOnTop = null,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            TextDirection? textDirection = null,
            float? value = null,
            Thumb? thumb = null
        );
    }

    public abstract class RangeSliderTickMarkShape {
        public RangeSliderTickMarkShape() {
        }

        public abstract Size getPreferredSize(
            SliderThemeData sliderTheme = null,
            bool isEnabled = false
        );

        public abstract void paint(
            PaintingContext context,
            Offset center,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset startThumbCenter = null,
            Offset endThumbCenter = null,
            bool isEnabled = false,
            TextDirection? textDirection = null
        );
    }

    public abstract class RangeSliderTrackShape {
        public RangeSliderTrackShape() {
        }

        public abstract Rect getPreferredRect(
            RenderBox parentBox = null,
            Offset offset = null,
            SliderThemeData sliderTheme = null,
            bool isEnabled = false,
            bool isDiscrete = false
        );

        public abstract void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset startThumbCenter = null,
            Offset endThumbCenter = null,
            bool isEnabled = false,
            bool isDiscrete = false,
            TextDirection? textDirection = null
        );
    }

    public class RectangularSliderTrackShape : SliderTrackShape {
        public RectangularSliderTrackShape(
            float disabledThumbGapWidth = 2.0f) {
            this.disabledThumbGapWidth = disabledThumbGapWidth;
        }

        public readonly float disabledThumbGapWidth;


        public override Rect getPreferredRect(
            RenderBox parentBox = null,
            Offset offset = null,
            SliderThemeData sliderTheme = null,
            bool isEnabled = false,
            bool isDiscrete = false) {
            offset = offset ?? Offset.zero;
            
            D.assert(parentBox != null);
            D.assert(sliderTheme != null);
            float thumbWidth = sliderTheme.thumbShape.getPreferredSize(isEnabled, isDiscrete).width;
            float overlayWidth = sliderTheme.overlayShape.getPreferredSize(isEnabled, isDiscrete).width;
            float trackHeight = sliderTheme.trackHeight.Value;
            D.assert(overlayWidth >= 0);
            D.assert(trackHeight >= 0);
            D.assert(parentBox.size.width >= overlayWidth);
            D.assert(parentBox.size.height >= trackHeight);

            float trackLeft = offset.dx + overlayWidth / 2;
            float trackTop = offset.dy + (parentBox.size.height - trackHeight) / 2;
            float trackWidth = parentBox.size.width - Mathf.Max(thumbWidth, overlayWidth);
            return Rect.fromLTWH(trackLeft, trackTop, trackWidth, trackHeight);
        }

        public override void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset thumbCenter = null,
            bool isEnabled = false,
            bool isDiscrete = false,
            TextDirection? textDirection = null) {
            D.assert(context != null);
            D.assert(offset != null);
            D.assert(parentBox != null);
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.disabledActiveTrackColor != null);
            D.assert(sliderTheme.disabledInactiveTrackColor != null);
            D.assert(sliderTheme.activeTrackColor != null);
            D.assert(sliderTheme.inactiveTrackColor != null);
            D.assert(sliderTheme.thumbShape != null);
            D.assert(enableAnimation != null);
            D.assert(textDirection != null);
            D.assert(thumbCenter != null);

            if (sliderTheme.trackHeight == 0) {
                return;
            }

            ColorTween activeTrackColorTween = new ColorTween(begin: sliderTheme.disabledActiveTrackColor,
                end: sliderTheme.activeTrackColor);
            ColorTween inactiveTrackColorTween = new ColorTween(begin: sliderTheme.disabledInactiveTrackColor,
                end: sliderTheme.inactiveTrackColor);
            Paint activePaint = new Paint {color = activeTrackColorTween.evaluate(enableAnimation)};
            Paint inactivePaint = new Paint {color = inactiveTrackColorTween.evaluate(enableAnimation)};
            Paint leftTrackPaint = null;
            Paint rightTrackPaint = null;
            switch (textDirection) {
                case TextDirection.ltr:
                    leftTrackPaint = activePaint;
                    rightTrackPaint = inactivePaint;
                    break;
                case TextDirection.rtl:
                    leftTrackPaint = inactivePaint;
                    rightTrackPaint = activePaint;
                    break;
            }

            Rect trackRect = getPreferredRect(
                parentBox: parentBox,
                offset: offset,
                sliderTheme: sliderTheme,
                isEnabled: isEnabled,
                isDiscrete: isDiscrete
            );

            Size thumbSize = sliderTheme.thumbShape.getPreferredSize(isEnabled, isDiscrete);
            Rect leftTrackSegment = Rect.fromLTRB(trackRect.left + trackRect.height / 2, trackRect.top,
                thumbCenter.dx - thumbSize.width / 2, trackRect.bottom);
            if (!leftTrackSegment.isEmpty) {
                context.canvas.drawRect(leftTrackSegment, leftTrackPaint);
            }

            Rect rightTrackSegment = Rect.fromLTRB(thumbCenter.dx + thumbSize.width / 2, trackRect.top, trackRect.right,
                trackRect.bottom);
            if (!rightTrackSegment.isEmpty) {
                context.canvas.drawRect(rightTrackSegment, rightTrackPaint);
            }
        }
    }

    public class RoundedRectSliderTrackShape : SliderTrackShape {
        public RoundedRectSliderTrackShape() {
        }

        public override Rect getPreferredRect(
            RenderBox parentBox = null,
            Offset offset = null,
            SliderThemeData sliderTheme = null,
            bool isEnabled = false,
            bool isDiscrete = false) {
            offset = offset ?? Offset.zero;

            D.assert(parentBox != null);
            D.assert(sliderTheme != null);
            float thumbWidth = sliderTheme.thumbShape.getPreferredSize(isEnabled, isDiscrete).width;
            float overlayWidth = sliderTheme.overlayShape.getPreferredSize(isEnabled, isDiscrete).width;
            float trackHeight = sliderTheme.trackHeight.Value;
            D.assert(overlayWidth >= 0);
            D.assert(trackHeight >= 0);
            D.assert(parentBox.size.width >= overlayWidth);
            D.assert(parentBox.size.height >= trackHeight);

            float trackLeft = offset.dx + overlayWidth / 2;
            float trackTop = offset.dy + (parentBox.size.height - trackHeight) / 2;
            float trackWidth = parentBox.size.width - Mathf.Max(thumbWidth, overlayWidth);
            return Rect.fromLTWH(trackLeft, trackTop, trackWidth, trackHeight);
        }

        public override void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset thumbCenter = null,
            bool isEnabled = false,
            bool isDiscrete = false,
            TextDirection? textDirection = null) {
            D.assert(context != null);
            D.assert(offset != null);
            D.assert(parentBox != null);
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.disabledActiveTrackColor != null);
            D.assert(sliderTheme.disabledInactiveTrackColor != null);
            D.assert(sliderTheme.activeTrackColor != null);
            D.assert(sliderTheme.inactiveTrackColor != null);
            D.assert(sliderTheme.thumbShape != null);
            D.assert(enableAnimation != null);
            D.assert(textDirection != null);
            D.assert(thumbCenter != null);

            if (sliderTheme.trackHeight <= 0) {
                return;
            }

            ColorTween activeTrackColorTween = new ColorTween(begin: sliderTheme.disabledActiveTrackColor,
                end: sliderTheme.activeTrackColor);
            ColorTween inactiveTrackColorTween = new ColorTween(begin: sliderTheme.disabledInactiveTrackColor,
                end: sliderTheme.inactiveTrackColor);
            Paint activePaint = new Paint {color = activeTrackColorTween.evaluate(enableAnimation)};
            Paint inactivePaint = new Paint {color = inactiveTrackColorTween.evaluate(enableAnimation)};
            Paint leftTrackPaint = null;
            Paint rightTrackPaint = null;
            switch (textDirection) {
                case TextDirection.ltr:
                    leftTrackPaint = activePaint;
                    rightTrackPaint = inactivePaint;
                    break;
                case TextDirection.rtl:
                    leftTrackPaint = inactivePaint;
                    rightTrackPaint = activePaint;
                    break;
            }

            Rect trackRect = getPreferredRect(
                parentBox: parentBox,
                offset: offset,
                sliderTheme: sliderTheme,
                isEnabled: isEnabled,
                isDiscrete: isDiscrete
            );

            Rect leftTrackArcRect = Rect.fromLTWH(trackRect.left, trackRect.top, trackRect.height, trackRect.height);
            if (!leftTrackArcRect.isEmpty) {
                context.canvas.drawArc(leftTrackArcRect, Mathf.PI / 2, Mathf.PI, false, leftTrackPaint);
            }

            Rect rightTrackArcRect = Rect.fromLTWH(trackRect.right - trackRect.height / 2, trackRect.top,
                trackRect.height, trackRect.height);
            if (!rightTrackArcRect.isEmpty) {
                context.canvas.drawArc(rightTrackArcRect, -Mathf.PI / 2, Mathf.PI, false, rightTrackPaint);
            }

            Size thumbSize = sliderTheme.thumbShape.getPreferredSize(isEnabled, isDiscrete);
            Rect leftTrackSegment = Rect.fromLTRB(trackRect.left + trackRect.height / 2, trackRect.top,
                thumbCenter.dx - thumbSize.width / 2, trackRect.bottom);
            if (!leftTrackSegment.isEmpty) {
                context.canvas.drawRect(leftTrackSegment, leftTrackPaint);
            }

            Rect rightTrackSegment = Rect.fromLTRB(thumbCenter.dx + thumbSize.width / 2, trackRect.top, trackRect.right,
                trackRect.bottom);
            if (!rightTrackSegment.isEmpty) {
                context.canvas.drawRect(rightTrackSegment, rightTrackPaint);
            }
        }
    }

    public class RectangularRangeSliderTrackShape : RangeSliderTrackShape {
        public RectangularRangeSliderTrackShape() {
        }

        public override Rect getPreferredRect(
            RenderBox parentBox = null,
            Offset offset = null,
            SliderThemeData sliderTheme = null,
            bool isEnabled = false,
            bool isDiscrete = false
        ) {
            offset = offset ?? Offset.zero;

            D.assert(parentBox != null);
            D.assert(offset != null);
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.overlayShape != null);


            float overlayWidth = sliderTheme.overlayShape.getPreferredSize(isEnabled, isDiscrete).width;
            float trackHeight = sliderTheme.trackHeight.Value;

            D.assert(overlayWidth >= 0);
            D.assert(trackHeight >= 0);
            D.assert(parentBox.size.width >= overlayWidth);
            D.assert(parentBox.size.height >= trackHeight);

            float trackLeft = offset.dx + overlayWidth / 2;
            float trackTop = offset.dy + (parentBox.size.height - trackHeight) / 2;
            float trackWidth = parentBox.size.width - overlayWidth;
            return Rect.fromLTWH(trackLeft, trackTop, trackWidth, trackHeight);
        }

        public override void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset startThumbCenter = null,
            Offset endThumbCenter = null,
            bool isEnabled = false,
            bool isDiscrete = false,
            TextDirection? textDirection = null
        ) {
            D.assert(context != null);
            D.assert(offset != null);
            D.assert(parentBox != null);
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.disabledActiveTrackColor != null);
            D.assert(sliderTheme.disabledInactiveTrackColor != null);
            D.assert(sliderTheme.activeTrackColor != null);
            D.assert(sliderTheme.inactiveTrackColor != null);
            D.assert(sliderTheme.rangeThumbShape != null);
            D.assert(enableAnimation != null);
            D.assert(startThumbCenter != null);
            D.assert(endThumbCenter != null);
            D.assert(textDirection != null);

            ColorTween activeTrackColorTween = new ColorTween(begin: sliderTheme.disabledActiveTrackColor,
                end: sliderTheme.activeTrackColor);
            ColorTween inactiveTrackColorTween = new ColorTween(begin: sliderTheme.disabledInactiveTrackColor,
                end: sliderTheme.inactiveTrackColor);
            Paint activePaint = new Paint {color = activeTrackColorTween.evaluate(enableAnimation)};
            Paint inactivePaint = new Paint {color = inactiveTrackColorTween.evaluate(enableAnimation)};

            Offset leftThumbOffset = null;
            Offset rightThumbOffset = null;
            switch (textDirection) {
                case TextDirection.ltr:
                    leftThumbOffset = startThumbCenter;
                    rightThumbOffset = endThumbCenter;
                    break;
                case TextDirection.rtl:
                    leftThumbOffset = endThumbCenter;
                    rightThumbOffset = startThumbCenter;
                    break;
            }

            Size thumbSize = sliderTheme.rangeThumbShape.getPreferredSize(isEnabled, isDiscrete);
            float thumbRadius = thumbSize.width / 2;

            Rect trackRect = getPreferredRect(
                parentBox: parentBox,
                offset: offset,
                sliderTheme: sliderTheme,
                isEnabled: isEnabled,
                isDiscrete: isDiscrete
            );
            Rect leftTrackSegment = Rect.fromLTRB(trackRect.left, trackRect.top, leftThumbOffset.dx - thumbRadius,
                trackRect.bottom);
            if (!leftTrackSegment.isEmpty) {
                context.canvas.drawRect(leftTrackSegment, inactivePaint);
            }

            Rect middleTrackSegment = Rect.fromLTRB(leftThumbOffset.dx + thumbRadius, trackRect.top,
                rightThumbOffset.dx - thumbRadius, trackRect.bottom);
            if (!middleTrackSegment.isEmpty) {
                context.canvas.drawRect(middleTrackSegment, activePaint);
            }

            Rect rightTrackSegment = Rect.fromLTRB(rightThumbOffset.dx + thumbRadius, trackRect.top, trackRect.right,
                trackRect.bottom);
            if (!rightTrackSegment.isEmpty) {
                context.canvas.drawRect(rightTrackSegment, inactivePaint);
            }
        }
    }

    public class RoundedRectRangeSliderTrackShape : RangeSliderTrackShape {
        public override Rect getPreferredRect(
            RenderBox parentBox = null,
            Offset offset = null,
            SliderThemeData sliderTheme = null,
            bool isEnabled = false,
            bool isDiscrete = false
        ) {
            offset = offset ?? Offset.zero;

            D.assert(parentBox != null);
            D.assert(offset != null);
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.overlayShape != null);
            D.assert(sliderTheme.trackHeight != null);

            float overlayWidth = sliderTheme.overlayShape.getPreferredSize(isEnabled, isDiscrete).width;
            float trackHeight = sliderTheme.trackHeight.Value;

            D.assert(overlayWidth >= 0);
            D.assert(trackHeight >= 0);
            D.assert(parentBox.size.width >= overlayWidth);
            D.assert(parentBox.size.height >= trackHeight);

            float trackLeft = offset.dx + overlayWidth / 2;
            float trackTop = offset.dy + (parentBox.size.height - trackHeight) / 2;
            float trackWidth = parentBox.size.width - overlayWidth;
            return Rect.fromLTWH(trackLeft, trackTop, trackWidth, trackHeight);
        }

        public override void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset startThumbCenter = null,
            Offset endThumbCenter = null,
            bool isEnabled = false,
            bool isDiscrete = false,
            TextDirection? textDirection = null
        ) {
            D.assert(context != null);
            D.assert(offset != null);
            D.assert(parentBox != null);
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.disabledActiveTrackColor != null);
            D.assert(sliderTheme.disabledInactiveTrackColor != null);
            D.assert(sliderTheme.activeTrackColor != null);
            D.assert(sliderTheme.inactiveTrackColor != null);
            D.assert(sliderTheme.rangeThumbShape != null);
            D.assert(enableAnimation != null);
            D.assert(startThumbCenter != null);
            D.assert(endThumbCenter != null);
            D.assert(textDirection != null);

            ColorTween activeTrackColorTween = new ColorTween(begin: sliderTheme.disabledActiveTrackColor,
                end: sliderTheme.activeTrackColor);
            ColorTween inactiveTrackColorTween = new ColorTween(begin: sliderTheme.disabledInactiveTrackColor,
                end: sliderTheme.inactiveTrackColor);
            Paint activePaint = new Paint {color = activeTrackColorTween.evaluate(enableAnimation)};
            Paint inactivePaint = new Paint {color = inactiveTrackColorTween.evaluate(enableAnimation)};

            Offset leftThumbOffset = null;
            Offset rightThumbOffset = null;
            switch (textDirection) {
                case TextDirection.ltr:
                    leftThumbOffset = startThumbCenter;
                    rightThumbOffset = endThumbCenter;
                    break;
                case TextDirection.rtl:
                    leftThumbOffset = endThumbCenter;
                    rightThumbOffset = startThumbCenter;
                    break;
            }

            Size thumbSize = sliderTheme.rangeThumbShape.getPreferredSize(isEnabled, isDiscrete);
            float thumbRadius = thumbSize.width / 2;
            D.assert(thumbRadius > 0);

            Rect trackRect = getPreferredRect(
                parentBox: parentBox,
                offset: offset,
                sliderTheme: sliderTheme,
                isEnabled: isEnabled,
                isDiscrete: isDiscrete
            );

            float trackRadius = trackRect.height / 2;

            Rect leftTrackArcRect = Rect.fromLTWH(trackRect.left, trackRect.top, trackRect.height, trackRect.height);
            if (!leftTrackArcRect.isEmpty) {
                context.canvas.drawArc(leftTrackArcRect, Mathf.PI / 2, Mathf.PI, false, inactivePaint);
            }

            Rect leftTrackSegment = Rect.fromLTRB(trackRect.left + trackRadius, trackRect.top,
                leftThumbOffset.dx - thumbRadius, trackRect.bottom);
            if (!leftTrackSegment.isEmpty) {
                context.canvas.drawRect(leftTrackSegment, inactivePaint);
            }

            Rect middleTrackSegment = Rect.fromLTRB(leftThumbOffset.dx + thumbRadius, trackRect.top,
                rightThumbOffset.dx - thumbRadius, trackRect.bottom);
            if (!middleTrackSegment.isEmpty) {
                context.canvas.drawRect(middleTrackSegment, activePaint);
            }

            Rect rightTrackSegment = Rect.fromLTRB(rightThumbOffset.dx + thumbRadius, trackRect.top,
                trackRect.right - trackRadius, trackRect.bottom);
            if (!rightTrackSegment.isEmpty) {
                context.canvas.drawRect(rightTrackSegment, inactivePaint);
            }

            Rect rightTrackArcRect = Rect.fromLTWH(trackRect.right - trackRect.height, trackRect.top, trackRect.height,
                trackRect.height);
            if (!rightTrackArcRect.isEmpty) {
                context.canvas.drawArc(rightTrackArcRect, -Mathf.PI / 2, Mathf.PI, false, inactivePaint);
            }
        }
    }

    public class RoundSliderTickMarkShape : SliderTickMarkShape {
        public RoundSliderTickMarkShape(
            float? tickMarkRadius = null) {
            this.tickMarkRadius = tickMarkRadius;
        }

        public readonly float? tickMarkRadius;


        public override Size getPreferredSize(
            SliderThemeData sliderTheme = null,
            bool isEnabled = false
        ) {
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.trackHeight != null);
            return Size.fromRadius(tickMarkRadius ?? sliderTheme.trackHeight.Value / 2f);
        }


        public override void paint(
            PaintingContext context,
            Offset center,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset thumbCenter = null,
            bool isEnabled = false,
            TextDirection? textDirection = null
        ) {
            D.assert(context != null);
            D.assert(center != null);
            D.assert(parentBox != null);
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.disabledActiveTickMarkColor != null);
            D.assert(sliderTheme.disabledInactiveTickMarkColor != null);
            D.assert(sliderTheme.activeTickMarkColor != null);
            D.assert(sliderTheme.inactiveTickMarkColor != null);
            D.assert(enableAnimation != null);
            D.assert(textDirection != null);
            D.assert(thumbCenter != null);

            Color begin = null;
            Color end = null;
            switch (textDirection) {
                case TextDirection.ltr:
                    bool isTickMarkRightOfThumb = center.dx > thumbCenter.dx;
                    begin = isTickMarkRightOfThumb
                        ? sliderTheme.disabledInactiveTickMarkColor
                        : sliderTheme.disabledActiveTickMarkColor;
                    end = isTickMarkRightOfThumb ? sliderTheme.inactiveTickMarkColor : sliderTheme.activeTickMarkColor;
                    break;
                case TextDirection.rtl:
                    bool isTickMarkLeftOfThumb = center.dx < thumbCenter.dx;
                    begin = isTickMarkLeftOfThumb
                        ? sliderTheme.disabledInactiveTickMarkColor
                        : sliderTheme.disabledActiveTickMarkColor;
                    end = isTickMarkLeftOfThumb ? sliderTheme.inactiveTickMarkColor : sliderTheme.activeTickMarkColor;
                    break;
            }

            Paint paint = new Paint {color = new ColorTween(begin: begin, end: end).evaluate(enableAnimation)};

            float tickMarkRadius = getPreferredSize(
                                       isEnabled: isEnabled,
                                       sliderTheme: sliderTheme
                                   ).width / 2f;
            if (tickMarkRadius > 0) {
                context.canvas.drawCircle(center, tickMarkRadius, paint);
            }
        }
    }

    class RoundRangeSliderTickMarkShape : RangeSliderTickMarkShape {
        public RoundRangeSliderTickMarkShape(float? tickMarkRadius = null) {
            this.tickMarkRadius = tickMarkRadius;
        }

        public readonly float? tickMarkRadius;

        public override Size getPreferredSize(
            SliderThemeData sliderTheme = null,
            bool isEnabled = false
        ) {
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.trackHeight != null);

            return Size.fromRadius(tickMarkRadius ?? sliderTheme.trackHeight.Value / 2f);
        }

        public override void paint(
            PaintingContext context,
            Offset center,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset startThumbCenter = null,
            Offset endThumbCenter = null,
            bool isEnabled = false,
            TextDirection? textDirection = null
        ) {
            D.assert(context != null);
            D.assert(center != null);
            D.assert(parentBox != null);
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.disabledActiveTickMarkColor != null);
            D.assert(sliderTheme.disabledInactiveTickMarkColor != null);
            D.assert(sliderTheme.activeTickMarkColor != null);
            D.assert(sliderTheme.inactiveTickMarkColor != null);
            D.assert(enableAnimation != null);
            D.assert(startThumbCenter != null);
            D.assert(endThumbCenter != null);
            D.assert(textDirection != null);

            bool isBetweenThumbs = false;
            switch (textDirection) {
                case TextDirection.ltr:
                    isBetweenThumbs = startThumbCenter.dx < center.dx && center.dx < endThumbCenter.dx;
                    break;
                case TextDirection.rtl:
                    isBetweenThumbs = endThumbCenter.dx < center.dx && center.dx < startThumbCenter.dx;
                    break;
            }

            Color begin = isBetweenThumbs
                ? sliderTheme.disabledActiveTickMarkColor
                : sliderTheme.disabledInactiveTickMarkColor;
            Color end = isBetweenThumbs ? sliderTheme.activeTickMarkColor : sliderTheme.inactiveTickMarkColor;
            Paint paint = new Paint {color = new ColorTween(begin: begin, end: end).evaluate(enableAnimation)};

            float tickMarkRadius = getPreferredSize(
                                       isEnabled: isEnabled,
                                       sliderTheme: sliderTheme
                                   ).width / 2;
            if (tickMarkRadius > 0) {
                context.canvas.drawCircle(center, tickMarkRadius, paint);
            }
        }
    }

    class _EmptySliderTickMarkShape : SliderTickMarkShape {
        public override Size getPreferredSize(
            SliderThemeData sliderTheme = null,
            bool isEnabled = false) {
            return Size.zero;
        }

        public override void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset thumbCenter = null,
            bool isEnabled = false,
            TextDirection? textDirection = null) {
        }
    }

    class _EmptySliderComponentShape : SliderComponentShape {
        public override Size getPreferredSize(
            bool isEnabled = false,
            bool isDiscrete = false,
            TextPainter textPainter = null) {
            return Size.zero;
        }

        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            TextDirection? textDirection = null,
            float? value = null) {
        }
    }

    public class RoundSliderThumbShape : SliderComponentShape {
        public RoundSliderThumbShape(
            float enabledThumbRadius = 10.0f,
            float? disabledThumbRadius = null
        ) {
            this.enabledThumbRadius = enabledThumbRadius;
            this.disabledThumbRadius = disabledThumbRadius;
        }

        public readonly float enabledThumbRadius;

        public readonly float? disabledThumbRadius;

        float _disabledThumbRadius {
            get { return disabledThumbRadius ?? enabledThumbRadius; }
        }


        public override Size getPreferredSize(bool isEnabled, bool isDiscrete, TextPainter textPainter = null) {
            return Size.fromRadius(isEnabled == true ? enabledThumbRadius : _disabledThumbRadius);
        }


        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            TextDirection? textDirection = null,
            float? value = null
        ) {
            D.assert(context != null);
            D.assert(center != null);
            D.assert(enableAnimation != null);
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.disabledThumbColor != null);
            D.assert(sliderTheme.thumbColor != null);

            Canvas canvas = context.canvas;
            Tween<float> radiusTween = new FloatTween(
                begin: _disabledThumbRadius,
                end: enabledThumbRadius
            );
            ColorTween colorTween = new ColorTween(
                begin: sliderTheme.disabledThumbColor,
                end: sliderTheme.thumbColor
            );
            canvas.drawCircle(
                center,
                radiusTween.evaluate(enableAnimation),
                new Paint {color = colorTween.evaluate(enableAnimation)}
            );
        }
    }

    public class RoundRangeSliderThumbShape : RangeSliderThumbShape {
        public RoundRangeSliderThumbShape(
            float enabledThumbRadius = 10.0f,
            float? disabledThumbRadius = null
        ) {
            this.enabledThumbRadius = enabledThumbRadius;
            this.disabledThumbRadius = disabledThumbRadius;
        }

        public readonly float enabledThumbRadius;

        public readonly float? disabledThumbRadius;

        float _disabledThumbRadius {
            get { return disabledThumbRadius ?? enabledThumbRadius; }
        }

        public override Size getPreferredSize(bool isEnabled, bool isDiscrete) {
            return Size.fromRadius(isEnabled == true ? enabledThumbRadius : _disabledThumbRadius);
        }

        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            bool isEnabled = false,
            bool? isOnTop = null,
            TextDirection? textDirection = null,
            SliderThemeData sliderTheme = null,
            Thumb? thumb = null
        ) {
            D.assert(context != null);
            D.assert(center != null);
            D.assert(activationAnimation != null);
            D.assert(sliderTheme != null);
            D.assert(sliderTheme.showValueIndicator != null);
            D.assert(sliderTheme.overlappingShapeStrokeColor != null);
            D.assert(enableAnimation != null);
            Canvas canvas = context.canvas;
            Tween<float> radiusTween = new FloatTween(
                begin: _disabledThumbRadius,
                end: enabledThumbRadius
            );
            ColorTween colorTween = new ColorTween(
                begin: sliderTheme.disabledThumbColor,
                end: sliderTheme.thumbColor
            );
            float radius = radiusTween.evaluate(enableAnimation);

            if (isOnTop == true) {
                bool showValueIndicator = false;
                switch (sliderTheme.showValueIndicator) {
                    case ShowValueIndicator.onlyForDiscrete:
                        showValueIndicator = isDiscrete;
                        break;
                    case ShowValueIndicator.onlyForContinuous:
                        showValueIndicator = !isDiscrete;
                        break;
                    case ShowValueIndicator.always:
                        showValueIndicator = true;
                        break;
                    case ShowValueIndicator.never:
                        showValueIndicator = false;
                        break;
                }

                if (!showValueIndicator || activationAnimation.value == 0) {
                    Paint strokePaint = new Paint {
                        color = sliderTheme.overlappingShapeStrokeColor,
                        strokeWidth = 1.0f,
                        style = PaintingStyle.stroke
                    };
                    canvas.drawCircle(center, radius, strokePaint);
                }
            }

            canvas.drawCircle(
                center,
                radius,
                new Paint {color = colorTween.evaluate(enableAnimation)}
            );
        }
    }

    public class RoundSliderOverlayShape : SliderComponentShape {
        public RoundSliderOverlayShape(
            float overlayRadius = 24.0f) {
            this.overlayRadius = overlayRadius;
        }

        public readonly float overlayRadius;

        public override Size getPreferredSize(bool isEnabled, bool isDiscrete, TextPainter textPainter = null) {
            return Size.fromRadius(overlayRadius);
        }

        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            TextDirection? textDirection = null,
            float? value = null
        ) {
            Canvas canvas = context.canvas;
            FloatTween radiusTween = new FloatTween(
                begin: 0.0f,
                end: overlayRadius
            );

            canvas.drawCircle(
                center,
                radiusTween.evaluate(activationAnimation),
                new Paint {color = sliderTheme.overlayColor}
            );
        }
    }

    public class PaddleSliderValueIndicatorShape : SliderComponentShape {
        public PaddleSliderValueIndicatorShape() {
        }

        static readonly _PaddleSliderTrackShapePathPainter _pathPainter = new _PaddleSliderTrackShapePathPainter();

        public override Size getPreferredSize(
            bool isEnabled,
            bool isDiscrete,
            TextPainter labelPainter = null) {
            D.assert(labelPainter != null);
            return _pathPainter.getPreferredSize(isEnabled, isDiscrete, labelPainter);
        }

        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            TextDirection? textDirection = null,
            float? value = null
        ) {
            D.assert(context != null);
            D.assert(center != null);
            D.assert(activationAnimation != null);
            D.assert(enableAnimation != null);
            D.assert(labelPainter != null);
            D.assert(parentBox != null);
            D.assert(sliderTheme != null);

            ColorTween enableColor = new ColorTween(
                begin: sliderTheme.disabledThumbColor,
                end: sliderTheme.valueIndicatorColor
            );

            _pathPainter.drawValueIndicator(
                parentBox,
                context.canvas,
                center,
                new Paint {color = enableColor.evaluate(enableAnimation)},
                activationAnimation.value,
                labelPainter,
                null
            );
        }
    }

    public class PaddleRangeSliderValueIndicatorShape : RangeSliderValueIndicatorShape {
        public PaddleRangeSliderValueIndicatorShape() {
        }

        static readonly _PaddleSliderTrackShapePathPainter _pathPainter = new _PaddleSliderTrackShapePathPainter();

        public override Size getPreferredSize(bool isEnabled, bool isDiscrete, TextPainter labelPainter = null) {
            D.assert(labelPainter != null);
            return _pathPainter.getPreferredSize(isEnabled, isDiscrete, labelPainter);
        }

        public override float getHorizontalShift(
            RenderBox parentBox = null,
            Offset center = null,
            TextPainter labelPainter = null,
            Animation<float> activationAnimation = null
        ) {
            return _pathPainter.getHorizontalShift(
                parentBox: parentBox,
                center: center,
                labelPainter: labelPainter,
                scale: activationAnimation.value
            );
        }

        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            bool? isOnTop = null,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            TextDirection? textDirection = null,
            float? value = null,
            Thumb? thumb = null
        ) {
            D.assert(context != null);
            D.assert(center != null);
            D.assert(activationAnimation != null);
            D.assert(enableAnimation != null);
            D.assert(labelPainter != null);
            D.assert(parentBox != null);
            D.assert(sliderTheme != null);
            ColorTween enableColor = new ColorTween(
                begin: sliderTheme.disabledThumbColor,
                end: sliderTheme.valueIndicatorColor
            );

            _pathPainter.drawValueIndicator(
                parentBox,
                context.canvas,
                center,
                new Paint {color = enableColor.evaluate(enableAnimation)},
                activationAnimation.value,
                labelPainter,
                isOnTop == true ? sliderTheme.overlappingShapeStrokeColor : null
            );
        }
    }

    class _PaddleSliderTrackShapePathPainter {
        public _PaddleSliderTrackShapePathPainter() {
        }


        const float _topLobeRadius = 16.0f;
        const float _labelTextDesignSize = 14.0f;
        const float _bottomLobeRadius = 10.0f;
        const float _labelPadding = 8.0f;
        const float _distanceBetweenTopBottomCenters = 40.0f;
        const float _middleNeckWidth = 2.0f;
        const float _bottomNeckRadius = 4.5f;
        const float _neckTriangleBase = _topNeckRadius + _middleNeckWidth / 2f;
        const float _rightBottomNeckCenterX = _middleNeckWidth / 2f + _bottomNeckRadius;
        const float _rightBottomNeckAngleStart = Mathf.PI;
        static readonly Offset _topLobeCenter = new Offset(0.0f, -_distanceBetweenTopBottomCenters);
        const float _topNeckRadius = 13.0f;
        const float _neckTriangleHypotenuse = _topLobeRadius + _topNeckRadius;
        const float _twoSeventyDegrees = 3.0f * Mathf.PI / 2.0f;
        const float _ninetyDegrees = Mathf.PI / 2.0f;
        const float _thirtyDegrees = Mathf.PI / 6.0f;
        const float _preferredHeight = _distanceBetweenTopBottomCenters + _topLobeRadius + _bottomLobeRadius;
        static readonly bool _debuggingLabelLocation = false;

        public Size getPreferredSize(
            bool isEnabled,
            bool isDiscrete,
            TextPainter labelPainter
        ) {
            D.assert(labelPainter != null);
            float textScaleFactor = labelPainter.height / _labelTextDesignSize;
            return new Size(labelPainter.width + 2 * _labelPadding * textScaleFactor,
                _preferredHeight * textScaleFactor);
        }

        static void _addArc(Path path, Offset center, float radius, float startAngle, float endAngle) {
            D.assert(center.isFinite);
            Rect arcRect = Rect.fromCircle(center: center, radius: radius);
            path.arcTo(arcRect, startAngle, endAngle - startAngle, false);
        }

        internal float getHorizontalShift(
            RenderBox parentBox,
            Offset center,
            TextPainter labelPainter,
            float scale
        ) {
            float textScaleFactor = labelPainter.height / _labelTextDesignSize;
            float inverseTextScale = textScaleFactor != 0 ? 1.0f / textScaleFactor : 0.0f;
            float labelHalfWidth = labelPainter.width / 2.0f;
            float halfWidthNeeded = Mathf.Max(
                0.0f,
                inverseTextScale * labelHalfWidth - (_topLobeRadius - _labelPadding)
            );
            float shift = _getIdealOffset(parentBox, halfWidthNeeded, textScaleFactor * scale, center);
            return shift * textScaleFactor;
        }

        float _getIdealOffset(
            RenderBox parentBox,
            float halfWidthNeeded,
            float scale,
            Offset center
        ) {
            const float edgeMargin = 4.0f;
            Rect topLobeRect = Rect.fromLTWH(
                -_topLobeRadius - halfWidthNeeded,
                -_topLobeRadius - _distanceBetweenTopBottomCenters,
                2.0f * (_topLobeRadius + halfWidthNeeded),
                2.0f * _topLobeRadius
            );

            Offset topLeft = (topLobeRect.topLeft * scale) + center;
            Offset bottomRight = (topLobeRect.bottomRight * scale) + center;
            float shift = 0.0f;

            float startGlobal = parentBox.localToGlobal(Offset.zero).dx;
            if (topLeft.dx < startGlobal + edgeMargin) {
                shift = startGlobal + edgeMargin - topLeft.dx;
            }

            float endGlobal = parentBox.localToGlobal(new Offset(parentBox.size.width, parentBox.size.height)).dx;
            if (bottomRight.dx > endGlobal - edgeMargin) {
                shift = endGlobal - edgeMargin - bottomRight.dx;
            }

            shift = scale == 0.0f ? 0.0f : shift / scale;
            if (shift < 0.0f) {
                shift = Mathf.Max(shift, -halfWidthNeeded);
            }
            else {
                shift = Mathf.Min(shift, halfWidthNeeded);
            }

            return shift;
        }

        public void drawValueIndicator(
            RenderBox parentBox,
            Canvas canvas,
            Offset center,
            Paint paint,
            float scale,
            TextPainter labelPainter,
            Color strokePaintColor
        ) {
            if (scale == 0.0f) {
                return;
            }

            float textScaleFactor = labelPainter.height / _labelTextDesignSize;
            float overallScale = scale * textScaleFactor;
            float inverseTextScale = textScaleFactor != 0 ? 1.0f / textScaleFactor : 0.0f;
            float labelHalfWidth = labelPainter.width / 2.0f;

            canvas.save();
            canvas.translate(center.dx, center.dy);
            canvas.scale(overallScale, overallScale);

            float bottomNeckTriangleHypotenuse = _bottomNeckRadius + _bottomLobeRadius / overallScale;
            float rightBottomNeckCenterY =
                -Mathf.Sqrt(Mathf.Pow(bottomNeckTriangleHypotenuse, 2) - Mathf.Pow(_rightBottomNeckCenterX, 2));
            float rightBottomNeckAngleEnd = Mathf.PI + Mathf.Atan(rightBottomNeckCenterY / _rightBottomNeckCenterX);
            Path path = new Path();
            path.moveTo(_middleNeckWidth / 2, rightBottomNeckCenterY);
            _addArc(
                path,
                new Offset(_rightBottomNeckCenterX, rightBottomNeckCenterY),
                _bottomNeckRadius,
                _rightBottomNeckAngleStart,
                rightBottomNeckAngleEnd
            );
            _addArc(
                path,
                Offset.zero,
                _bottomLobeRadius / overallScale,
                rightBottomNeckAngleEnd - Mathf.PI,
                2 * Mathf.PI - rightBottomNeckAngleEnd
            );
            _addArc(
                path,
                new Offset(-_rightBottomNeckCenterX, rightBottomNeckCenterY),
                _bottomNeckRadius,
                Mathf.PI - rightBottomNeckAngleEnd,
                0
            );

            float halfWidthNeeded = Mathf.Max(
                0.0f,
                inverseTextScale * labelHalfWidth - (_topLobeRadius - _labelPadding)
            );

            float shift = _getIdealOffset(parentBox, halfWidthNeeded, overallScale, center);
            float leftWidthNeeded = halfWidthNeeded - shift;
            float rightWidthNeeded = halfWidthNeeded + shift;

            float leftAmount = Mathf.Max(0.0f, Mathf.Min(1.0f, leftWidthNeeded / _neckTriangleBase));
            float rightAmount = Mathf.Max(0.0f, Mathf.Min(1.0f, rightWidthNeeded / _neckTriangleBase));

            float leftTheta = (1.0f - leftAmount) * _thirtyDegrees;
            float rightTheta = (1.0f - rightAmount) * _thirtyDegrees;

            Offset leftTopNeckCenter = new Offset(
                -_neckTriangleBase,
                _topLobeCenter.dy + Mathf.Cos(leftTheta) * _neckTriangleHypotenuse
            );

            Offset neckRightCenter = new Offset(
                _neckTriangleBase,
                _topLobeCenter.dy + Mathf.Cos(rightTheta) * _neckTriangleHypotenuse
            );

            float leftNeckArcAngle = _ninetyDegrees - leftTheta;
            float rightNeckArcAngle = Mathf.PI + _ninetyDegrees - rightTheta;

            float neckStretchBaseline = Mathf.Max(0.0f,
                rightBottomNeckCenterY - Mathf.Max(leftTopNeckCenter.dy, neckRightCenter.dy));
            float t = Mathf.Pow(inverseTextScale, 3.0f);
            float stretch = (neckStretchBaseline * t).clamp(0.0f, 10.0f * neckStretchBaseline);
            Offset neckStretch = new Offset(0.0f, neckStretchBaseline - stretch);

            D.assert(() => {
                if (!_debuggingLabelLocation) {
                    return true;
                }

                Offset leftCenter = _topLobeCenter - new Offset(leftWidthNeeded, 0.0f) + neckStretch;
                Offset rightCenter = _topLobeCenter + new Offset(rightWidthNeeded, 0.0f) + neckStretch;
                Rect valueRect = Rect.fromLTRB(
                    leftCenter.dx - _topLobeRadius,
                    leftCenter.dy - _topLobeRadius,
                    rightCenter.dx + _topLobeRadius,
                    rightCenter.dy + _topLobeRadius
                );
                Paint outlinePaint = new Paint {
                    color = new Color(0xffff0000),
                    style = PaintingStyle.stroke,
                    strokeWidth = 1.0f
                };
                canvas.drawRect(valueRect, outlinePaint);
                return true;
            });

            _addArc(
                path,
                leftTopNeckCenter + neckStretch,
                _topNeckRadius,
                0.0f,
                -leftNeckArcAngle
            );
            _addArc(
                path,
                _topLobeCenter - new Offset(leftWidthNeeded, 0.0f) + neckStretch,
                _topLobeRadius,
                _ninetyDegrees + leftTheta,
                _twoSeventyDegrees
            );
            _addArc(
                path,
                _topLobeCenter + new Offset(rightWidthNeeded, 0.0f) + neckStretch,
                _topLobeRadius,
                _twoSeventyDegrees,
                _twoSeventyDegrees + Mathf.PI - rightTheta
            );
            _addArc(
                path,
                neckRightCenter + neckStretch,
                _topNeckRadius,
                rightNeckArcAngle,
                Mathf.PI
            );

            if (strokePaintColor != null) {
                Paint strokePaint = new Paint {
                    color = strokePaintColor,
                    strokeWidth = 1.0f,
                    style = PaintingStyle.stroke
                };

                canvas.drawPath(path, strokePaint);
            }

            canvas.drawPath(path, paint);

            canvas.save();
            canvas.translate(shift, -_distanceBetweenTopBottomCenters + neckStretch.dy);
            canvas.scale(inverseTextScale, inverseTextScale);
            labelPainter.paint(canvas, Offset.zero - new Offset(labelHalfWidth, labelPainter.height / 2.0f));
            canvas.restore();
            canvas.restore();
        }
    }
    
    public delegate Thumb? RangeThumbSelector(TextDirection textDirection,
    RangeValues values,
    float tapValue,
    Size thumbSize,
    Size trackSize,
    float dx
    );

    public class RangeValues {
        public RangeValues(
            float start,
            float end
        ) {
            this.start = start;
            this.end = end;
        }
        
        public readonly float start;

        public readonly float end;

        public bool Equals(RangeValues obj) {
            if (ReferenceEquals(obj, null)) {
                return false;
            }
            
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return start.Equals(obj.start) 
                   && end.Equals(obj.end);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(obj, null)) {
                return false;
            }
            
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            
            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((RangeValues) obj);
        }
        
        public static bool operator ==(RangeValues left, RangeValues right) {
            return Equals(left, right);
        }

        public static bool operator !=(RangeValues left, RangeValues right) {
            return !Equals(left, right);
        }
        
        public override int GetHashCode() {
            unchecked {
                return (start.GetHashCode() * 397) ^ end.GetHashCode();
            }
        }

        public override string ToString() {
            return $"{GetType()}({start}, {end})";
        }
    }
    
    
    public class RangeLabels {
        public RangeLabels(
            string start,
            string end
        ) {
            this.start = start;
            this.end = end;
        }
        
        public readonly string start;

        public readonly string end;

        public bool Equals(RangeLabels obj) {
            if (ReferenceEquals(obj, null)) {
                return false;
            }
            
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return start.Equals(obj.start) 
                   && end.Equals(obj.end);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(obj, null)) {
                return false;
            }
            
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            
            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((RangeLabels) obj);
        }
        
        public static bool operator ==(RangeLabels left, RangeLabels right) {
            return Equals(left, right);
        }

        public static bool operator !=(RangeLabels left, RangeLabels right) {
            return !Equals(left, right);
        }
        
        public override int GetHashCode() {
            unchecked {
                return (start.GetHashCode() * 397) ^ end.GetHashCode();
            }
        }

        public override string ToString() {
            return $"{GetType()}({start}, {end})";
        }
    }
}