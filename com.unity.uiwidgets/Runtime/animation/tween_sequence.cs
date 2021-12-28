using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.scheduler;
using System;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.UIWidgets.animation {
    public class TweenSequence<T> : Animatable<T> {
        public TweenSequence(List<TweenSequenceItem<T>> items) {
            D.assert(items != null);
            D.assert(items.isNotEmpty);
            foreach (var item in items) {
                _items.Add(item);
            }
            //_items.addAll(items);
            float totalWeight = 0.0f;
            foreach (TweenSequenceItem<T> item in _items)
                totalWeight += item.weight;
            D.assert(totalWeight > 0.0f);
            float start = 0.0f;
            for (int i = 0; i < _items.Count; i += 1) {
              float end = i == _items.Count - 1 ? 1.0f : start + _items[i].weight / totalWeight;
              _intervals.Add(new _Interval(start, end));
              start = end;
            }
        }

        public readonly List<TweenSequenceItem<T>> _items = new List<TweenSequenceItem<T>>();
        public readonly List<_Interval> _intervals = new List<_Interval>();

        public T _evaluateAt(float t, int index) {
            TweenSequenceItem<T> element = _items[index];
            float tInterval = _intervals[index].value(t);
            return element.tween.transform(tInterval);
        }

        public override T transform(float t) {
            D.assert(t >= 0.0 && t <= 1.0);
            if (t == 1.0)
              return _evaluateAt(t, _items.Count - 1);
            for (int index = 0; index < _items.Count; index++) {
              if (_intervals[index].contains(t))
                return _evaluateAt(t, index);
            }
            D.assert(false, ()=> $"TweenSequence.evaluate() could not find an interval for {t}");
            return default(T);
        }
        public override string ToString(){
            return $"TweenSequence({_items.Count} items)";
        }
        
    }

    class FlippedTweenSequence : TweenSequence<float> {
        public FlippedTweenSequence(List<TweenSequenceItem<float>> items)
            : base(items) {
            D.assert(items != null);
        }

        public override float transform(float t) => 1 - base.transform(1 - t);
    }

    public class TweenSequenceItem<T> {

        public TweenSequenceItem(
            Animatable<T> tween = null,
            float? weight = 0.0f
        ) {
            D.assert(tween != null);
            D.assert(weight != null);
            D.assert(weight > 0.0);
            this.tween = tween;
            this.weight = weight.Value;
        }

        public readonly Animatable<T> tween;


        public readonly float weight;
    }

    public class _Interval {
         public _Interval(float start, float end) {
             this.start = start;
             this.end = end;
             D.assert(end > start);
        }

        public readonly float start;
        public readonly float end;

        public bool contains(float t) {
            return  t >= start && t < end;
        }

        public float value(float t) {
            return (t - start) / (end - start);
        }


        public override string ToString() {
            return   $"<{start}," +  $" {end}>";
        }

        
    }
}