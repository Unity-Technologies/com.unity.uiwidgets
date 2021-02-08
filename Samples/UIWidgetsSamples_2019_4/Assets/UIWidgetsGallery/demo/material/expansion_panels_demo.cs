using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using UIWidgets.Runtime.material;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsGallery.demo.material
{
    enum Location {
        Barbados,
        Bahamas,
        Bermuda
    }

    delegate Widget DemoItemBodyBuilder(DemoItem item);
    delegate string ValueToString<T>(T value);
    
    class DualHeaderWithHint : StatelessWidget {
        public DualHeaderWithHint(
            string name = null,
            string value = null,
            string hint = null,
            bool showHint = false
        )
        {
            this.name = name;
            this.value = value;
            this.hint = hint;
            this.showHint = showHint;
        }

    public readonly string name;
    public readonly string value;
    public readonly string hint;
    public readonly bool showHint;

    Widget _crossFade(Widget first, Widget second, bool isExpanded) {
    return new AnimatedCrossFade(
        firstChild: first,
    secondChild: second,
    firstCurve: new Interval(0.0f, 0.6f, curve: Curves.fastOutSlowIn),
    secondCurve: new Interval(0.4f, 1.0f, curve: Curves.fastOutSlowIn),
    sizeCurve: Curves.fastOutSlowIn,
    crossFadeState: isExpanded ? CrossFadeState.showSecond : CrossFadeState.showFirst,
    duration: new TimeSpan(0,0,0,0, 200)
    );
    }

    public override Widget build(BuildContext context) {
     ThemeData theme = Theme.of(context);
     TextTheme textTheme = theme.textTheme;

    return new Row(
        children: new List<Widget>{
        new Expanded(
            flex: 2,
            child: new Container(
                margin: EdgeInsets.only(left: 24.0f),
    child: new FittedBox(
        fit: BoxFit.scaleDown,
    alignment: Alignment.centerLeft,
    child: new Text(
        name,
        style: textTheme.bodyText2.copyWith(fontSize: 15.0f)
    )
    )
    )
    ),
        new Expanded(
        flex: 3,
        child: new Container(
        margin: EdgeInsets.only(left: 24.0f),
    child: _crossFade(
        new Text(value, style: textTheme.caption.copyWith(fontSize: 15.0f)),
        new Text(hint, style: textTheme.caption.copyWith(fontSize: 15.0f)),
    showHint
    )
    )
    )
    }
    );
    }
}
    
    class CollapsibleBody : StatelessWidget {
        public CollapsibleBody(
            EdgeInsets margin = null,
            Widget child = null,
            VoidCallback onSave = null,
            VoidCallback onCancel = null
        )
        {
            margin = margin ?? EdgeInsets.zero;
            this.margin = margin;
            this.child = child;
            this.onSave = onSave;
            this.onCancel = onCancel;
        }

    public readonly EdgeInsets margin;
    public readonly Widget child;
    public readonly VoidCallback onSave;
    public readonly VoidCallback onCancel;


    public override Widget build(BuildContext context) {
     ThemeData theme = Theme.of(context);
     TextTheme textTheme = theme.textTheme;

    return new Column(
        children: new List<Widget>{
        new Container(
            margin: EdgeInsets.only(
    left: 24.0f,
    right: 24.0f,
    bottom: 24.0f
    ) - margin,
    child: new Center(
        child: new DefaultTextStyle(
        style: textTheme.caption.copyWith(fontSize: 15.0f),
    child: child
    )
    )
    ),
        new  Divider(height: 1.0f),
        new Container(
        padding: EdgeInsets.symmetric(vertical: 16.0f),
    child: new Row(
        mainAxisAlignment: MainAxisAlignment.end,
    children: new List<Widget>{
        new Container(
            margin: EdgeInsets.only(right: 8.0f),
    child: new FlatButton(
        onPressed: onCancel,
    child: new  Text("CANCEL", style: new TextStyle(
        color: Colors.black54,
    fontSize: 15.0f,
    fontWeight: FontWeight.w500
    ))
    )
    ),
    new Container(
        margin: EdgeInsets.only(right: 8.0f),
    child: new FlatButton(
        onPressed: onSave,
    textTheme: ButtonTextTheme.accent,
    child: new  Text("SAVE")
    )
    )
    }
    )
    )
    }
    );
    }
}

    abstract class DemoItem
    {
       public virtual bool isExpanded { get; set; }
       
       public virtual ExpansionPanelHeaderBuilder headerBuilder { get; }

       internal abstract Widget build();
    }
    
    class DemoItem<T> : DemoItem {
        public DemoItem(
            string name = null,
            T value = default,
            string hint = null,
            DemoItemBodyBuilder builder = null,
            ValueToString<T> valueToString  = null
        ) {
            textController = new TextEditingController(text: valueToString(value));
            this.name = name;
            this.value = value;
            this.hint = hint;
            this.builder = builder;
            this.valueToString = valueToString;
        }

        public readonly string name;
        public readonly string hint;
        public readonly TextEditingController textController;
        public readonly DemoItemBodyBuilder builder;
        public readonly ValueToString<T> valueToString;

        internal T value;
        bool _isExpanded = false;

        public override bool isExpanded
        {
          get { return this._isExpanded; }
          set { this._isExpanded = value; }
        }

        public override ExpansionPanelHeaderBuilder headerBuilder {
            get
            {
                return (BuildContext context, bool isExpanded) => {
                    return new DualHeaderWithHint(
                        name: name,
                        value: valueToString(value),
                        hint: hint,
                        showHint: isExpanded
                    );
                };
            }
        }

        internal override Widget build() => builder(this);
    }


    class ExpansionPanelsDemo : StatefulWidget {
    public static readonly string routeName = "/material/expansion_panels";

    public override State  createState() => new _ExpansionPanelsDemoState();
    }
    
    class _ExpansionPanelsDemoState : State<ExpansionPanelsDemo> {
  List<DemoItem> _demoItems;

  public override void initState() {
    base.initState();

    _demoItems = new List<DemoItem>
    {
      new DemoItem<string>(
        name: "Trip",
        value: "Caribbean cruise",
        hint: "Change trip name",
        valueToString: (string value) => value,
        builder: (DemoItem _item) =>
        {
          var item = (DemoItem<string>) _item;
          void close()
          {
            setState(() => { item.isExpanded = false; });
          }

          return new Form(
            child: new Builder(
              builder: (BuildContext subContext) =>
              {
                return new CollapsibleBody(
                  margin: EdgeInsets.symmetric(horizontal: 16.0f),
                  onSave: () =>
                  {
                    Form.of(subContext).save();
                    close();
                  },
                  onCancel: () =>
                  {
                    Form.of(subContext).reset();
                    close();
                  },
                  child: new Padding(
                    padding: EdgeInsets.symmetric(horizontal: 16.0f),
                    child: new TextFormField(
                      controller: item.textController,
                      decoration: new InputDecoration(
                        hintText: item.hint,
                        labelText: item.name
                      ),
                      onSaved: (string value) => { item.value = value; }
                    )
                  )
                );
              }
            )
          );
        }
      ),
      new DemoItem<Location>(
        name: "Location",
        value: Location.Bahamas,
        hint: "Select location",
        valueToString: (Location location) => location.ToString(),
        builder: (DemoItem _item) =>
        {
          var item = (DemoItem<Location>) _item;
          void close()
          {
            setState(() => { item.isExpanded = false; });
          }

          return new Form(
            child: new Builder(
              builder: (BuildContext subContext) =>
              {
                return new CollapsibleBody(
                  onSave: () =>
                  {
                    Form.of(subContext).save();
                    close();
                  },
                  onCancel: () =>
                  {
                    Form.of(subContext).reset();
                    close();
                  },
                  child: new FormField<Location>(
                    initialValue: item.value,
                    onSaved: (Location result) => { item.value = result; },
                    builder: (FormFieldState<Location> field) =>
                    {
                      return new Column(
                        mainAxisSize: MainAxisSize.min,
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: new List<Widget>
                        {
                          new RadioListTile<Location>(
                            value: Location.Bahamas,
                            title: new Text("Bahamas"),
                            groupValue: field.value,
                            onChanged: field.didChange
                          ),
                          new RadioListTile<Location>(
                            value: Location.Barbados,
                            title: new Text("Barbados"),
                            groupValue: field.value,
                            onChanged: field.didChange
                          ),
                          new RadioListTile<Location>(
                            value: Location.Bermuda,
                            title: new Text("Bermuda"),
                            groupValue: field.value,
                            onChanged: field.didChange
                          )
                        }
                      );
                    }
                  )
                );
              }
            )
          );
        }
      ),
      new DemoItem<float>(
        name: "Sun",
        value: 80.0f,
        hint: "Select sun level",
        valueToString: (float amount) => $"{amount.round()}",
        builder: (DemoItem _item) =>
        {
          var item = (DemoItem<float>) _item;
          void close()
          {
            setState(() => { item.isExpanded = false; });
          }

          return new Form(
            child: new Builder(
              builder: (BuildContext subContext) =>
              {
                return new CollapsibleBody(
                  onSave: () =>
                  {
                    Form.of(subContext).save();
                    close();
                  },
                  onCancel: () =>
                  {
                    Form.of(subContext).reset();
                    close();
                  },
                  child: new FormField<float>(
                    initialValue: item.value,
                    onSaved: (float value) => { item.value = value; },
                    builder: (FormFieldState<float> field) =>
                    {
                      return new Container(
                        // Allow room for the value indicator.
                        padding: EdgeInsets.only(top: 44.0f),
                        child: new Slider(
                          min: 0.0f,
                          max: 100.0f,
                          divisions: 5,
                          activeColor: Colors.orange[(uint) (100 + (field.value * 5.0f).round())],
                          label: $"{field.value.round()}",
                          value: field.value,
                          onChanged: field.didChange
                        )
                      );
                    }
                  )
                );
              }
            )
          );
        }
      )
    };
  }


  public override Widget build(BuildContext context) {
    return new Scaffold(
      appBar: new AppBar(
        title: new Text("Expansion panels"),
        actions: new List<Widget>{
          new MaterialDemoDocumentationButton(ExpansionPanelsDemo.routeName)
        }
      ),
      body: new SingleChildScrollView(
        child: new SafeArea(
          top: false,
          bottom: false,
          child: new Container(
            margin: EdgeInsets.all(24.0f),
            child: new ExpansionPanelList(
              expansionCallback: (int index, bool isExpanded) => {
                setState(() => {
                  _demoItems[index].isExpanded = !isExpanded;
                });
              },
              children: _demoItems.Select<DemoItem, ExpansionPanel>((DemoItem item) => {
                return new ExpansionPanel(
                  isExpanded: item.isExpanded,
                  headerBuilder: item.headerBuilder,
                  body: item.build()
                );
              }).ToList()
            )
          )
        )
      )
    );
  }
}
}