using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoPageScaffold : StatefulWidget {
        /// Creates a layout for pages with a navigation bar at the top.
        public CupertinoPageScaffold(
            Widget child,
            Key key = null,
            ObstructingPreferredSizeWidget navigationBar = null,
            Color backgroundColor = null,
            bool resizeToAvoidBottomInset = true
        ) : base(key: key) {
            D.assert(child != null);
            this.child = child;
            this.navigationBar = navigationBar;
            this.backgroundColor = backgroundColor;
            this.resizeToAvoidBottomInset = resizeToAvoidBottomInset;
        }

        public readonly ObstructingPreferredSizeWidget navigationBar;
        public readonly Widget child;
        public readonly Color backgroundColor;
        public readonly bool resizeToAvoidBottomInset;


        public override State createState() {
            return new _CupertinoPageScaffoldState();
        }
    }

    class _CupertinoPageScaffoldState : State<CupertinoPageScaffold> {
        public readonly ScrollController _primaryScrollController = new ScrollController();

        void _handleStatusBarTap() {
            // Only act on the scroll controller if it has any attached scroll positions.
            if (_primaryScrollController.hasClients) {
                _primaryScrollController.animateTo(
                    0.0f,
                    duration: new TimeSpan(0, 0, 0, 0, 500),
                    curve: Curves.linearToEaseOut
                );
            }
        }

        public override Widget build(BuildContext context) {
            //List<Widget> stacked = new List<Widget>();

            Widget paddedContent = widget.child;

            MediaQueryData existingMediaQuery = MediaQuery.of(context);
            if (widget.navigationBar != null) {
                float topPadding = widget.navigationBar.preferredSize.height + existingMediaQuery.padding.top;

                float bottomPadding = widget.resizeToAvoidBottomInset
                    ? existingMediaQuery.viewInsets.bottom
                    : 0.0f;

                EdgeInsets newViewInsets = widget.resizeToAvoidBottomInset
                    ? existingMediaQuery.viewInsets.copyWith(bottom: 0.0f)
                    : existingMediaQuery.viewInsets;

                bool fullObstruction = widget.navigationBar.shouldFullyObstruct(context);
                    /*widget.navigationBar.fullObstruction == false
                        ? CupertinoTheme.of(context).barBackgroundColor.alpha == 0xFF
                        : widget.navigationBar.fullObstruction;*/

                if (fullObstruction == true) {
                    paddedContent = new MediaQuery(
                        data: existingMediaQuery
                            .removePadding(removeTop: true)
                            .copyWith(
                                viewInsets: newViewInsets
                            ),
                        child: new Padding(
                            padding: EdgeInsets.only(top: topPadding, bottom: bottomPadding),
                            child: paddedContent
                        )
                    );
                }
                else {
                    paddedContent = new MediaQuery(
                        data: existingMediaQuery.copyWith(
                            padding: existingMediaQuery.padding.copyWith(
                                top: topPadding
                            ),
                            viewInsets: newViewInsets
                        ),
                        child: new Padding(
                            padding: EdgeInsets.only(bottom: bottomPadding),
                            child: paddedContent
                        )
                    );
                }
            }
            else {
                float bottomPadding = widget.resizeToAvoidBottomInset
                    ? existingMediaQuery.viewInsets.bottom
                    : 0.0f;
                paddedContent = new Padding(
                    padding: EdgeInsets.only(bottom: bottomPadding),
                    child: paddedContent
                );
            }
            List<Widget> childrenWigets = new List<Widget>();
            childrenWigets.Add( new PrimaryScrollController(
                controller: _primaryScrollController,
                child: paddedContent
            ));
            if (widget.navigationBar != null) {
                childrenWigets.Add(new Positioned(
                    top: 0.0f,
                    left: 0.0f,
                    right: 0.0f,
                    child: new MediaQuery(
                        data: existingMediaQuery.copyWith(textScaleFactor: 1),
                        child: widget.navigationBar
                    )
                ));
            }
            childrenWigets.Add(new Positioned(
                    top: 0.0f,
                    left: 0.0f,
                    right: 0.0f,
                    height: existingMediaQuery.padding.top,
                    child: new GestureDetector(
                        //excludeFromSemantics: true,
                        onTap: _handleStatusBarTap
                    )
                
            ));

            return new DecoratedBox(
                decoration: new BoxDecoration(
                    color: CupertinoDynamicColor.resolve(widget.backgroundColor, context)
                           ?? CupertinoTheme.of(context).scaffoldBackgroundColor
                ),
                child: new Stack(
                    children: childrenWigets));
        }
    }

    public abstract class ObstructingPreferredSizeStateWidget : StatefulWidget {
        
    }

    public abstract class ObstructingPreferredSizeWidget : PreferredSizeWidget {
        protected ObstructingPreferredSizeWidget(Key key = null) : base(key: key) {}
       // public virtual bool? fullObstruction { get; }
        public abstract bool shouldFullyObstruct(BuildContext context);
    }
}