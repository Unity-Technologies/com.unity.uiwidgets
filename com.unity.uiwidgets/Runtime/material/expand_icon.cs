using System;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    public class ExpandIcon : StatefulWidget {
        public ExpandIcon(
            Key key = null,
            bool isExpanded = false,
            float size = 24.0f,
            ValueChanged<bool> onPressed = null,
            EdgeInsetsGeometry padding = null,
            Color color = null,
            Color disabledColor = null,
            Color expandedColor = null
        ) : base(key: key) {
            this.isExpanded = isExpanded;
            this.size = size;
            this.onPressed = onPressed;
            this.padding = padding ?? EdgeInsets.all(8.0f);
            this.color = color;
            this.disabledColor = disabledColor;
            this.expandedColor = expandedColor;
        }

        public readonly bool isExpanded;

        public readonly float size;

        public readonly ValueChanged<bool> onPressed;

        public readonly EdgeInsetsGeometry padding;

        public readonly Color color;

        public readonly Color disabledColor;

        public readonly Color expandedColor;

        public override State createState() {
            return new _ExpandIconState();
        }
    }


    public class _ExpandIconState : SingleTickerProviderStateMixin<ExpandIcon> {
        AnimationController _controller;
        Animation<float> _iconTurns;

        static readonly Animatable<float> _iconTurnTween =
            new FloatTween(begin: 0.0f, end: 0.5f).chain(new CurveTween(curve: Curves.fastOutSlowIn));

        public override void initState() {
            base.initState();
            _controller = new AnimationController(duration: ThemeUtils.kThemeAnimationDuration, vsync: this);
            _iconTurns = _controller.drive(_iconTurnTween);
            if (widget.isExpanded) {
                _controller.setValue(Mathf.PI);
            }
        }

        public override void dispose() {
            _controller.dispose();
            base.dispose();
        }


        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            ExpandIcon _oldWidget = (ExpandIcon) oldWidget;
            if (widget.isExpanded != _oldWidget.isExpanded) {
                if (widget.isExpanded) {
                    _controller.forward();
                }
                else {
                    _controller.reverse();
                }
            }
        }


        void _handlePressed() {
            if (widget.onPressed != null) {
                widget.onPressed(widget.isExpanded);
            }
        }

        Color _iconColor {
            get {
                if (widget.isExpanded && widget.expandedColor != null) {
                    return widget.expandedColor;
                }

                if (widget.color != null) {
                    return widget.color;
                }

                switch (Theme.of(context).brightness) {
                    case Brightness.light:
                        return Colors.black54;
                    case Brightness.dark:
                        return Colors.white60;
                }

                D.assert(false);
                return null;
            }
        }


        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            return new IconButton(
                padding: widget.padding,
                iconSize: widget.size,
                color: _iconColor,
                disabledColor: widget.disabledColor,
                onPressed: widget.onPressed == null ? (VoidCallback) null : _handlePressed,
                icon: new RotationTransition(
                    turns: _iconTurns,
                    child: new Icon(Icons.expand_more))
            );
        }
    }
}