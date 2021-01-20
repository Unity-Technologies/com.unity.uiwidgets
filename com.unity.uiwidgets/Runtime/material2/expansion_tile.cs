using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    static class ExpansionTileUtils {
        public static readonly TimeSpan _kExpand = new TimeSpan(0, 0, 0, 0, 200);
    }

    public class ExpansionTile : StatefulWidget {
        public ExpansionTile(
            Key key = null,
            Widget leading = null,
            Widget title = null,
            Color backgroundColor = null,
            ValueChanged<bool> onExpansionChanged = null,
            List<Widget> children = null,
            Widget trailing = null,
            bool initiallyExpanded = false
        ) : base(key: key) {
            D.assert(title != null);
            this.leading = leading;
            this.title = title;
            this.backgroundColor = backgroundColor;
            this.onExpansionChanged = onExpansionChanged;
            this.children = children ?? new List<Widget>();
            this.trailing = trailing;
            this.initiallyExpanded = initiallyExpanded;
        }

        public readonly Widget leading;

        public readonly Widget title;

        public readonly ValueChanged<bool> onExpansionChanged;

        public readonly List<Widget> children;

        public readonly Color backgroundColor;

        public readonly Widget trailing;

        public readonly bool initiallyExpanded;

        public override State createState() {
            return new _ExpansionTileState();
        }
    }

    public class _ExpansionTileState : SingleTickerProviderStateMixin<ExpansionTile> {
        static readonly Animatable<float> _easeOutTween = new CurveTween(curve: Curves.easeOut);
        static readonly Animatable<float> _easeInTween = new CurveTween(curve: Curves.easeIn);
        static readonly Animatable<float> _halfTween = new FloatTween(begin: 0.0f, end: 0.5f);

        readonly ColorTween _borderColorTween = new ColorTween();
        readonly ColorTween _headerColorTween = new ColorTween();
        readonly ColorTween _iconColorTween = new ColorTween();
        readonly ColorTween _backgroundColorTween = new ColorTween();

        AnimationController _controller;
        Animation<float> _iconTurns;
        Animation<float> _heightFactor;
        Animation<Color> _borderColor;
        Animation<Color> _headerColor;
        Animation<Color> _iconColor;
        Animation<Color> _backgroundColor;

        bool _isExpanded = false;

        public override void initState() {
            base.initState();
            _controller = new AnimationController(duration: ExpansionTileUtils._kExpand, vsync: this);
            _heightFactor = _controller.drive(_easeInTween);
            _iconTurns = _controller.drive(_halfTween.chain(_easeInTween));
            _borderColor = _controller.drive(_borderColorTween.chain(_easeOutTween));
            _headerColor = _controller.drive(_headerColorTween.chain(_easeInTween));
            _iconColor = _controller.drive(_iconColorTween.chain(_easeInTween));
            _backgroundColor = _controller.drive(_backgroundColorTween.chain(_easeOutTween));

            _isExpanded = PageStorage.of(context)?.readState(context) == null
                ? widget.initiallyExpanded
                : (bool) PageStorage.of(context)?.readState(context);

            if (_isExpanded) {
                _controller.setValue(1.0f);
            }
        }

        public override void dispose() {
            _controller.dispose();
            base.dispose();
        }

        void _handleTap() {
            setState(() => {
                _isExpanded = !_isExpanded;
                if (_isExpanded) {
                    _controller.forward();
                }
                else {
                    _controller.reverse().Then(() => {
                        if (!mounted) {
                            return;
                        }

                        setState(() => { });
                    });
                }

                PageStorage.of(context)?.writeState(context, _isExpanded);
            });
            if (widget.onExpansionChanged != null) {
                widget.onExpansionChanged(_isExpanded);
            }
        }

        Widget _buildChildren(BuildContext context, Widget child) {
            Color borderSideColor = _borderColor.value ?? Colors.transparent;

            return new Container(
                decoration: new BoxDecoration(
                    color: _backgroundColor.value ?? Colors.transparent,
                    border: new Border(
                        top: new BorderSide(color: borderSideColor),
                        bottom: new BorderSide(color: borderSideColor))),
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget> {
                        ListTileTheme.merge(
                            iconColor: _iconColor.value,
                            textColor: _headerColor.value,
                            child: new ListTile(
                                onTap: _handleTap,
                                leading: widget.leading,
                                title: widget.title,
                                trailing: widget.trailing ?? new RotationTransition(
                                              turns: _iconTurns,
                                              child: new Icon(Icons.expand_more)
                                          )
                            )
                        ),
                        new ClipRect(
                            child: new Align(
                                heightFactor: _heightFactor.value,
                                child: child)
                        )
                    }
                )
            );
        }

        public override void didChangeDependencies() {
            ThemeData theme = Theme.of(context);
            _borderColorTween.end = theme.dividerColor;
            _headerColorTween.begin = theme.textTheme.subhead.color;
            _headerColorTween.end = theme.accentColor;
            _iconColorTween.begin = theme.unselectedWidgetColor;
            _iconColorTween.end = theme.accentColor;
            _backgroundColorTween.end = widget.backgroundColor;
            base.didChangeDependencies();
        }

        public override Widget build(BuildContext context) {
            bool closed = !_isExpanded && _controller.isDismissed;
            return new AnimatedBuilder(
                animation: _controller.view,
                builder: _buildChildren,
                child: closed ? null : new Column(children: widget.children));
        }
    }
}