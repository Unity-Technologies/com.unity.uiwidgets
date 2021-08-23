using System;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.widgets;

namespace UIWidgetsSample
{
    public class StreamBuilderSample : UIWidgetsPanel
    {
        protected override void main()
        {
            ui_.runApp(
                new MyStreamBuilderWidget()
            );
        }
    }

    class MyStreamBuilderWidget : StatelessWidget
    {
        private Stream<int> counter()
        {
            return Stream<int>.periodic(new TimeSpan(0, 0, 0, 1), i =>
            {
                return i * 3;
            }).take(5);
        }
        public override Widget build(BuildContext context)
        {
            return new WidgetsApp(
                title: "Text Fields",
                home: new StreamBuilder<int>(
                    stream: counter(),
                    builder: (BuildContext sub_context, AsyncSnapshot<int> snapshot) =>
                    {
                        if (snapshot.hasError)
                            return new Text($"Error: {snapshot.error}");
                        switch (snapshot.connectionState) {
                            case ConnectionState.none:
                                return new Text("没有Stream");
                            case ConnectionState.waiting:
                                return new Text("等待数据...");
                            case ConnectionState.active:
                                return new Text($"active: {snapshot.data}");
                            case ConnectionState.done:
                                return new Text("Stream已关闭");
                        }
                        return null; // unreachable
                    }
                ),
                pageRouteBuilder: (settings, builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (Buildcontext, animation, secondaryAnimation) => builder(context)
                    )
            );
        }
    }
}