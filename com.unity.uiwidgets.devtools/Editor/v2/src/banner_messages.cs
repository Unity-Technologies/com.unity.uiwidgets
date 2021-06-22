using System.Collections.Generic;
using Unity.UIWidgets.foundation;
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

    // class BannerMessagesController
    // {
    //     ValueNotifier<List<BannerMessage>> _messagesForScreen(string screenId) {
    //         return _messages.putIfAbsent(
    //             screenId, () => new ValueNotifier<List<BannerMessage>>());
    //     }
    //
    //     ValueListenable<List<BannerMessage>> messagesForScreen(string screenId) {
    //         return _messagesForScreen(screenId);
    //     }
    // }


}