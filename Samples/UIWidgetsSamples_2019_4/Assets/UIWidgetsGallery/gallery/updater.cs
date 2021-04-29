using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery
{
    public delegate Future<string> UpdateUrlFetcher();

    internal class Updater : StatefulWidget
    {
        public Updater(UpdateUrlFetcher updateUrlFetcher, Widget child = null, Key key = null)
            :
            base(key: key)
        {
            D.assert(updateUrlFetcher != null);
            this.child = child;
            this.updateUrlFetcher = updateUrlFetcher;
        }

        public readonly UpdateUrlFetcher updateUrlFetcher;
        public readonly Widget child;


        public override State createState()
        {
            return new UpdaterState();
        }
    }


    internal class UpdaterState : State<Updater>
    {
        public override void initState()
        {
            base.initState();
            this._checkForUpdates();
        }

        private static DateTime _lastUpdateCheck;

        private void _checkForUpdates()
        {
            // Only prompt once a day
            if (_lastUpdateCheck != null &&
                DateTime.Now - _lastUpdateCheck < new TimeSpan(1, 0, 0, 0))
                return; // We already checked for updates recently
            _lastUpdateCheck = DateTime.Now;
            this.widget.updateUrlFetcher.Invoke().then(updateUrl =>
            {
                if (updateUrl != null)
                    material_.showDialog<bool>(context: this.context, builder: this._buildDialog).then(wantsUpdate =>
                    {
                        if (wantsUpdate != null && (bool) wantsUpdate)
                            BrowserUtils.launch((string) updateUrl);
                    });
            });
        }

        private Widget _buildDialog(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            TextStyle dialogTextStyle =
                theme.textTheme.subtitle1.copyWith(color: theme.textTheme.caption.color);
            return new AlertDialog(
                title: new Text("Update UIWidgets Gallery?"),
                content: new Text("A newer version is available.", style: dialogTextStyle),
                actions: new List<Widget>
                {
                    new FlatButton(
                        child: new Text("NO THANKS"),
                        onPressed: () => { Navigator.pop(context, false); }
                    ),
                    new FlatButton(
                        child: new Text("UPDATE"),
                        onPressed: () => { Navigator.pop(context, true); }
                    )
                }
            );
        }

        public override Widget build(BuildContext context)
        {
            return this.widget.child;
        }
    }
}