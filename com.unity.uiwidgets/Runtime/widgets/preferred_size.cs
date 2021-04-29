using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public interface SizePreferred {
        Size preferredSize { get; }
    }

    public abstract class PreferredSizeWidget : StatefulWidget, SizePreferred {
        protected PreferredSizeWidget(Key key = null) : base(key: key) {
        }

        public virtual Size preferredSize { get; }
    }

    /**
     *  In flutter this class inherits StatelessWidget explicitly, in particular, the class inheritance diagram looks like follows:
     *  PreferredSizeWidget   <--   Widget
     *           |                    |
     *          v                    v
     *  PreferredSize       <--    StatelessWidget
     * 
     *  It is hard to implement such a complex inheritance diagram in C#. so we choose to create a slightly different diagram, which is more simple and making every C# code happy:
     *
     *  PreferredSize      <--    PreferredSizeWidget    <--    StatefulWidget   <--    Widget
     *
     *  The bad side is that, we should make all classes that inherit PreferredSizeWidget to be statefulWidget now. Though it is not that bad since a statelessWidget can be made a statefulWidget easily
     * 
     */
    public class PreferredSize : PreferredSizeWidget {
        public PreferredSize(
            Key key = null,
            Widget child = null,
            Size preferredSize = null) : base(key: key) {
            D.assert(child != null);
            D.assert(preferredSize != null);
            this.child = child;
            this.preferredSize = preferredSize;
        }

        public readonly Widget child;
        
        public override Size preferredSize { get; }
        
        public override State createState() {
            return new _PreferredSizeState();
        }
    }

    class _PreferredSizeState : State<PreferredSize> {
        public override Widget build(BuildContext buildContext) {
            return widget.child;
        }
    }
}