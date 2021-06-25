using System;
using System.Collections.Generic;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.DevTools.config_specific.drag_and_drop
{

    public delegate void HandleDrop(Dictionary<string, object> data);
    public class DragAndDrop : StatefulWidget {
        public DragAndDrop(
            HandleDrop handleDrop= null,
            Widget child = null
        )
        {
            this.handleDrop = handleDrop;
            this.child = child;
        }


        public readonly HandleDrop handleDrop;

        public readonly Widget child;
        
        public override State createState() => new DragAndDropState();
    }
    
    public class DragAndDropState : State<DragAndDrop> {
        ValueNotifier<bool> _dragging = new ValueNotifier<bool>(false);

        NotificationsState notifications;

    
        public override void didChangeDependencies() {
            notifications = Notifications.of(context);
            base.didChangeDependencies();
        }

    
    public override Widget build(BuildContext context) {
        return new ValueListenableBuilder<bool>(
            valueListenable: _dragging,
            builder: (context2, dragging, _) => {
            // TODO(kenz): use AnimatedOpacity instead.
            return new Opacity(
                opacity: dragging ? 0.5f : 1.0f,
                child: widget.child
            );
        }
        );
    }

    void dragOver() {
        _dragging.value = true;
    }

    void dragLeave() {
        _dragging.value = false;
    }

    void drop() {
        _dragging.value = false;
    }
    }
}