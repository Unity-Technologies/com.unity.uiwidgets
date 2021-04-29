using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public abstract class StatusTransitionWidget : StatefulWidget {
        protected StatusTransitionWidget(
            Animation<float> animation,
            Key key = null
        ) : base(key: key) {
            D.assert(animation != null);
            this.animation = animation;
        }

        public readonly Animation<float> animation;

        public abstract Widget build(BuildContext context);
        public override State createState() => new _StatusTransitionState();
    }
    public class _StatusTransitionState : State<StatusTransitionWidget> {
        public override void initState() {
            base.initState();
            widget.animation.addStatusListener(_animationStatusChanged);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (StatusTransitionWidget)oldWidget;
            base.didUpdateWidget(oldWidget);
            if (widget.animation != ((StatusTransitionWidget)oldWidget).animation) {
                ((StatusTransitionWidget)oldWidget).animation.removeStatusListener(_animationStatusChanged);
                widget.animation.addStatusListener(_animationStatusChanged);
            }
        }

        public override void dispose() {
            widget.animation.removeStatusListener(_animationStatusChanged);
            base.dispose();
        }

        void _animationStatusChanged(AnimationStatus status) {
            setState(() =>{
                // The animation's state is our build state, and it changed already.
            });
        }

        public override Widget build(BuildContext context) {
            return widget.build(context);
        }
    }

}