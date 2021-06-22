
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Unity.UIWidgets.DevTools
{
    public class Provider<T> : InheritedProvider<T>
    {
        public Provider(
            Key key = null,
            Create<T> create = null,
            Dispose<T> dispose = null,
            bool? lazy = null,
            TransitionBuilder builder = null,
            Widget child = null
        ) :
            base(
                key: key,
                lazy: lazy,
                builder: builder,
                create: create,
                dispose: dispose,
                debugCheckInvalidValueType: null,
                child: child
            )
        {
            D.assert(create != null);
        }
        
        
        public static T of<T>(BuildContext context, bool listen = true) {
            D.assert(context != null);
//             D.assert(
//                 context.owner.debugBuilding ||
//                 listen == false ||
//                 debugIsInInheritedProviderUpdate,
//                 @"
//             Tried to listen to a value exposed with provider, from outside of the widget tree.
//
//                 This is likely caused by an event handler (like a button's onPressed) that called
//             Provider.of without passing `listen: false`.
//
//             To fix, write:
//             Provider.of<$T>(context, listen: false);
//
//             It is unsupported because may pointlessly rebuild the widget associated to the
//                 event handler, when the widget tree doesn't care about the value.
//
//                 The context used was: $context
//             "
//                 );

            var inheritedElement = _inheritedElementOf<T>(context);

            if (listen) {
                context.dependOnInheritedElement(inheritedElement);
            }

            return inheritedElement.value;
        }
        
        static _InheritedProviderScopeElement<T> _inheritedElementOf<T>(
            BuildContext context
        ) {
            D.assert(context != null, ()=> @"
            Tried to call context.read/watch/select or similar on a `context` that is null.

                This can happen if you used the context of a StatefulWidget and that
                StatefulWidget was disposed.
            ");
            // D.assert(
            //     _debugIsSelecting == false,
            //     'Cannot call context.read/watch/select inside the callback of a context.select'
            // );
            // assert(
            //     T != dynamic,
            //     '''
            // Tried to call Provider.of<dynamic>. This is likely a mistake and is therefore
            //     unsupported.
            //
            // If you want to expose a variable that can be anything, consider changing
            //     `dynamic` to `Object` instead.
            // ''',
            //     );
            _InheritedProviderScopeElement<T> inheritedElement = null;

            if (context.widget is _InheritedProviderScope<T>) {
                // An InheritedProvider<T>'s update tries to obtain a parent provider of
                // the same type.
                context.visitAncestorElements((parent) => {
                    inheritedElement = parent.getElementForInheritedWidgetOfExactType<
                        _InheritedProviderScope<T>>() as _InheritedProviderScopeElement<T>;
                    return false;
                });
            } else {
                inheritedElement = context.getElementForInheritedWidgetOfExactType<
                    _InheritedProviderScope<T>>() as _InheritedProviderScopeElement<T>;
            }

            if (inheritedElement == null) {
                Debug.Log("error:" + context.widget.GetType());
            }

            return inheritedElement;
        }
        
    }
    
    public class MultiProvider : Nested
    {
        public MultiProvider(
            Key key = null,
            List<SingleChildWidgetMixinStatelessWidget> providers = null,
            Widget child = null,
            TransitionBuilder builder = null
        ) :
        base(
            key: key,
            children: providers,
            child: builder != null
            ? new Builder(
                builder: (context) => builder(context, child)
                )
                : child
            )
        {
            D.assert(providers != null);
        }
    }
}