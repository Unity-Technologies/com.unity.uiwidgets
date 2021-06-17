using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery
{
    public static class BrowserUtils
    {
        internal static void launch(string url)
        {
            Process.Start(url);
        }

        internal static bool canLaunch(string url)
        {
            return true;
        }
    }

    internal class ComponentDemoTabData : IEquatable<ComponentDemoTabData>
    {
        public ComponentDemoTabData(
            Widget demoWidget = null,
            string exampleCodeTag = null,
            string description = null,
            string tabName = null,
            string documentationUrl = null
        )
        {
            this.demoWidget = demoWidget;
            this.exampleCodeTag = exampleCodeTag;
            this.description = description;
            this.tabName = tabName;
            this.documentationUrl = documentationUrl;
        }

        public readonly Widget demoWidget;
        public readonly string exampleCodeTag;
        public readonly string description;
        public readonly string tabName;
        public readonly string documentationUrl;

        public bool Equals(ComponentDemoTabData other)
        {
            if (ReferenceEquals(null, other)) return false;

            if (ReferenceEquals(this, other)) return true;

            return this.tabName.Equals(other.tabName)
                   && this.description.Equals(other.description)
                   && this.documentationUrl.Equals(other.documentationUrl);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;

            if (ReferenceEquals(this, obj)) return true;

            if (obj.GetType() != this.GetType()) return false;

            return this.Equals((ComponentDemoTabData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (this.tabName.GetHashCode() * 397);
                hashCode = (hashCode * 397) ^ this.description.GetHashCode();
                hashCode = (hashCode * 397) ^ this.documentationUrl.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ComponentDemoTabData left, ComponentDemoTabData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ComponentDemoTabData left, ComponentDemoTabData right)
        {
            return !Equals(left, right);
        }
    }


    internal class TabbedComponentDemoScaffold : StatelessWidget
    {
        public TabbedComponentDemoScaffold(
            List<ComponentDemoTabData> demos,
            string title,
            List<Widget> actions = null,
            bool isScrollable = true,
            bool showExampleCodeAction = true
        )
        {
            this.title = title;
            this.demos = demos;
            this.actions = actions;
            this.isScrollable = isScrollable;
            this.showExampleCodeAction = showExampleCodeAction;
        }

        public readonly List<ComponentDemoTabData> demos;
        public readonly string title;
        public readonly List<Widget> actions;
        public readonly bool isScrollable;
        public readonly bool showExampleCodeAction;

        private void _showExampleCode(BuildContext context)
        {
            string tag = this.demos[DefaultTabController.of(context).index].exampleCodeTag;
            if (tag != null) D.assert(false, () => "TO DO >>>");
            /*Navigator.push(context, MaterialPageRoute<FullScreenCodeDialog>(
                builder: (BuildContext context) => FullScreenCodeDialog(exampleCodeTag: tag)
              ));*/
        }

        private Future _showApiDocumentation(BuildContext context)
        {
            string url = this.demos[DefaultTabController.of(context).index].documentationUrl;
            if (url == null)
                return null;

            if (BrowserUtils.canLaunch(url))
                BrowserUtils.launch(url);
            else
                material_.showDialog<object>(
                    context: context,
                    builder: (BuildContext subContext) =>
                    {
                        return new SimpleDialog(
                            title: new Text("Couldn't display URL:"),
                            children: new List<Widget>
                            {
                                new Padding(
                                    padding: EdgeInsets.symmetric(horizontal: 16.0f),
                                    child: new Text(url)
                                )
                            }
                        );
                    }
                ); 

            return null;
        }

        public override Widget build(BuildContext context)
        {
            var children = new List<Widget>();
            if (actions != null)
            {
                children.AddRange(actions);
            }
            children.Add(new Builder(
                    builder: (BuildContext subContext) =>
                    {
                        return new IconButton(
                            icon: new Icon(Icons.library_books),
                            onPressed: () => this._showApiDocumentation(context)
                        );
                    }
                )
            );

            if (this.showExampleCodeAction)
                children.Add(new Builder(
                    builder: (BuildContext subContext) =>
                    {
                        return new IconButton(
                            icon: new Icon(Icons.code),
                            tooltip: "Show example code",
                            onPressed: () => this._showExampleCode(context)
                        );
                    }
                ));

            return new DefaultTabController(
                length: this.demos.Count,
                child: new Scaffold(
                    appBar: new AppBar(
                        title: new Text(this.title),
                        actions: children,
                        bottom: new TabBar(
                            isScrollable: this.isScrollable,
                            tabs: this.demos.Select<ComponentDemoTabData, Widget>((ComponentDemoTabData data) =>
                                new Tab(text: data.tabName)).ToList()
                        )
                    ),
                    body: new TabBarView(
                        children: this.demos.Select<ComponentDemoTabData, Widget>((ComponentDemoTabData demo) =>
                        {
                            return new SafeArea(
                                top: false,
                                bottom: false,
                                child: new Column(
                                    children: new List<Widget>
                                    {
                                        new Padding(
                                            padding: EdgeInsets.all(16.0f),
                                            child: new Text(demo.description,
                                                style: Theme.of(context).textTheme.subtitle1
                                            )
                                        ),
                                        new Expanded(child: demo.demoWidget)
                                    }
                                )
                            );
                        }).ToList()
                    )
                )
            );
        }
    }

    internal class MaterialDemoDocumentationButton : StatelessWidget
    {
        internal MaterialDemoDocumentationButton(string routeName, Key key = null) : base(key: key)
        {
            D.assert(
                GalleryDemo.kDemoDocumentationUrl[routeName] != null,
                () => $"A documentation URL was not specified for demo route {routeName} in kAllGalleryDemos"
            );
            this.documentationUrl = GalleryDemo.kDemoDocumentationUrl[routeName];
        }

        public readonly string documentationUrl;

        public override Widget build(BuildContext context)
        {
            return new IconButton(
                icon: new Icon(Icons.library_books),
                tooltip: "API documentation",
                onPressed: () => BrowserUtils.launch(this.documentationUrl)
            );
        }
    }
}