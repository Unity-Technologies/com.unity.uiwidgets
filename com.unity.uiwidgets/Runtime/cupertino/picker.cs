using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    static class CupertinoPickerUtils {
        public static Color _kHighlighterBorder = new Color(0xFF7F7F7F);
        public static Color _kDefaultBackground = new Color(0xFFD2D4DB);
        public const float _kDefaultDiameterRatio = 1.35f;
        public const float _kDefaultPerspective = 0.004f;
        public const float _kForegroundScreenOpacityFraction = 0.7f;
    }

    public class CupertinoPicker : StatefulWidget {
        public CupertinoPicker(
            float itemExtent,
            ValueChanged<int> onSelectedItemChanged,
            List<Widget> children = null,
            Key key = null,
            float diameterRatio = CupertinoPickerUtils._kDefaultDiameterRatio,
            Color backgroundColor = null,
            float offAxisFraction = 0.0f,
            bool useMagnifier = false,
            float magnification = 1.0f,
            FixedExtentScrollController scrollController = null,
            bool looping = false,
            ListWheelChildDelegate childDelegate = null
        ) : base(key: key) {
            D.assert(children != null || childDelegate != null);
            D.assert(diameterRatio > 0.0, () => RenderListWheelViewport.diameterRatioZeroMessage);
            D.assert(magnification > 0);
            D.assert(itemExtent > 0);

            this.childDelegate = childDelegate ?? (looping
                                     ? (ListWheelChildDelegate) new ListWheelChildLoopingListDelegate(
                                         children: children)
                                     : (ListWheelChildDelegate) new ListWheelChildListDelegate(children: children));

            this.itemExtent = itemExtent;
            this.onSelectedItemChanged = onSelectedItemChanged;
            this.diameterRatio = diameterRatio;
            this.backgroundColor = backgroundColor ?? CupertinoPickerUtils._kDefaultBackground;
            this.offAxisFraction = offAxisFraction;
            this.useMagnifier = useMagnifier;
            this.magnification = magnification;
            this.scrollController = scrollController;
        }

        public static CupertinoPicker builder(
            float itemExtent,
            ValueChanged<int> onSelectedItemChanged,
            IndexedWidgetBuilder itemBuilder,
            Key key = null,
            float diameterRatio = CupertinoPickerUtils._kDefaultDiameterRatio,
            Color backgroundColor = null,
            float offAxisFraction = 0.0f,
            bool useMagnifier = false,
            float magnification = 1.0f,
            FixedExtentScrollController scrollController = null,
            int? childCount = null
        ) {
            D.assert(itemBuilder != null);
            D.assert(diameterRatio > 0.0f, () => RenderListWheelViewport.diameterRatioZeroMessage);
            D.assert(magnification > 0);
            D.assert(itemExtent > 0);

            return new CupertinoPicker(
                itemExtent: itemExtent,
                onSelectedItemChanged: onSelectedItemChanged,
                key: key,
                diameterRatio: diameterRatio,
                backgroundColor: backgroundColor,
                offAxisFraction: offAxisFraction,
                useMagnifier: useMagnifier,
                magnification: magnification,
                scrollController: scrollController,
                childDelegate: new ListWheelChildBuilderDelegate(builder: itemBuilder, childCount: childCount)
            );
        }

        public readonly float diameterRatio;
        public readonly Color backgroundColor;
        public readonly float offAxisFraction;
        public readonly bool useMagnifier;
        public readonly float magnification;
        public readonly FixedExtentScrollController scrollController;
        public readonly float itemExtent;
        public readonly ValueChanged<int> onSelectedItemChanged;
        public readonly ListWheelChildDelegate childDelegate;

        public override State createState() {
            return new _CupertinoPickerState();
        }
    }

    class _CupertinoPickerState : State<CupertinoPicker> {
        FixedExtentScrollController _controller;

        public override void initState() {
            base.initState();
            if (widget.scrollController == null) {
                _controller = new FixedExtentScrollController();
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            if (widget.scrollController != null && ((CupertinoPicker) oldWidget).scrollController == null) {
                _controller = null;
            }
            else if (widget.scrollController == null && ((CupertinoPicker) oldWidget).scrollController != null) {
                D.assert(_controller == null);
                _controller = new FixedExtentScrollController();
            }

            base.didUpdateWidget(oldWidget);
        }

        public override void dispose() {
            _controller?.dispose();
            base.dispose();
        }

        void _handleSelectedItemChanged(int index) {
            if (widget.onSelectedItemChanged != null) {
                widget.onSelectedItemChanged(index);
            }
        }

        Widget _buildGradientScreen() {
            if (widget.backgroundColor != null && widget.backgroundColor.alpha < 255) {
                return new Container();
            }

            Color widgetBackgroundColor = widget.backgroundColor ?? new Color(0xFFFFFFFF);
            return Positioned.fill(
                child: new IgnorePointer(
                    child: new Container(
                        decoration: new BoxDecoration(
                            gradient: new LinearGradient(
                                colors: new List<Color> {
                                    widgetBackgroundColor,
                                    widgetBackgroundColor.withAlpha(0xF2),
                                    widgetBackgroundColor.withAlpha(0xDD),
                                    widgetBackgroundColor.withAlpha(0),
                                    widgetBackgroundColor.withAlpha(0),
                                    widgetBackgroundColor.withAlpha(0xDD),
                                    widgetBackgroundColor.withAlpha(0xF2),
                                    widgetBackgroundColor,
                                },
                                stops: new List<float> {
                                    0.0f, 0.05f, 0.09f, 0.22f, 0.78f, 0.91f, 0.95f, 1.0f
                                },
                                begin: Alignment.topCenter,
                                end: Alignment.bottomCenter
                            )
                        )
                    )
                )
            );
        }

        Widget _buildMagnifierScreen() {
            Color foreground = widget.backgroundColor?.withAlpha(
                (int) (widget.backgroundColor.alpha * CupertinoPickerUtils._kForegroundScreenOpacityFraction)
            );

            return new IgnorePointer(
                child: new Column(
                    children: new List<Widget> {
                        new Expanded(
                            child: new Container(
                                color: foreground
                            )
                        ),
                        new Container(
                            decoration: new BoxDecoration(
                                border: new Border(
                                    top: new BorderSide(width: 0.0f, color: CupertinoPickerUtils._kHighlighterBorder),
                                    bottom: new BorderSide(width: 0.0f, color: CupertinoPickerUtils._kHighlighterBorder)
                                )
                            ),
                            constraints: BoxConstraints.expand(
                                height: widget.itemExtent * widget.magnification
                            )
                        ),
                        new Expanded(
                            child: new Container(
                                color: foreground
                            )
                        ),
                    }
                )
            );
        }

        Widget _buildUnderMagnifierScreen() {
            Color foreground = widget.backgroundColor?.withAlpha(
                (int) (widget.backgroundColor.alpha * CupertinoPickerUtils._kForegroundScreenOpacityFraction)
            );

            return new Column(
                children: new List<Widget> {
                    new Expanded(child: new Container()),
                    new Container(
                        color: foreground,
                        constraints: BoxConstraints.expand(
                            height: widget.itemExtent * widget.magnification
                        )
                    ),
                    new Expanded(child: new Container())
                }
            );
        }

        Widget _addBackgroundToChild(Widget child) {
            return new DecoratedBox(
                decoration: new BoxDecoration(
                    color: widget.backgroundColor
                ),
                child: child
            );
        }

        public override Widget build(BuildContext context) {
            Widget result = new Stack(
                children: new List<Widget> {
                    Positioned.fill(
                        child: ListWheelScrollView.useDelegate(
                            controller: widget.scrollController ?? _controller,
                            physics: new FixedExtentScrollPhysics(),
                            diameterRatio: widget.diameterRatio,
                            perspective: CupertinoPickerUtils._kDefaultPerspective,
                            offAxisFraction: widget.offAxisFraction,
                            useMagnifier: widget.useMagnifier,
                            magnification: widget.magnification,
                            itemExtent: widget.itemExtent,
                            onSelectedItemChanged: _handleSelectedItemChanged,
                            childDelegate: widget.childDelegate
                        )
                    ),
                    _buildGradientScreen(),
                    _buildMagnifierScreen()
                }
            );
            if (widget.backgroundColor != null && widget.backgroundColor.alpha < 255) {
                result = new Stack(
                    children: new List<Widget> {
                        _buildUnderMagnifierScreen(), _addBackgroundToChild(result),
                    }
                );
            }
            else {
                result = _addBackgroundToChild(result);
            }

            return result;
        }
    }
}