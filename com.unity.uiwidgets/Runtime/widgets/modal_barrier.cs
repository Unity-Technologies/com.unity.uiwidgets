using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.widgets {
    public class ModalBarrier : StatelessWidget {
        public readonly Color color;
        public readonly bool dismissible;

        public ModalBarrier(
            Key key = null, 
            Color color = null, 
            bool dismissible = true) : base(key) {
            this.color = color;
            this.dismissible = dismissible;
        }

        public override Widget build(BuildContext context) {
            return new _ModalBarrierGestureDetector(
                onDismiss: () => {
                    if (dismissible) {
                        Navigator.maybePop<object>(context);
                    }
                },
                child: new MouseRegion(
                    opaque: true,
                    child: new ConstrainedBox(
                        constraints: BoxConstraints.expand(),
                        child: color == null ? null : new DecoratedBox(
                            decoration: new BoxDecoration(
                            color: color
                            )
                        )
                    )
                )
            );
        }
    }

    public class AnimatedModalBarrier : AnimatedWidget {
        public readonly bool dismissible;

        public AnimatedModalBarrier(
            Key key = null, 
            Animation<Color> color = null,
            bool dismissible = true
            ) : base(key, color) {
            this.dismissible = dismissible;
        }

        public Animation<Color> color {
            get { return (Animation<Color>) listenable; }
        }

        protected internal override Widget build(BuildContext context) {
            return new ModalBarrier(
                color: color?.value, 
                dismissible: dismissible);
        }
    }
    
    public class _AnyTapGestureRecognizer : BaseTapGestureRecognizer {
        public _AnyTapGestureRecognizer( object debugOwner = null) : base(debugOwner: debugOwner) {}

        public VoidCallback onAnyTapUp;
        
        protected override bool isPointerAllowed(PointerDownEvent _event ) {
            if (onAnyTapUp == null)
              return false;
            return base.isPointerAllowed(_event);
        }

        protected override void handleTapDown(PointerDownEvent down = null) {
            // Do nothing.
        }

        protected override void handleTapUp(PointerDownEvent down = null, PointerUpEvent up = null) {
            if (onAnyTapUp != null)
              onAnyTapUp();
        }

        protected override void handleTapCancel(PointerDownEvent down = null, PointerCancelEvent cancel = null, string reason = null) {
            // Do nothing.
        }

        public override string debugDescription {
          get {
              return "any tap";
          }
        }
    }
    
    public class _AnyTapGestureRecognizerFactory : GestureRecognizerFactory<_AnyTapGestureRecognizer> {
        public _AnyTapGestureRecognizerFactory(VoidCallback onAnyTapUp = null) {
            this.onAnyTapUp = onAnyTapUp;
        }

        public readonly VoidCallback onAnyTapUp;

        public override _AnyTapGestureRecognizer constructor() => new _AnyTapGestureRecognizer();

        public override void initializer(_AnyTapGestureRecognizer instance) {
            instance.onAnyTapUp = onAnyTapUp;
        }
    }
    
    public class _ModalBarrierGestureDetector : StatelessWidget {
        public _ModalBarrierGestureDetector(
            Key key = null,
            Widget child = null,
            VoidCallback onDismiss = null
        ) : base(key: key) {
            D.assert(child != null);
            D.assert(onDismiss != null);
            this.child = child;
            this.onDismiss = onDismiss;
        }
        
        public readonly Widget child;
        
        public readonly VoidCallback onDismiss;

        public override Widget build(BuildContext context) { 
            Dictionary<Type, GestureRecognizerFactory> gestures = new Dictionary<Type, GestureRecognizerFactory>(){
              {typeof(_AnyTapGestureRecognizer), new _AnyTapGestureRecognizerFactory(onAnyTapUp: onDismiss)}
            };

            return new RawGestureDetector(
                gestures: gestures,
                behavior: HitTestBehavior.opaque,
                child: child
            );
        }
}
    
    
}