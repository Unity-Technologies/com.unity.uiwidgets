using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoTabController : ChangeNotifier {

        public CupertinoTabController(int? initialIndex = 0) {
            D.assert(initialIndex != null);
            D.assert(initialIndex >= 0);
            _index = initialIndex.Value;
        }

        public bool _isDisposed = false;

        public int index {
            get { return _index; }
            set {
                D.assert(value >= 0);
                if (_index == value) {
                    return;
                }
                _index = value;
                notifyListeners();
            }
        }
        int _index;
        public override void dispose() {
             base.dispose();
            _isDisposed = true;
        }
    }

    public class CupertinoTabScaffold : StatefulWidget {
        public CupertinoTabScaffold(
            Key key = null,
            CupertinoTabBar tabBar = null,
            IndexedWidgetBuilder tabBuilder = null,
            CupertinoTabController controller = null,
            Color backgroundColor = null,
            bool resizeToAvoidBottomInset = true
        ) : base(key: key) {
            D.assert(tabBar != null);
            D.assert(tabBuilder != null);
            D.assert(
                controller == null || controller.index < tabBar.items.Count, () =>
                    $"The CupertinoTabController's current index {controller.index} is " +
                    $"out2 of bounds for the tab bar with {tabBar.items.Count} tabs"
            );
            this.tabBar = tabBar;
            this.controller = controller;
            this.tabBuilder = tabBuilder;
            this.backgroundColor = backgroundColor;
            this.resizeToAvoidBottomInset = resizeToAvoidBottomInset;
        }

        public readonly CupertinoTabController controller;
        public readonly CupertinoTabBar tabBar;

        public readonly IndexedWidgetBuilder tabBuilder;

        public readonly Color backgroundColor;

        public readonly bool resizeToAvoidBottomInset;

        public override State createState() {
            return new _CupertinoTabScaffoldState();
        }
    }

    class _CupertinoTabScaffoldState : State<CupertinoTabScaffold> {
        int _currentPage;
        CupertinoTabController _controller;

        public override void initState() {
            base.initState();
            _updateTabController();
        }
        void _updateTabController( bool shouldDisposeOldController = false ) {
             CupertinoTabController newController =
                // User provided a new controller, update `_controller` with it.
                widget.controller
                ?? new CupertinoTabController(initialIndex: widget.tabBar.currentIndex);

            if (newController == _controller) {
                return;
            }

            if (shouldDisposeOldController) {
                _controller?.dispose();
            } else if (_controller?._isDisposed == false) {
                _controller.removeListener(_onCurrentIndexChange);
            }

            newController.addListener(_onCurrentIndexChange);
            _controller = newController;
        }
        void _onCurrentIndexChange() {
            D.assert(
                _controller.index >= 0 && _controller.index < widget.tabBar.items.Count,()=>
                $"The {GetType()}'s current index {_controller.index} is " +
            $"out of bounds for the tab bar with {widget.tabBar.items.Count} tabs"
                );
            setState(()=> {});
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            CupertinoTabScaffold oldWidget = _oldWidget as CupertinoTabScaffold;
            base.didUpdateWidget(oldWidget);
            if (widget.controller != oldWidget.controller) {
                _updateTabController(shouldDisposeOldController: oldWidget.controller == null);
            } else if (_controller.index >= widget.tabBar.items.Count) {
                _controller.index = widget.tabBar.items.Count - 1;
            }
        }
        public override Widget build(BuildContext context) {
            List<Widget> stacked = new List<Widget> { };

            MediaQueryData existingMediaQuery = MediaQuery.of(context);
            MediaQueryData newMediaQuery = MediaQuery.of(context);

            Widget content = new _TabSwitchingView(
                currentTabIndex: _controller.index,
                tabNumber: widget.tabBar.items.Count,
                tabBuilder: widget.tabBuilder
            );
            EdgeInsets contentPadding = EdgeInsets.zero;

            if (widget.resizeToAvoidBottomInset) {
                newMediaQuery = newMediaQuery.removeViewInsets(removeBottom: true);
                contentPadding = EdgeInsets.only(bottom: existingMediaQuery.viewInsets.bottom);
            }

            if (widget.tabBar != null &&
                (!widget.resizeToAvoidBottomInset ||
                 widget.tabBar.preferredSize.height > existingMediaQuery.viewInsets.bottom)) {
                float bottomPadding = widget.tabBar.preferredSize.height + existingMediaQuery.padding.bottom;

                if (widget.tabBar.opaque(context)) {
                    contentPadding = EdgeInsets.only(bottom: bottomPadding);
                    newMediaQuery = newMediaQuery.removePadding(removeBottom: true);
                }
                else {
                    newMediaQuery = newMediaQuery.copyWith(
                        padding: newMediaQuery.padding.copyWith(
                            bottom: bottomPadding
                        )
                    );
                }
            }

            content = new MediaQuery(
                data: newMediaQuery,
                child: new Padding(
                    padding: contentPadding,
                    child: content
                )
            );
            stacked.Add(content);
            stacked.Add(
                new MediaQuery(
                        data: existingMediaQuery.copyWith(textScaleFactor: 1),
                        child: new Align(
                            alignment: Alignment.bottomCenter,
                            child: widget.tabBar.copyWith(
                                currentIndex: _controller.index,
                                onTap: (int newIndex) =>{
                                    _controller.index = newIndex;
                                    if (widget.tabBar.onTap != null)
                                        widget.tabBar.onTap(newIndex);
                                }
                            )
                        )
                )
                );

            return new DecoratedBox(
                decoration: new BoxDecoration(
                    color: CupertinoDynamicColor.resolve(widget.backgroundColor, context)
                           ?? CupertinoTheme.of(context).scaffoldBackgroundColor
                ),
                child: new Stack(
                    children: stacked
                )
            );
        }
        
        public override void dispose() {
          
            if (widget.controller == null) {
                _controller?.dispose();
            } else if (_controller?._isDisposed == false) {
                _controller.removeListener(_onCurrentIndexChange);
            }

            base.dispose();
        }
    
    }

    class _TabSwitchingView : StatefulWidget {
        public _TabSwitchingView(
            int currentTabIndex,
            int tabNumber,
            IndexedWidgetBuilder tabBuilder
        ) {
            D.assert(tabNumber > 0);
            D.assert(tabBuilder != null);
            this.currentTabIndex = currentTabIndex;
            this.tabNumber = tabNumber;
            this.tabBuilder = tabBuilder;
        }

        public readonly int currentTabIndex;
        public readonly int tabNumber;
        public readonly IndexedWidgetBuilder tabBuilder;

        public override State createState() {
            return new _TabSwitchingViewState();
        }
    }

    class _TabSwitchingViewState : State<_TabSwitchingView> {
    
        public readonly List<bool> shouldBuildTab = new List<bool>();
        public readonly List<FocusScopeNode> tabFocusNodes = new List<FocusScopeNode>();
        public readonly List<FocusScopeNode> discardedNodes = new List<FocusScopeNode>();


        public override void initState() {
            base.initState();
            List<bool> tabBool = new List<bool>();
            for (int i = 0; i < widget.tabNumber; i++) {
                tabBool.Add(false);
            }
            foreach (var tab in tabBool) {
                shouldBuildTab.Add(tab);
            }
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _focusActiveTab();
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            _TabSwitchingView oldWidget = _oldWidget as _TabSwitchingView;
            base.didUpdateWidget(oldWidget);
            int lengthDiff = widget.tabNumber - shouldBuildTab.Count;
            if (lengthDiff > 0) {
                List<bool> tabBool = new List<bool>();
                for (int i = 0; i < lengthDiff; i++) {
                    tabBool.Add(false);
                }
                foreach (var tab in tabBool) {
                    shouldBuildTab.Add(tab);
                }
            } 
            else if (lengthDiff < 0) {
                for (int i = widget.tabNumber; i < shouldBuildTab.Count; i++) {
                    shouldBuildTab.RemoveAt(i);
                }
               
            }
            _focusActiveTab();
        }

        void _focusActiveTab() {
            if (tabFocusNodes.Count!= widget.tabNumber) {
                if (tabFocusNodes.Count> widget.tabNumber) {
                    for (int i = widget.tabNumber; i < tabFocusNodes.Count; i++) {
                        discardedNodes.Add(tabFocusNodes[i]);
                    }
                    for (int i = widget.tabNumber; i < tabFocusNodes.Count; i++ ){
                        tabFocusNodes.RemoveAt(i);
                    }
                }

                else {
                    List<FocusScopeNode> scopeNodes = new List<FocusScopeNode>();
                    var length = widget.tabNumber - tabFocusNodes.Count;
                    for (int i = 0; i < length; i++) {
                        scopeNodes.Add(new FocusScopeNode(debugLabel: $"CupertinoTabScaffold Tab {i + tabFocusNodes.Count}")
                        );
                    }
                    tabFocusNodes.AddRange(scopeNodes);

                }

            }
            FocusScope.of(context).setFirstFocus(tabFocusNodes[widget.currentTabIndex]);
        }

        public override void dispose() {
            foreach(FocusScopeNode focusScopeNode in tabFocusNodes) {
                focusScopeNode.dispose();
            }
            foreach( FocusScopeNode focusScopeNode in discardedNodes) {
                focusScopeNode.dispose();
            }
            base.dispose();
        }

        public override Widget build(BuildContext context) {

            List<Widget> stages = new List<Widget>();
            int count = widget.tabNumber;
            for (int i = 0; i < count; i++) {
                bool active = i == widget.currentTabIndex;
                shouldBuildTab[i] = active || shouldBuildTab[i];
                int temp = i;
                stages.Add(new Offstage(
                    offstage: !active,
                    child: new TickerMode(
                        enabled: active,
                        child: new FocusScope(
                            node: tabFocusNodes[i],
                            child: new Builder(
                                builder: (BuildContext context1) => {
                                    return shouldBuildTab[temp] ? widget.tabBuilder(context1, temp) : new Container();
                                }
                            )
                        )
                    )
                ));
               
            }

            return new Stack(
                fit: StackFit.expand,
                children:stages
                    
            );
        }
    }
}