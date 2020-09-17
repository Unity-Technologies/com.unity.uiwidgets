using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public class Visibility : StatelessWidget {
        public Visibility(
            Key key = null,
            Widget child = null,
            Widget replacement = null,
            bool visible = true,
            bool maintainState = false,
            bool maintainAnimation = false,
            bool maintainSize = false,
            bool maintainInteractivity = false
        ) : base(key: key) {
            D.assert(child != null);
            D.assert(maintainState == true || maintainAnimation == false,
                () => "Cannot maintain animations if the state is not also maintained.");
            D.assert(maintainAnimation == true || maintainSize == false,
                () => "Cannot maintain size if animations are not maintained.");
            D.assert(maintainSize == true || maintainInteractivity == false,
                () => "Cannot maintain interactivity if size is not maintained.");
            this.replacement = replacement ?? SizedBox.shrink();
            this.child = child;
            this.visible = visible;
            this.maintainState = maintainState;
            this.maintainAnimation = maintainAnimation;
            this.maintainSize = maintainSize;
            this.maintainInteractivity = maintainInteractivity;
        }

        public readonly Widget child;

        public readonly Widget replacement;

        public readonly bool visible;

        public readonly bool maintainState;

        public readonly bool maintainAnimation;

        public readonly bool maintainSize;

        public readonly bool maintainInteractivity;

        public override Widget build(BuildContext context) {
            if (maintainSize) {
                Widget result = child;
                if (!maintainInteractivity) {
                    result = new IgnorePointer(
                        child: child,
                        ignoring: !visible
                    );
                }

                return new Opacity(
                    opacity: visible ? 1.0f : 0.0f,
                    child: result
                );
            }

            D.assert(!maintainInteractivity);
            D.assert(!maintainSize);
            if (maintainState) {
                Widget result = child;
                if (!maintainAnimation) {
                    result = new TickerMode(child: child, enabled: visible);
                }

                return new Offstage(
                    child: result,
                    offstage: !visible
                );
            }

            D.assert(!maintainAnimation);
            D.assert(!maintainState);
            return visible ? child : replacement;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("visible", value: visible, ifFalse: "hidden", ifTrue: "visible"));
            properties.add(new FlagProperty("maintainState", value: maintainState, ifFalse: "maintainState"));
            properties.add(new FlagProperty("maintainAnimation", value: maintainAnimation,
                ifFalse: "maintainAnimation"));
            properties.add(new FlagProperty("maintainSize", value: maintainSize, ifFalse: "maintainSize"));
            properties.add(new FlagProperty("maintainInteractivity", value: maintainInteractivity,
                ifFalse: "maintainInteractivity"));
        }
    }
}