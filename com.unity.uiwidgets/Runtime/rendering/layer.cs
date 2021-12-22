using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Object = System.Object;
using Rect = Unity.UIWidgets.ui.Rect;
using Shader = Unity.UIWidgets.ui.Shader;
namespace Unity.UIWidgets.rendering {
    public class AnnotationEntry<T> {
        
        public AnnotationEntry(
             T annotation = default(T),
             Offset localPosition = null
        ) {
            D.assert(localPosition != null);
            this.annotation = annotation;
            this.localPosition = localPosition;
        }

        public readonly  T annotation;
        public readonly  Offset localPosition;
        public override string ToString() {
            return $"{GetType()}"+ $"(annotation: {annotation}, localPostion: {localPosition})";
        }
    }
    public class AnnotationResult<T> {
        public readonly  List<AnnotationEntry<T>> _entries = new List<AnnotationEntry<T>>();

        public void add(AnnotationEntry<T> entry) {
             _entries.Add(entry);
        }

        public IEnumerable<AnnotationEntry<T>> entries {
            get {
                return _entries;
            }
        }

        public IEnumerable<T> annotations  {
            get{
                List<T> results = new List<T>();
                foreach (AnnotationEntry<T> entry in _entries)
                    results.Add(entry.annotation);
                return results;
            }
        }
    }
    public abstract class Layer : AbstractNodeMixinDiagnosticableTree {
        public new ContainerLayer parent {
            get { return (ContainerLayer) base.parent; }
        }

        public bool _needsAddToScene = true;

        protected void markNeedsAddToScene() {
            D.assert(
                !alwaysNeedsAddToScene,()=>
                GetType() +" with alwaysNeedsAddToScene set called markNeedsAddToScene.\n" + 
                "The layer's alwaysNeedsAddToScene is set to true, and therefore it should not call markNeedsAddToScene."
                );
            if (_needsAddToScene) {
                return;
            }
            
            _needsAddToScene = true;
        }
        void debugMarkClean() {
            D.assert(()=> {
                _needsAddToScene = false;
                return true;
            });
        }

        protected virtual bool alwaysNeedsAddToScene {
            get { return false; }
        }
        
        bool  debugSubtreeNeedsAddToScene {
            get {
                bool result = false;
                D.assert(()=>{
                    result = _needsAddToScene;
                    return true;
                });
                return result;
            }
        }

        protected EngineLayer engineLayer {
            get { return _engineLayer; }
            set {
                _engineLayer = value;
                if (!alwaysNeedsAddToScene) {
                    if (parent != null && !parent.alwaysNeedsAddToScene) {
                        parent.markNeedsAddToScene();
                    }
                }
            }
        }

        protected EngineLayer _engineLayer;

        internal virtual void updateSubtreeNeedsAddToScene() {
            _needsAddToScene = _needsAddToScene || alwaysNeedsAddToScene;
        }

        public Layer nextSibling {
            get { return _nextSibling; }
        }
        internal Layer _nextSibling;

        public Layer previousSibling {
            get { return _previousSibling; }
        }
        internal Layer _previousSibling;

        protected override void dropChild(AbstractNodeMixinDiagnosticableTree child) {
            if (!alwaysNeedsAddToScene) {
                markNeedsAddToScene();
            }
            base.dropChild(child);
        }

        protected override void adoptChild(AbstractNodeMixinDiagnosticableTree child) {
            if (!alwaysNeedsAddToScene) {
                markNeedsAddToScene();
            }
            base.adoptChild(child);
        }

        public virtual void remove() {
            parent?._removeChild(this);
        }

        public virtual bool findAnnotations<S>(
            AnnotationResult<S> result,
            Offset localPosition, 
            bool onlyFirst
        ) {
            return false;
        }
        S find<S>(Offset localPosition) {
            AnnotationResult<S> result = new AnnotationResult<S>();
            findAnnotations<S>(result, localPosition, onlyFirst: true);
            return !result.entries.Any() ?  default : result.entries.First().annotation;
        }

        AnnotationResult<S> findAllAnnotations<S>(Offset localPosition) {
            AnnotationResult<S> result = new AnnotationResult<S>();
            findAnnotations<S>(result, localPosition, onlyFirst: false);
            return result;
        }

        public abstract void addToScene(SceneBuilder builder, Offset layerOffset = null);

        internal void _addToSceneWithRetainedRendering(SceneBuilder builder) {
            if (!_needsAddToScene && _engineLayer != null) {
                builder.addRetained(_engineLayer);
                return;
            }

            addToScene(builder);
            _needsAddToScene = false;
        }
        public object debugCreator;

        public override string toStringShort() {
            return base.toStringShort() + (owner == null ? " DETACHED" : "");
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>("owner", owner,
                level: parent != null ? DiagnosticLevel.hidden : DiagnosticLevel.info,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<object>("creator", debugCreator,
                defaultValue: foundation_.kNullDefaultValue, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<string>("engine layer", foundation_.describeIdentity(_engineLayer)));
        }
        public void replaceWith(Layer newLayer) {
            D.assert(parent != null);
            D.assert(attached == parent.attached);
            D.assert(newLayer.parent == null);
            D.assert(newLayer._nextSibling == null);
            D.assert(newLayer._previousSibling == null);
            D.assert(!newLayer.attached);

            newLayer._nextSibling = nextSibling;
            if (_nextSibling != null) {
                _nextSibling._previousSibling = newLayer;
            }

            newLayer._previousSibling = previousSibling;
            if (_previousSibling != null) {
                _previousSibling._nextSibling = newLayer;
            }

            D.assert(() => {
                Layer node = this;
                while (node.parent != null) {
                    node = node.parent;
                }

                D.assert(node != newLayer);
                return true;
            });

            parent.adoptChild(newLayer);
            D.assert(newLayer.attached == parent.attached);

            if (parent.firstChild == this) {
                parent._firstChild = newLayer;
            }

            if (parent.lastChild == this) {
                parent._lastChild = newLayer;
            }

            _nextSibling = null;
            _previousSibling = null;
            parent.dropChild(this);
            D.assert(!attached);
        }

        //internal abstract S find<S>(Offset regionOffset) where S : class;

        

        
    }
    

    public class PictureLayer : Layer {
        public PictureLayer(Rect canvasBounds) {
            this.canvasBounds = canvasBounds;
        }

        public readonly Rect canvasBounds;

        Picture _picture;
        public Picture picture {
            get { return _picture; }
            set {
                markNeedsAddToScene();
                _picture = value;
            }
        }

        bool _isComplexHint = false; 
        public bool isComplexHint {
            get { return _isComplexHint; }
            set {
                if (value != _isComplexHint) {
                    _isComplexHint = value;
                    markNeedsAddToScene();
                }
            }
        }

        bool _willChangeHint = false;
        public bool willChangeHint {
            get { return _willChangeHint; }
            set {
                if (value != _willChangeHint) {
                    _willChangeHint = value;
                    markNeedsAddToScene();
                }
            }
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            builder.addPicture(layerOffset, picture,
                isComplexHint: isComplexHint, willChangeHint: willChangeHint);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Rect>("paint bounds", canvasBounds));
            properties.add(new DiagnosticsProperty<string>("picture", foundation_.describeIdentity(_picture)));
            properties.add(new DiagnosticsProperty<string>(
                    "raster cache hints",
                    $"isComplex = {isComplexHint} willChange = {willChangeHint}")
            );
        }

        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition,bool onlyFirst ) {
            return false;
        }
    }

    public class TextureLayer : Layer {
        public TextureLayer(
            Rect rect,
            int? textureId,
            bool freeze = false
        ) {
            D.assert(rect != null);
            D.assert(textureId != null);
            this.rect = rect;
            this.textureId = textureId.Value;
            this.freeze = freeze;
        }

        public readonly Rect rect;

        public readonly int textureId;

        public readonly bool freeze;


        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition, bool onlyFirst ) {
            return false;
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;

            Rect shiftedRect = layerOffset == Offset.zero ? rect : rect.shift(layerOffset);
            builder.addTexture(
                textureId,
                offset: shiftedRect.topLeft,
                width: shiftedRect.width,
                height: shiftedRect.height,
                freeze: freeze
            );
        }
    }

    public class ContainerLayer : Layer {
        public Layer firstChild {
            get { return _firstChild; }
        }
        internal Layer _firstChild;

        public Layer lastChild {
            get { return _lastChild; }
        }
        internal Layer _lastChild;
        internal bool hasChildren {
            get { return _firstChild != null; }
        }

        public ui.Scene buildScene(ui.SceneBuilder builder) {
            List<PictureLayer> temporaryLayers =  new List<PictureLayer>();
            D.assert(()=> {
                if (RenderingDebugUtils.debugCheckElevationsEnabled) {
                    temporaryLayers = _debugCheckElevations();
                }
                return true;
            });
            updateSubtreeNeedsAddToScene();
            addToScene(builder);
            
            _needsAddToScene = false;
            ui.Scene scene = builder.build();
            D.assert(()=>{
                if (temporaryLayers != null) {
                    foreach ( PictureLayer temporaryLayer in temporaryLayers) {
                        temporaryLayer.remove();
                    }
                }
                return true;
            });
            return scene;
        }
        bool _debugUltimatePreviousSiblingOf(Layer child, Layer equals = null) {
            D.assert(child.attached == attached);
            while (child.previousSibling != null) {
                D.assert(child.previousSibling != child);
                child = child.previousSibling;
                D.assert(child.attached == attached);
            }
            return child == equals;
        }

        bool _debugUltimateNextSiblingOf(Layer child, Layer equals = null) {
            D.assert(child.attached == attached);
            while (child._nextSibling != null) {
                D.assert(child._nextSibling != child);
                child = child._nextSibling;
                D.assert(child.attached == attached);
            }
            return child == equals;
        }

        PictureLayer _highlightConflictingLayer(PhysicalModelLayer child) {
            PictureRecorder recorder = new PictureRecorder();
            var canvas = new Canvas(recorder);
            canvas.drawPath(child.clipPath, new Paint() {
                color = new Color(0xFFAA0000),
                style = PaintingStyle.stroke,
                strokeWidth = child.elevation.Value + 10.0f,
            });
            PictureLayer pictureLayer = new PictureLayer(child.clipPath.getBounds());
            pictureLayer.picture = recorder.endRecording();
            pictureLayer.debugCreator = child;
            child.append(pictureLayer);
            return pictureLayer;
        }

        List<PictureLayer> _processConflictingPhysicalLayers(PhysicalModelLayer predecessor, PhysicalModelLayer child) {
            IEnumerable<DiagnosticsNode> infoCollector() {
                yield return child.toDiagnosticsNode(name: "Attempted to composite layer",
                    style: DiagnosticsTreeStyle.errorProperty);
                yield return predecessor.toDiagnosticsNode(name: "after layer",
                    style: DiagnosticsTreeStyle.errorProperty);
                yield return new ErrorDescription("which occupies the same area at a higher elevation.");
            }
            
            UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                exception: new UIWidgetsError("Painting order is out of order with respect to elevation.\n" +
                                              "See https://api.flutter.dev/flutter/rendering/debugCheckElevationsEnabled.html " +
                                              "for more details."),
                library: "rendering library",
                context: new ErrorDescription("during compositing"),
                informationCollector: infoCollector
            ));
            
            return new List<PictureLayer> {
                _highlightConflictingLayer(predecessor),
                _highlightConflictingLayer(child)
            };
        }

        protected List<PictureLayer> _debugCheckElevations() {
            List<PhysicalModelLayer> physicalModelLayers =
                depthFirstIterateChildren().OfType<PhysicalModelLayer>().ToList();
            List<PictureLayer> addedLayers = new List<PictureLayer>();

            for (int i = 0; i < physicalModelLayers.Count; i++) {
                PhysicalModelLayer physicalModelLayer = physicalModelLayers[i];
                D.assert(physicalModelLayer.lastChild?.debugCreator != physicalModelLayer,
                    () => "debugCheckElevations has either already visited this layer or failed to remove the" +
                          " added picture from it.");
                float accumulatedElevation = physicalModelLayer.elevation.Value;
                Layer ancestor = physicalModelLayer.parent;
                while (ancestor != null) {
                    if (ancestor is PhysicalModelLayer modelLayer) {
                        accumulatedElevation += modelLayer.elevation.Value;
                    }

                    ancestor = ancestor.parent;
                }

                for (int j = 0; j <= i; j++) {
                    PhysicalModelLayer predecessor = physicalModelLayers[j];
                    float predecessorAccumulatedElevation = predecessor.elevation.Value;
                    ancestor = predecessor.parent;
                    while (ancestor != null) {
                        if (ancestor == predecessor) {
                            continue;
                        }

                        if (ancestor is PhysicalModelLayer modelLayer) {
                            predecessorAccumulatedElevation += modelLayer.elevation.Value;
                        }

                        ancestor = ancestor.parent;
                    }

                    if (predecessorAccumulatedElevation <= accumulatedElevation) {
                        continue;
                    }

                    Path intersection = Path.combine(
                        PathOperation.intersect,
                        predecessor._debugTransformedClipPath,
                        physicalModelLayer._debugTransformedClipPath);

                    if (intersection != null && intersection.computeMetrics().Any((metric) => metric.length > 0)) {
                        addedLayers.AddRange(_processConflictingPhysicalLayers(predecessor, physicalModelLayer));
                    }
                }
            }

            return addedLayers;
        }

        internal override void updateSubtreeNeedsAddToScene() {
            base.updateSubtreeNeedsAddToScene();
            Layer child = firstChild;
            while (child != null) {
                child.updateSubtreeNeedsAddToScene();
                _needsAddToScene = _needsAddToScene || child._needsAddToScene;
                child = child.nextSibling;
            }
        }
        

        public override bool findAnnotations<S>(
            AnnotationResult<S> result,
            Offset localPosition, 
            bool onlyFirst 
        ) {
            for (Layer child = lastChild; child != null; child = child.previousSibling) {
                bool isAbsorbed = child.findAnnotations<S>(result, localPosition, onlyFirst: onlyFirst);
                if (isAbsorbed)
                    return true;
                if (onlyFirst && result.entries.Any())
                    return isAbsorbed;
            }
            return false;
           
        }
        public AnnotationResult<S> findAllAnnotations<S>(Offset localPosition) {
            AnnotationResult<S> result = new AnnotationResult<S>();
            findAnnotations<S>(result, localPosition, onlyFirst: false);
            return result;
            
        }


        public override void attach(object owner) {
            base.attach(owner);

            var child = firstChild;
            while (child != null) {
                child.attach(owner);
                child = child.nextSibling;
            }
        }

        public override void detach() {
            base.detach();

            var child = firstChild;
            while (child != null) {
                child.detach();
                child = child.nextSibling;
            }
        }

        public void append(Layer child) {
            D.assert(child != this);
            D.assert(child != firstChild);
            D.assert(child != lastChild);
            D.assert(child.parent == null);
            D.assert(!child.attached);
            D.assert(child.nextSibling == null);
            D.assert(child.previousSibling == null);
            D.assert(() => {
                Layer node = this;
                while (node.parent != null) {
                    node = node.parent;
                }
                D.assert(node != child);
                return true;
            });
            adoptChild(child);
            child._previousSibling = lastChild;
            if (lastChild != null) {
                lastChild._nextSibling = child;
            }
            _lastChild = child;
            if (_firstChild == null) {
                _firstChild = child;
            }
            D.assert(child.attached == attached);
        }

        internal void _removeChild(Layer child) {
            D.assert(child.parent == this);
            D.assert(child.attached == attached);
            D.assert(_debugUltimatePreviousSiblingOf(child, equals: firstChild));
            D.assert(_debugUltimateNextSiblingOf(child, equals: lastChild));

            if (child._previousSibling == null) {
                D.assert(firstChild == child);
                _firstChild = child.nextSibling;
            }
            else {
                child._previousSibling._nextSibling = child.nextSibling;
            }

            if (child._nextSibling == null) {
                D.assert(lastChild == child);
                _lastChild = child.previousSibling;
            }
            else {
                child._nextSibling._previousSibling = child.previousSibling;
            }

            D.assert((firstChild == null) == (lastChild == null));
            D.assert(firstChild == null || firstChild.attached == attached);
            D.assert(lastChild == null || lastChild.attached == attached);
            D.assert(firstChild == null ||
                     _debugUltimateNextSiblingOf(firstChild, equals: lastChild));
            D.assert(lastChild == null ||
                     _debugUltimatePreviousSiblingOf(lastChild, equals: firstChild));

            child._nextSibling = null;
            child._previousSibling = null;
            dropChild(child);
            D.assert(!child.attached);
        }

        public void removeAllChildren() {
            Layer child = firstChild;
            while (child != null) {
                Layer next = child.nextSibling;
                child._previousSibling = null;
                child._nextSibling = null;
                D.assert(child.attached == attached);
                dropChild(child);
                child = next;
            }

            _firstChild = null;
            _lastChild = null;
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            addChildrenToScene(builder, layerOffset);
        }

        public void addChildrenToScene(SceneBuilder builder, Offset childOffset = null) {
            Layer child = firstChild;
            while (child != null) {
                if (childOffset == null || childOffset == Offset.zero) {
                    child._addToSceneWithRetainedRendering(builder);
                }
                else {
                    child.addToScene(builder, childOffset);
                }
                child = child.nextSibling;
            }
        }

        public virtual void applyTransform(Layer child, Matrix4 transform) {
            D.assert(child != null);
            D.assert(transform != null);
        }

        public List<Layer> depthFirstIterateChildren() {
            if (firstChild == null) {
                return new List<Layer>();
            }
            List<Layer> children = new List<Layer>();
            Layer child = firstChild;
            while (child != null) {
                children.Add(child);
                if (child is ContainerLayer containerLayer) {
                    children.AddRange(containerLayer.depthFirstIterateChildren());
                }

                child = child.nextSibling;
            }

            return children;
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            var children = new List<DiagnosticsNode>();
            if (firstChild == null) {
                return children;
            }
            Layer child = firstChild;
            int count = 1;
            while (true) {
                children.Add(child.toDiagnosticsNode(name: "child " + count));
                if (child == lastChild) {
                    break;
                }
                count += 1;
                child = child.nextSibling;
            }

            return children;
        }
    }
    
    public class ShaderMaskLayer : ContainerLayer {
        public ShaderMaskLayer(
            Shader shader = null,
            Rect maskRect = null,
            BlendMode blendMode = BlendMode.clear
        ) {
            _shader = shader;
            _maskRect = maskRect;
            _blendMode = blendMode;

        }

        public Shader shader {
            get {
                return _shader;
            }
            set{
                if (value != _shader) {
                    _shader = value;
                    markNeedsAddToScene();
                }
            }
        }
        Shader _shader;

        public Rect maskRect {
            get { return _maskRect; }
            set {
                if (value != _maskRect) {
                    _maskRect = value;
                    markNeedsAddToScene();
                }     
            }
        }
        Rect _maskRect;


        public BlendMode blendMode {
            get { return _blendMode; }
            set {
                if (value != _blendMode) {
                    _blendMode = value;
                    markNeedsAddToScene();
                }
            }
        }
        BlendMode _blendMode;

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            D.assert(shader != null);
            D.assert(maskRect != null);
            D.assert(layerOffset != null);
            Rect shiftedMaskRect = layerOffset == Offset.zero ? maskRect : maskRect.shift(layerOffset);
            engineLayer = builder.pushShaderMask(
              shader,
              shiftedMaskRect,
              blendMode,
              oldLayer: _engineLayer as ui.ShaderMaskEngineLayer
            );
            addChildrenToScene(builder, layerOffset);
            builder.pop();
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Shader>("shader", shader));
            properties.add(new DiagnosticsProperty<Rect>("maskRect", maskRect));
            properties.add(new DiagnosticsProperty<BlendMode>("blendMode", blendMode));
        }
    }

    public class OffsetLayer : ContainerLayer {
        public OffsetLayer(Offset offset = null) {
            _offset = offset ?? Offset.zero;
        }
        
        Offset _offset;
        public Offset offset {
            get { return _offset; }
            set {
                value = value ?? Offset.zero;
                if (value != _offset) {
                    _offset = value;
                    markNeedsAddToScene();
                }
            }
        }

        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition,  bool onlyFirst) {
            return base.findAnnotations<S>(result, localPosition - offset, onlyFirst: onlyFirst);
        }

        public override void applyTransform(Layer child, Matrix4 transform) {
            D.assert(child != null);
            D.assert(transform != null);
            transform.multiply(Matrix4.translationValues(offset.dx, offset.dy, 0.0f));
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;

            engineLayer = builder.pushOffset(
                (float) (layerOffset.dx + offset.dx),
                (float) (layerOffset.dy + offset.dy),
                oldLayer: engineLayer as OffsetEngineLayer);
            addChildrenToScene(builder);
            builder.pop();
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Offset>("offset", offset));
        }

        public Future<ui.Image> toImage(Rect bounds, float pixelRatio = 1.0f)// async
        {
            D.assert(bounds != null);
            ui.SceneBuilder builder = new SceneBuilder();
            Matrix4 transform = Matrix4.translationValues(
                (-bounds.left  - offset.dx) * pixelRatio,
                (-bounds.top - offset.dy) * pixelRatio,
                0.0f
            );
            transform.scale(pixelRatio, pixelRatio);
            builder.pushTransform(transform.storage);
            ui.Scene scene = buildScene(builder);

            try {
                // Size is rounded up to the next pixel to make sure we don't clip off
                // anything.
                return scene.toImage(
                    (pixelRatio * bounds.width).ceil(),
                    (pixelRatio * bounds.height).ceil()
                );
            } finally {
                scene.DisposePtr(scene._ptr);// ???
            }
        }
        

        
    }

    public class ClipRectLayer : ContainerLayer {
        public ClipRectLayer(
            Rect clipRect = null,
            Clip clipBehavior = Clip.hardEdge
        ) {
            D.assert(clipBehavior != Clip.none);
            _clipRect = clipRect;
            _clipBehavior = clipBehavior;
        }

        Rect _clipRect;
        public Rect clipRect {
            get { return _clipRect; }
            set {
                if (value != _clipRect) {
                    _clipRect = value;
                    markNeedsAddToScene();
                }
            }
        }

        Clip _clipBehavior;
        public Clip clipBehavior {
            get { return _clipBehavior; }
            set {
                D.assert(value != Clip.none);
                if (value != _clipBehavior) {
                    _clipBehavior = value;
                    markNeedsAddToScene();
                }
            }
        }
       
        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition,  bool onlyFirst ) {
            if (!clipRect.contains(localPosition))
                return false;
            return base.findAnnotations<S>(result, localPosition, onlyFirst: onlyFirst);
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            D.assert(clipRect != null);

            bool enabled = true;
            D.assert(() => {
                enabled = !D.debugDisableClipLayers;
                return true;
            });

            if (enabled) {
                var shiftedClipRect = layerOffset == Offset.zero ? clipRect : clipRect.shift(layerOffset);
                engineLayer = builder.pushClipRect(
                    rect: shiftedClipRect,
                    clipBehavior: clipBehavior,
                    oldLayer: engineLayer as ClipRectEngineLayer);
            }
            else {
                engineLayer = null;
            }

            addChildrenToScene(builder, layerOffset);

            if (enabled) {
                builder.pop();
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Rect>("clipRect", clipRect));
            properties.add(new DiagnosticsProperty<Clip>("clipBehavior", clipBehavior));
        }
    }

    public class ClipRRectLayer : ContainerLayer {
        public ClipRRectLayer(
            RRect clipRRect = null,
            Clip clipBehavior = Clip.antiAlias
        ) {
            D.assert(clipBehavior != Clip.none);
            _clipRRect = clipRRect;
            _clipBehavior = clipBehavior;
        }

        RRect _clipRRect; 
        public RRect clipRRect {
            get { return _clipRRect; }
            set {
                if (value != _clipRRect) {
                    _clipRRect = value;
                    markNeedsAddToScene();
                }
            }
        }

        Clip _clipBehavior;

        public Clip clipBehavior {
            get { return _clipBehavior; }
            set {
                D.assert(value != Clip.none);
                if (value != _clipBehavior) {
                    _clipBehavior = value;
                    markNeedsAddToScene();
                }
            }
        }

        public  override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition,  bool onlyFirst ) {
            if (!clipRRect.contains(localPosition))
                return false;
            return base.findAnnotations<S>(result, localPosition, onlyFirst: onlyFirst);
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            D.assert(clipRRect != null);
            bool enabled = true;
            D.assert(() => {
                enabled = !D.debugDisableClipLayers;
                return true;
            });

            if (enabled) {
                var shiftedClipRRect = layerOffset == Offset.zero ? clipRRect : clipRRect.shift(layerOffset);
                engineLayer = builder.pushClipRRect(
                    shiftedClipRRect,
                    clipBehavior: clipBehavior,
                    oldLayer: engineLayer as ClipRRectEngineLayer
                    );
            }
            else {
                engineLayer = null;
            }

            addChildrenToScene(builder, layerOffset);

            if (enabled) {
                builder.pop();
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<RRect>("clipRRect", clipRRect));
            properties.add(new DiagnosticsProperty<Clip>("clipBehavior", clipBehavior));
        }
    }

    public class ClipPathLayer : ContainerLayer {
        public ClipPathLayer(
            Path clipPath = null,
            Clip clipBehavior = Clip.antiAlias
        ) {
            D.assert(clipBehavior != Clip.none);
            _clipPath = clipPath;
            _clipBehavior = clipBehavior;
        }

        Path _clipPath;

        public Path clipPath {
            get { return _clipPath; }
            set {
                if (value != _clipPath) {
                    _clipPath = value;
                    markNeedsAddToScene();
                }
            }
        }

        Clip _clipBehavior;

        public Clip clipBehavior {
            get { return _clipBehavior; }
            set {
                D.assert(value != Clip.none);
                if (value != _clipBehavior) {
                    _clipBehavior = value;
                    markNeedsAddToScene();
                }
            }
        }

        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition,  bool onlyFirst ) {
            if (!clipPath.contains(localPosition))
                return false;
            return base.findAnnotations<S>(result, localPosition, onlyFirst: onlyFirst);
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            D.assert(clipPath != null);

            bool enabled = true;
            D.assert(() => {
                enabled = !D.debugDisableClipLayers;
                return true;
            });

            if (enabled) {
                var shiftedPath = layerOffset == Offset.zero ? clipPath : clipPath.shift(layerOffset);
                engineLayer = builder.pushClipPath(
                    shiftedPath,
                    clipBehavior: clipBehavior,
                    oldLayer: engineLayer as ClipPathEngineLayer);
            }
            else {
                engineLayer = null;
            }

            addChildrenToScene(builder, layerOffset);

            if (enabled) {
                builder.pop();
            }
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Clip>("clipBehavior", clipBehavior));
        }
    }
    
    public class ColorFilterLayer : ContainerLayer {

        public ColorFilterLayer(ColorFilter colorFilter = null) {
            _colorFilter = colorFilter;
        }


        public ColorFilter colorFilter {
            get {
                return _colorFilter;
            }
            set {
                D.assert(value != null);
                if (value != _colorFilter) {
                    _colorFilter = value;
                    markNeedsAddToScene();
                }
            }
        }

        ColorFilter _colorFilter;
        
        
        public override void addToScene(ui.SceneBuilder builder, Offset layerOffset = null ) {
            D.assert(colorFilter != null);
            engineLayer =  builder.pushColorFilter(
                colorFilter,
                oldLayer: _engineLayer as ui.ColorFilterEngineLayer
            );
            addChildrenToScene(builder, layerOffset);
            builder.pop();
        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ColorFilter>("colorFilter", colorFilter));
        }
    }
    
    public class ImageFilterLayer : ContainerLayer {

        public ImageFilterLayer(
            ui.ImageFilter imageFilter
        ) {
            _imageFilter = imageFilter;
        }
        
    public ui.ImageFilter  imageFilter {
        get {
            return _imageFilter;
        }
        set {
            D.assert(value != null);
            if (value != _imageFilter) {
                _imageFilter = value;
                markNeedsAddToScene();
            }
        }
    }

    ui.ImageFilter _imageFilter;
    
    public override void addToScene(ui.SceneBuilder builder,  Offset layerOffset = null) {
        D.assert(imageFilter != null);
        engineLayer = builder.pushImageFilter(
            imageFilter,
            oldLayer: _engineLayer as ui.ImageFilterEngineLayer
        );
        addChildrenToScene(builder, layerOffset);
        builder.pop();
    }
    
    public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
        base.debugFillProperties(properties);
        properties.add(new DiagnosticsProperty<ui.ImageFilter>("imageFilter", imageFilter));
    }
    }
    
    public class TransformLayer : OffsetLayer {
        public TransformLayer(Matrix4 transform = null, Offset offset = null) : base(offset) {
            offset = offset ?? Offset.zero;
            _transform = transform ?? Matrix4.identity();
        }

        public Matrix4 transform {
            get { return _transform; }
            set {
                D.assert(value != null);
                bool componentIsFinite = true;
                foreach (var component in value.storage) {
                    if (!component.isFinite()) {
                        componentIsFinite = false;
                        break;
                    }
                }
                D.assert(componentIsFinite);
                if (value == _transform)
                    return;
                _transform = value;
                _inverseDirty = true;
                markNeedsAddToScene();
            }

        }
        Matrix4 _transform;
        
        Matrix4 _lastEffectiveTransform;
        Matrix4 _invertedTransform;
        
        bool _inverseDirty = true;

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            D.assert(transform != null);
            _lastEffectiveTransform = _transform;

            var totalOffset = offset + layerOffset;
            if (totalOffset != Offset.zero) {
                _lastEffectiveTransform = Matrix4.translationValues(totalOffset.dx, totalOffset.dy, 0.0f);
                _lastEffectiveTransform.multiply(transform);
            }

            engineLayer = builder.pushTransform(
                _lastEffectiveTransform.storage,
                oldLayer: engineLayer as TransformEngineLayer);
            
            addChildrenToScene(builder);
            builder.pop();
        }
        Offset _transformOffset(Offset localPosition) {
            if (_inverseDirty) {
                _invertedTransform = Matrix4.tryInvert(
                    PointerEvent.removePerspectiveTransform(transform)
                );
                _inverseDirty = false;
            }
            if (_invertedTransform == null)
                return null;

            return MatrixUtils.transformPoint(_invertedTransform, localPosition);
        }
        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition,  bool onlyFirst ) {
            Offset transformedOffset = _transformOffset(localPosition);
            if (transformedOffset == null)
                return false;
            return base.findAnnotations<S>(result, transformedOffset, onlyFirst: onlyFirst);
        }

        public override void applyTransform(Layer child, Matrix4 transform) {
            D.assert(child != null);
            D.assert(transform != null);
            D.assert(_lastEffectiveTransform != null || this.transform != null);
            if (_lastEffectiveTransform == null) {
                transform.multiply(this.transform);
            }
            else {
                transform.multiply(_lastEffectiveTransform);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Matrix4>("transform", transform));
        }
    }

    public class OpacityLayer : ContainerLayer {
        public OpacityLayer(int alpha = 255, Offset offset = null) {
            _alpha = alpha;
            _offset = offset ?? Offset.zero;
        }

        int _alpha;
        public int alpha {
            get { return _alpha; }
            set {
                if (value != _alpha) {
                    _alpha = value;
                    markNeedsAddToScene();
                }
            }
        }

        Offset _offset;
        public Offset offset {
            get { return _offset; }
            set {
                value = value ?? Offset.zero;
                if (value != _offset) {
                    _offset = value;
                    markNeedsAddToScene();
                }
            }
        }
        public override void applyTransform(Layer child, Matrix4 transform) {
            D.assert(child != null);
            D.assert(transform != null);
            transform.translate(offset.dx, offset.dy);
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            bool enabled = firstChild != null;
            D.assert(() => {
                enabled = enabled && !D.debugDisableOpacityLayers;
                return true;
            });
            
            if (enabled) {
                engineLayer = builder.pushOpacity(
                    alpha, 
                    offset: offset + layerOffset,
                    oldLayer: engineLayer as OpacityEngineLayer);
            }
            else {
                engineLayer = null;
            }

            addChildrenToScene(builder, layerOffset);
            if (enabled) {
                builder.pop();
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new IntProperty("alpha", alpha));
            properties.add(new DiagnosticsProperty<Offset>("offset", offset));
        }
    }

    public class BackdropFilterLayer : ContainerLayer {
        public BackdropFilterLayer(ImageFilter filter = null) {
            D.assert(filter != null);
            _filter = filter;
        }

        ImageFilter _filter;
        public ImageFilter filter {
            get { return _filter; }
            set {
                if (value != _filter) {
                    _filter = value;
                    markNeedsAddToScene();
                }
            }
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            D.assert(filter != null);
            engineLayer = builder.pushBackdropFilter(
                filter: filter,
                oldLayer: engineLayer as BackdropFilterEngineLayer);
            
            addChildrenToScene(builder, layerOffset);
            builder.pop();
        }
    }

    public class LayerLink {
        public LeaderLayer leader {
            get { return _leader; }
        }
        internal LeaderLayer _leader;

        public override string ToString() {
            return $"{foundation_.describeIdentity(this)}({(_leader != null ? "<linked>" : "<dangling>")})";
        }
    }

    public class LeaderLayer : ContainerLayer {
        public LeaderLayer(LayerLink link, Offset offset = null) {
            D.assert(link != null);
            offset = offset ?? Offset.zero;
            _link = link;
            this.offset = offset;
        }

        public LayerLink link {
            get { return _link; }
            set {
                D.assert(value != null);
                _link = value;
            }

        }
        LayerLink _link;
        public Offset offset;

        protected override bool alwaysNeedsAddToScene {
            get { return true; }
        }

        public override void attach(object owner) {
            base.attach(owner);
            D.assert(link.leader == null);
            _lastOffset = null;
            link._leader = this;
        }

        public override void detach() {
            D.assert(link.leader == this);
            link._leader = null;
            _lastOffset = null;
            base.detach();
        }

        internal Offset _lastOffset;

        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition, bool onlyFirst ) {
            return base.findAnnotations<S>(result, localPosition - offset, onlyFirst: onlyFirst);
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
           
            D.assert(offset != null);
            _lastOffset = offset + layerOffset;
            if (_lastOffset != Offset.zero) {
                engineLayer = builder.pushTransform(
                     Matrix4.translationValues(_lastOffset.dx, _lastOffset.dy,0.0f).storage,
                    oldLayer: engineLayer as TransformEngineLayer);
            }

            addChildrenToScene(builder, Offset.zero);
            if (_lastOffset != Offset.zero) {
                builder.pop();
            }
        }

        public override void applyTransform(Layer child, Matrix4 transform) {
            D.assert(_lastOffset != null);
            if (_lastOffset != Offset.zero) {
                transform.translate(_lastOffset.dx, _lastOffset.dy);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Offset>("offset", offset));
            properties.add(new DiagnosticsProperty<LayerLink>("link", link));
        }
    }

    public class FollowerLayer : ContainerLayer {
        public FollowerLayer(
            LayerLink link = null,
            bool showWhenUnlinked = true,
            Offset unlinkedOffset = null,
            Offset linkedOffset = null
        ) {
            D.assert(link != null);
            _link = link;
            this.showWhenUnlinked = showWhenUnlinked;
            this.unlinkedOffset = unlinkedOffset ?? Offset.zero;
            this.linkedOffset = linkedOffset ?? Offset.zero;
        }

        public LayerLink link {
            get { return _link; }
            set {
                D.assert(value != null);
                _link = value;
            }
        }
        LayerLink _link;
        
        public bool showWhenUnlinked;
        public Offset unlinkedOffset;
        public Offset linkedOffset;

        Offset _lastOffset;
        Matrix4 _lastTransform;
        Matrix4 _invertedTransform = Matrix4.identity();
        bool _inverseDirty = true;
        
        Offset _transformOffset<S>(Offset localPosition) {
            if (_inverseDirty) {
                _invertedTransform = Matrix4.tryInvert(getLastTransform());
                _inverseDirty = false;
            }
            if (_invertedTransform == null)
                return null;
            Vector4 vector = new Vector4(localPosition.dx, localPosition.dy, 0.0f, 1.0f);
            Vector4 result = _invertedTransform.transform(vector);
            return new Offset(result[0] - linkedOffset.dx, result[1] - linkedOffset.dy);
        }

        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition,  bool onlyFirst ) {
            if (link.leader == null) {
                if (showWhenUnlinked) {
                    return base.findAnnotations(result, localPosition - unlinkedOffset, onlyFirst: onlyFirst);
                }
                return false;
            }
            Offset transformedOffset = _transformOffset<S>(localPosition);
            if (transformedOffset == null) {
                return false;
            }
            return base.findAnnotations<S>(result, transformedOffset, onlyFirst: onlyFirst);
        }

        public Matrix4 getLastTransform() {
            if (_lastTransform == null) {
                return null;
            }

            Matrix4 result = Matrix4.translationValues(-_lastOffset.dx, -_lastOffset.dy,0.0f );
            result.multiply(_lastTransform);
            return result;
        }

        Matrix4 _collectTransformForLayerChain(List<ContainerLayer> layers) {
            Matrix4 result = Matrix4.identity();
            for (int index = layers.Count - 1; index > 0; index -= 1) {
                layers[index].applyTransform(layers[index - 1], result);
            }

            return result;
        }

        void _establishTransform() {
            D.assert(link != null);
            _lastTransform = null;
            if (link._leader == null) {
                return;
            }

            D.assert(link.leader.owner == owner,
                () => "Linked LeaderLayer anchor is not in the same layer tree as the FollowerLayer.");
            D.assert(link.leader._lastOffset != null,
                () => "LeaderLayer anchor must come before FollowerLayer in paint order, but the reverse was true.");

            HashSet<Layer> ancestors = new HashSet<Layer>();
            Layer ancestor = parent;
            while (ancestor != null) {
                ancestors.Add(ancestor);
                ancestor = ancestor.parent;
            }

            ContainerLayer layer = link.leader;
            List<ContainerLayer> forwardLayers = new List<ContainerLayer> {null, layer};
            do {
                layer = layer.parent;
                forwardLayers.Add(layer);
            } while (!ancestors.Contains(layer));

            ancestor = layer;

            layer = this;
            List<ContainerLayer> inverseLayers = new List<ContainerLayer> {layer};
            do {
                layer = layer.parent;
                inverseLayers.Add(layer);
            } while (layer != ancestor);

            Matrix4 forwardTransform = _collectTransformForLayerChain(forwardLayers);
            Matrix4 inverseTransform = _collectTransformForLayerChain(inverseLayers);
            if (inverseTransform.invert() == 0) {
                return;
            }

            inverseTransform.multiply(forwardTransform);
            inverseTransform.translate(linkedOffset.dx, linkedOffset.dy);
            _lastTransform = inverseTransform;
            _inverseDirty = true;
        }

        protected override bool alwaysNeedsAddToScene {
            get { return true; }
        }


        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            D.assert(link != null);
            D.assert(link != null);
            if (link.leader == null && !showWhenUnlinked) {
                _lastTransform = null;
                _lastOffset = null;
                _inverseDirty = true;
                engineLayer = null;
                return;
                
            }

            _establishTransform();
            if (_lastTransform != null) {
                engineLayer = builder.pushTransform(
                    _lastTransform.storage,
                    oldLayer: engineLayer as TransformEngineLayer);
                addChildrenToScene(builder);
                builder.pop();
                _lastOffset = unlinkedOffset + layerOffset;
            }
            else {
                _lastOffset = null;
                var matrix = Matrix4.translationValues(unlinkedOffset.dx, unlinkedOffset.dy, 0.0f);
                engineLayer = builder.pushTransform(
                    matrix.storage,
                    oldLayer: engineLayer as TransformEngineLayer);
                addChildrenToScene(builder);
                builder.pop();
            }

            _inverseDirty = true;
        }

        public override void applyTransform(Layer child, Matrix4 transform) {
            D.assert(child != null);
            D.assert(transform != null);
            if (_lastTransform != null) {
                transform.multiply(_lastTransform);
            }
            else {
                transform.multiply(Matrix4.translationValues(unlinkedOffset.dx, unlinkedOffset.dy, 0.0f));
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<LayerLink>("link", link));
            properties.add(new TransformProperty("transform", getLastTransform(),
                defaultValue: foundation_.kNullDefaultValue));
        }
    }
    
    /*public class PlatformViewLayer : Layer {
        public PlatformViewLayer(
            Rect rect = null,
            int viewId = default,
            MouseTrackerAnnotation hoverAnnotation = null
        ) {
            D.assert(rect != null);
        }
        
        public readonly Rect rect;

  
        public readonly int viewId;
  
        public readonly MouseTrackerAnnotation hoverAnnotation;

  
        public override void addToScene(ui.SceneBuilder builder,  Offset layerOffset = null) {
            Rect shiftedRect = layerOffset == Offset.zero ? rect : rect.shift(layerOffset);
            builder.addPlatformView(
                viewId,
                offset: shiftedRect.topLeft,
                width: shiftedRect.width,
                height: shiftedRect.height
            );
        }

  
        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition, bool onlyFirst ) {
            if (hoverAnnotation == null || !rect.contains(localPosition)) {
                return false;
            }
            if (typeof(S) == typeof(MouseTrackerAnnotation)) {
                Object untypedValue = hoverAnnotation;
                S typedValue = (S)untypedValue;
                result.add(new AnnotationEntry<S>(
                    annotation: typedValue,
                    localPosition: localPosition
                ));
                return true;
            }
            return false;
        }
    }*/

    public class PerformanceOverlayLayer : Layer {
        public PerformanceOverlayLayer(
            Rect overlayRect = null,
            int optionsMask = default,
            int rasterizerThreshold = default,
            bool checkerboardRasterCacheImages = default,
            bool checkerboardOffscreenLayers = default
            
        ) {
            _overlayRect = overlayRect;
            this.optionsMask = optionsMask ;
            this.rasterizerThreshold = rasterizerThreshold ;
            this.checkerboardOffscreenLayers = checkerboardRasterCacheImages;
            this.checkerboardRasterCacheImages = checkerboardOffscreenLayers;
        }

        public Rect overlayRect {
            get { return _overlayRect; }
            set {
                if (value != _overlayRect) {
                    _overlayRect = value;
                    markNeedsAddToScene();
                }
            }
        }
        Rect _overlayRect;

        public readonly int optionsMask;
        public readonly int rasterizerThreshold;
        public readonly bool checkerboardRasterCacheImages;
        public readonly bool checkerboardOffscreenLayers;

        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition,  bool onlyFirst ) {
            return false;
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            
            var shiftedOverlayRect = layerOffset == Offset.zero ? overlayRect : overlayRect.shift(layerOffset);
            builder.addPerformanceOverlay(optionsMask, shiftedOverlayRect);
            //TODO: add implementations
            //builder.setRasterizerTracingThreshold(rasterizerThreshold);
            //builder.setCheckerboardRasterCacheImages(checkerboardRasterCacheImages);
            //builder.setCheckerboardOffscreenLayers(checkerboardOffscreenLayers);
        }
    }

    public class AnnotatedRegionLayer<T> : ContainerLayer
        where T : class {
        public AnnotatedRegionLayer(
            T value = null,
            Size size = null,
            Offset offset = null,
            bool opaque = false
            ) {
            offset = offset ?? Offset.zero;
            D.assert(value != null);
            this.value = value;
            this.size = size;
            this.offset = offset;
            this.opaque = opaque;
        }

        public readonly T value;

        public readonly Size size;

        public readonly Offset offset;

        public readonly bool opaque;

        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition, bool onlyFirst ) {
            bool isAbsorbed = base.findAnnotations(result, localPosition, onlyFirst: onlyFirst);
            if (result.entries.Any() && onlyFirst)
                return isAbsorbed;
            if (size != null && !(offset & size).contains(localPosition)) {
                return isAbsorbed;
            }
            if (typeof(T) == typeof(S)) {
                isAbsorbed = isAbsorbed || opaque;
                object untypedValue = value;
                S typedValue = (S)untypedValue;
                result.add(new AnnotationEntry<S>(
                    annotation: typedValue,
                    localPosition: localPosition - offset
                ));
            }
            return isAbsorbed;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<T>("value", value));
            properties.add(new DiagnosticsProperty<Size>("size", size, defaultValue: null));
            properties.add(new DiagnosticsProperty<Offset>("offset", offset, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("opaque", opaque, defaultValue: false));
        }
        
    }


    public class PhysicalModelLayer : ContainerLayer {
        public PhysicalModelLayer(
            Path clipPath = null,
            Clip clipBehavior = Clip.none,
            float? elevation = null,
            Color color = null,
            Color shadowColor = null) {
            _clipPath = clipPath;
            _clipBehavior = clipBehavior;
            _elevation = elevation;
            _color = color;
            this.shadowColor = shadowColor;
        }

        public Path clipPath { 
            get { return _clipPath; }
            set {
                if (value != _clipPath) {
                    _clipPath = value;
                    markNeedsAddToScene();
                }
            }
        }
        Path _clipPath;
        internal Path _debugTransformedClipPath {
            get {
                ContainerLayer ancestor = parent;
                Matrix4 matrix = Matrix4.identity();
                while (ancestor != null && ancestor.parent != null) {
                    ancestor.applyTransform(this, matrix);
                    ancestor = ancestor.parent;
                }

                return clipPath.transform(matrix._m4storage);
            }
        }
        
        public Clip clipBehavior {
            get { return _clipBehavior; }
            set {
                if (value != _clipBehavior) {
                    _clipBehavior = value;
                    markNeedsAddToScene();
                }
            }
        }
        Clip _clipBehavior;

        public float? elevation {
            get { return _elevation; }
            set {
                if (value != _elevation) {
                    _elevation = value;
                    markNeedsAddToScene();
                }
            }
        }
        float? _elevation;

        public Color color {
            get { return _color; }
            set {
                if (value != _color) {
                    _color = value;
                    markNeedsAddToScene();
                }
            }
        }
        Color _color;

        public Color shadowColor {
            get { return _shadowColor; }
            set {
                if (value != _shadowColor) {
                    _shadowColor = value;
                    markNeedsAddToScene();
                }
            }
        }
        Color _shadowColor;

        public override bool findAnnotations<S>(AnnotationResult<S> result, Offset localPosition,  bool onlyFirst ) {
            if (!clipPath.contains(localPosition))
                return false;
            return base.findAnnotations<S>(result, localPosition, onlyFirst: onlyFirst);
        }

        public override void addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            
            D.assert(clipPath != null);
            D.assert(color != null);
            D.assert(elevation != null);
            D.assert(shadowColor != null);

            bool enabled = true;
            D.assert(() => {
                enabled = !D.debugDisablePhysicalShapeLayers;
                return true;
            });

            if (enabled) {
                engineLayer = builder.pushPhysicalShape(
                    path: layerOffset == Offset.zero ? clipPath : clipPath.shift(layerOffset),
                    elevation: elevation.Value,
                    color: color,
                    shadowColor: shadowColor,
                    clipBehavior: clipBehavior,
                    oldLayer: engineLayer as PhysicalShapeEngineLayer);
            }
            else {
                engineLayer = null;
            }
            addChildrenToScene(builder, layerOffset);
            if (enabled) {
                builder.pop();
            }
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("elevation", elevation));
            properties.add(new ColorProperty("color", color));
        }
    }
}