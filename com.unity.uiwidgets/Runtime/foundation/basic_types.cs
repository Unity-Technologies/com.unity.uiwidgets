using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.UIWidgets.foundation {
    public delegate void ValueChanged<T>(T value);

    public delegate void ValueSetter<T>(T value);

    public delegate T ValueGetter<T>();

    public delegate IEnumerable<T> IterableFilter<T>(IEnumerable<T> input);

    public delegate Future AsyncCallback();

    public delegate Future AsyncValueSetter<T>(T value);

    public delegate Future<T> AsyncValueGetter<T>();

    public struct Factory<T> {
        public Factory(ValueGetter<T> constructor) {
            D.assert(constructor != null);
            this.constructor = constructor;
        }

        public readonly ValueGetter<T> constructor;

        public Type type => typeof(T);

        public override string ToString() {
            return $"Factory(type: {type})";
        }
    }


    public static class ObjectUtils {
        public static T SafeDestroy<T>(T obj) where T : Object {
            if (Application.isEditor) {
                Object.DestroyImmediate(obj);
            }
            else {
                Object.Destroy(obj);
            }

            return null;
        }
    }

    
}