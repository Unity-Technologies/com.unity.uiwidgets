using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    static class TooltipUtils {
        public static readonly TimeSpan _kFadeDuration = new TimeSpan(0, 0, 0, 0, 200);
        public static readonly TimeSpan _kShowDuration = new TimeSpan(0, 0, 0, 0, 1500);
    }


    public class Tooltip : StatefulWidget {
        public Tooltip(
            Key key = null,
            string message = null,
            float height = 32.0f,
            EdgeInsets padding = null,
            float verticalOffset = 24.0f,
            bool preferBelow = true,
            Widget child = null
        ) : base(key: key) {
            D.assert(message != null);
            this.message = message;
            this.height = height;
            this.padding = padding ?? EdgeInsets.symmetric(horizontal: 16.0f);
            this.verticalOffset = verticalOffset;
            this.preferBelow = preferBelow;
            this.child = child;
        }


        public readonly string message;

        public readonly float height;

        public readonly EdgeInsets padding;

        public readonly float verticalOffset;

        public readonly bool preferBelow;

        public readonly Widget child;

        public override State createState() {
            return new _TooltipState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("message", message, showName: false));
            properties.add(new FloatProperty("vertical offset", verticalOffset));
            properties.add(new FlagProperty("position", value: preferBelow, ifTrue: "below", ifFalse: "above",
                showName: true));
        }
    }


    public class _TooltipState : SingleTickerProviderStateMixin<Tooltip> {
        AnimationController _controller;
        OverlayEntry _entry;
        Timer _timer;

        public override void initState() {
            base.initState();
            _controller = new AnimationController(duration: TooltipUtils._kFadeDuration, vsync: this);
            _controller.addStatusListener(_handleStatusChanged);
        }

        void _handleStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.dismissed) {
                _removeEntry();
            }
        }

        bool ensureTooltipVisible() {
            if (_entry != null) {
                _timer?.cancel();
                _timer = null;
                _controller.forward();
                return false;
            }

            RenderBox box = (RenderBox) this.context.findRenderObject();
            Offset target = box.localToGlobal(box.size.center(Offset.zero));

            Widget overlay = new _TooltipOverlay(
                message: widget.message,
                height: widget.height,
                padding: widget.padding,
                animation: new CurvedAnimation(
                    parent: _controller,
                    curve: Curves.fastOutSlowIn),
                target: target,
                verticalOffset: widget.verticalOffset,
                preferBelow: widget.preferBelow
            );

            _entry = new OverlayEntry(builder: (BuildContext context) => overlay);
            Overlay.of(this.context, debugRequiredFor: widget).insert(_entry);
            GestureBinding.instance.pointerRouter.addGlobalRoute(_handlePointerEvent);
            _controller.forward();
            return true;
        }

        void _removeEntry() {
            D.assert(_entry != null);
            _timer?.cancel();
            _timer = null;
            _entry.remove();
            _entry = null;
            GestureBinding.instance.pointerRouter.removeGlobalRoute(_handlePointerEvent);
        }

        void _handlePointerEvent(PointerEvent pEvent) {
            D.assert(_entry != null);
            if (pEvent is PointerUpEvent || pEvent is PointerCancelEvent) {
                _timer = _timer ?? Timer.create(TooltipUtils._kShowDuration,
                                  () => _controller.reverse());
            }
            else if (pEvent is PointerDownEvent) {
                _controller.reverse();
            }
        }

        public override void deactivate() {
            if (_entry != null) {
                _controller.reverse();
            }

            base.deactivate();
        }

        public override void dispose() {
            if (_entry != null) {
                _removeEntry();
            }

            _controller.dispose();
            base.dispose();
        }

        void _handleLongPress() {
            bool tooltipCreated = ensureTooltipVisible();
            if (tooltipCreated) {
                Feedback.forLongPress(context);
            }
        }


        public override Widget build(BuildContext context) {
            D.assert(Overlay.of(context, debugRequiredFor: widget) != null);
            return new GestureDetector(
                behavior: HitTestBehavior.opaque,
                onLongPress: _handleLongPress,
                child: widget.child
            );
        }
    }


    public class _TooltipPositionDelegate : SingleChildLayoutDelegate {
        public _TooltipPositionDelegate(
            Offset target = null,
            float? verticalOffset = null,
            bool? preferBelow = null) {
            D.assert(target != null);
            D.assert(verticalOffset != null);
            D.assert(preferBelow != null);
            this.target = target;
            this.verticalOffset = verticalOffset ?? 0.0f;
            this.preferBelow = preferBelow ?? true;
        }

        public readonly Offset target;

        public readonly float verticalOffset;

        public readonly bool preferBelow;

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return constraints.loosen();
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            return Geometry.positionDependentBox(
                size: size,
                childSize: childSize,
                target: target,
                verticalOffset: verticalOffset,
                preferBelow: preferBelow);
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate oldDelegate) {
            _TooltipPositionDelegate _oldDelegate = (_TooltipPositionDelegate) oldDelegate;
            return target != _oldDelegate.target ||
                   verticalOffset != _oldDelegate.verticalOffset ||
                   preferBelow != _oldDelegate.preferBelow;
        }
    }


    class _TooltipOverlay : StatelessWidget {
        public _TooltipOverlay(
            Key key = null,
            string message = null,
            float? height = null,
            EdgeInsets padding = null,
            Animation<float> animation = null,
            Offset target = null,
            float? verticalOffset = null,
            bool? preferBelow = null
        ) : base(key: key) {
            this.message = message;
            this.height = height;
            this.padding = padding;
            this.animation = animation;
            this.target = target;
            this.verticalOffset = verticalOffset;
            this.preferBelow = preferBelow;
        }

        public readonly string message;

        public readonly float? height;

        public readonly EdgeInsets padding;

        public readonly Animation<float> animation;

        public readonly Offset target;

        public readonly float? verticalOffset;

        public readonly bool? preferBelow;


        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            ThemeData darkTheme = new ThemeData(
                brightness: Brightness.dark,
                textTheme: theme.brightness == Brightness.dark ? theme.textTheme : theme.primaryTextTheme
            );
            return Positioned.fill(
                child: new IgnorePointer(
                    child: new CustomSingleChildLayout(
                        layoutDelegate: new _TooltipPositionDelegate(
                            target: target,
                            verticalOffset: verticalOffset,
                            preferBelow: preferBelow),
                        child: new FadeTransition(
                            opacity: animation,
                            child: new Opacity(
                                opacity: 0.9f,
                                child: new ConstrainedBox(
                                    constraints: new BoxConstraints(minHeight: height ?? 0.0f),
                                    child: new Container(
                                        decoration: new BoxDecoration(
                                            color: darkTheme.backgroundColor,
                                            borderRadius: BorderRadius.circular(2.0f)),
                                        padding: padding,
                                        child: new Center(
                                            widthFactor: 1.0f,
                                            heightFactor: 1.0f,
                                            child: new Text(message, style: darkTheme.textTheme.body1)
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );
        }
    }
}