using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public class UniqueKey : LocalKey {
        public UniqueKey() {
        }

        public override string ToString() {
            return $"[#{foundation_.shortHash(this)}]";
        }
    }

    public class ObjectKey : LocalKey, IEquatable<ObjectKey> {
        public ObjectKey(object value) {
            this.value = value;
        }

        public readonly object value;

        public bool Equals(ObjectKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return ReferenceEquals(value, other.value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ObjectKey) obj);
        }

        public override int GetHashCode() {
            return (value != null ? RuntimeHelpers.GetHashCode(value) : 0);
        }

        public static bool operator ==(ObjectKey left, ObjectKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(ObjectKey left, ObjectKey right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            if (GetType() == typeof(ObjectKey)) {
                return $"[{foundation_.describeIdentity(value)}]";
            }

            return $"[{GetType()} {foundation_.describeIdentity(value)}]";
        }
    }

    public class CompositeKey : Key, IEquatable<CompositeKey> {
        readonly object componentKey1;
        readonly object componentKey2;

        public CompositeKey(object componentKey1, object componentKey2) {
            this.componentKey1 = componentKey1;
            this.componentKey2 = componentKey2;
        }

        public bool Equals(CompositeKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(componentKey1, other.componentKey1) &&
                   Equals(componentKey2, other.componentKey2);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((CompositeKey) obj);
        }

        public override int GetHashCode() {
            return (componentKey1 != null ? componentKey1.GetHashCode() : 0) ^
                   (componentKey2 != null ? componentKey2.GetHashCode() : 0);
        }

        public static bool operator ==(CompositeKey left, CompositeKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(CompositeKey left, CompositeKey right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return GetType() + $"({componentKey1},{componentKey2})";
        }
    }

    public abstract class GlobalKey : Key {
        protected GlobalKey() {
        }

        public new static GlobalKey key(string debugLabel = null) {
            return new LabeledGlobalKey<State<StatefulWidget>>(debugLabel);
        }

        static readonly Dictionary<CompositeKey, Element> _registry =
            new Dictionary<CompositeKey, Element>();

        static readonly HashSet<Element> _debugIllFatedElements = new HashSet<Element>();

        readonly static Dictionary<Element, Dictionary<Element, GlobalKey>> _debugReservations =
            new Dictionary<Element, Dictionary<Element, GlobalKey>>();
        
        internal static void _debugRemoveReservationFor(Element parent, Element child) {
            D.assert(() => {
                D.assert(parent != null); 
                D.assert(child != null);
    
                if (_debugReservations.ContainsKey(parent) && _debugReservations[parent].ContainsKey(child)) {
                    _debugReservations[parent]?.Remove(child);
                }
                
                return true;
            });
        }


        internal void _register(Element element) {
            CompositeKey compKey = new CompositeKey(Window.instance, this);
            D.assert(() => {
                if (_registry.ContainsKey(compKey)) {
                    D.assert(element.widget != null);
                    D.assert(_registry[compKey].widget != null);
                    D.assert(element.widget.GetType() != _registry[compKey].widget.GetType());
                    _debugIllFatedElements.Add(_registry[compKey]);
                }

                return true;
            });
            _registry[compKey] = element;
        }

        internal void _unregister(Element element) {
            CompositeKey compKey = new CompositeKey(Window.instance, this);
            D.assert(() => {
                if (_registry.ContainsKey(compKey) && _registry[compKey] != element) {
                    D.assert(element.widget != null);
                    D.assert(_registry[compKey].widget != null);
                    D.assert(element.widget.GetType() != _registry[compKey].widget.GetType());
                }

                return true;
            });
            if (_registry[compKey] == element) {
                _registry.Remove(compKey);
            }
        }

        internal void _debugReserveFor(Element parent, Element child) {
            CompositeKey compKey = new CompositeKey(Window.instance, this);
            D.assert(() => {
                D.assert(parent != null);
                D.assert(child != null);
                _debugReservations.putIfAbsent(parent, () => new Dictionary<Element, GlobalKey>());
                _debugReservations[parent][child] = this;
                return true;
            });
        }

        internal static void _debugVerifyGlobalKeyReservation() {
            D.assert(() => {
                Dictionary<GlobalKey, Element> keyToParent = new Dictionary<GlobalKey, Element>();
                _debugReservations.ToList().ForEach((entry) => {
                    Element parent = entry.Key;
                    Dictionary<Element, GlobalKey> chidToKey = entry.Value;
                    if (parent.renderObject?.attached == false)
                        return;
                    chidToKey.ToList().ForEach((childEntry) => {
                        Element child = childEntry.Key;
                        GlobalKey key = childEntry.Value;
                        if (child._parent == null)
                            return;
                        if (keyToParent.ContainsKey(key) && keyToParent[key] != parent) {
                            Element older = keyToParent[key];
                            Element newer = parent;
                            UIWidgetsError error = null;
                            if (older.toString() != newer.toString()) {
                                error = new UIWidgetsError(new List<DiagnosticsNode>{
                                    new ErrorSummary("Multiple widgets used the same GlobalKey."),
                                    new ErrorDescription(
                                        $"The key {key} was used by multiple widgets. The parents of those widgets were:\n" +
                                        $"- {older.toString()}\n" +
                                        $"- {newer.toString()}\n" +
                                        "A GlobalKey can only be specified on one widget at a time in the widget tree."
                                    )
                                });
                            }
                            else {
                                error = new UIWidgetsError(new List<DiagnosticsNode>{
                                    new ErrorSummary("Multiple widgets used the same GlobalKey."),
                                    new ErrorDescription(
                                        $"The key {key} was used by multiple widgets. The parents of those widgets were " +
                                        "different widgets that both had the following description:\n" +
                                        $"  {parent.toString()}\n" +
                                        "A GlobalKey can only be specified on one widget at a time in the widget tree."
                                    )
                                });
                            }

                            if (child._parent != older) {
                                older.visitChildren((Element currentChild) => {
                                    if (currentChild == child)
                                        older.forgetChild(child);
                                });
                            }

                            if (child._parent != newer) {
                                newer.visitChildren((Element currentChild) => {
                                    if (currentChild == child)
                                        newer.forgetChild(child);
                                });
                            }

                            throw error;
                        }
                        else {
                            keyToParent[key] = parent;
                        }
                    });
                });
                _debugReservations.Clear();
                return true;
            });
        }

        internal static void _debugVerifyIllFatedPopulation() {
            D.assert(() => {
                Dictionary<GlobalKey, HashSet<Element>> duplicates = null;
                foreach (Element element in _debugIllFatedElements) {
                    if (element._debugLifecycleState != _ElementLifecycle.defunct) {
                        D.assert(element != null);
                        D.assert(element.widget != null);
                        D.assert(element.widget.key != null);
                        GlobalKey key = (GlobalKey) element.widget.key;
                        CompositeKey compKey = new CompositeKey(Window.instance, key);
                        D.assert(_registry.ContainsKey(compKey));

                        duplicates = duplicates ?? new Dictionary<GlobalKey, HashSet<Element>>();
                        var elements = duplicates.putIfAbsent(key, () => new HashSet<Element>());
                        elements.Add(element);
                        elements.Add(_registry[compKey]);
                    }
                }

                _debugIllFatedElements.Clear();

                if (duplicates != null) {
                    List<DiagnosticsNode> information = new List<DiagnosticsNode>();
                    information.Add(new ErrorSummary("Multiple widgets used the same GlobalKey."));
                    foreach (GlobalKey key in duplicates.Keys) {
                        HashSet<Element> elements = duplicates.getOrDefault(key);
                        information.Add( Element.describeElements($"The key $key was used by {elements.Count} widgets", elements));
                    }

                    information.Add(new ErrorDescription("A GlobalKey can only be specified on one widget at a time in the widget tree."));
                    throw new UIWidgetsError(information);
                }

                return true;
            });
        }

        internal Element _currentElement {
            get {
                Element result;
                CompositeKey compKey = new CompositeKey(Window.instance, this);
                _registry.TryGetValue(compKey, out result);
                return result;
            }
        }

        public BuildContext currentContext {
            get { return _currentElement; }
        }

        public Widget currentWidget {
            get { return _currentElement == null ? null : _currentElement.widget; }
        }

        public State currentState {
            get {
                Element element = _currentElement;
                if (element is StatefulElement) {
                    var statefulElement = (StatefulElement) element;
                    State state = statefulElement.state;
                    if (state is State) {
                        return (State) state;
                    }
                }

                return null;
            }
        }
    }

    public abstract class GlobalKey<T> : GlobalKey where T : State {
        public new static GlobalKey<T> key(string debugLabel = null) {
            return new LabeledGlobalKey<T>(debugLabel);
        }

        public new T currentState {
            get {
                Element element = _currentElement;
                if (element is StatefulElement) {
                    var statefulElement = (StatefulElement) element;
                    State state = statefulElement.state;
                    if (state is T) {
                        return (T) state;
                    }
                }

                return null;
            }
        }
    }

    public class LabeledGlobalKey<T> : GlobalKey<T> where T : State {
        public LabeledGlobalKey(string _debugLabel = null) {
            this._debugLabel = _debugLabel;
        }

        readonly string _debugLabel;

        public override string ToString() {
            string label = _debugLabel != null ? " " + _debugLabel : "";
            if (GetType() == typeof(LabeledGlobalKey<T>)) {
                return $"[GlobalKey#{foundation_.shortHash(this)}{label}]";
            }

            return $"[{foundation_.describeIdentity(this)}{label}]";
        }
    }

    public class GlobalObjectKey<T> : GlobalKey<T>, IEquatable<GlobalObjectKey<T>> where T : State {
        public GlobalObjectKey(object value) {
            this.value = value;
        }

        public readonly object value;

        public bool Equals(GlobalObjectKey<T> other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return ReferenceEquals(value, other.value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((GlobalObjectKey<T>) obj);
        }

        public override int GetHashCode() {
            return (value != null ? RuntimeHelpers.GetHashCode(value) : 0);
        }

        public static bool operator ==(GlobalObjectKey<T> left, GlobalObjectKey<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(GlobalObjectKey<T> left, GlobalObjectKey<T> right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            string selfType = GetType().ToString();
            string suffix = "`1[UIWidgets.widgets.State]";
            if (selfType.EndsWith(suffix)) {
                selfType = selfType.Substring(0, selfType.Length - suffix.Length);
            }

            return $"[{selfType} {foundation_.describeIdentity(value)}]";
        }
    }

    public interface TypeMatcher {
        bool check(object obj);
    }

    public class TypeMatcher<T> : TypeMatcher {
        public bool check(object obj) {
            return obj is T;
        }
    }

    public abstract class Widget : DiagnosticableTree {
        protected Widget(Key key = null) {
            this.key = key;
        }

        public readonly Key key;

        public abstract Element createElement();

        public override string toStringShort() {
            return key == null ? GetType().ToString() : $"{GetType()}-{key}";
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.dense;
        }

        public static bool canUpdate(Widget oldWidget, Widget newWidget) {
            return oldWidget.GetType() == newWidget.GetType() && Equals(oldWidget.key, newWidget.key);
        }

        internal static int _debugConcreteSubtype(Widget widget) {
            return widget is StatefulWidget ? 1 :
                widget is StatelessWidget ? 2 :
                0;
        }
    }

    public abstract class StatelessWidget : Widget {
        protected StatelessWidget(Key key = null) : base(key: key) {
        }

        public override Element createElement() {
            return new StatelessElement(this);
        }

        public abstract Widget build(BuildContext context);
    }

    public abstract class StatefulWidget : Widget {
        protected StatefulWidget(Key key = null) : base(key: key) {
        }

        public override Element createElement() {
            return new StatefulElement(this);
        }

        public abstract State createState();
    }

    enum _StateLifecycle {
        created,
        initialized,
        ready,
        defunct,
    }

    public delegate void StateSetter(VoidCallback fn);

    public abstract class State : Diagnosticable {
        public StatefulWidget widget {
            get { return _widget; }
        }

        internal StatefulWidget _widget;

        internal _StateLifecycle _debugLifecycleState = _StateLifecycle.created;

        public virtual bool _debugTypesAreRight(Widget widget) {
            return widget is StatefulWidget;
        }

        public BuildContext context {
            get { return _element; }
        }

        internal StatefulElement _element;

        public bool mounted {
            get { return _element != null; }
        }

        public virtual void initState() {
            D.assert(_debugLifecycleState == _StateLifecycle.created);
        }

        public virtual void didUpdateWidget(StatefulWidget oldWidget) {
        }

        public virtual void reassemble() {
        }

        public void setState(VoidCallback fn = null) {
            D.assert(() => {
                if (_debugLifecycleState == _StateLifecycle.defunct) {
                    throw new UIWidgetsError(new List<DiagnosticsNode> {
                        new ErrorSummary($"setState() called after dispose(): {this}"),
                        new ErrorDescription(
                          "This error happens if you call setState() on a State object for a widget that " +
                          "no longer appears in the widget tree (e.g., whose parent widget no longer " +
                          "includes the widget in its build). This error can occur when code calls " +
                          "setState() from a timer or an animation callback."
                        ),
                        new ErrorHint(
                          "The preferred solution is " +
                          "to cancel the timer or stop listening to the animation in the dispose() " +
                          "callback. Another solution is to check the \"mounted\" property of this " +
                          "object before calling setState() to ensure the object is still in the " +
                          "tree."
                        ),
                        new ErrorHint(
                          "This error might indicate a memory leak if setState() is being called " +
                          "because another object is retaining a reference to this State object " +
                          "after it has been removed from the tree. To avoid memory leaks, " +
                          "consider breaking the reference to this object during dispose()."
                        )
                    });
                }

                if (_debugLifecycleState == _StateLifecycle.created && !mounted) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary($"setState() called in constructor: {this}"),
                        new ErrorHint(
                            "This happens when you call setState() on a State object for a widget that " +
                            "hasn't been inserted into the widget tree yet. It is not necessary to call " +
                            "setState() in the constructor, since the state is already assumed to be dirty " +
                            "when it is initially created."
                        )
                    });
                }

                return true;
            });

            
            if (fn != null) {
                fn();
            }

            _element?.markNeedsBuild();
        }

        public virtual void deactivate() {
        }

        public virtual void dispose() {
            D.assert(_debugLifecycleState == _StateLifecycle.ready);
            D.assert(() => {
                _debugLifecycleState = _StateLifecycle.defunct;
                return true;
            });
        }

        public abstract Widget build(BuildContext context);

        public virtual void didChangeDependencies() {
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);

            D.assert(() => {
                properties.add(new EnumProperty<_StateLifecycle>(
                    "lifecycle state", _debugLifecycleState,
                    defaultValue: _StateLifecycle.ready));
                return true;
            });

            properties.add(new ObjectFlagProperty<StatefulWidget>(
                "_widget", _widget, ifNull: "no widget"));
            properties.add(new ObjectFlagProperty<StatefulElement>(
                "_element", _element, ifNull: "not mounted"));
        }
    }

    public abstract class State<T> : State where T : StatefulWidget {
        public new T widget {
            get { return (T) base.widget; }
        }

        public override bool _debugTypesAreRight(Widget widget) {
            return widget is T;
        }
    }

    public abstract class ProxyWidget : Widget {
        protected ProxyWidget(Key key = null, Widget child = null) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;
    }

    public abstract class ParentDataWidget : ProxyWidget {
        public ParentDataWidget(Key key = null, Widget child = null)
            : base(key: key, child: child) {
        }

        public abstract bool debugIsValidRenderObject(RenderObject renderObject);

        internal abstract IEnumerable<DiagnosticsNode> _debugDescribeIncorrectParentDataType(
            ParentData parentData,
            RenderObjectWidget parentDataCreator = null,
            DiagnosticsNode ownershipChain = null
        );

        public virtual Type debugTypicalAncestorWidgetClass { get; }

        public abstract void applyParentData(RenderObject renderObject);

        public virtual bool debugCanApplyOutOfTurn() {
            return false;
        }
    }

    public abstract class ParentDataWidget<T> : ParentDataWidget where T : IParentData {
        public ParentDataWidget(Key key = null, Widget child = null)
            : base(key: key, child: child) {
        }

        public override Element createElement() {
            return new ParentDataElement(this);
        }
        

        public override bool debugIsValidRenderObject(RenderObject renderObject) {
            D.assert(typeof(T) != typeof(IParentData));
            return renderObject.parentData is T;
        }

        public override Type debugTypicalAncestorWidgetClass { get; }

        internal override IEnumerable<DiagnosticsNode> _debugDescribeIncorrectParentDataType(
            ParentData parentData,
            RenderObjectWidget parentDataCreator = null,
            DiagnosticsNode ownershipChain = null
        ) {
            D.assert(typeof(T) != typeof(ParentData));
            D.assert(debugTypicalAncestorWidgetClass != null);

            List<DiagnosticsNode> result = new List<DiagnosticsNode>();
            string description =
                $"The ParentDataWidget {this} wants to apply ParentData of type {typeof(T)} to a RenderObject";
            if (parentData == null) {
                result.Add( new ErrorDescription(
                    $"{description}, which has not been set up to receive any ParentData."
                ));
            }
            else {
                result.Add( new ErrorDescription(
                    $"{description}, which has been set up to accept ParentData of incompatible type {parentData.GetType()}."
                ));
            }

            result.Add(
                new ErrorHint( $"Usually, this means that the {GetType()} widget has the wrong ancestor RenderObjectWidget. " +
                               $"Typically, {GetType()} widgets are placed directly inside {debugTypicalAncestorWidgetClass} widgets."
                ));
               
            if (parentDataCreator != null) {
                result.Add(
                    new ErrorHint(
                    $"The offending {GetType()} is currently placed inside a {parentDataCreator.GetType()} widget."));
            }

            if (ownershipChain != null) {
                result.Add(new ErrorDescription(
                    $"The ownership chain for the RenderObject that received the incompatible parent data was:\n  {ownershipChain}"
                ));
            }

            return result;
        }
    }

    public abstract class InheritedWidget : ProxyWidget {
        protected InheritedWidget(Key key = null, Widget child = null) : base(key, child) {
        }

        public override Element createElement() {
            return new InheritedElement(this);
        }

        public abstract bool updateShouldNotify(InheritedWidget oldWidget);
    }

    public abstract class RenderObjectWidget : Widget {
        protected RenderObjectWidget(Key key = null) : base(key) {
        }

        public abstract RenderObject createRenderObject(BuildContext context);

        public virtual void updateRenderObject(BuildContext context, RenderObject renderObject) {
        }

        public virtual void didUnmountRenderObject(RenderObject renderObject) {
        }
    }

    public abstract class LeafRenderObjectWidget : RenderObjectWidget {
        protected LeafRenderObjectWidget(Key key = null) : base(key: key) {
        }

        public override Element createElement() {
            return new LeafRenderObjectElement(this);
        }
    }

    public abstract class SingleChildRenderObjectWidget : RenderObjectWidget {
        protected SingleChildRenderObjectWidget(Key key = null, Widget child = null) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override Element createElement() {
            return new SingleChildRenderObjectElement(this);
        }
    }

    public abstract class MultiChildRenderObjectWidget : RenderObjectWidget {
        protected MultiChildRenderObjectWidget(Key key = null, List<Widget> children = null) : base(key: key) {
            children = children ?? new List<Widget>();
            D.assert(() => {
                int index = children.IndexOf(null);
                if (index >= 0) {
                    throw new UIWidgetsError(
                        $"{GetType()}'s children must not contain any null values, " +
                        $"but a null value was found at index {index}");
                }

                return true;
            });
            this.children = children;
        }

        public readonly List<Widget> children;

        public override Element createElement() {
            return new MultiChildRenderObjectElement(this);
        }
    }

    enum _ElementLifecycle {
        initial,
        active,
        inactive,
        defunct,
    }

    class _InactiveElements {
        bool _locked = false;
        readonly HashSet<Element> _elements = new HashSet<Element>();

        internal void _unmount(Element element) {
            D.assert(element._debugLifecycleState == _ElementLifecycle.inactive);
            D.assert(() => {
                if (WidgetsD.debugPrintGlobalKeyedWidgetLifecycle) {
                    if (element.widget.key is GlobalKey) {
                        Debug.LogFormat("Discarding {0} from inactive elements list.", element);
                    }
                }

                return true;
            });

            element.visitChildren(child => {
                D.assert(child._parent == element);
                _unmount(child);
            });
            element.unmount();

            D.assert(element._debugLifecycleState == _ElementLifecycle.defunct);
        }

        internal void _unmountAll() {
            _locked = true;

            List<Element> elements = _elements.ToList();
            elements.Sort(Element._sort);
            _elements.Clear();

            try {
                elements.Reverse();
                elements.ForEach(_unmount);
            }
            finally {
                D.assert(_elements.isEmpty());
                _locked = false;
            }
        }

        internal static void _deactivateRecursively(Element element) {
            D.assert(element._debugLifecycleState == _ElementLifecycle.active);
            element.deactivate();
            D.assert(element._debugLifecycleState == _ElementLifecycle.inactive);
            element.visitChildren(_deactivateRecursively);
            D.assert(() => {
                element.debugDeactivated();
                return true;
            });
        }

        internal void add(Element element) {
            D.assert(!_locked);
            D.assert(!_elements.Contains(element));
            D.assert(element._parent == null);

            if (element._active) {
                _deactivateRecursively(element);
            }

            _elements.Add(element);
        }

        internal void remove(Element element) {
            D.assert(!_locked);
            D.assert(_elements.Contains(element));
            D.assert(element._parent == null);
            _elements.Remove(element);
            D.assert(!element._active);
        }

        internal bool debugContains(Element element) {
            bool result = false;
            D.assert(() => {
                result = _elements.Contains(element);
                return true;
            });
            return result;
        }
    }

    public delegate void ElementVisitor(Element element);

    public delegate bool ElementVisitorBool(Element element);

    public interface BuildContext {
        Widget widget { get; }

        BuildOwner owner { get; }
        
        bool debugDoingBuild { get; }

        RenderObject findRenderObject();

        Size size { get; }

        InheritedWidget inheritFromElement(InheritedElement ancestor, object aspect = null);

        InheritedWidget dependOnInheritedElement(InheritedElement ancestor, object aspect = null);

        InheritedWidget inheritFromWidgetOfExactType(Type targetType, object aspect = null);

        T dependOnInheritedWidgetOfExactType<T>(object aspect = null) where T : InheritedWidget;

        InheritedElement ancestorInheritedElementForWidgetOfExactType(Type targetType);

        InheritedElement getElementForInheritedWidgetOfExactType<T>() where T : InheritedWidget;

        Widget ancestorWidgetOfExactType(Type targetType);

        T findAncestorWidgetOfExactType<T>() where T : Widget;

        State ancestorStateOfType(TypeMatcher matcher);
        T findAncestorStateOfType<T>() where T : State; 
        
        State rootAncestorStateOfType(TypeMatcher matcher);

        T findRootAncestorStateOfType<T>() where T: State;

        RenderObject ancestorRenderObjectOfType(TypeMatcher matcher);

        T findAncestorRenderObjectOfType<T>() where T : RenderObject;

        void visitAncestorElements(ElementVisitorBool visitor);

        void visitChildElements(ElementVisitor visitor);

        DiagnosticsNode describeElement(string name, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.errorProperty);

        DiagnosticsNode describeWidget(string name, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.errorProperty);

        List<DiagnosticsNode> describeMissingAncestor(Type expectedAncestorType);

        DiagnosticsNode describeOwnershipChain(string name);
    }

    public class BuildOwner {
        public BuildOwner(VoidCallback onBuildScheduled = null) {
            this.onBuildScheduled = onBuildScheduled;
        }

        public VoidCallback onBuildScheduled;

        internal readonly _InactiveElements _inactiveElements = new _InactiveElements();

        readonly List<Element> _dirtyElements = new List<Element>();

        bool _scheduledFlushDirtyElements = false;

        bool? _dirtyElementsNeedsResorting = null;

        bool _debugIsInBuildScope {
            get { return _dirtyElementsNeedsResorting != null; }
        }

        public FocusManager focusManager = new FocusManager();

        public void scheduleBuildFor(Element element) {
            D.assert(element != null);
            D.assert(element.owner == this);
            D.assert(() => {
                if (WidgetsD.debugPrintScheduleBuildForStacks) {
                    Debug.LogFormat("scheduleBuildFor() called for {0} {1}", element,
                              (_dirtyElements.Contains(element) ? " (ALREADY IN LIST)" : ""));
                }

                if (!element.dirty) {
                    throw new UIWidgetsError(
                      new List<DiagnosticsNode>() {
                          new ErrorSummary("scheduleBuildFor() called for a widget that is not marked as dirty."),
                          element.describeElement("The method was called for the following element"),
                          new ErrorDescription(
                              "This element is not current marked as dirty. Make sure to set the dirty flag before " +
                          "calling scheduleBuildFor()."),
                          new ErrorHint(
                              "If you did not attempt to call scheduleBuildFor() yourself, then this probably " +
                          "indicates a bug in the widgets framework."
                          ),
                      }
                    );
                }

                return true;
            });

            if (element._inDirtyList) {
                D.assert(() => {
                    if (WidgetsD.debugPrintScheduleBuildForStacks) {
                        Debug.LogFormat(
                            "BuildOwner.scheduleBuildFor() called; _dirtyElementsNeedsResorting was {0} (now true); dirty list is: {1}",
                            _dirtyElementsNeedsResorting, _dirtyElements);
                    }

                    if (!_debugIsInBuildScope) {
                        throw new UIWidgetsError(
                            new List<DiagnosticsNode>() {
                                new ErrorSummary("BuildOwner.scheduleBuildFor() called inappropriately."),
                                new ErrorHint(
                                    "The BuildOwner.scheduleBuildFor() method should only be called while the " +
                                "buildScope() method is actively rebuilding the widget tree."
                                )
                            }
                        );
                    }

                    return true;
                });

                _dirtyElementsNeedsResorting = true;
                return;
            }

            if (!_scheduledFlushDirtyElements && onBuildScheduled != null) {
                _scheduledFlushDirtyElements = true;
                onBuildScheduled();
            }

            _dirtyElements.Add(element);
            element._inDirtyList = true;

            D.assert(() => {
                if (WidgetsD.debugPrintScheduleBuildForStacks) {
                    Debug.Log("...dirty list is now: " + _dirtyElements);
                }

                return true;
            });
        }

        int _debugStateLockLevel = 0;
        internal bool _debugStateLocked {
            get { return _debugStateLockLevel > 0; }
        }

        public bool debugBuilding {
            get { return _debugBuilding; }
        }
        bool _debugBuilding = false;

        internal Element _debugCurrentBuildTarget;

        public void lockState(VoidCallback callback) {
            D.assert(callback != null);
            D.assert(_debugStateLockLevel >= 0);
            D.assert(() => {
                _debugStateLockLevel += 1;
                return true;
            });

            try {
                callback();
            }
            finally {
                D.assert(() => {
                    _debugStateLockLevel -= 1;
                    return true;
                });
            }

            D.assert(_debugStateLockLevel >= 0);
        }

        public void buildScope(Element context, VoidCallback callback = null) {
            if (callback == null && _dirtyElements.isEmpty()) {
                return;
            }

            D.assert(context != null);
            D.assert(_debugStateLockLevel >= 0);
            D.assert(!_debugBuilding);
            D.assert(() => {
                if (WidgetsD.debugPrintBuildScope) {
                    Debug.LogFormat($"buildScope called with context {context}; dirty list is: {_dirtyElements}");
                }
                _debugStateLockLevel += 1;
                _debugBuilding = true;
                return true;
            });

            try {
                _scheduledFlushDirtyElements = true;
                if (callback != null) {
                    D.assert(_debugStateLocked);
                    Element debugPreviousBuildTarget = null;
                    D.assert(() => {
                        context._debugSetAllowIgnoredCallsToMarkNeedsBuild(true);
                        debugPreviousBuildTarget = _debugCurrentBuildTarget;
                        _debugCurrentBuildTarget = context;
                        return true;
                    }); 
                    _dirtyElementsNeedsResorting = false;
                    try {
                        callback();
                    }
                    finally {
                        D.assert(() => {
                            context._debugSetAllowIgnoredCallsToMarkNeedsBuild(false);
                            D.assert(_debugCurrentBuildTarget == context);
                            _debugCurrentBuildTarget = debugPreviousBuildTarget;
                            _debugElementWasRebuilt(context);
                            return true;
                        });
                    }
                }

                _dirtyElements.Sort(Element._sort);
                _dirtyElementsNeedsResorting = false;
                int dirtyCount = _dirtyElements.Count;
                int index = 0;
                while (index < dirtyCount) {
                    D.assert(_dirtyElements[index] != null);
                    D.assert(_dirtyElements[index]._inDirtyList);
                    D.assert(!_dirtyElements[index]._active ||
                             _dirtyElements[index]._debugIsInScope(context));

                    try {
                        _dirtyElements[index].rebuild();
                    }
                    catch (Exception ex) {
                        IEnumerable<DiagnosticsNode> infoCollector() {
                            yield return new DiagnosticsDebugCreator(new DebugCreator(_dirtyElements[index]));
                            yield return _dirtyElements[index].describeElement($"The element being rebuilt at the time was index {index} of {dirtyCount}");
                        }
                        
                        WidgetsD._debugReportException(
                            new ErrorDescription("while rebuilding dirty elements"), 
                            ex,
                            informationCollector: infoCollector
                        );
                    }

                    index++;
                    if (dirtyCount < _dirtyElements.Count || _dirtyElementsNeedsResorting.Value) {
                        _dirtyElements.Sort(Element._sort);
                        _dirtyElementsNeedsResorting = false;
                        dirtyCount = _dirtyElements.Count;
                        while (index > 0 && _dirtyElements[index - 1].dirty) {
                            index -= 1;
                        }
                    }
                }

                D.assert(() => {
                    if (_dirtyElements.Any(element => element._active && element.dirty)) {
                        throw new UIWidgetsError(
                           new List<DiagnosticsNode>() {
                               new ErrorSummary("buildScope missed some dirty elements."),
                               new ErrorHint("This probably indicates that the dirty list should have been resorted but was not."),
                               Element.describeElements("The list of dirty elements at the end of the buildScope call was", _dirtyElements),

                           });
                    }

                    return true;
                });
            }
            finally {
                foreach (Element element in _dirtyElements) {
                    D.assert(element._inDirtyList);
                    element._inDirtyList = false;
                }

                _dirtyElements.Clear();
                _scheduledFlushDirtyElements = false;
                _dirtyElementsNeedsResorting = null;

                D.assert(_debugBuilding);
                D.assert(() => {
                    _debugBuilding = false;
                    _debugStateLockLevel -= 1;
                    if (WidgetsD.debugPrintBuildScope) {
                        Debug.Log("buildScope finished");
                    }

                    return true;
                });
            }

            D.assert(_debugStateLockLevel >= 0);
        }

        Dictionary<Element, HashSet<GlobalKey>> _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans;

        internal void _debugTrackElementThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans(Element node, GlobalKey key) {
            _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans =
                _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans ??
                new Dictionary<Element, HashSet<GlobalKey>>();

            var keys = _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans
                .putIfAbsent(node, () => new HashSet<GlobalKey>());
            keys.Add(key);
        }

        internal void _debugElementWasRebuilt(Element node) {
            _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans?.Remove(node);
        }

        public void finalizeTree() {
            try {
                lockState(() => { _inactiveElements._unmountAll(); });

                D.assert(() => {
                    try {
                        GlobalKey._debugVerifyGlobalKeyReservation();
                        GlobalKey._debugVerifyIllFatedPopulation();
                        if (_debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans != null &&
                            _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans.isNotEmpty()) {
                            var keys = new HashSet<GlobalKey>();
                            foreach (Element element in _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans
                                .Keys) {
                                if (element._debugLifecycleState != _ElementLifecycle.defunct) {
                                    keys.UnionWith(
                                        _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans[element]);
                                }
                            }

                            if (keys.isNotEmpty()) {
                                var keyStringCount = new Dictionary<string, int>();
                                foreach (string key in LinqUtils<string, GlobalKey>.SelectList(keys,(key => key.ToString()))) {
                                    if (keyStringCount.ContainsKey(key)) {
                                        keyStringCount[key] += 1;
                                    }
                                    else {
                                        keyStringCount[key] = 1;
                                    }
                                }

                                var keyLabels = new List<string>();
                                foreach (var entry in keyStringCount) {
                                    var key = entry.Key;
                                    var count = entry.Value;
                                    if (count == 1) {
                                        keyLabels.Add(key);
                                    }
                                    else {
                                        keyLabels.Add(
                                            $"{key} ({count} different affected keys had this toString representation)");
                                    }
                                }

                                var elements = _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans.Keys;
                                var elementStringCount = new Dictionary<string, int>();
                                foreach (string element in LinqUtils<string, Element>.SelectList(elements,(element => element.ToString()))) {
                                    if (elementStringCount.ContainsKey(element)) {
                                        elementStringCount[element] += 1;
                                    }
                                    else {
                                        elementStringCount[element] = 1;
                                    }
                                }

                                var elementLabels = new List<string>();
                                foreach (var entry in elementStringCount) {
                                    var element = entry.Key;
                                    var count = entry.Value;

                                    if (count == 1) {
                                        elementLabels.Add(element);
                                    }
                                    else {
                                        elementLabels.Add(
                                            $"{element} ({count} different affected elements had this toString representation)");
                                    }
                                }

                                D.assert(keyLabels.isNotEmpty());

                                string the = keys.Count == 1 ? " the" : "";
                                string s = keys.Count == 1 ? "" : "s";
                                string were = keys.Count == 1 ? "was" : "were";
                                string their = keys.Count == 1 ? "its" : "their";
                                string respective = elementLabels.Count == 1 ? "" : " respective";
                                string those = keys.Count == 1 ? "that" : "those";
                                string s2 = elementLabels.Count == 1 ? "" : "s";
                                string those2 = elementLabels.Count == 1 ? "that" : "those";
                                string they = elementLabels.Count == 1 ? "it" : "they";
                                string think = elementLabels.Count == 1 ? "thinks" : "think";
                                string are = elementLabels.Count == 1 ? "is" : "are"; 

                                throw new UIWidgetsError(
                                    new List<DiagnosticsNode>() {
                                        new ErrorSummary("Duplicate GlobalKeys detected in widget tree."),
                                        
                                        new ErrorDescription(
                                            $"The following GlobalKey{s} {were} specified multiple times in the widget tree. This will lead to " +
                                        "parts of the widget tree being truncated unexpectedly, because the second time a key is seen, " +
                                        $"the previous instance is moved to the new location. The key{s} {were}:\n" +  "- " +
                                            string.Join("\n  ", keyLabels.ToArray()) + "\n" +
                                        $"This was determined by noticing that after {the} widget{s} with the above global key{s} {were} moved " +
                                        $"out of {their} {respective} previous parent{s2}, {those2} previous parent{s2} never updated during this frame, meaning " +
                                        $"that {they} either did not update at all or updated before the widget{s} {were} moved, in either case " +
                                        $"implying that {they} still {think} that {they} should have a child with {those} global key{s}.\n" + 
                                        $"The specific parent{s2} that did not update after having one or more children forcibly removed " +
                                        $"due to GlobalKey reparenting {are}:\n" +
                                        "- " + string.Join("\n  ", elementLabels.ToArray()) + "\n" +
                                        "\nA GlobalKey can only be specified on one widget at a time in the widget tree."
                                        )
                                    });
                            }
                        }
                    }
                    finally {
                        _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans?.Clear();
                    }

                    return true;
                });
            }
            catch (Exception ex) {
                WidgetsD._debugReportException(new ErrorSummary("while finalizing the widget tree"), ex);
            }
        }

        public void reassemble(Element root) {
            try {
                D.assert(root._parent == null);
                D.assert(root.owner == this);
                root.reassemble();
            }
            finally {
                
            }
        }
    }

    public abstract class Element : DiagnosticableTree, BuildContext {
        protected Element(Widget widget) {
            D.assert(widget != null);
            _widget = widget;
        }

        internal Element _parent;

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj);
        }

        static int _nextHashCode = 1;
        readonly int _cachedHash = _nextHashCode = (_nextHashCode + 1) % 0xffffff;

        public override int GetHashCode() {
            return _cachedHash;
        }

        internal object _slot;

        public object slot {
            get { return _slot; }
        }

        internal int _depth;

        public int depth {
            get { return _depth; }
        }

        internal static int _sort(Element a, Element b) {
            if (a.depth < b.depth) {
                return -1;
            }

            if (b.depth < a.depth) {
                return 1;
            }

            if (b.dirty && !a.dirty) {
                return -1;
            }

            if (a.dirty && !b.dirty) {
                return 1;
            }

            return 0;
        }

        internal static int _debugConcreteSubtype(Element element) {
            return element is StatefulElement ? 1 :
                element is StatelessElement ? 2 :
                0;
        }

        internal Widget _widget;
        
        public Widget widget {
            get { return _widget; }
        }
        
        internal BuildOwner _owner;

        public BuildOwner owner {
            get { return _owner; }
        }

        public bool _active = false;

        public virtual void reassemble() {
            markNeedsBuild();
            visitChildren((Element child) => {
                child.reassemble();
            });
        }

        public virtual bool debugDoingBuild { get; }

        internal bool _debugIsInScope(Element target) {
            Element current = this;
            while (current != null) {
                if (target == current) {
                    return true;
                }

                current = current._parent;
            }

            return false;
        }

        public virtual RenderObject renderObject {
            get {
                RenderObject result = null;
                ElementVisitor visit = null;
                visit = (element) => {
                    D.assert(result == null);
                    if (element is RenderObjectElement) {
                        result = element.renderObject;
                    }
                    else {
                        element.visitChildren(visit);
                    }
                };
                visit(this);
                return result;
            }
        }

        public List<DiagnosticsNode> describeMissingAncestor(Type expectedAncestorType) {
            List<DiagnosticsNode> information = new List<DiagnosticsNode>();
            List<Element> ancestors = new List<Element>();
            visitAncestorElements((Element element) => {
                ancestors.Add(element);
                return true;
            });

            information.Add(new DiagnosticsProperty<Element>(
                $"The specific widget that could not find a {expectedAncestorType} ancestor was",
                this,
                style: DiagnosticsTreeStyle.errorProperty
            ));

            if (ancestors.isNotEmpty()) {
                information.Add(describeElements("The ancestors of this widget were", ancestors));
            }
            else {
                information.Add(new ErrorDescription(
                    "This widget is the root of the tree, so it has no " +
                    $"ancestors, let alone a \"{expectedAncestorType}\" ancestor."
                ));
            }

            return information;
        }

        public static DiagnosticsNode describeElements(string name, IEnumerable<Element> elements) {
            return new DiagnosticsBlock(
                name: name,
                children: LinqUtils<DiagnosticsNode, Element>.SelectList(elements,((Element element) => new DiagnosticsProperty<Element>("", element))),
                allowTruncate: true
            );
        }

        public DiagnosticsNode describeElement(string name,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.errorProperty) {
            return new DiagnosticsProperty<Element>(name, this, style: style);
        }

        public DiagnosticsNode describeWidget(string name,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.errorProperty) {
            return new DiagnosticsProperty<Element>(name, this, style: style);
        }

        public DiagnosticsNode describeOwnershipChain(string name) {
            return new StringProperty(name, debugGetCreatorChain(10));
        }

        internal _ElementLifecycle _debugLifecycleState = _ElementLifecycle.initial;

        public virtual void visitChildren(ElementVisitor visitor) {
        }

        public virtual void debugVisitOnstageChildren(ElementVisitor visitor) {
            visitChildren(visitor);
        }

        public void visitChildElements(ElementVisitor visitor) {
            D.assert(() => {
                if (owner == null || !owner._debugStateLocked) {
                    return true;
                }

                throw new UIWidgetsError(
                    new List<DiagnosticsNode>() {
                        new ErrorSummary("visitChildElements() called during build."),
                        new ErrorDescription(
                            "The BuildContext.visitChildElements() method can't be called during " +
                        "build because the child list is still being updated at that point, " + 
                        "so the children might not be constructed yet, or might be old children " +
                        "that are going to be replaced."
                        ),
                    }
                );
            });

            visitChildren(visitor);
        }

        protected virtual Element updateChild(Element child, Widget newWidget, object newSlot) {
            if (newWidget == null) {
                if (child != null) {
                    deactivateChild(child);
                }
                return null;
            }

            Element newChild;
            if (child != null) {
                bool hasSameSuperclass = true;

                D.assert(() => {
                    int oldElementClass = _debugConcreteSubtype(child);
                    int newWidgetClass = Widget._debugConcreteSubtype(newWidget);
                    hasSameSuperclass = oldElementClass == newWidgetClass;
                    return true;
                });
                if (hasSameSuperclass && Equals(child.widget,newWidget)) {
                    if (!Equals(child.slot, newSlot)) {
                        updateSlotForChild(child, newSlot);
                    }

                    newChild = child;
                }
                else if (hasSameSuperclass && Widget.canUpdate(child.widget, newWidget)) {
                    if (!Equals(child.slot, newSlot)) {
                        updateSlotForChild(child, newSlot);
                    }

                    child.update(newWidget);
                    D.assert(Equals(child.widget, newWidget));
                    D.assert(() => {
                        child.owner._debugElementWasRebuilt(child);
                        return true;
                    });
                    newChild = child;
                }
                else {
                    deactivateChild(child);
                    D.assert(child._parent == null);
                    newChild = inflateWidget(newWidget, newSlot);
                }
            }
            else {
                newChild = inflateWidget(newWidget, newSlot);
            }

            D.assert(() => {
                if (child != null)
                    _debugRemoveGlobalKeyReservation(child);
                Key key = newWidget?.key;
                if (key is GlobalKey gKey) {
                    gKey._debugReserveFor(this, newChild);
                }

                return true;
            });

            return newChild;
        }

        public virtual void mount(Element parent, object newSlot) {
            D.assert(_debugLifecycleState == _ElementLifecycle.initial);
            D.assert(widget != null);
            D.assert(_parent == null);
            D.assert(parent == null || parent._debugLifecycleState == _ElementLifecycle.active);
            D.assert(slot == null);
            D.assert(depth == 0);
            D.assert(!_active);
            _parent = parent;
            _slot = newSlot;
            _depth = _parent != null ? _parent.depth + 1 : 1;
            _active = true;
            if (parent != null) {
                _owner = parent.owner;
            }

            Key key = widget.key;
            if (key is GlobalKey gKey) {
                gKey._register(this);
            }

            _updateInheritance();
            D.assert(() => {
                _debugLifecycleState = _ElementLifecycle.active;
                return true;
            });
        }

        void _debugRemoveGlobalKeyReservation(Element child) {
            GlobalKey._debugRemoveReservationFor(this, child);
        }

        public virtual void update(Widget newWidget) {
            D.assert(_debugLifecycleState == _ElementLifecycle.active
                     && widget != null
                     && newWidget != null
                     && newWidget != widget
                     && depth != 0
                     && _active
                     && Widget.canUpdate(widget, newWidget));

            D.assert(() => {
                _debugForgottenChildrenWithGlobalKey.Each(_debugRemoveGlobalKeyReservation);
                _debugForgottenChildrenWithGlobalKey.Clear();
                return true;
            });
            _widget = newWidget;
        }

        protected void updateSlotForChild(Element child, object newSlot) {
            D.assert(_debugLifecycleState == _ElementLifecycle.active);
            D.assert(child != null);
            D.assert(child._parent == this);

            void visit(Element element) {
                element._updateSlot(newSlot);
                if (!(element is RenderObjectElement))
                    element.visitChildren(visit);
            }
            visit(child);
        }

        internal virtual void _updateSlot(object newSlot) {
            D.assert(_debugLifecycleState == _ElementLifecycle.active);
            D.assert(widget != null);
            D.assert(_parent != null);
            D.assert(_parent._debugLifecycleState == _ElementLifecycle.active);
            D.assert(depth != 0);

            _slot = newSlot;
        }

        void _updateDepth(int parentDepth) {
            int expectedDepth = parentDepth + 1;
            if (_depth < expectedDepth) {
                _depth = expectedDepth;
                visitChildren(child => { child._updateDepth(expectedDepth); });
            }
        }

        public virtual void detachRenderObject() {
            visitChildren(child => { child.detachRenderObject(); });
            _slot = null;
        }

        public virtual void attachRenderObject(object newSlot) {
            D.assert(_slot == null);
            visitChildren(child => { child.attachRenderObject(newSlot); });
            _slot = newSlot;
        }

        Element _retakeInactiveElement(GlobalKey key, Widget newWidget) {
            Element element = key._currentElement;
            if (element == null) {
                return null;
            }

            if (!Widget.canUpdate(element.widget, newWidget)) {
                return null;
            }

            D.assert(() => {
                if (WidgetsD.debugPrintGlobalKeyedWidgetLifecycle) {
                    Debug.LogFormat("Attempting to take {0} from {1} to put in {2}.",
                        element, element._parent == null ? "inactive elements list" : element._parent.ToString(), this);
                }

                return true;
            });

            Element parent = element._parent;
            if (parent != null) {
                D.assert(() => {
                    if (parent == this) {
                        throw new UIWidgetsError(
                            new List<DiagnosticsNode>()
                            {
                                new ErrorSummary("A GlobalKey was used multiple times inside one widget's child list."),
                                new DiagnosticsProperty<GlobalKey>("The offending GlobalKey was", key),
                                parent.describeElement("The parent of the widgets with that key was"),
                                element.describeElement("The first child to get instantiated with that key became"),
                                new DiagnosticsProperty<Widget>("The second child that was to be instantiated with that key was", widget, style: DiagnosticsTreeStyle.errorProperty),
                                new ErrorDescription("A GlobalKey can only be specified on one widget at a time in the widget tree."),
                                
                            }
                        );
                    }

                    parent.owner._debugTrackElementThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans(
                        parent,
                        key
                    );
                    return true;
                });
                parent.forgetChild(element);
                parent.deactivateChild(element);
            }

            D.assert(element._parent == null);
            owner._inactiveElements.remove(element);
            return element;
        }

        protected Element inflateWidget(Widget newWidget, object newSlot) {
            D.assert(newWidget != null);
            Key key = newWidget.key;

            Element newChild;
            if (key is GlobalKey) {
                newChild = _retakeInactiveElement((GlobalKey) key, newWidget);
                if (newChild != null) {
                    D.assert(newChild._parent == null);
                    D.assert(() => {
                        _debugCheckForCycles(newChild);
                        return true;
                    });
                    newChild._activateWithParent(this, newSlot);
                    Element updatedChild = updateChild(newChild, newWidget, newSlot);
                    D.assert(newChild == updatedChild);
                    return updatedChild;
                }
            }

            newChild = newWidget.createElement();
            D.assert(() => {
                _debugCheckForCycles(newChild);
                return true;
            });
            newChild.mount(this, newSlot);
            D.assert(newChild._debugLifecycleState == _ElementLifecycle.active);
            return newChild;
        }

        void _debugCheckForCycles(Element newChild) {
            D.assert(newChild._parent == null);
            D.assert(() => {
                Element node = this;
                while (node._parent != null) {
                    node = node._parent;
                }

                D.assert(node != newChild);
                return true;
            });
        }

        protected void deactivateChild(Element child) {
            D.assert(child != null);
            D.assert(child._parent == this);
            child._parent = null;
            child.detachRenderObject();
            owner._inactiveElements.add(child);
            D.assert(() => {
                if (WidgetsD.debugPrintGlobalKeyedWidgetLifecycle) {
                    if (child.widget.key is GlobalKey) {
                        Debug.LogFormat("Deactivated {0} (keyed child of {1})", child, this);
                    }
                }

                return true;
            });
        }

        HashSet<Element> _debugForgottenChildrenWithGlobalKey = new HashSet<Element>();

        public virtual void forgetChild(Element child) {
            D.assert(() => {
                if (child.widget.key is GlobalKey)
                    _debugForgottenChildrenWithGlobalKey.Add(child);
                return true;
            });
        }

        void _activateWithParent(Element parent, object newSlot) {
            D.assert(_debugLifecycleState == _ElementLifecycle.inactive);
            _parent = parent;
            D.assert(() => {
                if (WidgetsD.debugPrintGlobalKeyedWidgetLifecycle) {
                    Debug.LogFormat("Reactivating {0} (now child of {1}).", this, _parent);
                }

                return true;
            });
            _updateDepth(_parent.depth);
            _activateRecursively(this);
            attachRenderObject(newSlot);
            D.assert(_debugLifecycleState == _ElementLifecycle.active);
        }

        static void _activateRecursively(Element element) {
            D.assert(element._debugLifecycleState == _ElementLifecycle.inactive);
            element.activate();
            D.assert(element._debugLifecycleState == _ElementLifecycle.active);
            element.visitChildren(_activateRecursively);
        }

        public virtual void activate() {
            D.assert(_debugLifecycleState == _ElementLifecycle.inactive);
            D.assert(widget != null);
            D.assert(owner != null);
            D.assert(depth != 0);
            D.assert(!_active);

            bool hadDependencies = (_dependencies != null && _dependencies.isNotEmpty()) ||
                                   _hadUnsatisfiedDependencies;
            _active = true;
            
            _dependencies?.Clear();
            _hadUnsatisfiedDependencies = false;
            _updateInheritance();
            D.assert(() => {
                _debugLifecycleState = _ElementLifecycle.active;
                return true;
            });
            if (_dirty) {
                owner.scheduleBuildFor(this);
            }

            if (hadDependencies) {
                didChangeDependencies();
            }
        }

        public virtual void deactivate() {
            D.assert(_debugLifecycleState == _ElementLifecycle.active);
            D.assert(_widget != null);
            D.assert(depth != 0);
            D.assert(_active);
            if (_dependencies != null && _dependencies.isNotEmpty()) {
                foreach (InheritedElement dependency in _dependencies) {
                    dependency._dependents.Remove(this);
                }
            }

            _inheritedWidgets = null;
            _active = false;
            D.assert(() => {
                _debugLifecycleState = _ElementLifecycle.inactive;
                return true;
            });
        }

        public virtual void debugDeactivated() {
            D.assert(_debugLifecycleState == _ElementLifecycle.inactive);
        }

        public virtual void unmount() {
            D.assert(_debugLifecycleState == _ElementLifecycle.inactive);
            D.assert(_widget != null);
            D.assert(depth != 0);
            D.assert(!_active);
            if (widget.key is GlobalKey) {
                GlobalKey key = (GlobalKey) widget.key;
                key._unregister(this);
            }

            D.assert(() => {
                _debugLifecycleState = _ElementLifecycle.defunct;
                return true;
            });
        }

        public RenderObject findRenderObject() {
            return renderObject;
        }

        public Size size {
            get {
                D.assert(() => {
                    if (_debugLifecycleState != _ElementLifecycle.active) {
                        throw new UIWidgetsError(
                            new List<DiagnosticsNode>() {
                                new ErrorSummary("Cannot get size of inactive element."),
                                new ErrorDescription(
                                    "In order for an element to have a valid size, the element must be " +
                                "active, which means it is part of the tree.\n" +
                                $"Instead, this element is in the {_debugLifecycleState} state."
                                ),
                                describeElement("The size getter was called for the following element"),
                            }
                            );
                    }

                    if (owner.debugBuilding) {
                        throw new UIWidgetsError(
                            new List<DiagnosticsNode>() {
                                new ErrorSummary("Cannot get size during build."),
                                new ErrorDescription(
                                    "The size of this render object has not yet been determined because " +
                                "the framework is still in the process of building widgets, which " + 
                                "means the render tree for this frame has not yet been determined. " +
                                "The size getter should only be called from paint callbacks or " + 
                                "interaction event handlers (e.g. gesture callbacks)."
                                ),
                                new ErrorSpacer(),
                                new ErrorHint(
                                    "If you need some sizing information during build to decide which " + 
                                "widgets to build, consider using a LayoutBuilder widget, which can " + 
                                "tell you the layout constraints at a given location in the tree."
                                ),
                                new ErrorSpacer(),
                                describeElement("The size getter was called for the following element"),
                            }
                            );
                    }

                    return true;
                });
                RenderObject renderObject = findRenderObject();
                D.assert(() => {
                    if (renderObject == null) {
                        throw new UIWidgetsError(
                           new List<DiagnosticsNode>() {
                               new ErrorSummary("Cannot get size without a render object."),
                               new ErrorHint(
                                   "In order for an element to have a valid size, the element must have " +
                               "an associated render object. This element does not have an associated " + 
                               "render object, which typically means that the size getter was called " + 
                               "too early in the pipeline (e.g., during the build phase) before the " +
                               "framework has created the render tree."
                               ),
                               describeElement("The size getter was called for the following element"),
                           }
                            );
                    }

                    if (renderObject is RenderSliver) {
                        throw new UIWidgetsError(
                           new List<DiagnosticsNode>() {
                               new ErrorSummary("Cannot get size from a RenderSliver."),
                               new ErrorHint(
                                   "The render object associated with this element is a " + 
                               $"{renderObject.GetType()}, which is a subtype of RenderSliver. " + 
                               "Slivers do not have a size per se. They have a more elaborate " +
                               "geometry description, which can be accessed by calling " + 
                               "findRenderObject and then using the \"geometry\" getter on the " +
                               "resulting object."
                               ),
                               describeElement("The size getter was called for the following element"),
                               renderObject.describeForError("The associated render sliver was"),
                           }
                            );
                    }

                    if (!(renderObject is RenderBox)) {
                        throw new UIWidgetsError(
                            new List<DiagnosticsNode>() {
                                new ErrorSummary("Cannot get size from a render object that is not a RenderBox."),
                                new ErrorHint(
                                    "Instead of being a subtype of RenderBox, the render object associated " +
                                $"with this element is a {renderObject.GetType()}. If this type of " +
                                "render object does have a size, consider calling findRenderObject " +
                                "and extracting its size manually."
                                ),
                                describeElement("The size getter was called for the following element"),
                                renderObject.describeForError("The associated render object was"),
                            }
                            );
                    }

                    RenderBox box = (RenderBox) renderObject;
                    if (!box.hasSize) {
                        throw new UIWidgetsError(
                        new List<DiagnosticsNode>() {
                            new ErrorSummary("Cannot get size from a render object that has not been through layout."),
                            new ErrorHint(
                                "The size of this render object has not yet been determined because " +
                            "this render object has not yet been through layout, which typically " + 
                            "means that the size getter was called too early in the pipeline " + 
                            "(e.g., during the build phase) before the framework has determined " +
                            "the size and position of the render objects during layout."
                            ),
                            describeElement("The size getter was called for the following element"),
                            box.describeForError("The render object from which the size was to be obtained was"),
                        }
                            );
                    }

                    if (box.debugNeedsLayout) {
                        throw new UIWidgetsError(
                            new List<DiagnosticsNode>() {
                                new ErrorSummary("Cannot get size from a render object that has been marked dirty for layout."),
                                new ErrorHint(
                                    "The size of this render object is ambiguous because this render object has " + 
                                "been modified since it was last laid out, which typically means that the size " +
                                "getter was called too early in the pipeline (e.g., during the build phase) " + 
                                "before the framework has determined the size and position of the render " +
                                "objects during layout."
                                ),
                                describeElement("The size getter was called for the following element"),
                                box.describeForError("The render object from which the size was to be obtained was"),
                                new ErrorHint(
                                    "Consider using debugPrintMarkNeedsLayoutStacks to determine why the render " +
                                "object in question is dirty, if you did not expect this."
                                ),
                            }
                            
                            );
                    }

                    return true;
                });
                if (renderObject is RenderBox) {
                    return ((RenderBox) renderObject).size;
                }

                return null;
            }
        }

        internal Dictionary<Type, InheritedElement> _inheritedWidgets;
        internal HashSet<InheritedElement> _dependencies;
        bool _hadUnsatisfiedDependencies = false;

        bool _debugCheckStateIsActiveForAncestorLookup() {
            D.assert(() => {
                if (_debugLifecycleState != _ElementLifecycle.active) {
                    throw new UIWidgetsError(
                        new List<DiagnosticsNode>() {
                            new ErrorSummary("Looking up a deactivated widget's ancestor is unsafe."),
                            new ErrorDescription(
                                "At this point the state of the widget's element tree is no longer " +
                            "stable."
                            ),
                            new ErrorHint(
                                "To safely refer to a widget's ancestor in its dispose() method, " +  
                            "save a reference to the ancestor by calling dependOnInheritedWidgetOfExactType() " +
                            "in the widget's didChangeDependencies() method."
                            ),
                        }
                        );
                }

                return true;
            });
            return true;
        }

        public virtual InheritedWidget inheritFromElement(InheritedElement ancestor, object aspect = null) {
            return dependOnInheritedElement(ancestor, aspect: aspect);
        }

        public virtual InheritedWidget dependOnInheritedElement(InheritedElement ancestor, object aspect = null) {
            D.assert(ancestor != null);
            _dependencies = _dependencies ?? new HashSet<InheritedElement>();
            _dependencies.Add(ancestor);
            ancestor.updateDependencies(this, aspect);
            return ancestor.widget;
        }

        public virtual InheritedWidget inheritFromWidgetOfExactType(Type targetType, object aspect = null) {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            InheritedElement ancestor = null;
            if (_inheritedWidgets != null) {
                _inheritedWidgets.TryGetValue(targetType, out ancestor);
            }

            if (ancestor != null) {
                return inheritFromElement(ancestor, aspect: aspect);
            }

            _hadUnsatisfiedDependencies = true;
            return null;
        }

        public T dependOnInheritedWidgetOfExactType<T>(object aspect = null) where T : InheritedWidget {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            InheritedElement ancestor = _inheritedWidgets == null ? null : _inheritedWidgets.getOrDefault(typeof(T));
            
            if (ancestor != null) {
                D.assert(ancestor is InheritedElement);
                return dependOnInheritedElement(ancestor, aspect: aspect) as T;
            }

            _hadUnsatisfiedDependencies = true;
            return null;
        }

        public virtual InheritedElement ancestorInheritedElementForWidgetOfExactType(Type targetType) {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            InheritedElement ancestor = null;
            if (_inheritedWidgets != null) {
                _inheritedWidgets.TryGetValue(targetType, out ancestor);
            }

            return ancestor;
        }

        public InheritedElement getElementForInheritedWidgetOfExactType<T>() where T : InheritedWidget {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            InheritedElement ancestor = _inheritedWidgets == null ? null : _inheritedWidgets[typeof(T)];
            return ancestor;
        }

        internal virtual void _updateInheritance() {
            D.assert(_active);
            _inheritedWidgets = _parent?._inheritedWidgets;
        }

        public virtual Widget ancestorWidgetOfExactType(Type targetType) {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            Element ancestor = _parent;
            while (ancestor != null && ancestor.widget.GetType() != targetType) {
                ancestor = ancestor._parent;
            }

            return ancestor?.widget;
        }

        public T findAncestorWidgetOfExactType<T>() where T : Widget {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            Element ancestor = _parent;
            while (ancestor != null && ancestor.widget.GetType() != typeof(T)) {
                ancestor = ancestor._parent;
            }

            return ancestor?.widget as T;
        }

        public virtual State ancestorStateOfType(TypeMatcher matcher) {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            Element ancestor = _parent;
            while (ancestor != null) {
                var element = ancestor as StatefulElement;
                if (element != null && matcher.check(element.state)) {
                    break;
                }

                ancestor = ancestor._parent;
            }

            var statefulAncestor = ancestor as StatefulElement;
            return statefulAncestor?.state;
        }

        public T findAncestorStateOfType<T>() where T : State{
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            Element ancestor = _parent;
            while (ancestor != null) {
                if (ancestor is StatefulElement stateAncestor && stateAncestor.state is T) {
                    break;
                }

                ancestor = ancestor._parent;
            }

            StatefulElement statefulAncestor = ancestor as StatefulElement;
            return statefulAncestor?.state as T;
        }

        public virtual State rootAncestorStateOfType(TypeMatcher matcher) {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            Element ancestor = _parent;
            StatefulElement statefulAncestor = null;
            while (ancestor != null) {
                var element = ancestor as StatefulElement;
                if (element != null && matcher.check(element.state)) {
                    statefulAncestor = element;
                }

                ancestor = ancestor._parent;
            }

            return statefulAncestor?.state;
        }

        public T findRootAncestorStateOfType<T>() where T : State {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            Element ancestor = _parent;
            StatefulElement statefulAncestor = null;
            while (ancestor != null) {
                if (ancestor is StatefulElement stateAncestor && stateAncestor.state is T) {
                    statefulAncestor = stateAncestor;
                }

                ancestor = ancestor._parent;
            }

            return statefulAncestor?.state as T;
        }

        public virtual RenderObject ancestorRenderObjectOfType(TypeMatcher matcher) {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            Element ancestor = _parent;
            while (ancestor != null) {
                var element = ancestor as RenderObjectElement;
                if (element != null && matcher.check(ancestor.renderObject)) {
                    return element.renderObject;
                }

                ancestor = ancestor._parent;
            }

            return null;
        }

        public T findAncestorRenderObjectOfType<T>() where T : RenderObject {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            Element ancestor = _parent;
            while (ancestor != null) {
                if (ancestor is RenderObjectElement && ancestor.renderObject is T) {
                    return ancestor.renderObject as T;
                }

                ancestor = ancestor._parent;
            }

            return null;
        }

        public virtual void visitAncestorElements(ElementVisitorBool visitor) {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());

            Element ancestor = _parent;
            while (ancestor != null && visitor(ancestor)) {
                ancestor = ancestor._parent;
            }
        }

        public virtual void didChangeDependencies() {
            D.assert(_active);
            D.assert(_debugCheckOwnerBuildTargetExists("didChangeDependencies"));

            markNeedsBuild();
        }

        internal bool _debugCheckOwnerBuildTargetExists(string methodName) {
            D.assert(() => {
                if (owner._debugCurrentBuildTarget == null) {
                    throw new UIWidgetsError(
                       new List<DiagnosticsNode>() {
                           new ErrorSummary(
                               $"{methodName} for {widget.GetType()} was called at an " +
                           "inappropriate time."
                           ),
                           new ErrorDescription("It may only be called while the widgets are being built."),
                           new ErrorHint(
                               $"A possible cause of this error is when {methodName} is called during " +
                           "one of:\n" + 
                           " * network I/O event\n" + 
                           " * file I/O event\n" +
                           " * timer\n" +
                           " * microtask (caused by Future.then, async/await, scheduleMicrotask)"
                           ),
                       }
                    );
                }

                return true;
            });

            return true;
        }

        public string debugGetCreatorChain(int limit) {
            var chain = new List<string>();
            Element node = this;
            while (chain.Count < limit && node != null) {
                chain.Add(node.toStringShort());
                node = node._parent;
            }

            if (node != null) {
                chain.Add("\u22EF");
            }

            return string.Join(" \u2190 ", chain.ToArray());
        }

        public List<Element> debugGetDiagnosticChain() {
            var chain = new List<Element>() {this};
            Element node = _parent;
            while (node != null) {
                chain.Add(node);
                node = node._parent;
            }

            return chain;
        }

        public override string toStringShort() {
            return widget != null ? widget.toStringShort() : GetType().ToString();
        }

        public DiagnosticsNode toDiagnosticsNode(string name = null, DiagnosticsTreeStyle? style = null) {
            return new _ElementDiagnosticableTreeNode(
                name: name,
                value: this,
                style: style ?? DiagnosticsTreeStyle.dense
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.dense;
            properties.add(new ObjectFlagProperty<int>("depth", depth, ifNull: "no depth"));
            properties.add(new ObjectFlagProperty<Widget>("widget", widget, ifNull: "no widget"));
            if (widget != null) {
                properties.add(new DiagnosticsProperty<Key>("key",
                    widget.key, showName: false, defaultValue: foundation_.kNullDefaultValue,
                    level: DiagnosticLevel.hidden));
                widget.debugFillProperties(properties);
            }

            properties.add(new FlagProperty("dirty", value: dirty, ifTrue: "dirty"));
            if (_dependencies != null && _dependencies.isNotEmpty()) {
                List<DiagnosticsNode> diagnosticsDependencies = LinqUtils<DiagnosticsNode, InheritedElement>
                    .SelectList(_dependencies, ((InheritedElement element) =>
                        element.widget.toDiagnosticsNode(style: DiagnosticsTreeStyle.sparse)));
                properties.add(new DiagnosticsProperty<List<DiagnosticsNode>>("dependencies", diagnosticsDependencies));
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            var children = new List<DiagnosticsNode>();
            visitChildren(child => {
                if (child != null) {
                    children.Add(child.toDiagnosticsNode());
                }
                else {
                    children.Add(DiagnosticsNode.message("<null child>"));
                }
            });
            return children;
        }

        internal bool _dirty = true;

        public bool dirty {
            get { return _dirty; }
        }

        internal bool _inDirtyList = false;

        bool _debugBuiltOnce = false;

        bool _debugAllowIgnoredCallsToMarkNeedsBuild = false;

        internal bool _debugSetAllowIgnoredCallsToMarkNeedsBuild(bool value) {
            D.assert(_debugAllowIgnoredCallsToMarkNeedsBuild == !value);
            _debugAllowIgnoredCallsToMarkNeedsBuild = value;
            return true;
        }


        public void markNeedsBuild() {
            D.assert(_debugLifecycleState != _ElementLifecycle.defunct);
            if (!_active) {
                return;
            }

            D.assert(owner != null);
            D.assert(_debugLifecycleState == _ElementLifecycle.active);
            D.assert(() => {
                if (owner.debugBuilding) {
                    D.assert(owner._debugCurrentBuildTarget != null);
                    D.assert(owner._debugStateLocked);
                    if (_debugIsInScope(owner._debugCurrentBuildTarget)) {
                        return true;
                    }

                    if (!_debugAllowIgnoredCallsToMarkNeedsBuild) {
                        List<DiagnosticsNode> information = new List<DiagnosticsNode>() {
                            new ErrorSummary("setState() or markNeedsBuild() called during build.\n"),
                            new ErrorDescription("This " + widget.GetType() +
                                                 " widget cannot be marked as needing to build because the framework " +
                                                 "is already in the process of building widgets. A widget can be marked as " +
                                                 "needing to be built during the build phase only if one of its ancestors " +
                                                 "is currently building. This exception is allowed because the framework " +
                                                 "builds parent widgets before children, which means a dirty descendant " +
                                                 "will always be built. Otherwise, the framework might not visit this " +
                                                 "widget during this build phase.\n"),
                            describeElement("The widget on which setState() or markNeedsBuild() was called was")
                        };
                        if (owner._debugCurrentBuildTarget != null) {
                            information.Add(owner._debugCurrentBuildTarget.describeWidget("The widget which was currently being built when the offending call was made was"));
                        }
                        throw new UIWidgetsError(
                            information
                        );
                    }

                    D.assert(dirty);
                }
                else if (owner._debugStateLocked) {
                    D.assert(!_debugAllowIgnoredCallsToMarkNeedsBuild);
                    throw new UIWidgetsError(
                       new List<DiagnosticsNode>() {
                           new ErrorSummary("setState() or markNeedsBuild() called when widget tree was locked."),
                           new ErrorDescription(
                               $"This {widget.GetType()} widget cannot be marked as needing to build " +
                           "because the framework is locked."
                           ),
                           describeElement("The widget on which setState() or markNeedsBuild() was called was"),
                       }
                    );
                }

                return true;
            });

            if (dirty) {
                return;
            }

            _dirty = true;
            owner.scheduleBuildFor(this);
        }

        public void rebuild() {
            D.assert(_debugLifecycleState != _ElementLifecycle.initial);
            if (!_active || !_dirty) {
                return;
            }

            D.assert(() => {
                
                if (WidgetsD.debugPrintRebuildDirtyWidgets) {
                    if (!_debugBuiltOnce) {
                        Debug.Log("Building " + this);
                        _debugBuiltOnce = true;
                    }
                    else {
                        Debug.Log("Rebuilding " + this);
                    }
                }

                return true;
            });
            D.assert(_debugLifecycleState == _ElementLifecycle.active);
            D.assert(owner._debugStateLocked);
            Element debugPreviousBuildTarget = null;
            D.assert(() => {
                debugPreviousBuildTarget = owner._debugCurrentBuildTarget;
                owner._debugCurrentBuildTarget = this;
                return true;
            });
            performRebuild();

            D.assert(() => {
                D.assert(owner._debugCurrentBuildTarget == this);
                owner._debugCurrentBuildTarget = debugPreviousBuildTarget;
                return true;
            });
            D.assert(!_dirty);
        }

        protected abstract void performRebuild();
    }

    internal class _ElementDiagnosticableTreeNode : DiagnosticableTreeNode {
        internal _ElementDiagnosticableTreeNode(
            Element value,
            DiagnosticsTreeStyle style,
            string name = null,
            bool stateful = false
        ) : base(
            name: name,
            value: value,
            style: style
        ) {
            this.stateful = stateful;
        }

        readonly bool stateful;
        
        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            Dictionary<string, object> json = base.toJsonMap(Delegate);
            Element element = value as Element;
            json["widgetRuntimeType"] = element.widget?.GetType()?.ToString();
            json["stateful"] = stateful;
            return json;
        }
    }
    
    public delegate Widget ErrorWidgetBuilder(UIWidgetsErrorDetails details);

    public class ErrorWidget : LeafRenderObjectWidget {
        public ErrorWidget(Exception exception, string message = null) : base(key: new UniqueKey()) {
            this.message = message ?? _stringify(exception);
            _uiWidgetsError = exception is UIWidgetsError uiWidgetsError ? uiWidgetsError : null;
        }

        public static ErrorWidget withDetails(string message = "", UIWidgetsError error = null
        ) {
            return new ErrorWidget(error, message);
        }

        public static ErrorWidgetBuilder builder = _defaultErrorWidgetBuilder;

        static Widget _defaultErrorWidgetBuilder(UIWidgetsErrorDetails details) {
            string message = "";
            D.assert(() => {
                message = _stringify(details.exception);
                return true;
            });
            object exception = details.exception;
            return withDetails(message: message,
                error: exception is UIWidgetsError uiWidgetsError ? uiWidgetsError : null);
        }

        static string _stringify(Exception exception) {
            try {
                return exception.ToString();
            }
            catch {
            }

            return "Error";
        }

        public readonly string message;
        readonly UIWidgetsError _uiWidgetsError;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderErrorBox(message);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            if (_uiWidgetsError == null) {
                properties.add(new StringProperty("message", message, quoted: false));
            }
            else {
                properties.add(DiagnosticsNode.message(message: _uiWidgetsError.ToString(),
                    style: DiagnosticsTreeStyle.whitespace));
            }
        }
    }

    public delegate Widget WidgetBuilder(BuildContext context);

    public delegate Widget IndexedWidgetBuilder(BuildContext context, int index);

    public delegate Widget TransitionBuilder(BuildContext context, Widget child);

    public delegate Widget ControlsWidgetBuilder(BuildContext context, VoidCallback onStepContinue = null,
        VoidCallback onStepCancel = null);

    public abstract class ComponentElement : Element {
        protected ComponentElement(Widget widget) : base(widget) {
        }

        Element _child;

        bool _debugDoingBuild = false;

        public override bool debugDoingBuild {
            get => _debugDoingBuild;
        }


        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            D.assert(_child == null);
            D.assert(_active);
            _firstBuild();
            D.assert(_child != null);
        }

        protected virtual void _firstBuild() {
            rebuild();
        }

        protected override void performRebuild() {
            D.assert(_debugSetAllowIgnoredCallsToMarkNeedsBuild(true));

            Widget built;
            try {
                D.assert(() => {
                    _debugDoingBuild = true;
                    return true;
                });
                built = build();
                D.assert(() => {
                    _debugDoingBuild = false;
                    return true;
                });
                WidgetsD.debugWidgetBuilderValue(widget, built);
            }
            catch (Exception e) {
                _debugDoingBuild = false;

                IEnumerable<DiagnosticsNode> informationCollector() {
                    yield return new DiagnosticsDebugCreator(new DebugCreator(this));
                }
                built = ErrorWidget.builder(WidgetsD._debugReportException("building " + this, e, informationCollector));
            }
            finally {
                _dirty = false;
                D.assert(_debugSetAllowIgnoredCallsToMarkNeedsBuild(false));
            }

            try {
                _child = updateChild(_child, built, slot);
                D.assert(_child != null);
            }
            catch (Exception e) {
                IEnumerable<DiagnosticsNode> informationCollector() {
                    yield return new DiagnosticsDebugCreator(new DebugCreator(this));
                }
                built = ErrorWidget.builder(WidgetsD._debugReportException("building " + this, e, informationCollector));
                _child = updateChild(null, built, slot);
            }
        }

        protected abstract Widget build();

        public override void visitChildren(ElementVisitor visitor) {
            if (_child != null) {
                visitor(_child);
            }
        }

        public override void forgetChild(Element child) {
            D.assert(child == _child);
            _child = null;
            base.forgetChild(child);
        }
    }

    public class StatelessElement : ComponentElement {
        public StatelessElement(StatelessWidget widget) : base(widget) {
        }

        public new StatelessWidget widget {
            get { return (StatelessWidget) base.widget; }
        }

        protected override Widget build() {
            return widget.build(this);
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(widget == newWidget);
            _dirty = true;
            rebuild();
        }
    }

    public class StatefulElement : ComponentElement {
        public StatefulElement(StatefulWidget widget) : base(widget) {
            _state = widget.createState();
            D.assert(() => {
                if (!_state._debugTypesAreRight(widget)) {
                    throw new UIWidgetsError(
                        new List<DiagnosticsNode>() {
                            new ErrorSummary("StatefulWidget.createState must return a subtype of State<${widget.runtimeType}>"),
                            new ErrorDescription(
                                $"The createState function for {widget.GetType()} returned a state " + 
                            $"of type {_state.GetType()}, which is not a subtype of " + 
                            $"State<${widget.GetType()}>, violating the contract for createState."
                            ),
                        });
                }

                return true;
            });
            D.assert(_state._element == null);
            _state._element = this;
            D.assert(_state._widget == null, () => $"The createState function for {widget} returned an old or invalid state " +
                                                   $"instance: {_state._widget}, which is not null, violating the contract " +
                                                   "for createState.");
            _state._widget = widget;
            D.assert(_state._debugLifecycleState == _StateLifecycle.created);
        }

        protected override Widget build() {
            return _state.build(this);
        }

        public new StatefulWidget widget {
            get { return (StatefulWidget) base.widget; }
        }

        public State state {
            get { return _state; }
        }

        State _state;

        public override void reassemble() {
            state.reassemble();
            base.reassemble();
        }

        protected override void _firstBuild() {
            D.assert(_state._debugLifecycleState == _StateLifecycle.created);

            try {
                _debugSetAllowIgnoredCallsToMarkNeedsBuild(true);
                _state.initState();
            }
            finally {
                _debugSetAllowIgnoredCallsToMarkNeedsBuild(false);
            }

            D.assert(() => {
                _state._debugLifecycleState = _StateLifecycle.initialized;
                return true;
            });
            _state.didChangeDependencies();
            D.assert(() => {
                _state._debugLifecycleState = _StateLifecycle.ready;
                return true;
            });

            base._firstBuild();
        }

        protected override void performRebuild() {
            if (_didChangeDependencies) {
                _state.didChangeDependencies();
                _didChangeDependencies = false;
            }

            base.performRebuild();
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(widget == newWidget);
            StatefulWidget oldWidget = _state._widget;
            _dirty = true;
            _state._widget = widget;
            try {
                _debugSetAllowIgnoredCallsToMarkNeedsBuild(true);
                _state.didUpdateWidget(oldWidget);
            }
            finally {
                _debugSetAllowIgnoredCallsToMarkNeedsBuild(false);
            }

            rebuild();
        }

        public override void activate() {
            base.activate();
            D.assert(_active);
            markNeedsBuild();
        }

        public override void deactivate() {
            _state.deactivate();
            base.deactivate();
        }

        public override void unmount() {
            base.unmount();
            _state.dispose();
            D.assert(() => {
                if (_state._debugLifecycleState == _StateLifecycle.defunct) {
                    return true;
                }

                throw new UIWidgetsError(
                 new List<DiagnosticsNode>() {
                     new ErrorSummary($"{_state.GetType()}.dispose failed to call super.dispose."),
                     new ErrorDescription(
                         "dispose() implementations must always call their superclass dispose() method, to ensure " +
                     "that all the resources used by the widget are fully released."
                     ),
                 });
            });
            _state._element = null;
            _state = null;
        }

        public override InheritedWidget inheritFromElement(InheritedElement ancestor, object aspect = null) {
            return dependOnInheritedElement(ancestor, aspect);
        }

        public override InheritedWidget dependOnInheritedElement(InheritedElement ancestor, object aspect = null) {
            D.assert(ancestor != null);
            D.assert(() => {
                Type targetType = ancestor.widget.GetType();
                if (state._debugLifecycleState == _StateLifecycle.created) {
                    throw new UIWidgetsError(
                        new List<DiagnosticsNode>() {
                            new ErrorSummary($"dependOnInheritedWidgetOfExactType<{targetType}>() or dependOnInheritedElement() was called before {_state.GetType()}.initState() completed."),
                            new ErrorDescription(
                                "When an inherited widget changes, for example if the value of Theme.of() changes, " +
                            "its dependent widgets are rebuilt. If the dependent widget's reference to " + 
                            "the inherited widget is in a constructor or an initState() method, " +
                            "then the rebuilt dependent widget will not reflect the changes in the " +
                            "inherited widget."
                            ),
                            new ErrorHint(
                                "Typically references to inherited widgets should occur in widget build() methods. Alternatively, " +
                            "initialization based on inherited widgets can be placed in the didChangeDependencies method, which " +
                            "is called after initState and whenever the dependencies change thereafter."
                            ),
                        }
                        );
                }

                if (state._debugLifecycleState == _StateLifecycle.defunct) {
                    throw new UIWidgetsError(
                        new List<DiagnosticsNode>() {
                            new ErrorSummary($"dependOnInheritedWidgetOfExactType<{targetType}>() or dependOnInheritedElement() was called after dispose(): $this"),
                            new ErrorDescription(
                                "This error happens if you call dependOnInheritedWidgetOfExactType() on the " +
                            "BuildContext for a widget that no longer appears in the widget tree " + 
                            "(e.g., whose parent widget no longer includes the widget in its " +
                            "build). This error can occur when code calls " +
                            "dependOnInheritedWidgetOfExactType() from a timer or an animation callback."
                            ),
                            new ErrorHint(
                                "The preferred solution is to cancel the timer or stop listening to the " +
                            "animation in the dispose() callback. Another solution is to check the " +
                            "\"mounted\" property of this object before calling " +
                            "dependOnInheritedWidgetOfExactType() to ensure the object is still in the " +
                            "tree."
                            ),
                            new ErrorHint(
                                "This error might indicate a memory leak if " +
                            "dependOnInheritedWidgetOfExactType() is being called because another object " +
                            "is retaining a reference to this State object after it has been " +
                            "removed from the tree. To avoid memory leaks, consider breaking the " +
                            "reference to this object during dispose()."
                            ),
                        }
                    );
                }

                return true;
            });
            return base.dependOnInheritedElement(ancestor, aspect: aspect);
        }

        bool _didChangeDependencies = false;

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _didChangeDependencies = true;
        }

        public override DiagnosticsNode toDiagnosticsNode(string name = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.sparse) {
            return new _ElementDiagnosticableTreeNode(
                name: name,
                value: this,
                style: style,
                stateful: true
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<State>("state", state,
                defaultValue: foundation_.kNullDefaultValue));
        }
    }

    public abstract class ProxyElement : ComponentElement {
        protected ProxyElement(Widget widget) : base(widget) {
        }

        public new ProxyWidget widget {
            get { return (ProxyWidget) base.widget; }
        }

        protected override Widget build() {
            return widget.child;
        }

        public override void update(Widget newWidget) {
            ProxyWidget oldWidget = widget;
            D.assert(widget != null);
            D.assert(widget != newWidget);
            base.update(newWidget);
            D.assert(widget == newWidget);
            updated(oldWidget);
            _dirty = true;
            rebuild();
        }

        protected virtual void updated(ProxyWidget oldWidget) {
            notifyClients(oldWidget);
        }

        public abstract void notifyClients(ProxyWidget oldWidget);
    }

    public class ParentDataElement : ProxyElement {
        public ParentDataElement(ParentDataWidget widget) : base(widget) {
        }

        public new ParentDataWidget widget {
            get { return (ParentDataWidget) base.widget; }
        }

        void _applyParentData(ParentDataWidget widget) {
            ElementVisitor applyParentDataToChild = null;
            applyParentDataToChild = child => {
                if (child is RenderObjectElement) {
                    ((RenderObjectElement) child)._updateParentData(widget);
                }
                else {
                    D.assert(!(child is ParentDataElement));
                    child.visitChildren(applyParentDataToChild);
                }
            };
            visitChildren(applyParentDataToChild);
        }

        public void applyWidgetOutOfTurn(ParentDataWidget newWidget) {
            D.assert(newWidget != null);
            D.assert(newWidget.debugCanApplyOutOfTurn());
            D.assert(newWidget.child == widget.child);
            _applyParentData(newWidget);
        }

        public override void notifyClients(ProxyWidget oldWidget) {
            _applyParentData((ParentDataWidget) widget);
        }
    }

    public class ParentDataElement<T> : ParentDataElement where T : IParentData {
        public ParentDataElement(ParentDataWidget<T> widget) : base(widget)
        {
        }
        public new ParentDataWidget<T> widget {
            get { return (ParentDataWidget<T>) base.widget; }
        }
        void _applyParentData(ParentDataWidget<T> widget) {
            ElementVisitor applyParentDataToChild = null;
            applyParentDataToChild = child => {
                if (child is RenderObjectElement) {
                    ((RenderObjectElement) child)._updateParentData(widget);
                }
                else {
                    D.assert(!(child is ParentDataElement<IParentData>));
                    child.visitChildren(applyParentDataToChild);
                }
            };
            visitChildren(applyParentDataToChild);
        }

        public override void notifyClients(ProxyWidget oldWidget) {
            _applyParentData((ParentDataWidget<T>) widget);
        }

    }

    public class InheritedElement : ProxyElement {
        public InheritedElement(Widget widget) : base(widget) {
        }

        public new InheritedWidget widget {
            get { return (InheritedWidget) base.widget; }
        }

        internal readonly Dictionary<Element, object> _dependents = new Dictionary<Element, object>();

        internal override void _updateInheritance() {
            D.assert(_active);
            Dictionary<Type, InheritedElement> incomingWidgets = _parent?._inheritedWidgets;

            if (incomingWidgets != null) {
                _inheritedWidgets = new Dictionary<Type, InheritedElement>(incomingWidgets);
            }
            else {
                _inheritedWidgets = new Dictionary<Type, InheritedElement>();
            }

            
            _inheritedWidgets[widget.GetType()] = this;
        }

        public override void debugDeactivated() {
            D.assert(() => {
                D.assert(_dependents.isEmpty());
                return true;
            });
            base.debugDeactivated();
        }

        public object getDependencies(Element dependent) {
            return _dependents[dependent];
        }

        public void setDependencies(Element dependent, object value) {
            object existing;
            if (_dependents.TryGetValue(dependent, out existing)) {
                if (Equals(existing, value)) {
                    return;
                }
            }

            _dependents[dependent] = value;
        }

        public void updateDependencies(Element dependent, object aspect) {
            setDependencies(dependent, null);
        }

        public void notifyDependent(InheritedWidget oldWidget, Element dependent) {
            dependent.didChangeDependencies();
        }

        protected override void updated(ProxyWidget oldWidget) {
            if (widget.updateShouldNotify((InheritedWidget) oldWidget)) {
                base.updated(oldWidget);
            }
        }

        public override void notifyClients(ProxyWidget oldWidgetRaw) {
            var oldWidget = (InheritedWidget) oldWidgetRaw;

            D.assert(_debugCheckOwnerBuildTargetExists("notifyClients"));
            foreach (Element dependent in _dependents.Keys) {
                D.assert(() => {
                    Element ancestor = dependent._parent;
                    while (ancestor != this && ancestor != null) {
                        ancestor = ancestor._parent;
                    }

                    return ancestor == this;
                });
                D.assert(dependent._dependencies.Contains(this));
                notifyDependent(oldWidget, dependent);
            }
        }
    }

    public abstract class RenderObjectElement : Element {
        protected RenderObjectElement(RenderObjectWidget widget) : base(widget) {
        }
        
        public new RenderObjectWidget widget {
            get { return (RenderObjectWidget) base.widget; }
        }

        RenderObject _renderObject;
        
        public override RenderObject renderObject {
            get { return _renderObject; }
        }

        public bool _debugDoingBuild = false;

        public override bool debugDoingBuild {
            get { return _debugDoingBuild; }
        }

        RenderObjectElement _ancestorRenderObjectElement;

        RenderObjectElement _findAncestorRenderObjectElement() {
            Element ancestor = _parent;
            while (ancestor != null && !(ancestor is RenderObjectElement)) {
                ancestor = ancestor._parent;
            }

            return ancestor as RenderObjectElement;
        }

        ParentDataElement _findAncestorParentDataElement() {
            Element ancestor = _parent;
            ParentDataElement result = null;
            while (ancestor != null && !(ancestor is RenderObjectElement)) {
                if (ancestor is ParentDataElement parentDataElement) {
                    result = parentDataElement;
                    break;
                }

                ancestor = ancestor._parent;
            }

            D.assert(() => {
                if (result == null || ancestor == null) {
                    return true;
                }
                
                List<ParentDataElement> badAncestors = new List<ParentDataElement>();
                ancestor = ancestor._parent;
                while (ancestor != null && !(ancestor is RenderObjectElement)) {
                    if (ancestor is ParentDataElement parentDataElement) {
                        badAncestors.Add(parentDataElement);
                    }

                    ancestor = ancestor._parent;
                }

                if (badAncestors.isNotEmpty()) {
                    badAncestors.Insert(0, result);
                    try {
                        List<ErrorDescription> errors = new List<ErrorDescription>();
                        foreach (ParentDataElement<ParentData> parentDataElement in badAncestors) {
                            errors.Add(new ErrorDescription(
                                $"- {parentDataElement.widget} (typically placed directly inside a {parentDataElement.widget.debugTypicalAncestorWidgetClass} widget)"));
                        }

                        List<DiagnosticsNode> results = new List<DiagnosticsNode>();
                        results.Add( new ErrorSummary("Incorrect use of ParentDataWidget."));
                        results.Add(new ErrorDescription("The following ParentDataWidgets are providing parent data to the same RenderObject:"));
                        results.AddRange(errors);
                        results.Add(new ErrorDescription("However, a RenderObject can only receive parent data from at most one ParentDataWidget."));
                        results.Add(new ErrorHint("Usually, this indicates that at least one of the offending ParentDataWidgets listed above is not placed directly inside a compatible ancestor widget."));
                        results.Add(new ErrorDescription($"The ownership chain for the RenderObject that received the parent data was:\n  {debugGetCreatorChain(10)}"));
                        throw new UIWidgetsError(
                           results
                           );
                    }
                    catch (UIWidgetsError e) {
                        WidgetsD._debugReportException("while looking for parent data.", e);
                    }
                }

                return true;
            });

            return result;
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            D.assert(() => {
                _debugDoingBuild = true;
                return true;
            });
            _renderObject = widget.createRenderObject(this);
            D.assert(() => {
                _debugDoingBuild = false;
                return true;
            });
            D.assert(() => {
                _debugUpdateRenderObjectOwner();
                return true;
            });
            D.assert(slot == newSlot);
            attachRenderObject(newSlot);
            _dirty = false;
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(widget == newWidget);
            D.assert(() => {
                _debugUpdateRenderObjectOwner();
                return true;
            });
            D.assert(() => {
                _debugDoingBuild = true;
                return true;
            });
            widget.updateRenderObject(this, renderObject);
            D.assert(() => {
                _debugDoingBuild = false;
                return true;
            });
            _dirty = false;
        }

        void _debugUpdateRenderObjectOwner() {
            D.assert(() => {
                _renderObject.debugCreator = new DebugCreator(this);
                return true;
            });
        }

        protected override void performRebuild() {
            D.assert(() => {
                _debugDoingBuild = true;
                return true;
            });
            widget.updateRenderObject(this, renderObject);
            D.assert(() => {
                _debugDoingBuild = false;
                return true;
            });
            _dirty = false;
        }

        protected List<Element> updateChildren(List<Element> oldChildren, List<Widget> newWidgets,
            HashSet<Element> forgottenChildren = null) {
            D.assert(oldChildren != null);
            D.assert(newWidgets != null);

            var replaceWithNullIfForgotten = new Func<Element, Element>(child =>
                forgottenChildren != null && forgottenChildren.Contains(child) ? (Element) null : child);


            int newChildrenTop = 0;
            int oldChildrenTop = 0;
            int newChildrenBottom = newWidgets.Count - 1;
            int oldChildrenBottom = oldChildren.Count - 1;

            var newChildren = oldChildren.Count == newWidgets.Count
                ? oldChildren
                : CollectionUtils.CreateRepeatedList<Element>(null, newWidgets.Count);

            Element previousChild = null;

            while ((oldChildrenTop <= oldChildrenBottom) && (newChildrenTop <= newChildrenBottom)) {
                Element oldChild = replaceWithNullIfForgotten(oldChildren[oldChildrenTop]);
                Widget newWidget = newWidgets[newChildrenTop];
                D.assert(oldChild == null || oldChild._debugLifecycleState == _ElementLifecycle.active);
                if (oldChild == null || !Widget.canUpdate(oldChild.widget, newWidget)) {
                    break;
                }

                Element newChild = updateChild(oldChild, newWidget, new IndexedSlot<Element>(newChildrenTop, previousChild));
                D.assert(newChild._debugLifecycleState == _ElementLifecycle.active);
                newChildren[newChildrenTop] = newChild;
                previousChild = newChild;
                newChildrenTop += 1;
                oldChildrenTop += 1;
            }

            while ((oldChildrenTop <= oldChildrenBottom) && (newChildrenTop <= newChildrenBottom)) {
                Element oldChild = replaceWithNullIfForgotten(oldChildren[oldChildrenBottom]);
                Widget newWidget = newWidgets[newChildrenBottom];
                D.assert(oldChild == null || oldChild._debugLifecycleState == _ElementLifecycle.active);
                if (oldChild == null || !Widget.canUpdate(oldChild.widget, newWidget)) {
                    break;
                }

                oldChildrenBottom -= 1;
                newChildrenBottom -= 1;
            }

            bool haveOldChildren = oldChildrenTop <= oldChildrenBottom;
            Dictionary<Key, Element> oldKeyedChildren = null;
            if (haveOldChildren) {
                oldKeyedChildren = new Dictionary<Key, Element>();
                while (oldChildrenTop <= oldChildrenBottom) {
                    Element oldChild = replaceWithNullIfForgotten(oldChildren[oldChildrenTop]);
                    D.assert(oldChild == null || oldChild._debugLifecycleState == _ElementLifecycle.active);
                    if (oldChild != null) {
                        if (oldChild.widget.key != null) {
                            oldKeyedChildren[oldChild.widget.key] = oldChild;
                        }
                        else {
                            deactivateChild(oldChild);
                        }
                    }

                    oldChildrenTop += 1;
                }
            }

            while (newChildrenTop <= newChildrenBottom) {
                Element oldChild = null;
                Widget newWidget = newWidgets[newChildrenTop];
                if (haveOldChildren) {
                    Key key = newWidget.key;
                    if (key != null) {
                        oldChild = oldKeyedChildren.getOrDefault(key);
                        if (oldChild != null) {
                            if (Widget.canUpdate(oldChild.widget, newWidget)) {
                                oldKeyedChildren.Remove(key);
                            }
                            else {
                                oldChild = null;
                            }
                        }
                    }
                }

                D.assert(oldChild == null || Widget.canUpdate(oldChild.widget, newWidget));
                Element newChild = updateChild(oldChild, newWidget, new IndexedSlot<Element>(newChildrenTop, previousChild));
                D.assert(newChild._debugLifecycleState == _ElementLifecycle.active);
                D.assert(oldChild == newChild || oldChild == null ||
                         oldChild._debugLifecycleState != _ElementLifecycle.active);
                newChildren[newChildrenTop] = newChild;
                previousChild = newChild;
                newChildrenTop += 1;
            }

            D.assert(oldChildrenTop == oldChildrenBottom + 1);
            D.assert(newChildrenTop == newChildrenBottom + 1);
            D.assert(newWidgets.Count - newChildrenTop == oldChildren.Count - oldChildrenTop);
            newChildrenBottom = newWidgets.Count - 1;
            oldChildrenBottom = oldChildren.Count - 1;

            while ((oldChildrenTop <= oldChildrenBottom) && (newChildrenTop <= newChildrenBottom)) {
                Element oldChild = oldChildren[oldChildrenTop];
                D.assert(replaceWithNullIfForgotten(oldChild) != null);
                D.assert(oldChild._debugLifecycleState == _ElementLifecycle.active);
                Widget newWidget = newWidgets[newChildrenTop];
                D.assert(Widget.canUpdate(oldChild.widget, newWidget));
                Element newChild = updateChild(oldChild, newWidget, new IndexedSlot<Element>(newChildrenTop, previousChild));
                D.assert(newChild._debugLifecycleState == _ElementLifecycle.active);
                D.assert(oldChild == newChild || oldChild == null ||
                         oldChild._debugLifecycleState != _ElementLifecycle.active);
                newChildren[newChildrenTop] = newChild;
                previousChild = newChild;
                newChildrenTop += 1;
                oldChildrenTop += 1;
            }

            if (haveOldChildren && oldKeyedChildren.isNotEmpty()) {
                foreach (Element oldChild in oldKeyedChildren.Values) {
                    if (forgottenChildren == null || !forgottenChildren.Contains(oldChild)) {
                        deactivateChild(oldChild);
                    }
                }
            }

            return newChildren;
        }

        public override void deactivate() {
            base.deactivate();
            D.assert(!renderObject.attached,
                () => "A RenderObject was still attached when attempting to deactivate its " +
                      "RenderObjectElement: " + renderObject);
        }

        public override void unmount() {
            base.unmount();
            D.assert(!renderObject.attached,
                () => "A RenderObject was still attached when attempting to unmount its " +
                      "RenderObjectElement: " + renderObject);
            widget.didUnmountRenderObject(renderObject);
        }

        internal void _updateParentData(ParentDataWidget parentDataWidget) {
            bool applyParentData = true;
            D.assert(() => {
                try {
                    if (!parentDataWidget.debugIsValidRenderObject(renderObject)) {
                        applyParentData = false;
                        List<DiagnosticsNode> results = new List<DiagnosticsNode>();
                        results.Add( new ErrorSummary("Incorrect use of ParentDataWidget.\n") );
                        results.AddRange(parentDataWidget._debugDescribeIncorrectParentDataType(
                            parentData: renderObject.parentData,
                            parentDataCreator: _ancestorRenderObjectElement.widget,
                            ownershipChain: new ErrorDescription(debugGetCreatorChain(10))
                        ));
                        throw new UIWidgetsError(
                            results
                        );
                    }
                }
                catch (UIWidgetsError e) {
                    WidgetsD._debugReportException(new ErrorSummary("while apply parent data"), e);
                }

                return true;
            });
            if (applyParentData)
                parentDataWidget.applyParentData(renderObject);
        }

        internal override void _updateSlot(object newSlot) {
            D.assert(slot != newSlot);
            base._updateSlot(newSlot);
            D.assert(slot == newSlot);
            _ancestorRenderObjectElement.moveChildRenderObject(renderObject, slot);
        }

        public override void attachRenderObject(object newSlot) {
            D.assert(_ancestorRenderObjectElement == null);
            _slot = newSlot;
            _ancestorRenderObjectElement = _findAncestorRenderObjectElement();
            _ancestorRenderObjectElement?.insertChildRenderObject(renderObject, newSlot);

            ParentDataElement parentDataElement = _findAncestorParentDataElement();
            if (parentDataElement != null) {
                _updateParentData(parentDataElement.widget);
            }
        }

        public override void detachRenderObject() {
            if (_ancestorRenderObjectElement != null) {
                _ancestorRenderObjectElement.removeChildRenderObject(renderObject);
                _ancestorRenderObjectElement = null;
            }

            _slot = null;
        }

        protected abstract void insertChildRenderObject(RenderObject child, object slot);

        protected abstract void moveChildRenderObject(RenderObject child, object slot);

        protected abstract void removeChildRenderObject(RenderObject child);

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<RenderObject>("renderObject", renderObject,
                defaultValue: foundation_.kNullDefaultValue));
        }
    }

    public abstract class RootRenderObjectElement : RenderObjectElement {
        protected RootRenderObjectElement(RenderObjectWidget widget) : base(widget) {
        }

        public void assignOwner(BuildOwner owner) {
            _owner = owner;
        }

        public override void mount(Element parent, object newSlot) {
            D.assert(parent == null);
            D.assert(newSlot == null);
            base.mount(parent, newSlot);
        }
    }

    public class LeafRenderObjectElement : RenderObjectElement {
        public LeafRenderObjectElement(LeafRenderObjectWidget widget) : base(widget) {
        }

        public override void forgetChild(Element child) {
            D.assert(false);
            base.forgetChild(child);
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(false);
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            return widget.debugDescribeChildren();
        }
    }

    public class SingleChildRenderObjectElement : RenderObjectElement {
        public SingleChildRenderObjectElement(SingleChildRenderObjectWidget widget) : base(widget) {
        }

        public new SingleChildRenderObjectWidget widget {
            get { return (SingleChildRenderObjectWidget) base.widget; }
        }

        Element _child;

        public override void visitChildren(ElementVisitor visitor) {
            if (_child != null) {
                visitor(_child);
            }
        }

        public override void forgetChild(Element child) {
            D.assert(child == _child);
            _child = null;
            base.forgetChild(child);
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            _child = updateChild(_child, widget.child, null);
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(widget == newWidget);
            _child = updateChild(_child, widget.child, null);
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            var renderObject = (RenderObjectWithChildMixin) this.renderObject;
            D.assert(slot == null);
            D.assert(renderObject.debugValidateChild(child));
            renderObject.child = child;
            D.assert(renderObject == this.renderObject);
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            var renderObject = (RenderObjectWithChildMixin) this.renderObject;
            D.assert(renderObject.child == child);
            renderObject.child = null;
            D.assert(renderObject == this.renderObject);
        }
    }

    public class MultiChildRenderObjectElement : RenderObjectElement {
        public MultiChildRenderObjectElement(MultiChildRenderObjectWidget widget)
            : base(widget) {
            D.assert(!WidgetsD.debugChildrenHaveDuplicateKeys(widget, widget.children));
        }

        public new MultiChildRenderObjectWidget widget {
            get { return (MultiChildRenderObjectWidget) base.widget; }
        }

        protected IEnumerable<Element> children {
            get { return LinqUtils<Element>.WhereList(_children,((child) => !_forgottenChildren.Contains(child))); }
        }

        List<Element> _children;

        readonly HashSet<Element> _forgottenChildren = new HashSet<Element>();

        protected override void insertChildRenderObject(RenderObject child, object slotRaw) {
            IndexedSlot<Element> slot = (IndexedSlot<Element>) slotRaw;
            var renderObject = (ContainerRenderObjectMixin) this.renderObject;
            D.assert(renderObject.debugValidateChild(child));
            renderObject.insert(child, after: slot == null ? null : slot.value?.renderObject);
            D.assert(renderObject == this.renderObject);
        }

        protected override void moveChildRenderObject(RenderObject child, object slotRaw) {
            IndexedSlot<Element> slot = (IndexedSlot<Element>) slotRaw;
            var renderObject = (ContainerRenderObjectMixin) this.renderObject;
            D.assert(child.parent == renderObject);
            renderObject.move(child, after: slot == null ? null : slot.value?.renderObject);
            D.assert(renderObject == this.renderObject);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            var renderObject = (ContainerRenderObjectMixin) this.renderObject;
            D.assert(child.parent == renderObject);
            renderObject.remove(child);
            D.assert(renderObject == this.renderObject);
        }

        public override void visitChildren(ElementVisitor visitor) {
            foreach (Element child in _children) {
                if (!_forgottenChildren.Contains(child)) {
                    visitor(child);
                }
            }
        }

        public override void forgetChild(Element child) {
            D.assert(_children.Contains(child));
            D.assert(!_forgottenChildren.Contains(child));
            _forgottenChildren.Add(child);
            base.forgetChild(child);
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            _children = CollectionUtils.CreateRepeatedList<Element>(null, widget.children.Count);
            Element previousChild = null;
            for (int i = 0; i < _children.Count; i += 1) {
                Element newChild = inflateWidget(widget.children[i], new IndexedSlot<Element>(i, previousChild));
                _children[i] = newChild;
                previousChild = newChild;
            }
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(widget == newWidget);
            _children = updateChildren(_children, widget.children,
                forgottenChildren: _forgottenChildren);
            _forgottenChildren.Clear();
        }
    }

    public class DebugCreator {
        public DebugCreator(Element element) {
            this.element = element;
        }

        public readonly Element element;

        public override string ToString() {
            return element.debugGetCreatorChain(12);
        }
    }

    public class IndexedSlot<K> : IEquatable<IndexedSlot<K>> {
        public IndexedSlot(int index, K value) {
            this.index = index;
            this.value = value;
        }

        public readonly K value;

        public readonly int index;

        public static bool operator ==(IndexedSlot<K> slot, object other) {
            if (slot is null) {
                return other is null;
            }

            return slot.Equals(other);
        }

        public static bool operator !=(IndexedSlot<K> slot, object other) {
            return !(slot == other);
        }

        public bool Equals(IndexedSlot<K> other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return EqualityComparer<K>.Default.Equals(value, other.value) && index == other.index;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((IndexedSlot<K>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<K>.Default.GetHashCode(value) * 397) ^ index;
            }
        }
    }
}