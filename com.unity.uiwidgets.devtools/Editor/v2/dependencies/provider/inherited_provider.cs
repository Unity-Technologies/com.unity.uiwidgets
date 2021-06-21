using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    delegate bool UpdateShouldNotify<T>(T previous, T current);

    delegate T Create<T>(BuildContext context);

    delegate void Dispose<T>(BuildContext context, T value);

    // delegate VoidCallback StartListening<T>(InheritedContext<T> element, T value);
    
    public class InheritedProvider<T> : SingleChildStatelessWidget
    {
        protected internal override Widget buildWithChild(BuildContext context, Widget child)
        {
            throw new System.NotImplementedException();
        }
    }
}