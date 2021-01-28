using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public class BoxDecoration : Decoration, IEquatable<BoxDecoration> {
        public BoxDecoration(
            Color color = null,
            DecorationImage image = null,
            Border border = null,
            BorderRadius borderRadius = null,
            List<BoxShadow> boxShadow = null,
            Gradient gradient = null,
            BlendMode? backgroundBlendMode = null,
            BoxShape shape = BoxShape.rectangle
        ) {
            D.assert(
                backgroundBlendMode == null || color != null || gradient != null,
                () => "backgroundBlendMode applies to BoxDecoration\'s background color or " +
                "gradient, but no color or gradient was provided."
            );

            this.color = color;
            this.image = image;
            this.border = border;
            this.borderRadius = borderRadius;
            this.boxShadow = boxShadow;
            this.gradient = gradient;
            this.backgroundBlendMode = backgroundBlendMode;
            this.shape = shape;
        }

        public BoxDecoration copyWith(
            Color color = null,
            DecorationImage image = null,
            Border border = null,
            BorderRadius borderRadius = null,
            List<BoxShadow> boxShadow = null,
            Gradient gradient = null,
            BlendMode? backgroundBlendMode = null,
            BoxShape? shape = null
        ) {
            return new BoxDecoration(
                color: color ?? this.color,
                image: image ?? this.image,
                border: border ?? this.border,
                borderRadius: borderRadius ?? this.borderRadius,
                boxShadow: boxShadow ?? this.boxShadow,
                gradient: gradient ?? this.gradient,
                backgroundBlendMode: backgroundBlendMode ?? this.backgroundBlendMode,
                shape: shape ?? this.shape
            );
        }
        
        public override bool debugAssertIsValid() {
            D.assert(shape != BoxShape.circle || borderRadius == null);
            return base.debugAssertIsValid();
        }

        public readonly Color color;
        public readonly DecorationImage image;
        public readonly Border border;
        public readonly BorderRadius borderRadius;
        public readonly List<BoxShadow> boxShadow;
        public readonly Gradient gradient;
        public readonly BlendMode? backgroundBlendMode;
        public readonly BoxShape shape;

        public override EdgeInsetsGeometry padding {
            get { return border?.dimensions; }
        }
        
        public override Path getClipPath(Rect rect, TextDirection textDirection) {
            Path clipPath = null;
            switch (shape) {
                case BoxShape.circle:
                    clipPath = new Path();
                    clipPath.addOval(rect);
                    break;
                case BoxShape.rectangle:
                    if (borderRadius != null) {
                        clipPath = new Path();
                        clipPath.addRRect(borderRadius.resolve(textDirection).toRRect(rect));
                    }

                    break;
            }
            return clipPath;
        }

        public BoxDecoration scale(float factor) {
            return new BoxDecoration(
                color: Color.lerp(null, color, factor),
                image: image,
                border: Border.lerp(null, border, factor),
                borderRadius: BorderRadius.lerp(null, borderRadius, factor),
                boxShadow: BoxShadow.lerpList(null, boxShadow, factor),
                gradient: gradient?.scale(factor),
                backgroundBlendMode: backgroundBlendMode,
                shape: shape
            );
        }

        public override bool isComplex {
            get { return boxShadow != null; }
        }

        public override Decoration lerpFrom(Decoration a, float t) {
            if (a == null) {
                return scale(t);
            }

            if (a is BoxDecoration boxDecoration) {
                return lerp(boxDecoration, this, t);
            }

            return base.lerpFrom(a, t);
        }

        public override Decoration lerpTo(Decoration b, float t) {
            if (b == null) {
                return scale(1.0f - t);
            }

            if (b is BoxDecoration boxDecoration) {
                return lerp(this, boxDecoration, t);
            }

            return base.lerpTo(b, t);
        }

        public static BoxDecoration lerp(BoxDecoration a, BoxDecoration b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b.scale(t);
            }

            if (b == null) {
                return a.scale(1.0f - t);
            }

            if (t == 0.0) {
                return a;
            }

            if (t == 1.0) {
                return b;
            }

            return new BoxDecoration(
                color: Color.lerp(a.color, b.color, t),
                image: t < 0.5 ? a.image : b.image,
                border: Border.lerp(a.border, b.border, t),
                borderRadius: BorderRadius.lerp(a.borderRadius, b.borderRadius, t),
                boxShadow: BoxShadow.lerpList(a.boxShadow, b.boxShadow, t),
                gradient: Gradient.lerp(a.gradient, b.gradient, t),
                backgroundBlendMode: t < 0.5 ? a.backgroundBlendMode : b.backgroundBlendMode,
                shape: t < 0.5 ? a.shape : b.shape
            );
        }

        public bool Equals(BoxDecoration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(color, other.color) && Equals(image, other.image) &&
                   Equals(border, other.border) && Equals(borderRadius, other.borderRadius) &&
                   Equals(boxShadow, other.boxShadow) && Equals(gradient, other.gradient) &&
                   backgroundBlendMode == other.backgroundBlendMode && shape == other.shape;
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

            return Equals((BoxDecoration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (image != null ? image.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (border != null ? border.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (borderRadius != null ? borderRadius.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (boxShadow != null ? boxShadow.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (gradient != null ? gradient.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ backgroundBlendMode.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) shape;
                return hashCode;
            }
        }

        public static bool operator ==(BoxDecoration left, BoxDecoration right) {
            return Equals(left, right);
        }

        public static bool operator !=(BoxDecoration left, BoxDecoration right) {
            return !Equals(left, right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.whitespace;
            properties.emptyBodyDescription = "<no decorations specified>";
            properties.add(new DiagnosticsProperty<Color>("color", color,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<DecorationImage>("image", image,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Border>("border", border,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<BorderRadius>("borderRadius", borderRadius,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumerableProperty<BoxShadow>("boxShadow", boxShadow,
                defaultValue: foundation_.kNullDefaultValue, style: DiagnosticsTreeStyle.whitespace));
            properties.add(new DiagnosticsProperty<Gradient>("gradient", gradient,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<BlendMode?>("backgroundBlendMode", backgroundBlendMode,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<BoxShape>("shape", shape, defaultValue: BoxShape.rectangle));
        }

        public override bool hitTest(Size size, Offset position, TextDirection textDirection) {
            D.assert((Offset.zero & size).contains(position));
            switch (shape) {
                case BoxShape.rectangle:
                    if (borderRadius != null) {
                        RRect bounds = borderRadius.toRRect(Offset.zero & size);
                        return bounds.contains(position);
                    }

                    return true;
                case BoxShape.circle:
                    Offset center = size.center(Offset.zero);
                    float distance = (position - center).distance;
                    return distance <= Mathf.Min(size.width, size.height) / 2.0;
            }

            return false;
        }

        public override BoxPainter createBoxPainter(VoidCallback onChanged = null) {
            D.assert(onChanged != null || image == null);
            return new _BoxDecorationPainter(this, onChanged);
        }
    }

    class _BoxDecorationPainter : BoxPainter {
        public _BoxDecorationPainter(BoxDecoration decoration, VoidCallback onChanged)
            : base(onChanged) {
            D.assert(decoration != null);
            _decoration = decoration;
        }

        readonly BoxDecoration _decoration;

        Paint _cachedBackgroundPaint;

        Rect _rectForCachedBackgroundPaint;

        Paint _getBackgroundPaint(Rect rect, TextDirection textDirection) {
            D.assert(rect != null);
            D.assert(_decoration.gradient != null || _rectForCachedBackgroundPaint == null);

            if (_cachedBackgroundPaint == null ||
                (_decoration.gradient != null && _rectForCachedBackgroundPaint != rect)) {
                var paint = new Paint();
                if (_decoration.backgroundBlendMode != null) {
                    paint.blendMode = _decoration.backgroundBlendMode.Value;
                }

                if (_decoration.color != null) {
                    paint.color = _decoration.color;
                }

                if (_decoration.gradient != null) {
                    paint.shader = _decoration.gradient.createShader(rect, textDirection: textDirection);
                    _rectForCachedBackgroundPaint = rect;
                }

                _cachedBackgroundPaint = paint;
            }

            return _cachedBackgroundPaint;
        }

        void _paintBox(Canvas canvas, Rect rect, Paint paint, TextDirection textDirection) {
            switch (_decoration.shape) {
                case BoxShape.circle:
                    D.assert(_decoration.borderRadius == null);
                    Offset center = rect.center;
                    float radius = rect.shortestSide / 2.0f;
                    canvas.drawCircle(center, radius, paint);
                    break;
                case BoxShape.rectangle:
                    if (_decoration.borderRadius == null) {
                        canvas.drawRect(rect, paint);
                    }
                    else {
                        canvas.drawRRect(_decoration.borderRadius.toRRect(rect), paint);
                    }

                    break;
            }
        }

        void _paintShadows(Canvas canvas, Rect rect, TextDirection textDirection) {
            if (_decoration.boxShadow == null) {
                return;
            }

            foreach (BoxShadow boxShadow in _decoration.boxShadow) {
                Paint paint = boxShadow.toPaint();
                Rect bounds = rect.shift(boxShadow.offset).inflate(boxShadow.spreadRadius);
                _paintBox(canvas, bounds, paint, textDirection);
            }
        }

        void _paintBackgroundColor(Canvas canvas, Rect rect, TextDirection textDirection) {
            if (_decoration.color != null || _decoration.gradient != null) {
                _paintBox(canvas, rect, _getBackgroundPaint(rect, textDirection), textDirection);
            }
        }

        DecorationImagePainter _imagePainter;

        void _paintBackgroundImage(Canvas canvas, Rect rect, ImageConfiguration configuration) {
            if (_decoration.image == null) {
                return;
            }

            _imagePainter = _imagePainter ?? _decoration.image.createPainter(onChanged);

            Path clipPath = null;
            switch (_decoration.shape) {
                case BoxShape.circle:
                    clipPath = new Path();
                    clipPath.addOval(rect);
                    break;
                case BoxShape.rectangle:
                    if (_decoration.borderRadius != null) {
                        clipPath = new Path();
                        clipPath.addRRect(_decoration.borderRadius.toRRect(rect));
                    }

                    break;
            }

            _imagePainter.paint(canvas, rect, clipPath, configuration);
        }

        public override void Dispose() {
            _imagePainter?.Dispose();
            base.Dispose();
        }

        public override void paint(Canvas canvas, Offset offset, ImageConfiguration configuration) {
            D.assert(configuration != null);
            D.assert(configuration.size != null);

            Rect rect = offset & configuration.size;
            TextDirection textDirection = configuration.textDirection;

            _paintShadows(canvas, rect, textDirection);
            _paintBackgroundColor(canvas, rect, textDirection);
            _paintBackgroundImage(canvas, rect, configuration);
            _decoration.border?.paint(
                canvas,
                rect,
                shape: _decoration.shape,
                borderRadius: _decoration.borderRadius
            );
        }

        public override string ToString() {
            return $"BoxPainter for {_decoration}";
        }
    }
}