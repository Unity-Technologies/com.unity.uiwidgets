using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.gallery
{
    public static class GalleryBackdropUtils
    {
        public const float _kFrontHeadingHeight = 32.0f; // front layer beveled rectangle
        public const float _kFrontClosedHeight = 92.0f; // front layer height when closed
        public const float _kBackAppBarHeight = 56.0f; // back layer (options) appbar height

        public static readonly Animatable<BorderRadius> _kFrontHeadingBevelRadius = new BorderRadiusTween(
            begin: BorderRadius.only(
                topLeft: Radius.circular(12.0f),
                topRight: Radius.circular(12.0f)
            ),
            end: BorderRadius.only(
                topLeft: Radius.circular(_kFrontHeadingHeight),
                topRight: Radius.circular(_kFrontHeadingHeight)
            )
        );
    }

    internal class _TappableWhileStatusIs : StatefulWidget
    {
        public _TappableWhileStatusIs(
            AnimationStatus status,
            Key key = null,
            AnimationController controller = null,
            Widget child = null
        ) : base(key: key)
        {
            this.controller = controller;
            this.status = status;
            this.child = child;
        }

        public readonly AnimationController controller;
        public readonly AnimationStatus status;
        public readonly Widget child;

        public override State createState()
        {
            return new _TappableWhileStatusIsState();
        }
    }

    internal class _TappableWhileStatusIsState : State<_TappableWhileStatusIs>
    {
        private bool _active;

        public override void initState()
        {
            base.initState();
            this.widget.controller.addStatusListener(this._handleStatusChange);
            this._active = this.widget.controller.status == this.widget.status;
        }


        public override void dispose()
        {
            this.widget.controller.removeStatusListener(this._handleStatusChange);
            base.dispose();
        }

        private void _handleStatusChange(AnimationStatus status)
        {
            bool value = this.widget.controller.status == this.widget.status;
            if (this._active != value)
                this.setState(() => { this._active = value; });
        }

        public override Widget build(BuildContext context)
        {
            Widget child = new AbsorbPointer(
                absorbing: !this._active,
                child: this.widget.child
            );

            if (!this._active)
                child = new FocusScope(
                    canRequestFocus: false,
                    debugLabel: "_TappableWhileStatusIs",
                    child: child
                );

            return child;
        }
    }

    internal class _CrossFadeTransition : AnimatedWidget
    {
        public _CrossFadeTransition(
            Key key = null,
            Alignment alignment = null,
            Animation<float> progress = null,
            Widget child0 = null,
            Widget child1 = null
        ) : base(key: key, listenable: progress)
        {
            alignment = alignment ?? Alignment.center;
            this.alignment = alignment;
            this.child0 = child0;
            this.child1 = child1;
        }

        public readonly Alignment alignment;
        public readonly Widget child0;
        public readonly Widget child1;


        protected override Widget build(BuildContext context)
        {
            Animation<float> progress = this.listenable as Animation<float>;

            float opacity1 = new CurvedAnimation(
                parent: new ReverseAnimation(progress),
                curve: new Interval(0.5f, 1.0f)
            ).value;

            float opacity2 = new CurvedAnimation(
                parent: progress,
                curve: new Interval(0.5f, 1.0f)
            ).value;

            return new Stack(
                alignment: this.alignment,
                children: new List<Widget>
                {
                    new Opacity(
                        opacity: opacity1,
                        child: this.child1
                    ),
                    new Opacity(
                        opacity: opacity2,
                        child: this.child0
                    )
                }
            );
        }
    }

    internal class _BackAppBar : StatelessWidget
    {
        public _BackAppBar(
            Key key = null,
            Widget leading = null,
            Widget title = null,
            Widget trailing = null
        ) : base(key: key)
        {
            leading = leading ?? new SizedBox(width: 56.0f);
            D.assert(title != null);
            this.leading = leading;
            this.title = title;
            this.trailing = trailing;
        }

        public readonly Widget leading;
        public readonly Widget title;
        public readonly Widget trailing;


        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);

            List<Widget> children = new List<Widget>
            {
                new Container(
                    alignment: Alignment.center,
                    width: 56.0f,
                    child: this.leading
                ),
                new Expanded(
                    child: this.title
                )
            };

            if (this.trailing != null)
                children.Add(new Container(
                    alignment: Alignment.center,
                    width: 56.0f,
                    child: this.trailing
                ));

            return IconTheme.merge(
                data: theme.primaryIconTheme,
                child: new DefaultTextStyle(
                    style: theme.primaryTextTheme.headline6,
                    child: new SizedBox(
                        height: GalleryBackdropUtils._kBackAppBarHeight,
                        child: new Row(
                            children: children
                        )
                    )
                )
            );
        }
    }

    public class Backdrop : StatefulWidget
    {
        public Backdrop(
            Widget frontAction = null,
            Widget frontTitle = null,
            Widget frontLayer = null,
            Widget frontHeading = null,
            Widget backTitle = null,
            Widget backLayer = null
        )
        {
            this.frontAction = frontAction;
            this.frontTitle = frontTitle;
            this.frontLayer = frontLayer;
            this.frontHeading = frontHeading;
            this.backTitle = backTitle;
            this.backLayer = backLayer;
        }

        public readonly Widget frontAction;
        public readonly Widget frontTitle;
        public readonly Widget frontLayer;
        public readonly Widget frontHeading;
        public readonly Widget backTitle;
        public readonly Widget backLayer;

        public override State createState()
        {
            return new _BackdropState();
        }
    }

    internal class _BackdropState : SingleTickerProviderStateMixin<Backdrop>
    {
        private GlobalKey _backdropKey = GlobalKey.key(debugLabel: "Backdrop");
        private AnimationController _controller;
        private Animation<float> _frontOpacity;

        private static readonly Animatable<float> _frontOpacityTween = new FloatTween(begin: 0.2f, end: 1.0f)
            .chain(new CurveTween(curve: new Interval(0.0f, 0.4f, curve: Curves.easeInOut)));

        public override void initState()
        {
            base.initState();
            this._controller = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 0, 300),
                value: 1.0f,
                vsync: this
            );
            this._frontOpacity = this._controller.drive(_frontOpacityTween);
        }

        public override void dispose()
        {
            this._controller.dispose();
            base.dispose();
        }

        private float _backdropHeight
        {
            get
            {
                // Warning: this can be safely called from the event handlers but it may
                // not be called at build time.
                RenderBox renderBox = this._backdropKey.currentContext.findRenderObject() as RenderBox;
                return Mathf.Max(0.0f,
                    renderBox.size.height - GalleryBackdropUtils._kBackAppBarHeight -
                    GalleryBackdropUtils._kFrontClosedHeight);
            }
        }

        private void _handleDragUpdate(DragUpdateDetails details)
        {
            this._controller.setValue(this._controller.value - details.primaryDelta.Value / this._backdropHeight);
        }

        private void _handleDragEnd(DragEndDetails details)
        {
            if (this._controller.isAnimating || this._controller.status == AnimationStatus.completed)
                return;

            float flingVelocity = details.velocity.pixelsPerSecond.dy / this._backdropHeight;
            if (flingVelocity < 0.0)
                this._controller.fling(velocity: Mathf.Max(2.0f, -flingVelocity));
            else if (flingVelocity > 0.0)
                this._controller.fling(velocity: Mathf.Min(-2.0f, -flingVelocity));
            else
                this._controller.fling(velocity: this._controller.value < 0.5 ? -2.0f : 2.0f);
        }

        private void _toggleFrontLayer()
        {
            AnimationStatus status = this._controller.status;
            bool isOpen = status == AnimationStatus.completed || status == AnimationStatus.forward;
            this._controller.fling(velocity: isOpen ? -2.0f : 2.0f);
        }

        private Widget _buildStack(BuildContext context, BoxConstraints constraints)
        {
            Animation<RelativeRect> frontRelativeRect = this._controller.drive(new RelativeRectTween(
                begin: RelativeRect.fromLTRB(0.0f,
                    constraints.biggest.height - GalleryBackdropUtils._kFrontClosedHeight, 0.0f, 0.0f),
                end: RelativeRect.fromLTRB(0.0f, GalleryBackdropUtils._kBackAppBarHeight, 0.0f, 0.0f)
            ));

            var children = new List<Widget>
            {
                new Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: new List<Widget>
                    {
                        new _BackAppBar(
                            leading: this.widget.frontAction,
                            title: new _CrossFadeTransition(
                                progress: this._controller,
                                alignment: Alignment.center,
                                //alignment: AlignmentDirectional.centerStart,
                                child0: this.widget.frontTitle,
                                child1: this.widget.backTitle
                            ),
                            trailing: new IconButton(
                                onPressed: this._toggleFrontLayer,
                                tooltip: "Toggle options page",
                                icon: new AnimatedIcon(
                                    icon: AnimatedIcons.close_menu,
                                    progress: this._controller
                                )
                            )
                        ),
                        new Expanded(
                            child: new _TappableWhileStatusIs(
                                AnimationStatus.dismissed,
                                controller: this._controller,
                                child: new Visibility(
                                    child: this.widget.backLayer,
                                    visible: this._controller.status != AnimationStatus.completed,
                                    maintainState: true
                                )
                            )
                        )
                    }
                ),
                // Front layer
                new PositionedTransition(
                    rect: frontRelativeRect,
                    child: new AnimatedBuilder(
                        animation: this._controller,
                        builder: (BuildContext subContext, Widget child) =>
                        {
                            return new PhysicalShape(
                                elevation: 12.0f,
                                color: Theme.of(subContext).canvasColor,
                                clipper: new ShapeBorderClipper(
                                    shape: new BeveledRectangleBorder(
                                        borderRadius: GalleryBackdropUtils._kFrontHeadingBevelRadius.transform(
                                            this._controller.value)
                                    )
                                ),
                                clipBehavior: Clip.antiAlias,
                                child: child
                            );
                        },
                        child: new _TappableWhileStatusIs(
                            AnimationStatus.completed,
                            controller: this._controller,
                            child: new FadeTransition(
                                opacity: this._frontOpacity,
                                child: this.widget.frontLayer
                            )
                        )
                    )
                )
            };

            if (this.widget.frontHeading != null)
                children.Add(new PositionedTransition(
                    rect: frontRelativeRect,
                    child: new Container(
                        alignment: Alignment.topLeft,
                        child: new GestureDetector(
                            behavior: HitTestBehavior.opaque,
                            onTap: this._toggleFrontLayer,
                            onVerticalDragUpdate: this._handleDragUpdate,
                            onVerticalDragEnd: this._handleDragEnd,
                            child: this.widget.frontHeading
                        )
                    )
                ));

            return new Stack(
                key: this._backdropKey,
                children: children
            );
        }

        public override Widget build(BuildContext context)
        {
            return new LayoutBuilder(builder: this._buildStack);
        }
    }
}