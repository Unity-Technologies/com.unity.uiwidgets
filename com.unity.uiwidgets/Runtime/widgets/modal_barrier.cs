using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class ModalBarrier : StatelessWidget {
        public readonly Color color;
        public readonly bool dismissible;

        public ModalBarrier(Key key = null, Color color = null, bool dismissible = true) : base(key) {
            this.color = color;
            this.dismissible = dismissible;
        }

        public override Widget build(BuildContext context) {
            return new GestureDetector(
                onTapDown: details => {
                    if (dismissible) {
                        Navigator.maybePop<object>(context);
                    }
                },
                behavior: HitTestBehavior.opaque,
                child: new ConstrainedBox(
                    constraints: BoxConstraints.expand(),
                    child: color == null ? null : new DecoratedBox(decoration: new BoxDecoration(color))
                )
            );
        }
    }

    public class AnimatedModalBarrier : AnimatedWidget {
        public readonly bool dismissible;

        public AnimatedModalBarrier(Key key = null, Animation<Color> color = null,
            bool dismissible = true) : base(key, color) {
            this.dismissible = dismissible;
        }

        public Animation<Color> color {
            get { return (Animation<Color>) listenable; }
        }

        protected internal override Widget build(BuildContext context) {
            return new ModalBarrier(color: color?.value, dismissible: dismissible);
        }
    }
}