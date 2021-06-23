using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    class ValueListenableProvider<T> : DeferredInheritedProvider<ValueListenable<T>, T> {

      public ValueListenableProvider(
        Key key = null,
        Create<ValueListenable<T>> create = null,
        UpdateShouldNotify<T> updateShouldNotify = null,
        bool? lazy = null,
        TransitionBuilder builder = null,
        Widget child = null
      ) : base(
        key: key,
        create: create,
        lazy: lazy,
        builder: builder,
        updateShouldNotify: updateShouldNotify,
        startListening: _startListening<T>(),
        dispose: _dispose,
        child: child
      )
      {
        
      }

      private static void _dispose(BuildContext context, ValueListenable<T> notifier)
      {
        if (notifier is ValueNotifier<T>) {
          ((ValueNotifier<T>)notifier).dispose();
        }
      }

      public ValueListenableProvider(
        Key key = null,
        ValueListenable<T> value = null,
        UpdateShouldNotify<T> updateShouldNotify = null,
        TransitionBuilder builder = null,
        Widget child = null
      ) : base(
          key: key,
          builder: builder,
          value: value,
          updateShouldNotify: updateShouldNotify,
          startListening: _startListening<T>(),
          child: child
        )
      {

      }

      public static DeferredStartListening<ValueListenable<T>, T> _startListening<T>() {
        return (_, setState, controller, __) => {
          setState(controller.value);
          
          controller.addListener(() => setState(controller.value));
          return () => controller.removeListener(() => setState(controller.value));
        };
      }
    }

}