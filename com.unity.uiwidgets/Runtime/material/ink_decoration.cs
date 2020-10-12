using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using ImageUtils = Unity.UIWidgets.widgets.ImageUtils;

namespace Unity.UIWidgets.material {
    public class Ink : StatefulWidget {
        public Ink(
            Key key = null,
            EdgeInsets padding = null,
            Color color = null,
            Decoration decoration = null,
            float? width = null,
            float? height = null,
            Widget child = null) : base(key: key) {
            D.assert(padding == null || padding.isNonNegative);
            D.assert(decoration == null || decoration.debugAssertIsValid());
            D.assert(color == null || decoration == null,
                () => "Cannot provide both a color and a decoration\n" +
                "The color argument is just a shorthand for \"decoration: new BoxDecoration(color: color)\".");
            decoration = decoration ?? (color != null ? new BoxDecoration(color: color) : null);
            this.padding = padding;
            this.width = width;
            this.height = height;
            this.child = child;
            this.decoration = decoration;
        }

        public static Ink image(
            Key key = null,
            EdgeInsets padding = null,
            ImageProvider image = null,
            ColorFilter colorFilter = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            Rect centerSlice = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            float? width = null,
            float? height = null,
            Widget child = null
        ) {
            D.assert(padding == null || padding.isNonNegative);
            D.assert(image != null);

            alignment = alignment ?? Alignment.center;
            Decoration decoration = new BoxDecoration(
                image: new DecorationImage(
                    image: image,
                    colorFilter: colorFilter,
                    fit: fit,
                    alignment: alignment,
                    centerSlice: centerSlice,
                    repeat: repeat)
            );

            return new Ink(
                key: key,
                padding: padding,
                decoration: decoration,
                width: width,
                height: height,
                child: child);
        }


        public readonly Widget child;

        public readonly EdgeInsets padding;

        public readonly Decoration decoration;

        public readonly float? width;

        public readonly float? height;

        public EdgeInsets _paddingIncludingDecoration {
            get {
                if (decoration == null || decoration.padding == null) {
                    return padding;
                }

                EdgeInsets decorationPadding = decoration.padding;
                if (padding == null) {
                    return decorationPadding;
                }

                return padding.add(decorationPadding);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", padding, defaultValue: null));
            properties.add(new DiagnosticsProperty<Decoration>("bg", decoration, defaultValue: null));
        }

        public override State createState() {
            return new _InkState();
        }
    }


    class _InkState : State<Ink> {
        InkDecoration _ink;

        void _handleRemoved() {
            _ink = null;
        }

        public override void deactivate() {
            _ink?.dispose();
            D.assert(_ink == null);
            base.deactivate();
        }

        public Widget _build(BuildContext context, BoxConstraints constraints) {
            if (_ink == null) {
                _ink = new InkDecoration(
                    decoration: widget.decoration,
                    configuration: ImageUtils.createLocalImageConfiguration(context),
                    controller: Material.of(context),
                    referenceBox: (RenderBox) context.findRenderObject(),
                    onRemoved: _handleRemoved
                );
            }
            else {
                _ink.decoration = widget.decoration;
                _ink.configuration = ImageUtils.createLocalImageConfiguration(context);
            }

            Widget current = widget.child;
            EdgeInsets effectivePadding = widget._paddingIncludingDecoration;
            if (effectivePadding != null) {
                current = new Padding(
                    padding: effectivePadding,
                    child: current);
            }

            return current;
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            Widget result = new LayoutBuilder(
                builder: _build
            );
            if (widget.width != null || widget.height != null) {
                result = new SizedBox(
                    width: widget.width,
                    height: widget.height,
                    child: result);
            }

            return result;
        }
    }


    class InkDecoration : InkFeature {
        public InkDecoration(
            Decoration decoration = null,
            ImageConfiguration configuration = null,
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            VoidCallback onRemoved = null
        ) : base(controller: controller, referenceBox: referenceBox, onRemoved: onRemoved) {
            D.assert(configuration != null);
            D.assert(decoration != null);
            D.assert(controller != null);
            D.assert(referenceBox != null);
            _configuration = configuration;
            this.decoration = decoration;
            this.controller.addInkFeature(this);
        }

        BoxPainter _painter;

        public Decoration decoration {
            get { return _decoration; }
            set {
                if (value == _decoration) {
                    return;
                }

                _decoration = value;
                _painter?.Dispose();
                _painter = _decoration?.createBoxPainter(_handleChanged);
                controller.markNeedsPaint();
            }
        }

        Decoration _decoration;

        public ImageConfiguration configuration {
            get { return _configuration; }
            set {
                D.assert(value != null);
                if (value == _configuration) {
                    return;
                }

                _configuration = value;
                controller.markNeedsPaint();
            }
        }

        ImageConfiguration _configuration;

        void _handleChanged() {
            controller.markNeedsPaint();
        }

        public override void dispose() {
            _painter?.Dispose();
            base.dispose();
        }

        protected override void paintFeature(Canvas canvas, Matrix4 transform) {
            if (_painter == null) {
                return;
            }

            Offset originOffset = transform.getAsTranslation();
            ImageConfiguration sizedConfiguration = configuration.copyWith(
                size: referenceBox.size);

            if (originOffset == null) {
                canvas.save();
                canvas.concat(transform.toMatrix3());
                _painter.paint(canvas, Offset.zero, sizedConfiguration);
                canvas.restore();
            }
            else {
                _painter.paint(canvas, originOffset, sizedConfiguration);
            }
        }
    }
}