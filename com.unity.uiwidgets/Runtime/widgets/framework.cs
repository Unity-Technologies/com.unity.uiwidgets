using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        static readonly Dictionary<CompositeKey, Element> _debugReservations =
            new Dictionary<CompositeKey, Element>();

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

        internal void _debugReserveFor(Element parent) {
            CompositeKey compKey = new CompositeKey(Window.instance, this);
            D.assert(() => {
                D.assert(parent != null);
                if (_debugReservations.ContainsKey(compKey) && _debugReservations[compKey] != parent) {
                    string older = _debugReservations[compKey].ToString();
                    string newer = parent.ToString();
                    if (older != newer) {
                        throw new UIWidgetsError(
                            "Multiple widgets used the same GlobalKey.\n" +
                            $"The key {this} was used by multiple widgets. The parents of those widgets were:\n" +
                            $"- {older}\n" + $"- {newer}\n" +
                            "A GlobalKey can only be specified on one widget at a time in the widget tree.");
                    }

                    throw new UIWidgetsError(
                        "Multiple widgets used the same GlobalKey.\n" +
                        $"The key {this} was used by multiple widgets. The parents of those widgets were " +
                        "different widgets that both had the following description:\n" + $"  {newer}\n" +
                        "A GlobalKey can only be specified on one widget at a time in the widget tree.");
                }

                _debugReservations[compKey] = parent;
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
                _debugReservations.Clear();

                if (duplicates != null) {
                    var buffer = new StringBuilder();
                    buffer.AppendLine("Multiple widgets used the same GlobalKey.\n");
                    foreach (GlobalKey key in duplicates.Keys) {
                        HashSet<Element> elements = duplicates[key];
                        buffer.AppendLine($"The key {key} was used by {elements.Count} widgets:");
                        foreach (Element element in elements) {
                            buffer.AppendLine("- " + element);
                        }
                    }

                    buffer.Append("A GlobalKey can only be specified on one widget at a time in the widget tree.");
                    throw new UIWidgetsError(buffer.ToString());
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
            return key == null ? GetType().ToString() : GetType() + "-" + key;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.dense;
        }

        public static bool canUpdate(Widget oldWidget, Widget newWidget) {
            return oldWidget.GetType() == newWidget.GetType() && Equals(oldWidget.key, newWidget.key);
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

        public void setState(VoidCallback fn = null) {
            D.assert(() => {
                if (_debugLifecycleState == _StateLifecycle.defunct) {
                    throw new UIWidgetsError(
                        "setState() called after dispose(): " + this + "\n" +
                        "This error happens if you call setState() on a State object for a widget that " +
                        "no longer appears in the widget tree (e.g., whose parent widget no longer " +
                        "includes the widget in its build). This error can occur when code calls " +
                        "setState() from a timer or an animation callback. The preferred solution is " +
                        "to cancel the timer or stop listening to the animation in the dispose() " +
                        "callback. Another solution is to check the \"mounted\" property of this " +
                        "object before calling setState() to ensure the object is still in the " +
                        "tree.\n" +
                        "This error might indicate a memory leak if setState() is being called " +
                        "because another object is retaining a reference to this State object " +
                        "after it has been removed from the tree. To avoid memory leaks, " +
                        "consider breaking the reference to this object during dispose()."
                    );
                }

                if (_debugLifecycleState == _StateLifecycle.created && !mounted) {
                    throw new UIWidgetsError(
                        "setState() called in constructor: " + this + "\n" +
                        "This happens when you call setState() on a State object for a widget that " +
                        "hasn\"t been inserted into the widget tree yet. It is not necessary to call " +
                        "setState() in the constructor, since the state is already assumed to be dirty " +
                        "when it is initially created."
                    );
                }

                return true;
            });

            if (fn != null) {
                fn();
            }

            _element.markNeedsBuild();
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

        public abstract bool debugIsValidAncestor(RenderObjectWidget ancestor);

        public abstract string debugDescribeInvalidAncestorChain(
            string description = null,
            string ownershipChain = null,
            bool foundValidAncestor = false,
            IEnumerable<Widget> badAncestors = null
        );

        public abstract void applyParentData(RenderObject renderObject);

        public virtual bool debugCanApplyOutOfTurn() {
            return false;
        }
    }

    public abstract class ParentDataWidget<T> : ParentDataWidget where T : RenderObjectWidget {
        public ParentDataWidget(Key key = null, Widget child = null)
            : base(key: key, child: child) {
        }

        public override Element createElement() {
            return new ParentDataElement(this);
        }

        public override bool debugIsValidAncestor(RenderObjectWidget ancestor) {
            D.assert(typeof(T) != typeof(RenderObjectWidget));
            return ancestor is T;
        }

        public override string debugDescribeInvalidAncestorChain(
            string description = null,
            string ownershipChain = null,
            bool foundValidAncestor = false,
            IEnumerable<Widget> badAncestors = null
        ) {
            D.assert(typeof(T) != typeof(RenderObjectWidget));

            string result;
            if (!foundValidAncestor) {
                result = string.Format(
                    "{0} widgets must be placed inside {1} widgets.\n" +
                    "{2} has no {1} ancestor at all.\n", GetType(), typeof(T), description);
            }
            else {
                D.assert(badAncestors != null);
                D.assert(badAncestors.Any());
                result = string.Format(
                    "{0} widgets must be placed directly inside {1} widgets.\n" +
                    "{2} has a {1} ancestor, but there are other widgets between them:\n",
                    GetType(), typeof(T), description);

                foreach (Widget ancestor in badAncestors) {
                    if (ancestor.GetType() == GetType()) {
                        result +=
                            $"- {ancestor} (this is a different {GetType()} than the one with the problem)\n";
                    }
                    else {
                        result += $"- {ancestor}\n";
                    }
                }

                result += "These widgets cannot come between a " + GetType() + " and its " + typeof(T) + ".\n";
            }

            result += "The ownership chain for the parent of the offending "
                      + GetType() + " was:\n  " + ownershipChain;
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
            D.assert(!children.Any(child => child == null));
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

        internal void _deactivateRecursively(Element element) {
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

        RenderObject findRenderObject();

        Size size { get; }

        InheritedWidget inheritFromElement(InheritedElement ancestor, object aspect = null);

        InheritedWidget inheritFromWidgetOfExactType(Type targetType, object aspect = null);

        InheritedElement ancestorInheritedElementForWidgetOfExactType(Type targetType);

        Widget ancestorWidgetOfExactType(Type targetType);

        State ancestorStateOfType(TypeMatcher matcher);

        State rootAncestorStateOfType(TypeMatcher matcher);

        RenderObject ancestorRenderObjectOfType(TypeMatcher matcher);

        void visitAncestorElements(ElementVisitorBool visitor);

        void visitChildElements(ElementVisitor visitor);
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

        public readonly FocusManager focusManager = new FocusManager();

        public void scheduleBuildFor(Element element) {
            D.assert(element != null);
            D.assert(element.owner == this);
            D.assert(() => {
                if (WidgetsD.debugPrintScheduleBuildForStacks) {
                    Debug.Log("scheduleBuildFor() called for " + element +
                              (_dirtyElements.Contains(element) ? " (ALREADY IN LIST)" : ""));
                }

                if (!element.dirty) {
                    throw new UIWidgetsError(
                        "scheduleBuildFor() called for a widget that is not marked as dirty.\n" +
                        "The method was called for the following element:\n" +
                        "  " + element + "\n" +
                        "This element is not current marked as dirty. Make sure to set the dirty flag before " +
                        "calling scheduleBuildFor().\n" +
                        "If you did not attempt to call scheduleBuildFor() yourself, then this probably " +
                        "indicates a bug in the widgets framework."
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
                            "BuildOwner.scheduleBuildFor() called inappropriately.\n" +
                            "The BuildOwner.scheduleBuildFor() method should only be called while the " +
                            "buildScope() method is actively rebuilding the widget tree."
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

        internal bool debugBuilding {
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
                    Debug.LogFormat("buildScope called with context {0}; dirty list is: {1}",
                        context, _dirtyElements);
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
                        WidgetsD._debugReportException(
                            "while rebuilding dirty elements", ex,
                            informationCollector: (information) => {
                                information.AppendLine(
                                    "The element being rebuilt at the time was index "
                                    + index + " of " + dirtyCount + ":");
                                information.Append("  " + _dirtyElements[index]);
                            }
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
                            "buildScope missed some dirty elements.\n" +
                            "This probably indicates that the dirty list should have been resorted but was not.\n" +
                            "The list of dirty elements at the end of the buildScope call was:\n" +
                            "  " + _dirtyElements);
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
            if (_debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans != null) {
                _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans.Remove(node);
            }
        }

        public void finalizeTree() {
            try {
                lockState(() => { _inactiveElements._unmountAll(); });

                D.assert(() => {
                    try {
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
                                foreach (string key in keys.Select(key => key.ToString())) {
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
                                foreach (string element in elements.Select(element => element.ToString())) {
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

                                throw new UIWidgetsError(
                                    "Duplicate GlobalKeys detected in widget tree.\n" +
                                    "The following GlobalKeys were specified multiple times in the widget tree. This will lead to " +
                                    "parts of the widget tree being truncated unexpectedly, because the second time a key is seen, " +
                                    "the previous instance is moved to the new location. The keys were:\n" +
                                    "- " + string.Join("\n  ", keyLabels.ToArray()) + "\n" +
                                    "This was determined by noticing that after the widgets with the above global keys were moved " +
                                    "out of their respective previous parents, those previous parents never updated during this frame, meaning " +
                                    "that they either did not update at all or updated before the widgets were moved, in either case " +
                                    "implying that they still think that they should have a child with those global keys.\n" +
                                    "The specific parents that did not update after having one or more children forcibly removed " +
                                    "due to GlobalKey reparenting are:\n" +
                                    "- " + string.Join("\n  ", elementLabels.ToArray()) + "\n" +
                                    "A GlobalKey can only be specified on one widget at a time in the widget tree."
                                );
                            }
                        }
                    }
                    finally {
                        if (_debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans != null) {
                            _debugElementsThatWillNeedToBeRebuiltDueToGlobalKeyShenanigans.Clear();
                        }
                    }

                    return true;
                });
            }
            catch (Exception ex) {
                WidgetsD._debugReportException("while finalizing the widget tree", ex);
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

        internal Widget _widget;

        public Widget widget {
            get { return _widget; }
        }

        internal BuildOwner _owner;

        public BuildOwner owner {
            get { return _owner; }
        }

        public bool _active = false;

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
                    "visitChildElements() called during build.\n" +
                    "The BuildContext.visitChildElements() method can\"t be called during " +
                    "build because the child list is still being updated at that point, " +
                    "so the children might not be constructed yet, or might be old children " +
                    "that are going to be replaced."
                );
            });

            visitChildren(visitor);
        }

        protected virtual Element updateChild(Element child, Widget newWidget, object newSlot) {
            D.assert(() => {
                if (newWidget != null && newWidget.key is GlobalKey) {
                    GlobalKey key = (GlobalKey) newWidget.key;
                    key._debugReserveFor(this);
                }

                return true;
            });

            if (newWidget == null) {
                if (child != null) {
                    deactivateChild(child);
                }

                return null;
            }

            if (child != null) {
                if (Equals(child.widget, newWidget)) {
                    if (!Equals(child.slot, newSlot)) {
                        updateSlotForChild(child, newSlot);
                    }

                    return child;
                }

                if (Widget.canUpdate(child.widget, newWidget)) {
                    if (!Equals(child.slot, newSlot)) {
                        updateSlotForChild(child, newSlot);
                    }

                    child.update(newWidget);
                    D.assert(child.widget == newWidget);
                    D.assert(() => {
                        child.owner._debugElementWasRebuilt(child);
                        return true;
                    });
                    return child;
                }

                deactivateChild(child);
                D.assert(child._parent == null);
            }

            return inflateWidget(newWidget, newSlot);
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

            if (widget.key is GlobalKey) {
                GlobalKey key = (GlobalKey) widget.key;
                key._register(this);
            }

            _updateInheritance();
            D.assert(() => {
                _debugLifecycleState = _ElementLifecycle.active;
                return true;
            });
        }

        public virtual void update(Widget newWidget) {
            D.assert(_debugLifecycleState == _ElementLifecycle.active
                     && widget != null
                     && newWidget != null
                     && newWidget != widget
                     && depth != 0
                     && _active
                     && Widget.canUpdate(widget, newWidget));

            _widget = newWidget;
        }

        protected void updateSlotForChild(Element child, object newSlot) {
            D.assert(_debugLifecycleState == _ElementLifecycle.active);
            D.assert(child != null);
            D.assert(child._parent == this);

            ElementVisitor visit = null;
            visit = (element) => {
                element._updateSlot(newSlot);
                if (!(element is RenderObjectElement)) {
                    element.visitChildren(visit);
                }
            };
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
                            "A GlobalKey was used multiple times inside one widget\"s child list.\n" +
                            $"The offending GlobalKey was: {key}\n" +
                            $"The parent of the widgets with that key was:\n  {parent}\n" +
                            $"The first child to get instantiated with that key became:\n  {element}\n" +
                            $"The second child that was to be instantiated with that key was:\n  {widget}\n" +
                            "A GlobalKey can only be specified on one widget at a time in the widget tree.");
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

        protected abstract void forgetChild(Element child);

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
            if (_dependencies != null) {
                _dependencies.Clear();
            }

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
            D.assert(widget != null);
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
            D.assert(widget != null);
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
                            "Cannot get size of inactive element.\n" +
                            "In order for an element to have a valid size, the element must be " +
                            "active, which means it is part of the tree. Instead, this element " +
                            "is in the " + _debugLifecycleState + " state.\n" +
                            "The size getter was called for the following element:\n" +
                            "  " + this + "\n");
                    }

                    if (owner.debugBuilding) {
                        throw new UIWidgetsError(
                            "Cannot get size during build.\n" +
                            "The size of this render object has not yet been determined because " +
                            "the framework is still in the process of building widgets, which " +
                            "means the render tree for this frame has not yet been determined. " +
                            "The size getter should only be called from paint callbacks or " +
                            "interaction event handlers (e.g. gesture callbacks).\n" +
                            "\n" +
                            "If you need some sizing information during build to decide which " +
                            "widgets to build, consider using a LayoutBuilder widget, which can " +
                            "tell you the layout constraints at a given location in the tree." +
                            "\n" +
                            "The size getter was called for the following element:\n" +
                            "  " + this + "\n");
                    }

                    return true;
                });
                RenderObject renderObject = findRenderObject();
                D.assert(() => {
                    if (renderObject == null) {
                        throw new UIWidgetsError(
                            "Cannot get size without a render object.\n" +
                            "In order for an element to have a valid size, the element must have " +
                            "an associated render object. This element does not have an associated " +
                            "render object, which typically means that the size getter was called " +
                            "too early in the pipeline (e.g., during the build phase) before the " +
                            "framework has created the render tree.\n" +
                            "The size getter was called for the following element:\n" +
                            "  " + this + "\n");
                    }

                    if (renderObject is RenderSliver) {
                        throw new UIWidgetsError(
                            "Cannot get size from a RenderSliver.\n" +
                            "The render object associated with this element is a " +
                            renderObject.GetType() + ", which is a subtype of RenderSliver. " +
                            "Slivers do not have a size per se. They have a more elaborate " +
                            "geometry description, which can be accessed by calling " +
                            "findRenderObject and then using the \"geometry\" getter on the " +
                            "resulting object.\n" +
                            "The size getter was called for the following element:\n" +
                            "  " + this + "\n" +
                            "The associated render sliver was:\n" +
                            "  " + renderObject.toStringShallow(joiner: "\n  "));
                    }

                    if (!(renderObject is RenderBox)) {
                        throw new UIWidgetsError(
                            "Cannot get size from a render object that is not a RenderBox.\n" +
                            "Instead of being a subtype of RenderBox, the render object associated " +
                            "with this element is a " + renderObject.GetType() + ". If this type of " +
                            "render object does have a size, consider calling findRenderObject " +
                            "and extracting its size manually.\n" +
                            "The size getter was called for the following element:\n" +
                            "  " + this + "\n" +
                            "The associated render object was:\n" +
                            "  " + renderObject.toStringShallow(joiner: "\n  "));
                    }

                    RenderBox box = (RenderBox) renderObject;
                    if (!box.hasSize) {
                        throw new UIWidgetsError(
                            "Cannot get size from a render object that has not been through layout.\n" +
                            "The size of this render object has not yet been determined because " +
                            "this render object has not yet been through layout, which typically " +
                            "means that the size getter was called too early in the pipeline " +
                            "(e.g., during the build phase) before the framework has determined " +
                            "the size and position of the render objects during layout.\n" +
                            "The size getter was called for the following element:\n" +
                            "  " + this + "\n" +
                            "The render object from which the size was to be obtained was:\n" +
                            "  " + box.toStringShallow(joiner: "\n  "));
                    }

                    if (box.debugNeedsLayout) {
                        throw new UIWidgetsError(
                            "Cannot get size from a render object that has been marked dirty for layout.\n" +
                            "The size of this render object is ambiguous because this render object has " +
                            "been modified since it was last laid out, which typically means that the size " +
                            "getter was called too early in the pipeline (e.g., during the build phase) " +
                            "before the framework has determined the size and position of the render " +
                            "objects during layout.\n" +
                            "The size getter was called for the following element:\n" +
                            "  " + this + "\n" +
                            "The render object from which the size was to be obtained was:\n" +
                            "  \n" + box.toStringShallow(joiner: "\n  ") +
                            "Consider using debugPrintMarkNeedsLayoutStacks to determine why the render " +
                            "object in question is dirty, if you did not expect this.");
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
                        "Looking up a deactivated widget\"s ancestor is unsafe.\n" +
                        "At this point the state of the widget\"s element tree is no longer " +
                        "stable. To safely refer to a widget\"s ancestor in its dispose() method, " +
                        "save a reference to the ancestor by calling inheritFromWidgetOfExactType() " +
                        "in the widget\"s didChangeDependencies() method.\n");
                }

                return true;
            });
            return true;
        }

        public virtual InheritedWidget inheritFromElement(InheritedElement ancestor, object aspect = null) {
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

        public virtual InheritedElement ancestorInheritedElementForWidgetOfExactType(Type targetType) {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            InheritedElement ancestor = null;
            if (_inheritedWidgets != null) {
                _inheritedWidgets.TryGetValue(targetType, out ancestor);
            }

            return ancestor;
        }

        internal virtual void _updateInheritance() {
            D.assert(_active);
            _inheritedWidgets = _parent == null ? null : _parent._inheritedWidgets;
        }

        public virtual Widget ancestorWidgetOfExactType(Type targetType) {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            Element ancestor = _parent;
            while (ancestor != null && ancestor.widget.GetType() != targetType) {
                ancestor = ancestor._parent;
            }

            return ancestor == null ? null : ancestor.widget;
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
            return statefulAncestor == null ? null : statefulAncestor.state;
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

            return statefulAncestor == null ? null : statefulAncestor.state;
        }

        public virtual RenderObject ancestorRenderObjectOfType(TypeMatcher matcher) {
            D.assert(_debugCheckStateIsActiveForAncestorLookup());
            Element ancestor = _parent;
            while (ancestor != null) {
                var element = ancestor as RenderObjectElement;
                if (element != null && matcher.check(ancestor.renderObject)) {
                    break;
                }

                ancestor = ancestor._parent;
            }

            var renderObjectAncestor = ancestor as RenderObjectElement;
            return renderObjectAncestor == null ? null : renderObjectAncestor.renderObject;
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
                        methodName + " for " + widget.GetType() + " was called at an " +
                        "inappropriate time.\n" +
                        "It may only be called while the widgets are being built. A possible " +
                        "cause of this error is when $methodName is called during " +
                        "one of:\n" +
                        " * network I/O event\n" +
                        " * file I/O event\n" +
                        " * timer\n" +
                        " * microtask (caused by Future.then, async/await, scheduleMicrotask)"
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
            var chain = new List<Element>();
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
                List<DiagnosticsNode> diagnosticsDependencies = _dependencies
                    .Select((InheritedElement element) => element.widget.toDiagnosticsNode(style: DiagnosticsTreeStyle.sparse))
                    .ToList();
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
                        throw new UIWidgetsError(
                            "setState() or markNeedsBuild() called during build.\n" +
                            "This " + widget.GetType() +
                            " widget cannot be marked as needing to build because the framework " +
                            "is already in the process of building widgets. A widget can be marked as " +
                            "needing to be built during the build phase only if one of its ancestors " +
                            "is currently building. This exception is allowed because the framework " +
                            "builds parent widgets before children, which means a dirty descendant " +
                            "will always be built. Otherwise, the framework might not visit this " +
                            "widget during this build phase.\n" +
                            "The widget on which setState() or markNeedsBuild() was called was:\n" +
                            "  " + this + "\n" +
                            (owner._debugCurrentBuildTarget == null
                                ? ""
                                : "The widget which was currently being built when the offending call was made was:\n  " +
                                  owner._debugCurrentBuildTarget)
                        );
                    }

                    D.assert(dirty);
                }
                else if (owner._debugStateLocked) {
                    D.assert(!_debugAllowIgnoredCallsToMarkNeedsBuild);
                    throw new UIWidgetsError(
                        "setState() or markNeedsBuild() called when widget tree was locked.\n" +
                        "This " + widget.GetType() + " widget cannot be marked as needing to build " +
                        "because the framework is locked.\n" +
                        "The widget on which setState() or markNeedsBuild() was called was:\n" +
                        "  " + this + "\n"
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

    public delegate Widget ErrorWidgetBuilder(UIWidgetsErrorDetails details);

    public class ErrorWidget : LeafRenderObjectWidget {
        public ErrorWidget(Exception exception) : base(key: new UniqueKey()) {
            message = _stringify(exception);
        }

        public static ErrorWidgetBuilder builder = (details) => new ErrorWidget(details.exception);

        public readonly string message;

        static string _stringify(Exception exception) {
            try {
                return exception.ToString();
            }
            catch {
            }

            return "Error";
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return null;
            // return new RenderErrorBox(message);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("message", message, quoted: false));
        }
    }

    public delegate Widget WidgetBuilder(BuildContext context);

    public delegate Widget IndexedWidgetBuilder(BuildContext context, int index);

    public delegate Widget TransitionBuilder(BuildContext context, Widget child);
    
    public delegate Widget ControlsWidgetBuilder(BuildContext context, VoidCallback onStepContinue = null, VoidCallback onStepCancel = null);

    public abstract class ComponentElement : Element {
        protected ComponentElement(Widget widget) : base(widget) {
        }

        Element _child;

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
                built = build();
                WidgetsD.debugWidgetBuilderValue(widget, built);
            }
            catch (Exception e) {
                built = ErrorWidget.builder(WidgetsD._debugReportException("building " + this, e));
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
                built = ErrorWidget.builder(WidgetsD._debugReportException("building " + this, e));
                _child = updateChild(null, built, slot);
            }
        }

        protected abstract Widget build();

        public override void visitChildren(ElementVisitor visitor) {
            if (_child != null) {
                visitor(_child);
            }
        }

        protected override void forgetChild(Element child) {
            D.assert(child == _child);
            _child = null;
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
                        "StatefulWidget.createState must return a subtype of State<" + widget.GetType() + ">\n" +
                        "The createState function for " + widget.GetType() + " returned a state " +
                        "of type " + _state.GetType() + ", which is not a subtype of " +
                        "State<" + widget.GetType() + ">, violating the contract for createState.");
                }

                return true;
            });
            D.assert(_state._element == null);
            _state._element = this;
            D.assert(_state._widget == null);
            _state._widget = widget;
            D.assert(_state._debugLifecycleState == _StateLifecycle.created);
        }

        public new StatefulWidget widget {
            get { return (StatefulWidget) base.widget; }
        }

        protected override Widget build() {
            return state.build(this);
        }

        public State state {
            get { return _state; }
        }

        State _state;

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
                    _state.GetType() + ".dispose failed to call base.dispose.\n" +
                    "dispose() implementations must always call their superclass dispose() method, to ensure " +
                    "that all the resources used by the widget are fully released.");
            });
            _state._element = null;
            _state = null;
        }

        public override InheritedWidget inheritFromElement(InheritedElement ancestor, object aspect = null) {
            D.assert(ancestor != null);
            D.assert(() => {
                Type targetType = ancestor.widget.GetType();
                if (state._debugLifecycleState == _StateLifecycle.created) {
                    throw new UIWidgetsError(
                        "inheritFromWidgetOfExactType(" + targetType + ") or inheritFromElement() was called before " +
                        _state.GetType() + ".initState() completed.\n" +
                        "When an inherited widget changes, for example if the value of Theme.of() changes, " +
                        "its dependent widgets are rebuilt. If the dependent widget\"s reference to " +
                        "the inherited widget is in a constructor or an initState() method, " +
                        "then the rebuilt dependent widget will not reflect the changes in the " +
                        "inherited widget.\n" +
                        "Typically references to inherited widgets should occur in widget build() methods. Alternatively, " +
                        "initialization based on inherited widgets can be placed in the didChangeDependencies method, which " +
                        "is called after initState and whenever the dependencies change thereafter."
                    );
                }

                if (state._debugLifecycleState == _StateLifecycle.defunct) {
                    throw new UIWidgetsError(
                        "inheritFromWidgetOfExactType(" + targetType +
                        ") or inheritFromElement() was called after dispose(): " + this + "\n" +
                        "This error happens if you call inheritFromWidgetOfExactType() on the " +
                        "BuildContext for a widget that no longer appears in the widget tree " +
                        "(e.g., whose parent widget no longer includes the widget in its " +
                        "build). This error can occur when code calls " +
                        "inheritFromWidgetOfExactType() from a timer or an animation callback. " +
                        "The preferred solution is to cancel the timer or stop listening to the " +
                        "animation in the dispose() callback. Another solution is to check the " +
                        "\"mounted\" property of this object before calling " +
                        "inheritFromWidgetOfExactType() to ensure the object is still in the " +
                        "tree.\n" +
                        "This error might indicate a memory leak if " +
                        "inheritFromWidgetOfExactType() is being called because another object " +
                        "is retaining a reference to this State object after it has been " +
                        "removed from the tree. To avoid memory leaks, consider breaking the " +
                        "reference to this object during dispose()."
                    );
                }

                return true;
            });
            return base.inheritFromElement(ancestor, aspect: aspect);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _state.didChangeDependencies();
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

        protected abstract void notifyClients(ProxyWidget oldWidget);
    }

    public class ParentDataElement : ProxyElement {
        public ParentDataElement(ParentDataWidget widget) : base(widget) {
        }

        public new ParentDataWidget widget {
            get { return (ParentDataWidget) base.widget; }
        }

        public override void mount(Element parent, object newSlot) {
            D.assert(() => {
                var badAncestors = new List<Widget>();
                Element ancestor = parent;
                while (ancestor != null) {
                    if (ancestor is ParentDataElement) {
                        badAncestors.Add(ancestor.widget);
                    }
                    else if (ancestor is RenderObjectElement) {
                        if (widget.debugIsValidAncestor(((RenderObjectElement) ancestor).widget)) {
                            break;
                        }

                        badAncestors.Add(ancestor.widget);
                    }

                    ancestor = ancestor._parent;
                }

                if (ancestor != null && badAncestors.isEmpty()) {
                    return true;
                }

                throw new UIWidgetsError(
                    "Incorrect use of ParentDataWidget.\n" +
                    widget.debugDescribeInvalidAncestorChain(
                        description: ToString(),
                        ownershipChain: parent.debugGetCreatorChain(10),
                        foundValidAncestor: ancestor != null,
                        badAncestors: badAncestors
                    )
                );
            });
            base.mount(parent, newSlot);
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

        protected override void notifyClients(ProxyWidget oldWidget) {
            _applyParentData(widget);
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
            Dictionary<Type, InheritedElement> incomingWidgets =
                _parent == null ? null : _parent._inheritedWidgets;

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

        protected override void notifyClients(ProxyWidget oldWidgetRaw) {
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
            while (ancestor != null && !(ancestor is RenderObjectElement)) {
                var element = ancestor as ParentDataElement;
                if (element != null) {
                    return element;
                }

                ancestor = ancestor._parent;
            }

            return null;
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            _renderObject = widget.createRenderObject(this);
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
            widget.updateRenderObject(this, renderObject);
            _dirty = false;
        }

        void _debugUpdateRenderObjectOwner() {
            D.assert(() => {
                _renderObject.debugCreator = new _DebugCreator(this);
                return true;
            });
        }

        protected override void performRebuild() {
            widget.updateRenderObject(this, renderObject);
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

                Element newChild = updateChild(oldChild, newWidget, previousChild);
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

            // Update the middle of the list.
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
                Element newChild = updateChild(oldChild, newWidget, previousChild);
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
                Element newChild = updateChild(oldChild, newWidget, previousChild);
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

        internal void _updateParentData(ParentDataWidget parentData) {
            parentData.applyParentData(renderObject);
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
            if (_ancestorRenderObjectElement != null) {
                _ancestorRenderObjectElement.insertChildRenderObject(renderObject, newSlot);
            }

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

        protected override void forgetChild(Element child) {
            D.assert(false);
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

        protected override void forgetChild(Element child) {
            D.assert(child == _child);
            _child = null;
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
            get { return _children.Where((child) => !_forgottenChildren.Contains(child)); }
        }

        List<Element> _children;

        readonly HashSet<Element> _forgottenChildren = new HashSet<Element>();

        protected override void insertChildRenderObject(RenderObject child, object slotRaw) {
            Element slot = (Element) slotRaw;
            var renderObject = (ContainerRenderObjectMixin) this.renderObject;
            D.assert(renderObject.debugValidateChild(child));
            renderObject.insert(child, after: slot == null ? null : slot.renderObject);
            D.assert(renderObject == this.renderObject);
        }

        protected override void moveChildRenderObject(RenderObject child, object slotRaw) {
            Element slot = (Element) slotRaw;
            var renderObject = (ContainerRenderObjectMixin) this.renderObject;
            D.assert(child.parent == renderObject);
            renderObject.move(child, after: slot == null ? null : slot.renderObject);
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

        protected override void forgetChild(Element child) {
            D.assert(_children.Contains(child));
            D.assert(!_forgottenChildren.Contains(child));
            _forgottenChildren.Add(child);
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            _children = CollectionUtils.CreateRepeatedList<Element>(null, widget.children.Count);
            Element previousChild = null;
            for (int i = 0; i < _children.Count; i += 1) {
                Element newChild = inflateWidget(widget.children[i], previousChild);
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

    class _DebugCreator {
        internal _DebugCreator(RenderObjectElement element) {
            this.element = element;
        }

        public readonly RenderObjectElement element;

        public override string ToString() {
            return element.debugGetCreatorChain(12);
        }
    }
}