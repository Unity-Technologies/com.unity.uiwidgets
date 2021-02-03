using System;
using System.Collections.Generic;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.services;
using UnityEngine;

namespace Unity.UIWidgets.service {
    
    public class RawKeyboard {
        public static readonly RawKeyboard instance = new RawKeyboard();

        
        public static readonly Dictionary<PhysicalKeyboardKey, LogicalKeyboardKey> _keysPressed = new Dictionary<PhysicalKeyboardKey, LogicalKeyboardKey>();
        
        public HashSet<LogicalKeyboardKey> keysPressed 
        {
            get {
                HashSet<LogicalKeyboardKey> keyboardKeys = new HashSet<LogicalKeyboardKey>();
                foreach (var value in _keysPressed.Values) {
                    keyboardKeys.Add(value);
                }
                return keyboardKeys;
            }
        }

        RawKeyboard() {
            SystemChannels.keyEvent.setMessageHandler(_handleKeyEvent);
        }
        
        readonly List<ValueChanged<RawKeyEvent>> _listeners = new List<ValueChanged<RawKeyEvent>>();
        
        public void addListener(ValueChanged<RawKeyEvent> listener) {
            _listeners.Add(listener);
        }
        
        public void removeListener(ValueChanged<RawKeyEvent> listener) {
            _listeners.Remove(listener);
        }

        Future<object> _handleKeyEvent(object message)  {
            Debug.Log("message: reach c# from cpp!");
            return Future.value().to<object>();
            // RawKeyEvent keyEvent = RawKeyEvent.fromMessage(message as Map<String, dynamic>);
            // if (keyEvent == null) {
            //     return;
            // }
            // if (keyEvent.data is RawKeyEventDataMacOs && keyEvent.logicalKey == LogicalKeyboardKey.fn) {
            //     // On macOS laptop keyboards, the fn key is used to generate home/end and
            //     // f1-f12, but it ALSO generates a separate down/up keyEvent for the fn key
            //     // itself. Other platforms hide the fn key, and just produce the key that
            //     // it is combined with, so to keep it possible to write cross platform
            //     // code that looks at which keys are pressed, the fn key is ignored on
            //     // macOS.
            //     return;
            // }
            // if (keyEvent is RawKeyDownEvent) {
            //     _keysPressed[keyEvent.physicalKey] = keyEvent.logicalKey;
            // }
            // if (keyEvent is RawKeyUpEvent) {
            //     // Use the physical key in the key up keyEvent to find the physical key from
            //     // the corresponding key down keyEvent and remove it, even if the logical
            //     // keys don't match.
            //     _keysPressed.remove(keyEvent.physicalKey);
            // }
            // // Make sure that the modifiers reflect reality, in case a modifier key was
            // // pressed/released while the app didn't have focus.
            // _synchronizeModifiers(keyEvent);
            // if (_listeners.isEmpty) {
            //     return;
            // }
            // for (final ValueChanged<RawKeyEvent> listener in List<ValueChanged<RawKeyEvent>>.from(_listeners)) {
            //     if (_listeners.contains(listener)) {
            //         listener(keyEvent);
            //     }
            // }
        }

        
        internal void _handleKeyEvent(Event evt) {
            if (_listeners.isEmpty()) {
                return;
            }

            var keyboardEvent = RawKeyEvent.processGUIEvent(evt);
            if (keyboardEvent == null) {
                return;
            }
            
            foreach (var listener in new List<ValueChanged<RawKeyEvent>>(_listeners)) {
                if (_listeners.Contains(listener)) {
                    listener(keyboardEvent);
                }    
            }
        }
    }

    public abstract class RawKeyEvent {
        
        protected RawKeyEvent(RawKeyEventData data) {
            this.data = data;
        }

        public static RawKeyEvent processGUIEvent(Event evt) {
            if (evt == null) {
                return null;
            }

            if (evt.type == EventType.ValidateCommand) {
                var cmd = toKeyCommand(evt.commandName);
                if (cmd != null) {
                    evt.Use();
                    return null;
                }
            } else if (evt.type == EventType.ExecuteCommand) { // Validate/ExecuteCommand is editor only
                var cmd = toKeyCommand(evt.commandName);
                if (cmd != null) {
                    return new RawKeyCommandEvent(new RawKeyEventData(evt), cmd.Value);
                }
            }
    
            if (evt.type == EventType.KeyDown) {
                return new RawKeyDownEvent(new RawKeyEventData(evt));
            } else if (evt.type == EventType.KeyUp) {
                return new RawKeyUpEvent(new RawKeyEventData(evt));
            }

            return null;
        }
        
        public readonly RawKeyEventData data;

        static KeyCommand? toKeyCommand(string commandName) {
            switch (commandName) {
                case "Paste":
                    return KeyCommand.Paste;
                case "Copy":
                    return KeyCommand.Copy;
                case "SelectAll":
                    return KeyCommand.SelectAll;
                case "Cut":
                    return KeyCommand.Cut;
                    
            }

            return null;
        }
    }
    
    public class RawKeyDownEvent: RawKeyEvent {
        public RawKeyDownEvent(RawKeyEventData data) : base(data) {
        }
    }

    public class RawKeyUpEvent : RawKeyEvent {
        public RawKeyUpEvent(RawKeyEventData data) : base(data) {
        }
    }

    public class RawKeyCommandEvent : RawKeyEvent {

        public readonly KeyCommand command;
        public RawKeyCommandEvent(RawKeyEventData data, KeyCommand command) : base(data) {
            this.command = command;
        }
    }

    public enum KeyCommand {
        Copy,
        Cut,
        Paste,
        SelectAll,
    }
    
    public class RawKeyEventData {
        public readonly Event unityEvent;

        public RawKeyEventData(Event unityEvent) {
            this.unityEvent = unityEvent;
        }
    }
}