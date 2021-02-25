using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    static class CupertinoPickerUtils {
        public static Color _kHighlighterBorder = CupertinoDynamicColor.withBrightness(
            color: new Color(0x33000000),
            darkColor: new Color(0x33FFFFFF)
        );
        public const float _kDefaultDiameterRatio = 1.07f;
        public const float _kDefaultPerspective = 0.003f;
        public const float _kSqueeze = 1.45f;
        public const float _kOverAndUnderCenterOpacity = 0.447f;
        
        public static Color _kDefaultBackground = new Color(0xFFD2D4DB);
        public const float _kForegroundScreenOpacityFraction = 0.7f;
       
    }

    public class CupertinoPicker : StatefulWidget {
        public CupertinoPicker(
            List<Widget> children,
            float itemExtent,
            Key key = null,
            float? diameterRatio = null,
            Color backgroundColor = null,
            float offAxisFraction = 0.0f,
            bool useMagnifier = false,
            float magnification = 1.0f,
            FixedExtentScrollController scrollController = null,
            float? squeeze = null,
            ValueChanged<int> onSelectedItemChanged = null,
            bool looping = false
        ) : base(key: key) {
            diameterRatio = diameterRatio == null ? CupertinoPickerUtils._kDefaultDiameterRatio : diameterRatio;
            squeeze = squeeze == null ? CupertinoPickerUtils._kSqueeze : squeeze;
           // D.assert(children != null);
            D.assert(diameterRatio > 0.0f, ()=>RenderListWheelViewport.diameterRatioZeroMessage);
            D.assert(magnification > 0);
            D.assert(itemExtent > 0);
            D.assert(squeeze > 0);
            this.diameterRatio = diameterRatio;
            this.backgroundColor = backgroundColor;
            this.offAxisFraction = offAxisFraction;
            this.useMagnifier = useMagnifier;
            this.magnification = magnification;
            this.scrollController = scrollController;
            this.squeeze = squeeze;
            this.itemExtent = itemExtent;
            this.onSelectedItemChanged = onSelectedItemChanged;
            childDelegate = looping
                ? (ListWheelChildDelegate) new ListWheelChildLoopingListDelegate(children: children)
                : new ListWheelChildListDelegate(children: children);
        }

        public CupertinoPicker(
            float itemExtent,
            Key key = null,
            float? diameterRatio = null,
            Color backgroundColor = null,
            float offAxisFraction = 0.0f,
            bool useMagnifier = false,
            float magnification = 1.0f,
            FixedExtentScrollController scrollController = null,
            float? squeeze = null,
            
            ValueChanged<int> onSelectedItemChanged = null,
            IndexedWidgetBuilder itemBuilder = null,
            int? childCount = null
        ) {
            diameterRatio = diameterRatio == null ? CupertinoPickerUtils._kDefaultDiameterRatio : diameterRatio;
            squeeze = squeeze == null ?  CupertinoPickerUtils._kSqueeze : squeeze;
            D.assert(itemBuilder != null);
            D.assert(diameterRatio > 0.0f,()=> RenderListWheelViewport.diameterRatioZeroMessage);
            D.assert(magnification > 0);
            D.assert(itemExtent > 0);
            D.assert(squeeze > 0);
            this.diameterRatio = diameterRatio;
            this.backgroundColor = backgroundColor;
            this.offAxisFraction = offAxisFraction;
            this.useMagnifier = useMagnifier;
            this.magnification = magnification;
            this.scrollController = scrollController;
            this.squeeze = squeeze;
            this.itemExtent = itemExtent;
            this.onSelectedItemChanged = onSelectedItemChanged;
            childDelegate =new ListWheelChildBuilderDelegate(builder: itemBuilder, childCount: childCount);
            
        }

        public readonly float? diameterRatio;
        public readonly Color backgroundColor;
        public readonly float offAxisFraction;
        public readonly bool useMagnifier;
        public readonly float magnification;
        public readonly FixedExtentScrollController scrollController;
        public readonly float itemExtent;
        public readonly ValueChanged<int> onSelectedItemChanged;
        public ListWheelChildDelegate childDelegate;
        public readonly float? squeeze;

        public override State createState() {
            return new _CupertinoPickerState();
        }
    }

    class _CupertinoPickerState : State<CupertinoPicker> {
        FixedExtentScrollController _controller;
        int _lastHapticIndex;
        public override void initState() {
            base.initState();
            if (widget.scrollController == null) {
                _controller = new FixedExtentScrollController();
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (CupertinoPicker) oldWidget;
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
            //bool hasSuitableHapticHardware;
            /*switch (defaultTargetPlatform) {
                case TargetPlatform.iOS:
                    hasSuitableHapticHardware = true;
                    break;
                case TargetPlatform.android:
                case TargetPlatform.fuchsia:
                case TargetPlatform.linux:
                case TargetPlatform.macOS:
                case TargetPlatform.windows:
                    hasSuitableHapticHardware = false;
                    break;
            }*/
            /*D.assert(hasSuitableHapticHardware != null);
            if (hasSuitableHapticHardware && index != _lastHapticIndex) {
                _lastHapticIndex = index;
                HapticFeedback.selectionClick();
            }*/

            if (widget.onSelectedItemChanged != null) {
                widget.onSelectedItemChanged(index);
            }
        }
        Widget _buildMagnifierScreen() {
            Color resolvedBorderColor = CupertinoDynamicColor.resolve(CupertinoPickerUtils._kHighlighterBorder, context);

            return new IgnorePointer(
                child: new Center(
                    child: new Container(
                        decoration: new BoxDecoration(
                            border: new Border(
                                top: new BorderSide(width: 0.0f, color: resolvedBorderColor),
                                bottom: new BorderSide(width: 0.0f, color: resolvedBorderColor)
                            )
                        ),
                        constraints: BoxConstraints.expand(
                            height: widget.itemExtent * widget.magnification
                        )
                    )
                )
            );
        }
        public override Widget build(BuildContext context) {
            Color resolvedBackgroundColor = CupertinoDynamicColor.resolve(widget.backgroundColor, context);

            Widget result = new DefaultTextStyle(
                style: CupertinoTheme.of(context).textTheme.pickerTextStyle,
                child: new Stack(
                    children: new List<Widget>{
                Positioned.fill(
                    child: new _CupertinoPickerSemantics(
                        scrollController: widget.scrollController ?? _controller,
                        child: new ListWheelScrollView(
                            controller: widget.scrollController ?? _controller,
                            physics: new FixedExtentScrollPhysics(), 
                            diameterRatio: widget.diameterRatio ?? RenderListWheelViewport.defaultDiameterRatio ,
                            perspective: CupertinoPickerUtils._kDefaultPerspective,
                            offAxisFraction: widget.offAxisFraction,
                            useMagnifier: widget.useMagnifier,
                            magnification: widget.magnification,
                            overAndUnderCenterOpacity: CupertinoPickerUtils._kOverAndUnderCenterOpacity,
                            itemExtent: widget.itemExtent ,
                            squeeze: widget.squeeze ?? CupertinoPickerUtils._kSqueeze,
                            onSelectedItemChanged: _handleSelectedItemChanged,
                            childDelegate: widget.childDelegate
                            )
                        )
                    ), 
                _buildMagnifierScreen(),
                
                    }
                    )
                );
            return new DecoratedBox(
                decoration: new BoxDecoration(color: resolvedBackgroundColor),
                child: result
            );
        }


        
    }
    public class _CupertinoPickerSemantics : SingleChildRenderObjectWidget { 
        public _CupertinoPickerSemantics(
            Key key = null,
            Widget child = null,
            FixedExtentScrollController scrollController = null
            ) : base(key: key, child: child) {
            this.scrollController = scrollController;
        }
    
        
        public readonly FixedExtentScrollController scrollController;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoPickerSemantics(scrollController, Directionality.of(context));
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (_RenderCupertinoPickerSemantics) renderObject;
            ((_RenderCupertinoPickerSemantics)renderObject).textDirection = Directionality.of(context);
            ((_RenderCupertinoPickerSemantics)renderObject).controller = scrollController;
        }
    }
    public class _RenderCupertinoPickerSemantics : RenderProxyBox { 
        public _RenderCupertinoPickerSemantics(FixedExtentScrollController controller, TextDirection textDirection) { 
            this.controller = controller;
            _textDirection = textDirection;
        }

        public FixedExtentScrollController controller {
            get { return _controller; }
            set {
                if (value == _controller)
                    return;
                if (_controller != null)
                    _controller.removeListener(_handleScrollUpdate);
                else
                    _currentIndex = value.initialItem == 0 ? 0 : value.initialItem ;
                value.addListener(_handleScrollUpdate);
                _controller = value;
            }
        }
        FixedExtentScrollController _controller;

        public TextDirection textDirection {
            get { return _textDirection; }
            set {
                if (textDirection == value)
                    return;
                _textDirection = value;
                //markNeedsSemanticsUpdate();
            }
        }
        TextDirection _textDirection;

        int _currentIndex = 0;
        void _handleIncrease() {
            controller.jumpToItem(_currentIndex + 1);
        }
        void _handleDecrease() { 
            if (_currentIndex == 0) 
                return; 
            controller.jumpToItem(_currentIndex - 1);
        }
        void _handleScrollUpdate() {
            if (controller.selectedItem == _currentIndex)
                return;
            _currentIndex = controller.selectedItem;
            //markNeedsSemanticsUpdate();
        } 
        
        
       
    }

}