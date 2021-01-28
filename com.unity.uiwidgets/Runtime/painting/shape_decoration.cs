using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class ShapeDecoration : Decoration, IEquatable<ShapeDecoration> {
        public ShapeDecoration(
            Color color = null,
            DecorationImage image = null,
            Gradient gradient = null,
            List<BoxShadow> shadows = null,
            ShapeBorder shape = null
        ) {
            D.assert(!(color != null && gradient != null));
            D.assert(shape != null);

            this.color = color;
            this.image = image;
            this.gradient = gradient;
            this.shadows = shadows;
            this.shape = shape;
        }

        public readonly Color color;
        public readonly DecorationImage image;
        public readonly Gradient gradient;
        public readonly List<BoxShadow> shadows;
        public readonly ShapeBorder shape;

        public static ShapeDecoration fromBoxDecoration(BoxDecoration source) {
            ShapeBorder shape = null;

            switch (source.shape) {
                case BoxShape.circle:
                    if (source.border != null) {
                        D.assert(source.border.isUniform);
                        shape = new CircleBorder(side: source.border.top);
                    }
                    else {
                        shape = new CircleBorder();
                    }

                    break;
                case BoxShape.rectangle:
                    if (source.borderRadius != null) {
                        D.assert(source.border == null || source.border.isUniform);
                        shape = new RoundedRectangleBorder(
                            side: source.border?.top ?? BorderSide.none,
                            borderRadius: source.borderRadius
                        );
                    }
                    else {
                        shape = source.border ?? new Border();
                    }

                    break;
            }

            return new ShapeDecoration(
                color: source.color,
                image: source.image,
                gradient: source.gradient,
                shadows: source.boxShadow,
                shape: shape
            );
        }

        public override Path getClipPath(Rect rect, TextDirection textDirection) {
            return shape.getOuterPath(rect);
        }

        public override EdgeInsetsGeometry padding {
            get { return shape.dimensions; }
        }

        public override bool isComplex {
            get { return shadows != null; }
        }

        public override Decoration lerpFrom(Decoration a, float t) {
            if (a is BoxDecoration decoration) {
                return lerp(fromBoxDecoration(decoration), this, t);
            }
            else if (a == null || a is ShapeDecoration) {
                return lerp(a, this, t);
            }

            return base.lerpFrom(a, t);
        }

        public override Decoration lerpTo(Decoration b, float t) {
            if (b is BoxDecoration decoration) {
                return lerp(this, fromBoxDecoration(decoration), t);
            }
            else if (b == null || b is ShapeDecoration) {
                return lerp(this, b, t);
            }

            return base.lerpTo(b, t);
        }

        public static ShapeDecoration lerp(ShapeDecoration a, ShapeDecoration b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a != null && b != null) {
                if (t == 0.0) {
                    return a;
                }

                if (t == 1.0) {
                    return b;
                }
            }

            return new ShapeDecoration(
                color: Color.lerp(a?.color, b?.color, t),
                gradient: Gradient.lerp(a?.gradient, b?.gradient, t),
                image: t < 0.5 ? a.image : b.image,
                shadows: BoxShadow.lerpList(a?.shadows, b?.shadows, t),
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t)
            );
        }

        public bool Equals(ShapeDecoration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(color, other.color) && Equals(image, other.image) &&
                   Equals(gradient, other.gradient) && Equals(shadows, other.shadows) &&
                   Equals(shape, other.shape);
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

            return Equals((ShapeDecoration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (image != null ? image.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (gradient != null ? gradient.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (shadows != null ? shadows.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (shape != null ? shape.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ShapeDecoration left, ShapeDecoration right) {
            return Equals(left, right);
        }

        public static bool operator !=(ShapeDecoration left, ShapeDecoration right) {
            return !Equals(left, right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.whitespace;
            properties.add(new ColorProperty("color", color,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Gradient>("gradient", gradient,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<DecorationImage>("image", image,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumerableProperty<BoxShadow>("shadows", shadows,
                defaultValue: foundation_.kNullDefaultValue, style: DiagnosticsTreeStyle.whitespace));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape));
        }

        public override bool hitTest(Size size, Offset position, TextDirection textDirection) {
            return shape.getOuterPath(Offset.zero & size, textDirection: textDirection).contains(position);
        }

        public override BoxPainter createBoxPainter(VoidCallback onChanged = null) {
            D.assert(onChanged != null || image == null);
            return new _ShapeDecorationPainter(this, onChanged);
        }
    }

    class _ShapeDecorationPainter : BoxPainter {
        public _ShapeDecorationPainter(ShapeDecoration decoration, VoidCallback onChanged)
            : base(onChanged) {
            D.assert(decoration != null);
            _decoration = decoration;
        }

        readonly ShapeDecoration _decoration;

        Rect _lastRect;
        Path _outerPath;
        Path _innerPath;
        Paint _interiorPaint;
        int? _shadowCount;
        Path[] _shadowPaths;
        Paint[] _shadowPaints;

        void _precache(Rect rect) {
            D.assert(rect != null);
            if (rect == _lastRect) {
                return;
            }

            if (_interiorPaint == null && (_decoration.color != null || _decoration.gradient != null)) {
                _interiorPaint = new Paint();
                if (_decoration.color != null) {
                    _interiorPaint.color = _decoration.color;
                }
            }

            if (_decoration.gradient != null) {
                // this._interiorPaint.shader = this._decoration.gradient.createShader(rect);
            }

            if (_decoration.shadows != null) {
                if (_shadowCount == null) {
                    _shadowCount = _decoration.shadows.Count;
                    _shadowPaths = new Path[_shadowCount.Value];
                    _shadowPaints = new Paint[_shadowCount.Value];
                    for (int index = 0; index < _shadowCount.Value; index += 1) {
                        _shadowPaints[index] = _decoration.shadows[index].toPaint();
                    }
                }

                for (int index = 0; index < _shadowCount; index += 1) {
                    BoxShadow shadow = _decoration.shadows[index];
                    _shadowPaths[index] = _decoration.shape.getOuterPath(
                        rect.shift(shadow.offset).inflate(shadow.spreadRadius));
                }
            }

            if (_interiorPaint != null || _shadowCount != null) {
                _outerPath = _decoration.shape.getOuterPath(rect);
            }

            if (_decoration.image != null) {
                _innerPath = _decoration.shape.getInnerPath(rect);
            }

            _lastRect = rect;
        }

        void _paintShadows(Canvas canvas) {
            if (_shadowCount != null) {
                for (int index = 0; index < _shadowCount.Value; index += 1) {
                    canvas.drawPath(_shadowPaths[index], _shadowPaints[index]);
                }
            }
        }

        void _paintInterior(Canvas canvas) {
            if (_interiorPaint != null) {
                canvas.drawPath(_outerPath, _interiorPaint);
            }
        }

        DecorationImagePainter _imagePainter;

        void _paintImage(Canvas canvas, ImageConfiguration configuration) {
            if (_decoration.image == null) {
                return;
            }

            _imagePainter = _imagePainter ?? _decoration.image.createPainter(onChanged);
            _imagePainter.paint(canvas, _lastRect, _innerPath, configuration);
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
            _precache(rect);
            _paintShadows(canvas);
            _paintInterior(canvas);
            _paintImage(canvas, configuration);
            _decoration.shape.paint(canvas, rect, textDirection: textDirection);
        }
    }
}