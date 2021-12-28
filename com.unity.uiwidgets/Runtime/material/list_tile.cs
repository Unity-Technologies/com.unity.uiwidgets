using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public enum ListTileStyle {
        list,
        drawer
    }

    public class ListTileTheme : InheritedTheme {
        public ListTileTheme(
            Key key = null,
            bool dense = false,
            ListTileStyle style = ListTileStyle.list,
            Color selectedColor = null,
            Color iconColor = null,
            Color textColor = null,
            EdgeInsetsGeometry contentPadding = null,
            Widget child = null) : base(key: key, child: child) {
            this.dense = dense;
            this.style = style;
            this.selectedColor = selectedColor;
            this.iconColor = iconColor;
            this.textColor = textColor;
            this.contentPadding = contentPadding;
        }

        public static Widget merge(
            Key key = null,
            bool? dense = null,
            ListTileStyle? style = null,
            Color selectedColor = null,
            Color iconColor = null,
            Color textColor = null,
            EdgeInsetsGeometry contentPadding = null,
            Widget child = null) {
            D.assert(child != null);
            return new Builder(
                builder: (BuildContext context) => {
                    ListTileTheme parent = of(context);
                    return new ListTileTheme(
                        key: key,
                        dense: dense ?? parent.dense,
                        style: style ?? parent.style,
                        selectedColor: selectedColor ?? parent.selectedColor,
                        iconColor: iconColor ?? parent.iconColor,
                        textColor: textColor ?? parent.textColor,
                        contentPadding: contentPadding ?? parent.contentPadding,
                        child: child);
                }
            );
        }

        public readonly bool dense;

        public readonly ListTileStyle style;

        public readonly Color selectedColor;

        public readonly Color iconColor;

        public readonly Color textColor;

        public readonly EdgeInsetsGeometry contentPadding;

        public static ListTileTheme of(BuildContext context) {
            ListTileTheme result = context.dependOnInheritedWidgetOfExactType<ListTileTheme>();
            return result ?? new ListTileTheme();
        }

        public override Widget wrap(BuildContext context, Widget child) {
            ListTileTheme ancestorTheme = context.findAncestorWidgetOfExactType<ListTileTheme>();
            return ReferenceEquals(this, ancestorTheme)
                ? child
                : new ListTileTheme(
                    dense: dense,
                    style: style,
                    selectedColor: selectedColor,
                    iconColor: iconColor,
                    textColor: textColor,
                    contentPadding: contentPadding,
                    child: child
                );
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            ListTileTheme _oldWidget = (ListTileTheme) oldWidget;
            return dense != _oldWidget.dense ||
                   style != _oldWidget.style ||
                   selectedColor != _oldWidget.selectedColor ||
                   iconColor != _oldWidget.iconColor ||
                   textColor != _oldWidget.textColor ||
                   contentPadding != _oldWidget.contentPadding;
        }
    }

    public enum ListTileControlAffinity {
        leading,
        trailing,
        platform
    }

    public class ListTile : StatelessWidget {
        public ListTile(
            Key key = null,
            Widget leading = null,
            Widget title = null,
            Widget subtitle = null,
            Widget trailing = null,
            bool isThreeLine = false,
            bool? dense = null,
            EdgeInsetsGeometry contentPadding = null,
            bool enabled = true,
            GestureTapCallback onTap = null,
            GestureLongPressCallback onLongPress = null,
            bool selected = false
        ) : base(key: key) {
            D.assert(!isThreeLine || subtitle != null);
            this.leading = leading;
            this.title = title;
            this.subtitle = subtitle;
            this.trailing = trailing;
            this.isThreeLine = isThreeLine;
            this.dense = dense;
            this.contentPadding = contentPadding;
            this.enabled = enabled;
            this.onTap = onTap;
            this.onLongPress = onLongPress;
            this.selected = selected;
        }

        public readonly Widget leading;

        public readonly Widget title;

        public readonly Widget subtitle;

        public readonly Widget trailing;

        public readonly bool isThreeLine;

        public readonly bool? dense;

        public readonly EdgeInsetsGeometry contentPadding;

        public readonly bool enabled;

        public readonly GestureTapCallback onTap;

        public readonly GestureLongPressCallback onLongPress;

        public readonly bool selected;

        public static IEnumerable<Widget> divideTiles(BuildContext context = null, IEnumerable<Widget> tiles = null,
            Color color = null) {
            D.assert(tiles != null);
            D.assert(color != null || context != null);

            IEnumerator<Widget> enumerator = tiles.GetEnumerator();
            List<Widget> result = new List<Widget> { };

            Decoration decoration = new BoxDecoration(
                border: new Border(
                    bottom: Divider.createBorderSide(context, color: color)
                )
            );

            Widget tile = enumerator.Current;
            while (enumerator.MoveNext()) {
                result.Add(new DecoratedBox(
                    position: DecorationPosition.foreground,
                    decoration: decoration,
                    child: tile
                ));
                tile = enumerator.Current;
            }

            return result;
        }

        Color _iconColor(ThemeData theme, ListTileTheme tileTheme) {
            if (!enabled) {
                return theme.disabledColor;
            }

            if (selected && tileTheme?.selectedColor != null) {
                return tileTheme.selectedColor;
            }

            if (!selected && tileTheme?.iconColor != null) {
                return tileTheme.iconColor;
            }

            switch (theme.brightness) {
                case Brightness.light:
                    return selected ? theme.primaryColor : Colors.black45;
                case Brightness.dark:
                    return selected ? theme.accentColor : null;
            }

            return null;
        }

        Color _textColor(ThemeData theme, ListTileTheme tileTheme, Color defaultColor) {
            if (!enabled) {
                return theme.disabledColor;
            }

            if (selected && tileTheme?.selectedColor != null) {
                return tileTheme.selectedColor;
            }

            if (!selected && tileTheme?.textColor != null) {
                return tileTheme.textColor;
            }

            if (selected) {
                switch (theme.brightness) {
                    case Brightness.light:
                        return theme.primaryColor;
                    case Brightness.dark:
                        return theme.accentColor;
                }
            }

            return defaultColor;
        }

        bool _isDenseLayout(ListTileTheme tileTheme) {
            return dense ?? tileTheme?.dense ?? false;
        }

        TextStyle _titleTextStyle(ThemeData theme, ListTileTheme tileTheme) {
            TextStyle style = null;
            if (tileTheme != null) {
                switch (tileTheme.style) {
                    case ListTileStyle.drawer:
                        style = theme.textTheme.bodyText1;
                        break;
                    case ListTileStyle.list:
                        style = theme.textTheme.subtitle1;
                        break;
                }
            }
            else {
                style = theme.textTheme.subtitle1;
            }

            Color color = _textColor(theme, tileTheme, style.color);
            return _isDenseLayout(tileTheme)
                ? style.copyWith(fontSize: 13.0f, color: color)
                : style.copyWith(color: color);
        }

        TextStyle _subtitleTextStyle(ThemeData theme, ListTileTheme tileTheme) {
            TextStyle style = theme.textTheme.bodyText2;
            Color color = _textColor(theme, tileTheme, theme.textTheme.caption.color);
            return _isDenseLayout(tileTheme)
                ? style.copyWith(color: color, fontSize: 12.0f)
                : style.copyWith(color: color);
        }

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            ThemeData theme = Theme.of(context);
            ListTileTheme tileTheme = ListTileTheme.of(context);

            IconThemeData iconThemeData = null;
            if (leading != null || trailing != null) {
                iconThemeData = new IconThemeData(color: _iconColor(theme, tileTheme));
            }

            Widget leadingIcon = null;
            if (leading != null) {
                leadingIcon = IconTheme.merge(
                    data: iconThemeData,
                    child: leading);
            }

            TextStyle titleStyle = _titleTextStyle(theme, tileTheme);
            Widget titleText = new AnimatedDefaultTextStyle(
                style: titleStyle,
                duration: material_.kThemeChangeDuration,
                child: title ?? new SizedBox()
            );

            Widget subtitleText = null;
            TextStyle subtitleStyle = null;
            if (subtitle != null) {
                subtitleStyle = _subtitleTextStyle(theme, tileTheme);
                subtitleText = new AnimatedDefaultTextStyle(
                    style: subtitleStyle,
                    duration: material_.kThemeChangeDuration,
                    child: subtitle);
            }

            Widget trailingIcon = null;
            if (trailing != null) {
                trailingIcon = IconTheme.merge(
                    data: iconThemeData,
                    child: trailing);
            }

            EdgeInsets _defaultContentPadding = EdgeInsets.symmetric(horizontal: 16.0f);
            TextDirection textDirection = Directionality.of(context);
            EdgeInsets resolvedContentPadding =
                contentPadding?.resolve(textDirection) ?? tileTheme?.contentPadding?.resolve(textDirection) ?? _defaultContentPadding;

            return new InkWell(
                onTap: enabled ? onTap : null,
                onLongPress: enabled ? onLongPress : null,
                canRequestFocus: enabled,
                child: new SafeArea(
                    top: false,
                    bottom: false,
                    mininum: resolvedContentPadding,
                    child: new _ListTile(
                        leading: leadingIcon,
                        title: titleText,
                        subtitle: subtitleText,
                        trailing: trailingIcon,
                        isDense: _isDenseLayout(tileTheme),
                        isThreeLine: isThreeLine,
                        titleBaselineType: titleStyle.textBaseline,
                        subtitleBaselineType: subtitleStyle?.textBaseline
                    )
                )
            );
        }
    }

    public enum _ListTileSlot {
        leading,
        title,
        subtitle,
        trailing
    }

    public class _ListTile : RenderObjectWidget {
        public _ListTile(
            Key key = null,
            Widget leading = null,
            Widget title = null,
            Widget subtitle = null,
            Widget trailing = null,
            bool? isThreeLine = null,
            bool? isDense = null,
            TextBaseline? titleBaselineType = null,
            TextBaseline? subtitleBaselineType = null) : base(key: key) {
            D.assert(isThreeLine != null);
            D.assert(isDense != null);
            D.assert(titleBaselineType != null);
            this.leading = leading;
            this.title = title;
            this.subtitle = subtitle;
            this.trailing = trailing;
            this.isThreeLine = isThreeLine ?? false;
            this.isDense = isDense ?? false;
            this.titleBaselineType = titleBaselineType ?? TextBaseline.alphabetic;
            this.subtitleBaselineType = subtitleBaselineType;
        }

        public readonly Widget leading;

        public readonly Widget title;

        public readonly Widget subtitle;

        public readonly Widget trailing;

        public readonly bool isThreeLine;

        public readonly bool isDense;

        public readonly TextBaseline titleBaselineType;

        public readonly TextBaseline? subtitleBaselineType;

        public override Element createElement() {
            return new _ListTileElement(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderListTile(
                isThreeLine: isThreeLine,
                isDense: isDense,
                titleBaselineType: titleBaselineType,
                subtitleBaselineType: subtitleBaselineType
            );
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            _RenderListTile _renderObject = (_RenderListTile) renderObject;
            _renderObject.isThreeLine = isThreeLine;
            _renderObject.isDense = isDense;
            _renderObject.titleBaselineType = titleBaselineType;
            _renderObject.subtitleBaselineType = subtitleBaselineType;
        }
    }


    public class _ListTileElement : RenderObjectElement {
        public _ListTileElement(RenderObjectWidget widget) : base(widget) {
        }

        readonly Dictionary<_ListTileSlot, Element> slotToChild = new Dictionary<_ListTileSlot, Element>();
        readonly Dictionary<Element, _ListTileSlot> childToSlot = new Dictionary<Element, _ListTileSlot>();

        public new _ListTile widget {
            get { return (_ListTile) base.widget; }
        }

        public new _RenderListTile renderObject {
            get { return (_RenderListTile) base.renderObject; }
        }

        public override void visitChildren(ElementVisitor visitor) {
            foreach (var element in slotToChild.Values) {
                visitor(element);
            }
        }

        public override void forgetChild(Element child) {
            D.assert(slotToChild.Values.Contains(child));
            D.assert(childToSlot.Keys.Contains(child));
            _ListTileSlot slot = childToSlot[child];
            childToSlot.Remove(child);
            slotToChild.Remove(slot);
            base.forgetChild(child);
        }

        void _mountChild(Widget widget, _ListTileSlot slot) {
            Element oldChild = slotToChild.getOrDefault(slot);
            Element newChild = updateChild(oldChild, widget, slot);
            if (oldChild != null) {
                slotToChild.Remove(slot);
                childToSlot.Remove(oldChild);
            }

            if (newChild != null) {
                slotToChild[slot] = newChild;
                childToSlot[newChild] = slot;
            }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            _mountChild(widget.leading, _ListTileSlot.leading);
            _mountChild(widget.title, _ListTileSlot.title);
            _mountChild(widget.subtitle, _ListTileSlot.subtitle);
            _mountChild(widget.trailing, _ListTileSlot.trailing);
        }

        void _updateChild(Widget widget, _ListTileSlot slot) {
            Element oldChild = slotToChild.getOrDefault(slot);
            Element newChild = updateChild(oldChild, widget, slot);
            if (oldChild != null) {
                childToSlot.Remove(oldChild);
                slotToChild.Remove(slot);
            }

            if (newChild != null) {
                slotToChild[slot] = newChild;
                childToSlot[newChild] = slot;
            }
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(widget == newWidget);
            _updateChild(widget.leading, _ListTileSlot.leading);
            _updateChild(widget.title, _ListTileSlot.title);
            _updateChild(widget.subtitle, _ListTileSlot.subtitle);
            _updateChild(widget.trailing, _ListTileSlot.trailing);
        }

        void _updateRenderObject(RenderBox child, _ListTileSlot slot) {
            switch (slot) {
                case _ListTileSlot.leading:
                    renderObject.leading = (RenderBox) child;
                    break;
                case _ListTileSlot.title:
                    renderObject.title = (RenderBox) child;
                    break;
                case _ListTileSlot.subtitle:
                    renderObject.subtitle = (RenderBox) child;
                    break;
                case _ListTileSlot.trailing:
                    renderObject.trailing = (RenderBox) child;
                    break;
            }
        }

        protected override void insertChildRenderObject(RenderObject child, object slotValue) {
            D.assert(child is RenderBox);
            D.assert(slotValue is _ListTileSlot);
            _ListTileSlot slot = (_ListTileSlot) slotValue;
            _updateRenderObject((RenderBox) child, slot);
            D.assert(renderObject.childToSlot.Keys.Contains(child));
            D.assert(renderObject.slotToChild.Keys.Contains(slot));
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(child is RenderBox);
            D.assert(renderObject.childToSlot.Keys.Contains(child));
            _ListTileSlot slot = renderObject.childToSlot[(RenderBox) child];
            _updateRenderObject(null, slot);
            D.assert(!renderObject.childToSlot.Keys.Contains(child));
            D.assert(!renderObject.slotToChild.Keys.Contains(slot));
        }

        protected override void moveChildRenderObject(RenderObject child, object slotValue) {
            D.assert(false, () => "not reachable");
        }
    }


    public class _RenderListTile : RenderBox {
        public _RenderListTile(
            bool? isDense = null,
            bool? isThreeLine = null,
            TextBaseline? titleBaselineType = null,
            TextBaseline? subtitleBaselineType = null) {
            D.assert(isDense != null);
            D.assert(isThreeLine != null);
            D.assert(titleBaselineType != null);
            _isDense = isDense ?? false;
            _isThreeLine = isThreeLine ?? false;
            _titleBaselineType = titleBaselineType ?? TextBaseline.alphabetic;
            _subtitleBaselineType = subtitleBaselineType;
        }

        const float _minLeadingWidth = 40.0f;

        const float _horizontalTitleGap = 16.0f;

        const float _minVerticalPadding = 4.0f;

        public readonly Dictionary<_ListTileSlot, RenderBox> slotToChild = new Dictionary<_ListTileSlot, RenderBox>();
        public readonly Dictionary<RenderBox, _ListTileSlot> childToSlot = new Dictionary<RenderBox, _ListTileSlot>();

        RenderBox _updateChild(RenderBox oldChild, RenderBox newChild, _ListTileSlot slot) {
            if (oldChild != null) {
                dropChild(oldChild);
                childToSlot.Remove(oldChild);
                slotToChild.Remove(slot);
            }

            if (newChild != null) {
                childToSlot[newChild] = slot;
                slotToChild[slot] = newChild;
                adoptChild(newChild);
            }

            return newChild;
        }

        RenderBox _leading;

        public RenderBox leading {
            get { return _leading; }
            set { _leading = _updateChild(_leading, value, _ListTileSlot.leading); }
        }

        RenderBox _title;

        public RenderBox title {
            get { return _title; }
            set { _title = _updateChild(_title, value, _ListTileSlot.title); }
        }

        RenderBox _subtitle;

        public RenderBox subtitle {
            get { return _subtitle; }
            set { _subtitle = _updateChild(_subtitle, value, _ListTileSlot.subtitle); }
        }

        RenderBox _trailing;

        public RenderBox trailing {
            get { return _trailing; }
            set { _trailing = _updateChild(_trailing, value, _ListTileSlot.trailing); }
        }

        List<RenderObject> _children {
            get {
                List<RenderObject> ret = new List<RenderObject>();
                if (leading != null) {
                    ret.Add(leading);
                }

                if (title != null) {
                    ret.Add(title);
                }

                if (subtitle != null) {
                    ret.Add(subtitle);
                }

                if (trailing != null) {
                    ret.Add(trailing);
                }

                return ret;
            }
        }

        public bool isDense {
            get { return _isDense; }
            set {
                if (_isDense == value) {
                    return;
                }

                _isDense = value;
                markNeedsLayout();
            }
        }

        bool _isDense;

        public bool isThreeLine {
            get { return _isThreeLine; }
            set {
                if (_isThreeLine == value) {
                    return;
                }

                _isThreeLine = value;
                markNeedsLayout();
            }
        }

        bool _isThreeLine;

        public TextBaseline titleBaselineType {
            get { return _titleBaselineType; }
            set {
                if (_titleBaselineType == value) {
                    return;
                }

                _titleBaselineType = value;
                markNeedsLayout();
            }
        }

        TextBaseline _titleBaselineType;

        public TextBaseline? subtitleBaselineType {
            get { return _subtitleBaselineType; }
            set {
                if (_subtitleBaselineType == value) {
                    return;
                }

                _subtitleBaselineType = value;
                markNeedsLayout();
            }
        }

        TextBaseline? _subtitleBaselineType;

        public override void attach(object owner) {
            base.attach(owner);
            foreach (RenderBox child in _children) {
                child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            foreach (RenderBox child in _children) {
                child.detach();
            }
        }

        public override void redepthChildren() {
            foreach (var child in _children) {
                redepthChild(child);
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            foreach (var child in _children) {
                visitor(child);
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            List<DiagnosticsNode> value = new List<DiagnosticsNode>();

            void add(RenderBox child, string name) {
                if (child != null) {
                    value.Add(child.toDiagnosticsNode(name: name));
                }
            }

            add(leading, "leading");
            add(title, "title");
            add(subtitle, "subtitle");
            add(trailing, "trailing");
            return value;
        }

        protected override bool sizedByParent {
            get { return false; }
        }

        static float _minWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMinIntrinsicWidth(height);
        }

        static float _maxWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMaxIntrinsicWidth(height);
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            float leadingWidth = leading != null
                ? Mathf.Max(leading.getMinIntrinsicWidth(height), _minLeadingWidth) + _horizontalTitleGap
                : 0.0f;
            return leadingWidth + Mathf.Max(_minWidth(title, height), _minWidth(subtitle, height)) +
                   _maxWidth(trailing, height);
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            float leadingWidth = leading != null
                ? Mathf.Max(leading.getMaxIntrinsicWidth(height), _minLeadingWidth) + _horizontalTitleGap
                : 0.0f;
            return leadingWidth + Mathf.Max(_maxWidth(title, height), _maxWidth(subtitle, height)) +
                   _maxWidth(trailing, height);
        }

        float _defaultTileHeight {
            get {
                bool hasSubtitle = subtitle != null;
                bool isTwoLine = !isThreeLine && hasSubtitle;
                bool isOneLine = !isThreeLine && !hasSubtitle;

                if (isOneLine) {
                    return isDense ? 48.0f : 56.0f;
                }

                if (isTwoLine) {
                    return isDense ? 64.0f : 72.0f;
                }

                return isDense ? 76.0f : 88.0f;
            }
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            return Mathf.Max(
                _defaultTileHeight,
                title.getMinIntrinsicHeight(width) + subtitle?.getMinIntrinsicHeight(width) ?? 0.0f);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return computeMinIntrinsicHeight(width);
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            D.assert(title != null);
            BoxParentData parentData = (BoxParentData) title.parentData;
            return parentData.offset.dy + title.getDistanceToActualBaseline(baseline);
        }

        static float _boxBaseline(RenderBox box, TextBaseline baseline) {
            return box.getDistanceToBaseline(baseline) ?? 0.0f;
        }

        static Size _layoutBox(RenderBox box, BoxConstraints constraints) {
            if (box == null) {
                return Size.zero;
            }

            box.layout(constraints, parentUsesSize: true);
            return box.size;
        }

        static void _positionBox(RenderBox box, Offset offset) {
            BoxParentData parentData = (BoxParentData) box.parentData;
            parentData.offset = offset;
        }

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            bool hasLeading = leading != null;
            bool hasSubtitle = subtitle != null;
            bool hasTrailing = trailing != null;
            bool isTwoLine = !isThreeLine && hasSubtitle;
            bool isOneLine = !isThreeLine && !hasSubtitle;
            BoxConstraints maxIconHeightConstrains = new BoxConstraints(
                maxHeight: isDense ? 48.0f : 56.0f
            );
            BoxConstraints looseConstraints = constraints.loosen();
            BoxConstraints iconConstraints = looseConstraints.enforce(maxIconHeightConstrains);

            float tileWidth = looseConstraints.maxWidth;
            Size leadingSize = _layoutBox(leading, iconConstraints);
            Size trailingSize = _layoutBox(trailing, iconConstraints);
            D.assert(
                tileWidth != leadingSize.width,
                () => "Leading widget consumes entire width. Please use a sized widget."
            );
            D.assert(
                tileWidth != trailingSize.width,
                () => "Trailing widget consumes entire width. Please use a sized widget."
            );

            float titleStart = hasLeading
                ? Mathf.Max(_minLeadingWidth, leadingSize.width) + _horizontalTitleGap
                : 0.0f;
            BoxConstraints textConstraints = looseConstraints.tighten(
                width: tileWidth - titleStart - (hasTrailing ? trailingSize.width + _horizontalTitleGap : 0.0f));
            Size titleSize = _layoutBox(title, textConstraints);
            Size subtitleSize = _layoutBox(subtitle, textConstraints);

            float titleBaseline = 0.0f;
            float subtitleBaseline = 0.0f;
            if (isTwoLine) {
                titleBaseline = isDense ? 28.0f : 32.0f;
                subtitleBaseline = isDense ? 48.0f : 52.0f;
            }
            else if (isThreeLine) {
                titleBaseline = isDense ? 22.0f : 28.0f;
                subtitleBaseline = isDense ? 42.0f : 48.0f;
            }
            else {
                D.assert(isOneLine);
            }

            float defaultTileHeight = _defaultTileHeight;

            float tileHeight = 0.0f;
            float titleY = 0.0f;
            float subtitleY = 0.0f;
            if (!hasSubtitle) {
                tileHeight = Mathf.Max(defaultTileHeight, titleSize.height + 2.0f * _minVerticalPadding);
                titleY = (tileHeight - titleSize.height) / 2.0f;
            }
            else {
                D.assert(subtitleBaselineType != null);
                titleY = titleBaseline - _boxBaseline(title, titleBaselineType);
                subtitleY = subtitleBaseline -
                            _boxBaseline(subtitle, subtitleBaselineType ?? TextBaseline.alphabetic);
                tileHeight = defaultTileHeight;

                float titleOverlap = titleY + titleSize.height - subtitleY;
                if (titleOverlap > 0.0f) {
                    titleY -= titleOverlap / 2.0f;
                    subtitleY += titleOverlap / 2.0f;
                }

                if (titleY < _minVerticalPadding ||
                    (subtitleY + subtitleSize.height + _minVerticalPadding) > tileHeight) {
                    tileHeight = titleSize.height + subtitleSize.height + 2.0f * _minVerticalPadding;
                    titleY = _minVerticalPadding;
                    subtitleY = titleSize.height + _minVerticalPadding;
                }
            }

            float leadingY;
            float trailingY;

            if (tileHeight > 72.0f) {
                leadingY = 16.0f;
                trailingY = 16.0f;
            }
            else {
                leadingY = Mathf.Min((tileHeight - leadingSize.height) / 2.0f, 16.0f);
                trailingY = (tileHeight - trailingSize.height) / 2.0f;
            }

            if (hasLeading) {
                _positionBox(leading, new Offset(0.0f, leadingY));
            }

            _positionBox(title, new Offset(titleStart, titleY));
            if (hasSubtitle) {
                _positionBox(subtitle, new Offset(titleStart, subtitleY));
            }

            if (hasTrailing) {
                _positionBox(trailing, new Offset(tileWidth - trailingSize.width, trailingY));
            }

            size = constraints.constrain(new Size(tileWidth, tileHeight));
            D.assert(size.width == constraints.constrainWidth(tileWidth));
            D.assert(size.height == constraints.constrainHeight(tileHeight));
        }

        public override void paint(PaintingContext context, Offset offset) {
            void doPaint(RenderBox child) {
                if (child != null) {
                    BoxParentData parentData = (BoxParentData) child.parentData;
                    context.paintChild(child, parentData.offset + offset);
                }
            }

            doPaint(leading);
            doPaint(title);
            doPaint(subtitle);
            doPaint(trailing);
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position) {
            D.assert(position != null);
            foreach (RenderBox child in _children) {
                BoxParentData parentData = child.parentData as BoxParentData;
                bool isHit = result.addWithPaintOffset(
                    offset: parentData.offset,
                    position: position,
                    hitTest: (BoxHitTestResult resultIn, Offset transformed) => {
                        D.assert(transformed == position - parentData.offset);
                        return child.hitTest(resultIn, position: transformed);
                    }
                );
                if (isHit) {
                    return true;
                }
            }

            return false;
        }
    }
}