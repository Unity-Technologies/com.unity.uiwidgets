using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public abstract class GestureRecognizerFactory {
        public abstract GestureRecognizer constructorRaw();

        public abstract void initializerRaw(GestureRecognizer instance);

        internal abstract bool _debugAssertTypeMatches(Type type);
    }

    public abstract class GestureRecognizerFactory<T> : GestureRecognizerFactory where T : GestureRecognizer {
        public override GestureRecognizer constructorRaw() {
            return constructor();
        }

        public override void initializerRaw(GestureRecognizer instance) {
            initializer((T) instance);
        }

        public abstract T constructor();

        public abstract void initializer(T instance);

        internal override bool _debugAssertTypeMatches(Type type) {
            D.assert(type == typeof(T),
                () => "GestureRecognizerFactory of type " + typeof(T) + $" was used where type {type} was specified.");
            return true;
        }
    }

    public delegate T GestureRecognizerFactoryConstructor<T>() where T : GestureRecognizer;

    public delegate void GestureRecognizerFactoryInitializer<T>(T instance) where T : GestureRecognizer;

    public class GestureRecognizerFactoryWithHandlers<T> : GestureRecognizerFactory<T> where T : GestureRecognizer {
        public GestureRecognizerFactoryWithHandlers(
            GestureRecognizerFactoryConstructor<T> constructor,
            GestureRecognizerFactoryInitializer<T> initializer) {
            D.assert(constructor != null);
            D.assert(initializer != null);

            _constructor = constructor;
            _initializer = initializer;
        }

        readonly GestureRecognizerFactoryConstructor<T> _constructor;

        readonly GestureRecognizerFactoryInitializer<T> _initializer;

        public override T constructor() {
            return _constructor();
        }

        public override void initializer(T instance) {
            _initializer(instance);
        }
    }

    public class GestureDetector : StatelessWidget {
        public GestureDetector(
            Key key = null,
            Widget child = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapUpCallback onTapUp = null,
            GestureTapCallback onTap = null,
            GestureTapCancelCallback onTapCancel = null,
            GestureTapDownCallback onSecondaryTapDown = null,
            GestureTapUpCallback onSecondaryTapUp = null,
            GestureTapCancelCallback onSecondaryTapCancel = null,
            GestureDoubleTapCallback onDoubleTap = null,
            GestureLongPressCallback onLongPress = null,
            GestureLongPressStartCallback onLongPressStart = null,
            GestureLongPressMoveUpdateCallback onLongPressMoveUpdate = null,
            GestureLongPressUpCallback onLongPressUp = null,
            GestureLongPressEndCallback onLongPressEnd = null,
            GestureDragDownCallback onVerticalDragDown = null,
            GestureDragStartCallback onVerticalDragStart = null,
            GestureDragUpdateCallback onVerticalDragUpdate = null,
            GestureDragEndCallback onVerticalDragEnd = null,
            GestureDragCancelCallback onVerticalDragCancel = null,
            GestureDragDownCallback onHorizontalDragDown = null,
            GestureDragStartCallback onHorizontalDragStart = null,
            GestureDragUpdateCallback onHorizontalDragUpdate = null,
            GestureDragEndCallback onHorizontalDragEnd = null,
            GestureDragCancelCallback onHorizontalDragCancel = null,
            GestureForcePressStartCallback onForcePressStart = null,
            GestureForcePressPeakCallback onForcePressPeak = null,
            GestureForcePressUpdateCallback onForcePressUpdate = null,
            GestureForcePressEndCallback onForcePressEnd = null,
            GestureDragDownCallback onPanDown = null,
            GestureDragStartCallback onPanStart = null,
            GestureDragUpdateCallback onPanUpdate = null,
            GestureDragEndCallback onPanEnd = null,
            GestureDragCancelCallback onPanCancel = null,
            GestureScaleStartCallback onScaleStart = null,
            GestureScaleUpdateCallback onScaleUpdate = null,
            GestureScaleEndCallback onScaleEnd = null,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            DragStartBehavior dragStartBehavior = DragStartBehavior.down
        ) : base(key) {
            D.assert(() => {
                bool haveVerticalDrag =
                    onVerticalDragStart != null || onVerticalDragUpdate != null ||
                    onVerticalDragEnd != null;
                bool haveHorizontalDrag =
                    onHorizontalDragStart != null || onHorizontalDragUpdate != null ||
                    onHorizontalDragEnd != null;
                bool havePan = onPanStart != null || onPanUpdate != null || onPanEnd != null;
                bool haveScale = onScaleStart != null || onScaleUpdate != null || onScaleEnd != null;
                if (havePan || haveScale) {
                    if (havePan && haveScale) {
                        throw new UIWidgetsError(new List<DiagnosticsNode>{
                            new ErrorSummary("Incorrect GestureDetector arguments."),
                            new ErrorDescription(
                                "Having both a pan gesture recognizer and a scale gesture recognizer is redundant; scale is a superset of pan."
                            ),
                            new ErrorHint("Just use the scale gesture recognizer.")
                        });
                    }

                    string recognizer = havePan ? "pan" : "scale";
                    if (haveVerticalDrag && haveHorizontalDrag) {
                        throw new UIWidgetsError(
                            "Incorrect GestureDetector arguments.\n" +
                            $"Simultaneously having a vertical drag gesture recognizer, a horizontal drag gesture recognizer, and a {recognizer} gesture recognizer " +
                            $"will result in the {recognizer} gesture recognizer being ignored, since the other two will catch all drags."
                        );
                    }
                }

                return true;
            });

            this.child = child;
            this.onTapDown = onTapDown;
            this.onTapUp = onTapUp;
            this.onTap = onTap;
            this.onTapCancel = onTapCancel;
            this.onSecondaryTapDown = onSecondaryTapDown;
            this.onSecondaryTapUp = onSecondaryTapUp;
            this.onSecondaryTapCancel = onSecondaryTapCancel;
            this.onDoubleTap = onDoubleTap;
            this.onLongPress = onLongPress;
            this.onLongPressUp = onLongPressUp;
            this.onLongPressStart = onLongPressStart;
            this.onLongPressMoveUpdate = onLongPressMoveUpdate;
            this.onLongPressEnd = onLongPressEnd;
            this.onVerticalDragDown = onVerticalDragDown;
            this.onVerticalDragStart = onVerticalDragStart;
            this.onVerticalDragUpdate = onVerticalDragUpdate;
            this.onVerticalDragEnd = onVerticalDragEnd;
            this.onVerticalDragCancel = onVerticalDragCancel;
            this.onHorizontalDragDown = onHorizontalDragDown;
            this.onHorizontalDragStart = onHorizontalDragStart;
            this.onHorizontalDragUpdate = onHorizontalDragUpdate;
            this.onHorizontalDragEnd = onHorizontalDragEnd;
            this.onHorizontalDragCancel = onHorizontalDragCancel;
            this.onForcePressEnd = onForcePressEnd;
            this.onForcePressPeak = onForcePressPeak;
            this.onForcePressStart = onForcePressStart;
            this.onForcePressUpdate = onForcePressUpdate;
            this.onPanDown = onPanDown;
            this.onPanStart = onPanStart;
            this.onPanUpdate = onPanUpdate;
            this.onPanEnd = onPanEnd;
            this.onPanCancel = onPanCancel;
            this.onScaleStart = onScaleStart;
            this.onScaleUpdate = onScaleUpdate;
            this.onScaleEnd = onScaleEnd;
            this.behavior = behavior;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly Widget child;
        public readonly GestureTapDownCallback onTapDown;
        public readonly GestureTapUpCallback onTapUp;
        public readonly GestureTapCallback onTap;
        public readonly GestureTapCancelCallback onTapCancel;
        public readonly GestureTapDownCallback onSecondaryTapDown;
        public readonly GestureTapUpCallback onSecondaryTapUp;
        public readonly GestureTapCancelCallback onSecondaryTapCancel;
        public readonly GestureDoubleTapCallback onDoubleTap;
        public readonly GestureLongPressCallback onLongPress;
        public readonly GestureLongPressUpCallback onLongPressUp;
        public readonly GestureLongPressStartCallback onLongPressStart;
        public readonly GestureLongPressMoveUpdateCallback onLongPressMoveUpdate;
        public readonly GestureLongPressEndCallback onLongPressEnd;
        public readonly GestureDragDownCallback onVerticalDragDown;
        public readonly GestureDragStartCallback onVerticalDragStart;
        public readonly GestureDragUpdateCallback onVerticalDragUpdate;
        public readonly GestureDragEndCallback onVerticalDragEnd;
        public readonly GestureDragCancelCallback onVerticalDragCancel;
        public readonly GestureDragDownCallback onHorizontalDragDown;
        public readonly GestureDragStartCallback onHorizontalDragStart;
        public readonly GestureDragUpdateCallback onHorizontalDragUpdate;
        public readonly GestureDragEndCallback onHorizontalDragEnd;
        public readonly GestureDragCancelCallback onHorizontalDragCancel;
        public readonly GestureDragDownCallback onPanDown;
        public readonly GestureDragStartCallback onPanStart;
        public readonly GestureDragUpdateCallback onPanUpdate;
        public readonly GestureDragEndCallback onPanEnd;
        public readonly GestureDragCancelCallback onPanCancel;
        public readonly GestureScaleStartCallback onScaleStart;
        public readonly GestureScaleUpdateCallback onScaleUpdate;
        public readonly GestureScaleEndCallback onScaleEnd;
        public readonly HitTestBehavior behavior;
        public readonly DragStartBehavior dragStartBehavior;
        public readonly GestureForcePressStartCallback onForcePressStart;
        public readonly GestureForcePressPeakCallback onForcePressPeak;
        public readonly GestureForcePressUpdateCallback onForcePressUpdate;
        public readonly GestureForcePressEndCallback onForcePressEnd;

        public override Widget build(BuildContext context) {
            var gestures = new Dictionary<Type, GestureRecognizerFactory>();

            if (onTapDown != null ||
                onTapUp != null ||
                onTap != null ||
                onTapCancel != null ||
                onSecondaryTapDown != null ||
                onSecondaryTapUp != null ||
                onSecondaryTapCancel != null) {
                gestures[typeof(TapGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<TapGestureRecognizer>(
                        () => new TapGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onTapDown = onTapDown;
                            instance.onTapUp = onTapUp;
                            instance.onTap = onTap;
                            instance.onTapCancel = onTapCancel;
                            instance.onSecondaryTapDown = onSecondaryTapDown;
                            instance.onSecondaryTapUp = onSecondaryTapUp;
                            instance.onSecondaryTapCancel = onSecondaryTapCancel;
                        }
                    );
            }

            if (onDoubleTap != null) {
                gestures[typeof(DoubleTapGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<DoubleTapGestureRecognizer>(
                        () => new DoubleTapGestureRecognizer(debugOwner: this),
                        instance => { instance.onDoubleTap = onDoubleTap; }
                    );
            }

            if (onLongPress != null ||
                onLongPressUp != null ||
                onLongPressStart != null ||
                onLongPressMoveUpdate != null ||
                onLongPressEnd != null) {
                gestures[typeof(LongPressGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<LongPressGestureRecognizer>(
                        () => new LongPressGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onLongPress = onLongPress;
                            instance.onLongPressStart = onLongPressStart;
                            instance.onLongPressMoveUpdate = onLongPressMoveUpdate;
                            instance.onLongPressEnd = onLongPressEnd;
                            instance.onLongPressUp = onLongPressUp;
                        }
                    );
            }

            if (onVerticalDragDown != null ||
                onVerticalDragStart != null ||
                onVerticalDragUpdate != null ||
                onVerticalDragEnd != null ||
                onVerticalDragCancel != null) {
                gestures[typeof(VerticalDragGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<VerticalDragGestureRecognizer>(
                        () => new VerticalDragGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onDown = onVerticalDragDown;
                            instance.onStart = onVerticalDragStart;
                            instance.onUpdate = onVerticalDragUpdate;
                            instance.onEnd = onVerticalDragEnd;
                            instance.onCancel = onVerticalDragCancel;
                            instance.dragStartBehavior = dragStartBehavior;
                        }
                    );
            }

            if (onHorizontalDragDown != null ||
                onHorizontalDragStart != null ||
                onHorizontalDragUpdate != null ||
                onHorizontalDragEnd != null ||
                onHorizontalDragCancel != null) {
                gestures[typeof(HorizontalDragGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<HorizontalDragGestureRecognizer>(
                        () => new HorizontalDragGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onDown = onHorizontalDragDown;
                            instance.onStart = onHorizontalDragStart;
                            instance.onUpdate = onHorizontalDragUpdate;
                            instance.onEnd = onHorizontalDragEnd;
                            instance.onCancel = onHorizontalDragCancel;
                            instance.dragStartBehavior = dragStartBehavior;
                        }
                    );
            }

            if (onPanDown != null ||
                onPanStart != null ||
                onPanUpdate != null ||
                onPanEnd != null ||
                onPanCancel != null) {
                gestures[typeof(PanGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<PanGestureRecognizer>(
                        () => new PanGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onDown = onPanDown;
                            instance.onStart = onPanStart;
                            instance.onUpdate = onPanUpdate;
                            instance.onEnd = onPanEnd;
                            instance.onCancel = onPanCancel;
                            instance.dragStartBehavior = dragStartBehavior;
                        }
                    );
            }

            if (onScaleStart != null ||
                onScaleUpdate != null ||
                onScaleEnd != null) {
                gestures[typeof(ScaleGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<ScaleGestureRecognizer>(
                        () => new ScaleGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onStart = onScaleStart;
                            instance.onUpdate = onScaleUpdate;
                            instance.onEnd = onScaleEnd;
                        }
                    );
            }
            if (onForcePressStart != null ||
                onForcePressPeak != null ||
                onForcePressUpdate != null ||
                onForcePressEnd != null) {
                gestures[typeof(ForcePressGestureRecognizer)] = 
                    new GestureRecognizerFactoryWithHandlers<ForcePressGestureRecognizer>(
                    () => new ForcePressGestureRecognizer(debugOwner: this),
                    (ForcePressGestureRecognizer instance) => {
                        instance.onStart = onForcePressStart;
                        instance.onPeak = onForcePressPeak;
                        instance.onUpdate = onForcePressUpdate;
                        instance.onEnd = onForcePressEnd;
                    }
                );
            }


            return new RawGestureDetector(
                gestures: gestures,
                behavior: behavior,
                child: child
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<DragStartBehavior>("startBehavior", dragStartBehavior));
        }
    }

    public class RawGestureDetector : StatefulWidget {
        public RawGestureDetector(
            Key key = null,
            Widget child = null,
            Dictionary<Type, GestureRecognizerFactory> gestures = null,
            HitTestBehavior? behavior = null
        ) : base(key: key) {
            D.assert(gestures != null);
            this.child = child;
            this.gestures = gestures ?? new Dictionary<Type, GestureRecognizerFactory>();
            this.behavior = behavior;
        }

        public readonly Widget child;

        public readonly Dictionary<Type, GestureRecognizerFactory> gestures;

        public readonly HitTestBehavior? behavior;

        public override State createState() {
            return new RawGestureDetectorState();
        }
    }

    public class RawGestureDetectorState : State<RawGestureDetector> {
        Dictionary<Type, GestureRecognizer> _recognizers = new Dictionary<Type, GestureRecognizer>();

        public override void initState() {
            base.initState();
            _syncAll(widget.gestures);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            _syncAll(widget.gestures);
        }

        public void replaceGestureRecognizers(Dictionary<Type, GestureRecognizerFactory> gestures) {
            D.assert(() => {
                if (!context.findRenderObject().owner.debugDoingLayout) {
                    throw new UIWidgetsError(new List<DiagnosticsNode> {
                        new ErrorSummary(
                            "Unexpected call to replaceGestureRecognizers() method of RawGestureDetectorState."),
                        new ErrorDescription(
                            "The replaceGestureRecognizers() method can only be called during the layout phase."),
                        new ErrorHint(
                            "To set the gesture recognizers at other times, trigger a new build using setState() " +
                            "and provide the new gesture recognizers as constructor arguments to the corresponding " +
                            "RawGestureDetector or GestureDetector object."
                        )
                    });
                }

                return true;
            });
            _syncAll(gestures);
        }

        public override void dispose() {
            foreach (GestureRecognizer recognizer in _recognizers.Values) {
                recognizer.dispose();
            }

            _recognizers = null;
            base.dispose();
        }

        void _syncAll(Dictionary<Type, GestureRecognizerFactory> gestures) {
            D.assert(_recognizers != null);
            var oldRecognizers = _recognizers;
            _recognizers = new Dictionary<Type, GestureRecognizer>();

            foreach (Type type in gestures.Keys) {
                D.assert(gestures[type] != null);
                D.assert(gestures[type]._debugAssertTypeMatches(type));
                D.assert(!_recognizers.ContainsKey(type));
                _recognizers[type] = oldRecognizers.ContainsKey(type)
                    ? oldRecognizers[type]
                    : gestures[type].constructorRaw();
                D.assert(_recognizers[type].GetType() == type,
                    () => "GestureRecognizerFactory of type " + type + " created a GestureRecognizer of type " +
                          _recognizers[type].GetType() +
                          ". The GestureRecognizerFactory must be specialized with the type of the class that it returns from its constructor method.");
                gestures[type].initializerRaw(_recognizers[type]);
            }

            foreach (Type type in oldRecognizers.Keys) {
                if (!_recognizers.ContainsKey(type)) {
                    oldRecognizers[type].dispose();
                }
            }
        }

        void _handlePointerDown(PointerDownEvent evt) {
            D.assert(_recognizers != null);
            foreach (GestureRecognizer recognizer in _recognizers.Values) {
                recognizer.addPointer(evt);
            }
        }

        void _handlePointerScroll(PointerScrollEvent evt) {
            D.assert(_recognizers != null);
            foreach (GestureRecognizer recognizer in _recognizers.Values) {
                recognizer.addScrollPointer(evt);
            }
        }

        HitTestBehavior _defaultBehavior {
            get { return widget.child == null ? HitTestBehavior.translucent : HitTestBehavior.deferToChild; }
        }


        public override Widget build(BuildContext context) {
            Widget result = new Listener(
                onPointerDown: _handlePointerDown,
                behavior: widget.behavior ?? _defaultBehavior,
                child: widget.child
            );
           
            return result;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            if (_recognizers == null) {
                properties.add(DiagnosticsNode.message("DISPOSED"));
            }
            else {
                List<string> gestures = LinqUtils<string, GestureRecognizer>.SelectList(_recognizers.Values,  (recognizer => recognizer.debugDescription));
                properties.add(new EnumerableProperty<string>("gestures", gestures, ifEmpty: "<none>"));
                properties.add(new EnumerableProperty<GestureRecognizer>("recognizers", _recognizers.Values,
                    level: DiagnosticLevel.fine));
            }

            properties.add(new EnumProperty<HitTestBehavior?>("behavior", widget.behavior,
                defaultValue: foundation_.kNullDefaultValue));
        }
    }
}