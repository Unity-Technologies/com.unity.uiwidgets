using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample
{
    public class TextFieldSample : UIWidgetsSamplePanel
    {
        protected override void main()
        {
            ui_.runApp(
                new MaterialApp(
                    title: "Text Fields",
                    home: new MyCustomForm()
                )
            );
        }

        protected new void OnEnable()
        {
            base.OnEnable();
            // TODO: add font
            // FontManager.instance.addFont(Resources.Load<Font>(path: "fonts/MaterialIcons-Regular"), "Material Icons");
        }
    }

    class MyCustomForm : StatefulWidget
    {
        public override State createState()
        {
            return new _MyCustomFormState();
        }
    }

    class _MyCustomFormState : State<MyCustomForm>
    {
        readonly TextEditingController myController = new TextEditingController();

        public override void dispose()
        {
            this.myController.dispose();
            base.dispose();
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Retrieve Text Input")
                ),
                body: new Padding(
                    padding: EdgeInsets.all(16.0f)
                    ,
                    child: new TextField(
                        controller: this.myController,
                        autofocus: true,
                        decoration: new InputDecoration(
                            hintText: "hinthere",
                            labelText: "pwd",
                            prefixIcon: new Icon(Unity.UIWidgets.material.Icons.search)
                        )
                    )
                ),
                floatingActionButton: new FloatingActionButton(
                    // When the user presses the button, show an alert dialog with the
                    // text the user has typed into our text field.
                    onPressed: () =>
                    {
                        material_.showDialog<object>(
                            context: context,
                            builder: (_context) =>
                            {
                                return new AlertDialog(
                                    // Retrieve the text the user has typed in using our
                                    // TextEditingController
                                    content: new Text(this.myController.text)
                                );
                            });
                    },
                    tooltip: "Show me the value",
                    child: new Icon(Icons.search)
                )
            );
        }
    }
}