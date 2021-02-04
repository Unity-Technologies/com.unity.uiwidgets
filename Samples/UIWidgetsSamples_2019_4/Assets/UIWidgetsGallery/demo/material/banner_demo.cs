using System.Collections.Generic;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    internal enum BannerDemoAction
    {
        reset,
        showMultipleActions,
        showLeading,
    }

    internal class BannerDemo : StatefulWidget
    {
        public BannerDemo(Key key = null) : base(key: key)
        {
        }

        public static readonly string routeName = "/material/banner";

        public override State createState()
        {
            return new _BannerDemoState();
        }
    }

    internal class _BannerDemoState : State<BannerDemo>
    {
        private const int _numItems = 20;
        private bool _displayBanner = true;
        private bool _showMultipleActions = true;
        private bool _showLeading = true;

        private void handleDemoAction(BannerDemoAction action)
        {
            this.setState(() =>
            {
                switch (action)
                {
                    case BannerDemoAction.reset:
                        this._displayBanner = true;
                        this._showMultipleActions = true;
                        this._showLeading = true;
                        break;
                    case BannerDemoAction.showMultipleActions:
                        this._showMultipleActions = !this._showMultipleActions;
                        break;
                    case BannerDemoAction.showLeading:
                        this._showLeading = !this._showLeading;
                        break;
                }
            });
        }

        public override Widget build(BuildContext context)
        {
            var children = new List<Widget>
            {
                new FlatButton(
                    child: new Text("SIGN IN"),
                    onPressed: () => { this.setState(() => { this._displayBanner = false; }); }
                )
            };

            if (this._showMultipleActions)
                children.Add(new FlatButton(
                    child: new Text("DISMISS"),
                    onPressed: () => { this.setState(() => { this._displayBanner = false; }); }
                ));


            Widget banner = new MaterialBanner(
                content: new Text("Your password was updated on your other device. Please sign in again."),
                leading: this._showLeading ? new CircleAvatar(child: new Icon(Icons.access_alarm)) : null,
                actions: children
            );

            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Banner"),
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(BannerDemo.routeName),
                        new PopupMenuButton<BannerDemoAction>(
                            onSelected: this.handleDemoAction,
                            itemBuilder: (BuildContext subContext) =>
                            {
                                var menuEntries = new List<PopupMenuEntry<BannerDemoAction>>();
                                menuEntries.Add(new PopupMenuItem<BannerDemoAction>(
                                    value: BannerDemoAction.reset,
                                    child: new Text("Reset the banner")
                                ));

                                menuEntries.Add(new PopupMenuDivider<BannerDemoAction>());

                                menuEntries.Add(new CheckedPopupMenuItem<BannerDemoAction>(
                                    value: BannerDemoAction.showMultipleActions,
                                    isChecked: this._showMultipleActions,
                                    child: new Text("Multiple actions")
                                ));

                                menuEntries.Add(new CheckedPopupMenuItem<BannerDemoAction>(
                                    value: BannerDemoAction.showLeading,
                                    isChecked: this._showLeading,
                                    child: new Text("Leading icon")
                                ));

                                return menuEntries;
                            })
                    }
                ),
                body: ListView.builder(itemCount: this._displayBanner ? _numItems + 1 : _numItems,
                    itemBuilder: (BuildContext subContext, int index) =>
                    {
                        if (index == 0 && this._displayBanner) return banner;

                        var itemIndex = this._displayBanner ? index : index + 1;
                        return new ListTile(title: new Text($"Item {itemIndex}"));
                    })
            );
        }
    }
}