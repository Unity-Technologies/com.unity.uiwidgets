using System;
using System.Collections;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using ui_ = Unity.UIWidgets.widgets.ui_;
using Unity.UIWidgets.async;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine.Networking;
using Color = Unity.UIWidgets.ui.Color;
using Image = Unity.UIWidgets.widgets.Image;
using Timer = Unity.UIWidgets.async.Timer;

namespace UIWidgetsSample
{
    public class NumberCreator
    {
        public NumberCreator()
        {
            Timer.periodic(TimeSpan.FromSeconds(1), t =>
            {
                _controller.sink.add(_count);
                _count++;
                return default;
            });
        }

        private int _count = 1;
        private readonly StreamController<int> _controller =  StreamController<int>.create();

        public Stream<int> stream
        {
            get => _controller.stream;
        }
    }
    public class StreamTest : UIWidgetsPanel
    {
        protected override void main()
        {
            ui_.runApp(new MyApp());
        }

        class MyApp : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new WidgetsApp(
                    home: new ExampleApp(),
                    color: Color.white,
                    pageRouteBuilder: (settings, builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (Buildcontext, animation, secondaryAnimation) => builder(context)
                        )
                );
            }
        }

        class ExampleApp : StatefulWidget
        {
            public ExampleApp(Key key = null) : base(key)
            {
            }

            public override State createState()
            {
                return new ExampleState();
            }
        }

        class ExampleState : State<ExampleApp>
        {
            readonly Stream<int> myStream = new NumberCreator().stream;

            IEnumerator _loadCoroutine(string key, Completer completer, Isolate isolate) {
                var url = new Uri(key);
                using (var www = UnityWebRequest.Get(url)) {
                    yield return www.SendWebRequest();
                    using (Isolate.getScope(isolate)) {
                        if (www.isNetworkError || www.isHttpError) {
                            completer.completeError(new Exception($"Failed to load from url \"{url}\": {www.error}"));
                            yield break;
                        }

                        var data = www.downloadHandler.data;
                        completer.complete(data);
                    }
                }
            }
            public override Widget build(BuildContext context)
            {
                Future<byte[]> f = null;
                var completer = Completer.create();
                var isolate = Isolate.current;
                var panel = UIWidgetsPanelWrapper.current.window;
                if (panel.isActive()) {
                    panel.startCoroutine(_loadCoroutine("https://buljan.rcsdk8.org/sites/main/files/main-images/camera_lense_0.jpeg", completer, isolate));
                     f = completer.future.to<byte[]>().then_<byte[]>(data => {
                        if (data != null && data.Length > 0) {
                            return data;
                        }

                        throw new Exception("not loaded");
                    });
                }
                var futureBuilder = new FutureBuilder<byte[]>(
                    future: f,
                    builder: (ctx, snapshot) =>
                    {
                        int width = 200;
                        int height = 200;
                        Color color = Colors.blue;
                        if (snapshot.connectionState == ConnectionState.done)
                        {
                            return new Container(alignment: Alignment.center, width: width, height:height, color: color, child: Image.memory(snapshot.data) );
                        } else if (snapshot.connectionState == ConnectionState.waiting)
                        {
                            return new Container(alignment: Alignment.center, width: width, height:height, color: color, child: new Text("waiting") );
                        }
                        else
                        {
                            return new Container(alignment: Alignment.center, width: width, height:height, color: color, child: new Text("else") );
                        }
                    }
                );
                var streamBuilder = new StreamBuilder<int>(
                    stream: myStream,
                    initialData: 1,
                    builder: (ctx, snapshot) =>
                    {
                        var data = snapshot.data;
                        return new Container(child: new Text($"stream data: {data}"));
                    }
                );

                return new Container(
                    color: Colors.blueGrey,
                    child: new Column(
                        children: new List<Widget>
                        {
                            streamBuilder,
                            futureBuilder
                        }
                    )
                );
            }
        }
    }
}