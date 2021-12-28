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
            AlignmentGeometry alignment = null,
            EdgeInsetsGeometry padding = null,
            Color color = null,
            Decoration decoration = null,
            Decoration foregroundDecoration = null,
            float? width = null,
            float? height = null,
            BoxConstraints constraints = null,
            EdgeInsetsGeometry margin = null,
            Matrix4 transform = null,
            Widget child = null,
            Clip clipBehavior = Clip.none
        ) : base(key: key) {
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
            this.foregroundDecoration = foregroundDecoration;
            this.color = color;
            this.decoration = decoration;// ?? (color != null ? new BoxDecoration(color) : null);
            this.constraints = (width != null || height != null)
                ? (constraints != null ? constraints.tighten(width, height) : BoxConstraints.tightFor(width, height))
                : constraints;
            this.margin = margin;
            this.transform = transform;
            this.child = child;
            this.clipBehavior = clipBehavior ;
            
           
        }

        public readonly Widget child;
        public readonly AlignmentGeometry alignment;
        public readonly EdgeInsetsGeometry padding;
        public readonly Decoration decoration;
        public readonly Decoration foregroundDecoration;
        public readonly BoxConstraints constraints;
        public readonly EdgeInsetsGeometry margin;
        public readonly Matrix4 transform;
        public readonly Color color;
        public readonly Clip clipBehavior;

        EdgeInsetsGeometry _paddingIncludingDecoration {
            get {
                if (decoration == null || decoration.padding == null) {
                    return padding;
                }
                EdgeInsetsGeometry decorationPadding = decoration.padding;
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

            EdgeInsetsGeometry effetivePadding = _paddingIncludingDecoration;
            if (effetivePadding != null) {
                current = new Padding(padding: effetivePadding, child: current);
            }

            if (color != null)
                current = new ColoredBox(color: color, child: current);
            
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

            if (clipBehavior != Clip.none) {
                current = new ClipPath(
                    clipper: new _DecorationClipper(
                        textDirection: Directionality.of(context),
                        decoration: decoration
                    ),
                    clipBehavior: clipBehavior,
                    child: current
                );
            }

            return current;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment",
                alignment, showName: false, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("padding",
                padding, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Clip>("clipBehavior", clipBehavior, defaultValue: Clip.none));
            if (color != null)
                properties.add(new DiagnosticsProperty<Color>("bg", color));
            else
                properties.add(new DiagnosticsProperty<Decoration>("bg",
                decoration, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Decoration>("fg",
                foregroundDecoration, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<BoxConstraints>("constraints",
                constraints, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("margin",
                margin, defaultValue: foundation_.kNullDefaultValue));
            properties.add(ObjectFlagProperty<Matrix4>.has("transform",
                transform));
        }
    }
    
    /// A clipper that uses [Decoration.getClipPath] to clip.
    public class _DecorationClipper : CustomClipper<Path> {
        public _DecorationClipper(
            TextDirection? textDirection = null,
            Decoration decoration = null
        ) {
            D.assert(decoration != null);
            this.textDirection =  textDirection ?? TextDirection.ltr;
            this.decoration = decoration;
        } 

        public readonly TextDirection textDirection;
        public readonly Decoration decoration;
        
        public override Path getClip(Size size) {
            return decoration.getClipPath(Offset.zero & size, textDirection);
        }

        public override bool shouldReclip(CustomClipper<Path> oldClipper) {
            return ((_DecorationClipper)oldClipper).decoration != decoration
                   || ((_DecorationClipper)oldClipper).textDirection != textDirection;
        }
    }
}