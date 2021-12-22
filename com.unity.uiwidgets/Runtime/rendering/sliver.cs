using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public enum GrowthDirection {
        forward,
        reverse,
    }

    public static class GrowthDirectionUtils {
        public static AxisDirection? applyGrowthDirectionToAxisDirection(
            AxisDirection? axisDirection, GrowthDirection? growthDirection) {
            D.assert(axisDirection != null);
            D.assert(growthDirection != null);
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
            AxisDirection? axisDirection,
            GrowthDirection? growthDirection,
            ScrollDirection userScrollDirection,
            float scrollOffset,
            float precedingScrollExtent,
            float overlap,
            float remainingPaintExtent,
            float crossAxisExtent,
            AxisDirection? crossAxisDirection,
            float viewportMainAxisExtent,
            float remainingCacheExtent,
            float cacheOrigin
        ) {
            D.assert(axisDirection != null);
            D.assert(growthDirection != null);
            D.assert(crossAxisDirection != null);
            this.axisDirection = axisDirection.Value;
            this.growthDirection = growthDirection.Value;
            this.userScrollDirection = userScrollDirection;
            this.scrollOffset = scrollOffset;
            this.precedingScrollExtent = precedingScrollExtent;
            this.overlap = overlap;
            this.remainingPaintExtent = remainingPaintExtent;
            this.crossAxisExtent = crossAxisExtent;
            this.crossAxisDirection = crossAxisDirection.Value;
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

        public readonly AxisDirection? axisDirection;

        public readonly GrowthDirection? growthDirection;

        public readonly ScrollDirection userScrollDirection;

        public readonly float scrollOffset;
        
        public readonly float precedingScrollExtent;

        public readonly float overlap;

        public readonly float remainingPaintExtent;

        public readonly float crossAxisExtent;

        public readonly AxisDirection? crossAxisDirection;

        public readonly float viewportMainAxisExtent;

        public readonly float cacheOrigin;

        public readonly float remainingCacheExtent;

        public Axis? axis {
            get { return AxisUtils.axisDirectionToAxis(axisDirection); }
        }

        public GrowthDirection normalizedGrowthDirection {
            get {
                D.assert(axisDirection != null);
                D.assert(growthDirection != null);
                switch (axisDirection) {
                    case AxisDirection.down:
                    case AxisDirection.right:
                        return growthDirection.Value;
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
                bool hasErrors = false;
                StringBuilder errorMessage = new StringBuilder("\n");
                var verify = new Action<bool, string>((bool check, string message) => {
                    if (check) {
                        return;
                    }
                    hasErrors = true;
                    errorMessage.AppendLine($"  {message}");
                });
                void verifyFloat(float? property, string name, bool mustBePositive = false, bool mustBeNegative = false) {
                    verify(property != null, $"The \"{name}\" is null.");
                    if (property.Value.isNaN()) {
                        string additional = ".";
                        if (mustBePositive) {
                            additional = ", expected greater than or equal to zero.";
                        } else if (mustBeNegative) {
                            additional = ", expected less than or equal to zero.";
                        }
                        verify(false, $"The \"{name}\" is NaN" + $"{additional}");
                    } else if (mustBePositive) {
                        verify(property >= 0.0f, $"The \"{name}\" is negative.");
                    } else if (mustBeNegative) {
                        verify(property <= 0.0f, $"The \"{name}\" is positive.");
                    }
                }
                verify(axis != null, "The \"axis\" is null.");
                verify(growthDirection != null, "The \"growthDirection\" is null.");
                verifyFloat(scrollOffset, "scrollOffset");
                verifyFloat(overlap, "overlap");
                verifyFloat(crossAxisExtent, "crossAxisExtent");
                verifyFloat(scrollOffset, "scrollOffset", mustBePositive: true);
                verify(crossAxisDirection != null, "The \"crossAxisDirection\" is null.");
                verify(AxisUtils.axisDirectionToAxis(axisDirection) != AxisUtils.axisDirectionToAxis(crossAxisDirection), "The \"axisDirection\" and the \"crossAxisDirection\" are along the same axis.");
                verifyFloat(viewportMainAxisExtent, "viewportMainAxisExtent", mustBePositive: true);
                verifyFloat(remainingPaintExtent, "remainingPaintExtent", mustBePositive: true);
                verifyFloat(remainingCacheExtent, "remainingCacheExtent", mustBePositive: true);
                verifyFloat(cacheOrigin, "cacheOrigin", mustBeNegative: true);
                verifyFloat(precedingScrollExtent, "precedingScrollExtent", mustBePositive: true);
                verify(isNormalized, "The constraints are not normalized."); // should be redundant with earlier checks

                
                if (hasErrors) {
                    List<DiagnosticsNode> diagnosticInfo = new List<DiagnosticsNode>();
                    diagnosticInfo.Add(new ErrorSummary($"{GetType()} is not valid: {errorMessage}"));
                    if (informationCollector != null) {
                        diagnosticInfo.AddRange(informationCollector.Invoke());
                    }
                    diagnosticInfo.Add(new DiagnosticsProperty<SliverConstraints>("The offending constraints were", this, style: DiagnosticsTreeStyle.errorProperty));
                    
                    throw new UIWidgetsError(diagnosticInfo);
                }
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
            D.assert(other.debugAssertIsValid());
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
            List<string> properties = new List<string>();
            properties.Add($"{axisDirection}");
            properties.Add($"{growthDirection}");
            properties.Add($"{userScrollDirection}");
            properties.Add($"scrollOffset: {scrollOffset : F1}");
            properties.Add($"remainingPaintExtent: {remainingPaintExtent : F1}");
            if (overlap != 0.0)
                properties.Add($"overlap: {overlap: F1}");
            properties.Add($"crossAxisExtent: {crossAxisExtent : F1}");
            properties.Add($"crossAxisDirection: {crossAxisDirection}");
            properties.Add($"viewportMainAxisExtent: {viewportMainAxisExtent : F1}");
            properties.Add($"remainingCacheExtent: {remainingCacheExtent : F1}");
            properties.Add($"cacheOrigin: {cacheOrigin : F1}");
            return $"SliverConstraints({string.Join(", ",properties)})";
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

        internal static List<DiagnosticsNode> _debugCompareFloats(string labelA, float valueA, string labelB, float valueB) {
            List<DiagnosticsNode> diagnosticInfo = new List<DiagnosticsNode>();
            if (valueA.ToString("F1") != valueB.ToString("F1")) {
                diagnosticInfo.Add(new ErrorDescription($"The {labelA} is {valueA:F1}, but the {labelB} is {valueB:F1}. "));
            }
            else {
                diagnosticInfo.Add(new ErrorDescription($"The {labelA} is {valueA}, but the {labelB} is {valueB}."));
                diagnosticInfo.Add(new ErrorHint("Maybe you have fallen prey to floating point rounding errors, and should explicitly " +
                                                 $"apply the min() or max() functions, or the clamp() method, to the {labelB}? "));
            }

            return diagnosticInfo;
        }

        public bool debugAssertIsValid(InformationCollector informationCollector = null) {
            D.assert(() => {
                void verify(bool check, string summary, List<DiagnosticsNode> details = null) {
                    if (check) {
                        return;
                    }
                    
                    List<DiagnosticsNode> diagnosticInfo = new List<DiagnosticsNode>();
                    diagnosticInfo.Add(new ErrorSummary($"{GetType()} is not valid: {summary}"));
                    if (details != null) {
                        diagnosticInfo.AddRange(details);
                    }

                    if (informationCollector != null) {
                        diagnosticInfo.AddRange(informationCollector.Invoke());
                    }

                    throw new UIWidgetsError(diagnosticInfo);
                };

                verify(scrollExtent >= 0.0f, "The \"scrollExtent\" is negative.");
                verify(paintExtent >= 0.0f, "The \"paintExtent\" is negative.");
                verify(layoutExtent >= 0.0f, "The \"layoutExtent\" is negative.");
                verify(cacheExtent >= 0.0f, "The \"cacheExtent\" is negative.");
                if (layoutExtent > paintExtent) {
                    verify(false,
                        "The \"layoutExtent\" exceeds the \"paintExtent\".",
                        details: _debugCompareFloats("paintExtent", paintExtent, "layoutExtent",
                            layoutExtent)
                    );
                }

                if (paintExtent - maxPaintExtent > foundation_.precisionErrorTolerance) {
                    var details = _debugCompareFloats("maxPaintExtent", maxPaintExtent, "paintExtent",
                        paintExtent);
                    details.Add(new ErrorDescription("By definition, a sliver can\"t paint more than the maximum that it can paint!"));
                    verify(false,
                        "The \"maxPaintExtent\" is less than the \"paintExtent\".",
                        details:  details
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
            float mainAxisOffset,
            float crossAxisOffset,
            float mainAxisPosition,
            float crossAxisPosition,
            Offset paintOffset = null,
            SliverHitTest hitTest = null
        ) {
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
        public SliverHitTestEntry(
            RenderSliver target,
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
        public float? layoutOffset;

        public override string ToString() {
           return layoutOffset == null ? "layoutOffset = None" : $"layoutOffset = {layoutOffset:F1}";
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
                    DiagnosticsNode contract, violation, hint = null;
                    if (debugDoingThisLayout) {
                        D.assert(sizedByParent);
                        violation = new ErrorDescription("It appears that the geometry setter was called from performLayout().");
                    }
                    else {
                        violation = new ErrorDescription("The geometry setter was called from outside layout (neither performResize() nor performLayout() were being run for this object).");
                        if (owner != null && owner.debugDoingLayout) {
                            hint = new ErrorDescription("Only the object itself can set its geometry. It is a contract violation for other objects to set it.");
                        }
                    }

                    if (sizedByParent) {
                        contract = new ErrorDescription("Because this RenderSliver has sizedByParent set to true, it must set its geometry in performResize().");
                    }
                    else {
                        contract = new ErrorDescription(
                            "Because this RenderSliver has sizedByParent set to false, it must set its geometry in performLayout().");
                    }

                    List<DiagnosticsNode> information = new List<DiagnosticsNode>();
                    information.Add(new ErrorSummary("RenderSliver geometry setter called incorrectly."));
                    information.Add(violation);
                    if (hint != null) {
                        information.Add(hint);
                    }
                    information.Add(contract);
                    information.Add(describeForError("The RenderSliver in question is"));
                    throw new UIWidgetsError(information);
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
                return null;
            }
        }

        public override Rect semanticBounds {
            get { return paintBounds; }
        }

        protected override void debugResetSize() {
        }

        protected override void debugAssertDoesMeetConstraints() {
            D.assert(
                () => {
                    IEnumerable<DiagnosticsNode> infoCollector() {
                        yield return describeForError("The RenderSliver that returned the offending geometry was");
                    }
                    
                    return geometry.debugAssertIsValid(
                        informationCollector: infoCollector);
                });
            D.assert(() => {
                if (geometry.paintOrigin + geometry.paintExtent > constraints.remainingPaintExtent) {
                    List<DiagnosticsNode> diagnosticInfo = new List<DiagnosticsNode>();
                    diagnosticInfo.Add(new ErrorSummary("SliverGeometry has a paintOffset that exceeds the remainingPaintExtent from the constraints."));
                    diagnosticInfo.Add(describeForError("The render object whose geometry violates the constraints is the following:"));
                    diagnosticInfo.AddRange(SliverGeometry._debugCompareFloats("remainingPaintExtent", constraints.remainingPaintExtent,
                        "paintOrigin + paintExtent", geometry.paintOrigin + geometry.paintExtent));
                    diagnosticInfo.Add(new ErrorDescription("The paintExtent must cause the child sliver to paint within the viewport, and so " +
                                                            "cannot exceed the remainingPaintExtent."));
                    
                    throw new UIWidgetsError(diagnosticInfo);
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

        public virtual bool hitTest(SliverHitTestResult result, float mainAxisPosition = 0.0f, float crossAxisPosition = 0.0f) {
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

        protected virtual bool hitTestSelf(float mainAxisPosition = 0.0f, float crossAxisPosition = 0.0f) {
            return false;
        }

        protected virtual bool hitTestChildren(SliverHitTestResult result, float mainAxisPosition = 0.0f,
            float crossAxisPosition = 0.0f) {
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

        public virtual float? childMainAxisPosition(RenderObject child) {
            D.assert(() => { throw new UIWidgetsError(GetType() + " does not implement childPosition."); });

            return 0.0f;
        }

        public virtual float? childCrossAxisPosition(RenderObject child) {
            return 0.0f;
        }

        public virtual float? childScrollOffset(RenderObject child) {
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
            return null;
        }

        public Size getAbsoluteSize() {
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
                var path = new Path();
                path.moveTo(p0.dx,p0.dy);
                path.lineTo(p1.dx, p1.dy);
                path.moveTo(p1.dx - dx1, p1.dy - dy1);
                path.lineTo(p1.dx, p1.dy);
                path.lineTo(p1.dx - dx2, p1.dy - dy2);
                canvas.drawPath(
                    path,
                    paint
                );

                return true;
            });
        }

        public override void debugPaint(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (D.debugPaintSizeEnabled) {
                    float strokeWidth = Mathf.Min(4.0f, geometry.paintExtent / 30.0f);
                    Paint paint = new Paint();
                    paint.color = new Color(0xFF33CC33);
                    paint.strokeWidth = strokeWidth;
                    paint.style = PaintingStyle.stroke;
                    paint.maskFilter = MaskFilter.blur(BlurStyle.solid, strokeWidth);
                    float arrowExtent = geometry.paintExtent;
                    float padding = Mathf.Max(2.0f, strokeWidth);
                    Canvas canvas = context.canvas;
                    canvas.drawCircle(
                        offset.translate(padding, padding),
                        padding * 0.5f,
                        paint
                    );
                    switch (constraints.axis) {
                        case Axis.vertical:
                            canvas.drawLine(
                                offset,
                                offset.translate(constraints.crossAxisExtent, 0.0f),
                                paint
                            );
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
                            canvas.drawLine(
                                offset,
                                offset.translate(0.0f, constraints.crossAxisExtent),
                                paint
                            );
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

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) { }
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

        public static bool hitTestBoxChild(
            this RenderSliver it, 
            BoxHitTestResult result, 
            RenderBox child,
            float mainAxisPosition = 0.0f, 
            float crossAxisPosition = 0.0f) {
            bool rightWayUp = _getRightWayUp(it.constraints);
            float? delta = it.childMainAxisPosition(child);
            float? crossAxisDelta = it.childCrossAxisPosition(child);
            float? absolutePosition = mainAxisPosition - delta;
            float? absoluteCrossAxisPosition = crossAxisPosition - crossAxisDelta;
            Offset paintOffset = null;
            Offset transformedPosition = null;
            switch (it.constraints.axis) {
            case Axis.horizontal:
                if (!rightWayUp) {
                    absolutePosition = child.size.width - absolutePosition;
                    delta = it.geometry.paintExtent - child.size.width - delta;
                }
                paintOffset = new Offset(delta ?? 0.0f, crossAxisDelta ?? 0.0f);
                transformedPosition = new Offset(absolutePosition ?? 0.0f, absoluteCrossAxisPosition ?? 0.0f);
                break;
            case Axis.vertical:
                if (!rightWayUp) {
                    absolutePosition = child.size.height - absolutePosition;
                    delta = it.geometry.paintExtent - child.size.height - delta;
                }
                paintOffset = new Offset(crossAxisDelta ?? 0.0f, delta ?? 0.0f);
                transformedPosition = new Offset(absoluteCrossAxisPosition ?? 0.0f, absolutePosition ?? 0.0f);
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
            float? delta = it.childMainAxisPosition(child);
            float? crossAxisDelta = it.childCrossAxisPosition(child);
            switch (it.constraints.axis) {
                case Axis.horizontal:
                    if (!rightWayUp) {
                        delta = it.geometry.paintExtent - child.size.width - delta;
                    }

                    transform.translate(delta, crossAxisDelta ?? 0.0f);
                    break;
                case Axis.vertical:
                    if (!rightWayUp) {
                        delta = it.geometry.paintExtent - child.size.height - delta;
                    }

                    transform.translate(crossAxisDelta ?? 0.0f, delta ?? 0.0f);
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
            D.assert(childParentData.paintOffset != null);
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

        public override float? childMainAxisPosition(RenderObject child) {
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

            SliverConstraints constraints = this.constraints;
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