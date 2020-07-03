using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.gallery {
    public class SearchDemo : StatefulWidget {
        public const string routeName = "/material/search";

        public override State createState() {
            return new _SearchDemoState();
        }
    }

    class _SearchDemoState : State<SearchDemo> {
        readonly _SearchDemoSearchDelegate _delegate = new _SearchDemoSearchDelegate();
        readonly GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();
        int? _lastIntegerSelected;

        public override Widget build(BuildContext context) {
            return new Scaffold(
                key: this._scaffoldKey,
                appBar: new AppBar(
                    leading: new IconButton(
                        tooltip: "Navigation menu",
                        icon: new AnimatedIcon(
                            icon: AnimatedIcons.arrow_menu,
                            color: Colors.white,
                            progress: this._delegate.transitionAnimation
                        ),
                        onPressed: () => { this._scaffoldKey.currentState.openDrawer(); }
                    ),
                    title: new Text("Numbers"),
                    actions: new List<Widget> {
                        new IconButton(
                            tooltip: "Search",
                            icon: new Icon(Icons.search),
                            onPressed: () => {
                                SearchUtils.showSearch(
                                    context: context,
                                    del: this._delegate
                                ).Done((selected) => {
                                    if (selected != null && (int) selected != this._lastIntegerSelected) {
                                        this.setState(() => { this._lastIntegerSelected = (int) selected; });
                                    }
                                });
                            }
                        ),
                        new MaterialDemoDocumentationButton(SearchDemo.routeName),
                        new IconButton(
                            tooltip: "More (not implemented)",
                            icon: new Icon(
                                Theme.of(context).platform == RuntimePlatform.IPhonePlayer
                                    ? Icons.more_horiz
                                    : Icons.more_vert
                            ),
                            onPressed: () => { }
                        )
                    }
                ),
                body: new Center(
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: new List<Widget> {
                            new Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: new List<Widget> {
                                    new Row(
                                        mainAxisAlignment: MainAxisAlignment.center,
                                        children: new List<Widget> {
                                            new Text("Press the "),
                                            new Tooltip(
                                                message: "search",
                                                child: new Icon(
                                                    Icons.search,
                                                    size: 18.0f
                                                )
                                            ),
                                            new Text(" icon in the AppBar")
                                        }
                                    ),
                                    new Text("and search for an integer between 0 and 100,000.")
                                }
                            ),
                            new SizedBox(height: 64.0f),
                            new Text(
                                this._lastIntegerSelected == null
                                    ? $"Last selected integer: {this._lastIntegerSelected}."
                                    : $"Last selected integer: NONE."
                            ),
                        }
                    )
                ),
                floatingActionButton: FloatingActionButton.extended(
                    tooltip: "Back", // Tests depend on this label to exit the demo.
                    onPressed: () => { Navigator.of(context).pop(); },
                    label:
                    new Text("Close demo"),
                    icon:
                    new Icon(Icons.close)
                ),
                drawer: new Drawer(
                    child: new Column(
                        children: new List<Widget> {
                            new UserAccountsDrawerHeader(
                                accountName: new Text("Peter Widget"),
                                accountEmail: new Text("peter.widget@example.com"),
                                currentAccountPicture: new CircleAvatar(
                                    backgroundImage: new AssetImage(
                                        "people/square/peter"
                                    )
                                ),
                                margin: EdgeInsets.zero
                            ),
                            MediaQuery.removePadding(
                                context: context,
                                // DrawerHeader consumes top MediaQuery padding.
                                removeTop: true,
                                child: new ListTile(
                                    leading: new Icon(Icons.payment),
                                    title: new Text("Placeholder")
                                )
                            )
                        }
                    )
                )
            );
        }
    }

    class _SearchDemoSearchDelegate : SearchDelegate {
        static List<int> listGenerate(int count, Func<int, int> func) {
            var list = new List<int>();
            for (int i = 0; i < count; i++) {
                list.Add(func(i));
            }

            list.Reverse();
            return list;
        }

        readonly List<int> _data = listGenerate(100001, (int i) => i);

        readonly List<int> _history = new List<int> {42607, 85604, 66374, 44, 174};

        public override Widget buildLeading(BuildContext context) {
            return new IconButton(
                tooltip: "Back",
                icon: new AnimatedIcon(
                    icon: AnimatedIcons.arrow_menu,
                    progress: this.transitionAnimation
                ),
                onPressed: () => { this.close(context, null); }
            );
        }

        public override Widget buildSuggestions(BuildContext context) {
            List<int> suggestions;
            if (this.query.isEmpty()) {
                suggestions = this._history;
            }
            else {
                suggestions = new List<int>();
                foreach (var item in this._data) {
                    var str = item.ToString();
                    var flag = true;
                    if (str.Length < this.query.Length) {
                        continue;
                    }

                    for (int i = 0; i < this.query.Length; i++) {
                        if (str[i] != this.query[i]) {
                            flag = false;
                            break;
                        }
                    }

                    if (flag) {
                        suggestions.Add(item);
                    }
                }
            }

            List<string> suggestionStrings = new List<string>();
            foreach (var item in suggestions) {
                suggestionStrings.Add(item.ToString());
            }

            return new _SuggestionList(
                query: this.query,
                suggestions: suggestionStrings,
                onSelected: (string suggestion) => {
                    this.query = suggestion;
                    this.showResults(context);
                }
            );
        }

        public override Widget buildResults(BuildContext context) {
            int searched;
            try {
                searched = int.Parse(this.query);
            }
            catch (Exception) {
                return new Center(
                    child: new Text(
                        $"{this.query}\n is not a valid integer between 0 and 100,000.\nTry again.",
                        textAlign: TextAlign.center
                    )
                );
            }

            if (!this._data.Contains(searched)) {
                return new Center(
                    child: new Text(
                        $"{this.query}\n is not a valid integer between 0 and 100,000.\nTry again.",
                        textAlign: TextAlign.center
                    )
                );
            }

            return new ListView(
                children: new List<Widget> {
                    new _ResultCard(
                        title: "This integer",
                        integer: searched,
                        searchDelegate: this
                    ),
                    new _ResultCard(
                        title: "Next integer",
                        integer: searched + 1,
                        searchDelegate: this
                    ),
                    new _ResultCard(
                        title: "Previous integer",
                        integer: searched - 1,
                        searchDelegate: this
                    )
                }
            );
        }

        public override List<Widget> buildActions(BuildContext context) {
            return new List<Widget> {
                this.query.isEmpty()
                    ? new IconButton(
                        tooltip: "Voice Search",
                        icon: new Icon(Icons.mic),
                        onPressed: () => { this.query = "TODO: implement voice input"; }
                    )
                    : new IconButton(
                        tooltip: "Clear",
                        icon: new Icon(Icons.clear),
                        onPressed:
                        () => {
                            this.query = "";
                            this.showSuggestions(context);
                        }
                    )
            };
        }
    }

    class _ResultCard : StatelessWidget {
        public _ResultCard(
            int integer, string title, SearchDelegate searchDelegate
        ) {
            this.integer = integer;
            this.title = title;
            this.searchDelegate = searchDelegate;
        }

        public readonly int integer;
        public readonly string title;
        public readonly SearchDelegate searchDelegate;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            return new GestureDetector(
                onTap: () => { this.searchDelegate.close(context, this.integer); },
                child:
                new Card(
                    child: new Padding(
                        padding: EdgeInsets.all(8.0f),
                        child: new Column(
                            children: new List<Widget> {
                                new Text(this.title),
                                new Text(this.integer.ToString(),
                                    style: theme.textTheme.headline.copyWith(fontSize: 72.0f)
                                ),
                            }
                        )
                    )
                )
            );
        }
    }

    class _SuggestionList : StatelessWidget {
        public _SuggestionList(
            List<string> suggestions, string query, ValueChanged<string> onSelected
        ) {
            this.suggestions = suggestions;
            this.query = query;
            this.onSelected = onSelected;
        }

        public readonly List<string> suggestions;
        public readonly string query;
        public readonly ValueChanged<string> onSelected;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            return ListView.builder(
                itemCount: this.suggestions.Count,
                itemBuilder: (BuildContext _context, int i) => {
                    string suggestion = this.suggestions[i];
                    return new ListTile(
                        leading: this.query.isEmpty() ? new Icon(Icons.history) : new Icon(null),
                        title: new RichText(
                            text: new TextSpan(
                                text: suggestion.Substring(0, this.query.Length),
                                style: theme.textTheme.subhead.copyWith(fontWeight: FontWeight.bold),
                                children: new List<TextSpan> {
                                    new TextSpan(
                                        text: suggestion.Substring(this.query.Length),
                                        style: theme.textTheme.subhead
                                    )
                                }
                            )
                        ),
                        onTap: () => { this.onSelected(suggestion); }
                    );
                }
            );
        }
    }
}