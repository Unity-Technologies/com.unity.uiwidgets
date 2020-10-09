using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public class ParentData {
        public virtual void detach() {
        }

        public override string ToString() {
            return "<none>";
        }
    }

    public delegate void PaintingContextCallback(PaintingContext context, Offset offset);

    public class PaintingContext : ClipContext {
        PaintingContext(
            ContainerLayer containerLayer = null,
            Rect estimatedBounds = null
        ) {
            D.assert(containerLayer != null);
            D.assert(estimatedBounds != null);
            _containerLayer = containerLayer;
            this.estimatedBounds = estimatedBounds;
        }

        readonly ContainerLayer _containerLayer;

        public readonly Rect estimatedBounds;

        public static void repaintCompositedChild(RenderObject child, bool debugAlsoPaintedParent = false) {
            D.assert(child._needsPaint);

            _repaintCompositedChild(
                child,
                debugAlsoPaintedParent: debugAlsoPaintedParent
            );
        }

        static void _repaintCompositedChild(
            RenderObject child,
            bool debugAlsoPaintedParent = false,
            PaintingContext childContext = null
        ) {
            D.assert(child.isRepaintBoundary);
            D.assert(() => {
                child.debugRegisterRepaintBoundaryPaint(
                    includedParent: debugAlsoPaintedParent,
                    includedChild: true
                );
                return true;
            });
            if (child._layer == null) {
                D.assert(debugAlsoPaintedParent);
                child._layer = new OffsetLayer();
            }
            else {
                D.assert(debugAlsoPaintedParent || child._layer.attached);
                child._layer.removeAllChildren();
            }

            D.assert(() => {
                child._layer.debugCreator = child.debugCreator ?? child.GetType().ToString();
                return true;
            });
            childContext = childContext ?? new PaintingContext(child._layer, child.paintBounds);
            child._paintWithContext(childContext, Offset.zero);
            childContext.stopRecordingIfNeeded();
        }

        public static void debugInstrumentRepaintCompositedChild(
            RenderObject child,
            bool debugAlsoPaintedParent = false,
            PaintingContext customContext = null
        ) {
            D.assert(() => {
                _repaintCompositedChild(
                    child,
                    debugAlsoPaintedParent: debugAlsoPaintedParent,
                    childContext: customContext
                );
                return true;
            });
        }

        public void paintChild(RenderObject child, Offset offset) {
            if (child.isRepaintBoundary) {
                stopRecordingIfNeeded();
                _compositeChild(child, offset);
            }
            else {
                child._paintWithContext(this, offset);
            }
        }

        void _compositeChild(RenderObject child, Offset offset) {
            D.assert(!_isRecording);
            D.assert(child.isRepaintBoundary);
            D.assert(_canvas == null || _canvas.getSaveCount() == 1);

            if (child._needsPaint) {
                repaintCompositedChild(child, debugAlsoPaintedParent: true);
            }
            else {
                D.assert(child._layer != null);
                D.assert(() => {
                    child.debugRegisterRepaintBoundaryPaint(
                        includedParent: true,
                        includedChild: false
                    );
                    child._layer.debugCreator = child.debugCreator ?? child;
                    return true;
                });
            }

            child._layer.offset = offset;
            appendLayer(child._layer);
        }

        protected virtual void appendLayer(Layer layer) {
            D.assert(!_isRecording);

            layer.remove();
            _containerLayer.append(layer);
        }

        bool _isRecording {
            get {
                bool hasCanvas = _canvas != null;
                D.assert(() => {
                    if (hasCanvas) {
                        D.assert(_currentLayer != null);
                        D.assert(_recorder != null);
                        D.assert(_canvas != null);
                    }
                    else {
                        D.assert(_currentLayer == null);
                        D.assert(_recorder == null);
                        D.assert(_canvas == null);
                    }

                    return true;
                });

                return hasCanvas;
            }
        }

        PictureLayer _currentLayer;
        PictureRecorder _recorder;
        Canvas _canvas;

        public override Canvas canvas {
            get {
                if (_canvas == null) {
                    _startRecording();
                }

                return _canvas;
            }
        }

        void _startRecording() {
            D.assert(!_isRecording);

            _currentLayer = new PictureLayer(estimatedBounds);
            _recorder = new PictureRecorder();
            _canvas = new RecorderCanvas(_recorder);
            _containerLayer.append(_currentLayer);
        }

        protected virtual void stopRecordingIfNeeded() {
            if (!_isRecording) {
                return;
            }

            D.assert(() => {
                if (D.debugRepaintRainbowEnabled) {
                    var paint = new Paint {
                        style = PaintingStyle.stroke,
                        strokeWidth = 6.0f,
                        color = D.debugCurrentRepaintColor.toColor()
                    };
                    canvas.drawRect(estimatedBounds.deflate(3.0f), paint);
                }

                if (D.debugPaintLayerBordersEnabled) {
                    Paint paint = new Paint {
                        style = PaintingStyle.stroke,
                        strokeWidth = 1.0f,
                        color = new Color(0xFFFF9800),
                    };
                    canvas.drawRect(estimatedBounds, paint);
                }

                return true;
            });

            _currentLayer.picture = _recorder.endRecording();
            _currentLayer = null;
            _recorder = null;
            _canvas = null;
        }

        public void setIsComplexHint() {
            if (_currentLayer != null) {
                _currentLayer.isComplexHint = true;
            }
        }

        public void setWillChangeHint() {
            if (_currentLayer != null) {
                _currentLayer.willChangeHint = true;
            }
        }

        public void addLayer(Layer layer) {
            stopRecordingIfNeeded();
            appendLayer(layer);
        }

        public void pushLayer(ContainerLayer childLayer, PaintingContextCallback painter, Offset offset,
            Rect childPaintBounds = null) {
            D.assert(!childLayer.attached);
            D.assert(childLayer.parent == null);
            D.assert(painter != null);

            stopRecordingIfNeeded();
            appendLayer(childLayer);

            var childContext = createChildContext(childLayer, childPaintBounds ?? estimatedBounds);
            painter(childContext, offset);
            childContext.stopRecordingIfNeeded();
        }

        protected PaintingContext createChildContext(ContainerLayer childLayer, Rect bounds) {
            return new PaintingContext(childLayer, bounds);
        }

        public void pushClipRect(bool needsCompositing, Offset offset, Rect clipRect, PaintingContextCallback painter,
            Clip clipBehavior = Clip.hardEdge) {
            Rect offsetClipRect = clipRect.shift(offset);
            if (needsCompositing) {
                pushLayer(new ClipRectLayer(offsetClipRect, clipBehavior: clipBehavior),
                    painter, offset, childPaintBounds: offsetClipRect);
            }
            else {
                clipRectAndPaint(offsetClipRect, clipBehavior, offsetClipRect, () => painter(this, offset));
            }
        }

        public void pushClipRRect(bool needsCompositing, Offset offset, Rect bounds, RRect clipRRect,
            PaintingContextCallback painter, Clip clipBehavior = Clip.antiAlias) {
            Rect offsetBounds = bounds.shift(offset);
            RRect offsetClipRRect = clipRRect.shift(offset);
            if (needsCompositing) {
                pushLayer(new ClipRRectLayer(offsetClipRRect, clipBehavior: clipBehavior),
                    painter, offset, childPaintBounds: offsetBounds);
            }
            else {
                clipRRectAndPaint(offsetClipRRect, clipBehavior, offsetBounds, () => painter(this, offset));
            }
        }

        public void pushClipPath(bool needsCompositing, Offset offset, Rect bounds, Path clipPath,
            PaintingContextCallback painter, Clip clipBehavior = Clip.antiAlias) {
            Rect offsetBounds = bounds.shift(offset);
            Path offsetClipPath = clipPath.shift(offset);
            if (needsCompositing) {
                pushLayer(new ClipPathLayer(clipPath: offsetClipPath, clipBehavior: clipBehavior), painter, offset,
                    childPaintBounds: offsetBounds);
            }
            else {
                clipPathAndPaint(offsetClipPath, clipBehavior, offsetBounds, () => painter(this, offset));
            }
        }

        public void pushTransform(bool needsCompositing, Offset offset, Matrix4 transform,
            PaintingContextCallback painter) {
            Matrix4 effectiveTransform;
            if (offset == null || offset == Offset.zero) {
                effectiveTransform = transform;
            }
            else {
                effectiveTransform = new Matrix4().translationValues(offset.dx, offset.dy, 0);
                effectiveTransform.multiply(transform);
                effectiveTransform.translate(-offset.dx, -offset.dy);
            }

            if (needsCompositing) {
                // it could just be "scale == 0", ignore the assertion.
                // D.assert(invertible);

                pushLayer(
                    new TransformLayer(effectiveTransform),
                    painter,
                    offset,
                    childPaintBounds:  MatrixUtils.inverseTransformRect(effectiveTransform, estimatedBounds)
                );
            }
            else {
                canvas.save();
                canvas.concat(effectiveTransform.toMatrix3());
                painter(this, offset);
                canvas.restore();
            }
        }

        public void pushOpacity(Offset offset, int alpha, PaintingContextCallback painter) {
            pushLayer(new OpacityLayer(alpha: alpha), painter, offset);
        }

        public override string ToString() {
            return
                $"{GetType()}#{GetHashCode()}(layer: {_containerLayer}, canvas bounds: {estimatedBounds}";
        }
    }

    public abstract class Constraints {
        public abstract bool isTight { get; }

        public abstract bool isNormalized { get; }

        public virtual bool debugAssertIsValid(
            bool isAppliedConstraint = false,
            InformationCollector informationCollector = null
        ) {
            D.assert(isNormalized);
            return isNormalized;
        }
    }

    public delegate void RenderObjectVisitor(RenderObject child);

    public delegate void LayoutCallback<T>(T constraints) where T : Constraints;

    public class PipelineOwner {
        public PipelineOwner(
            VoidCallback onNeedVisualUpdate = null) {
            this.onNeedVisualUpdate = onNeedVisualUpdate;
        }

        public readonly VoidCallback onNeedVisualUpdate;

        public void requestVisualUpdate() {
            if (onNeedVisualUpdate != null) {
                onNeedVisualUpdate();
            }
        }

        public AbstractNodeMixinDiagnosticableTree rootNode {
            get { return _rootNode; }
            set {
                if (_rootNode == value) {
                    return;
                }

                if (_rootNode != null) {
                    _rootNode.detach();
                }

                _rootNode = value;
                if (_rootNode != null) {
                    _rootNode.attach(this);
                }
            }
        }

        AbstractNodeMixinDiagnosticableTree _rootNode;

        internal List<RenderObject> _nodesNeedingLayout = new List<RenderObject>();

        public bool debugDoingLayout {
            get { return _debugDoingLayout; }
        }

        internal bool _debugDoingLayout = false;

        public void flushLayout() {
            D.assert(() => {
                _debugDoingLayout = true;
                return true;
            });

            try {
                while (_nodesNeedingLayout.isNotEmpty()) {
                    var dirtyNodes = _nodesNeedingLayout;
                    _nodesNeedingLayout = new List<RenderObject>();
                    dirtyNodes.Sort((a, b) => a.depth - b.depth);
                    foreach (var node in dirtyNodes) {
                        if (node._needsLayout && node.owner == this) {
                            node._layoutWithoutResize();
                        }
                    }
                }
            }
            finally {
                D.assert(() => {
                    _debugDoingLayout = false;
                    return true;
                });
            }
        }

        internal bool _debugAllowMutationsToDirtySubtrees = false;

        internal void _enableMutationsToDirtySubtrees(VoidCallback callback) {
            D.assert(_debugDoingLayout);
            bool oldState = false;
            D.assert(() => {
                oldState = _debugAllowMutationsToDirtySubtrees;
                _debugAllowMutationsToDirtySubtrees = true;
                return true;
            });
            try {
                callback();
            }
            finally {
                D.assert(() => {
                    _debugAllowMutationsToDirtySubtrees = oldState;
                    return true;
                });
            }
        }

        internal List<RenderObject> _nodesNeedingCompositingBitsUpdate = new List<RenderObject>();

        public void flushCompositingBits() {
            _nodesNeedingCompositingBitsUpdate.Sort((a, b) => a.depth - b.depth);
            foreach (RenderObject node in _nodesNeedingCompositingBitsUpdate) {
                if (node._needsCompositingBitsUpdate && node.owner == this) {
                    node._updateCompositingBits();
                }
            }

            _nodesNeedingCompositingBitsUpdate.Clear();
        }

        internal List<RenderObject> _nodesNeedingPaint = new List<RenderObject>();

        public bool debugDoingPaint {
            get { return _debugDoingPaint; }
        }

        internal bool _debugDoingPaint = false;

        public void flushPaint() {
            D.assert(() => {
                _debugDoingPaint = true;
                return true;
            });

            try {
                var dirtyNodes = _nodesNeedingPaint;
                _nodesNeedingPaint = new List<RenderObject>();
                dirtyNodes.Sort((a, b) => a.depth - b.depth);
                foreach (var node in dirtyNodes) {
                    D.assert(node._layer != null);
                    if (node._needsPaint && node.owner == this) {
                        if (node._layer.attached) {
                            PaintingContext.repaintCompositedChild(node);
                        }
                        else {
                            node._skippedPaintingOnLayer();
                        }
                    }
                }
            }
            finally {
                D.assert(() => {
                    _debugDoingPaint = false;
                    return true;
                });
            }
        }
    }

    public abstract class RenderObject : AbstractNodeMixinDiagnosticableTree, HitTestTarget {
        protected RenderObject() {
            _needsCompositing = isRepaintBoundary || alwaysNeedsCompositing;
        }

        public ParentData parentData;

        public virtual void setupParentData(RenderObject child) {
            D.assert(_debugCanPerformMutations);

            if (!(child.parentData is ParentData)) {
                child.parentData = new ParentData();
            }
        }

        protected override void adoptChild(AbstractNodeMixinDiagnosticableTree childNode) {
            var child = (RenderObject) childNode;

            D.assert(_debugCanPerformMutations);
            D.assert(child != null);
            setupParentData(child);
            markNeedsLayout();
            markNeedsCompositingBitsUpdate();
            base.adoptChild(child);
        }

        protected override void dropChild(AbstractNodeMixinDiagnosticableTree childNode) {
            var child = (RenderObject) childNode;

            D.assert(_debugCanPerformMutations);
            D.assert(child != null);
            D.assert(child.parentData != null);
            child._cleanRelayoutBoundary();
            child.parentData.detach();
            child.parentData = null;
            base.dropChild(child);
            markNeedsLayout();
            markNeedsCompositingBitsUpdate();
        }

        public virtual void visitChildren(RenderObjectVisitor visitor) {
        }

        public object debugCreator;

        void _debugReportException(string method, Exception exception) {
            UIWidgetsError.reportError(new UIWidgetsErrorDetailsForRendering(
                exception: exception,
                library: "rendering library",
                context: "during " + method,
                renderObject: this,
                informationCollector: information => {
                    information.AppendLine(
                        "The following RenderObject was being processed when the exception was fired:");
                    information.AppendLine("  " + toStringShallow(joiner: "\n  "));
                    var descendants = new List<string>();
                    const int maxDepth = 5;
                    int depth = 0;
                    const int maxLines = 25;
                    int lines = 0;
                    RenderObjectVisitor visitor = null;
                    visitor = new RenderObjectVisitor((RenderObject child) => {
                        if (lines < maxLines) {
                            depth += 1;
                            descendants.Add(new string(' ', 2 * depth) + child);
                            if (depth < maxDepth) {
                                child.visitChildren(visitor);
                            }

                            depth -= 1;
                        }
                        else if (lines == maxLines) {
                            descendants.Add("  ...(descendants list truncated after " + lines + " lines)");
                        }

                        lines += 1;
                    });
                    visitChildren(visitor);
                    if (lines > 1) {
                        information.AppendLine(
                            "This RenderObject had the following descendants (showing up to depth " +
                            maxDepth + "):");
                    }
                    else if (descendants.Count == 1) {
                        information.AppendLine("This RenderObject had the following child:");
                    }
                    else {
                        information.AppendLine("This RenderObject has no descendants.");
                    }

                    information.Append(string.Join("\n", descendants.ToArray()));
                }
            ));
        }

        public bool debugDoingThisResize {
            get { return _debugDoingThisResize; }
        }

        bool _debugDoingThisResize = false;

        public bool debugDoingThisLayout {
            get { return _debugDoingThisLayout; }
        }

        bool _debugDoingThisLayout = false;

        public static RenderObject debugActiveLayout {
            get { return _debugActiveLayout; }
        }

        static RenderObject _debugActiveLayout;

        public bool debugCanParentUseSize {
            get { return _debugCanParentUseSize; }
        }

        bool _debugCanParentUseSize;

        bool _debugMutationsLocked = false;

        bool _debugCanPerformMutations {
            get {
                bool result = true;
                D.assert(() => {
                    RenderObject node = this;
                    while (true) {
                        if (node._doingThisLayoutWithCallback) {
                            result = true;
                            break;
                        }

                        if (owner != null && owner._debugAllowMutationsToDirtySubtrees && node._needsLayout) {
                            result = true;
                            break;
                        }

                        if (node._debugMutationsLocked) {
                            result = false;
                            break;
                        }

                        if (!(node.parent is RenderObject)) {
                            result = true;
                            break;
                        }

                        node = (RenderObject) node.parent;
                    }

                    return true;
                });
                return result;
            }
        }

        public new PipelineOwner owner {
            get { return (PipelineOwner) base.owner; }
        }

        public override void attach(object ownerObject) {
            var owner = (PipelineOwner) ownerObject;

            base.attach(owner);
            if (_needsLayout && _relayoutBoundary != null) {
                _needsLayout = false;
                markNeedsLayout();
            }

            if (_needsCompositingBitsUpdate) {
                _needsCompositingBitsUpdate = false;
                markNeedsCompositingBitsUpdate();
            }

            if (_needsPaint && _layer != null) {
                _needsPaint = false;
                markNeedsPaint();
            }
        }

        public bool debugNeedsLayout {
            get {
                bool result = false;
                D.assert(() => {
                    result = _needsLayout;
                    return true;
                });
                return result;
            }
        }

        internal bool _needsLayout = true;

        public RenderObject _relayoutBoundary;
        bool _doingThisLayoutWithCallback = false;

        public Constraints constraints {
            get { return _constraints; }
        }

        Constraints _constraints;

        protected abstract void debugAssertDoesMeetConstraints();

        internal static bool debugCheckingIntrinsics = false;

        bool _debugSubtreeRelayoutRootAlreadyMarkedNeedsLayout() {
            if (_relayoutBoundary == null) {
                return true;
            }

            RenderObject node = this;
            while (node != _relayoutBoundary) {
                D.assert(node._relayoutBoundary == _relayoutBoundary);
                D.assert(node.parent != null);
                node = (RenderObject) node.parent;
                if ((!node._needsLayout) && (!node._debugDoingThisLayout)) {
                    return false;
                }
            }

            D.assert(node._relayoutBoundary == node);
            return true;
        }

        public virtual void markNeedsLayout() {
            D.assert(_debugCanPerformMutations);
            if (_needsLayout) {
                D.assert(_debugSubtreeRelayoutRootAlreadyMarkedNeedsLayout());
                return;
            }

            D.assert(_relayoutBoundary != null);
            if (_relayoutBoundary != this) {
                markParentNeedsLayout();
            }
            else {
                _needsLayout = true;
                if (owner != null) {
                    D.assert(() => {
                        if (D.debugPrintMarkNeedsLayoutStacks) {
                            Debug.Log("markNeedsLayout() called for " + this);
                        }

                        return true;
                    });

                    owner._nodesNeedingLayout.Add(this);
                    owner.requestVisualUpdate();
                }
            }
        }

        protected void markParentNeedsLayout() {
            _needsLayout = true;

            RenderObject parent = (RenderObject) this.parent;
            if (!_doingThisLayoutWithCallback) {
                parent.markNeedsLayout();
            }
            else {
                D.assert(parent._debugDoingThisLayout);
            }
        }

        public void markNeedsLayoutForSizedByParentChange() {
            markNeedsLayout();
            markParentNeedsLayout();
        }

        void _cleanRelayoutBoundary() {
            if (_relayoutBoundary != this) {
                _relayoutBoundary = null;
                _needsLayout = true;
                visitChildren(child => { child._cleanRelayoutBoundary(); });
            }
        }

        public void scheduleInitialLayout() {
            D.assert(attached);
            D.assert(!(parent is RenderObject));
            D.assert(!owner._debugDoingLayout);
            D.assert(_relayoutBoundary == null);

            _relayoutBoundary = this;
            D.assert(() => {
                _debugCanParentUseSize = false;
                return true;
            });
            owner._nodesNeedingLayout.Add(this);
        }

        internal void _layoutWithoutResize() {
            D.assert(_relayoutBoundary == this);
            RenderObject debugPreviousActiveLayout = null;
            D.assert(!_debugMutationsLocked);
            D.assert(!_doingThisLayoutWithCallback);
            D.assert(() => {
                _debugMutationsLocked = true;
                _debugDoingThisLayout = true;
                debugPreviousActiveLayout = _debugActiveLayout;
                _debugActiveLayout = this;
                if (D.debugPrintLayouts) {
                    Debug.Log("Laying out (without resize) " + this);
                }

                return true;
            });

            try {
                performLayout();
            }
            catch (Exception ex) {
                _debugReportException("performLayout", ex);
            }

            D.assert(() => {
                _debugActiveLayout = debugPreviousActiveLayout;
                _debugDoingThisLayout = false;
                _debugMutationsLocked = false;
                return true;
            });

            _needsLayout = false;
            markNeedsPaint();
        }

        public void layout(Constraints constraints, bool parentUsesSize = false) {
            D.assert(constraints != null);
            D.assert(constraints.debugAssertIsValid(
                isAppliedConstraint: true,
                informationCollector: information => {
//                final List<String> stack = StackTrace.current.toString().split('\n');
//                int targetFrame;
//                final Pattern layoutFramePattern = RegExp(r'^#[0-9]+ +RenderObject.layout \(');
//                for (int i = 0; i < stack.length; i += 1) {
//                    if (layoutFramePattern.matchAsPrefix(stack[i]) != null) {
//                        targetFrame = i + 1;
//                        break;
//                    }
//                }
//                if (targetFrame != null && targetFrame < stack.length) {
//                    information.writeln(
//                        'These invalid constraints were provided to $runtimeType\'s layout() '
//                    'function by the following function, which probably computed the '
//                    'invalid constraints in question:'
//                        );
//                    final Pattern targetFramePattern = RegExp(r'^#[0-9]+ +(.+)$');
//                    final Match targetFrameMatch = targetFramePattern.matchAsPrefix(stack[targetFrame]);
//                    if (targetFrameMatch != null && targetFrameMatch.groupCount > 0) {
//                        information.writeln('  ${targetFrameMatch.group(1)}');
//                    } else {
//                        information.writeln(stack[targetFrame]);
//                    }
//                }
                }));
            D.assert(!_debugDoingThisResize);
            D.assert(!_debugDoingThisLayout);

            RenderObject relayoutBoundary;
            if (!parentUsesSize || sizedByParent || constraints.isTight || !(this.parent is RenderObject)) {
                relayoutBoundary = this;
            }
            else {
                RenderObject parent = (RenderObject) this.parent;
                relayoutBoundary = parent._relayoutBoundary;
            }

            D.assert(() => {
                _debugCanParentUseSize = parentUsesSize;
                return true;
            });

            if (!_needsLayout && Equals(constraints, _constraints) &&
                relayoutBoundary == _relayoutBoundary) {
                D.assert(() => {
                    _debugDoingThisResize = sizedByParent;
                    _debugDoingThisLayout = !sizedByParent;
                    RenderObject debugPreviousActiveLayout1 = _debugActiveLayout;
                    _debugActiveLayout = this;
                    debugResetSize();
                    _debugActiveLayout = debugPreviousActiveLayout1;
                    _debugDoingThisLayout = false;
                    _debugDoingThisResize = false;
                    return true;
                });

                return;
            }

            _constraints = constraints;
            _relayoutBoundary = relayoutBoundary;

            D.assert(!_debugMutationsLocked);
            D.assert(!_doingThisLayoutWithCallback);
            D.assert(() => {
                _debugMutationsLocked = true;
                if (D.debugPrintLayouts) {
                    Debug.Log("Laying out (" + (sizedByParent ? "with separate resize" : "with resize allowed") +
                              ") " + this);
                }

                return true;
            });

            if (sizedByParent) {
                D.assert(() => {
                    _debugDoingThisResize = true;
                    return true;
                });

                try {
                    performResize();
                    D.assert(() => {
                        debugAssertDoesMeetConstraints();
                        return true;
                    });
                }
                catch (Exception ex) {
                    _debugReportException("performResize", ex);
                }

                D.assert(() => {
                    _debugDoingThisResize = false;
                    return true;
                });
            }

            RenderObject debugPreviousActiveLayout = null;
            D.assert(() => {
                _debugDoingThisLayout = true;
                debugPreviousActiveLayout = _debugActiveLayout;
                _debugActiveLayout = this;
                return true;
            });

            try {
                performLayout();
                D.assert(() => {
                    debugAssertDoesMeetConstraints();
                    return true;
                });
            }
            catch (Exception ex) {
                _debugReportException("performLayout", ex);
            }

            D.assert(() => {
                _debugActiveLayout = debugPreviousActiveLayout;
                _debugDoingThisLayout = false;
                _debugMutationsLocked = false;
                return true;
            });

            _needsLayout = false;
            markNeedsPaint();
        }

        protected virtual void debugResetSize() {
        }

        protected virtual bool sizedByParent {
            get { return false; }
        }

        protected abstract void performResize();

        protected abstract void performLayout();

        protected void invokeLayoutCallback<T>(LayoutCallback<T> callback) where T : Constraints {
            D.assert(_debugMutationsLocked);
            D.assert(_debugDoingThisLayout);
            D.assert(!_doingThisLayoutWithCallback);

            _doingThisLayoutWithCallback = true;
            try {
                owner._enableMutationsToDirtySubtrees(() => { callback((T) constraints); });
            }
            finally {
                _doingThisLayoutWithCallback = false;
            }
        }

        public bool debugDoingThisPaint {
            get { return _debugDoingThisPaint; }
        }

        bool _debugDoingThisPaint = false;

        public static RenderObject debugActivePaint {
            get { return _debugActivePaint; }
        }

        static RenderObject _debugActivePaint;

        public virtual bool isRepaintBoundary {
            get { return false; }
        }

        public virtual void debugRegisterRepaintBoundaryPaint(bool includedParent = true, bool includedChild = false) {
        }

        protected virtual bool alwaysNeedsCompositing {
            get { return false; }
        }

        internal OffsetLayer _layer;

        public OffsetLayer layer {
            get {
                D.assert(isRepaintBoundary,
                    () => "You can only access RenderObject.layer for render objects that are repaint boundaries.");
                D.assert(!_needsPaint);

                return _layer;
            }
        }

        public OffsetLayer debugLayer {
            get {
                OffsetLayer result = null;
                D.assert(() => {
                    result = _layer;
                    return true;
                });
                return result;
            }
        }

        internal bool _needsCompositingBitsUpdate = false;

        public void markNeedsCompositingBitsUpdate() {
            if (_needsCompositingBitsUpdate) {
                return;
            }

            _needsCompositingBitsUpdate = true;

            if (this.parent is RenderObject) {
                var parent = (RenderObject) this.parent;
                if (parent._needsCompositingBitsUpdate) {
                    return;
                }

                if (!isRepaintBoundary && !parent.isRepaintBoundary) {
                    parent.markNeedsCompositingBitsUpdate();
                    return;
                }
            }

            D.assert(() => {
                var parent = this.parent;
                if (parent is RenderObject) {
                    return ((RenderObject) parent)._needsCompositing;
                }

                return true;
            });

            if (owner != null) {
                owner._nodesNeedingCompositingBitsUpdate.Add(this);
            }
        }

        bool _needsCompositing;

        public bool needsCompositing {
            get {
                D.assert(!_needsCompositingBitsUpdate);
                return _needsCompositing;
            }
        }

        public void _updateCompositingBits() {
            if (!_needsCompositingBitsUpdate) {
                return;
            }

            bool oldNeedsCompositing = _needsCompositing;
            _needsCompositing = false;
            visitChildren(child => {
                child._updateCompositingBits();
                if (child.needsCompositing) {
                    _needsCompositing = true;
                }
            });

            if (isRepaintBoundary || alwaysNeedsCompositing) {
                _needsCompositing = true;
            }

            if (oldNeedsCompositing != _needsCompositing) {
                markNeedsPaint();
            }

            _needsCompositingBitsUpdate = false;
        }

        bool debugNeedsPaint {
            get {
                bool result = false;
                D.assert(() => {
                    result = _needsPaint;
                    return true;
                });
                return result;
            }
        }

        internal bool _needsPaint = true;

        public void markNeedsPaint() {
            D.assert(owner == null || !owner.debugDoingPaint);

            if (_needsPaint) {
                return;
            }

            _needsPaint = true;
            if (isRepaintBoundary) {
                D.assert(() => {
                    if (D.debugPrintMarkNeedsPaintStacks) {
                        Debug.Log("markNeedsPaint() called for " + this);
                    }

                    return true;
                });

                D.assert(_layer != null);

                if (owner != null) {
                    owner._nodesNeedingPaint.Add(this);
                    owner.requestVisualUpdate();
                }
            }
            else if (this.parent is RenderObject) {
                D.assert(_layer == null);
                var parent = (RenderObject) this.parent;
                parent.markNeedsPaint();
            }
            else {
                D.assert(() => {
                    if (D.debugPrintMarkNeedsPaintStacks) {
                        Debug.Log("markNeedsPaint() called for " + this + " (root of render tree)");
                    }

                    return true;
                });

                if (owner != null) {
                    owner.requestVisualUpdate();
                }
            }
        }

        internal void _skippedPaintingOnLayer() {
            D.assert(attached);
            D.assert(isRepaintBoundary);
            D.assert(_needsPaint);
            D.assert(_layer != null);
            D.assert(!_layer.attached);

            var ancestor = parent;
            while (ancestor is RenderObject) {
                var node = (RenderObject) ancestor;
                if (node.isRepaintBoundary) {
                    if (node._layer == null) {
                        break;
                    }

                    if (node._layer.attached) {
                        break;
                    }

                    node._needsPaint = true;
                }

                ancestor = ancestor.parent;
            }
        }

        public void scheduleInitialPaint(ContainerLayer rootLayer) {
            D.assert(rootLayer.attached);
            D.assert(attached);
            D.assert(!(parent is RenderObject));
            D.assert(!owner._debugDoingPaint);
            D.assert(isRepaintBoundary);
            D.assert(_layer == null);

            _layer = (OffsetLayer) rootLayer;
            D.assert(_needsPaint);
            owner._nodesNeedingPaint.Add(this);
        }

        public void replaceRootLayer(OffsetLayer rootLayer) {
            D.assert(rootLayer.attached);
            D.assert(attached);
            D.assert(!(parent is RenderObject));
            D.assert(!owner._debugDoingPaint);
            D.assert(isRepaintBoundary);
            D.assert(_layer != null);


            _layer.detach();
            _layer = rootLayer;
            markNeedsPaint();
        }

        internal void _paintWithContext(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (_debugDoingThisPaint) {
                    throw new UIWidgetsError(
                        "Tried to paint a RenderObject reentrantly.\n" +
                        "The following RenderObject was already being painted when it was " +
                        "painted again:\n" +
                        "  " + toStringShallow(joiner: "\n    ") + "\n" +
                        "Since this typically indicates an infinite recursion, it is " +
                        "disallowed."
                    );
                }

                return true;
            });

            if (_needsLayout) {
                return;
            }

            D.assert(() => {
                if (_needsCompositingBitsUpdate) {
                    throw new UIWidgetsError(
                        "Tried to paint a RenderObject before its compositing bits were " +
                        "updated.\n" +
                        "The following RenderObject was marked as having dirty compositing " +
                        "bits at the time that it was painted:\n" +
                        "  " + toStringShallow(joiner: "\n    ") + "\n" +
                        "A RenderObject that still has dirty compositing bits cannot be " +
                        "painted because this indicates that the tree has not yet been " +
                        "properly configured for creating the layer tree.\n" +
                        "This usually indicates an error in the Flutter framework itself."
                    );
                }

                return true;
            });

            RenderObject debugLastActivePaint = null;
            D.assert(() => {
                _debugDoingThisPaint = true;
                debugLastActivePaint = _debugActivePaint;
                _debugActivePaint = this;
                D.assert(!isRepaintBoundary || _layer != null);
                return true;
            });

            _needsPaint = false;
            try {
                paint(context, offset);
                D.assert(!_needsLayout);
                D.assert(!_needsPaint);
            }
            catch (Exception ex) {
                _debugReportException("paint", ex);
            }

            D.assert(() => {
                debugPaint(context, offset);
                _debugActivePaint = debugLastActivePaint;
                _debugDoingThisPaint = false;
                return true;
            });
        }

        public abstract Rect paintBounds { get; }

        public virtual void debugPaint(PaintingContext context, Offset offset) {
        }

        public virtual void paint(PaintingContext context, Offset offset) {
        }

        public virtual void applyPaintTransform(RenderObject child, Matrix4 transform) {
            D.assert(child.parent == this);
        }

        public Matrix4 getTransformTo(RenderObject ancestor) {
            D.assert(attached);

            if (ancestor == null) {
                var rootNode = owner.rootNode;
                if (rootNode is RenderObject) {
                    ancestor = (RenderObject) rootNode;
                }
            }

            var renderers = new List<RenderObject>();
            for (RenderObject renderer = this; renderer != ancestor; renderer = (RenderObject) renderer.parent) {
                D.assert(renderer != null);
                renderers.Add(renderer);
            }

            var transform = new Matrix4().identity();
            for (int index = renderers.Count - 1; index > 0; index -= 1) {
                renderers[index].applyPaintTransform(renderers[index - 1], transform);
            }

            return transform;
        }

        public virtual Rect describeApproximatePaintClip(RenderObject child) {
            return null;
        }

        public abstract Rect semanticBounds { get; }

        public virtual void handleEvent(PointerEvent evt, HitTestEntry entry) {
        }

        public override string toStringShort() {
            string header = foundation_.describeIdentity(this);
            if (_relayoutBoundary != null && _relayoutBoundary != this) {
                int count = 1;
                RenderObject target = (RenderObject) parent;
                while (target != null && target != _relayoutBoundary) {
                    target = (RenderObject) target.parent;
                    count += 1;
                }

                header += " relayoutBoundary=up" + count;
            }

            if (_needsLayout) {
                header += " NEEDS-LAYOUT";
            }

            if (_needsPaint) {
                header += " NEEDS-PAINT";
            }

            if (!attached) {
                header += " DETACHED";
            }

            return header;
        }

        public override string toString(DiagnosticLevel minLevel = DiagnosticLevel.debug) {
            return toStringShort();
        }

        public override string toStringDeep(
            string prefixLineOne = "",
            string prefixOtherLines = "",
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        ) {
            RenderObject debugPreviousActiveLayout = null;
            D.assert(() => {
                debugPreviousActiveLayout = _debugActiveLayout;
                _debugActiveLayout = null;
                return true;
            });
            string result = base.toStringDeep(
                prefixLineOne: prefixLineOne,
                prefixOtherLines: prefixOtherLines,
                minLevel: minLevel
            );
            D.assert(() => {
                _debugActiveLayout = debugPreviousActiveLayout;
                return true;
            });
            return result;
        }

        public override string toStringShallow(
            string joiner = ", ",
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        ) {
            RenderObject debugPreviousActiveLayout = null;
            D.assert(() => {
                debugPreviousActiveLayout = _debugActiveLayout;
                _debugActiveLayout = null;
                return true;
            });
            string result = base.toStringShallow(joiner: joiner, minLevel: minLevel);
            D.assert(() => {
                _debugActiveLayout = debugPreviousActiveLayout;
                return true;
            });
            return result;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>(
                "creator", debugCreator, defaultValue: foundation_.kNullDefaultValue,
                level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<ParentData>("parentData", parentData,
                tooltip: _debugCanParentUseSize ? "can use size" : null, missingIfNull: true));
            properties.add(new DiagnosticsProperty<Constraints>("constraints", constraints, missingIfNull: true));
            properties.add(new DiagnosticsProperty<OffsetLayer>("layer", _layer,
                defaultValue: foundation_.kNullDefaultValue));
        }

        public virtual void showOnScreen(
            RenderObject descendant = null,
            Rect rect = null,
            TimeSpan? duration = null,
            Curve curve = null
        ) {
            duration = duration ?? TimeSpan.Zero;
            curve = curve ?? Curves.ease;

            if (parent is RenderObject) {
                RenderObject renderParent = (RenderObject) parent;
                renderParent.showOnScreen(
                    descendant: descendant ?? this,
                    rect: rect,
                    duration: duration,
                    curve: curve
                );
            }
        }
    }

    public interface RenderObjectWithChildMixin {
        bool debugValidateChild(RenderObject child);
        RenderObject child { get; set; }
    }

    public interface RenderObjectWithChildMixin<ChildType> : RenderObjectWithChildMixin
        where ChildType : RenderObject {
        new ChildType child { get; set; }
    }

    public interface ContainerParentDataMixin<ChildType> where ChildType : RenderObject {
        ChildType previousSibling { get; set; }
        ChildType nextSibling { get; set; }
    }

    public interface ContainerRenderObjectMixin {
        int childCount { get; }
        bool debugValidateChild(RenderObject child);
        void insert(RenderObject child, RenderObject after = null);
        void remove(RenderObject child);
        void move(RenderObject child, RenderObject after = null);
        RenderObject firstChild { get; }
        RenderObject lastChild { get; }
        RenderObject childBefore(RenderObject child);
        RenderObject childAfter(RenderObject child);
    }

    public class UIWidgetsErrorDetailsForRendering : UIWidgetsErrorDetails {
        public UIWidgetsErrorDetailsForRendering(
            Exception exception = null,
            string library = null,
            string context = null,
            RenderObject renderObject = null,
            InformationCollector informationCollector = null,
            bool silent = false
        ) : base(
            exception: exception,
            library: library,
            context: context,
            informationCollector: informationCollector,
            silent: silent
        ) {
            this.renderObject = renderObject;
        }

        public readonly RenderObject renderObject;
    }
}