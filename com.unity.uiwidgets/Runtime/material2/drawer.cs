using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.material {
    static class DrawerUtils {
        public const float _kWidth = 304.0f;
        public const float _kEdgeDragWidth = 20.0f;
        public const float _kMinFlingVelocity = 365.0f;
        public static readonly TimeSpan _kBaseSettleDuration = new TimeSpan(0, 0, 0, 0, 246);
    }


    public enum DrawerAlignment {
        start,
        end
    }

    public class Drawer : StatelessWidget {
        public Drawer(
            Key key = null,
            float elevation = 16.0f,
            Widget child = null) : base(key: key) {
            D.assert(elevation >= 0.0f);
            this.elevation = elevation;
            this.child = child;
        }

        public readonly float elevation;

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new ConstrainedBox(
                constraints: BoxConstraints.expand(width: DrawerUtils._kWidth),
                child: new Material(
                    elevation: elevation,
                    child: child
                )
            );
        }
    }

    public delegate void DrawerCallback(bool isOpened);


    public class DrawerController : StatefulWidget {
        public DrawerController(
            GlobalKey key = null,
            Widget child = null,
            DrawerAlignment? alignment = null,
            DrawerCallback drawerCallback = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(child != null);
            D.assert(alignment != null);
            this.child = child;
            this.alignment = alignment ?? DrawerAlignment.start;
            this.drawerCallback = drawerCallback;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly Widget child;

        public readonly DrawerAlignment alignment;

        public readonly DragStartBehavior? dragStartBehavior;

        public readonly DrawerCallback drawerCallback;

        public override State createState() {
            return new DrawerControllerState();
        }
    }


    public class DrawerControllerState : SingleTickerProviderStateMixin<DrawerController> {
        public override void initState() {
            base.initState();
            _controller = new AnimationController(duration: DrawerUtils._kBaseSettleDuration, vsync: this);
            _controller.addListener(_animationChanged);
            _controller.addStatusListener(_animationStatusChanged);
        }

        public override void dispose() {
            _historyEntry?.remove();
            _controller.dispose();
            base.dispose();
        }

        void _animationChanged() {
            setState(() => { });
        }

        LocalHistoryEntry _historyEntry;
        readonly FocusScopeNode _focusScopeNode = new FocusScopeNode();

        void _ensureHistoryEntry() {
            if (_historyEntry == null) {
                ModalRoute route = ModalRoute.of(context);
                if (route != null) {
                    _historyEntry = new LocalHistoryEntry(onRemove: _handleHistoryEntryRemoved);
                    route.addLocalHistoryEntry(_historyEntry);
                    FocusScope.of(context).setFirstFocus(_focusScopeNode);
                }
            }
        }

        void _animationStatusChanged(AnimationStatus status) {
            switch (status) {
                case AnimationStatus.forward:
                    _ensureHistoryEntry();
                    break;
                case AnimationStatus.reverse:
                    _historyEntry?.remove();
                    _historyEntry = null;
                    break;
                case AnimationStatus.dismissed:
                    break;
                case AnimationStatus.completed:
                    break;
            }
        }

        void _handleHistoryEntryRemoved() {
            _historyEntry = null;
            close();
        }

        AnimationController _controller;


        void _handleDragDown(DragDownDetails details) {
            _controller.stop();
            _ensureHistoryEntry();
        }

        void _handleDragCancel() {
            if (_controller.isDismissed || _controller.isAnimating) {
                return;
            }

            if (_controller.value < 0.5) {
                close();
            }
            else {
                open();
            }
        }

        public readonly GlobalKey _drawerKey = GlobalKey.key();


        float _width {
            get {
                RenderBox box = (RenderBox) _drawerKey.currentContext?.findRenderObject();
                if (box != null) {
                    return box.size.width;
                }

                return DrawerUtils._kWidth;
            }
        }

        bool _previouslyOpened = false;

        void _move(DragUpdateDetails details) {
            float delta = (details.primaryDelta ?? 0) / _width;
            switch (widget.alignment) {
                case DrawerAlignment.start:
                    break;
                case DrawerAlignment.end:
                    delta = -delta;
                    break;
            }

            _controller.setValue(_controller.value + delta);

            bool opened = _controller.value > 0.5;
            if (opened != _previouslyOpened && widget.drawerCallback != null) {
                widget.drawerCallback(opened);
            }

            _previouslyOpened = opened;
        }

        void _settle(DragEndDetails details) {
            if (_controller.isDismissed) {
                return;
            }

            if (details.velocity.pixelsPerSecond.dx.abs() >= DrawerUtils._kMinFlingVelocity) {
                float visualVelocity = details.velocity.pixelsPerSecond.dx / DrawerUtils._kWidth;
                switch (widget.alignment) {
                    case DrawerAlignment.start:
                        break;
                    case DrawerAlignment.end:
                        visualVelocity = -visualVelocity;
                        break;
                }

                _controller.fling(velocity: visualVelocity);
            }
            else if (_controller.value < 0.5) {
                close();
            }
            else {
                open();
            }
        }

        public void open() {
            _controller.fling(velocity: 1.0f);
            if (widget.drawerCallback != null) {
                widget.drawerCallback(true);
            }
        }

        public void close() {
            _controller.fling(velocity: -1.0f);
            if (widget.drawerCallback != null) {
                widget.drawerCallback(false);
            }
        }

        ColorTween _color = new ColorTween(begin: Colors.transparent, end: Colors.black54);
        GlobalKey _gestureDetectorKey = GlobalKey.key();

        Alignment _drawerOuterAlignment {
            get {
                switch (widget.alignment) {
                    case DrawerAlignment.start:
                        return Alignment.centerLeft;
                    case DrawerAlignment.end:
                        return Alignment.centerRight;
                }

                return null;
            }
        }

        Alignment _drawerInnerAlignment {
            get {
                switch (widget.alignment) {
                    case DrawerAlignment.start:
                        return Alignment.centerRight;
                    case DrawerAlignment.end:
                        return Alignment.centerLeft;
                }

                return null;
            }
        }

        Widget _buildDrawer(BuildContext context) {
            bool drawerIsStart = widget.alignment == DrawerAlignment.start;
            EdgeInsets padding = MediaQuery.of(context).padding;
            float dragAreaWidth = drawerIsStart ? padding.left : padding.right;

            dragAreaWidth = Mathf.Max(dragAreaWidth, DrawerUtils._kEdgeDragWidth);
            if (_controller.status == AnimationStatus.dismissed) {
                return new Align(
                    alignment: _drawerOuterAlignment,
                    child: new GestureDetector(
                        key: _gestureDetectorKey,
                        onHorizontalDragUpdate: _move,
                        onHorizontalDragEnd: _settle,
                        behavior: HitTestBehavior.translucent,
                        dragStartBehavior: widget.dragStartBehavior ?? DragStartBehavior.down,
                        child: new Container(width: dragAreaWidth)
                    )
                );
            }
            else {
                return new GestureDetector(
                    key: _gestureDetectorKey,
                    onHorizontalDragDown: _handleDragDown,
                    onHorizontalDragUpdate: _move,
                    onHorizontalDragEnd: _settle,
                    onHorizontalDragCancel: _handleDragCancel,
                    dragStartBehavior: widget.dragStartBehavior ?? DragStartBehavior.down,
                    child: new RepaintBoundary(
                        child: new Stack(
                            children: new List<Widget> {
                                new GestureDetector(
                                    onTap: close,
                                    child: new Container(
                                        color: _color.evaluate(_controller)
                                    )
                                ),
                                new Align(
                                    alignment: _drawerOuterAlignment,
                                    child: new Align(
                                        alignment: _drawerInnerAlignment,
                                        widthFactor: _controller.value,
                                        child: new RepaintBoundary(
                                            child: new FocusScope(
                                                key: _drawerKey,
                                                node: _focusScopeNode,
                                                child: widget.child)
                                        )
                                    )
                                )
                            }
                        )
                    )
                );
            }
        }


        public override Widget build(BuildContext context) {
            return new ListTileTheme(
                style: ListTileStyle.drawer,
                child: _buildDrawer(context));
        }
    }
}