using System.Collections.Generic;
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
            
        }
        
        readonly List<ValueChanged<RawKeyEvent>> _listeners = new List<ValueChanged<RawKeyEvent>>();
        
        public void addListener(ValueChanged<RawKeyEvent> listener) {
            _listeners.Add(listener);
        }
        
        public void removeListener(ValueChanged<RawKeyEvent> listener) {
            _listeners.Remove(listener);
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