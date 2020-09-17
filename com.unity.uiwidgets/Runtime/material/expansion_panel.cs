using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    static class ExpansionPanelUtils {
        public const float _kPanelHeaderCollapsedHeight = 48.0f;
        public const float _kPanelHeaderExpandedHeight = 64.0f;
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
            TimeSpan? animationDuration = null) : base(key: key) {
            this.children = children ?? new List<ExpansionPanel>();
            this.expansionCallback = expansionCallback;
            this.animationDuration = animationDuration ?? Constants.kThemeChangeDuration;
            _allowOnlyOnePanelOpen = false;
            initialOpenPanelValue = null;
        }

        ExpansionPanelList(
            Key key = null,
            List<ExpansionPanel> children = null,
            ExpansionPanelCallback expansionCallback = null,
            TimeSpan? animationDuration = null,
            object initialOpenPanelValue = null) : base(key: key) {
            this.children = children ?? new List<ExpansionPanel>();
            this.expansionCallback = expansionCallback;
            this.animationDuration = animationDuration ?? Constants.kThemeChangeDuration;
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

        public override State createState() {
            return new _ExpansionPanelListState();
        }
    }


    public class _ExpansionPanelListState : State<ExpansionPanelList> {
        ExpansionPanelRadio _currentOpenPanel;

        public override void initState() {
            base.initState();
            if (widget._allowOnlyOnePanelOpen) {
                D.assert(_allIdentifierUnique(), () => "All object identifiers are not unique!");
                foreach (ExpansionPanelRadio child in widget.children) {
                    if (widget.initialOpenPanelValue != null &&
                        child.value == widget.initialOpenPanelValue) {
                        _currentOpenPanel = child;
                    }
                }
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            ExpansionPanelList _oldWidget = (ExpansionPanelList) oldWidget;
            if (widget._allowOnlyOnePanelOpen) {
                D.assert(_allIdentifierUnique(), () => "All object identifiers are not unique!");
                foreach (ExpansionPanelRadio newChild in widget.children) {
                    if (widget.initialOpenPanelValue != null &&
                        newChild.value == widget.initialOpenPanelValue) {
                        _currentOpenPanel = newChild;
                    }
                }
            }
            else if (_oldWidget._allowOnlyOnePanelOpen) {
                _currentOpenPanel = null;
            }
        }

        bool _allIdentifierUnique() {
            Dictionary<object, bool> identifierMap = new Dictionary<object, bool>();
            foreach (ExpansionPanelRadio child in widget.children) {
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

                _currentOpenPanel = isExpanded ? null : pressedChild;
            }

            setState(() => { });
        }

        public override Widget build(BuildContext context) {
            List<MergeableMaterialItem> items = new List<MergeableMaterialItem>();
            EdgeInsets kExpandedEdgeInsets = EdgeInsets.symmetric(
                vertical: ExpansionPanelUtils._kPanelHeaderExpandedHeight -
                          ExpansionPanelUtils._kPanelHeaderCollapsedHeight);

            for (int index = 0; index < widget.children.Count; index++) {
                int expandIndex = index;
                if (_isChildExpanded(index) && index != 0 && !_isChildExpanded(index - 1)) {
                    items.Add(new MaterialGap(
                        key: new _SaltedKey<BuildContext, int>(context, index * 2 - 1)));
                }

                ExpansionPanel child = widget.children[index];
                Widget headerWidget = child.headerBuilder(
                    context,
                    _isChildExpanded(index)
                );
                Row header = new Row(
                    children: new List<Widget> {
                        new Expanded(
                            child: new AnimatedContainer(
                                duration: widget.animationDuration,
                                curve: Curves.fastOutSlowIn,
                                margin: _isChildExpanded(index) ? kExpandedEdgeInsets : EdgeInsets.zero,
                                child: new ConstrainedBox(
                                    constraints: new BoxConstraints(
                                        minHeight: ExpansionPanelUtils._kPanelHeaderCollapsedHeight),
                                    child: headerWidget
                                )
                            )
                        ),
                        new Container(
                            margin: EdgeInsets.fromLTRB(0, 0, 8, 0),
                            child: new ExpandIcon(
                                isExpanded: _isChildExpanded(index),
                                padding: EdgeInsets.all(16.0f),
                                onPressed: !child.canTapOnHeader
                                    ? (ValueChanged<bool>) ((bool isExpanded) => {
                                        _handlePressed(isExpanded, expandIndex);
                                    })
                                    : null
                            )
                        )
                    }
                );

                items.Add(new MaterialSlice(
                        key: new _SaltedKey<BuildContext, int>(context, index * 2),
                        child: new Column(
                            children: new List<Widget> {
                                child.canTapOnHeader
                                    ? (Widget) new InkWell(
                                        onTap: () =>
                                            _handlePressed(_isChildExpanded(expandIndex), expandIndex),
                                        child: header
                                    )
                                    : header,
                                new AnimatedCrossFade(
                                    firstChild: new Container(height: 0.0f),
                                    secondChild: child.body,
                                    firstCurve: new Interval(0.0f, 0.6f, curve: Curves.fastOutSlowIn),
                                    secondCurve: new Interval(0.4f, 1.0f, curve: Curves.fastOutSlowIn),
                                    sizeCurve: Curves.fastOutSlowIn,
                                    crossFadeState: _isChildExpanded(index)
                                        ? CrossFadeState.showSecond
                                        : CrossFadeState.showFirst,
                                    duration: widget.animationDuration
                                )
                            }
                        )
                    )
                );

                if (_isChildExpanded(index) && index != widget.children.Count - 1) {
                    items.Add(new MaterialGap(
                        key: new _SaltedKey<BuildContext, int>(context, index * 2 + 1)));
                }
            }

            return new MergeableMaterial(
                hasDividers: true,
                children: items);
        }
    }
}