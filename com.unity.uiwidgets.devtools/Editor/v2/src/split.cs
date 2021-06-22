using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
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
            // _verifyFractionsSumTo1(initialFractions);
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
            if (aspectRatio >= horizontalAspectRatio) return Axis.horizontal;
            return Axis.vertical;
        }
        
        public readonly Axis? axis;
        
        public readonly List<Widget> children;

        public readonly List<float?> initialFractions;
        
        public readonly List<float?> minSizes;
        
        public readonly List<SizedBox> splitters;
        public override State createState()
        {
            return new _SplitState();
        }
    }
    
    class _SplitState : State<Split>
    {
        public override Widget build(BuildContext context)
        {
            // return new LayoutBuilder(builder: _buildLayout);
            return new Container(child: new Text("this is a text!"));
        }


    }
}