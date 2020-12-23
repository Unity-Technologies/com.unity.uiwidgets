using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public abstract class MergeableMaterialItem {
        public MergeableMaterialItem(
            LocalKey key) {
            D.assert(key != null);
            this.key = key;
        }

        public readonly LocalKey key;
    }

    public class MaterialSlice : MergeableMaterialItem {
        public MaterialSlice(
            LocalKey key = null,
            Widget child = null) : base(key: key) {
            D.assert(key != null);
            D.assert(child != null);
            this.child = child;
        }

        public readonly Widget child;

        public override string ToString() {
            return "MergeableSlice(key: " + key + ", child: " + child + ")";
        }
    }

    public class MaterialGap : MergeableMaterialItem {
        public MaterialGap(
            LocalKey key = null,
            float size = 16.0f) : base(key: key) {
            D.assert(key != null);
            this.size = size;
        }

        public readonly float size;

        public override string ToString() {
            return "MaterialGap(key: " + key + ", child: " + size + ")";
        }
    }


    public class MergeableMaterial : StatefulWidget {
        public MergeableMaterial(
            Key key = null,
            Axis mainAxis = Axis.vertical,
            int elevation = 2,
            bool hasDividers = false,
            List<MergeableMaterialItem> children = null) : base(key: key) {
            this.mainAxis = mainAxis;
            this.elevation = elevation;
            this.hasDividers = hasDividers;
            this.children = children ?? new List<MergeableMaterialItem>();
        }

        public readonly List<MergeableMaterialItem> children;

        public readonly Axis mainAxis;

        public readonly int elevation;

        public readonly bool hasDividers;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<Axis>("mainAxis", mainAxis));
            properties.add(new FloatProperty("elevation", elevation));
        }

        public override State createState() {
            return new _MergeableMaterialState();
        }
    }


    public class _AnimationTuple {
        public _AnimationTuple(
            AnimationController controller = null,
            CurvedAnimation startAnimation = null,
            CurvedAnimation endAnimation = null,
            CurvedAnimation gapAnimation = null,
            float gapStart = 0.0f) {
            this.controller = controller;
            this.startAnimation = startAnimation;
            this.endAnimation = endAnimation;
            this.gapAnimation = gapAnimation;
            this.gapStart = gapStart;
        }

        public readonly AnimationController controller;

        public readonly CurvedAnimation startAnimation;

        public readonly CurvedAnimation endAnimation;

        public readonly CurvedAnimation gapAnimation;

        public float gapStart;
    }


    public class _MergeableMaterialState : TickerProviderStateMixin<MergeableMaterial> {
        List<MergeableMaterialItem> _children;

        public readonly Dictionary<LocalKey, _AnimationTuple> _animationTuples =
            new Dictionary<LocalKey, _AnimationTuple>();

        public override void initState() {
            base.initState();
            _children = new List<MergeableMaterialItem>();
            _children.AddRange(widget.children);

            for (int i = 0; i < _children.Count; i++) {
                if (_children[i] is MaterialGap) {
                    _initGap((MaterialGap) _children[i]);
                    _animationTuples[_children[i].key].controller.setValue(1.0f);
                }
            }

            D.assert(_debugGapsAreValid(_children));
        }

        void _initGap(MaterialGap gap) {
            AnimationController controller = new AnimationController(
                duration: ThemeUtils.kThemeAnimationDuration,
                vsync: this);

            CurvedAnimation startAnimation = new CurvedAnimation(
                parent: controller,
                curve: Curves.fastOutSlowIn);

            CurvedAnimation endAnimation = new CurvedAnimation(
                parent: controller,
                curve: Curves.fastOutSlowIn);

            CurvedAnimation gapAnimation = new CurvedAnimation(
                parent: controller,
                curve: Curves.fastOutSlowIn);

            controller.addListener(_handleTick);

            _animationTuples[gap.key] = new _AnimationTuple(
                controller: controller,
                startAnimation: startAnimation,
                endAnimation: endAnimation,
                gapAnimation: gapAnimation);
        }

        public override void dispose() {
            foreach (MergeableMaterialItem child in _children) {
                if (child is MaterialGap) {
                    _animationTuples[child.key].controller.dispose();
                }
            }

            base.dispose();
        }


        void _handleTick() {
            setState(() => { });
        }

        bool _debugHasConsecutiveGaps(List<MergeableMaterialItem> children) {
            for (int i = 0; i < widget.children.Count - 1; i++) {
                if (widget.children[i] is MaterialGap &&
                    widget.children[i + 1] is MaterialGap) {
                    return true;
                }
            }

            return false;
        }

        bool _debugGapsAreValid(List<MergeableMaterialItem> children) {
            if (_debugHasConsecutiveGaps(children)) {
                return false;
            }

            if (children.isNotEmpty()) {
                if (children.first() is MaterialGap || children.last() is MaterialGap) {
                    return false;
                }
            }

            return true;
        }

        void _insertChild(int index, MergeableMaterialItem child) {
            _children.Insert(index, child);

            if (child is MaterialGap) {
                _initGap((MaterialGap) child);
            }
        }

        void _removeChild(int index) {
            MergeableMaterialItem child = _children[index];
            _children.RemoveAt(index);

            if (child is MaterialGap) {
                _animationTuples[child.key] = null;
            }
        }

        bool _isClosingGap(int index) {
            if (index < _children.Count - 1 && _children[index] is MaterialGap) {
                return _animationTuples[_children[index].key].controller.status == AnimationStatus.reverse;
            }

            return false;
        }

        void _removeEmptyGaps() {
            int j = 0;

            while (j < _children.Count) {
                if (_children[j] is MaterialGap &&
                    _animationTuples[_children[j].key].controller.status == AnimationStatus.dismissed) {
                    _removeChild(j);
                }
                else {
                    j++;
                }
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);

            MergeableMaterial _oldWidget = (MergeableMaterial) oldWidget;
            HashSet<LocalKey> oldKeys = new HashSet<LocalKey>();
            foreach (MergeableMaterialItem child in _oldWidget.children) {
                oldKeys.Add(child.key);
            }

            HashSet<LocalKey> newKeys = new HashSet<LocalKey>();
            foreach (MergeableMaterialItem child in widget.children) {
                newKeys.Add(child.key);
            }

            HashSet<LocalKey> newOnly = new HashSet<LocalKey>();
            foreach (var key in newKeys) {
                if (!oldKeys.Contains(key)) {
                    newOnly.Add(key);
                }
            }

            HashSet<LocalKey> oldOnly = new HashSet<LocalKey>();
            foreach (var key in oldKeys) {
                if (!newKeys.Contains(key)) {
                    oldOnly.Add(key);
                }
            }

            List<MergeableMaterialItem> newChildren = widget.children;
            int i = 0;
            int j = 0;

            D.assert(_debugGapsAreValid(newChildren));
            _removeEmptyGaps();

            while (i < newChildren.Count && j < _children.Count) {
                if (newOnly.Contains(newChildren[i].key) ||
                    oldOnly.Contains(_children[j].key)) {
                    int startNew = i;
                    int startOld = j;

                    while (newOnly.Contains(newChildren[i].key)) {
                        i++;
                    }

                    while (oldOnly.Contains(_children[j].key) || _isClosingGap(j)) {
                        j++;
                    }

                    int newLength = i - startNew;
                    int oldLength = j - startOld;

                    if (newLength > 0) {
                        if (oldLength > 1 || oldLength == 1 && _children[startOld] is MaterialSlice) {
                            if (newLength == 1 && newChildren[startNew] is MaterialGap) {
                                float gapSizeSum = 0.0f;

                                while (startOld < j) {
                                    if (_children[startOld] is MaterialGap) {
                                        MaterialGap gap = (MaterialGap) _children[startOld];
                                        gapSizeSum += gap.size;
                                    }

                                    _removeChild(startOld);
                                    j--;
                                }

                                _insertChild(startOld, newChildren[startNew]);
                                _animationTuples[newChildren[startNew].key].gapStart = gapSizeSum;
                                _animationTuples[newChildren[startNew].key].controller.forward();
                                j++;
                            }
                            else {
                                for (int k = 0; k < oldLength; k++) {
                                    _removeChild(startOld);
                                }

                                for (int k = 0; k < newLength; k++) {
                                    _insertChild(startOld + k, newChildren[startNew + k]);
                                }

                                j += (newLength - oldLength);
                            }
                        }
                        else if (oldLength == 1) {
                            if (newLength == 1 && newChildren[startNew] is MaterialGap &&
                                _children[startOld].key == newChildren[startNew].key) {
                                _animationTuples[newChildren[startNew].key].controller.forward();
                            }
                            else {
                                float gapSize = _getGapSize(startOld);

                                _removeChild(startOld);

                                for (int k = 0; k < newLength; k++) {
                                    _insertChild(startOld + k, newChildren[startNew + k]);
                                }

                                j += (newLength - 1);
                                float gapSizeSum = 0.0f;

                                for (int k = startNew; k < i; k++) {
                                    if (newChildren[k] is MaterialGap) {
                                        MaterialGap gap = (MaterialGap) newChildren[k];
                                        gapSizeSum += gap.size;
                                    }
                                }

                                for (int k = startNew; k < i; k++) {
                                    if (newChildren[k] is MaterialGap) {
                                        MaterialGap gap = (MaterialGap) newChildren[k];

                                        _animationTuples[gap.key].gapStart = gapSize * gap.size / gapSizeSum;
                                        _animationTuples[gap.key].controller.setValue(0.0f);
                                        _animationTuples[gap.key].controller.forward();
                                    }
                                }
                            }
                        }
                        else {
                            for (int k = 0; k < newLength; k++) {
                                _insertChild(startOld + k, newChildren[startNew + k]);

                                if (newChildren[startNew + k] is MaterialGap) {
                                    MaterialGap gap = (MaterialGap) newChildren[startNew + k];
                                    _animationTuples[gap.key].controller.forward();
                                }
                            }

                            j += newLength;
                        }
                    }
                    else {
                        if (oldLength > 1 || oldLength == 1 && _children[startOld] is MaterialSlice) {
                            float gapSizeSum = 0.0f;

                            while (startOld < j) {
                                if (_children[startOld] is MaterialGap) {
                                    MaterialGap gap = (MaterialGap) _children[startOld];
                                    gapSizeSum += gap.size;
                                }

                                _removeChild(startOld);
                                j--;
                            }

                            if (gapSizeSum != 0.0) {
                                MaterialGap gap = new MaterialGap(key: new UniqueKey(), size: gapSizeSum);
                                _insertChild(startOld, gap);
                                _animationTuples[gap.key].gapStart = 0.0f;
                                _animationTuples[gap.key].controller.setValue(1.0f);
                                _animationTuples[gap.key].controller.reverse();
                                j++;
                            }
                        }
                        else if (oldLength == 1) {
                            MaterialGap gap = (MaterialGap) _children[startOld];
                            _animationTuples[gap.key].gapStart = 0.0f;
                            _animationTuples[gap.key].controller.reverse();
                        }
                    }
                }
                else {
                    if ((_children[j] is MaterialGap) == (newChildren[i] is MaterialGap)) {
                        _children[j] = newChildren[i];

                        i++;
                        j++;
                    }
                    else {
                        D.assert(_children[j] is MaterialGap);
                        j++;
                    }
                }
            }

            while (j < _children.Count) {
                _removeChild(j);
            }

            while (i < newChildren.Count) {
                _insertChild(j, newChildren[i]);

                i++;
                j++;
            }
        }


        BorderRadius _borderRadius(int index, bool start, bool end) {
            D.assert(MaterialConstantsUtils.kMaterialEdges[MaterialType.card].topLeft ==
                     MaterialConstantsUtils.kMaterialEdges[MaterialType.card].topRight);
            D.assert(MaterialConstantsUtils.kMaterialEdges[MaterialType.card].topLeft ==
                     MaterialConstantsUtils.kMaterialEdges[MaterialType.card].bottomLeft);
            D.assert(MaterialConstantsUtils.kMaterialEdges[MaterialType.card].topLeft ==
                     MaterialConstantsUtils.kMaterialEdges[MaterialType.card].bottomRight);

            Radius cardRadius = MaterialConstantsUtils.kMaterialEdges[MaterialType.card].topLeft;
            Radius startRadius = Radius.zero;
            Radius endRadius = Radius.zero;

            if (index > 0 && _children[index - 1] is MaterialGap) {
                startRadius = Radius.lerp(
                    Radius.zero,
                    cardRadius,
                    _animationTuples[_children[index - 1].key].startAnimation.value);
            }

            if (index < _children.Count - 2 && _children[index + 1] is MaterialGap) {
                endRadius = Radius.lerp(
                    Radius.zero,
                    cardRadius,
                    _animationTuples[_children[index + 1].key].endAnimation.value);
            }

            if (widget.mainAxis == Axis.vertical) {
                return BorderRadius.vertical(
                    top: start ? cardRadius : startRadius,
                    bottom: end ? cardRadius : endRadius);
            }
            else {
                return BorderRadius.horizontal(
                    left: start ? cardRadius : startRadius,
                    right: end ? cardRadius : endRadius);
            }
        }


        float _getGapSize(int index) {
            MaterialGap gap = (MaterialGap) _children[index];

            return MathUtils.lerpFloat(_animationTuples[gap.key].gapStart,
                gap.size,
                _animationTuples[gap.key].gapAnimation.value);
        }


        bool _willNeedDivider(int index) {
            if (index < 0) {
                return false;
            }

            if (index >= _children.Count) {
                return false;
            }

            return _children[index] is MaterialSlice || _isClosingGap(index);
        }


        public override Widget build(BuildContext context) {
            _removeEmptyGaps();

            List<Widget> widgets = new List<Widget>();
            List<Widget> slices = new List<Widget>();
            int i;

            for (i = 0; i < _children.Count; i++) {
                if (_children[i] is MaterialGap) {
                    D.assert(slices.isNotEmpty());
                    widgets.Add(
                        new Container(
                            decoration: new BoxDecoration(
                                color: Theme.of(context).cardColor,
                                borderRadius: _borderRadius(i - 1, widgets.isEmpty(), false),
                                shape: BoxShape.rectangle),
                            child: new ListBody(
                                mainAxis: widget.mainAxis,
                                children: slices)
                        )
                    );

                    slices = new List<Widget>();
                    widgets.Add(
                        new SizedBox(
                            width: widget.mainAxis == Axis.horizontal ? _getGapSize(i) : (float?) null,
                            height: widget.mainAxis == Axis.vertical ? _getGapSize(i) : (float?) null)
                    );
                }
                else {
                    MaterialSlice slice = (MaterialSlice) _children[i];
                    Widget child = slice.child;

                    if (widget.hasDividers) {
                        bool hasTopDivider = _willNeedDivider(i - 1);
                        bool hasBottomDivider = _willNeedDivider(i + 1);

                        Border border;
                        BorderSide divider = Divider.createBorderSide(
                            context,
                            width: 0.5f
                        );

                        if (i == 0) {
                            border = new Border(
                                bottom: hasBottomDivider ? divider : BorderSide.none);
                        }
                        else if (i == _children.Count - 1) {
                            border = new Border(
                                top: hasTopDivider ? divider : BorderSide.none);
                        }
                        else {
                            border = new Border(
                                top: hasTopDivider ? divider : BorderSide.none,
                                bottom: hasBottomDivider ? divider : BorderSide.none
                            );
                        }

                        D.assert(border != null);

                        child = new AnimatedContainer(
                            key: new _MergeableMaterialSliceKey(_children[i].key),
                            decoration: new BoxDecoration(border: border),
                            duration: ThemeUtils.kThemeAnimationDuration,
                            curve: Curves.fastOutSlowIn,
                            child: child
                        );
                    }

                    slices.Add(
                        new Material(
                            type: MaterialType.transparency,
                            child: child
                        )
                    );
                }
            }

            if (slices.isNotEmpty()) {
                widgets.Add(
                    new Container(
                        decoration: new BoxDecoration(
                            color: Theme.of(context).cardColor,
                            borderRadius: _borderRadius(i - 1, widgets.isEmpty(), true),
                            shape: BoxShape.rectangle
                        ),
                        child: new ListBody(
                            mainAxis: widget.mainAxis,
                            children: slices
                        )
                    )
                );
                slices = new List<Widget>();
            }

            return new _MergeableMaterialListBody(
                mainAxis: widget.mainAxis,
                boxShadows: ShadowConstants.kElevationToShadow[widget.elevation],
                items: _children,
                children: widgets
            );
        }
    }


    class _MergeableMaterialSliceKey : GlobalKey {
        public _MergeableMaterialSliceKey(LocalKey value) : base() {
            this.value = value;
        }

        public readonly LocalKey value;

        public bool Equals(_MergeableMaterialSliceKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.value == value;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((_MergeableMaterialSliceKey) obj);
        }

        public static bool operator ==(_MergeableMaterialSliceKey left, _MergeableMaterialSliceKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(_MergeableMaterialSliceKey left, _MergeableMaterialSliceKey right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = value.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            return "_MergeableMaterialSliceKey(" + value + ")";
        }
    }


    class _MergeableMaterialListBody : ListBody {
        public _MergeableMaterialListBody(
            List<Widget> children = null,
            Axis mainAxis = Axis.vertical,
            List<MergeableMaterialItem> items = null,
            List<BoxShadow> boxShadows = null
        ) : base(children: children, mainAxis: mainAxis) {
            this.items = items;
            this.boxShadows = boxShadows;
        }

        public readonly List<MergeableMaterialItem> items;

        public readonly List<BoxShadow> boxShadows;

        AxisDirection _getDirection(BuildContext context) {
            return AxisDirectionUtils.getAxisDirectionFromAxisReverseAndDirectionality(context, mainAxis, false) ??
                   AxisDirection.right;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderMergeableMaterialListBody(
                axisDirection: _getDirection(context),
                boxShadows: boxShadows
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            _RenderMergeableMaterialListBody materialRenderListBody = (_RenderMergeableMaterialListBody) renderObject;
            materialRenderListBody.axisDirection = _getDirection(context);
            materialRenderListBody.boxShadows = boxShadows;
        }
    }


    class _RenderMergeableMaterialListBody : RenderListBody {
        public _RenderMergeableMaterialListBody(
            List<RenderBox> children = null,
            AxisDirection axisDirection = AxisDirection.down,
            List<BoxShadow> boxShadows = null
        ) : base(children: children, axisDirection: axisDirection) {
            this.boxShadows = boxShadows;
        }

        public List<BoxShadow> boxShadows;

        void _paintShadows(Canvas canvas, Rect rect) {
            foreach (BoxShadow boxShadow in boxShadows) {
                Paint paint = boxShadow.toPaint();
                canvas.drawRRect(
                    MaterialConstantsUtils.kMaterialEdges[MaterialType.card].toRRect(rect), paint);
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            RenderBox child = firstChild;
            int i = 0;

            while (child != null) {
                ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                Rect rect = (childParentData.offset + offset) & child.size;
                if (i % 2 == 0) {
                    _paintShadows(context.canvas, rect);
                }

                child = childParentData.nextSibling;
                i++;
            }

            defaultPaint(context, offset);
        }
    }
}