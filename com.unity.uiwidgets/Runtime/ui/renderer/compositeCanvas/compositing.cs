using System;
using Unity.UIWidgets.flow;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.uiOld{
    public class SceneBuilder {
        ContainerLayer _rootLayer;
        ContainerLayer _currentLayer;

        public SceneBuilder() {
        }

        public Scene build() {
            return new Scene(_rootLayer);
        }

        void _pushLayer(ContainerLayer layer) {
            if (_rootLayer == null) {
                _rootLayer = layer;
                _currentLayer = layer;
                return;
            }

            if (_currentLayer == null) {
                return;
            }

            _currentLayer.add(layer);
            _currentLayer = layer;
        }

        public Layer pushTransform(Matrix3 matrix) {
            var layer = new TransformLayer();
            layer.transform = matrix;
            _pushLayer(layer);
            return layer;
        }

        public Layer pushOffset(float dx, float dy) {
            var layer = new TransformLayer();
            layer.transform = Matrix3.makeTrans(dx, dy);
            _pushLayer(layer);
            return layer;
        }

        public Layer pushClipRect(Rect clipRect) {
            var layer = new ClipRectLayer();
            layer.clipRect = clipRect;
            _pushLayer(layer);
            return layer;
        }

        public Layer pushClipRRect(RRect clipRRect) {
            var layer = new ClipRRectLayer();
            layer.clipRRect = clipRRect;
            _pushLayer(layer);
            return layer;
        }

        public Layer pushClipPath(Path clipPath) {
            var layer = new ClipPathLayer();
            layer.clipPath = clipPath;
            _pushLayer(layer);
            return layer;
        }

        public Layer pushOpacity(int alpha, Offset offset = null) {
            offset = offset ?? Offset.zero;

            var layer = new OpacityLayer();
            layer.alpha = alpha;
            layer.offset = offset;
            _pushLayer(layer);
            return layer;
        }

        public Layer pushBackdropFilter(ImageFilter filter) {
            var layer = new BackdropFilterLayer();
            layer.filter = filter;
            _pushLayer(layer);
            return layer;
        }


        public void addRetained(Layer layer) {
            if (_currentLayer == null) {
                return;
            }

            _currentLayer.add(layer);
        }

        public void pop() {
            if (_currentLayer == null) {
                return;
            }

            _currentLayer = _currentLayer.parent;
        }

        public void addPicture(Offset offset, Picture picture,
            bool isComplexHint = false, bool willChangeHint = false) {
            D.assert(offset != null);
            D.assert(picture != null);

            if (_currentLayer == null) {
                return;
            }

            var layer = new PictureLayer();
            layer.offset = offset;
            layer.picture = picture;
            layer.isComplex = isComplexHint;
            layer.willChange = willChangeHint;
            _currentLayer.add(layer);
        }

        public void addTexture(
            Texture texture,
            Offset offset,
            float width,
            float height,
            bool freeze) {
            if (_currentLayer == null) {
                return;
            }

            var layer = new TextureLayer();
            layer.offset = offset;
            layer.size = new Size(width, height);
            layer.texture = texture;
            layer.freeze = freeze;
            _currentLayer.add(layer);
        }

        public void addPerformanceOverlay(int enabledOptions, Rect bounds) {
            if (_currentLayer == null) {
                return;
            }

            var layer = new PerformanceOverlayLayer(enabledOptions);
            layer.paintBounds = Rect.fromLTRB(
                bounds.left,
                bounds.top,
                bounds.right,
                bounds.bottom
            );
            _currentLayer.add(layer);
        }

        public Layer pushPhysicalShape(Path path, float elevation, Color color, Color shadowColor, Clip clipBehavior) {
            var layer = new PhysicalShapeLayer(clipBehavior);
            layer.path = path;
            layer.elevation = elevation;
            layer.color = color;
            layer.shadowColor = shadowColor;
            layer.devicePixelRatio = Window.instance.devicePixelRatio;

            _pushLayer(layer);
            return layer;
        }
    }

    public class Scene : IDisposable {
        public Scene(Layer rootLayer) {
            _layerTree = new LayerTree();
            _layerTree.rootLayer = rootLayer;
        }

        readonly LayerTree _layerTree;

        public LayerTree takeLayerTree() {
            return _layerTree;
        }

        public void Dispose() {
        }
    }
}