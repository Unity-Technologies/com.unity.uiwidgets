using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {

    public class CupertinoContextMenuAction : StatefulWidget {

        public CupertinoContextMenuAction(
            Key key = null,
            Widget child = null,
            bool isDefaultAction = false,
            bool isDestructiveAction = false,
            VoidCallback onPressed = null,
            IconData trailingIcon = null
        ) : base(key: key) {
            D.assert(child != null);
            this.child = child;
            this.isDefaultAction = isDefaultAction;
            this.isDestructiveAction = isDestructiveAction;
            this.onPressed = onPressed;
            this.trailingIcon = trailingIcon;

        }
        public readonly Widget child;
        public readonly bool isDefaultAction;
        public readonly bool isDestructiveAction;
        public readonly VoidCallback onPressed;
        public readonly IconData trailingIcon;

        public override State createState() {
            return new _CupertinoContextMenuActionState();
        }
    } 
    public class _CupertinoContextMenuActionState : State<CupertinoContextMenuAction> {
        public static Color _kBackgroundColor = new Color(0xFFEEEEEE);
        public static Color _kBackgroundColorPressed = new Color(0xFFDDDDDD);
        public static float _kButtonHeight = 56.0f;

        public static readonly TextStyle _kActionSheetActionStyle = new TextStyle(
            fontFamily: ".SF UI Text",
            inherit: false,
            fontSize: 20.0f,
            fontWeight: FontWeight.w400,
            color: CupertinoColors.black,
            textBaseline: TextBaseline.alphabetic
        );
        public GlobalKey _globalKey = GlobalKey<State<StatefulWidget>>.key();
        bool _isPressed = false;

        void onTapDown(TapDownDetails details) {
            setState(()=>{
              _isPressed = true;
            });
        }

        void onTapUp(TapUpDetails details) {
            setState(()=>{
                _isPressed = false;
            });
        }

        void onTapCancel() {
            setState(()=>{
              _isPressed = false;
            });
        }

        TextStyle _textStyle {
            get {
                if (widget.isDefaultAction) {
                    return _kActionSheetActionStyle.copyWith(
                        fontWeight: FontWeight.w600
                    );
                }
                if (widget.isDestructiveAction) {
                    return _kActionSheetActionStyle.copyWith(
                        color: CupertinoColors.destructiveRed
                    );
                }
                return _kActionSheetActionStyle;
            }
           
        }

        public override Widget build(BuildContext context) {
            List<Widget> widgets = new List<Widget>();
            widgets.Add(new Flexible(child: widget.child));
            if (widget.trailingIcon != null) {
                widgets.Add(new Icon(
                    widget.trailingIcon,
                    color: _textStyle.color));
            }

            return new GestureDetector(
                key: _globalKey,
                onTapDown: onTapDown,
                onTapUp: onTapUp,
                onTapCancel: onTapCancel,
                onTap: widget.onPressed == null
                    ? (GestureTapCallback) null
                    : () => {
                        if (widget.onPressed != null) {
                            widget.onPressed();
                        }
                    },
                behavior: HitTestBehavior.opaque,
                child: 
                    new ConstrainedBox(
                        constraints: new BoxConstraints(
                            minHeight: _kButtonHeight
                        ),
                        child: 
                            new Container(
                                padding: EdgeInsets.symmetric(
                                    vertical: 16.0f,
                                    horizontal: 10.0f
                                ),
                                decoration: new BoxDecoration(
                                    color: _isPressed ? _kBackgroundColorPressed : _kBackgroundColor,
                                    border: new Border(
                                        bottom: new BorderSide(width: 1.0f, color: _kBackgroundColorPressed)
                                        )
                                    ),
                                child: new DefaultTextStyle(
                                    style: _textStyle,
                                    child: new Row(
                                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                        children:widgets
                                        )
                                    )
                            
                        )
                    )
            );
        }
    }

}