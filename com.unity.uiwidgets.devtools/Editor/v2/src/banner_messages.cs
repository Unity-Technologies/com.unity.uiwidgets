using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    class BannerMessages : StatelessWidget {
        public BannerMessages(Key key = null, Screen screen = null) : base(key: key)
        {
            this.screen = screen;
        }

        public readonly Screen screen;
        
    
        public override Widget build(BuildContext context)
        {
            return new Container();
            // var controller = Provider<object>.of<BannerMessagesController>(context);
            // var messagesForScreen = controller?.messagesForScreen(screen.screenId);
            // List<Widget> temp = new List<Widget>();
            // if (messagesForScreen != null)
            // {
            //     temp.Add(new ValueListenableBuilder<List<BannerMessage>>(
            //             valueListenable: messagesForScreen,
            //             builder: (context2, messages, _) => {
            //                 return new Column(
            //                 children: messages
            //                 );
            //             }
            //     ));
            // }
            // temp.Add(new Expanded(
            //     child: screen.build(context)
            // ));
            //     
            //
            // return new Column(
            //     children: temp
            //     );
        }
    }
    
    public class BannerMessage : StatelessWidget {
        public BannerMessage(
            Key key = null,
            List<TextSpan> textSpans = null,
            Color backgroundColor = null,
            Color foregroundColor = null,
            string screenId = null,
            string headerText = null
        ) : base(key: key)
        {
            this.textSpans = textSpans;
            this.backgroundColor = backgroundColor;
            this.foregroundColor = foregroundColor;
            this.screenId = screenId;
            this.headerText = headerText;
        }

        public readonly List<TextSpan> textSpans;
        public readonly Color backgroundColor;
        public readonly Color foregroundColor;
        public readonly string screenId;
        public readonly string headerText;
    
        public override Widget build(BuildContext context)
        {
            List<InlineSpan> inlineSpans = new List<InlineSpan>();
            foreach (var textSpan in textSpans)
            {
                inlineSpans.Add(textSpan);
            }
            
            return new Card(
                color: backgroundColor,
                margin: EdgeInsets.only(bottom: ThemeUtils.denseRowSpacing),
                child: new Padding(
                    padding: EdgeInsets.all(ThemeUtils.defaultSpacing),
                    child: new Column(
                        mainAxisSize: MainAxisSize.min,
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: new List<Widget>{
                            new Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: new List<Widget>{
                                    new Text(
                                    headerText,
                                    style: Theme.of(context)
                                        .textTheme
                                        .headline6
                                        .copyWith(color: foregroundColor)
                                    ),
                                }
                            ),
                            new SizedBox(height: ThemeUtils.defaultSpacing),
                            new RichText(
                                    text: new TextSpan(
                                        children: inlineSpans
                                    )
                            ),
                        }
                    )
                )
            );
        }
    }
    

    class BannerMessagesController
    {
        Dictionary<string, ValueNotifier<List<BannerMessage>>> _messages = new Dictionary<string, ValueNotifier<List<BannerMessage>>>();
        ValueNotifier<List<BannerMessage>> _messagesForScreen(string screenId) {
            return _messages.putIfAbsent(
                screenId, () => new ValueNotifier<List<BannerMessage>>(null));
        }
    
        ValueListenable<List<BannerMessage>> messagesForScreen(string screenId) {
            return _messagesForScreen(screenId);
        }
    }


}