using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.widgets {
    public class DecoratedBox : SingleChildRenderObjectWidget {
        public DecoratedBox(
            Key key = null,
            Decoration decoration = null,
            DecorationPosition position = DecorationPosition.background,
            Widget child = null
        ) : base(key, child) {
            D.assert(decoration != null);
            this.position = position;
            this.decoration = decoration;
        }

        public readonly Decoration decoration;

        public readonly DecorationPosition position;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderDecoratedBox(
                decoration: decoration,
                position: position,
                configuration: ImageUtils.createLocalImageConfiguration(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderDecoratedBox) renderObjectRaw;
            renderObject.decoration = decoration;
            renderObject.configuration = ImageUtils.createLocalImageConfiguration(context);
            renderObject.position = position;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            string label = "decoration";
            switch (position) {
                case DecorationPosition.background:
                    label = "bg";
                    break;
                case DecorationPosition.foreground:
                    label = "fg";
                    break;
            }

            properties.add(new EnumProperty<DecorationPosition>(
                "position", position, level: DiagnosticLevel.hidden));
            properties.add(new DiagnosticsProperty<Decoration>(
                label,
                decoration,
                ifNull: "no decoration",
                showName: decoration != null
            ));
        }
    }

    public class Container : StatelessWidget {
        public Container(
            Key key = null,
            Alignment alignment = null,
            EdgeInsets padding = null,
            Color color = null,
            Decoration decoration = null,
            Decoration forgroundDecoration = null,
            float? width = null,
            float? height = null,
            BoxConstraints constraints = null,
            EdgeInsets margin = null,
            Matrix4 transfrom = null,
            Widget child = null
        ) : base(key) {
            D.assert(margin == null || margin.isNonNegative);
            D.assert(padding == null || padding.isNonNegative);
            D.assert(decoration == null || decoration.debugAssertIsValid());
            D.assert(constraints == null || constraints.debugAssertIsValid());
            D.assert(color == null || decoration == null,
                () => "Cannot provide both a color and a decoration\n" +
                "The color argument is just a shorthand for \"decoration: new BoxDecoration(color: color)\"."
            );

            this.alignment = alignment;
            this.padding = padding;
            foregroundDecoration = forgroundDecoration;
            this.margin = margin;
            transform = transfrom;
            this.child = child;

            this.decoration = decoration ?? (color != null ? new BoxDecoration(color) : null);
            this.constraints = (width != null || height != null)
                ? (constraints != null ? constraints.tighten(width, height) : BoxConstraints.tightFor(width, height))
                : constraints;
        }

        public readonly Widget child;
        public readonly Alignment alignment;
        public readonly EdgeInsets padding;
        public readonly Decoration decoration;
        public readonly Decoration foregroundDecoration;
        public readonly BoxConstraints constraints;
        public readonly EdgeInsets margin;
        public readonly Matrix4 transform;

        EdgeInsets _paddingIncludingDecoration {
            get {
                if (decoration == null || decoration.padding == null) {
                    return padding;
                }

                Debug.LogError("EdgeInsets needs to be update to EdgeInsetsGeometry");
                EdgeInsets decorationPadding = (EdgeInsets)decoration.padding;
                if (padding == null) {
                    return decorationPadding;
                }

                return padding.add(decorationPadding);
            }
        }

        public override Widget build(BuildContext context) {
            Widget current = child;

            if (child == null && (constraints == null || !constraints.isTight)) {
                current = new LimitedBox(
                    maxWidth: 0.0f,
                    maxHeight: 0.0f,
                    child: new ConstrainedBox(constraints: BoxConstraints.expand())
                );
            }

            if (alignment != null) {
                current = new Align(alignment: alignment, child: current);
            }

            EdgeInsets effetivePadding = _paddingIncludingDecoration;
            if (effetivePadding != null) {
                current = new Padding(padding: effetivePadding, child: current);
            }

            if (decoration != null) {
                current = new DecoratedBox(decoration: decoration, child: current);
            }

            if (foregroundDecoration != null) {
                current = new DecoratedBox(
                    decoration: foregroundDecoration,
                    position: DecorationPosition.foreground,
                    child: current
                );
            }

            if (constraints != null) {
                current = new ConstrainedBox(constraints: constraints, child: current);
            }

            if (margin != null) {
                current = new Padding(padding: margin, child: current);
            }

            if (transform != null) {
                current = new Transform(transform: new Matrix4(transform), child: current);
            }

            return current;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Alignment>("alignment",
                alignment, showName: false, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding",
                padding, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Decoration>("bg",
                decoration, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Decoration>("fg",
                foregroundDecoration, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<BoxConstraints>("constraints",
                constraints, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<EdgeInsets>("margin",
                margin, defaultValue: foundation_.kNullDefaultValue));
            properties.add(ObjectFlagProperty<Matrix4>.has("transform",
                transform));
        }
    }
}