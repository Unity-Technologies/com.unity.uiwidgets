using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public delegate UiWidgetAction ActionFactory();
    public delegate void OnInvokeCallback(FocusNode node, Intent tag);
    public class Intent : Diagnosticable {
        public Intent(LocalKey key) : base() {
            D.assert(key != null);
            this.key = key;
        }

        public static readonly Intent doNothing = new Intent(DoNothingAction.key);
        public readonly LocalKey key;

        public virtual bool isEnabled(BuildContext context) => true;
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<LocalKey>("key", key));
        }
    }

    public abstract class UiWidgetAction : Diagnosticable {

        public UiWidgetAction(LocalKey intentKey) {
            D.assert(intentKey != null);
            this.intentKey = intentKey;
        }

        public readonly LocalKey intentKey;

        public abstract void invoke(FocusNode node,  Intent intent);
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<LocalKey>("intentKey", intentKey));
        }
    }

    

    
    public class CallbackAction : UiWidgetAction  {
        public CallbackAction(LocalKey intentKey = null, OnInvokeCallback onInvoke = null) : base(intentKey: intentKey) {
            D.assert(onInvoke != null);
            this.onInvoke = onInvoke;
        }
        protected readonly OnInvokeCallback onInvoke;

        public override void invoke(FocusNode node, Intent intent) {
            onInvoke.Invoke(node, intent);

        }
    }

    
    public class ActionDispatcher : Diagnosticable {
        public ActionDispatcher() {
        }

        public bool invokeAction(UiWidgetAction action, Intent intent, FocusNode focusNode = null) {
            D.assert(action != null);
            D.assert(intent != null);
            focusNode = focusNode ?? FocusManagerUtils.primaryFocus;
            if (action != null && intent.isEnabled(focusNode.context)) {
              action.invoke(focusNode, intent);
              return true;
            }
            return false;
        }
    }

    public class Actions : InheritedWidget {
        public Actions(
            Key key = null,
            ActionDispatcher dispatcher = null,
            Dictionary<LocalKey, ActionFactory> actions = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(actions != null);
            this.dispatcher = dispatcher;
            this.actions = actions;
        }

        public readonly ActionDispatcher dispatcher;

        public readonly Dictionary<LocalKey, ActionFactory> actions;

        public static ActionDispatcher _findDispatcher(Element element) {
            D.assert(element.widget is Actions);
            Actions actions = element.widget as Actions;
            ActionDispatcher dispatcher = actions.dispatcher;
            if (dispatcher == null) {
                bool visitAncestorElement(Element visitedElement) {
                    if (!(visitedElement.widget is Actions)) {

                        return true;
                    }

                    Actions actions1 = visitedElement.widget as Actions;
                    if (actions1.dispatcher == null) {

                        return true;
                    }

                    dispatcher = actions1.dispatcher;
                    return false;
                }

                element.visitAncestorElements(visitAncestorElement);
            }

            return dispatcher ?? new ActionDispatcher();
        }

        public static ActionDispatcher of(BuildContext context, bool nullOk = false) {
            D.assert(context != null);
            InheritedElement inheritedElement = context.getElementForInheritedWidgetOfExactType<Actions>();
            Actions inherited = context.dependOnInheritedElement(inheritedElement) as Actions;
            D.assert(() => {
                if (nullOk) {
                    return true;
                }
                if (inherited == null) {
                    throw new UIWidgetsError("Unable to find an $Actions widget in the context.\n" +
                                             "Actions.of() was called with a context that does not contain an " +
                                             $"{typeof(Actions)} widget.\n" +
                                             $"No {typeof(Actions)} ancestor could be found starting from the context that " +
                                             "was passed to {Actions.of()}. This can happen if the context comes " +
                                             "from a widget above those widgets.\n" +
                                             "The context used was:\n" +
                                             $"{context}");
                }

                return true;
            });
            return inherited?.dispatcher ?? _findDispatcher(inheritedElement);
        }

        public static bool invoke(
            BuildContext context,
            Intent intent,
            FocusNode focusNode = null,
            bool nullOk = false
        ) {
            D.assert(context != null);
            D.assert(intent != null);
            Element actionsElement = null;
            UiWidgetAction action = null;

            bool visitAncestorElement(Element element) {
                if (!(element.widget is Actions)) {
                    return true;
                }
                actionsElement = element;
                Actions actions = element.widget as Actions;
                action = actions.actions.getOrDefault(intent.key)?.Invoke();

                return action == null;
            }

            context.visitAncestorElements(visitAncestorElement);
            D.assert(() => {
                if (nullOk) {
                    return true;
                }

                if (actionsElement == null) {
                    throw new UIWidgetsError($"Unable to find a {typeof(Actions)} widget in the context.\n" +
                                             "Actions.invoke() was called with a context that does not contain an " +
                                             $"{typeof(Actions)} widget.\n" +
                                             $"No {typeof(Actions)} ancestor could be found starting from the context that " +
                                             "was passed to Actions.invoke(). This can happen if the context comes " +
                                             "from a widget above those widgets.\n" +
                                             "The context used was:\n" +
                                             $"  {context}");
                }

                if (action == null) {
                    throw new UIWidgetsError(
                        $"Unable to find an action for an intent in the {typeof(Actions)} widget in the context.\n" +
                        "Actions.invoke() was called on an {typeof(Actions)} widget that doesn't " +
                        "contain a mapping for the given intent.\n" +
                        "The context used was:\n" +
                        $"  {context}\n" +
                        "The intent requested was:\n" +
                        $" {intent}");
                }

                return true;
            });
            if (action == null) {
                return false;
            }

            return _findDispatcher(actionsElement).invokeAction(action, intent, focusNode: focusNode);
        }
       
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ActionDispatcher>("dispatcher", dispatcher));
            properties.add(new DiagnosticsProperty<Dictionary<LocalKey, ActionFactory>>("actions", actions));
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            oldWidget = (Actions) oldWidget;
            var dispatcherBool = ((Actions) oldWidget).dispatcher != dispatcher;
            bool actionBool = ((Actions) oldWidget).actions == actions;
            foreach (var actionsKey in ((Actions) oldWidget).actions.Keys) {
                if (!actions.ContainsKey(actionsKey) ||
                    actions[actionsKey] != ((Actions) oldWidget).actions[actionsKey]) {
                    actionBool = false;
                }
            }

            return dispatcherBool || actionBool;


        }
       
    }

    public class FocusableActionDetector : StatefulWidget {

        public FocusableActionDetector(
            Key key = null,
            bool? enabled = true,
            FocusNode focusNode = null,
            bool? autofocus = false,
            Dictionary<LogicalKeySet, Intent> shortcuts = null,
            Dictionary<LocalKey, ActionFactory> actions = null,
            ValueChanged<bool> onShowFocusHighlight = null,
            ValueChanged<bool> onShowHoverHighlight = null,
            ValueChanged<bool> onFocusChange = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(enabled != null);
            D.assert(autofocus != null);
            D.assert(child != null);
            this.enabled = enabled.Value;
            this.focusNode = focusNode;
            this.autofocus = autofocus.Value;
            this.shortcuts = shortcuts;
            this.actions = actions;
            this.onShowFocusHighlight = onShowFocusHighlight;
            this.onShowHoverHighlight = onShowHoverHighlight;
            this.onFocusChange = onFocusChange;
            this.child = child;
        }


        public readonly bool enabled;


        public readonly FocusNode focusNode;


        public readonly bool autofocus;


        public readonly Dictionary<LocalKey, ActionFactory> actions;


        public readonly Dictionary<LogicalKeySet, Intent> shortcuts;


        public readonly ValueChanged<bool> onShowFocusHighlight;


        public readonly ValueChanged<bool> onShowHoverHighlight;


        public readonly ValueChanged<bool> onFocusChange;

        public readonly Widget child;

        public override State createState() {
            return new _FocusableActionDetectorState();
        }
    }

    class _FocusableActionDetectorState : State<FocusableActionDetector> {
        
        public override void initState() {
            base.initState();
            SchedulerBinding.instance.addPostFrameCallback(((TimeSpan timespan) =>  {
              _updateHighlightMode(FocusManager.instance.highlightMode);
            }));
            FocusManager.instance.addHighlightModeListener(_handleFocusHighlightModeChange);
        }

       
        public override void dispose() {
            FocusManager.instance.removeHighlightModeListener(_handleFocusHighlightModeChange);
            base.dispose();
        }

        bool _canShowHighlight = false;
        void _updateHighlightMode(FocusHighlightMode mode) {
             _mayTriggerCallback(task: () =>{
              switch (FocusManager.instance.highlightMode) {
                case FocusHighlightMode.touch:
                  _canShowHighlight = false;
                  break;
                case FocusHighlightMode.traditional:
                  _canShowHighlight = true;
                  break;
              }
            });
        }

   
        void _handleFocusHighlightModeChange(FocusHighlightMode mode) {
            if (!mounted) {
              return;
            }
            _updateHighlightMode(mode);
        }

        bool _hovering = false;
        
        void _handleMouseEnter(PointerEnterEvent Event) {
            D.assert(widget.onShowHoverHighlight != null);
            if (!_hovering) {
              _mayTriggerCallback(task: ()=> {
                _hovering = true;
              });
            }
        }

        void _handleMouseExit(PointerExitEvent Event) {
            D.assert(widget.onShowHoverHighlight != null);
            if (_hovering) {
              _mayTriggerCallback(task: ()=>{
                _hovering = false;
              });
            }
        }

        bool _focused = false;
        void _handleFocusChange(bool focused) {
            if (_focused != focused) {
              _mayTriggerCallback(task: () =>{
                _focused = focused;
              });
              widget.onFocusChange?.Invoke(_focused);
            }
        }


        void _mayTriggerCallback(VoidCallback task = null, FocusableActionDetector oldWidget = null) {
            bool shouldShowHoverHighlight(FocusableActionDetector target) {
              return _hovering && target.enabled && _canShowHighlight;
            }
            bool shouldShowFocusHighlight(FocusableActionDetector target) {
              return _focused && target.enabled && _canShowHighlight;
            }
            D.assert(SchedulerBinding.instance.schedulerPhase != SchedulerPhase.persistentCallbacks);
            FocusableActionDetector oldTarget = oldWidget ?? widget;
            bool didShowHoverHighlight = shouldShowHoverHighlight(oldTarget);
            bool didShowFocusHighlight = shouldShowFocusHighlight(oldTarget);
            if (task != null)
              task();
            bool doShowHoverHighlight = shouldShowHoverHighlight(widget);
             bool doShowFocusHighlight = shouldShowFocusHighlight(widget);
            if (didShowFocusHighlight != doShowFocusHighlight)
              widget.onShowFocusHighlight?.Invoke(doShowFocusHighlight);
            if (didShowHoverHighlight != doShowHoverHighlight)
              widget.onShowHoverHighlight?.Invoke(doShowHoverHighlight);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget =(FocusableActionDetector) oldWidget;
            base.didUpdateWidget(oldWidget);
            if (widget.enabled != ((FocusableActionDetector)oldWidget).enabled) {
              SchedulerBinding.instance.addPostFrameCallback((TimeSpan timespan)=> {
                _mayTriggerCallback(oldWidget: (FocusableActionDetector)oldWidget);
              });
            }
        }

       
        public override Widget build(BuildContext context) {
            Widget child = new MouseRegion(
              onEnter: _handleMouseEnter,
              onExit: _handleMouseExit,
              child: new Focus(
                focusNode: widget.focusNode,
                autofocus: widget.autofocus,
                canRequestFocus: widget.enabled,
                onFocusChange: _handleFocusChange,
                child: widget.child
              )
            );
            if (widget.enabled && widget.actions != null && widget.actions.isNotEmpty()) {
               
                child = new Actions(actions: widget.actions, child: child);
            }
            if (widget.enabled && widget.shortcuts != null && widget.shortcuts.isNotEmpty()) {
              child = new Shortcuts(shortcuts: widget.shortcuts, child: child);
            }
            return child;
        }
    }

   
    public class DoNothingAction : UiWidgetAction {
        public DoNothingAction() : base(key) {
        }

        public readonly static LocalKey key = new ValueKey<Type>(typeof(DoNothingAction));
        public override void invoke(FocusNode node, Intent intent) { }
    
    }

    public abstract class ActivateAction : UiWidgetAction {
        public ActivateAction() : base(key) {
        }
        public readonly static LocalKey key = new ValueKey<Type>(typeof(ActivateAction));
    }

    public abstract class SelectAction : UiWidgetAction {

        public SelectAction() : base(key) {
        }
        public readonly static LocalKey key = new ValueKey<Type>(typeof(SelectAction));
    }

}