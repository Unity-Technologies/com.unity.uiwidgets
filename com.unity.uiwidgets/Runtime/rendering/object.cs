using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Debug = UnityEngine.Debug;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public interface IParentData {
        void detach();
    }
    
    public class ParentData : IParentData {
        public virtual void detach() {
        }

        public override string ToString() {
            return "<none>";
        }
    }

    public delegate void PaintingContextCallback(PaintingContext context, Offset offset);

    public class PaintingContext : ClipContext {
        public PaintingContext(
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
            D.assert(child._layer is OffsetLayer);
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

        public virtual void paintChild(RenderObject child, Offset offset) {
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
            D.assert(child._layer is OffsetLayer);
            ((OffsetLayer)child._layer).offset = offset;
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
            _canvas = new Canvas(_recorder);
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
            D.assert(painter != null);
            if (childLayer.hasChildren) {
                childLayer.removeAllChildren();
            }
            stopRecordingIfNeeded();
            appendLayer(childLayer);

            var childContext = createChildContext(childLayer, childPaintBounds ?? estimatedBounds);
            painter(childContext, offset);
            childContext.stopRecordingIfNeeded();
        }

        public virtual PaintingContext createChildContext(ContainerLayer childLayer, Rect bounds) {
            return new PaintingContext(childLayer, bounds);
        }

        public ClipRectLayer pushClipRect(
            bool needsCompositing, 
            Offset offset, 
            Rect clipRect, 
            PaintingContextCallback painter,
            Clip clipBehavior = Clip.hardEdge, 
            ClipRectLayer oldLayer = null) {
            Rect offsetClipRect = clipRect.shift(offset);
            if (needsCompositing) {
                ClipRectLayer layer = oldLayer ?? new ClipRectLayer();
                layer.clipRect = offsetClipRect;
                layer.clipBehavior = clipBehavior;
                pushLayer(layer, painter, offset, childPaintBounds: offsetClipRect);
                return layer;
            }
            else {
                clipRectAndPaint(offsetClipRect, clipBehavior, offsetClipRect, () => painter(this, offset));
                return null;
            }
        }

        public ClipRRectLayer pushClipRRect(
            bool needsCompositing, 
            Offset offset, 
            Rect bounds, 
            RRect clipRRect,
            PaintingContextCallback painter, 
            Clip clipBehavior = Clip.antiAlias,
            ClipRRectLayer oldLayer = null) {
            Rect offsetBounds = bounds.shift(offset);
            RRect offsetClipRRect = clipRRect.shift(offset);
            if (needsCompositing) {
                ClipRRectLayer layer = oldLayer ?? new ClipRRectLayer();
                layer.clipRRect = offsetClipRRect;
                layer.clipBehavior = clipBehavior;
                pushLayer(layer, painter, offset, childPaintBounds: offsetBounds);
                return layer;
            }
            else {
                clipRRectAndPaint(offsetClipRRect, clipBehavior, offsetBounds, () => painter(this, offset));
                return null;
            }
        }

        public ClipPathLayer pushClipPath(
            bool needsCompositing, 
            Offset offset, 
            Rect bounds, 
            Path clipPath,
            PaintingContextCallback painter, 
            Clip clipBehavior = Clip.antiAlias,
            ClipPathLayer oldLayer = null) {
            Rect offsetBounds = bounds.shift(offset);
            Path offsetClipPath = clipPath.shift(offset);
            if (needsCompositing) {
                ClipPathLayer layer = oldLayer ?? new ClipPathLayer();
                layer.clipPath = offsetClipPath;
                layer.clipBehavior = clipBehavior;
                pushLayer(layer, painter, offset, childPaintBounds: offsetBounds);
                return layer;
            }
            else {
                clipPathAndPaint(offsetClipPath, clipBehavior, offsetBounds, () => painter(this, offset));
                return null;
            }
        }
        
        public ColorFilterLayer pushColorFilter(Offset offset, ColorFilter colorFilter, PaintingContextCallback painter, ColorFilterLayer oldLayer = null) {
            D.assert(colorFilter != null);
            ColorFilterLayer layer = oldLayer ?? new ColorFilterLayer();
            layer.colorFilter = colorFilter;
            pushLayer(layer, painter, offset);
            return layer;
        }

        public TransformLayer pushTransform(
            bool needsCompositing, 
            Offset offset, 
            Matrix4 transform,
            PaintingContextCallback painter,
            TransformLayer oldLayer = null) {
            Matrix4 effectiveTransform;
            if (offset == null || offset == Offset.zero) {
                effectiveTransform = transform;
            }
            else {
                effectiveTransform = Matrix4.translationValues(offset.dx, offset.dy, 0);
                effectiveTransform.multiply(transform);
                effectiveTransform.translate(-offset.dx, -offset.dy);
            }

            if (needsCompositing) {
                TransformLayer layer = oldLayer ?? new TransformLayer();
                layer.transform = effectiveTransform;

                pushLayer(
                    layer,
                    painter,
                    offset,
                    childPaintBounds:  MatrixUtils.inverseTransformRect(effectiveTransform, estimatedBounds)
                );
                return layer;
            }
            else {
                canvas.save();
                canvas.transform(effectiveTransform._m4storage);
                painter(this, offset);
                canvas.restore();
                return null;
            }
        }
        
        public OpacityLayer pushOpacity(Offset offset, int alpha, PaintingContextCallback painter,  OpacityLayer oldLayer = null) {
            OpacityLayer layer = oldLayer ?? new OpacityLayer();
            layer.alpha = alpha;
            layer.offset = offset;
            pushLayer(layer, painter, Offset.zero);
            return layer;
        }

        public override string ToString() {
            return
                $"{GetType()}#{GetHashCode()}(layer: {_containerLayer}, canvas bounds: {estimatedBounds}";
        }
        
    }

    public abstract class Constraints {
        
        protected Constraints(){}
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

        void reassemble() {
            markNeedsLayout();
            markNeedsCompositingBitsUpdate();
            markNeedsPaint();
            visitChildren((RenderObject child) =>{
                child.reassemble();
            });
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
            IEnumerable<DiagnosticsNode> infoCollector() {
                yield return new DiagnosticsDebugCreator(debugCreator);
                yield return describeForError("The following RenderObject was being processed when the exception was fired");
                yield return describeForError("RenderObject", style: DiagnosticsTreeStyle.truncateChildren);
            }
            
            UIWidgetsError.reportError(new UIWidgetsErrorDetailsForRendering(
                exception: exception,
                library: "rendering library",
                context: new ErrorDescription("during " + method),
                renderObject: this,
                informationCollector: infoCollector
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
                visitChildren(_cleanChildRelayoutBoundary);
            }
        }
        
        static void _cleanChildRelayoutBoundary(RenderObject child) {
            child._cleanRelayoutBoundary();
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
            D.assert(
                () => {
                    IEnumerable<DiagnosticsNode> infoCollector() {
                        yield return new ErrorDescription(
                            $"These invalid constraints were provided to {GetType()}'s layout() " +
                                    "function by the following function, which probably computed the " +
                                    "invalid constraints in question:\n" +
                                    "  unknown");
                    }
                    return constraints.debugAssertIsValid(
                        isAppliedConstraint: true,
                        informationCollector: infoCollector);
                }
                );
            D.assert(!_debugDoingThisResize);
            D.assert(!_debugDoingThisLayout);

            RenderObject relayoutBoundary;
            if (!parentUsesSize || sizedByParent || constraints.isTight || !(parent is RenderObject)) {
                relayoutBoundary = this;
            }
            else {
                relayoutBoundary = (parent as RenderObject)._relayoutBoundary;
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
            if (_relayoutBoundary != null && relayoutBoundary != _relayoutBoundary) {
                visitChildren(_cleanChildRelayoutBoundary);
            }
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

        /// Rotate this render object (not yet implemented).
        void rotate(
            int oldAngle, // 0..3
            int newAngle, // 0..3
            TimeSpan time
        ) { }
        
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

        internal ContainerLayer _layer;

        public ContainerLayer layer {
            get {
                D.assert(!isRepaintBoundary || (_layer == null || _layer is OffsetLayer));
                return _layer;
            }
            set {
                D.assert(
                    !isRepaintBoundary,
                    () => "Attempted to set a layer to a repaint boundary render object.\n" +
                "The framework creates and assigns an OffsetLayer to a repaint " +
                "boundary automatically."
                    );
                _layer = value;
            }
        }
        
        public ContainerLayer debugLayer {
            get {
                ContainerLayer result = null;
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

        public virtual bool needsCompositing {
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

        public bool debugNeedsPaint {
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

                D.assert(_layer is OffsetLayer);

                if (owner != null) {
                    owner._nodesNeedingPaint.Add(this);
                    owner.requestVisualUpdate();
                }
            }
            else if (this.parent is RenderObject) {
                RenderObject parent = this.parent as RenderObject;
                parent.markNeedsPaint();
                D.assert(parent == this.parent);
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
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary("Tried to paint a RenderObject reentrantly."),
                        describeForError(
                            "The following RenderObject was already being painted when it was " +
                            "painted again"
                        ),
                        new ErrorDescription(
                            "Since this typically indicates an infinite recursion, it is " +
                            "disallowed."
                        )
                    });
                }

                return true;
            });

            if (_needsLayout) {
                return;
            }

            D.assert(() => {
                if (_needsCompositingBitsUpdate) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary(
                            "Tried to paint a RenderObject before its compositing bits were " +
                            "updated."
                        ),
                        describeForError(
                            "The following RenderObject was marked as having dirty compositing " +
                            "bits at the time that it was painted"
                        ),
                        new ErrorDescription(
                            "A RenderObject that still has dirty compositing bits cannot be " +
                            "painted because this indicates that the tree has not yet been " +
                            "properly configured for creating the layer tree."
                        ),
                        new ErrorHint(
                            "This usually indicates an error in the Flutter framework itself."
                        )
                    });
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
            bool ancestorSpecified = ancestor != null;
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
            if (ancestorSpecified)
                renderers.Add(ancestor);
            var transform = Matrix4.identity();
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

            if (_needsCompositingBitsUpdate)
                header += " NEEDS-COMPOSITING-BITS-UPDATE";
            
            if (!attached) {
                header += " DETACHED";
            }

            return header;
        }

        public override string toString(DiagnosticLevel minLevel = DiagnosticLevel.info) {
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
            properties.add(new FlagProperty("needsCompositing", value: _needsCompositing, ifTrue: "needs compositing"));
            properties.add(new DiagnosticsProperty<object>(
                "creator", debugCreator, defaultValue: foundation_.kNullDefaultValue,
                level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<ParentData>("parentData", parentData,
                tooltip: _debugCanParentUseSize ? "can use size" : null, missingIfNull: true));
            properties.add(new DiagnosticsProperty<Constraints>("constraints", constraints, missingIfNull: true));
            properties.add(new DiagnosticsProperty<ContainerLayer>("layer", _layer,
                defaultValue: foundation_.kNullDefaultValue));
        }

        public override List<DiagnosticsNode> debugDescribeChildren() => new List<DiagnosticsNode>();
        
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
    public DiagnosticsNode describeForError(string name, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.shallow) {
            return toDiagnosticsNode(name: name, style: style);
        }
    }

    public interface RenderObjectWithChildMixin {
        bool debugValidateChild(RenderObject child);
        RenderObject child { get; set; }
    }

    public interface RenderObjectWithChildMixin<ChildType> : RenderObjectWithChildMixin where ChildType : RenderObject {
        new ChildType child { get; set; }
    }

    public interface ContainerParentDataMixin<ChildType> : IParentData where ChildType : RenderObject {
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

    public interface RelayoutWhenSystemFontsChangeMixin {
        void systemFontsDidChange();
        void attach(object owner);
        void detach();
    }
    
    public class UIWidgetsErrorDetailsForRendering : UIWidgetsErrorDetails {
        public UIWidgetsErrorDetailsForRendering(
            Exception exception = null,
            StackTrace stack = null,
            string library = null,
            DiagnosticsNode context = null,
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
    
    public class DiagnosticsDebugCreator : DiagnosticsProperty<object> {
        public DiagnosticsDebugCreator(object value)
            : base(
                "debugCreator",
                value,
                level: DiagnosticLevel.hidden
            ) {
            D.assert(value != null);
        }
    }
}