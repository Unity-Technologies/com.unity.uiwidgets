using Unity.UIWidgets.widgets;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.DevTools {
    public abstract class SingleChildWidgetMixinStatelessWidget : StatelessWidget {
        
        public SingleChildWidgetMixinStatelessWidget(Key key = null) : base(key: key){}
        
        public abstract override Element createElement();
    }

}
