using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    class Initializer : StatefulWidget
    {
        public Initializer(
            Key key = null,
            string url = null,
            WidgetBuilder builder = null,
            bool allowConnectionScreenOnDisconnect = true
        ) : base(key: key)
        {
            D.assert(builder != null);
            this.url = url;
            this.builder = builder;
            this.allowConnectionScreenOnDisconnect = allowConnectionScreenOnDisconnect;
        }

        public readonly WidgetBuilder builder;

        public readonly string url;

        public readonly bool allowConnectionScreenOnDisconnect;

        public override State createState()
        {
            return new _InitializerState();
        }
    }

    class _InitializerState : State<Initializer>
    {
        public override Widget build(BuildContext context)
        {
            // return _checkLoaded() && _dependenciesLoaded
            //     ? widget.builder(context)
            //     : new Scaffold(
            //     body: new Center(child: new CircularProgressIndicator())
            // );
            return new Scaffold(
                body: new Center(child: new CircularProgressIndicator())
            );
        }
    }
    
    
    
}