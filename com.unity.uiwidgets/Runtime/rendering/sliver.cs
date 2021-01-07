using System;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public enum GrowthDirection {
        forward,
        reverse,
    }

    public static class GrowthDirectionUtils {
        public static AxisDirection applyGrowthDirectionToAxisDirection(
            AxisDirection axisDirection, GrowthDirection growthDirection) {
            switch (growthDirection) {
                case GrowthDirection.forward:
                    return axisDirection;
                case GrowthDirection.reverse:
                    return AxisUtils.flipAxisDirection(axisDirection);
            }

            throw new Exception("unknown growthDirection");
        }

        public static ScrollDirection applyGrowthDirectionToScrollDirection(
            ScrollDirection scrollDirection, GrowthDirection growthDirection) {
            switch (growthDirection) {
                case GrowthDirection.forward:
                    return scrollDirection;
                case GrowthDirection.reverse:
                    return ScrollDirectionUtils.flipScrollDirection(scrollDirection);
            }

            throw new Exception("unknown growthDirection");
        }
    }

    public class SliverConstraints : Constraints, IEquatable<SliverConstraints> {
        public SliverConstraints(
            AxisDirection axisDirection,
            GrowthDirection growthDirection,
            ScrollDirection userScrollDirection,
            float scrollOffset,
            float precedingScrollExtent,
            float overlap,
            float remainingPaintExtent,
            float crossAxisExtent,
            AxisDirection crossAxisDirection,
            float viewportMainAxisExtent,
            float remainingCacheExtent,
            float cacheOrigin
        ) {
            this.axisDirection = axisDirection;
            this.growthDirection = growthDirection;
            this.userScrollDirection = userScrollDirection;
            this.scrollOffset = scrollOffset;
            this.precedingScrollExtent = precedingScrollExtent;
            this.overlap = overlap;
            this.remainingPaintExtent = remainingPaintExtent;
            this.crossAxisExtent = crossAxisExtent;
            this.crossAxisDirection = crossAxisDirection;
            this.viewportMainAxisExtent = viewportMainAxisExtent;
            this.remainingCacheExtent = remainingCacheExtent;
            this.cacheOrigin = cacheOrigin;
        }

        public SliverConstraints copyWith(
            AxisDirection? axisDirection = null,
            GrowthDirection? growthDirection = null,
            ScrollDirection? userScrollDirection = null,
            float? scrollOffset = null,
            float? precedingScrollExtent = null,
            float? overlap = null,
            float? remainingPaintExtent = null,
            float? crossAxisExtent = null,
            AxisDirection? crossAxisDirection = null,
            float? viewportMainAxisExtent = null,
            float? remainingCacheExtent = null,
            float? cacheOrigin = null
        ) {
            return new SliverConstraints(
                axisDirection: axisDirection ?? this.axisDirection,
                growthDirection: growthDirection ?? this.growthDirection,
                userScrollDirection: userScrollDirection ?? this.userScrollDirection,
                scrollOffset: scrollOffset ?? this.scrollOffset,
                precedingScrollExtent: precedingScrollExtent ?? this.precedingScrollExtent,
                overlap: overlap ?? this.overlap,
                remainingPaintExtent: remainingPaintExtent ?? this.remainingPaintExtent,
                crossAxisExtent: crossAxisExtent ?? this.crossAxisExtent,
                crossAxisDirection: crossAxisDirection ?? this.crossAxisDirection,
                viewportMainAxisExtent: viewportMainAxisExtent ?? this.viewportMainAxisExtent,
                remainingCacheExtent: remainingCacheExtent ?? this.remainingCacheExtent,
                cacheOrigin: cacheOrigin ?? this.cacheOrigin
            );
        }

        public readonly AxisDirection axisDirection;

        public readonly GrowthDirection growthDirection;

        public readonly ScrollDirection userScrollDirection;

        public readonly float scrollOffset;
        
        public readonly float precedingScrollExtent;

        public readonly float overlap;

        public readonly float remainingPaintExtent;

        public readonly float crossAxisExtent;

        public readonly AxisDirection crossAxisDirection;

        public readonly float viewportMainAxisExtent;

        public readonly float cacheOrigin;

        public readonly float remainingCacheExtent;

        public Axis axis {
            get { return AxisUtils.axisDirectionToAxis(axisDirection); }
        }

        public GrowthDirection normalizedGrowthDirection {
            get {
                switch (axisDirection) {
                    case AxisDirection.down:
                    case AxisDirection.right:
                        return growthDirection;
                    case AxisDirection.up:
                    case AxisDirection.left:
                        switch (growthDirection) {
                            case GrowthDirection.forward:
                                return GrowthDirection.reverse;
                            case GrowthDirection.reverse:
                                return GrowthDirection.forward;
                        }

                        throw new Exception("unknown growthDirection");
                }

                throw new Exception("unknown axisDirection");
            }
        }

        public override bool isTight {
            get { return false; }
        }

        public override bool isNormalized {
            get {
                return scrollOffset >= 0.0f
                       && crossAxisExtent >= 0.0f
                       && AxisUtils.axisDirectionToAxis(axisDirection) !=
                       AxisUtils.axisDirectionToAxis(crossAxisDirection)
                       && viewportMainAxisExtent >= 0.0f
                       && remainingPaintExtent >= 0.0f;
            }
        }

        public BoxConstraints asBoxConstraints(
            float minExtent = 0.0f,
            float maxExtent = float.PositiveInfinity,
            float? crossAxisExtent = null
        ) {
            crossAxisExtent = crossAxisExtent ?? this.crossAxisExtent;
            switch (axis) {
                case Axis.horizontal:
                    return new BoxConstraints(
                        minHeight: crossAxisExtent.Value,
                        maxHeight: crossAxisExtent.Value,
                        minWidth: minExtent,
                        maxWidth: maxExtent
                    );
                case Axis.vertical:
                    return new BoxConstraints(
                        minWidth: crossAxisExtent.Value,
                        maxWidth: crossAxisExtent.Value,
                        minHeight: minExtent,
                        maxHeight: maxExtent
                    );
            }

            D.assert(false);
            return null;
        }

        public override bool debugAssertIsValid(
            bool isAppliedConstraint = false,
            InformationCollector informationCollector = null
        ) {
            D.assert(() => {
                var verify = new Action<bool, string>((bool check, string message) => {
                    if (check) {
                        return;
                    }

                    var information = new StringBuilder();
                    if (informationCollector != null) {
                        informationCollector(information);
                    }

                    throw new UIWidgetsError(
                        $"{GetType()} is not valid: {message}\n{information}The offending constraints were: \n  {this}");
                });

                verify(scrollOffset >= 0.0f, "The \"scrollOffset\" is negative.");
                verify(crossAxisExtent >= 0.0f, "The \"crossAxisExtent\" is negative.");
                verify(
                    AxisUtils.axisDirectionToAxis(axisDirection) !=
                    AxisUtils.axisDirectionToAxis(crossAxisDirection),
                    "The \"axisDirection\" and the \"crossAxisDirection\" are along the same axis.");
                verify(viewportMainAxisExtent >= 0.0f, "The \"viewportMainAxisExtent\" is negative.");
                verify(remainingPaintExtent >= 0.0f, "The \"remainingPaintExtent\" is negative.");
                verify(remainingCacheExtent >= 0.0f, "The \"remainingCacheExtent\" is negative.");
                verify(cacheOrigin <= 0.0f, "The \"cacheOrigin\" is positive.");
                verify(isNormalized, "The constraints are not normalized.");
                return true;
            });

            return true;
        }

        public bool Equals(SliverConstraints other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return axisDirection == other.axisDirection
                   && growthDirection == other.growthDirection
                   && userScrollDirection == other.userScrollDirection
                   && scrollOffset.Equals(other.scrollOffset)
                   && overlap.Equals(other.overlap)
                   && remainingPaintExtent.Equals(other.remainingPaintExtent)
                   && crossAxisExtent.Equals(other.crossAxisExtent)
                   && crossAxisDirection == other.crossAxisDirection
                   && viewportMainAxisExtent.Equals(other.viewportMainAxisExtent)
                   && cacheOrigin.Equals(other.cacheOrigin)
                   && remainingCacheExtent.Equals(other.remainingCacheExtent);
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

            return Equals((SliverConstraints) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) axisDirection;
                hashCode = (hashCode * 397) ^ (int) growthDirection;
                hashCode = (hashCode * 397) ^ (int) userScrollDirection;
                hashCode = (hashCode * 397) ^ scrollOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ overlap.GetHashCode();
                hashCode = (hashCode * 397) ^ remainingPaintExtent.GetHashCode();
                hashCode = (hashCode * 397) ^ crossAxisExtent.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) crossAxisDirection;
                hashCode = (hashCode * 397) ^ viewportMainAxisExtent.GetHashCode();
                hashCode = (hashCode * 397) ^ cacheOrigin.GetHashCode();
                hashCode = (hashCode * 397) ^ remainingCacheExtent.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SliverConstraints left, SliverConstraints right) {
            return Equals(left, right);
        }

        public static bool operator !=(SliverConstraints left, SliverConstraints right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return
                $"SliverConstraints({axisDirection}， {growthDirection}， {userScrollDirection}， scrollOffset: {scrollOffset:F1}, remainingPaintExtent: {remainingCacheExtent:F1}, " +
                $"{(overlap != 0.0f ? "overlap: " + overlap.ToString("F1") + ", " : "")}crossAxisExtent: {crossAxisExtent:F1}, crossAxisDirection: {crossAxisDirection}, " +
                $"viewportMainAxisExtent: {viewportMainAxisExtent:F1}, remainingCacheExtent: {remainingCacheExtent:F1} " +
                $"cacheOrigin: {cacheOrigin:F1})";
        }
    }

    public class SliverGeometry : Diagnosticable {
        public SliverGeometry(
            float scrollExtent = 0.0f,
            float paintExtent = 0.0f,
            float paintOrigin = 0.0f,
            float? layoutExtent = null,
            float maxPaintExtent = 0.0f,
            float maxScrollObstructionExtent = 0.0f,
            float? hitTestExtent = null,
            bool? visible = null,
            bool hasVisualOverflow = false,
            float? scrollOffsetCorrection = null,
            float? cacheExtent = null
        ) {
            D.assert(scrollOffsetCorrection != 0.0f);

            this.scrollExtent = scrollExtent;
            this.paintExtent = paintExtent;
            this.paintOrigin = paintOrigin;
            this.layoutExtent = layoutExtent ?? paintExtent;
            this.maxPaintExtent = maxPaintExtent;
            this.maxScrollObstructionExtent = maxScrollObstructionExtent;
            this.hitTestExtent = hitTestExtent ?? paintExtent;
            this.visible = visible ?? paintExtent > 0.0f;
            this.hasVisualOverflow = hasVisualOverflow;
            this.scrollOffsetCorrection = scrollOffsetCorrection;
            this.cacheExtent = cacheExtent ?? layoutExtent ?? paintExtent;
        }

        public static readonly SliverGeometry zero = new SliverGeometry();

        public readonly float scrollExtent;
        public readonly float paintOrigin;
        public readonly float paintExtent;
        public readonly float layoutExtent;
        public readonly float maxPaintExtent;
        public readonly float maxScrollObstructionExtent;
        public readonly float hitTestExtent;
        public readonly bool visible;
        public readonly bool hasVisualOverflow;
        public readonly float? scrollOffsetCorrection;
        public readonly float cacheExtent;
        public const float precisionErrorTolerance = 1e-10f;

        internal static string _debugCompareFloats(string labelA, float valueA, string labelB, float valueB) {
            if (valueA.ToString("F1") != valueB.ToString("F1")) {
                return $"The {labelA} is {valueA:F1}, but the {labelB} is {valueB:F1}. ";
            }

            return string.Format(
                "The {0} is {1}, but the {2} is {3}. " +
                "Maybe you have fallen prey to floating point rounding errors, and should explicitly " +
                "apply the min() or max() functions, or the clamp() method, to the {2}? ",
                labelA, valueA, labelB, valueB);
        }

        public bool debugAssertIsValid(InformationCollector informationCollector = null) {
            D.assert(() => {
                var verify = new Action<bool, string>((bool check, string message) => {
                    if (check) {
                        return;
                    }

                    var information = new StringBuilder();
                    if (informationCollector != null) {
                        informationCollector(information);
                    }

                    throw new UIWidgetsError($"{GetType()} is not valid: {message}\n{information}");
                });

                verify(scrollExtent >= 0.0f, "The \"scrollExtent\" is negative.");
                verify(paintExtent >= 0.0f, "The \"paintExtent\" is negative.");
                verify(layoutExtent >= 0.0f, "The \"layoutExtent\" is negative.");
                verify(cacheExtent >= 0.0f, "The \"cacheExtent\" is negative.");
                if (layoutExtent > paintExtent) {
                    verify(false,
                        "The \"layoutExtent\" exceeds the \"paintExtent\".\n" +
                        _debugCompareFloats("paintExtent", paintExtent, "layoutExtent",
                            layoutExtent)
                    );
                }

                if (paintExtent - maxPaintExtent > precisionErrorTolerance) {
                    verify(false,
                        "The \"maxPaintExtent\" is less than the \"paintExtent\".\n" +
                        _debugCompareFloats("maxPaintExtent", maxPaintExtent, "paintExtent",
                            paintExtent) +
                        "By definition, a sliver can\"t paint more than the maximum that it can paint!"
                    );
                }

                verify(hitTestExtent >= 0.0f, "The \"hitTestExtent\" is negative.");
                verify(scrollOffsetCorrection != 0.0f, "The \"scrollOffsetCorrection\" is zero.");
                return true;
            });
            return true;
        }

        public override string toStringShort() {
            return GetType().ToString();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("scrollExtent", scrollExtent));
            if (paintExtent > 0.0f) {
                properties.add(new FloatProperty("paintExtent", paintExtent,
                    unit: visible ? null : " but not painting"));
            }
            else if (paintExtent == 0.0f) {
                if (visible) {
                    properties.add(new FloatProperty("paintExtent", paintExtent,
                        unit: visible ? null : " but visible"));
                }

                properties.add(new FlagProperty("visible", value: visible, ifFalse: "hidden"));
            }
            else {
                properties.add(new FloatProperty("paintExtent", paintExtent, tooltip: "!"));
            }

            properties.add(new FloatProperty("paintOrigin", paintOrigin,
                defaultValue: 0.0f));
            properties.add(new FloatProperty("layoutExtent", layoutExtent,
                defaultValue: paintExtent));
            properties.add(new FloatProperty("maxPaintExtent", maxPaintExtent));
            properties.add(new FloatProperty("hitTestExtent", hitTestExtent,
                defaultValue: paintExtent));
            properties.add(new DiagnosticsProperty<bool>("hasVisualOverflow", hasVisualOverflow,
                defaultValue: false));
            properties.add(new FloatProperty("scrollOffsetCorrection", scrollOffsetCorrection,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new FloatProperty("cacheExtent", cacheExtent,
                defaultValue: 0.0f));
        }
    }


    public class SliverHitTestResult : HitTestResult {
        public delegate bool SliverHitTest(SliverHitTestResult result, float mainAxisPosition, float crossAxisPosition);

        public SliverHitTestResult() : base() {
        }

        public SliverHitTestResult(HitTestResult result) : base(result) {
        }

        public bool addWithAxisOffset(
            Offset paintOffset,
            float mainAxisOffset,
            float crossAxisOffset,
            float mainAxisPosition,
            float crossAxisPosition,
            SliverHitTest hitTest
        ) {
            D.assert(mainAxisOffset != null);
            D.assert(crossAxisOffset != null);
            D.assert(mainAxisPosition != null);
            D.assert(crossAxisPosition != null);
            D.assert(hitTest != null);
            if (paintOffset != null) {
                pushTransform(Matrix4.translationValues(-paintOffset.dx, -paintOffset.dy, 0));
            }
            bool isHit = hitTest(
                this,
                mainAxisPosition: mainAxisPosition - mainAxisOffset,
                crossAxisPosition: crossAxisPosition - crossAxisOffset
            );
            if (paintOffset != null) {
                popTransform();
            }
            return isHit;
        }
    }

    public class SliverHitTestEntry : HitTestEntry {
        public SliverHitTestEntry(RenderSliver target,
            float mainAxisPosition = 0.0f,
            float crossAxisPosition = 0.0f
        ) : base(target) {
            this.mainAxisPosition = mainAxisPosition;
            this.crossAxisPosition = crossAxisPosition;
        }
        
        public new RenderSliver target {
            get { return (RenderSliver) base.target; }
        }

        public readonly float mainAxisPosition;

        public readonly float crossAxisPosition;

        public override string ToString() {
            return $"{target.GetType()}@(mainAix: {mainAxisPosition}, crossAix: {crossAxisPosition})";
        }
    }

    public class SliverLogicalParentData : ParentData {
        public float layoutOffset = 0.0f;

        public override string ToString() {
            return "layoutOffset=" + layoutOffset.ToString("F1");
        }
    }

    public class SliverLogicalContainerParentData : ContainerParentDataMixinSliverLogicalParentData<RenderSliver> {
    }


    public class SliverPhysicalParentData : ParentData {
        public Offset paintOffset = Offset.zero;

        public void applyPaintTransform(Matrix4 transform) {
            transform.translate(paintOffset.dx, paintOffset.dy);
        }

        public override string ToString() {
            return "paintOffset=" + paintOffset;
        }
    }

    public class SliverPhysicalContainerParentData : ContainerParentDataMixinSliverPhysicalParentData<RenderSliver> {
    }

    public abstract class RenderSliver : RenderObject {
        public new SliverConstraints constraints {
            get { return (SliverConstraints) base.constraints; }
        }

        public SliverGeometry geometry {
            get { return _geometry; }
            set {
                D.assert(!(debugDoingThisResize && debugDoingThisLayout));
                D.assert(sizedByParent || !debugDoingThisResize);
                D.assert(() => {
                    if ((sizedByParent && debugDoingThisResize) ||
                        (!sizedByParent && debugDoingThisLayout)) {
                        return true;
                    }

                    D.assert(!debugDoingThisResize);
                    string contract = "", violation = "", hint = "";
                    if (debugDoingThisLayout) {
                        D.assert(sizedByParent);
                        violation = "It appears that the geometry setter was called from performLayout().";
                        hint = "";
                    }
                    else {
                        violation =
                            "The geometry setter was called from outside layout (neither performResize() nor performLayout() were being run for this object).";
                        if (owner != null && owner.debugDoingLayout) {
                            hint =
                                "Only the object itself can set its geometry. It is a contract violation for other objects to set it.";
                        }
                    }

                    if (sizedByParent) {
                        contract =
                            "Because this RenderSliver has sizedByParent set to true, it must set its geometry in performResize().";
                    }
                    else {
                        contract =
                            "Because this RenderSliver has sizedByParent set to false, it must set its geometry in performLayout().";
                    }

                    throw new UIWidgetsError(
                        "RenderSliver geometry setter called incorrectly.\n" +
                        violation + "\n" +
                        hint + "\n" +
                        contract + "\n" +
                        "The RenderSliver in question is:\n" +
                        "  " + this
                    );
                });

                _geometry = value;
            }
        }
        SliverGeometry _geometry;

        public override Rect paintBounds {
            get {
                switch (constraints.axis) {
                    case Axis.horizontal:
                        return Rect.fromLTWH(
                            0.0f, 0.0f,
                            geometry.paintExtent,
                            constraints.crossAxisExtent
                        );
                    case Axis.vertical:
                        return Rect.fromLTWH(
                            0.0f, 0.0f,
                            constraints.crossAxisExtent,
                            geometry.paintExtent
                        );
                }

                D.assert(false);
                return null;
            }
        }

        public override Rect semanticBounds {
            get { return paintBounds; }
        }

        protected override void debugResetSize() {
        }

        protected override void debugAssertDoesMeetConstraints() {
            D.assert(geometry.debugAssertIsValid(
                informationCollector: (information) => {
                    information.AppendLine("The RenderSliver that returned the offending geometry was:");
                    information.AppendLine("  " + toStringShallow(joiner: "\n  "));
                }));
            D.assert(() => {
                if (geometry.paintExtent > constraints.remainingPaintExtent) {
                    throw new UIWidgetsError(
                        "SliverGeometry has a paintOffset that exceeds the remainingPaintExtent from the constraints.\n" +
                        "The render object whose geometry violates the constraints is the following:\n" +
                        "  " + toStringShallow(joiner: "\n  ") + "\n" +
                        SliverGeometry._debugCompareFloats(
                            "remainingPaintExtent", constraints.remainingPaintExtent,
                            "paintExtent", geometry.paintExtent) +
                        "The paintExtent must cause the child sliver to paint within the viewport, and so " +
                        "cannot exceed the remainingPaintExtent."
                    );
                }

                return true;
            });
        }

        protected override void performResize() {
            D.assert(false);
        }

        public float centerOffsetAdjustment {
            get { return 0.0f; }
        }

        public virtual bool hitTest(SliverHitTestResult result, float mainAxisPosition = 0, float crossAxisPosition = 0) {
            if (mainAxisPosition >= 0.0f && mainAxisPosition < geometry.hitTestExtent &&
                crossAxisPosition >= 0.0f && crossAxisPosition < constraints.crossAxisExtent) {
                if (hitTestChildren(result, mainAxisPosition: mainAxisPosition,
                        crossAxisPosition: crossAxisPosition) ||
                    hitTestSelf(mainAxisPosition: mainAxisPosition, crossAxisPosition: crossAxisPosition)) {
                    result.add(new SliverHitTestEntry(
                        this,
                        mainAxisPosition: mainAxisPosition,
                        crossAxisPosition: crossAxisPosition
                    ));
                    return true;
                }
            }

            return false;
        }

        protected virtual bool hitTestSelf(float mainAxisPosition = 0, float crossAxisPosition = 0) {
            return false;
        }

        protected virtual bool hitTestChildren(SliverHitTestResult result, float mainAxisPosition = 0,
            float crossAxisPosition = 0) {
            return false;
        }

        public float calculatePaintOffset(SliverConstraints constraints, float from, float to) {
            D.assert(from <= to);
            float a = constraints.scrollOffset;
            float b = constraints.scrollOffset + constraints.remainingPaintExtent;
            return (to.clamp(a, b) - from.clamp(a, b)).clamp(0.0f, constraints.remainingPaintExtent);
        }

        public float calculateCacheOffset(SliverConstraints constraints, float from, float to) {
            D.assert(from <= to);
            float a = constraints.scrollOffset + constraints.cacheOrigin;
            float b = constraints.scrollOffset + constraints.remainingCacheExtent;
            return (to.clamp(a, b) - from.clamp(a, b)).clamp(0.0f, constraints.remainingCacheExtent);
        }

        public virtual float childMainAxisPosition(RenderObject child) {
            D.assert(() => { throw new UIWidgetsError(GetType() + " does not implement childPosition."); });

            return 0.0f;
        }

        public virtual float childCrossAxisPosition(RenderObject child) {
            return 0.0f;
        }

        public virtual float childScrollOffset(RenderObject child) {
            D.assert(child.parent == this);
            return 0.0f;
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            D.assert(() => { throw new UIWidgetsError(GetType() + " does not implement applyPaintTransform."); });
        }

        internal Size getAbsoluteSizeRelativeToOrigin() {
            D.assert(geometry != null);
            D.assert(!debugNeedsLayout);

            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(
                constraints.axisDirection, constraints.growthDirection)) {
                case AxisDirection.up:
                    return new Size(constraints.crossAxisExtent, -geometry.paintExtent);
                case AxisDirection.right:
                    return new Size(geometry.paintExtent, constraints.crossAxisExtent);
                case AxisDirection.down:
                    return new Size(constraints.crossAxisExtent, geometry.paintExtent);
                case AxisDirection.left:
                    return new Size(-geometry.paintExtent, constraints.crossAxisExtent);
            }

            D.assert(false);
            return null;
        }

        protected Size getAbsoluteSize() {
            D.assert(geometry != null);
            D.assert(!debugNeedsLayout);
            switch (constraints.axisDirection) {
                case AxisDirection.up:
                case AxisDirection.down:
                    return new Size(constraints.crossAxisExtent, geometry.paintExtent);
                case AxisDirection.right:
                case AxisDirection.left:
                    return new Size(geometry.paintExtent, constraints.crossAxisExtent);
            }
            return null;
        }
        
        void _debugDrawArrow(Canvas canvas, Paint paint, Offset p0, Offset p1, GrowthDirection direction) {
            D.assert(() => {
                if (p0 == p1) {
                    return true;
                }

                D.assert(p0.dx == p1.dx || p0.dy == p1.dy);
                float d = (p1 - p0).distance * 0.2f;
                Offset temp;
                float dx1 = 0, dx2 = 0, dy1 = 0, dy2 = 0;
                switch (direction) {
                    case GrowthDirection.forward:
                        dx1 = dx2 = dy1 = dy2 = d;
                        break;
                    case GrowthDirection.reverse:
                        temp = p0;
                        p0 = p1;
                        p1 = temp;
                        dx1 = dx2 = dy1 = dy2 = -d;
                        break;
                }

                if (p0.dx == p1.dx) {
                    dx2 = -dx2;
                }
                else {
                    dy2 = -dy2;
                }

//      canvas.drawPath(
//        new Path()
//          ..moveTo(p0.dx, p0.dy)
//          ..lineTo(p1.dx, p1.dy)
//          ..moveTo(p1.dx - dx1, p1.dy - dy1)
//          ..lineTo(p1.dx, p1.dy)
//          ..lineTo(p1.dx - dx2, p1.dy - dy2),
//        paint
//      );
                return true;
            });
        }

        public override void debugPaint(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (D.debugPaintSizeEnabled) {
                    float strokeWidth = Mathf.Min(4.0f, geometry.paintExtent / 30.0f);
                    Paint paint = new Paint();
//          ..color = const Color(0xFF33CC33)
//          ..strokeWidth = strokeWidth
//          ..style = PaintingStyle.stroke
//          ..maskFilter = new MaskFilter.blur(BlurStyle.solid, strokeWidth);
                    float arrowExtent = geometry.paintExtent;
                    float padding = Mathf.Max(2.0f, strokeWidth);
                    Canvas canvas = context.canvas;
//        canvas.drawCircle(
//          offset.translate(padding, padding),
//          padding * 0.5,
//          paint,
//        );
                    switch (constraints.axis) {
                        case Axis.vertical:
//            canvas.drawLine(
//              offset,
//              offset.translate(constraints.crossAxisExtent, 0.0f),
//              paint,
//            );
                            _debugDrawArrow(
                                canvas,
                                paint,
                                offset.translate(constraints.crossAxisExtent * 1.0f / 4.0f, padding),
                                offset.translate(constraints.crossAxisExtent * 1.0f / 4.0f,
                                    (arrowExtent - padding)),
                                constraints.normalizedGrowthDirection
                            );
                            _debugDrawArrow(
                                canvas,
                                paint,
                                offset.translate(constraints.crossAxisExtent * 3.0f / 4.0f, padding),
                                offset.translate(constraints.crossAxisExtent * 3.0f / 4.0f,
                                    (arrowExtent - padding)),
                                constraints.normalizedGrowthDirection
                            );
                            break;
                        case Axis.horizontal:
//            canvas.drawLine(
//              offset,
//              offset.translate(0.0f, constraints.crossAxisExtent),
//              paint,
//            );
                            _debugDrawArrow(
                                canvas,
                                paint,
                                offset.translate(padding, constraints.crossAxisExtent * 1.0f / 4.0f),
                                offset.translate((arrowExtent - padding),
                                    constraints.crossAxisExtent * 1.0f / 4.0f),
                                constraints.normalizedGrowthDirection
                            );
                            _debugDrawArrow(
                                canvas,
                                paint,
                                offset.translate(padding, constraints.crossAxisExtent * 3.0f / 4.0f),
                                offset.translate((arrowExtent - padding),
                                    constraints.crossAxisExtent * 3.0f / 4.0f),
                                constraints.normalizedGrowthDirection
                            );
                            break;
                    }
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverGeometry>("geometry", geometry));
        }
    }

    public static class RenderSliverHelpers {
        public static bool _getRightWayUp(SliverConstraints constraints) {
            D.assert(constraints != null);

            bool rightWayUp = true;

            switch (constraints.axisDirection) {
                case AxisDirection.up:
                case AxisDirection.left:
                    rightWayUp = false;
                    break;
                case AxisDirection.down:
                case AxisDirection.right:
                    rightWayUp = true;
                    break;
            }

            switch (constraints.growthDirection) {
                case GrowthDirection.forward:
                    break;
                case GrowthDirection.reverse:
                    rightWayUp = !rightWayUp;
                    break;
            }

            return rightWayUp;
        }

        public static bool hitTestBoxChild(this RenderSliver it, BoxHitTestResult result, RenderBox child,
            float mainAxisPosition = 0.0f, float crossAxisPosition = 0.0f) {
            bool rightWayUp = _getRightWayUp(it.constraints);
            float delta = it.childMainAxisPosition(child);
            float crossAxisDelta = it.childCrossAxisPosition(child);
            float absolutePosition = mainAxisPosition - delta;
            float absoluteCrossAxisPosition = crossAxisPosition - crossAxisDelta;
            Offset paintOffset = null;
            Offset transformedPosition = null;
            D.assert(it.constraints.axis != null);
            switch (it.constraints.axis) {
            case Axis.horizontal:
                if (!rightWayUp) {
                    absolutePosition = child.size.width - absolutePosition;
                    delta = it.geometry.paintExtent - child.size.width - delta;
                }
                paintOffset = new Offset(delta, crossAxisDelta);
                transformedPosition = new Offset(absolutePosition, absoluteCrossAxisPosition);
                break;
            case Axis.vertical:
                if (!rightWayUp) {
                absolutePosition = child.size.height - absolutePosition;
                delta = it.geometry.paintExtent - child.size.height - delta;
                }
                paintOffset = new Offset(crossAxisDelta, delta);
                transformedPosition = new Offset(absoluteCrossAxisPosition, absolutePosition);
                break;
            }
            D.assert(paintOffset != null);
            D.assert(transformedPosition != null);
            return result.addWithPaintOffset(
                offset: paintOffset,
                position: null, // Manually adapting from sliver to box position above.
                hitTest: (BoxHitTestResult boxHitTestResult, Offset _) => {
                    return child.hitTest(boxHitTestResult, position: transformedPosition);
                }
            );
        }

        public static void applyPaintTransformForBoxChild(this RenderSliver it, RenderBox child,
            Matrix4 transform) {
            bool rightWayUp = _getRightWayUp(it.constraints);
            float delta = it.childMainAxisPosition(child);
            float crossAxisDelta = it.childCrossAxisPosition(child);
            switch (it.constraints.axis) {
                case Axis.horizontal:
                    if (!rightWayUp) {
                        delta = it.geometry.paintExtent - child.size.width - delta;
                    }

                    transform.translate(delta, crossAxisDelta);
                    break;
                case Axis.vertical:
                    if (!rightWayUp) {
                        delta = it.geometry.paintExtent - child.size.height - delta;
                    }

                    transform.translate(crossAxisDelta, delta);
                    break;
            }
        }
    }

    public abstract class RenderSliverSingleBoxAdapter : RenderObjectWithChildMixinRenderSliver<RenderBox> {
        public RenderSliverSingleBoxAdapter(
            RenderBox child = null
        ) {
            this.child = child;
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverPhysicalParentData)) {
                child.parentData = new SliverPhysicalParentData();
            }
        }

        public void setChildParentData(RenderObject child, SliverConstraints constraints, SliverGeometry geometry) {
            var childParentData = (SliverPhysicalParentData) child.parentData;
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(constraints.axisDirection,
                constraints.growthDirection)) {
                case AxisDirection.up:
                    childParentData.paintOffset = new Offset(0.0f,
                        -(geometry.scrollExtent - (geometry.paintExtent + constraints.scrollOffset)));
                    break;
                case AxisDirection.right:
                    childParentData.paintOffset = new Offset(-constraints.scrollOffset, 0.0f);
                    break;
                case AxisDirection.down:
                    childParentData.paintOffset = new Offset(0.0f, -constraints.scrollOffset);
                    break;
                case AxisDirection.left:
                    childParentData.paintOffset =
                        new Offset(-(geometry.scrollExtent - (geometry.paintExtent + constraints.scrollOffset)),
                            0.0f);
                    break;
            }
        }

        protected override bool hitTestChildren(SliverHitTestResult result, float mainAxisPosition = 0.0f,
            float crossAxisPosition = 0.0f) {
            D.assert(geometry.hitTestExtent > 0.0f);
            if (child != null) {
                return this.hitTestBoxChild(new BoxHitTestResult(result), child, mainAxisPosition: mainAxisPosition,
                    crossAxisPosition: crossAxisPosition);
            }

            return false;
        }

        public override float childMainAxisPosition(RenderObject child) {
            return -constraints.scrollOffset;
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            D.assert(child != null);
            D.assert(child == this.child);

            var childParentData = (SliverPhysicalParentData) child.parentData;
            childParentData.applyPaintTransform(transform);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null && geometry.visible) {
                var childParentData = (SliverPhysicalParentData) child.parentData;
                context.paintChild(child, offset + childParentData.paintOffset);
            }
        }
    }

    public class RenderSliverToBoxAdapter : RenderSliverSingleBoxAdapter {
        public RenderSliverToBoxAdapter(
            RenderBox child = null
        ) : base(child) {
        }

        protected override void performLayout() {
            if (child == null) {
                geometry = SliverGeometry.zero;
                return;
            }

            child.layout(constraints.asBoxConstraints(), parentUsesSize: true);

            float childExtent = 0.0f;
            switch (constraints.axis) {
                case Axis.horizontal:
                    childExtent = child.size.width;
                    break;
                case Axis.vertical:
                    childExtent = child.size.height;
                    break;
            }

            float paintedChildSize = calculatePaintOffset(constraints, from: 0.0f, to: childExtent);
            float cacheExtent = calculateCacheOffset(constraints, from: 0.0f, to: childExtent);

            D.assert(paintedChildSize.isFinite());
            D.assert(paintedChildSize >= 0.0f);

            geometry = new SliverGeometry(
                scrollExtent: childExtent,
                paintExtent: paintedChildSize,
                cacheExtent: cacheExtent,
                maxPaintExtent: childExtent,
                hitTestExtent: paintedChildSize,
                hasVisualOverflow: childExtent > constraints.remainingPaintExtent
                                   || constraints.scrollOffset > 0.0f
            );

            setChildParentData(child, constraints, geometry);
        }
    }
}