using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public partial class material_ {
        public const float _kPanelHeaderCollapsedHeight = kMinInteractiveDimension;

        public static readonly EdgeInsets _kPanelHeaderExpandedDefaultPadding = EdgeInsets.symmetric(
            vertical: 64.0f - _kPanelHeaderCollapsedHeight);
    }


    class _SaltedKey<S, V> : LocalKey {
        public _SaltedKey(
            S salt,
            V value) {
            this.salt = salt;
            this.value = value;
        }

        public readonly S salt;

        public readonly V value;

        public bool Equals(_SaltedKey<S, V> other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.salt.Equals(salt)
                   && other.value.Equals(value);
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

            return Equals((_SaltedKey<S, V>) obj);
        }

        public static bool operator ==(_SaltedKey<S, V> left, _SaltedKey<S, V> right) {
            return Equals(left, right);
        }

        public static bool operator !=(_SaltedKey<S, V> left, _SaltedKey<S, V> right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = salt.GetHashCode();
                hashCode = (hashCode * 397) ^ value.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            string saltString = salt is string ? "<\'" + salt + "\'>" : "<" + salt + ">";
            string valueString = value is string ? "<\'" + value + "\'>" : "<" + value + ">";
            return "[" + saltString + " " + valueString + "]";
        }
    }

    public delegate void ExpansionPanelCallback(int panelIndex, bool isExpanded);

    public delegate Widget ExpansionPanelHeaderBuilder(BuildContext context, bool isExpanded);


    public class ExpansionPanel {
        public ExpansionPanel(
            ExpansionPanelHeaderBuilder headerBuilder = null,
            Widget body = null,
            bool isExpanded = false,
            bool canTapOnHeader = false) {
            D.assert(headerBuilder != null);
            D.assert(body != null);
            this.headerBuilder = headerBuilder;
            this.body = body;
            this.isExpanded = isExpanded;
            this.canTapOnHeader = false;
        }

        public readonly ExpansionPanelHeaderBuilder headerBuilder;

        public readonly Widget body;

        public readonly bool isExpanded;

        public readonly bool canTapOnHeader;
    }


    public class ExpansionPanelRadio : ExpansionPanel {
        public ExpansionPanelRadio(
            object value = null,
            ExpansionPanelHeaderBuilder headerBuilder = null,
            Widget body = null,
            bool canTapOnHeader = false)
            : base(body: body, headerBuilder: headerBuilder, canTapOnHeader: canTapOnHeader) {
            D.assert(headerBuilder != null);
            D.assert(body != null);
            D.assert(value != null);
            this.value = value;
        }

        public readonly object value;
    }

    public class ExpansionPanelList : StatefulWidget {
        public ExpansionPanelList(
            Key key = null,
            List<ExpansionPanel> children = null,
            ExpansionPanelCallback expansionCallback = null,
            TimeSpan? animationDuration = null,
            EdgeInsets expandedHeaderPadding = null
        ) : base(key: key) {
            this.children = children ?? new List<ExpansionPanel>();
            this.expansionCallback = expansionCallback;
            this.animationDuration = animationDuration ?? material_.kThemeChangeDuration;
            this.expandedHeaderPadding = expandedHeaderPadding ?? material_._kPanelHeaderExpandedDefaultPadding;
            _allowOnlyOnePanelOpen = false;
            initialOpenPanelValue = null;
        }

        ExpansionPanelList(
            Key key = null,
            List<ExpansionPanel> children = null,
            ExpansionPanelCallback expansionCallback = null,
            TimeSpan? animationDuration = null,
            object initialOpenPanelValue = null,
            EdgeInsets expandedHeaderPadding = null
        ) : base(key: key) {
            this.children = children ?? new List<ExpansionPanel>();
            this.expansionCallback = expansionCallback;
            this.animationDuration = animationDuration ?? material_.kThemeChangeDuration;
            this.expandedHeaderPadding = expandedHeaderPadding ?? material_._kPanelHeaderExpandedDefaultPadding;
            _allowOnlyOnePanelOpen = true;
            this.initialOpenPanelValue = initialOpenPanelValue;
        }

        public static ExpansionPanelList radio(
            Key key = null,
            List<ExpansionPanelRadio> children = null,
            ExpansionPanelCallback expansionCallback = null,
            TimeSpan? animationDuration = null,
            object initialOpenPanelValue = null) {
            children = children ?? new List<ExpansionPanelRadio>();
            var radio = new ExpansionPanelList(
                key: key,
                children: children.Cast<ExpansionPanel>().ToList(),
                expansionCallback: expansionCallback,
                animationDuration: animationDuration,
                initialOpenPanelValue: initialOpenPanelValue
            );
            return radio;
        }

        public readonly List<ExpansionPanel> children;

        public readonly ExpansionPanelCallback expansionCallback;

        public readonly TimeSpan animationDuration;

        public readonly bool _allowOnlyOnePanelOpen;

        public readonly object initialOpenPanelValue;

        public readonly EdgeInsets expandedHeaderPadding;

        public override State createState() {
            return new _ExpansionPanelListState();
        }
    }


    public class _ExpansionPanelListState : State<ExpansionPanelList> {
        ExpansionPanelRadio _currentOpenPanel;

        public override void initState() {
            base.initState();
            if (widget._allowOnlyOnePanelOpen) {
                D.assert(_allIdentifierUnique(), () => "All ExpansionPanelRadio identifier values must be unique.");
                foreach (ExpansionPanelRadio child in widget.children) {
                    if (widget.initialOpenPanelValue != null) {
                        _currentOpenPanel =
                            searchPanelByValue(widget.children.Cast<ExpansionPanelRadio>().ToList(),
                                widget.initialOpenPanelValue);
                    }
                }
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            ExpansionPanelList _oldWidget = (ExpansionPanelList) oldWidget;
            if (widget._allowOnlyOnePanelOpen) {
                D.assert(_allIdentifierUnique(), () => "All ExpansionPanelRadio identifier values must be unique.");
                if (!_oldWidget._allowOnlyOnePanelOpen) {
                    _currentOpenPanel =
                        searchPanelByValue(widget.children.Cast<ExpansionPanelRadio>().ToList(),
                            widget.initialOpenPanelValue);
                }
            }
            else {
                _currentOpenPanel = null;
            }
        }

        bool _allIdentifierUnique() {
            Dictionary<object, bool> identifierMap = new Dictionary<object, bool>();
            foreach (ExpansionPanelRadio child in widget.children.Cast<ExpansionPanelRadio>()) {
                identifierMap[child.value] = true;
            }

            return identifierMap.Count == widget.children.Count;
        }

        bool _isChildExpanded(int index) {
            if (widget._allowOnlyOnePanelOpen) {
                ExpansionPanelRadio radioWidget = (ExpansionPanelRadio) widget.children[index];
                return _currentOpenPanel?.value == radioWidget.value;
            }

            return widget.children[index].isExpanded;
        }

        void _handlePressed(bool isExpanded, int index) {
            if (widget.expansionCallback != null) {
                widget.expansionCallback(index, isExpanded);
            }

            if (widget._allowOnlyOnePanelOpen) {
                ExpansionPanelRadio pressedChild = (ExpansionPanelRadio) widget.children[index];

                for (int childIndex = 0; childIndex < widget.children.Count; childIndex++) {
                    ExpansionPanelRadio child = (ExpansionPanelRadio) widget.children[childIndex];
                    if (widget.expansionCallback != null && childIndex != index &&
                        child.value == _currentOpenPanel?.value) {
                        widget.expansionCallback(childIndex, false);
                    }
                }

                setState(() => { _currentOpenPanel = isExpanded ? null : pressedChild; });
            }
        }

        public ExpansionPanelRadio searchPanelByValue(List<ExpansionPanelRadio> panels, Object value) {
            foreach (ExpansionPanelRadio panel in panels) {
                if (panel.value == value)
                    return panel;
            }

            return null;
        }

        public override Widget build(BuildContext context) {
            List<MergeableMaterialItem> items = new List<MergeableMaterialItem>();

            for (int i = 0; i < widget.children.Count; i++) {
                int expandIndex = i;
                if (_isChildExpanded(expandIndex) && expandIndex != 0 && !_isChildExpanded(expandIndex - 1)) {
                    items.Add(new MaterialGap(
                        key: new _SaltedKey<BuildContext, int>(context, expandIndex * 2 - 1)));
                }

                ExpansionPanel child = widget.children[expandIndex];
                Widget headerWidget = child.headerBuilder(
                    context,
                    _isChildExpanded(expandIndex)
                );


                Widget expandIconContainer = new Container(
                    margin: EdgeInsetsDirectional.only(end: 8.0f),
                    child: new ExpandIcon(
                        isExpanded: _isChildExpanded(expandIndex),
                        padding: EdgeInsets.all(16.0f),
                        onPressed: !child.canTapOnHeader
                            ? (bool isExpanded) => _handlePressed(isExpanded, expandIndex)
                            : (ValueChanged<bool>) null
                    )
                );
                Widget header = new Row(
                    children: new List<Widget> {
                        new Expanded(
                            child: new AnimatedContainer(
                                duration: widget.animationDuration,
                                curve: Curves.fastOutSlowIn,
                                margin: _isChildExpanded(expandIndex) ? widget.expandedHeaderPadding : EdgeInsets.zero,
                                child: new ConstrainedBox(
                                    constraints: new BoxConstraints(
                                        minHeight: material_._kPanelHeaderCollapsedHeight),
                                    child: headerWidget
                                )
                            )
                        ),
                        expandIconContainer,
                    }
                );
                if (child.canTapOnHeader) {
                    header = new InkWell(
                        onTap: () => _handlePressed(_isChildExpanded(expandIndex), expandIndex),
                        child: header
                    );
                }

                items.Add(new MaterialSlice(
                        key: new _SaltedKey<BuildContext, int>(context, expandIndex * 2),
                        child: new Column(
                            children: new List<Widget> {
                                header,
                                new AnimatedCrossFade(
                                    firstChild: new Container(height: 0.0f),
                                    secondChild: child.body,
                                    firstCurve: new Interval(0.0f, 0.6f, curve: Curves.fastOutSlowIn),
                                    secondCurve: new Interval(0.4f, 1.0f, curve: Curves.fastOutSlowIn),
                                    sizeCurve: Curves.fastOutSlowIn,
                                    crossFadeState: _isChildExpanded(expandIndex)
                                        ? CrossFadeState.showSecond
                                        : CrossFadeState.showFirst,
                                    duration: widget.animationDuration
                                )
                            }
                        )
                    )
                );

                if (_isChildExpanded(expandIndex) && expandIndex != widget.children.Count - 1) {
                    items.Add(new MaterialGap(
                        key: new _SaltedKey<BuildContext, int>(context, expandIndex * 2 + 1)));
                }
            }

            return new MergeableMaterial(
                hasDividers: true,
                children: items);
        }
    }
}