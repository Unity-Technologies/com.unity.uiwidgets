using System;
using System.Collections.Generic;
using UIWidgets.Runtime.rendering;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using StrutStyle = Unity.UIWidgets.painting.StrutStyle;

namespace Unity.UIWidgets.widgets {
    public class Directionality : InheritedWidget {
        public Directionality(
            Widget child,
            TextDirection textDirection,
            Key key = null
        ) : base(key, child) {
            this.textDirection = textDirection;
        }

        public readonly TextDirection textDirection;

        public static TextDirection of(BuildContext context) {
            Directionality widget = context.dependOnInheritedWidgetOfExactType<Directionality>();
            return widget?.textDirection ?? TextDirection.ltr;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return textDirection != ((Directionality) oldWidget).textDirection;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<TextDirection>("textDirection", textDirection));
        }
    }

    public class Opacity : SingleChildRenderObjectWidget {
        public Opacity(
            Key key = null,
            float opacity = 0.0f,
            Widget child = null) : base(key, child) {
            D.assert(opacity >= 0.0 && opacity <= 1.0);
            this.opacity = opacity;
        }

        public readonly float opacity;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderOpacity(opacity: opacity);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderOpacity) renderObject).opacity = opacity;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("opacity", opacity));
        }
    }

    public class ShaderMask : SingleChildRenderObjectWidget {
        public ShaderMask(
            Key key = null,
            ShaderCallback shaderCallback = null,
            BlendMode blendMode = BlendMode.modulate,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(shaderCallback != null);
            this.shaderCallback = shaderCallback;
            this.blendMode = blendMode;
        }

        public readonly ShaderCallback shaderCallback;
        public readonly BlendMode blendMode;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderShaderMask(
                shaderCallback: shaderCallback,
                blendMode: blendMode
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (RenderShaderMask) renderObject;
            ((RenderShaderMask) renderObject).shaderCallback = shaderCallback;
            ((RenderShaderMask) renderObject).blendMode = blendMode;
        }
    }


    public class BackdropFilter : SingleChildRenderObjectWidget {
        public BackdropFilter(
            Key key = null,
            ImageFilter filter = null,
            Widget child = null)
            : base(key, child) {
            D.assert(filter != null);
            this.filter = filter;
        }

        public readonly ImageFilter filter;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderBackdropFilter(filter: filter);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderBackdropFilter) renderObject).filter = filter;
        }
    }


    public class CustomPaint : SingleChildRenderObjectWidget {
        public CustomPaint(
            Key key = null,
            CustomPainter painter = null,
            CustomPainter foregroundPainter = null,
            Size size = null,
            bool isComplex = false,
            bool willChange = false,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(painter != null || foregroundPainter != null || (!isComplex && !willChange));
            size = size ?? Size.zero;
            this.size = size;
            this.painter = painter;
            this.foregroundPainter = foregroundPainter;
            this.isComplex = isComplex;
            this.willChange = willChange;
        }

        public readonly CustomPainter painter;
        public readonly CustomPainter foregroundPainter;
        public readonly Size size;
        public readonly bool isComplex;
        public readonly bool willChange;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderCustomPaint(
                painter: painter,
                foregroundPainter: foregroundPainter,
                preferredSize: size,
                isComplex: isComplex,
                willChange: willChange
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderCustomPaint) renderObject).painter = painter;
            ((RenderCustomPaint) renderObject).foregroundPainter = foregroundPainter;
            ((RenderCustomPaint) renderObject).preferredSize = size;
            ((RenderCustomPaint) renderObject).isComplex = isComplex;
            ((RenderCustomPaint) renderObject).willChange = willChange;
        }

        public override void didUnmountRenderObject(RenderObject renderObject) {
            ((RenderCustomPaint) renderObject).painter = null;
            ((RenderCustomPaint) renderObject).foregroundPainter = null;
        }
    }

    public class ClipRect : SingleChildRenderObjectWidget {
        public ClipRect(
            Key key = null,
            CustomClipper<Rect> clipper = null,
            Clip clipBehavior = Clip.hardEdge,
            Widget child = null
        ) : base(key: key, child: child) {
            this.clipper = clipper;
            this.clipBehavior = clipBehavior;
        }

        public readonly CustomClipper<Rect> clipper;

        public readonly Clip clipBehavior;

        public override RenderObject createRenderObject(BuildContext context) {
            D.assert(clipBehavior != Clip.none);
            return new RenderClipRect(
                clipper: clipper,
                clipBehavior: clipBehavior);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            D.assert(clipBehavior != Clip.none);
            RenderClipRect _renderObject = (RenderClipRect) renderObject;
            _renderObject.clipper = clipper;
            _renderObject.clipBehavior = clipBehavior;
        }

        public override void didUnmountRenderObject(RenderObject renderObject) {
            RenderClipRect _renderObject = (RenderClipRect) renderObject;
            _renderObject.clipper = null;
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<CustomClipper<Rect>>("clipper", clipper, defaultValue: null));
        }
    }

    public class ClipRRect : SingleChildRenderObjectWidget {
        public ClipRRect(
            Key key = null,
            BorderRadius borderRadius = null,
            CustomClipper<RRect> clipper = null,
            Clip clipBehavior = Clip.antiAlias,
            Widget child = null
        ) : base(key: key, child: child) {
            this.borderRadius = borderRadius ?? BorderRadius.zero;
            this.clipper = clipper;
            this.clipBehavior = clipBehavior;
        }

        public readonly BorderRadius borderRadius;

        public readonly CustomClipper<RRect> clipper;

        public readonly Clip clipBehavior;

        public override RenderObject createRenderObject(BuildContext context) {
            D.assert(clipBehavior != Clip.none);
            return new RenderClipRRect(borderRadius: borderRadius, clipper: clipper,
                clipBehavior: clipBehavior);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            D.assert(clipBehavior != Clip.none);
            RenderClipRRect _renderObject = (RenderClipRRect) renderObject;
            _renderObject.borderRadius = borderRadius;
            _renderObject.clipper = clipper;
            _renderObject.clipBehavior = clipBehavior;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<BorderRadius>("borderRadius", borderRadius, showName: false,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<CustomClipper<RRect>>("clipper", clipper, defaultValue: null));
        }
    }

    public class ClipOval : SingleChildRenderObjectWidget {
        public ClipOval(
            Key key = null,
            CustomClipper<Rect> clipper = null,
            Clip clipBehavior = Clip.antiAlias,
            Widget child = null) : base(key: key, child: child
        ) {
            this.clipper = clipper;
            this.clipBehavior = clipBehavior;
        }

        public readonly CustomClipper<Rect> clipper;

        public readonly Clip clipBehavior;

        public override RenderObject createRenderObject(BuildContext context) {
            D.assert(clipBehavior != Clip.none);
            return new RenderClipOval(clipper: clipper, clipBehavior: clipBehavior);
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            D.assert(clipBehavior != Clip.none);
            RenderClipOval renderObject = _renderObject as RenderClipOval;
            renderObject.clipper = clipper;
            renderObject.clipBehavior = clipBehavior;
        }

        public override void didUnmountRenderObject(RenderObject _renderObject) {
            RenderClipOval renderObject = _renderObject as RenderClipOval;
            renderObject.clipper = null;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<CustomClipper<Rect>>("clipper", clipper, defaultValue: null));
        }
    }

    public class ClipPath : SingleChildRenderObjectWidget {
        public ClipPath(
            Key key = null,
            CustomClipper<Path> clipper = null,
            Clip clipBehavior = Clip.antiAlias,
            Widget child = null
        ) : base(key: key, child: child) {
            this.clipper = clipper;
            this.clipBehavior = clipBehavior;
        }

        public static Widget shape(
            Key key = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.antiAlias,
            Widget child = null
        ) {
            D.assert(shape != null);
            D.assert(clipBehavior != Clip.none);
            return new Builder(
                key: key,
                builder: (BuildContext context) => {
                    return new ClipPath(
                        clipper: new ShapeBorderClipper(
                            shape: shape,
                            textDirection: Directionality.of(context)
                        ),
                        clipBehavior: clipBehavior,
                        child: child
                    );
                }
            );
        }

        public readonly CustomClipper<Path> clipper;

        public readonly Clip clipBehavior;


        public override RenderObject createRenderObject(BuildContext context) {
            D.assert(clipBehavior != Clip.none);
            return new RenderClipPath(clipper: clipper, clipBehavior: clipBehavior);
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            D.assert(clipBehavior != Clip.none);
            RenderClipPath _renderObject = (RenderClipPath) renderObject;
            _renderObject.clipper = clipper;
            _renderObject.clipBehavior = clipBehavior;
        }


        public override void didUnmountRenderObject(RenderObject renderObject) {
            RenderClipPath _renderObject = (RenderClipPath) renderObject;
            _renderObject.clipper = null;
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<CustomClipper<Path>>("clipper", clipper, defaultValue: null));
        }
    }

    public class PhysicalModel : SingleChildRenderObjectWidget {
        public PhysicalModel(
            Key key = null,
            BoxShape shape = BoxShape.rectangle,
            Clip clipBehavior = Clip.none,
            BorderRadius borderRadius = null,
            float elevation = 0.0f,
            Color color = null,
            Color shadowColor = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(color != null);
            D.assert(elevation >= 0.0f);
            this.shape = shape;
            this.clipBehavior = clipBehavior;
            this.borderRadius = borderRadius;
            this.elevation = elevation;
            this.color = color;
            this.shadowColor = shadowColor ?? new Color(0xFF000000);
        }

        public readonly BoxShape shape;

        public readonly Clip clipBehavior;

        public readonly BorderRadius borderRadius;

        public readonly float elevation;

        public readonly Color color;

        public readonly Color shadowColor;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPhysicalModel(
                shape: shape,
                clipBehavior: clipBehavior,
                borderRadius: borderRadius,
                elevation: elevation,
                color: color,
                shadowColor: shadowColor);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderPhysicalModel _renderObject = (RenderPhysicalModel) renderObject;
            _renderObject.shape = shape;
            _renderObject.clipBehavior = clipBehavior;
            _renderObject.borderRadius = borderRadius;
            _renderObject.elevation = elevation;
            _renderObject.color = color;
            _renderObject.shadowColor = shadowColor;
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<BoxShape>("shape", shape));
            properties.add(new DiagnosticsProperty<BorderRadius>("borderRadius", borderRadius));
            properties.add(new FloatProperty("elevation", elevation));
            properties.add(new ColorProperty("color", color));
            properties.add(new ColorProperty("shadowColor", shadowColor));
        }
    }

    public class PhysicalShape : SingleChildRenderObjectWidget {
        public PhysicalShape(
            Key key = null,
            CustomClipper<Path> clipper = null,
            Clip clipBehavior = Clip.none,
            float elevation = 0.0f,
            Color color = null,
            Color shadowColor = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(clipper != null);
            D.assert(color != null);
            D.assert(elevation >= 0.0f);
            this.clipper = clipper;
            this.clipBehavior = clipBehavior;
            this.elevation = elevation;
            this.color = color;
            this.shadowColor = shadowColor ?? new Color(0xFF000000);
        }

        public readonly CustomClipper<Path> clipper;

        public readonly Clip clipBehavior;

        public readonly float elevation;

        public readonly Color color;

        public readonly Color shadowColor;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPhysicalShape(
                clipper: clipper,
                clipBehavior: clipBehavior,
                elevation: elevation,
                color: color,
                shadowColor: shadowColor);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderPhysicalShape _renderObject = (RenderPhysicalShape) renderObject;
            _renderObject.clipper = clipper;
            _renderObject.elevation = elevation;
            _renderObject.color = color;
            _renderObject.shadowColor = shadowColor;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<CustomClipper<Path>>("clipper", clipper));
            properties.add(new FloatProperty("elevation", elevation));
            properties.add(new ColorProperty("color", color));
            properties.add(new ColorProperty("shadowColor", shadowColor));
        }
    }

    public class Transform : SingleChildRenderObjectWidget {
        public Transform(
            Key key = null,
            Matrix4 transform = null,
            Offset origin = null,
            AlignmentGeometry alignment = null,
            bool transformHitTests = true,
            Widget child = null
        ) : base(key, child) {
            D.assert(transform != null);
            this.transform = transform;
            this.origin = origin;
            this.alignment = alignment;
            this.transformHitTests = transformHitTests;
        }

        public static Transform rotate(
            Key key = null,
            float angle = 0.0f,
            Offset origin = null,
            AlignmentGeometry alignment = null,
            bool transformHitTests = true,
            Widget child = null
        ) {
            return new Transform(key,
                Matrix4.rotationZ(angle),
                origin,
                alignment,
                transformHitTests,
                child);
        }


        public static Transform translate(
            Key key = null,
            Offset offset = null,
            bool transformHitTests = true,
            Widget child = null
        ) {
            return new Transform(
                key,
                Matrix4.translationValues(offset.dx, offset.dy, 0.0f),
                null,
                null,
                transformHitTests,
                child);
        }

        public static Transform scale(
            Key key = null,
            float scale = 1.0f,
            Offset origin = null,
            AlignmentGeometry alignment = null,
            bool transformHitTests = true,
            Widget child = null
        ) {
            return new Transform(key,
                Matrix4.diagonal3Values(scale, scale, 1.0f),
                origin,
                alignment ?? Alignment.center,
                transformHitTests,
                child);
        }

        public readonly Matrix4 transform;
        public readonly Offset origin;
        public readonly AlignmentGeometry alignment;
        public readonly bool transformHitTests;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderTransform(
                transform: transform,
                origin: origin,
                alignment: alignment,
                textDirection: Directionality.of(context),
                transformHitTests: transformHitTests
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderTransform) renderObjectRaw;
            renderObject.transform = transform;
            renderObject.origin = origin;
            renderObject.alignment = alignment;
            renderObject.textDirection = Directionality.of(context);
            renderObject.transformHitTests = transformHitTests;
        }
    }

    public class CompositedTransformTarget : SingleChildRenderObjectWidget {
        public CompositedTransformTarget(
            Key key = null,
            LayerLink link = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(link != null);
            this.link = link;
        }

        public readonly LayerLink link;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderLeaderLayer(
                link: link
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderLeaderLayer) renderObject).link = link;
        }
    }

    public class CompositedTransformFollower : SingleChildRenderObjectWidget {
        public CompositedTransformFollower(
            Key key = null,
            LayerLink link = null,
            bool showWhenUnlinked = true,
            Offset offset = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(link != null);
            this.showWhenUnlinked = showWhenUnlinked;
            this.offset = offset ?? Offset.zero;
            this.link = link;
        }

        public readonly LayerLink link;
        public readonly bool showWhenUnlinked;
        public readonly Offset offset;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderFollowerLayer(
                link: link,
                showWhenUnlinked: showWhenUnlinked,
                offset: offset
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderFollowerLayer) renderObject).link = link;
            ((RenderFollowerLayer) renderObject).showWhenUnlinked = showWhenUnlinked;
            ((RenderFollowerLayer) renderObject).offset = offset;
        }
    }

    public class FittedBox : SingleChildRenderObjectWidget {
        public FittedBox(
            Key key = null,
            BoxFit fit = BoxFit.contain,
            AlignmentGeometry alignment = null,
            Widget child = null
        ) : base(key: key, child: child) {
            this.fit = fit;
            this.alignment = alignment ?? Alignment.center;
        }

        public readonly BoxFit fit;

        public readonly AlignmentGeometry alignment;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderFittedBox(
                fit: fit,
                alignment: alignment,
                textDirection: Directionality.of(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            RenderFittedBox renderObject = _renderObject as RenderFittedBox;
            renderObject.fit = fit;
            renderObject.alignment = alignment;
            renderObject.textDirection = Directionality.of(context);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<BoxFit>("fit", fit));
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment));
        }
    }

    public class FractionalTranslation : SingleChildRenderObjectWidget {
        public FractionalTranslation(
            Key key = null,
            Offset translation = null,
            bool transformHitTests = true,
            Widget child = null) : base(key: key, child: child) {
            D.assert(translation != null);
            this.translation = translation;
            this.transformHitTests = transformHitTests;
        }

        public readonly Offset translation;
        public readonly bool transformHitTests;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderFractionalTranslation(
                translation: translation,
                transformHitTests: transformHitTests
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderFractionalTranslation) renderObject).translation = translation;
            ((RenderFractionalTranslation) renderObject).transformHitTests = transformHitTests;
        }
    }

    public class RotatedBox : SingleChildRenderObjectWidget {
        public RotatedBox(
            Key key = null,
            int? quarterTurns = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(quarterTurns != null);
            this.quarterTurns = quarterTurns;
        }


        public readonly int? quarterTurns;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderRotatedBox(quarterTurns ?? 0);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            (renderObject as RenderRotatedBox).quarterTurns = quarterTurns ?? 0;
        }
    }

    public class Padding : SingleChildRenderObjectWidget {
        public Padding(
            Key key = null,
            EdgeInsetsGeometry padding = null,
            Widget child = null
        ) : base(key, child) {
            D.assert(padding != null);
            this.padding = padding;
        }

        public readonly EdgeInsetsGeometry padding;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPadding(
                padding: padding,
                textDirection: Directionality.of(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderPadding) renderObjectRaw;
            renderObject.padding = padding;
            renderObject.textDirection = Directionality.of(context);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding));
        }
    }

    public class Align : SingleChildRenderObjectWidget {
        public Align(
            Key key = null,
            AlignmentGeometry alignment = null,
            float? widthFactor = null,
            float? heightFactor = null,
            Widget child = null
        ) : base(key, child) {
            D.assert(widthFactor == null || widthFactor >= 0.0);
            D.assert(heightFactor == null || heightFactor >= 0.0);

            this.alignment = alignment ?? Alignment.center;
            this.widthFactor = widthFactor;
            this.heightFactor = heightFactor;
        }

        public readonly AlignmentGeometry alignment;

        public readonly float? widthFactor;

        public readonly float? heightFactor;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPositionedBox(
                alignment: alignment,
                widthFactor: widthFactor,
                heightFactor: heightFactor,
                textDirection: Directionality.of(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderPositionedBox) renderObjectRaw;
            renderObject.alignment = alignment;
            renderObject.widthFactor = widthFactor;
            renderObject.heightFactor = heightFactor;
            renderObject.textDirection = Directionality.of(context);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment));
            properties.add(new FloatProperty("widthFactor",
                widthFactor, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new FloatProperty("heightFactor",
                heightFactor, defaultValue: foundation_.kNullDefaultValue));
        }
    }

    public class Center : Align {
        public Center(
            Key key = null,
            float? widthFactor = null,
            float? heightFactor = null,
            Widget child = null)
            : base(
                key: key,
                widthFactor: widthFactor,
                heightFactor: heightFactor,
                child: child) {
        }
    }

    public class CustomSingleChildLayout : SingleChildRenderObjectWidget {
        public CustomSingleChildLayout(
            Key key = null,
            SingleChildLayoutDelegate layoutDelegate = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(layoutDelegate != null);
            this.layoutDelegate = layoutDelegate;
        }

        public readonly SingleChildLayoutDelegate layoutDelegate;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderCustomSingleChildLayoutBox(layoutDelegate: layoutDelegate);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderCustomSingleChildLayoutBox) renderObject).layoutDelegate = layoutDelegate;
        }
    }

    public class LayoutId : ParentDataWidget<MultiChildLayoutParentData> {
        public LayoutId(
            Key key = null,
            object id = null,
            Widget child = null
        ) : base(key: key ?? new ValueKey<object>(id), child: child) {
            D.assert(child != null);
            D.assert(id != null);
            this.id = id;
        }

        public readonly object id;

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is MultiChildLayoutParentData);
            MultiChildLayoutParentData parentData = (MultiChildLayoutParentData) renderObject.parentData;
            if (parentData.id != id) {
                parentData.id = id;
                var targetParent = renderObject.parent;
                if (targetParent is RenderObject) {
                    ((RenderObject) targetParent).markNeedsLayout();
                }
            }
        }

        public override Type debugTypicalAncestorWidgetClass {
            get { return typeof(CustomMultiChildLayout); }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>("id", id));
        }
    }

    public class CustomMultiChildLayout : MultiChildRenderObjectWidget {
        public CustomMultiChildLayout(
            Key key = null,
            MultiChildLayoutDelegate layoutDelegate = null,
            List<Widget> children = null
        ) : base(key: key, children: children ?? new List<Widget>()) {
            D.assert(layoutDelegate != null);
            this.layoutDelegate = layoutDelegate;
        }

        public readonly MultiChildLayoutDelegate layoutDelegate;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderCustomMultiChildLayoutBox(layoutDelegate: layoutDelegate);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderCustomMultiChildLayoutBox) renderObject).layoutDelegate = layoutDelegate;
        }
    }

    public class SizedBox : SingleChildRenderObjectWidget {
        public SizedBox(
            Key key = null,
            float? width = null,
            float? height = null,
            Widget child = null)
            : base(key: key, child: child) {
            this.width = width;
            this.height = height;
        }

        public static SizedBox expand(
            Key key = null,
            Widget child = null) {
            return new SizedBox(key, float.PositiveInfinity, float.PositiveInfinity, child);
        }

        public static SizedBox shrink(
            Key key = null,
            Widget child = null) {
            return new SizedBox(key, 0, 0, child);
        }

        public static SizedBox fromSize(Key key = null, Widget child = null, Size size = null) {
            return new SizedBox(key,
                size == null ? (float?) null : size.width,
                size == null ? (float?) null : size.height, child);
        }

        public readonly float? width;

        public readonly float? height;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderConstrainedBox(
                additionalConstraints: _additionalConstraints
            );
        }

        BoxConstraints _additionalConstraints {
            get { return BoxConstraints.tightFor(width: width, height: height); }
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderConstrainedBox) renderObjectRaw;
            renderObject.additionalConstraints = _additionalConstraints;
        }

        public override string toStringShort() {
            string type;
            if (width == float.PositiveInfinity && height == float.PositiveInfinity) {
                type = GetType() + "expand";
            }
            else if (width == 0.0 && height == 0.0) {
                type = GetType() + "shrink";
            }
            else {
                type = GetType() + "";
            }

            return key == null ? type : type + "-" + key;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            DiagnosticLevel level;
            if ((width == float.PositiveInfinity && height == float.PositiveInfinity) ||
                (width == 0.0 && height == 0.0)) {
                level = DiagnosticLevel.hidden;
            }
            else {
                level = DiagnosticLevel.info;
            }

            properties.add(new FloatProperty("width", width,
                defaultValue: foundation_.kNullDefaultValue,
                level: level));
            properties.add(new FloatProperty("height", height,
                defaultValue: foundation_.kNullDefaultValue,
                level: level));
        }
    }

    public class ConstrainedBox : SingleChildRenderObjectWidget {
        public ConstrainedBox(
            Key key = null,
            BoxConstraints constraints = null,
            Widget child = null
        ) : base(key, child) {
            D.assert(constraints != null);
            D.assert(constraints.debugAssertIsValid());

            this.constraints = constraints;
        }

        public readonly BoxConstraints constraints;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderConstrainedBox(additionalConstraints: constraints);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderConstrainedBox) renderObjectRaw;
            renderObject.additionalConstraints = constraints;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<BoxConstraints>("constraints",
                constraints, showName: false));
        }
    }

    public class UnconstrainedBox : SingleChildRenderObjectWidget {
        public UnconstrainedBox(
            Key key = null,
            Widget child = null,
            TextDirection? textDirection = null,
            AlignmentGeometry alignment = null,
            Axis? constrainedAxis = null
        ) : base(key: key, child: child) {
            this.alignment = alignment ?? Alignment.center;
            this.textDirection = textDirection;
            this.constrainedAxis = constrainedAxis;
        }

        public readonly AlignmentGeometry alignment;
        public readonly TextDirection? textDirection;
        public readonly Axis? constrainedAxis;

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            RenderUnconstrainedBox renderObject = _renderObject as RenderUnconstrainedBox;
            renderObject.alignment = alignment;
            renderObject.constrainedAxis = constrainedAxis;
            renderObject.textDirection = textDirection ?? Directionality.of(context);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderUnconstrainedBox(
                textDirection: textDirection ?? Directionality.of(context),
                alignment: alignment,
                constrainedAxis: constrainedAxis
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment));
            properties.add(new EnumProperty<Axis?>("constrainedAxis", null));
            properties.add(new EnumProperty<TextDirection>("textDirection", (TextDirection) textDirection,
                defaultValue: null));
        }
    }


    public class FractionallySizedBox : SingleChildRenderObjectWidget {
        public FractionallySizedBox(
            Key key = null,
            AlignmentGeometry alignment = null,
            float? widthFactor = null,
            float? heightFactor = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(widthFactor == null || widthFactor >= 0.0f);
            D.assert(heightFactor == null || heightFactor >= 0.0f);
            this.alignment = alignment ?? Alignment.center;
            this.widthFactor = widthFactor;
            this.heightFactor = heightFactor;
        }

        public readonly float? widthFactor;

        public readonly float? heightFactor;

        public readonly AlignmentGeometry alignment;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderFractionallySizedOverflowBox(
                alignment: alignment,
                widthFactor: widthFactor,
                heightFactor: heightFactor,
                textDirection: Directionality.of(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            RenderFractionallySizedOverflowBox renderObject = _renderObject as RenderFractionallySizedOverflowBox;
            renderObject.alignment = alignment;
            renderObject.widthFactor = widthFactor;
            renderObject.heightFactor = heightFactor;
            renderObject.textDirection = Directionality.of(context);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment));
            properties.add(new FloatProperty("widthFactor", widthFactor, defaultValue: null));
            properties.add(new FloatProperty("heightFactor", heightFactor, defaultValue: null));
        }
    }

    public class LimitedBox : SingleChildRenderObjectWidget {
        public LimitedBox(
            Key key = null,
            float maxWidth = float.PositiveInfinity,
            float maxHeight = float.PositiveInfinity,
            Widget child = null
        ) : base(key, child) {
            D.assert(maxWidth >= 0.0);
            D.assert(maxHeight >= 0.0);

            this.maxHeight = maxHeight;
            this.maxWidth = maxWidth;
        }

        public readonly float maxWidth;
        public readonly float maxHeight;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderLimitedBox(
                maxWidth: maxWidth,
                maxHeight: maxHeight
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderLimitedBox) renderObjectRaw;
            renderObject.maxWidth = maxWidth;
            renderObject.maxHeight = maxHeight;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("maxWidth", maxWidth, defaultValue: float.PositiveInfinity));
            properties.add(new FloatProperty("maxHeight", maxHeight, defaultValue: float.PositiveInfinity));
        }
    }

    public class OverflowBox : SingleChildRenderObjectWidget {
        public OverflowBox(
            Key key = null,
            AlignmentGeometry alignment = null,
            float? minWidth = null,
            float? maxWidth = null,
            float? minHeight = null,
            float? maxHeight = null,
            Widget child = null
        ) : base(key: key, child: child) {
            this.alignment = alignment ?? Alignment.center;
            this.minWidth = minWidth;
            this.maxWidth = maxWidth;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
        }

        public readonly AlignmentGeometry alignment;

        public readonly float? minWidth;

        public readonly float? maxWidth;

        public readonly float? minHeight;

        public readonly float? maxHeight;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderConstrainedOverflowBox(
                alignment: alignment,
                minWidth: minWidth,
                maxWidth: maxWidth,
                minHeight: minHeight,
                maxHeight: maxHeight,
                textDirection: Directionality.of(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            RenderConstrainedOverflowBox renderObject = _renderObject as RenderConstrainedOverflowBox;
            renderObject.alignment = alignment;
            renderObject.minWidth = minWidth;
            renderObject.maxWidth = maxWidth;
            renderObject.minHeight = minHeight;
            renderObject.maxHeight = maxHeight;
            renderObject.textDirection = Directionality.of(context);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment));
            properties.add(new FloatProperty("minWidth", minWidth, defaultValue: null));
            properties.add(new FloatProperty("maxWidth", maxWidth, defaultValue: null));
            properties.add(new FloatProperty("minHeight", minHeight, defaultValue: null));
            properties.add(new FloatProperty("maxHeight", maxHeight, defaultValue: null));
        }
    }

    public class SizedOverflowBox : SingleChildRenderObjectWidget {
        public SizedOverflowBox(
            Key key = null,
            Size size = null,
            AlignmentGeometry alignment = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(size != null);
            D.assert(alignment != null);
            this.size = size;
            this.alignment = alignment ?? Alignment.center;
        }

        public readonly AlignmentGeometry alignment;
        public readonly Size size;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSizedOverflowBox(
                alignment: alignment,
                requestedSize: size,
                textDirection: Directionality.of(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (RenderSizedOverflowBox) renderObject;
            ((RenderSizedOverflowBox) renderObject).alignment = alignment;
            ((RenderSizedOverflowBox) renderObject).requestedSize = size;
            ((RenderSizedOverflowBox) renderObject).textDirection = Directionality.of(context);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment));
            properties.add(new DiagnosticsProperty<Size>("size", size, defaultValue: null));
        }
    }

    public class Offstage : SingleChildRenderObjectWidget {
        public Offstage(
            Key key = null,
            bool offstage = true,
            Widget child = null) : base(key: key, child: child) {
            this.offstage = offstage;
        }

        public readonly bool offstage;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderOffstage(offstage: offstage);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderOffstage) renderObject).offstage = offstage;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("offstage", offstage));
        }

        public override Element createElement() {
            return new _OffstageElement(this);
        }
    }

    class _OffstageElement : SingleChildRenderObjectElement {
        internal _OffstageElement(Offstage widget) : base(widget) {
        }

        new Offstage widget {
            get { return (Offstage) base.widget; }
        }

        public override void debugVisitOnstageChildren(ElementVisitor visitor) {
            if (!widget.offstage) {
                base.debugVisitOnstageChildren(visitor);
            }
        }
    }

    public class IntrinsicWidth : SingleChildRenderObjectWidget {
        public IntrinsicWidth(
            Key key = null,
            float? stepWidth = null,
            float? stepHeight = null,
            Widget child = null)
            : base(key: key, child: child) {
            D.assert(stepWidth == null || stepWidth >= 0.0f);
            D.assert(stepHeight == null || stepHeight >= 0.0f);
            this.stepWidth = stepWidth;
            this.stepHeight = stepHeight;
        }

        public readonly float? stepWidth;

        public readonly float? stepHeight;

        float? _stepWidth {
            get { return stepWidth == 0.0f ? null : stepWidth; }
        }

        float? _stepHeight {
            get { return stepHeight == 0.0f ? null : stepHeight; }
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderIntrinsicWidth(stepWidth: _stepWidth, stepHeight: _stepHeight);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderIntrinsicWidth) renderObjectRaw;
            renderObject.stepWidth = _stepWidth;
            renderObject.stepHeight = _stepHeight;
        }
    }

    public class IntrinsicHeight : SingleChildRenderObjectWidget {
        public IntrinsicHeight(Key key = null, Widget child = null)
            : base(key: key, child: child) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderIntrinsicHeight();
        }
    }

    public class Baseline : SingleChildRenderObjectWidget {
        public Baseline(
            Key key = null,
            float? baseline = null,
            TextBaseline? baselineType = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(baseline != null);
            D.assert(baselineType != null);
            this.baseline = baseline;
            this.baselineType = baselineType;
        }

        public readonly float? baseline;

        public readonly TextBaseline? baselineType;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderBaseline(baseline: baseline ?? 0.0f,
                baselineType: baselineType ?? TextBaseline.alphabetic);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            RenderBaseline renderObject = (RenderBaseline) renderObjectRaw;
            renderObject.baseline = baseline ?? 0.0f;
            renderObject.baselineType = baselineType ?? TextBaseline.alphabetic;
        }
    }

    public class SliverToBoxAdapter : SingleChildRenderObjectWidget {
        public SliverToBoxAdapter(
            Key key = null,
            Widget child = null) : base(key: key, child: child) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverToBoxAdapter();
        }
    }

    public class SliverPadding : SingleChildRenderObjectWidget {
        public SliverPadding(
            Key key = null,
            EdgeInsetsGeometry padding = null,
            Widget sliver = null
        ) : base(key: key, child: sliver) {
            D.assert(padding != null);
            this.padding = padding;
        }

        public readonly EdgeInsetsGeometry padding;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverPadding(
                padding: padding,
                textDirection: Directionality.of(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderSliverPadding) renderObjectRaw;
            renderObject.padding = padding;
            renderObject.textDirection = Directionality.of(context);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding));
        }
    }

    public class AspectRatio : SingleChildRenderObjectWidget {
        public AspectRatio(
            Key key = null,
            float? aspectRatio = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(aspectRatio != null);
            this.aspectRatio = aspectRatio;
        }

        public readonly float? aspectRatio;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderAspectRatio(aspectRatio: aspectRatio ?? 0.0f);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderAspectRatio) renderObject).aspectRatio = aspectRatio ?? 0.0f;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("aspectRatio", aspectRatio));
        }
    }

    public class MouseRegion : StatefulWidget {
        public MouseRegion(
            Key key = null,
            PointerEnterEventListener onEnter = null,
            PointerExitEventListener onExit = null,
            PointerHoverEventListener onHover = null,
            bool opaque = true,
            Widget child = null
        ) : base(key: key) {
            this.onEnter = onEnter;
            this.onExit = onExit;
            this.onHover = onHover;
            this.opaque = opaque;
            this.child = child;
        }

        public readonly PointerEnterEventListener onEnter;
        public readonly PointerHoverEventListener onHover;
        public readonly PointerExitEventListener onExit;
        public readonly bool opaque;
        public readonly Widget child;

        public override State createState() {
            return new _MouseRegionState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            List<string> listeners = new List<string>();
            if (onEnter != null) {
                listeners.Add("enter");
            }

            if (onExit != null) {
                listeners.Add("exit");
            }

            if (onHover != null) {
                listeners.Add("hover");
            }

            properties.add(new EnumerableProperty<string>("listeners", listeners, ifEmpty: "<none>"));
            properties.add(new DiagnosticsProperty<bool>("opaque", opaque, defaultValue: true));
        }
    }

    public class _MouseRegionState : State<MouseRegion> {
        void handleExit(PointerExitEvent _event) {
            if (widget.onExit != null && mounted) {
                widget.onExit(_event);
            }
        }

        public PointerExitEventListener getHandleExit() {
            return widget.onExit == null ? (PointerExitEventListener) null : handleExit;
        }

        public override Widget build(BuildContext context) {
            return new _RawMouseRegion(this);
        }
    }

    public class _RawMouseRegion : SingleChildRenderObjectWidget {
        public _RawMouseRegion(_MouseRegionState owner) : base(child: owner.widget.child) {
            this.owner = owner;
        }

        public readonly _MouseRegionState owner;

        public override RenderObject createRenderObject(BuildContext context) {
            MouseRegion widget = owner.widget;
            return new RenderMouseRegion(
                onEnter: widget.onEnter,
                onHover: widget.onHover,
                onExit: owner.getHandleExit(),
                opaque: widget.opaque
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderMouseRegion _renderObject = (RenderMouseRegion) renderObject;
            MouseRegion widget = owner.widget; 
            _renderObject.onEnter = widget.onEnter;
            _renderObject.onHover = widget.onHover;
            _renderObject.onExit = owner.getHandleExit();
            _renderObject.opaque = widget.opaque;
        }
    }

    public class ListBody : MultiChildRenderObjectWidget {
        public ListBody(
            Key key = null,
            Axis mainAxis = Axis.vertical,
            bool reverse = false,
            List<Widget> children = null
        ) : base(key: key, children: children ?? new List<Widget>()) {
            this.mainAxis = mainAxis;
            this.reverse = reverse;
        }

        public readonly Axis mainAxis;

        public readonly bool reverse;


        AxisDirection _getDirection(BuildContext context) {
            return AxisDirectionUtils.getAxisDirectionFromAxisReverseAndDirectionality(context, mainAxis,
                       reverse) ?? AxisDirection.right;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderListBody(
                axisDirection: _getDirection(context));
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderListBody _renderObject = (RenderListBody) renderObject;
            _renderObject.axisDirection = _getDirection(context);
        }
    }

    public class Stack : MultiChildRenderObjectWidget {
        public Stack(
            Key key = null,
            AlignmentGeometry alignment = null,
            TextDirection? textDirection = null,
            StackFit fit = StackFit.loose,
            Overflow overflow = Overflow.clip,
            List<Widget> children = null
        ) : base(key: key, children: children) {
            this.alignment = alignment ?? AlignmentDirectional.topStart;
            this.fit = fit;
            this.overflow = overflow;
            this.textDirection = textDirection;
        }

        public readonly AlignmentGeometry alignment;
        public readonly StackFit fit;
        public readonly Overflow overflow;
        public readonly TextDirection? textDirection;


        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderStack(
                alignment: alignment,
                textDirection: textDirection ?? Directionality.of(context),
                fit: fit,
                overflow: overflow
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderStack) renderObjectRaw;
            renderObject.alignment = alignment;
            renderObject.textDirection = Directionality.of(context);
            renderObject.fit = fit;
            renderObject.overflow = overflow;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment));
            properties.add(new EnumProperty<TextDirection>("textDirection", (TextDirection) textDirection,
                defaultValue: null));
            properties.add(new EnumProperty<StackFit>("fit", fit));
            properties.add(new EnumProperty<Overflow>("overflow", overflow));
        }
    }

    public class IndexedStack : Stack {
        public IndexedStack(
            Key key = null,
            AlignmentGeometry alignment = null,
            TextDirection? textDirection = null,
            StackFit sizing = StackFit.loose,
            int index = 0,
            List<Widget> children = null
        ) : base(key: key, alignment: alignment ?? AlignmentDirectional.topStart, textDirection: textDirection,
            fit: sizing, children: children) {
            this.index = index;
        }

        public readonly int index;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderIndexedStack(
                alignment: alignment,
                textDirection: textDirection ?? Directionality.of(context),
                index: index
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderIndexedStack renderIndexedStack = renderObject as RenderIndexedStack;
            renderIndexedStack.index = index;
            renderIndexedStack.alignment = alignment;
            renderIndexedStack.textDirection = textDirection ?? Directionality.of(context);
        }
    }

    public class Positioned : ParentDataWidget<StackParentData> {
        public Positioned(
            Key key = null,
            float? left = null,
            float? top = null,
            float? right = null,
            float? bottom = null,
            float? width = null,
            float? height = null,
            Widget child = null) :
            base(key, child) {
            D.assert(left == null || right == null || width == null);
            D.assert(top == null || bottom == null || height == null);
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.width = width;
            this.height = height;
        }

        public static Positioned fromRect(
            Rect rect = null,
            Widget child = null,
            Key key = null) {
            return new Positioned(key: key,
                left: rect.left,
                top: rect.top,
                width: rect.width,
                height: rect.height,
                right: null,
                bottom: null,
                child: child);
        }

        public static Positioned fromRelativeRect(
            Key key = null,
            RelativeRect rect = null,
            Widget child = null
        ) {
            return new Positioned(key: key, left: rect.left,
                top: rect.top,
                right: rect.right,
                bottom: rect.bottom,
                height: null,
                width: null,
                child: child);
        }

        public static Positioned fill(
            Widget child = null,
            Key key = null) {
            return new Positioned(
                key: key,
                left: 0.0f,
                top: 0.0f,
                right: 0.0f, bottom: 0.0f,
                child: child);
        }

        public static Positioned directional(
            Key key = null,
            TextDirection? textDirection = null,
            float? start = null,
            float? top = null,
            float? end = null,
            float? bottom = null,
            float? width = null,
            float? height = null,
            Widget child = null
        ) {
            float? left = null;
            float? right = null;
            switch (textDirection) {
                case TextDirection.rtl:
                    left = end;
                    right = start;
                    break;
                case TextDirection.ltr:
                    left = start;
                    right = end;
                    break;
            }

            return new Positioned(
                key: key,
                left: left,
                top: top,
                right: right,
                bottom: bottom,
                width: width,
                height: height,
                child: child);
        }

        public readonly float? left;

        public readonly float? top;

        public readonly float? right;

        public readonly float? bottom;

        public readonly float? width;

        public readonly float? height;

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is StackParentData);
            StackParentData parentData = (StackParentData) renderObject.parentData;
            bool needsLayout = false;

            if (parentData.left != left) {
                parentData.left = left;
                needsLayout = true;
            }

            if (parentData.top != top) {
                parentData.top = top;
                needsLayout = true;
            }

            if (parentData.right != right) {
                parentData.right = right;
                needsLayout = true;
            }

            if (parentData.bottom != bottom) {
                parentData.bottom = bottom;
                needsLayout = true;
            }

            if (parentData.width != width) {
                parentData.width = width;
                needsLayout = true;
            }

            if (parentData.height != height) {
                parentData.height = height;
                needsLayout = true;
            }

            if (needsLayout) {
                var targetParent = renderObject.parent;
                if (targetParent is RenderObject) {
                    ((RenderObject) targetParent).markNeedsLayout();
                }
            }
        }

        public override Type debugTypicalAncestorWidgetClass {
            get { return typeof(Stack); }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("left", left, defaultValue: null));
            properties.add(new FloatProperty("top", top, defaultValue: null));
            properties.add(new FloatProperty("right", right, defaultValue: null));
            properties.add(new FloatProperty("bottom", bottom, defaultValue: null));
            properties.add(new FloatProperty("width", width, defaultValue: null));
            properties.add(new FloatProperty("height", height, defaultValue: null));
        }
    }

    public class PositionedDirectional : StatelessWidget {
        public PositionedDirectional(
            Key key = null,
            float? start = null,
            float? top = null,
            float? end = null,
            float? bottom = null,
            float? width = null,
            float? height = null,
            Widget child = null
        ) : base(key: key) {
            this.child = child;
            this.start = start;
            this.top = top;
            this.end = end;
            this.bottom = bottom;
            this.width = width;
        }

        public readonly float? start;
        public readonly float? top;
        public readonly float? end;
        public readonly float? bottom;
        public readonly float? width;
        public readonly float? height;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return Positioned.directional(
                textDirection: Directionality.of(context),
                start: start,
                top: top,
                end: end,
                bottom: bottom,
                width: width,
                height: height,
                child: child
            );
        }
    }


    public class Flex : MultiChildRenderObjectWidget {
        public Flex(
            Key key = null,
            Axis direction = Axis.vertical,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.start,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.center,
            TextDirection? textDirection = null,
            VerticalDirection verticalDirection = VerticalDirection.down,
            TextBaseline? textBaseline = null,
            List<Widget> children = null
        ) : base(key, children) {
            this.direction = direction;
            this.mainAxisAlignment = mainAxisAlignment;
            this.mainAxisSize = mainAxisSize;
            this.crossAxisAlignment = crossAxisAlignment;
            this.textDirection = textDirection;
            this.verticalDirection = verticalDirection;
            this.textBaseline = textBaseline;
        }

        public readonly Axis direction;
        public readonly MainAxisAlignment mainAxisAlignment;
        public readonly MainAxisSize mainAxisSize;
        public readonly CrossAxisAlignment crossAxisAlignment;
        public readonly TextDirection? textDirection;
        public readonly VerticalDirection verticalDirection;
        public readonly TextBaseline? textBaseline;

        bool _needTextDirection {
            get {
                switch (direction) {
                    case Axis.horizontal:
                        return true;
                    case Axis.vertical:
                        return (crossAxisAlignment == CrossAxisAlignment.start ||
                                crossAxisAlignment == CrossAxisAlignment.end);
                }

                return false;
            }
        }

        public TextDirection getEffectiveTextDirection(BuildContext context) {
            return textDirection ?? (_needTextDirection ? Directionality.of(context) : TextDirection.ltr);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderFlex(
                direction: direction,
                mainAxisAlignment: mainAxisAlignment,
                mainAxisSize: mainAxisSize,
                crossAxisAlignment: crossAxisAlignment,
                textDirection: getEffectiveTextDirection(context),
                verticalDirection: verticalDirection,
                textBaseline: textBaseline ?? TextBaseline.alphabetic
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderFlex) renderObject).direction = direction;
            ((RenderFlex) renderObject).mainAxisAlignment = mainAxisAlignment;
            ((RenderFlex) renderObject).mainAxisSize = mainAxisSize;
            ((RenderFlex) renderObject).crossAxisAlignment = crossAxisAlignment;
            ((RenderFlex) renderObject).textDirection = textDirection ?? TextDirection.ltr;
            ((RenderFlex) renderObject).verticalDirection = verticalDirection;
            ((RenderFlex) renderObject).textBaseline = textBaseline ?? TextBaseline.alphabetic;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<Axis>("direction", direction));
            properties.add(new EnumProperty<MainAxisAlignment>("mainAxisAlignment", mainAxisAlignment));
            properties.add(new EnumProperty<MainAxisSize>("mainAxisSize", mainAxisSize,
                defaultValue: MainAxisSize.max));
            properties.add(new EnumProperty<CrossAxisAlignment>("crossAxisAlignment", crossAxisAlignment));
            properties.add(new EnumProperty<TextDirection?>("textDirection", textDirection, defaultValue: null));
            properties.add(new EnumProperty<VerticalDirection>("verticalDirection", verticalDirection,
                defaultValue: VerticalDirection.down));
            properties.add(new EnumProperty<TextBaseline?>("textBaseline", textBaseline, defaultValue: null));
        }
    }


    public class Row : Flex {
        public Row(
            Key key = null,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.start,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.center,
            TextDirection? textDirection = null,
            VerticalDirection verticalDirection = VerticalDirection.down,
            TextBaseline? textBaseline = null,
            List<Widget> children = null
        ) : base(
            children: children ?? new List<Widget>(),
            key: key,
            direction: Axis.horizontal,
            textDirection: textDirection,
            textBaseline: textBaseline,
            mainAxisAlignment: mainAxisAlignment,
            mainAxisSize: mainAxisSize,
            crossAxisAlignment: crossAxisAlignment,
            verticalDirection: verticalDirection
        ) {
        }
    }

    public class Column : Flex {
        public Column(
            Key key = null,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.start,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.center,
            TextDirection? textDirection = null,
            VerticalDirection verticalDirection = VerticalDirection.down,
            TextBaseline? textBaseline = null,
            List<Widget> children = null
        ) : base(
            children: children ?? new List<Widget>(),
            key: key,
            direction: Axis.vertical,
            textDirection: textDirection,
            textBaseline: textBaseline,
            mainAxisAlignment: mainAxisAlignment,
            mainAxisSize: mainAxisSize,
            crossAxisAlignment: crossAxisAlignment,
            verticalDirection: verticalDirection
        ) {
        }
    }

    public class Flexible : ParentDataWidget<FlexParentData> {
        public Flexible(
            Key key = null,
            int flex = 1,
            FlexFit fit = FlexFit.loose,
            Widget child = null
        ) : base(key: key, child: child) {
            this.flex = flex;
            this.fit = fit;
        }

        public readonly int flex;

        public readonly FlexFit fit;

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is FlexParentData);
            FlexParentData parentData = (FlexParentData) renderObject.parentData;
            bool needsLayout = false;

            if (parentData.flex != flex) {
                parentData.flex = flex;
                needsLayout = true;
            }

            if (parentData.fit != fit) {
                parentData.fit = fit;
                needsLayout = true;
            }

            if (needsLayout) {
                var targetParent = renderObject.parent;
                if (targetParent is RenderObject) {
                    ((RenderObject) targetParent).markNeedsLayout();
                }
            }
        }

        public override Type debugTypicalAncestorWidgetClass {
            get { return typeof(Flex); }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new IntProperty("flex", flex));
        }
    }

    public class Expanded : Flexible {
        public Expanded(
            Key key = null,
            int flex = 1,
            Widget child = null
        ) : base(key: key, flex: flex, fit: FlexFit.tight, child: child) {
            D.assert(child != null);
        }
    }

    public class Wrap : MultiChildRenderObjectWidget {
        public Wrap(
            Key key = null,
            Axis direction = Axis.horizontal,
            WrapAlignment alignment = WrapAlignment.start,
            float spacing = 0.0f,
            WrapAlignment runAlignment = WrapAlignment.start,
            float runSpacing = 0.0f,
            WrapCrossAlignment crossAxisAlignment = WrapCrossAlignment.start,
            TextDirection? textDirection = null,
            VerticalDirection verticalDirection = VerticalDirection.down,
            List<Widget> children = null
        ) : base(key: key, children: children) {
            this.direction = direction;
            this.alignment = alignment;
            this.spacing = spacing;
            this.runAlignment = runAlignment;
            this.runSpacing = runSpacing;
            this.crossAxisAlignment = crossAxisAlignment;
            this.textDirection = textDirection;
            this.verticalDirection = verticalDirection;
        }

        public readonly Axis direction;

        public readonly WrapAlignment alignment;

        public readonly float spacing;

        public readonly WrapAlignment runAlignment;

        public readonly float runSpacing;

        public readonly WrapCrossAlignment crossAxisAlignment;

        public readonly TextDirection? textDirection;

        public readonly VerticalDirection verticalDirection;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderWrap(
                children: null,
                direction: direction,
                alignment: alignment,
                spacing: spacing,
                runAlignment: runAlignment,
                runSpacing: runSpacing,
                crossAxisAlignment: crossAxisAlignment,
                textDirection: textDirection ?? Directionality.of(context),
                verticalDirection: verticalDirection
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            D.assert(renderObject is RenderWrap);
            RenderWrap renderWrap = renderObject as RenderWrap;
            renderWrap.direction = direction;
            renderWrap.alignment = alignment;
            renderWrap.spacing = spacing;
            renderWrap.runAlignment = runAlignment;
            renderWrap.runSpacing = runSpacing;
            renderWrap.crossAxisAlignment = crossAxisAlignment;
            renderWrap.textDirection = textDirection ?? Directionality.of(context);
            renderWrap.verticalDirection = verticalDirection;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<Axis>("direction", direction));
            properties.add(new EnumProperty<WrapAlignment>("alignment", alignment));
            properties.add(new FloatProperty("spacing", spacing));
            properties.add(new EnumProperty<WrapAlignment>("runAlignment", runAlignment));
            properties.add(new FloatProperty("runSpacing", runSpacing));
            properties.add(new FloatProperty("crossAxisAlignment", runSpacing));
            properties.add(new EnumProperty<TextDirection?>("textDirection", textDirection, defaultValue: null));
            properties.add(new EnumProperty<VerticalDirection>("verticalDirection", verticalDirection,
                defaultValue: VerticalDirection.down));
        }
    }


    public static class LayoutUtils {
        public static AxisDirection? getAxisDirectionFromAxisReverseAndDirectionality(
            BuildContext context,
            Axis axis,
            bool reverse
        ) {
            switch (axis) {
                case Axis.horizontal:
                    D.assert(WidgetsD.debugCheckHasDirectionality(context));
                    TextDirection textDirection = Directionality.of(context);
                    AxisDirection? axisDirection = AxisUtils.textDirectionToAxisDirection(textDirection);
                    return reverse ? AxisUtils.flipAxisDirection(axisDirection) : axisDirection;
                case Axis.vertical:
                    return reverse ? AxisDirection.up : AxisDirection.down;
            }

            throw new Exception("unknown axisDirection");
        }
    }
    
    public class Flow : MultiChildRenderObjectWidget {
        public Flow(
            Key key = null,
            FlowDelegate _delegate = null,
            List<Widget> children = null
            ) : base(key: key, children: children == null ? new List<Widget>() : new List<Widget>(RepaintBoundary.wrapAll(children))) 
        {
            D.assert(_delegate != null);
            this._delegate = _delegate;
        }
           
        public Flow(
            bool unwrapped,
            Key key = null,
            FlowDelegate _delegate = null,
            List<Widget> children = null
            ) : base(key: key, children: children ?? new List<Widget>()) 
        {
            D.assert(_delegate != null);
            this._delegate = _delegate;
        }
        
        private readonly FlowDelegate _delegate;

        public override RenderObject createRenderObject(BuildContext context) => new RenderFlow(del: _delegate);
                
        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var renderFlow = renderObject as RenderFlow;
            renderFlow.del = _delegate;
        }
    }

    public class RichText : MultiChildRenderObjectWidget {
        public RichText(
            Key key = null,
            InlineSpan text = null,
            TextAlign textAlign = TextAlign.start,
            TextDirection? textDirection = null,
            bool softWrap = true,
            TextOverflow overflow = TextOverflow.clip,
            float textScaleFactor = 1.0f,
            int? maxLines = null,
            Locale locale = null,
            StrutStyle strutStyle = null,
            TextWidthBasis textWidthBasis = TextWidthBasis.parent,
            TextHeightBehavior textHeightBehavior = null
        ) : base(key: key, children: _extractChildren(text)) {
            D.assert(text != null);
            D.assert(maxLines == null || maxLines > 0);

            this.text = text;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
            this.maxLines = maxLines;
            this.locale = locale;
            this.strutStyle = strutStyle;
            this.textWidthBasis = textWidthBasis;
            this.textHeightBehavior = textHeightBehavior;
        }


        public static List<Widget> _extractChildren(InlineSpan span) {
            List<Widget> result = new List<Widget>();
            span.visitChildren((InlineSpan span1) => {
                if (span1 is WidgetSpan) {
                    result.Add(((WidgetSpan) span1).child);
                }

                return true;
            });
            return result;
        }

        public readonly InlineSpan text;
        public readonly TextAlign textAlign;
        public readonly TextDirection? textDirection;
        public readonly bool softWrap;
        public readonly TextOverflow overflow;
        public readonly float textScaleFactor;
        public readonly int? maxLines;
        public readonly Locale locale;
        public readonly StrutStyle strutStyle;
        public readonly TextWidthBasis textWidthBasis;
        public readonly TextHeightBehavior textHeightBehavior;

        public override RenderObject createRenderObject(BuildContext context) {
            D.assert(textDirection != null || WidgetsD.debugCheckHasDirectionality(context));
            return new RenderParagraph(text: text,
                textAlign: textAlign,
                textDirection: textDirection ?? Directionality.of(context),
                softWrap: softWrap,
                overflow: overflow,
                textScaleFactor: textScaleFactor,
                maxLines: maxLines,
                strutStyle: strutStyle,
                textWidthBasis: textWidthBasis,
                textHeightBehavior: textHeightBehavior,
                locale: locale ?? Localizations.localeOf(context, nullOk: true)
            );
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (RenderParagraph) renderObject;
            D.assert(textDirection != null || WidgetsD.debugCheckHasDirectionality(context));
            ((RenderParagraph) renderObject).text = text;
            ((RenderParagraph) renderObject).textAlign = textAlign;
            ((RenderParagraph) renderObject).textDirection = textDirection ?? Directionality.of(context);
            ((RenderParagraph) renderObject).softWrap = softWrap;
            ((RenderParagraph) renderObject).overflow = overflow;
            ((RenderParagraph) renderObject).textScaleFactor = textScaleFactor;
            ((RenderParagraph) renderObject).maxLines = maxLines;
            ((RenderParagraph) renderObject).strutStyle = strutStyle;
            ((RenderParagraph) renderObject).textWidthBasis = textWidthBasis;
            ((RenderParagraph) renderObject).textHeightBehavior = textHeightBehavior;
            ((RenderParagraph) renderObject).locale = locale ?? Localizations.localeOf(context, nullOk: true);
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<TextAlign>("textAlign", textAlign, defaultValue: TextAlign.start));
            properties.add(new EnumProperty<TextDirection>("textDirection", textDirection.Value, defaultValue: null));
            properties.add(new FlagProperty("softWrap", value: softWrap, ifTrue: "wrapping at box width",
                ifFalse: "no wrapping except at line break characters", showName: true));
            properties.add(new EnumProperty<TextOverflow>("overflow", overflow, defaultValue: TextOverflow.clip));
            properties.add(new FloatProperty("textScaleFactor", textScaleFactor, defaultValue: 1.0));
            properties.add(new IntProperty("maxLines", maxLines, ifNull: "unlimited"));
            properties.add(new EnumProperty<TextWidthBasis>("textWidthBasis", textWidthBasis,
                defaultValue: TextWidthBasis.parent));
            properties.add(new StringProperty("text", text.toPlainText()));
        }
    }

    public class RawImage : LeafRenderObjectWidget {
        public RawImage(
            Key key = null,
            ui.Image image = null,
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
            bool invertColors = false,
            FilterQuality filterQuality = FilterQuality.low
        ) : base(key) {
            this.image = image;
            this.width = width;
            this.height = height;
            this.scale = scale;
            this.color = color;
            this.colorBlendMode = colorBlendMode;
            this.fit = fit;
            this.alignment = alignment ?? Alignment.center;
            this.repeat = repeat;
            this.centerSlice = centerSlice;
            this.invertColors = invertColors;
            this.matchTextDirection = matchTextDirection;
            this.filterQuality = filterQuality;
        }

        public readonly ui.Image image;
        public readonly float? width;
        public readonly float? height;
        public readonly float scale;
        public readonly Color color;
        public readonly BlendMode colorBlendMode;
        public readonly BoxFit? fit;
        public readonly AlignmentGeometry alignment;
        public readonly ImageRepeat repeat;
        public readonly Rect centerSlice;
        public readonly bool invertColors;
        public readonly bool matchTextDirection;
        public readonly FilterQuality filterQuality;

        public override RenderObject createRenderObject(BuildContext context) {
            TextDirection? textDirection = null;
            if ((matchTextDirection || !(alignment is Alignment))) {
                textDirection = Directionality.of(context);
            }

            return new RenderImage(
                image: image,
                width: width,
                height: height,
                scale: scale,
                color: color,
                colorBlendMode: colorBlendMode,
                fit: fit,
                alignment: alignment,
                repeat: repeat,
                centerSlice: centerSlice,
                matchTextDirection: matchTextDirection,
                textDirection: textDirection,
                invertColors: invertColors,
                filterQuality: filterQuality
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            TextDirection? textDirection = null;
            if ((matchTextDirection || !(alignment is Alignment))) {
                textDirection = Directionality.of(context);
            }

            var renderImage = (RenderImage) renderObject;
            renderImage.image = image;
            renderImage.width = width;
            renderImage.height = height;
            renderImage.scale = scale;
            renderImage.color = color;
            renderImage.colorBlendMode = colorBlendMode;
            renderImage.alignment = alignment;
            renderImage.fit = fit;
            renderImage.repeat = repeat;
            renderImage.centerSlice = centerSlice;
            renderImage.matchTextDirection = matchTextDirection;
            renderImage.textDirection = textDirection;
            renderImage.invertColors = invertColors;
            renderImage.filterQuality = filterQuality;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ui.Image>("image", image));
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
            properties.add(new FlagProperty("matchTextDirection", value: matchTextDirection,
                ifTrue: "match text direction"));
            properties.add(new DiagnosticsProperty<bool>("invertColors", invertColors));
            properties.add(new EnumProperty<FilterQuality>("filterMode", filterQuality));
        }
    }

    public class DefaultAssetBundle : InheritedWidget {
        public DefaultAssetBundle(
            Key key = null,
            AssetBundle bundle = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(bundle != null);
            D.assert(child != null);
            this.bundle = bundle;
        }

        public readonly AssetBundle bundle;

        public static AssetBundle of(BuildContext context) {
            DefaultAssetBundle result =
                (DefaultAssetBundle) context.dependOnInheritedWidgetOfExactType<DefaultAssetBundle>();
            return result?.bundle;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return bundle != ((DefaultAssetBundle) oldWidget).bundle;
        }
    }

    public class WidgetToRenderBoxAdapter : LeafRenderObjectWidget {
        public WidgetToRenderBoxAdapter(
            RenderBox renderBox = null,
            VoidCallback onBuild = null
        ) : base(key: new GlobalObjectKey<State>(renderBox)) {
            D.assert(renderBox != null);
            this.onBuild = onBuild;
        }


        public readonly RenderBox renderBox;
        public readonly VoidCallback onBuild;

        public override RenderObject createRenderObject(BuildContext context) {
            return renderBox;
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            if (onBuild != null) {
                onBuild();
            }
        }
    }

    public class Listener : StatelessWidget {
        public Listener(
            Key key = null,
            PointerDownEventListener onPointerDown = null,
            PointerMoveEventListener onPointerMove = null,
            PointerEnterEventListener onPointerEnter = null, //
            PointerExitEventListener onPointerExit = null, //
            PointerHoverEventListener onPointerHover = null,
            PointerUpEventListener onPointerUp = null,
            PointerCancelEventListener onPointerCancel = null,
            PointerSignalEventListener onPointerSignal = null,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            Widget child = null
        ) : base(key: key) {
            _child = child;
            this.onPointerDown = onPointerDown;
            this.onPointerMove = onPointerMove;
            this.onPointerUp = onPointerUp;
            this.onPointerCancel = onPointerCancel;
            this.onPointerSignal = onPointerSignal;
            this.onPointerHover = onPointerHover;
            //this.onPointerExit = onPointerExit;
            //this.onPointerEnter = onPointerEnter;
            this.behavior = behavior;
        }


        public readonly PointerDownEventListener onPointerDown;

        public readonly PointerMoveEventListener onPointerMove;

        public readonly PointerUpEventListener onPointerUp;

        public readonly PointerCancelEventListener onPointerCancel;

        public readonly PointerSignalEventListener onPointerSignal;

        public readonly PointerHoverEventListener onPointerHover;

        public readonly PointerEnterEventListener onPointerEnter;

        public readonly PointerExitEventListener onPointerExit;

        //public readonly PointerScrollEventListener onPointerScroll;

        public readonly HitTestBehavior behavior;

        //public readonly PointerDragFromEditorEnterEventListener onPointerDragFromEditorEnter;

        //public readonly PointerDragFromEditorHoverEventListener onPointerDragFromEditorHover;

        //public readonly PointerDragFromEditorExitEventListener onPointerDragFromEditorExit;

        //public readonly PointerDragFromEditorReleaseEventListener onPointerDragFromEditorRelease;

        public readonly Widget _child;

        public override Widget build(BuildContext context) {
            Widget result = _child;
            if (onPointerEnter != null ||
                onPointerExit != null ||
                onPointerHover != null) {
                result = new MouseRegion(
                    onEnter: onPointerEnter,
                    onExit: onPointerExit,
                    onHover: onPointerHover,
                    opaque: false,
                    child: result
                );
            }

            result = new _PointerListener(
                onPointerDown: onPointerDown,
                onPointerUp: onPointerUp,
                onPointerMove: onPointerMove,
                onPointerCancel: onPointerCancel,
                onPointerSignal: onPointerSignal,
                behavior: behavior,
                child: result
            );
            return result;
        }
    }

    public class _PointerListener : SingleChildRenderObjectWidget {
        public _PointerListener(
            Key key = null,
            PointerDownEventListener onPointerDown = null,
            PointerMoveEventListener onPointerMove = null,
            PointerUpEventListener onPointerUp = null,
            PointerCancelEventListener onPointerCancel = null,
            PointerSignalEventListener onPointerSignal = null,
            HitTestBehavior? behavior = HitTestBehavior.deferToChild,
            Widget child = null
        ) :
            base(key: key, child: child) {
            D.assert(behavior != null);
            this.onPointerDown = onPointerDown;
            this.onPointerMove = onPointerMove;
            this.onPointerUp = onPointerUp;
            this.onPointerCancel = onPointerCancel;
            this.onPointerSignal = onPointerSignal;
            this.behavior = behavior.Value;
        }

        public readonly PointerDownEventListener onPointerDown;
        public readonly PointerMoveEventListener onPointerMove;
        public readonly PointerUpEventListener onPointerUp;
        public readonly PointerCancelEventListener onPointerCancel;
        public readonly PointerSignalEventListener onPointerSignal;
        public readonly HitTestBehavior behavior;


        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPointerListener(
                onPointerDown: onPointerDown,
                onPointerMove: onPointerMove,
                onPointerUp: onPointerUp,
                onPointerCancel: onPointerCancel,
                onPointerSignal: onPointerSignal,
                behavior: behavior
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderPointerListener) renderObjectRaw;
            renderObject.onPointerDown = onPointerDown;
            renderObject.onPointerMove = onPointerMove;
            renderObject.onPointerUp = onPointerUp;
            renderObject.onPointerCancel = onPointerCancel;
            renderObject.onPointerSignal = onPointerSignal;
            renderObject.behavior = behavior;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            List<string> listeners = new List<string>();
            if (onPointerDown != null) {
                listeners.Add("down");
            }

            if (onPointerMove != null) {
                listeners.Add("move");
            }

            if (onPointerUp != null) {
                listeners.Add("up");
            }

            if (onPointerCancel != null) {
                listeners.Add("cancel");
            }

            if (onPointerSignal != null) {
                listeners.Add("signal");
            }

            properties.add(new EnumerableProperty<string>("listeners", listeners, ifEmpty: "<none>"));
            properties.add(new EnumProperty<HitTestBehavior>("behavior", behavior));
        }
    }

    public class RepaintBoundary : SingleChildRenderObjectWidget {
        public RepaintBoundary(
            Key key = null,
            Widget child = null) :
            base(key: key, child: child) {
        }

        public static RepaintBoundary wrap(Widget child, int childIndex) {
            D.assert(child != null);
            Key key = child.key != null ? (Key) new ValueKey<Key>(child.key) : new ValueKey<int>(childIndex);
            return new RepaintBoundary(key: key, child: child);
        }

        public static List<RepaintBoundary> wrapAll(List<Widget> widgets) {
            List<RepaintBoundary> result = CollectionUtils.CreateRepeatedList<RepaintBoundary>(null, widgets.Count);
            for (int i = 0; i < result.Count; ++i) {
                result[i] = wrap(widgets[i], i);
            }

            return result;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderRepaintBoundary();
        }
    }

    public class IgnorePointer : SingleChildRenderObjectWidget {
        public IgnorePointer(
            Key key = null,
            bool ignoring = true,
            Widget child = null
        ) : base(key: key, child: child) {
            this.ignoring = ignoring;
        }

        public readonly bool ignoring;


        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderIgnorePointer(
                ignoring: ignoring
            );
        }

        public override
            void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderIgnorePointer) renderObjectRaw;
            renderObject.ignoring = ignoring;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("ignoring", ignoring));
        }
    }

    public class AbsorbPointer : SingleChildRenderObjectWidget {
        public AbsorbPointer(
            Key key = null,
            bool absorbing = true,
            Widget child = null
        ) : base(key: key, child: child) {
            this.absorbing = absorbing;
        }

        public readonly bool absorbing;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderAbsorbPointer(
                absorbing: absorbing
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderAbsorbPointer) renderObject).absorbing = absorbing;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("absorbing", absorbing));
        }
    }

    public class MetaData : SingleChildRenderObjectWidget {
        public MetaData(
            Key key = null,
            object metaData = null,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            Widget child = null) : base(key: key, child: child) {
            this.metaData = metaData;
            this.behavior = behavior;
        }

        public readonly object metaData;

        public readonly HitTestBehavior behavior;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderMetaData(
                metaData: metaData,
                behavior: behavior);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var renderObj = (RenderMetaData) renderObject;
            renderObj.metaData = metaData;
            renderObj.behavior = behavior;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<HitTestBehavior>("behavior", behavior));
            properties.add(new DiagnosticsProperty<object>("metaData", metaData));
        }
    }

    public class IndexedSemantics : SingleChildRenderObjectWidget {
        public IndexedSemantics(
            Key key = null,
            int index = 0,
            Widget child = null
        ) : base(key: key, child: child) {
            this.index = index;
        }

        public readonly int index;


        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderIndexedSemantics(index: index);
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var _renderObject = (RenderIndexedSemantics) renderObject;
            _renderObject.index = index;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<int>("index", index));
        }
    }

    public class KeyedSubtree : StatelessWidget {
        public KeyedSubtree(
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(child != null);
            this.child = child;
        }


        public static KeyedSubtree wrap(Widget child, int childIndex) {
            Key key = child.key != null ? (Key) new ValueKey<Key>(child.key) : new ValueKey<int>(childIndex);
            return new KeyedSubtree(key: key, child: child);
        }

        public readonly Widget child;

        public static List<Widget> ensureUniqueKeysForList(IEnumerable<Widget> items, int baseIndex = 0) {
            if (items == null) {
                return null;
            }

            List<Widget> itemsWithUniqueKeys = new List<Widget>();
            int itemIndex = baseIndex;
            foreach (Widget item in items) {
                itemsWithUniqueKeys.Add(wrap(item, itemIndex));
                itemIndex += 1;
            }

            D.assert(!WidgetsD.debugItemsHaveDuplicateKeys(itemsWithUniqueKeys));
            return itemsWithUniqueKeys;
        }

        public override Widget build(BuildContext context) {
            return child;
        }
    }


    public class Builder : StatelessWidget {
        public Builder(
            Key key = null,
            WidgetBuilder builder = null
        ) : base(key: key) {
            D.assert(builder != null);
            this.builder = builder;
        }

        public readonly WidgetBuilder builder;

        public override Widget build(BuildContext context) {
            return builder(context);
        }
    }

    public delegate Widget StatefulWidgetBuilder(BuildContext context, StateSetter setState);

    public class StatefulBuilder : StatefulWidget {
        public StatefulBuilder(
            Key key = null,
            StatefulWidgetBuilder builder = null
        ) : base(key: key) {
            D.assert(builder != null);
            this.builder = builder;
        }


        public readonly StatefulWidgetBuilder builder;

        public override State createState() {
            return new _StatefulBuilderState();
        }
    }

    public class _StatefulBuilderState : State<StatefulBuilder> {
        public override Widget build(BuildContext context) {
            return widget.builder(context, setState);
        }
    }


    public class ColoredBox : SingleChildRenderObjectWidget {
        public ColoredBox(
            Color color = null,
            Widget child = null,
            Key key = null) : base(key: key, child: child) {
            D.assert(color != null);
            this.color = color;
        }

        public readonly Color color;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderColoredBox(color: color);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (_RenderColoredBox) renderObject;
            ((_RenderColoredBox) renderObject).color = color;
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Color>("color", color));
        }
    }

    public class _RenderColoredBox : RenderProxyBoxWithHitTestBehavior {
        public _RenderColoredBox(Color color = null)
            : base(behavior: HitTestBehavior.opaque) {
            _color = color;
        }

        public Color color {
            get { return _color; }
            set {
                D.assert(value != null);
                if (value == _color) {
                    return;
                }

                _color = value;
                markNeedsPaint();
            }
        }

        Color _color;

        public override void paint(PaintingContext context, Offset offset) {
            if (size > Size.zero) {
                Paint paint = new Paint();
                paint.color = color;
                context.canvas.drawRect(offset & size, paint);
            }

            if (child != null) {
                context.paintChild(child, offset);
            }
        }
    }
}