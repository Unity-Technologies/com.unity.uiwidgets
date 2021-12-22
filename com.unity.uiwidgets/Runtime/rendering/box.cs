using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    class _DebugSize : Size {
        internal _DebugSize(Size source, RenderBox _owner, bool _canBeUsedByParent) :
            base(source.width, source.height) {
            this._owner = _owner;
            this._canBeUsedByParent = _canBeUsedByParent;
        }

        internal readonly RenderBox _owner;
        internal readonly bool _canBeUsedByParent;
    }

    public class BoxConstraints : Constraints, IEquatable<BoxConstraints> {
        public BoxConstraints(
            float minWidth = 0.0f,
            float maxWidth = float.PositiveInfinity,
            float minHeight = 0.0f,
            float maxHeight = float.PositiveInfinity) {
            this.minWidth = minWidth;
            this.maxWidth = maxWidth;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
        }

        public readonly float minWidth;
        public readonly float maxWidth;
        public readonly float minHeight;
        public readonly float maxHeight;

        public static BoxConstraints tight(Size size) {
            return new BoxConstraints(
                size.width,
                size.width,
                size.height,
                size.height
            );
        }

        public static BoxConstraints tightFor(
            float? width = null,
            float? height = null
        ) {
            return new BoxConstraints(
                width ?? 0.0f,
                width ?? float.PositiveInfinity,
                height ?? 0.0f,
                height ?? float.PositiveInfinity
            );
        }

        public static BoxConstraints tightForFinite(
            float width = float.PositiveInfinity,
            float height = float.PositiveInfinity
        ) {
            return new BoxConstraints(
                !float.IsPositiveInfinity(width) ? width : 0.0f,
                !float.IsPositiveInfinity(width) ? width : float.PositiveInfinity,
                !float.IsPositiveInfinity(height) ? height : 0.0f,
                !float.IsPositiveInfinity(height) ? height : float.PositiveInfinity
            );
        }

        public static BoxConstraints loose(Size size) {
            return new BoxConstraints(
                minWidth: 0,
                maxWidth: size.width,
                minHeight: 0,
                maxHeight: size.height
            );
        }

        public static BoxConstraints expand(
            float? width = null,
            float? height = null
        ) {
            return new BoxConstraints(
                width ?? float.PositiveInfinity,
                width ?? float.PositiveInfinity,
                height ?? float.PositiveInfinity,
                height ?? float.PositiveInfinity
            );
        }

        public BoxConstraints copyWith(
            float? minWidth = null,
            float? maxWidth = null,
            float? minHeight = null,
            float? maxHeight = null
        ) {
            return new BoxConstraints(
                minWidth ?? this.minWidth,
                maxWidth ?? this.maxWidth,
                minHeight ?? this.minHeight,
                maxHeight ?? this.maxHeight
            );
        }

        public BoxConstraints deflate(EdgeInsets edges) {
            D.assert(edges != null);
            D.assert(debugAssertIsValid());
            float horizontal = edges.horizontal;
            float vertical = edges.vertical;
            float deflatedMinWidth = Mathf.Max(0.0f, minWidth - horizontal);
            float deflatedMinHeight = Mathf.Max(0.0f, minHeight - vertical);
            return new BoxConstraints(
                minWidth: deflatedMinWidth,
                maxWidth: Mathf.Max(deflatedMinWidth, maxWidth - horizontal),
                minHeight: deflatedMinHeight,
                maxHeight: Mathf.Max(deflatedMinHeight, maxHeight - vertical)
            );
        }

        public BoxConstraints loosen() {
            D.assert(debugAssertIsValid());
            return new BoxConstraints(
                minWidth: 0.0f,
                maxWidth: maxWidth,
                minHeight: 0.0f,
                maxHeight: maxHeight
            );
        }

        public BoxConstraints enforce(BoxConstraints constraints) {
            return new BoxConstraints(
                minWidth: minWidth.clamp(constraints.minWidth, constraints.maxWidth),
                maxWidth: maxWidth.clamp(constraints.minWidth, constraints.maxWidth),
                minHeight: minHeight.clamp(constraints.minHeight, constraints.maxHeight),
                maxHeight: maxHeight.clamp(constraints.minHeight, constraints.maxHeight)
            );
        }

        public BoxConstraints tighten(
            float? width = null,
            float? height = null
        ) {
            return new BoxConstraints(
                minWidth: width == null ? minWidth : width.Value.clamp(minWidth, maxWidth),
                maxWidth: width == null ? maxWidth : width.Value.clamp(minWidth, maxWidth),
                minHeight: height == null ? minHeight : height.Value.clamp(minHeight, maxHeight),
                maxHeight: height == null ? maxHeight : height.Value.clamp(minHeight, maxHeight)
            );
        }

        public BoxConstraints flipped {
            get {
                return new BoxConstraints(
                    minWidth: minHeight,
                    maxWidth: maxHeight,
                    minHeight: minWidth,
                    maxHeight: maxWidth
                );
            }
        }

        public BoxConstraints widthConstraints() {
            return new BoxConstraints(minWidth: minWidth, maxWidth: maxWidth);
        }

        public BoxConstraints heightConstraints() {
            return new BoxConstraints(minHeight: minHeight, maxHeight: maxHeight);
        }

        public float constrainWidth(float width = float.PositiveInfinity) {
            D.assert(debugAssertIsValid());
            return width.clamp(minWidth, maxWidth);
        }

        public float constrainHeight(float height = float.PositiveInfinity) {
            D.assert(debugAssertIsValid());
            return height.clamp(minHeight, maxHeight);
        }

        Size _debugPropagateDebugSize(Size size, Size result) {
            D.assert(() => {
                if (size is _DebugSize) {
                    result = new _DebugSize(result,
                        ((_DebugSize) size)._owner, ((_DebugSize) size)._canBeUsedByParent);
                }

                return true;
            });
            return result;
        }

        public Size constrain(Size size) {
            var result = new Size(constrainWidth(size.width), constrainHeight(size.height));
            D.assert(() => {
                result = _debugPropagateDebugSize(size, result);
                return true;
            });

            return result;
        }

        public Size constrainDimensions(float width, float height) {
            return new Size(constrainWidth(width), constrainHeight(height));
        }

        public Size constrainSizeAndAttemptToPreserveAspectRatio(Size size) {
            if (isTight) {
                Size result1 = smallest;
                D.assert(() => {
                    result1 = _debugPropagateDebugSize(size, result1);
                    return true;
                });
                return result1;
            }

            float width = size.width;
            float height = size.height;
            D.assert(width > 0.0);
            D.assert(height > 0.0);
            float aspectRatio = width / height;

            if (width > maxWidth) {
                width = maxWidth;
                height = width / aspectRatio;
            }

            if (height > maxHeight) {
                height = maxHeight;
                width = height * aspectRatio;
            }

            if (width < minWidth) {
                width = minWidth;
                height = width / aspectRatio;
            }

            if (height < minHeight) {
                height = minHeight;
                width = height * aspectRatio;
            }

            var result = new Size(constrainWidth(width), constrainHeight(height));
            D.assert(() => {
                result = _debugPropagateDebugSize(size, result);
                return true;
            });
            return result;
        }

        public Size biggest {
            get { return new Size(constrainWidth(), constrainHeight()); }
        }

        public Size smallest {
            get { return new Size(constrainWidth(0.0f), constrainHeight(0.0f)); }
        }

        public bool hasTightWidth {
            get { return minWidth >= maxWidth; }
        }

        public bool hasTightHeight {
            get { return minHeight >= maxHeight; }
        }

        public override bool isTight {
            get { return hasTightWidth && hasTightHeight; }
        }

        public bool hasBoundedWidth {
            get { return maxWidth < float.PositiveInfinity; }
        }

        public bool hasBoundedHeight {
            get { return maxHeight < float.PositiveInfinity; }
        }

        public bool hasInfiniteWidth {
            get { return minWidth >= float.PositiveInfinity; }
        }

        public bool hasInfiniteHeight {
            get { return minHeight >= float.PositiveInfinity; }
        }

        public bool isSatisfiedBy(Size size) {
            D.assert(debugAssertIsValid());
            return minWidth <= size.width && size.width <= maxWidth &&
                   minHeight <= size.height && size.height <= maxHeight;
        }

        public static BoxConstraints operator *(BoxConstraints it, float factor) {
            return new BoxConstraints(
                minWidth: it.minWidth * factor,
                maxWidth: it.maxWidth * factor,
                minHeight: it.minHeight * factor,
                maxHeight: it.maxHeight * factor
            );
        }

        public static BoxConstraints operator /(BoxConstraints it, float factor) {
            return new BoxConstraints(
                minWidth: it.minWidth / factor,
                maxWidth: it.maxWidth / factor,
                minHeight: it.minHeight / factor,
                maxHeight: it.maxHeight / factor
            );
        }

        public static BoxConstraints operator %(BoxConstraints it, float value) {
            return new BoxConstraints(
                minWidth: it.minWidth % value,
                maxWidth: it.maxWidth % value,
                minHeight: it.minHeight % value,
                maxHeight: it.maxHeight % value
            );
        }

        public static BoxConstraints lerp(BoxConstraints a, BoxConstraints b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b * t;
            }

            if (b == null) {
                return a * (1.0f - t);
            }

            D.assert(a.debugAssertIsValid());
            D.assert(b.debugAssertIsValid());
            D.assert(
                (a.minWidth.isFinite() && b.minWidth.isFinite()) ||
                (a.minWidth == float.PositiveInfinity && b.minWidth == float.PositiveInfinity),
                () => "Cannot interpolate between finite constraints and unbounded constraints.");
            D.assert(
                (a.maxWidth.isFinite() && b.maxWidth.isFinite()) ||
                (a.maxWidth == float.PositiveInfinity && b.maxWidth == float.PositiveInfinity),
                () => "Cannot interpolate between finite constraints and unbounded constraints.");
            D.assert(
                (a.minHeight.isFinite() && b.minHeight.isFinite()) ||
                (a.minHeight == float.PositiveInfinity && b.minHeight == float.PositiveInfinity),
                () => "Cannot interpolate between finite constraints and unbounded constraints.");
            D.assert(
                (a.maxHeight.isFinite() && b.maxHeight.isFinite()) ||
                (a.maxHeight == float.PositiveInfinity && b.maxHeight == float.PositiveInfinity),
                () => "Cannot interpolate between finite constraints and unbounded constraints.");
            return new BoxConstraints(
                minWidth: a.minWidth.isFinite()
                    ? MathUtils.lerpNullableFloat(a.minWidth, b.minWidth, t)
                    : float.PositiveInfinity,
                maxWidth: a.maxWidth.isFinite()
                    ? MathUtils.lerpNullableFloat(a.maxWidth, b.maxWidth, t)
                    : float.PositiveInfinity,
                minHeight: a.minHeight.isFinite()
                    ? MathUtils.lerpNullableFloat(a.minHeight, b.minHeight, t)
                    : float.PositiveInfinity,
                maxHeight: a.maxHeight.isFinite()
                    ? MathUtils.lerpNullableFloat(a.maxHeight, b.maxHeight, t)
                    : float.PositiveInfinity
            );
        }

        public override bool isNormalized {
            get {
                return minWidth >= 0.0 &&
                       minWidth <= maxWidth &&
                       minHeight >= 0.0 &&
                       minHeight <= maxHeight;
            }
        }

        public override bool debugAssertIsValid(
            bool isAppliedConstraint = false,
            InformationCollector informationCollector = null
        ) {
            D.assert(() => {
                var throwError = new Action<DiagnosticsNode>(message => {
                    var diagnositicsInfo = new List<DiagnosticsNode>();
                    diagnositicsInfo.Add(message);
                    if (informationCollector != null) {
                        diagnositicsInfo.AddRange(informationCollector.Invoke());
                    }
                    diagnositicsInfo.Add(new DiagnosticsProperty<BoxConstraints>("The offending constraints were", this, style: DiagnosticsTreeStyle.errorProperty));
                    throw new UIWidgetsError(
                        diagnositicsInfo
                    );
                });

                if (minWidth.isNaN() ||
                    maxWidth.isNaN() ||
                    minHeight.isNaN() ||
                    maxHeight.isNaN()) {
                    var affectedFieldsList = new List<string>();
                    if (minWidth.isNaN()) {
                        affectedFieldsList.Add("minWidth");
                    }

                    if (maxWidth.isNaN()) {
                        affectedFieldsList.Add("maxWidth");
                    }

                    if (minHeight.isNaN()) {
                        affectedFieldsList.Add("minHeight");
                    }

                    if (maxHeight.isNaN()) {
                        affectedFieldsList.Add("maxHeight");
                    }

                    D.assert(affectedFieldsList.isNotEmpty());
                    if (affectedFieldsList.Count > 1) {
                        var last = affectedFieldsList.Last();
                        affectedFieldsList.RemoveAt(affectedFieldsList.Count - 1);
                        affectedFieldsList.Add("and " + last);
                    }

                    string whichFields;
                    if (affectedFieldsList.Count > 2) {
                        whichFields = string.Join(", ", affectedFieldsList.ToArray());
                    }
                    else if (affectedFieldsList.Count == 2) {
                        whichFields = string.Join(" ", affectedFieldsList.ToArray());
                    }
                    else {
                        whichFields = affectedFieldsList.Single();
                    }

                    throwError(new ErrorSummary("BoxConstraints has NaN values in " + whichFields + "."));
                }

                if (minWidth < 0.0 && minHeight < 0.0) {
                    throwError(new ErrorSummary("BoxConstraints has both a negative minimum width and a negative minimum height."));
                }

                if (minWidth < 0.0) {
                    throwError(new ErrorSummary("BoxConstraints has a negative minimum width."));
                }

                if (minHeight < 0.0) {
                    throwError(new ErrorSummary("BoxConstraints has a negative minimum height."));
                }

                if (maxWidth < minWidth && maxHeight < minHeight) {
                    throwError(new ErrorSummary("BoxConstraints has both width and height constraints non-normalized."));
                }

                if (maxWidth < minWidth) {
                    throwError(new ErrorSummary("BoxConstraints has non-normalized width constraints."));
                }

                if (maxHeight < minHeight) {
                    throwError(new ErrorSummary("BoxConstraints has non-normalized height constraints."));
                }

                if (isAppliedConstraint) {
                    if (minWidth.isInfinite() && minHeight.isInfinite()) {
                        throwError(new ErrorSummary("BoxConstraints forces an infinite width and infinite height."));
                    }

                    if (minWidth.isInfinite()) {
                        throwError(new ErrorSummary("BoxConstraints forces an infinite width."));
                    }

                    if (minHeight.isInfinite()) {
                        throwError(new ErrorSummary("BoxConstraints forces an infinite height."));
                    }
                }

                D.assert(isNormalized);
                return true;
            });
            return isNormalized;
        }

        public BoxConstraints normalize() {
            if (isNormalized) {
                return this;
            }

            var minWidth = this.minWidth >= 0.0 ? this.minWidth : 0.0f;
            var minHeight = this.minHeight >= 0.0 ? this.minHeight : 0.0f;

            return new BoxConstraints(
                minWidth,
                minWidth > maxWidth ? minWidth : maxWidth,
                minHeight,
                minHeight > maxHeight ? minHeight : maxHeight
            );
        }

        public bool Equals(BoxConstraints other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return minWidth.Equals(other.minWidth)
                   && maxWidth.Equals(other.maxWidth)
                   && minHeight.Equals(other.minHeight)
                   && maxHeight.Equals(other.maxHeight);
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

            return Equals((BoxConstraints) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = minWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ maxWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ minHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ maxHeight.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BoxConstraints left, BoxConstraints right) {
            return Equals(left, right);
        }

        public static bool operator !=(BoxConstraints left, BoxConstraints right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            string annotation = isNormalized ? "" : "; NOT NORMALIZED";
            if (minWidth == float.PositiveInfinity &&
                minHeight == float.PositiveInfinity) {
                return "BoxConstraints(biggest" + annotation + ")";
            }

            if (minWidth == 0 && maxWidth == float.PositiveInfinity &&
                minHeight == 0 && maxHeight == float.PositiveInfinity) {
                return "BoxConstraints(unconstrained" + annotation + ")";
            }

            var describe = new Func<float, float, string, string>((min, max, dim) => {
                if (min == max) {
                    return dim + "=" + min.ToString("F1");
                }

                return min.ToString("F1") + "<=" + dim + "<=" + max.ToString("F1");
            });

            string width = describe(minWidth, maxWidth, "w");
            string height = describe(minHeight, maxHeight, "h");
            return "BoxConstraints(" + width + ", " + height + annotation + ")";
        }
    }

    public class BoxHitTestEntry : HitTestEntry {
        public BoxHitTestEntry(RenderBox target, Offset localPosition)
            : base(target) {
            D.assert(localPosition != null);
            this.localPosition = localPosition;
        }

        public new RenderBox target {
            get { return (RenderBox) base.target; }
        }

        public readonly Offset localPosition;

        public override string ToString() {
            return $"{foundation_.describeIdentity(target)}@{localPosition}";
        }
    }

    public delegate bool BoxHitTest(BoxHitTestResult result, Offset position);

    public class BoxHitTestResult : HitTestResult {
        public BoxHitTestResult() : base() {
        }

        public BoxHitTestResult(HitTestResult result) : base(result) {
        }

        public bool addWithPaintTransform(
            Matrix4 transform,
            Offset position,
            BoxHitTest hitTest
        ) {
            D.assert(hitTest != null);
            if (transform != null) {
                transform = Matrix4.tryInvert(PointerEvent.removePerspectiveTransform(transform));
                if (transform == null) {
                    // Objects are not visible on screen and cannot be hit-tested.
                    return false;
                }
            }

            return addWithRawTransform(
                transform: transform,
                position: position,
                hitTest: hitTest
            );
        }

        public bool addWithPaintOffset(
            Offset offset,
            Offset position,
            BoxHitTest hitTest
        ) {
            D.assert(hitTest != null);
            return addWithRawTransform(
                transform: offset != null ? Matrix4.translationValues(-offset.dx, -offset.dy, 0) : null,
                position: position,
                hitTest: hitTest
            );
        }

        public bool addWithRawTransform(
            Matrix4 transform = null,
            Offset position = null,
            BoxHitTest hitTest = null
        ) {
            D.assert(hitTest != null);
            Offset transformedPosition = position == null || transform == null
                ? position
                : MatrixUtils.transformPoint(transform, position);
            if (transform != null) {
                pushTransform(transform);
            }

            bool isHit = hitTest(this, transformedPosition);
            if (transform != null) {
                popTransform();
            }

            return isHit;
        }
    }

    public class BoxParentData : ParentData {
        public Offset offset {
            get { return _offset; }
            set { _offset = value; }
        }

        Offset _offset = Offset.zero;

        public override string ToString() {
            return "offset=" + offset;
        }
    }

    enum _IntrinsicDimension {
        minWidth,
        maxWidth,
        minHeight,
        maxHeight
    }

    class _IntrinsicDimensionsCacheEntry : IEquatable<_IntrinsicDimensionsCacheEntry> {
        internal _IntrinsicDimensionsCacheEntry(_IntrinsicDimension dimension, float argument) {
            this.dimension = dimension;
            this.argument = argument;
        }

        public readonly _IntrinsicDimension dimension;
        public readonly float argument;

        public bool Equals(_IntrinsicDimensionsCacheEntry other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return dimension == other.dimension && argument.Equals(other.argument);
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

            return Equals((_IntrinsicDimensionsCacheEntry) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) dimension * 397) ^ argument.GetHashCode();
            }
        }

        public static bool operator ==(_IntrinsicDimensionsCacheEntry a, _IntrinsicDimensionsCacheEntry b) {
            return Equals(a, b);
        }

        public static bool operator !=(_IntrinsicDimensionsCacheEntry a, _IntrinsicDimensionsCacheEntry b) {
            return !Equals(a, b);
        }
    }

    public abstract class RenderBox : RenderObject {
        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is BoxParentData)) {
                child.parentData = new BoxParentData();
            }
        }

        Dictionary<_IntrinsicDimensionsCacheEntry, float> _cachedIntrinsicDimensions;

        float _computeIntrinsicDimension(_IntrinsicDimension dimension, float argument,
            Func<float, float> computer) {
            D.assert(debugCheckingIntrinsics || !debugDoingThisResize);
            bool shouldCache = true;
            D.assert(() => {
                if (debugCheckingIntrinsics) {
                    shouldCache = false;
                }

                return true;
            });

            if (shouldCache) {
                _cachedIntrinsicDimensions =
                    _cachedIntrinsicDimensions
                    ?? new Dictionary<_IntrinsicDimensionsCacheEntry, float>();
                return _cachedIntrinsicDimensions.putIfAbsent(
                    new _IntrinsicDimensionsCacheEntry(dimension, argument),
                    () => computer(argument));
            }

            return computer(argument);
        }

        public float getMinIntrinsicWidth(float height) {
            D.assert(() => {
                if (height < 0.0) {
                    throw new UIWidgetsError(new List<DiagnosticsNode> {
                            new ErrorSummary("The height argument to getMinIntrinsicWidth was negative."),
                            new ErrorDescription("The argument to getMinIntrinsicWidth must not be negative or null."),
                            new ErrorHint(
                                "If you perform computations on another height before passing it to " +
                                "getMinIntrinsicWidth, consider using math.max() or double.clamp() " +
                                "to force the value into the valid range."
                            )
                        }
                    );
                }

                return true;
            });

            return _computeIntrinsicDimension(_IntrinsicDimension.minWidth, height, computeMinIntrinsicWidth);
        }

        protected internal virtual float computeMinIntrinsicWidth(float height) {
            return 0.0f;
        }

        public float getMaxIntrinsicWidth(float height) {
            D.assert(() => {
                if (height < 0.0) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary("The height argument to getMaxIntrinsicWidth was negative."),
                        new ErrorDescription("The argument to getMaxIntrinsicWidth must not be negative or null."),
                        new ErrorHint(
                            "If you perform computations on another height before passing it to " + 
                        "getMaxIntrinsicWidth, consider using math.max() or double.clamp() " +
                        "to force the value into the valid range."
                        )
                    });
                }
                return true;
            });

            return _computeIntrinsicDimension(_IntrinsicDimension.maxWidth, height, computeMaxIntrinsicWidth);
        }

        protected internal virtual float computeMaxIntrinsicWidth(float height) {
            return 0.0f;
        }

        public float getMinIntrinsicHeight(float width) {
            D.assert(() => {
                if (width < 0.0) {
                    throw new UIWidgetsError( new List<DiagnosticsNode>(){
                        new ErrorSummary("The width argument to getMinIntrinsicHeight was negative."),
                        new ErrorDescription("The argument to getMinIntrinsicHeight must not be negative or null."),
                        new ErrorHint(
                            "If you perform computations on another width before passing it to " +
                            "getMinIntrinsicHeight, consider using math.max() or double.clamp() " +
                            "to force the value into the valid range."
                        )
                    });
                }

                return true;
            });

            return _computeIntrinsicDimension(_IntrinsicDimension.minHeight, width,
                computeMinIntrinsicHeight);
        }

        protected internal virtual float computeMinIntrinsicHeight(float width) {
            return 0.0f;
        }

        public float getMaxIntrinsicHeight(float width) {
            D.assert(() => {
                if (width < 0.0) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary("The width argument to getMaxIntrinsicHeight was negative."),
                        new ErrorDescription("The argument to getMaxIntrinsicHeight must not be negative or null."),
                        new ErrorHint(
                            "If you perform computations on another width before passing it to " +
                            "getMaxIntrinsicHeight, consider using math.max() or double.clamp() " +
                            "to force the value into the valid range."
                        )
                    });
                }

                return true;
            });

            return _computeIntrinsicDimension(_IntrinsicDimension.maxHeight, width,
                computeMaxIntrinsicHeight);
        }

        protected internal virtual float computeMaxIntrinsicHeight(float width) {
            return 0.0f;
        }

        public bool hasSize {
            get { return _size != null; }
        }

        public Size size {
            get {
                D.assert(hasSize, () => "RenderBox was not laid out: " + this);
                D.assert(() => {
                    Size _size = this._size;
                    if (_size is _DebugSize) {
                        D.assert(((_DebugSize)_size)._owner == this);
                        if (debugActiveLayout != null) {
                            D.assert(debugDoingThisResize || debugDoingThisLayout ||
                                     (debugActiveLayout == parent && ((_DebugSize)_size)._canBeUsedByParent));
                        }

                        D.assert(_size == this._size);
                    }

                    return true;
                });

                return this._size;
            }
            set {
                D.assert(!(debugDoingThisResize && debugDoingThisLayout));
                D.assert(sizedByParent || !debugDoingThisResize);
                D.assert(() => {
                    if ((sizedByParent && debugDoingThisResize) ||
                        (!sizedByParent && debugDoingThisLayout)) {
                        return true;
                    }

                    D.assert(!debugDoingThisResize);
                    List<DiagnosticsNode> information = new List<DiagnosticsNode>{
                        new ErrorSummary("RenderBox size setter called incorrectly.")
                    };
                    if (debugDoingThisLayout) {
                        D.assert(sizedByParent);
                        information.Add(new ErrorDescription("It appears that the size setter was called from performLayout()."));
                    }
                    else {
                        information.Add(new ErrorDescription(
                            "The size setter was called from outside layout (neither performResize() nor performLayout() were being run for this object)."
                        ));
                        if (owner != null && owner.debugDoingLayout) {
                            information.Add(new ErrorDescription("Only the object itself can set its size. It is a contract violation for other objects to set it."));
                        }
                    }

                    if (sizedByParent) {
                        information.Add(new ErrorDescription("Because this RenderBox has sizedByParent set to true, it must set its size in performResize()."));
                    }
                    else {
                        information.Add(new ErrorDescription("Because this RenderBox has sizedByParent set to false, it must set its size in performLayout()."));
                    }
                    throw new UIWidgetsError(information);
                });
                D.assert(() => {
                    value = debugAdoptSize(value);
                    return true;
                });

                _size = value;

                D.assert(() => {
                    debugAssertDoesMeetConstraints();
                    return true;
                });
            }
        }

        Size _size;

        public Size debugAdoptSize(Size valueRaw) {
            Size result = valueRaw;
            D.assert(() => {
                if (valueRaw is _DebugSize) {
                    var value = (_DebugSize) valueRaw;
                    if (value._owner != this) {
                        if (value._owner.parent != this) {
                            throw new UIWidgetsError(new List<DiagnosticsNode>{
                                new ErrorSummary("The size property was assigned a size inappropriately."),
                                describeForError("The following render object"),
                                value._owner.describeForError("...was assigned a size obtained from"),
                                new ErrorDescription(
                                    "However, this second render object is not, or is no longer, a " +
                                    "child of the first, and it is therefore a violation of the " +
                                    "RenderBox layout protocol to use that size in the layout of the " +
                                    "first render object."
                                ),
                                new ErrorHint(
                                    "If the size was obtained at a time where it was valid to read " +
                                    "the size (because the second render object above was a child " +
                                    "of the first at the time), then it should be adopted using " +
                                    "debugAdoptSize at that time."
                                ),
                                new ErrorHint(
                                    "If the size comes from a grandchild or a render object from an " +
                                    "entirely different part of the render tree, then there is no " +
                                    "way to be notified when the size changes and therefore attempts " +
                                    "to read that size are almost certainly a source of bugs. A different " +
                                    "approach should be used."
                                )
                            });
                        }

                        if (!value._canBeUsedByParent) {
                            throw new UIWidgetsError(new List<DiagnosticsNode>{
                                new ErrorSummary("A child's size was used without setting parentUsesSize."),
                                describeForError("The following render object"),
                                value._owner.describeForError("...was assigned a size obtained from its child"),
                                new ErrorDescription(
                                    "However, when the child was laid out, the parentUsesSize argument " +
                                    "was not set or set to false. Subsequently this transpired to be " +
                                    "inaccurate: the size was nonetheless used by the parent.\n" +
                                    "It is important to tell the framework if the size will be used or not " +
                                    "as several important performance optimizations can be made if the " +
                                    "size will not be used by the parent."
                                )
                            });
                        }
                    }
                }

                result = new _DebugSize(valueRaw, this, debugCanParentUseSize);
                return true;
            });
            return result;
        }

        public override Rect semanticBounds {
            get { return Offset.zero & size; }
        }

        protected override void debugResetSize() {
            size = size;
        }

        Dictionary<TextBaseline, float?> _cachedBaselines;
        static bool _debugDoingBaseline = false;

        static bool _debugSetDoingBaseline(bool value) {
            _debugDoingBaseline = value;
            return true;
        }

        public float? getDistanceToBaseline(TextBaseline? baseline, bool onlyReal = false) {
            D.assert(!_debugDoingBaseline,
                () =>
                    "Please see the documentation for computeDistanceToActualBaseline for the required calling conventions of this method.");
            D.assert(!debugNeedsLayout);
            D.assert(() => {
                RenderObject parent = (RenderObject) this.parent;
                if (owner.debugDoingLayout) {
                    return (debugActiveLayout == parent) && parent.debugDoingThisLayout;
                }

                if (owner.debugDoingPaint) {
                    return ((debugActivePaint == parent) && parent.debugDoingThisPaint) ||
                           ((debugActivePaint == this) && debugDoingThisPaint);
                }

                D.assert(parent == this.parent);
                return false;
            });

            D.assert(_debugSetDoingBaseline(true));
            float? result = getDistanceToActualBaseline(baseline);
            D.assert(_debugSetDoingBaseline(false));

            if (result == null && !onlyReal) {
                return size.height;
            }

            return result;
        }

        public float? getDistanceToActualBaseline(TextBaseline? baseline) {
            //If there is no baseline, return null by default
            if (baseline == null) {
                return null;
            }
            
            D.assert(_debugDoingBaseline,
                () =>
                    "Please see the documentation for computeDistanceToActualBaseline for the required calling conventions of this method.");
            
            _cachedBaselines = _cachedBaselines ?? new Dictionary<TextBaseline, float?>();
            return _cachedBaselines.putIfAbsent(baseline.Value, () => computeDistanceToActualBaseline(baseline.Value));
        }

        public virtual float? computeDistanceToActualBaseline(TextBaseline baseline) {
            D.assert(_debugDoingBaseline,
                () =>
                    "Please see the documentation for computeDistanceToActualBaseline for the required calling conventions of this method.");

            return null;
        }

        public new BoxConstraints constraints {
            get { return (BoxConstraints) base.constraints; }
        }

        protected override void debugAssertDoesMeetConstraints() {
            D.assert(constraints != null);
            D.assert(() => {
                if (!hasSize) {
                    D.assert(!debugNeedsLayout);
                    DiagnosticsNode contract;
                    if (sizedByParent) {
                        contract = new ErrorDescription("Because this RenderBox has sizedByParent set to true, it must set its size in performResize().");
                    }
                    else {
                        contract = new ErrorDescription("Because this RenderBox has sizedByParent set to false, it must set its size in performLayout().");
                    }

                    throw new UIWidgetsError(new List<DiagnosticsNode> {
                        new ErrorSummary("RenderBox did not set its size during layout."),
                        contract,
                        new ErrorDescription(
                            "It appears that this did not happen; layout completed, but the size property is still null."),
                        new DiagnosticsProperty<RenderBox>("The RenderBox in question is", this,
                            style: DiagnosticsTreeStyle.errorProperty),
                    });
                }

                if (!_size.isFinite) {
                    List<DiagnosticsNode> information = new List<DiagnosticsNode>{
                        new ErrorSummary("${runtimeType} object was given an infinite size during layout."),
                        new ErrorDescription(
                            "This probably means that it is a render object that tries to be " +
                            "as big as possible, but it was put inside another render object " +
                            "that allows its children to pick their own size."
                        )
                    };
                    if (!constraints.hasBoundedWidth) {
                        RenderBox node = this;
                        while (!node.constraints.hasBoundedWidth && node.parent is RenderBox) {
                            node = (RenderBox) node.parent;
                            information.Add(node.describeForError("The nearest ancestor providing an unbounded width constraint is"));
                        }
                    }

                    if (!constraints.hasBoundedHeight) {
                        RenderBox node = this;
                        while (!node.constraints.hasBoundedHeight && node.parent is RenderBox) {
                            node = (RenderBox) node.parent;
                            information.Add(node.describeForError("The nearest ancestor providing an unbounded height constraint is"));
                        }
                    }

                    information.Add(new DiagnosticsProperty<BoxConstraints>("The constraints that applied to the ${runtimeType} were", constraints, style: DiagnosticsTreeStyle.errorProperty));
                    information.Add(new DiagnosticsProperty<Size>("The exact size it was given was", _size, style: DiagnosticsTreeStyle.errorProperty));
                    throw new UIWidgetsError(information);
                }

                if (!constraints.isSatisfiedBy(_size)) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary("${runtimeType} does not meet its constraints."),
                        new DiagnosticsProperty<BoxConstraints>("Constraints", constraints, style: DiagnosticsTreeStyle.errorProperty),
                        new DiagnosticsProperty<Size>("Size", _size, style: DiagnosticsTreeStyle.errorProperty),
                        new ErrorHint(
                            "If you are not writing your own RenderBox subclass, then this is not " +
                            "your fault."
                        )
                    });
                }

                if (D.debugCheckIntrinsicSizes) {
                    D.assert(!debugCheckingIntrinsics);
                    debugCheckingIntrinsics = true;
                    List<DiagnosticsNode> failures = new List<DiagnosticsNode>();

                    var testIntrinsic = new Func<Func<float, float>, string, float, float>(
                        (function, name, constraint) => {
                            float result = function(constraint);
                            if (result < 0) {
                                failures.Add(new ErrorDescription($" * {name}({constraint}) returned a negative value: {result}"));
                            }

                            if (result.isInfinite()) {
                                failures.Add(new ErrorDescription($" * {name}({constraint}) returned a non-finite value: {result}"));
                            }

                            return result;
                        });

                    var testIntrinsicsForValues =
                        new Action<Func<float, float>, Func<float, float>, string, float>(
                            (getMin, getMax, name, constraint) => {
                                float min = testIntrinsic(getMin, "getMinIntrinsic" + name, constraint);
                                float max = testIntrinsic(getMax, "getMaxIntrinsic" + name, constraint);
                                if (min > max) {
                                    failures.Add(new ErrorDescription($" * getMinIntrinsic{name}({constraint}) returned a larger value ({min}) than getMaxIntrinsic{name}({constraint}) ({max})"));
                                }
                            });

                    testIntrinsicsForValues(getMinIntrinsicWidth, getMaxIntrinsicWidth, "Width",
                        float.PositiveInfinity);
                    testIntrinsicsForValues(getMinIntrinsicHeight, getMaxIntrinsicHeight, "Height",
                        float.PositiveInfinity);

                    if (constraints.hasBoundedWidth) {
                        testIntrinsicsForValues(getMinIntrinsicWidth, getMaxIntrinsicWidth, "Width",
                            constraints.maxHeight);
                    }

                    if (constraints.hasBoundedHeight) {
                        testIntrinsicsForValues(getMinIntrinsicHeight, getMaxIntrinsicHeight, "Height",
                            constraints.maxWidth);
                    }

                    debugCheckingIntrinsics = false;
                    if (failures.Count > 0) {
                        
                        failures.Add(new ErrorSummary("The intrinsic dimension methods of the $runtimeType class returned values that violate the intrinsic protocol contract."));
                        failures.Add(new ErrorDescription("The following failure(s) was detected:"));
                        failures.Add(new ErrorHint(
                            "If you are not writing your own RenderBox subclass, then this is not\n" +
                            "your fault. Contact support: https://github.com/flutter/flutter/issues/new?template=BUG.md"
                        ));
                        throw new UIWidgetsError(failures);
                    }
                }

                return true;
            });
        }

        public override void markNeedsLayout() {
            if (_cachedBaselines != null && _cachedBaselines.isNotEmpty() ||
                _cachedIntrinsicDimensions != null && _cachedIntrinsicDimensions.isNotEmpty()) {
                if (_cachedBaselines != null) {
                    _cachedBaselines.Clear();
                }

                if (_cachedIntrinsicDimensions != null) {
                    _cachedIntrinsicDimensions.Clear();
                }

                if (parent is RenderObject) {
                    markParentNeedsLayout();
                    return;
                }
            }

            base.markNeedsLayout();
        }


        protected override void performResize() {
            size = constraints.smallest;
            D.assert(size.isFinite);
        }

        protected override void performLayout() {
            D.assert(() => {
                if (!sizedByParent) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary("$runtimeType did not implement performLayout()."),
                        new ErrorHint(
                            "RenderBox subclasses need to either override performLayout() to " +
                            "set a size and lay out any children, or, set sizedByParent to true " +
                            "so that performResize() sizes the render object."
                        )
                    });
                }

                return true;
            });
        }

        public virtual bool hitTest(BoxHitTestResult result, Offset position = null) {
            D.assert(position != null);
            D.assert(() => {
                if (!hasSize) {
                    if (debugNeedsLayout) {
                        throw new UIWidgetsError(new List<DiagnosticsNode>{
                            new ErrorSummary("Cannot hit test a render box that has never been laid out."),
                            describeForError("The hitTest() method was called on this RenderBox"),
                            new ErrorDescription(
                                "Unfortunately, this object's geometry is not known at this time, " +
                                "probably because it has never been laid out. " +
                                "This means it cannot be accurately hit-tested."
                            ),
                            new ErrorHint(
                                "If you are trying " +
                                "to perform a hit test during the layout phase itself, make sure " +
                                "you only hit test nodes that have completed layout (e.g. the node's " +
                                "children, after their layout() method has been called)."
                            )
                        });
                    }

                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary("Cannot hit test a render box with no size."),
                        describeForError("The hitTest() method was called on this RenderBox"),
                        new ErrorDescription(
                            "Although this node is not marked as needing layout, " +
                            "its size is not set."
                        ),
                        new ErrorHint(
                            "A RenderBox object must have an " +
                            "explicit size before it can be hit-tested. Make sure " +
                            "that the RenderBox in question sets its size during layout."
                        )
                    });
                }

                return true;
            });

            if (_size.contains(position)) {
                if (hitTestChildren(result, position: position) || hitTestSelf(position)) {
                    result.add(new BoxHitTestEntry(this, position));
                    return true;
                }
            }

            return false;
        }

        protected virtual bool hitTestSelf(Offset position) {
            return false;
        }

        protected virtual bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            return false;
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            D.assert(child != null);
            D.assert(child.parent == this);
            D.assert(() => {
                if (!(child.parentData is BoxParentData)) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary($"{GetType()} does not implement applyPaintTransform."),
                        describeForError($"The following {GetType()} object"),
                        child.describeForError("...did not use a BoxParentData class for the parentData field of the following child"),
                        new ErrorDescription($"The {GetType()} class inherits from RenderBox."),
                        new ErrorHint(
                            "The default applyPaintTransform implementation provided by RenderBox assumes that the " +
                            "children all use BoxParentData objects for their parentData field. " +
                            $"Since {GetType()} does not in fact use that ParentData class for its children, it must " +
                            "provide an implementation of applyPaintTransform that supports the specific ParentData " +
                            $"subclass used by its children (which apparently is {child.parentData.GetType()})."
                        ),
                    });
                }

                return true;
            });

            var childParentData = (BoxParentData) child.parentData;
            var offset = childParentData.offset;
            transform.translate(offset.dx, offset.dy);
        }

        public Offset globalToLocal(Offset point, RenderObject ancestor = null) {
            var transform = getTransformTo(ancestor);
            float det = transform.invert();
            if (det == 0) {
                return Offset.zero;
            }

            Vector3 n = new Vector3(0, 0, 1);
            Vector3 i = transform.perspectiveTransform(new Vector3(0, 0, 0));
            Vector3 d = transform.perspectiveTransform(new Vector3(0, 0, 1)) - i;
            Vector3 s = transform.perspectiveTransform(new Vector3(point.dx, point.dy, 0));
            Vector3 p = s - d * (n.dot(s) / n.dot(d));
            return new Offset(p.x, p.y);
        }

        public Offset localToGlobal(Offset point, RenderObject ancestor = null) {
            return MatrixUtils.transformPoint(getTransformTo(ancestor), point);
        }

        public override Rect paintBounds {
            get { return Offset.zero & size; }
        }

        int _debugActivePointers = 0;

        protected bool debugHandleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(() => {
                if (D.debugPaintPointersEnabled) {
                    if (evt is PointerDownEvent) {
                        _debugActivePointers += 1;
                    }
                    else if (evt is PointerUpEvent || evt is PointerCancelEvent) {
                        _debugActivePointers -= 1;
                    }

                    markNeedsPaint();
                }

                return true;
            });
            return true;
        }

        public override void debugPaint(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (D.debugPaintSizeEnabled) {
                    debugPaintSize(context, offset);
                }

                if (D.debugPaintBaselinesEnabled) {
                    debugPaintBaselines(context, offset);
                }

                if (D.debugPaintPointersEnabled) {
                    debugPaintPointers(context, offset);
                }

                return true;
            });
        }

        protected virtual void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                var paint = new Paint {
                    color = new Color(0xFF00FFFF),
                    strokeWidth = 1,
                    style = PaintingStyle.stroke,
                };
                context.canvas.drawRect((offset & size).deflate(0.5f), paint);
                return true;
            });
        }

        protected virtual void debugPaintBaselines(PaintingContext context, Offset offset) {
            D.assert(() => {
                Paint paint = new Paint {
                    style = PaintingStyle.stroke,
                    strokeWidth = 0.25f
                };

                Path path;
                // ideographic baseline
                float? baselineI = getDistanceToBaseline(TextBaseline.ideographic, onlyReal: true);
                if (baselineI != null) {
                    paint.color = new Color(0xFFFFD000);
                    path = new Path();
                    path.moveTo(offset.dx, offset.dy + baselineI.Value);
                    path.lineTo(offset.dx + size.width, offset.dy + baselineI.Value);
                    context.canvas.drawPath(path, paint);
                }

                // alphabetic baseline
                float? baselineA = getDistanceToBaseline(TextBaseline.alphabetic, onlyReal: true);
                if (baselineA != null) {
                    paint.color = new Color(0xFF00FF00);
                    path = new Path();
                    path.moveTo(offset.dx, offset.dy + baselineA.Value);
                    path.lineTo(offset.dx + size.width, offset.dy + baselineA.Value);
                    context.canvas.drawPath(path, paint);
                }

                return true;
            });
        }

        protected virtual void debugPaintPointers(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (_debugActivePointers > 0) {
                    var paint = new Paint {
                        color = new Color((uint)(0x00BBBB | ((0x04000000 * depth) & 0xFF000000))),
                    };
                    context.canvas.drawRect(offset & size, paint);
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Size>("size", _size, missingIfNull: true));
        }
    }

    public abstract class ContainerBoxParentData<ChildType> : ContainerParentDataMixinBoxParentData<ChildType>
        where ChildType : RenderBox {
    }

    public abstract class
        RenderBoxContainerDefaultsMixin<ChildType, ParentDataType>
        : ContainerRenderObjectMixinRenderBox<ChildType, ParentDataType>
        where ChildType : RenderBox
        where ParentDataType : ContainerParentDataMixinBoxParentData<ChildType> {
        public float? defaultComputeDistanceToFirstActualBaseline(TextBaseline baseline) {
            D.assert(!debugNeedsLayout);

            var child = firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                float? result = child.getDistanceToActualBaseline(baseline);
                if (result != null) {
                    return result.Value + childParentData.offset.dy;
                }

                child = childParentData.nextSibling;
            }

            return null;
        }

        public float? defaultComputeDistanceToHighestActualBaseline(TextBaseline baseline) {
            D.assert(!debugNeedsLayout);

            float? result = null;
            var child = firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                float? candidate = child.getDistanceToActualBaseline(baseline);
                if (candidate != null) {
                    candidate += childParentData.offset.dy;
                    if (result != null) {
                        result = Mathf.Min(result.Value, candidate.Value);
                    }
                    else {
                        result = candidate;
                    }
                }

                child = childParentData.nextSibling;
            }

            return result;
        }

        public bool defaultHitTestChildren(BoxHitTestResult result, Offset position = null) {
            ChildType child = lastChild;
            while (child != null) {
                ParentDataType childParentData = child.parentData as ParentDataType;
                bool isHit = result.addWithPaintOffset(
                    offset: childParentData.offset,
                    position: position,
                    hitTest: (BoxHitTestResult resultIn, Offset transformed) => {
                        D.assert(transformed == position - childParentData.offset);
                        return child.hitTest(resultIn, position: transformed);
                    }
                );
                if (isHit) {
                    return true;
                }

                child = childParentData.previousSibling;
            }

            return false;
        }

        public void defaultPaint(PaintingContext context, Offset offset) {
            var child = firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                context.paintChild(child, childParentData.offset + offset);
                child = childParentData.nextSibling;
            }
        }

        public List<ChildType> getChildrenAsList() {
            var result = new List<ChildType>();
            var child = firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                result.Add(child);
                child = childParentData.nextSibling;
            }

            return result;
        }
    }
}