using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class ImageFiltered : SingleChildRenderObjectWidget {
    
        public ImageFiltered(
            Key key = null,
            ImageFilter imageFilter = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(imageFilter != null);
            this.imageFilter = imageFilter;
        }
    
        public readonly ImageFilter imageFilter;

        public override RenderObject createRenderObject(BuildContext context) {
                return new _ImageFilterRenderObject(imageFilter);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((_ImageFilterRenderObject) renderObject).imageFilter = imageFilter;
        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ImageFilter>("imageFilter", imageFilter));
        }
    }

    public class _ImageFilterRenderObject : RenderProxyBox {
        public _ImageFilterRenderObject(ImageFilter _imageFilter) {
            this._imageFilter = _imageFilter;
        }

        public ImageFilter imageFilter {
            get {
                return _imageFilter;
            }
            set {
                D.assert(value != null);
                if (value != _imageFilter) {
                    _imageFilter = value;
                    markNeedsPaint();
                }
            }
        }
        ImageFilter _imageFilter;
        
        protected override bool alwaysNeedsCompositing {
            get { return child != null; }
        }
        
        public override void paint(PaintingContext context, Offset offset) {
            D.assert(imageFilter != null);
            if (layer == null) {
                layer = new ImageFilterLayer(imageFilter: imageFilter);
            } else {
                ImageFilterLayer filterLayer = layer as ImageFilterLayer;
                filterLayer.imageFilter = imageFilter;
            }
            context.pushLayer(layer, base.paint, offset);
            D.assert(layer != null);
        }
    }

}