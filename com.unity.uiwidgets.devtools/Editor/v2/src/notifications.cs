using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    public class Notifications : StatelessWidget 
    {
        public Notifications(Widget child)
        {
            this.child = child;
        }
        
        public readonly Widget child;
        public override Widget build(BuildContext context)
        {
            return new Overlay(initialEntries: new List<OverlayEntry>
            {
                new OverlayEntry(
                    builder: (context2) => new _NotificationsProvider(child: child),
                    maintainState: true,
                    opaque: true
                )
            }
            );
        }
    }
    
    
    class _NotificationsProvider : StatefulWidget {
        public _NotificationsProvider(Key key = null, Widget child = null) : base(key: key)
        {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState()
        {
            return new NotificationsState();
        }
    }
    
    class _InheritedNotifications : InheritedWidget {
        public _InheritedNotifications(NotificationsState data = null, Widget child = null)
        : base(child: child)
        {
            this.data = data;
        }

        public readonly NotificationsState data;
        

        public override bool updateShouldNotify(InheritedWidget oldWidget)
        {
            return ((_InheritedNotifications)oldWidget).data != data;
        }
    }

    class NotificationsState : State<_NotificationsProvider>
    {
        public override Widget build(BuildContext context)
        {
            return new _InheritedNotifications(data: this, child: widget.child);
        }
    }
    
}

