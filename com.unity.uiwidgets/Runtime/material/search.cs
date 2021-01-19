using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    public class SearchUtils {
        public static IPromise<object> showSearch(
            BuildContext context,
            SearchDelegate del,
            string query = ""
        ) {
            D.assert(del != null);
            D.assert(context != null);

            del.query = query ?? del.query;
            del._currentBody = _SearchBody.suggestions;
            return Navigator.of(context).push(new _SearchPageRoute(
                del: del
            ));
        }
    }

    public abstract class SearchDelegate {
        public abstract Widget buildSuggestions(BuildContext context);
        public abstract Widget buildResults(BuildContext context);
        public abstract Widget buildLeading(BuildContext context);
        public abstract List<Widget> buildActions(BuildContext context);

        public virtual ThemeData appBarTheme(BuildContext context) {
            D.assert(context != null);
            ThemeData theme = Theme.of(context);
            D.assert(theme != null);
            return theme.copyWith(
                primaryColor: Colors.white,
                primaryIconTheme: theme.primaryIconTheme.copyWith(color: Colors.grey),
                primaryColorBrightness: Brightness.light,
                primaryTextTheme: theme.textTheme
            );
        }

        public virtual string query {
            get { return _queryTextController.text; }
            set {
                D.assert(query != null);
                _queryTextController.text = value;
            }
        }

        public virtual void showResults(BuildContext context) {
            _focusNode.unfocus();
            _currentBody = _SearchBody.results;
        }

        public virtual void showSuggestions(BuildContext context) {
            FocusScope.of(context).requestFocus(_focusNode);
            _currentBody = _SearchBody.suggestions;
        }

        public virtual void close(BuildContext context, object result) {
            _currentBody = null;
            _focusNode.unfocus();
            var state = Navigator.of(context);
            state.popUntil((Route route) => route == _route);
            state.pop(result);
        }


        public virtual Animation<float> transitionAnimation {
            get { return _proxyAnimation; }
        }

        readonly internal FocusNode _focusNode = new FocusNode();

        readonly internal TextEditingController _queryTextController = new TextEditingController();

        readonly internal ProxyAnimation _proxyAnimation = new ProxyAnimation(Animations.kAlwaysDismissedAnimation);

        readonly internal ValueNotifier<_SearchBody?> _currentBodyNotifier = new ValueNotifier<_SearchBody?>(null);

        internal _SearchBody? _currentBody {
            get { return _currentBodyNotifier.value; }
            set { _currentBodyNotifier.value = value; }
        }

        internal _SearchPageRoute _route;
    }

    enum _SearchBody {
        suggestions,
        results
    }

    class _SearchPageRoute : PageRoute {
        public _SearchPageRoute(SearchDelegate del) {
            D.assert(del != null);
            D.assert(del._route == null,
                () => $"The {this.del.GetType()} instance is currently used by another active " +
                      "search. Please close that search by calling close() on the SearchDelegate " +
                      "before openening another search with the same delegate instance."
            );
            this.del = del;
            this.del._route = this;
        }

        public readonly SearchDelegate del;

        public override Color barrierColor {
            get { return null; }
        }

        public override TimeSpan transitionDuration {
            get { return new TimeSpan(0, 0, 0, 0, 300); }
        }

        public override bool maintainState {
            get { return false; }
        }

        public override Widget buildTransitions(
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child
        ) {
            return new FadeTransition(
                opacity: animation,
                child: child
            );
        }

        public override Animation<float> createAnimation() {
            Animation<float> animation = base.createAnimation();
            del._proxyAnimation.parent = animation;
            return animation;
        }

        public override Widget buildPage(
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation
        ) {
            return new _SearchPage(
                del: del,
                animation: animation
            );
        }

        protected internal override void didComplete(object result) {
            base.didComplete(result);
            D.assert(del._route == this);
            del._route = null;
            del._currentBody = null;
        }
    }

    class _SearchPage : StatefulWidget {
        public _SearchPage(
            SearchDelegate del,
            Animation<float> animation
        ) {
            this.del = del;
            this.animation = animation;
        }

        public readonly SearchDelegate del;

        public readonly Animation<float> animation;

        public override State createState() {
            return new _SearchPageState();
        }
    }

    class _SearchPageState : State<_SearchPage> {
        public override void initState() {
            base.initState();
            queryTextController.addListener(_onQueryChanged);
            widget.animation.addStatusListener(_onAnimationStatusChanged);
            widget.del._currentBodyNotifier.addListener(_onSearchBodyChanged);
            widget.del._focusNode.addListener(_onFocusChanged);
        }

        public override void dispose() {
            base.dispose();
            queryTextController.removeListener(_onQueryChanged);
            widget.animation.removeStatusListener(_onAnimationStatusChanged);
            widget.del._currentBodyNotifier.removeListener(_onSearchBodyChanged);
            widget.del._focusNode.removeListener(_onFocusChanged);
        }

        void _onAnimationStatusChanged(AnimationStatus status) {
            if (status != AnimationStatus.completed) {
                return;
            }

            widget.animation.removeStatusListener(_onAnimationStatusChanged);
            if (widget.del._currentBody == _SearchBody.suggestions) {
                FocusScope.of(context).requestFocus(widget.del._focusNode);
            }
        }

        void _onFocusChanged() {
            if (widget.del._focusNode.hasFocus && widget.del._currentBody != _SearchBody.suggestions) {
                widget.del.showSuggestions(context);
            }
        }

        void _onQueryChanged() {
            setState(() => { });
        }

        void _onSearchBodyChanged() {
            setState(() => { });
        }

        public override Widget build(BuildContext context) {
            MaterialD.debugCheckHasMaterialLocalizations(context);

            ThemeData theme = widget.del.appBarTheme(context);
            string searchFieldLabel = MaterialLocalizations.of(context).searchFieldLabel;
            Widget body = null;
            switch (widget.del._currentBody) {
                case _SearchBody.suggestions:
                    body = new KeyedSubtree(
                        key: new ValueKey<_SearchBody>(_SearchBody.suggestions),
                        child: widget.del.buildSuggestions(context)
                    );
                    break;
                case _SearchBody.results:
                    body = new KeyedSubtree(
                        key: new ValueKey<_SearchBody>(_SearchBody.results),
                        child: widget.del.buildResults(context)
                    );
                    break;
            }

            string routeName;
            switch (Theme.of(this.context).platform) {
                case RuntimePlatform.IPhonePlayer:
                    routeName = "";
                    break;
                case RuntimePlatform.Android:
                    routeName = searchFieldLabel;
                    break;
            }

            return new Scaffold(
                appBar: new AppBar(
                    backgroundColor: theme.primaryColor,
                    iconTheme: theme.primaryIconTheme,
                    textTheme: theme.primaryTextTheme,
                    brightness: theme.primaryColorBrightness,
                    leading: widget.del.buildLeading(context),
                    title: new TextField(
                        controller: queryTextController,
                        focusNode: widget.del._focusNode,
                        style: theme.textTheme.title,
                        textInputAction: TextInputAction.search,
                        onSubmitted: (string _) => { widget.del.showResults(context); },
                        decoration: new InputDecoration(
                            border: InputBorder.none,
                            hintText: searchFieldLabel
                        )
                    ),
                    actions: widget.del.buildActions(context)
                ),
                body: new AnimatedSwitcher(
                    duration: new TimeSpan(0, 0, 0, 0, 300),
                    child: body
                )
            );
        }

        TextEditingController queryTextController {
            get { return widget.del._queryTextController; }
        }
    }
}