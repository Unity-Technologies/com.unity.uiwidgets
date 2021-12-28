using System;
using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using UnityEngine.Networking;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample
{
    public class HttpRequestSample : UIWidgetsPanel
    {
        protected override void main()
        {
            ui_.runApp(new MaterialApp(
                title: "Http Request Sample",
                home: new Scaffold(
                    body: new AsyncRequestWidget(this.gameObject)
                )
            ));
        }
    }

    public class AsyncRequestWidget : StatefulWidget
    {

        public readonly GameObject gameObjOfUIWidgetsPanel;

        public AsyncRequestWidget(GameObject gameObjOfUiWidgetsPanel, Key key = null) : base(key)
        {
            this.gameObjOfUIWidgetsPanel = gameObjOfUiWidgetsPanel;
        }

        public override State createState()
        {
            return new _AsyncRequestWidgetState();
        }
    }

    [Serializable]
    public class TimeData
    {
        public long currentFileTime;
    }

    class _AsyncRequestWidgetState : State<AsyncRequestWidget>
    {

        long _fileTime;

        public override Widget build(BuildContext context)
        {
            var isolate = Isolate.current;

            return new Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: new List<Widget>()
                {
                    new FlatButton(child: new Text("Click To Get Time"), onPressed: () =>
                    {
                        UnityWebRequest www = UnityWebRequest.Get("http://worldclockapi.com/api/json/est/now");
                        var asyncOperation = www.SendWebRequest();
                        asyncOperation.completed += operation =>
                        {
                            var timeData = JsonUtility.FromJson<TimeData>(www.downloadHandler.text);
                            using (Isolate.getScope(isolate))
                            {
                                this.setState(() => { this._fileTime = timeData.currentFileTime; });
                            }

                        };
                    }),
                    new Text($"current file time: {this._fileTime}")
                });
        }
    }
}