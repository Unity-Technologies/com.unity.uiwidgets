using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    class RenderImage : RenderBox {
        public RenderImage(
            Image image = null,
            float? width = null,
            float? height = null,
            float scale = 1.0f,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            AlignmentGeometry alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool matchTextDirection = false,
            TextDirection? textDirection = null,
            bool invertColors = false,
            FilterQuality filterQuality = FilterQuality.low
        ) {
            D.assert(alignment != null);
            _image = image;
            _width = width;
            _height = height;
            _scale = scale;
            _color = color;
            _colorBlendMode = colorBlendMode;
            _fit = fit;
            _repeat = repeat;
            _centerSlice = centerSlice;
            _alignment = alignment ?? Alignment.center;
            _invertColors = invertColors;
            _filterQuality = filterQuality;
            _textDirection = textDirection;
            _matchTextDirection = matchTextDirection;
            _updateColorFilter();
        }
        Alignment _resolvedAlignment;
        bool _flipHorizontally;
        void _resolve() {
            if (_resolvedAlignment != null)
                return;
            _resolvedAlignment = alignment.resolve(textDirection);
            _flipHorizontally = matchTextDirection && textDirection == TextDirection.rtl;
        }

        
        void _markNeedResolution() {
            _resolvedAlignment = null;
            _flipHorizontally = false;
            markNeedsPaint();
        }

        Image _image;

        public Image image {
            get { return _image; }
            set {
                if (value == _image) {
                    return;
                }

                _image = value;
                markNeedsPaint();
                if (_width == null || _height == null) {
                    markNeedsLayout();
                }
            }
        }

        float? _width;

        public float? width {
            get { return _width; }
            set {
                if (value == _width) {
                    return;
                }

                _width = value;
                markNeedsLayout();
            }
        }

        float? _height;

        public float? height {
            get { return _height; }
            set {
                if (value == _height) {
                    return;
                }

                _height = value;
                markNeedsLayout();
            }
        }

        float _scale;

        public float scale {
            get { return _scale; }
            set {
                if (value == _scale) {
                    return;
                }

                _scale = value;
                markNeedsLayout();
            }
        }
        
        
        ColorFilter _colorFilter;

        void _updateColorFilter() {
            if (_color == null) {
                _colorFilter = null;
            } else {
                _colorFilter = ColorFilter.mode(_color, _colorBlendMode);
            }
        }

        Color _color;

        public Color color {
            get { return _color; }
            set {
                if (value == _color) {
                    return;
                }

                _color = value;
                _updateColorFilter();
                markNeedsPaint();
            }
        }

        BlendMode _colorBlendMode;

        public BlendMode colorBlendMode {
            get { return _colorBlendMode; }
            set {
                if (value == _colorBlendMode) {
                    return;
                }

                _colorBlendMode = value;
                _updateColorFilter();
                markNeedsPaint();
            }
        }

        FilterQuality _filterQuality;

        public FilterQuality filterQuality {
            get { return _filterQuality; }
            set {
                if (value == _filterQuality) {
                    return;
                }

                _filterQuality = value;
                markNeedsPaint();
            }
        }

        BoxFit? _fit;

        public BoxFit? fit {
            get { return _fit; }
            set {
                if (value == _fit) {
                    return;
                }

                _fit = value;
                markNeedsPaint();
            }
        }

        AlignmentGeometry _alignment;

        public AlignmentGeometry alignment {
            get { return _alignment; }
            set {
                if (value == _alignment) {
                    return;
                }

                _alignment = value;
                markNeedsPaint();
            }
        }

        ImageRepeat _repeat;

        public ImageRepeat repeat {
            get { return _repeat; }
            set {
                if (value == _repeat) {
                    return;
                }

                _repeat = value;
                markNeedsPaint();
            }
        }

        Rect _centerSlice;

        public Rect centerSlice {
            get { return _centerSlice; }
            set {
                if (value == _centerSlice) {
                    return;
                }

                _centerSlice = value;
                markNeedsPaint();
            }
        }

        bool _invertColors;

        public bool invertColors {
            get { return _invertColors; }
            set {
                if (value == _invertColors) {
                    return;
                }

                _invertColors = value;
                markNeedsPaint();
            }
        }

        public bool matchTextDirection {
            get { return _matchTextDirection; }
            set {
                if (value == _matchTextDirection)
                    return;
                _matchTextDirection = value;
                _markNeedResolution();
            }
        }
        bool _matchTextDirection;

        public TextDirection? textDirection {
            get { return _textDirection; }
            set {
                if (_textDirection == value)
                    return;
                _textDirection = value;
                _markNeedResolution();
            }
        }
        TextDirection? _textDirection;
       
       

        Size _sizeForConstraints(BoxConstraints constraints) {
            constraints = BoxConstraints.tightFor(
                _width,
                _height
            ).enforce(constraints);

            if (_image == null) {
                return constraints.smallest;
            }

            return constraints.constrainSizeAndAttemptToPreserveAspectRatio(new Size(
                (_image.width / _scale),
                (_image.height / _scale)
            ));
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            D.assert(height >= 0.0);
            if (_width == null && _height == null) {
                return 0.0f;
            }

            return _sizeForConstraints(BoxConstraints.tightForFinite(height: height)).width;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            D.assert(height >= 0.0);
            return _sizeForConstraints(BoxConstraints.tightForFinite(height: height)).width;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            D.assert(width >= 0.0);
            if (_width == null && _height == null) {
                return 0.0f;
            }

            return _sizeForConstraints(BoxConstraints.tightForFinite(width: width)).height;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            D.assert(width >= 0.0);
            return _sizeForConstraints(BoxConstraints.tightForFinite(width: width)).height;
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        protected override void performLayout() {
            size = _sizeForConstraints(constraints);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (_image == null) {
                return;
            }
            _resolve();
            D.assert(_resolvedAlignment != null);
            painting_.paintImage(
                canvas: context.canvas,
                rect: offset & size,
                image: _image,
                scale: _scale,
                colorFilter: _colorFilter,
                fit: _fit,
                alignment: _resolvedAlignment,
                centerSlice: _centerSlice,
                repeat: _repeat,
                flipHorizontally: _flipHorizontally,
                invertColors: invertColors,
                filterQuality: _filterQuality
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Image>("image", image));
            properties.add(new FloatProperty("width", width, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new FloatProperty("height", height, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new FloatProperty("scale", scale, defaultValue: 1.0f));
            properties.add(new ColorProperty("color", color,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<BlendMode>("colorBlendMode", colorBlendMode,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<BoxFit?>("fit", fit, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<ImageRepeat>("repeat", repeat, defaultValue: ImageRepeat.noRepeat));
            properties.add(new DiagnosticsProperty<Rect>("centerSlice", centerSlice,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<bool>("invertColors", invertColors));
            properties.add(new EnumProperty<FilterQuality>("filterMode", filterQuality));
            properties.add(new FlagProperty("matchTextDirection", value: matchTextDirection, ifTrue: "match text direction"));
            properties.add(new EnumProperty<TextDirection?>("textDirection", textDirection, defaultValue: null));

        }
    }
}