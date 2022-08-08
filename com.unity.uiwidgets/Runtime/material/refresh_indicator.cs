using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    class RefreshIndicatorUtils {
        public const float _kDragContainerExtentPercentage = 0.25f;

        public const float _kDragSizeFactorLimit = 1.5f;

        public static readonly TimeSpan _kIndicatorSnapDuration = new TimeSpan(0, 0, 0, 0, 150);

        public static readonly TimeSpan _kIndicatorScaleDuration = new TimeSpan(0, 0, 0, 0, 200);
    }

    public delegate Future RefreshCallback();

    enum _RefreshIndicatorMode {
        drag, // Pointer is down.
        armed, // Dragged far enough that an up event will run the onRefresh callback.
        snap, // Animating to the indicator"s final "displacement".
        refresh, // Running the refresh callback.
        done, // Animating the indicator"s fade-out after refreshing.
        canceled, // Animating the indicator"s fade-out after not arming.
    }

    public class RefreshIndicator : StatefulWidget {
        public RefreshIndicator(
            Key key = null,
            Widget child = null,
            float displacement = 40.0f,
            RefreshCallback onRefresh = null,
            Color color = null,
            Color backgroundColor = null,
            ScrollNotificationPredicate notificationPredicate = null,
            float strokeWidth = 2.0f
        ) : base(key: key) {
            D.assert(child != null);
            D.assert(onRefresh != null);
            this.child = child;
            this.displacement = displacement;
            this.onRefresh = onRefresh;
            this.color = color;
            this.backgroundColor = backgroundColor;
            this.notificationPredicate = notificationPredicate ?? ScrollNotification.defaultScrollNotificationPredicate;
            this.strokeWidth = strokeWidth;
        }

        public readonly Widget child;

        public readonly float displacement;

        public readonly RefreshCallback onRefresh;

        public readonly Color color;

        public readonly Color backgroundColor;

        public readonly ScrollNotificationPredicate notificationPredicate;

        public readonly float strokeWidth;

        public override State createState() {
            return new RefreshIndicatorState();
        }
    }

    public class RefreshIndicatorState : TickerProviderStateMixin<RefreshIndicator> {
        AnimationController _positionController;
        AnimationController _scaleController;
        Animation<float> _positionFactor;
        Animation<float> _scaleFactor;
        Animation<float> _value;
        Animation<Color> _valueColor;

        _RefreshIndicatorMode? _mode;
        Future _pendingRefreshFuture;
        bool? _isIndicatorAtTop;
        float? _dragOffset;

        static readonly Animatable<float> _threeQuarterTween = new FloatTween(begin: 0.0f, end: 0.75f);

        static readonly Animatable<float> _kDragSizeFactorLimitTween =
            new FloatTween(begin: 0.0f, end: RefreshIndicatorUtils._kDragSizeFactorLimit);

        static readonly Animatable<float> _oneToZeroTween = new FloatTween(begin: 1.0f, end: 0.0f);

        public RefreshIndicatorState() {
        }

        public override void initState() {
            base.initState();
            _positionController = new AnimationController(vsync: this);
            _positionFactor = _positionController.drive(_kDragSizeFactorLimitTween);
            _value =
                _positionController
                    .drive(_threeQuarterTween); // The "value" of the circular progress indicator during a drag.

            _scaleController = new AnimationController(vsync: this);
            _scaleFactor = _scaleController.drive(_oneToZeroTween);
        }

        public override void didChangeDependencies() {
            ThemeData theme = Theme.of(context);
            _valueColor = _positionController.drive(
                new ColorTween(
                    begin: (widget.color ?? theme.accentColor).withOpacity(0.0f),
                    end: (widget.color ?? theme.accentColor).withOpacity(1.0f)
                ).chain(new CurveTween(
                    curve: new Interval(0.0f, 1.0f / RefreshIndicatorUtils._kDragSizeFactorLimit)
                ))
            );
            base.didChangeDependencies();
        }

        public override void dispose() {
            _positionController.dispose();
            _scaleController.dispose();
            base.dispose();
        }

        bool _handleScrollNotification(ScrollNotification notification) {
            if (!widget.notificationPredicate(notification)) {
                return false;
            }

            if (notification is ScrollStartNotification && notification.metrics.extentBefore() == 0.0f &&
                _mode == null && _start(notification.metrics.axisDirection)) {
                setState(() => { _mode = _RefreshIndicatorMode.drag; });
                return false;
            }

            bool? indicatorAtTopNow = null;
            switch (notification.metrics.axisDirection) {
                case AxisDirection.down:
                    indicatorAtTopNow = true;
                    break;
                case AxisDirection.up:
                    indicatorAtTopNow = false;
                    break;
                case AxisDirection.left:
                case AxisDirection.right:
                    indicatorAtTopNow = null;
                    break;
            }

            if (indicatorAtTopNow != _isIndicatorAtTop) {
                if (_mode == _RefreshIndicatorMode.drag || _mode == _RefreshIndicatorMode.armed) {
                    _dismiss(_RefreshIndicatorMode.canceled);
                }
            }
            else if (notification is ScrollUpdateNotification) {
                if (_mode == _RefreshIndicatorMode.drag || _mode == _RefreshIndicatorMode.armed) {
                    if (notification.metrics.extentBefore() > 0.0f) {
                        _dismiss(_RefreshIndicatorMode.canceled);
                    }
                    else {
                        _dragOffset -= (notification as ScrollUpdateNotification).scrollDelta;
                        _checkDragOffset(notification.metrics.viewportDimension);
                    }
                }

                if (_mode == _RefreshIndicatorMode.armed &&
                    (notification as ScrollUpdateNotification).dragDetails == null) {
                    _show();
                }
            }
            else if (notification is OverscrollNotification) {
                if (_mode == _RefreshIndicatorMode.drag || _mode == _RefreshIndicatorMode.armed) {
                    _dragOffset -= (notification as OverscrollNotification).overscroll / 2.0f;
                    _checkDragOffset(notification.metrics.viewportDimension);
                }
            }
            else if (notification is ScrollEndNotification) {
                switch (_mode) {
                    case _RefreshIndicatorMode.armed:
                        _show();
                        break;
                    case _RefreshIndicatorMode.drag:
                        _dismiss(_RefreshIndicatorMode.canceled);
                        break;
                    default:
                        break;
                }
            }

            return false;
        }

        bool _handleGlowNotification(OverscrollIndicatorNotification notification) {
            if (notification.depth != 0 || !notification.leading) {
                return false;
            }

            if (_mode == _RefreshIndicatorMode.drag) {
                notification.disallowGlow();
                return true;
            }

            return false;
        }

        bool _start(AxisDirection direction) {
            D.assert(_mode == null);
            D.assert(_isIndicatorAtTop == null);
            D.assert(_dragOffset == null);
            switch (direction) {
                case AxisDirection.down:
                    _isIndicatorAtTop = true;
                    break;
                case AxisDirection.up:
                    _isIndicatorAtTop = false;
                    break;
                case AxisDirection.left:
                case AxisDirection.right:
                    _isIndicatorAtTop = null;
                    return false;
            }

            _dragOffset = 0.0f;
            _scaleController.setValue(0.0f);
            _positionController.setValue(0.0f);
            return true;
        }

        void _checkDragOffset(float containerExtent) {
            D.assert(_mode == _RefreshIndicatorMode.drag || _mode == _RefreshIndicatorMode.armed);
            float? newValue = _dragOffset /
                              (containerExtent * RefreshIndicatorUtils._kDragContainerExtentPercentage);
            if (_mode == _RefreshIndicatorMode.armed) {
                newValue = Mathf.Max(newValue ?? 0.0f, 1.0f / RefreshIndicatorUtils._kDragSizeFactorLimit);
            }

            _positionController.setValue(newValue?.clamp(0.0f, 1.0f) ?? 0.0f); // this triggers various rebuilds
            if (_mode == _RefreshIndicatorMode.drag && _valueColor.value.alpha == 0xFF) {
                _mode = _RefreshIndicatorMode.armed;
            }
        }

        Future _dismiss(_RefreshIndicatorMode newMode) {
            D.assert(newMode == _RefreshIndicatorMode.canceled || newMode == _RefreshIndicatorMode.done);
            setState(() => { _mode = newMode; });
            switch (_mode) {
                case _RefreshIndicatorMode.done:
                    return _scaleController
                        .animateTo(1.0f, duration: RefreshIndicatorUtils._kIndicatorScaleDuration).then((value) => {
                            if (mounted && _mode == newMode) {
                                _dragOffset = null;
                                _isIndicatorAtTop = null;
                                setState(() => { _mode = null; });
                            }
                        });
                case _RefreshIndicatorMode.canceled:
                    return _positionController
                        .animateTo(0.0f, duration: RefreshIndicatorUtils._kIndicatorScaleDuration).then((value) => {
                            if (mounted && _mode == newMode) {
                                _dragOffset = null;
                                _isIndicatorAtTop = null;
                                setState(() => { _mode = null; });
                            }
                        });
                default:
                    throw new Exception("Unknown refresh indicator mode: " + _mode);
            }
        }

        void _show() {
            D.assert(_mode != _RefreshIndicatorMode.refresh);
            D.assert(_mode != _RefreshIndicatorMode.snap);
            Completer completer = Completer.create();
            _pendingRefreshFuture = completer.future;
            _mode = _RefreshIndicatorMode.snap;
            _positionController
                .animateTo(1.0f / RefreshIndicatorUtils._kDragSizeFactorLimit,
                    duration: RefreshIndicatorUtils._kIndicatorSnapDuration)
                .then((value) => {
                    if (mounted && _mode == _RefreshIndicatorMode.snap) {
                        D.assert(widget.onRefresh != null);
                        setState(() => { _mode = _RefreshIndicatorMode.refresh; });

                        Future refreshResult = widget.onRefresh();
                        D.assert(() => {
                            if (refreshResult == null) {
                                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                                    exception: new UIWidgetsError(
                                        "The onRefresh callback returned null.\n" +
                                        "The RefreshIndicator onRefresh callback must return a Promise."
                                    ),
                                    context: new ErrorDescription("when calling onRefresh"),
                                    library: "material library"
                                ));
                            }

                            return true;
                        });
                        if (refreshResult == null) {
                            return;
                        }

                        refreshResult.whenComplete(() => {
                            if (mounted && _mode == _RefreshIndicatorMode.refresh) {
                                completer.complete();
                                _dismiss(_RefreshIndicatorMode.done);
                            }
                        });
                    }
                });
        }

        public Future show(bool atTop = true) {
            if (_mode != _RefreshIndicatorMode.refresh && _mode != _RefreshIndicatorMode.snap) {
                if (_mode == null) {
                    _start(atTop ? AxisDirection.down : AxisDirection.up);
                }

                _show();
            }

            return _pendingRefreshFuture;
        }

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            Widget child = new NotificationListener<ScrollNotification>(
                onNotification: _handleScrollNotification,
                child: new NotificationListener<OverscrollIndicatorNotification>(
                    onNotification: _handleGlowNotification,
                    child: widget.child
                )
            );
            D.assert(() => {
                if (_mode == null) {
                    D.assert(_dragOffset == null);
                    D.assert(_isIndicatorAtTop == null);
                }
                else {
                    D.assert(_dragOffset != null);
                    D.assert(_isIndicatorAtTop != null);
                }

                return true;
            });

            bool showIndeterminateIndicator =
                _mode == _RefreshIndicatorMode.refresh || _mode == _RefreshIndicatorMode.done;

            List<Widget> children = new List<Widget> {child};
            if (_mode != null) {
                children.Add(new Positioned(
                    top: _isIndicatorAtTop == true ? 0.0f : (float?) null,
                    bottom: _isIndicatorAtTop != true ? 0.0f : (float?) null,
                    left: 0.0f,
                    right: 0.0f,
                    child: new SizeTransition(
                        axisAlignment: _isIndicatorAtTop == true ? 1.0f : -1.0f,
                        sizeFactor: _positionFactor, // this is what brings it down
                        child: new Container(
                            padding: _isIndicatorAtTop == true
                                ? EdgeInsets.only(top: widget.displacement)
                                : EdgeInsets.only(bottom: widget.displacement),
                            alignment: _isIndicatorAtTop == true
                                ? Alignment.topCenter
                                : Alignment.bottomCenter,
                            child: new ScaleTransition(
                                scale: _scaleFactor,
                                child: new AnimatedBuilder(
                                    animation: _positionController,
                                    builder: (BuildContext _context, Widget _child) => {
                                        return new RefreshProgressIndicator(
                                            value: showIndeterminateIndicator ? (float?) null : _value.value,
                                            valueColor: _valueColor,
                                            backgroundColor: widget.backgroundColor,
                                            strokeWidth: widget.strokeWidth
                                        );
                                    }
                                )
                            )
                        )
                    )
                ));
            }

            return new Stack(
                children: children
            );
        }
    }
}