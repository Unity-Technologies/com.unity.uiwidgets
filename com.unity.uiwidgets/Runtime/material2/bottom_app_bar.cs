using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class BottomAppBar : StatefulWidget {
        public BottomAppBar(
            Key key = null,
            Color color = null,
            float? elevation = null,
            NotchedShape shape = null,
            Clip clipBehavior = Clip.none,
            float notchMargin = 4.0f,
            Widget child = null
        ) : base(key: key) {
            D.assert(elevation == null || elevation >= 0.0f);
            this.child = child;
            this.color = color;
            this.elevation = elevation;
            this.shape = shape;
            this.clipBehavior = clipBehavior;
            this.notchMargin = notchMargin;
        }

        public readonly Widget child;

        public readonly Color color;

        public readonly float? elevation;

        public readonly NotchedShape shape;

        public readonly Clip clipBehavior;

        public readonly float notchMargin;

        public override State createState() {
            return new _BottomAppBarState();
        }
    }

    class _BottomAppBarState : State<BottomAppBar> {
        ValueListenable<ScaffoldGeometry> geometryListenable;
        const float _defaultElevation = 8.0f;

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            geometryListenable = Scaffold.geometryOf(context);
        }

        public override Widget build(BuildContext context) {
            BottomAppBarTheme babTheme = BottomAppBarTheme.of(context);
            NotchedShape notchedShape = widget.shape ?? babTheme.shape;
            CustomClipper<Path> clipper = notchedShape != null
                ? (CustomClipper<Path>) new _BottomAppBarClipper(
                    geometry: geometryListenable,
                    shape: notchedShape,
                    notchMargin: widget.notchMargin
                )
                : new ShapeBorderClipper(shape: new RoundedRectangleBorder());
            return new PhysicalShape(
                clipper: clipper,
                elevation: widget.elevation ?? babTheme.elevation ?? _defaultElevation,
                color: widget.color ?? babTheme.color ?? Theme.of(context).bottomAppBarColor,
                clipBehavior: widget.clipBehavior,
                child: new Material(
                    type: MaterialType.transparency,
                    child: widget.child == null
                        ? null
                        : new SafeArea(child: widget.child)
                )
            );
        }
    }

    class _BottomAppBarClipper : CustomClipper<Path> {
        public _BottomAppBarClipper(
            ValueListenable<ScaffoldGeometry> geometry,
            NotchedShape shape,
            float notchMargin
        ) : base(reclip: geometry) {
            D.assert(geometry != null);
            D.assert(shape != null);
            this.geometry = geometry;
            this.shape = shape;
            this.notchMargin = notchMargin;
        }

        public readonly ValueListenable<ScaffoldGeometry> geometry;
        public readonly NotchedShape shape;
        public readonly float notchMargin;

        public override Path getClip(Size size) {
            Rect button = geometry.value.floatingActionButtonArea?.translate(
                0.0f,
                (geometry.value.bottomNavigationBarTop ?? 0.0f) * -1.0f
            );
            return shape.getOuterPath(Offset.zero & size, button?.inflate(notchMargin));
        }

        public override bool shouldReclip(CustomClipper<Path> _oldClipper) {
            _BottomAppBarClipper oldClipper = _oldClipper as _BottomAppBarClipper;
            return oldClipper.geometry != geometry
                   || oldClipper.shape != shape
                   || oldClipper.notchMargin != notchMargin;
        }
    }
}