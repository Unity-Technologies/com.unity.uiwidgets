using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public class SafeArea : StatelessWidget {
        public SafeArea(
            Key key = null,
            bool left = true,
            bool top = true,
            bool right = true,
            bool bottom = true,
            EdgeInsets mininum = null,
            Widget child = null,
            bool maintainBottomViewPadding = false
        ) : base(key: key) {
            D.assert(child != null);
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            minimum = mininum ?? EdgeInsets.zero;
            this.child = child;
            this.maintainBottomViewPadding = maintainBottomViewPadding;
        }

        public readonly bool left;

        public readonly bool top;

        public readonly bool right;

        public readonly bool bottom;

        public readonly EdgeInsets minimum;

        public readonly Widget child;
        public readonly bool maintainBottomViewPadding;

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            MediaQueryData data = MediaQuery.of(context);
            EdgeInsets padding = data.padding;
            if (data.padding.bottom == 0.0 && data.viewInsets.bottom != 0.0 && maintainBottomViewPadding)
                padding = padding.copyWith(bottom: data.viewPadding.bottom);
            return new Padding(
                padding: EdgeInsets.only(
                    left: Mathf.Max(left ? padding.left : 0.0f, minimum.left),
                    top: Mathf.Max(top ? padding.top : 0.0f, minimum.top),
                    right: Mathf.Max(right ? padding.right : 0.0f, minimum.right),
                    bottom: Mathf.Max(bottom ? padding.bottom : 0.0f, minimum.bottom)
                ),
                child: MediaQuery.removePadding(
                    context: context,
                    removeLeft: left,
                    removeTop: top,
                    removeRight: right,
                    removeBottom: bottom,
                    child: child));
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("left", value: left, ifTrue: "avoid left padding"));
            properties.add(new FlagProperty("top", value: top, ifTrue: "avoid top padding"));
            properties.add(new FlagProperty("right", value: right, ifTrue: "avoid right padding"));
            properties.add(new FlagProperty("bottom", value: bottom, ifTrue: "avoid bottom padding"));
        }
    }


    public class SliverSafeArea : StatelessWidget {
        public SliverSafeArea(
            Key key = null,
            bool left = true,
            bool top = true,
            bool right = true,
            bool bottom = true,
            EdgeInsets minimum = null,
            Widget sliver = null
            ) : base(key: key) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.minimum = minimum ?? EdgeInsets.zero;
            this.sliver = sliver;
        }

        public readonly bool left;

        public readonly bool top;

        public readonly bool right;

        public readonly bool bottom;

        public readonly EdgeInsets minimum;

        public readonly Widget sliver;

        public override Widget build(BuildContext context) {
            EdgeInsets padding = MediaQuery.of(context).padding;
            return new SliverPadding(
                padding: EdgeInsets.only(
                    left: Mathf.Max(left ? padding.left : 0.0f, minimum.left),
                    top: Mathf.Max(top ? padding.top : 0.0f, minimum.top),
                    right: Mathf.Max(right ? padding.right : 0.0f, minimum.right),
                    bottom: Mathf.Max(bottom ? padding.bottom : 0.0f, minimum.bottom)
                ),
                sliver: MediaQuery.removePadding(
                    context: context,
                    removeLeft: left,
                    removeTop: top,
                    removeRight: right,
                    removeBottom: bottom,
                    child: sliver));
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("left", value: left, ifTrue: "avoid left padding"));
            properties.add(new FlagProperty("top", value: top, ifTrue: "avoid top padding"));
            properties.add(new FlagProperty("right", value: right, ifTrue: "avoid right padding"));
            properties.add(new FlagProperty("bottom", value: bottom, ifTrue: "avoid bottom padding"));
        }
    }
}