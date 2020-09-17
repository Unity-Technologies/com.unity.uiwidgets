using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.material {
    public delegate void ReorderCallback(int oldIndex, int newIndex);

    public class ReorderableListView : StatefulWidget {
        public ReorderableListView(
            Widget header = null,
            List<Widget> children = null,
            ReorderCallback onReorder = null,
            Axis scrollDirection = Axis.vertical,
            EdgeInsets padding = null,
            bool reverse = false
        ) {
            D.assert(onReorder != null);
            D.assert(children != null);
            D.assert(
                children.All((Widget w) => w.key != null),
                () => "All children of this widget must have a key."
            );
            this.header = header;
            this.children = children;
            this.scrollDirection = scrollDirection;
            this.padding = padding;
            this.onReorder = onReorder;
            this.reverse = reverse;
        }

        public readonly Widget header;

        public readonly List<Widget> children;

        public readonly Axis scrollDirection;

        public readonly EdgeInsets padding;

        public readonly ReorderCallback onReorder;

        public readonly bool reverse;

        public override State createState() {
            return new _ReorderableListViewState();
        }
    }

    class _ReorderableListViewState : State<ReorderableListView> {
        GlobalKey _overlayKey = GlobalKey.key(debugLabel: "$ReorderableListView overlay key");

        OverlayEntry _listOverlayEntry;

        public override void initState() {
            base.initState();
            _listOverlayEntry = new OverlayEntry(
                opaque: true,
                builder: (BuildContext context) => {
                    return new _ReorderableListContent(
                        header: widget.header,
                        children: widget.children,
                        scrollDirection: widget.scrollDirection,
                        onReorder: widget.onReorder,
                        padding: widget.padding,
                        reverse: widget.reverse
                    );
                }
            );
        }

        public override Widget build(BuildContext context) {
            return new Overlay(
                key: _overlayKey,
                initialEntries: new List<OverlayEntry> {
                    _listOverlayEntry
                });
        }
    }

    class _ReorderableListContent : StatefulWidget {
        public _ReorderableListContent(
            Widget header,
            List<Widget> children,
            Axis scrollDirection,
            EdgeInsets padding,
            ReorderCallback onReorder,
            bool? reverse = null
        ) {
            this.header = header;
            this.children = children;
            this.scrollDirection = scrollDirection;
            this.padding = padding;
            this.onReorder = onReorder;
            this.reverse = reverse;
        }

        public readonly Widget header;
        public readonly List<Widget> children;
        public readonly Axis scrollDirection;
        public readonly EdgeInsets padding;
        public readonly ReorderCallback onReorder;
        public readonly bool? reverse;

        public override State createState() {
            return new _ReorderableListContentState();
        }
    }

    class _ReorderableListContentState : TickerProviderStateMixin<_ReorderableListContent> {
        const float _defaultDropAreaExtent = 100.0f;

        const float _dropAreaMargin = 8.0f;

        readonly TimeSpan _reorderAnimationDuration = new TimeSpan(0, 0, 0, 0, 200);

        readonly TimeSpan _scrollAnimationDuration = new TimeSpan(0, 0, 0, 0, 200);

        ScrollController _scrollController;

        AnimationController _entranceController;

        AnimationController _ghostController;

        Key _dragging;

        Size _draggingFeedbackSize;

        int _dragStartIndex = 0;

        int _ghostIndex = 0;

        int _currentIndex = 0;

        int _nextIndex = 0;

        bool _scrolling = false;

        float _dropAreaExtent {
            get {
                if (_draggingFeedbackSize == null) {
                    return _defaultDropAreaExtent;
                }

                float dropAreaWithoutMargin;
                switch (widget.scrollDirection) {
                    case Axis.horizontal:
                        dropAreaWithoutMargin = _draggingFeedbackSize.width;
                        break;
                    case Axis.vertical:
                    default:
                        dropAreaWithoutMargin = _draggingFeedbackSize.height;
                        break;
                }

                return dropAreaWithoutMargin + _dropAreaMargin;
            }
        }

        public override void initState() {
            base.initState();
            _entranceController = new AnimationController(vsync: this, duration: _reorderAnimationDuration);
            _ghostController = new AnimationController(vsync: this, duration: _reorderAnimationDuration);
            _entranceController.addStatusListener(_onEntranceStatusChanged);
        }

        public override void didChangeDependencies() {
            _scrollController = PrimaryScrollController.of(context) ?? new ScrollController();
            base.didChangeDependencies();
        }

        public override void dispose() {
            _entranceController.dispose();
            _ghostController.dispose();
            base.dispose();
        }

        void _requestAnimationToNextIndex() {
            if (_entranceController.isCompleted) {
                _ghostIndex = _currentIndex;
                if (_nextIndex == _currentIndex) {
                    return;
                }

                _currentIndex = _nextIndex;
                _ghostController.reverse(from: 1.0f);
                _entranceController.forward(from: 0.0f);
            }
        }

        void _onEntranceStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.completed) {
                setState(() => { _requestAnimationToNextIndex(); });
            }
        }

        void _scrollTo(BuildContext context) {
            if (_scrolling) {
                return;
            }

            RenderObject contextObject = context.findRenderObject();
            RenderAbstractViewport viewport = RenderViewportUtils.of(contextObject);
            D.assert(viewport != null);
            float margin = _dropAreaExtent;
            float scrollOffset = _scrollController.offset;
            float topOffset = Mathf.Max(_scrollController.position.minScrollExtent,
                viewport.getOffsetToReveal(contextObject, 0.0f).offset - margin
            );
            float bottomOffset = Mathf.Min(_scrollController.position.maxScrollExtent,
                viewport.getOffsetToReveal(contextObject, 1.0f).offset + margin
            );
            bool onScreen = scrollOffset <= topOffset && scrollOffset >= bottomOffset;

            if (!onScreen) {
                _scrolling = true;
                _scrollController.position.animateTo(
                    scrollOffset < bottomOffset ? bottomOffset : topOffset,
                    duration: _scrollAnimationDuration,
                    curve: Curves.easeInOut
                ).Then(() => { setState(() => { _scrolling = false; }); });
            }
        }

        Widget _buildContainerForScrollDirection(List<Widget> children = null) {
            switch (widget.scrollDirection) {
                case Axis.horizontal:
                    return new Row(children: children);
                case Axis.vertical:
                default:
                    return new Column(children: children);
            }
        }

        Widget _wrap(Widget toWrap, int index, BoxConstraints constraints) {
            D.assert(toWrap.key != null);
            GlobalObjectKey<State<_ReorderableListContent>> keyIndexGlobalKey =
                new GlobalObjectKey<State<_ReorderableListContent>>(toWrap.key);

            void onDragStarted() {
                setState(() => {
                    _dragging = toWrap.key;
                    _dragStartIndex = index;
                    _ghostIndex = index;
                    _currentIndex = index;
                    _entranceController.setValue(1.0f);
                    _draggingFeedbackSize = keyIndexGlobalKey.currentContext.size;
                });
            }

            void reorder(int startIndex, int endIndex) {
                setState(() => {
                    if (startIndex != endIndex) {
                        widget.onReorder(startIndex, endIndex);
                    }

                    _ghostController.reverse(from: 0.1f);
                    _entranceController.reverse(from: 0.1f);
                    _dragging = null;
                });
            }

            void onDragEnded() {
                reorder(_dragStartIndex, _currentIndex);
            }


            Widget wrapWithKeyedSubtree() {
                return new KeyedSubtree(
                    key: keyIndexGlobalKey,
                    child: toWrap
                );
            }

            Widget buildDragTarget(BuildContext context, List<Key> acceptedCandidates, List<Key> rejectedCandidates) {
                Widget toWrapWithKeyedSubtree = wrapWithKeyedSubtree();
                Widget child = new LongPressDraggable<Key>(
                    maxSimultaneousDrags: 1,
                    axis: widget.scrollDirection,
                    data: toWrap.key,
                    feedback: new Container(
                        alignment: Alignment.topLeft,
                        // These constraints will limit the cross axis of the drawn widget.
                        constraints: constraints,
                        child: new Material(
                            elevation: 6.0f,
                            child: toWrapWithKeyedSubtree
                        )
                    ),
                    child: _dragging == toWrap.key ? new SizedBox() : toWrapWithKeyedSubtree,
                    childWhenDragging: new SizedBox(),
                    dragAnchor: DragAnchor.child,
                    onDragStarted: onDragStarted,
                    onDragCompleted: onDragEnded,
                    onDraggableCanceled: (Velocity velocity, Offset offset) => { onDragEnded(); }
                );

                if (index >= widget.children.Count) {
                    child = toWrap;
                }

                Widget spacing;
                switch (widget.scrollDirection) {
                    case Axis.horizontal:
                        spacing = new SizedBox(width: _dropAreaExtent);
                        break;
                    case Axis.vertical:
                    default:
                        spacing = new SizedBox(height: _dropAreaExtent);
                        break;
                }

                if (_currentIndex == index) {
                    return _buildContainerForScrollDirection(children: new List<Widget> {
                        new SizeTransition(
                            sizeFactor: _entranceController,
                            axis: widget.scrollDirection,
                            child: spacing
                        ),
                        child
                    });
                }

                if (_ghostIndex == index) {
                    return _buildContainerForScrollDirection(children: new List<Widget> {
                        new SizeTransition(
                            sizeFactor: _ghostController,
                            axis: widget.scrollDirection,
                            child: spacing
                        ),
                        child
                    });
                }

                return child;
            }

            return new Builder(builder: (BuildContext context) => {
                return new DragTarget<Key>(
                    builder: buildDragTarget,
                    onWillAccept: (Key toAccept) => {
                        setState(() => {
                            _nextIndex = index;
                            _requestAnimationToNextIndex();
                        });
                        _scrollTo(context);
                        return _dragging == toAccept && toAccept != toWrap.key;
                    },
                    onAccept: (Key accepted) => { },
                    onLeave: (Key leaving) => { }
                );
            });
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));
            return new LayoutBuilder(builder: (BuildContext _, BoxConstraints constraints) => {
                List<Widget> wrappedChildren = new List<Widget> { };
                if (widget.header != null) {
                    wrappedChildren.Add(widget.header);
                }

                for (int i = 0; i < widget.children.Count; i += 1) {
                    wrappedChildren.Add(_wrap(widget.children[i], i, constraints));
                }

                Key endWidgetKey = Key.key("DraggableList - End Widget");
                Widget finalDropArea;
                switch (widget.scrollDirection) {
                    case Axis.horizontal:
                        finalDropArea = new SizedBox(
                            key: endWidgetKey,
                            width: _defaultDropAreaExtent,
                            height: constraints.maxHeight
                        );
                        break;
                    case Axis.vertical:
                    default:
                        finalDropArea = new SizedBox(
                            key: endWidgetKey,
                            height: _defaultDropAreaExtent,
                            width: constraints.maxWidth
                        );
                        break;
                }

                if (widget.reverse == true) {
                    wrappedChildren.Insert(0, _wrap(
                        finalDropArea,
                        widget.children.Count,
                        constraints)
                    );
                }
                else {
                    wrappedChildren.Add(_wrap(
                        finalDropArea, widget.children.Count,
                        constraints)
                    );
                }

                return new SingleChildScrollView(
                    scrollDirection: widget.scrollDirection,
                    child: _buildContainerForScrollDirection(children: wrappedChildren),
                    padding: widget.padding,
                    controller: _scrollController,
                    reverse: widget.reverse == true
                );
            });
        }
    }
}