using Unity.UIWidgets.engine;
using Unity.UIWidgets.widgets;
using ui_ = Unity.UIWidgets.widgets.ui_;
using Unity.UIWidgets.cupertino;

namespace UIWidgetsSample
{
    public class TextFieldTest : UIWidgetsPanel
    {
        protected override void main()
        {
            ui_.runApp(new MyApp());
        }

        class MyApp : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new CupertinoApp(
                    home: new HomeScreen() 
                );
            }
        }

        class HomeScreen : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new HomeScreen3();
            }
        }

        public class HomeScreen3 : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new CupertinoPageScaffold(
                    child: new Center(
                        child:
                        new Container(
                            width: 200,
                            child: new MyPrefilledText()
                        )
                    )
                );
            }
        }


        public class MyPrefilledText : StatefulWidget
        {
            public override State createState() => new _MyPrefilledTextState();
        }

        class _MyPrefilledTextState : State<MyPrefilledText>
        {
            TextEditingController _textController;

            public override void initState()
            {
                base.initState();
                _textController = new TextEditingController(text: "initial text");
            }

            public override Widget build(BuildContext context)
            {
                return new CupertinoTextField(controller: _textController);
            }
        }
    }
}