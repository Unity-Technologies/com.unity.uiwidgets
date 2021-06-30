using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.UIWidgets.DevTools
{

    public static class SplitUtils
    {
        const float defaultEpsilon = 1.0f / 1000;
        public static void _verifyFractionsSumTo1(List<float?> fractions) {
            var sumFractions = 0.0f;
            foreach (var fraction in fractions) {
                sumFractions += fraction.Value;
            }
            D.assert(
                (1.0f - sumFractions).abs() < defaultEpsilon,
                ()=>$"Fractions should sum to 1.0, but instead sum to {sumFractions}:\n{fractions}"
            );
        }
    }
    
    class Split : StatefulWidget
    {
        /// Builds a split oriented along [axis].
        public Split(
            Key key = null,
            Axis? axis = null,
            List<Widget> children = null,
            List<float?> initialFractions = null,
            List<float?> minSizes =null,
            List<SizedBox> splitters = null
        ) : base(key: key)
        {
            D.assert(axis != null);
            D.assert(children != null && children.Count >= 2);
            D.assert(initialFractions != null && initialFractions.Count >= 2);
            D.assert(children?.Count == initialFractions?.Count);
            SplitUtils._verifyFractionsSumTo1(initialFractions);
            if (minSizes != null)
            {
                D.assert(minSizes.Count == children?.Count);
            }

            if (splitters != null)
            {
                D.assert(splitters.Count == children?.Count - 1);
            }

            this.axis = axis;
            this.children = children;
            this.initialFractions = initialFractions;
            this.minSizes = minSizes;
            this.splitters = splitters;
        }

        public static Axis axisFor(BuildContext context, float horizontalAspectRatio) {
            var screenSize = MediaQuery.of(context).size;
            var aspectRatio = screenSize.width / screenSize.height;
            if (aspectRatio >= horizontalAspectRatio)
                return Axis.horizontal;
            return Axis.vertical;
        }
        
        public readonly Axis? axis;
        
        public readonly List<Widget> children;

        public readonly List<float?> initialFractions;
        
        public readonly List<float?> minSizes;
        
        public readonly List<SizedBox> splitters;
        
        public Key dividerKey(int index) => Key.key($"{this} dividerKey {index}");
        
        public override State createState()
        {
            return new _SplitState();
        }
    }
    
    class _SplitState : State<Split>
    {
        List<float?> fractions;
        bool isHorizontal
        {
            get
            {
                return widget.axis == Axis.horizontal;
            }
        }
        
        public override void initState() {
            base.initState();
            fractions = widget.initialFractions;
        }
        

        public override Widget build(BuildContext context)
        {
            return new LayoutBuilder(builder: _buildLayout);
        }

        Widget _buildLayout(BuildContext context, BoxConstraints constraints)
        {
            var width = constraints.maxWidth;
            var height = constraints.maxHeight;
            var axisSize = isHorizontal ? width : height;

            var availableSize = axisSize - _totalSplitterSize();
            
            float _sizeForIndex(int index) => availableSize * fractions[index].Value;

            List<float> sizes = new List<float>();
            for (int i = 0; i < fractions.Count; i++)
            {
                sizes.Add(_sizeForIndex(i));
            }
            
            float _minSizeForIndex(int index) {
                if (widget.minSizes == null) return 0.0f;

                float totalMinSize = 0;
                foreach (var minSize in widget.minSizes) {
                    totalMinSize += minSize.Value;
                }

                // Reduce the min sizes gracefully if the total required min size for all
                // children is greater than the available size for children.
                if (totalMinSize > availableSize) {
                    return widget.minSizes[index].Value * availableSize / totalMinSize;
                }

                return widget.minSizes[index].Value;
            }
            
            float _minFractionForIndex(int index) =>
                _minSizeForIndex(index) / availableSize;

            void _clampFraction(int index) {
                fractions[index] =
                    fractions[index].Value.clamp(_minFractionForIndex(index), 1.0f);
            }
            
            void updateSpacing(DragUpdateDetails dragDetails, int splitterIndex) {
              var dragDelta =
                  isHorizontal ? dragDetails.delta.dx : dragDetails.delta.dy;
              var fractionalDelta = dragDelta / axisSize;
              
              float updateSpacingBeforeSplitterIndex(float delta) {
                var startingDelta = delta;
                var index = splitterIndex;
                while (index >= 0) {
                  fractions[index] += delta;
                  var minFractionForIndex = _minFractionForIndex(index);
                  if (fractions[index] >= minFractionForIndex) {
                    _clampFraction(index);
                    return startingDelta;
                  }
                  delta = fractions[index].Value - minFractionForIndex;
                  _clampFraction(index);
                  index--;
                }

                return startingDelta - delta;
              }
              
              float updateSpacingAfterSplitterIndex(float delta) {
                var startingDelta = delta;
                var index = splitterIndex + 1;
                while (index < fractions.Count) {
                  fractions[index] += delta;
                  var minFractionForIndex = _minFractionForIndex(index);
                  if (fractions[index] >= minFractionForIndex) {
                    _clampFraction(index);
                    return startingDelta;
                  }
                  delta = fractions[index].Value - minFractionForIndex;
                  _clampFraction(index);
                  index++;
                }

                return startingDelta - delta;
              }

              setState(() => {
                  if (fractionalDelta <= 0.0f) 
                  {
                      var appliedDelta =
                          updateSpacingBeforeSplitterIndex(fractionalDelta);
                      updateSpacingAfterSplitterIndex(-appliedDelta);
                  } else {
                      var appliedDelta =
                          updateSpacingAfterSplitterIndex(-fractionalDelta);
                      updateSpacingBeforeSplitterIndex(-appliedDelta);
                  }
              });
              SplitUtils._verifyFractionsSumTo1(fractions);
            }
  
            

            List<Widget> children = new List<Widget>();
            for (int i = 0; i < widget.children.Count; i++) {
                children.Add(
                    new SizedBox(
                        width: isHorizontal ? sizes[i] : width,
                        height: isHorizontal ? height : sizes[i],
                        child: widget.children[i]
                    )
                    );
                if (i < widget.children.Count - 1)
                {
                    if (isHorizontal)
                    {
                        children.Add(
                            new MouseRegion(
                                // cursor: SystemMouseCursors.grab,
                                child: new GestureDetector(
                                    key: widget.dividerKey(i),
                                    behavior: HitTestBehavior.translucent,
                                    onHorizontalDragUpdate: (details) => updateSpacing(details, i),
                                    onVerticalDragUpdate: (details) =>
                                    {
                                        return;
                                    },
                                    dragStartBehavior: DragStartBehavior.down,
                                    child: widget.splitters != null
                                        ? widget.splitters[i]
                                        : new SizedBox(child:new DefaultSplitter(isHorizontal: isHorizontal))
                                )
                            )
                        );
                    }
                    else
                    {
                        children.Add(
                            new MouseRegion(
                                // cursor: SystemMouseCursors.grab,
                                child: new GestureDetector(
                                    key: widget.dividerKey(i),
                                    behavior: HitTestBehavior.translucent,
                                    onHorizontalDragUpdate: (details) =>
                                    {
                                        return;
                                    },
                                    onVerticalDragUpdate: (details) => updateSpacing(details, i),
                                    dragStartBehavior: DragStartBehavior.down,
                                    child: widget.splitters != null
                                        ? widget.splitters[i]
                                        : new SizedBox(child:new DefaultSplitter(isHorizontal: isHorizontal))
                                )
                            )
                        );
                    }
                    
                }
            }
            
            
            return new Flex(direction: widget.axis.Value, children: children);
        }

        float _totalSplitterSize() {
            var numSplitters = widget.children.Count - 1;
            if (widget.splitters == null) {
                return numSplitters * DefaultSplitter.splitterWidth;
            } else {
                var totalSize = 0.0f;
                foreach (var splitter in widget.splitters) {
                    totalSize += isHorizontal ? splitter.width.Value : splitter.height.Value;
                }
                return totalSize;
            }
        }
        
        
    }
    
    class DefaultSplitter : StatelessWidget {
        public DefaultSplitter(bool isHorizontal = true)
        {
            this.isHorizontal = isHorizontal;
        }

        public static readonly float iconSize = 24.0f;
        public static readonly float splitterWidth = 12.0f;
        bool isHorizontal;

    
        public override Widget build(BuildContext context)
        {
            return Transform.rotate(
                angle: isHorizontal ? utils.degToRad(90.0f) : utils.degToRad(0.0f),
                child: new Align(
                    widthFactor: 0.5f,
                    heightFactor: 0.5f,
                    child: new Icon(
                        Icons.drag_handle,
                        size: iconSize,
                        color: Theme.of(context).focusColor
                    )
                )
            );
        }
    }
    
}