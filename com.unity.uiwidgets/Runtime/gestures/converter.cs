using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.service;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public partial class gesture_ {
        internal static int _synthesiseDownButtons(int buttons, PointerDeviceKind kind) {
            switch (kind) {
                case PointerDeviceKind.mouse:
                    return buttons;
                case PointerDeviceKind.touch:
                case PointerDeviceKind.stylus:
                case PointerDeviceKind.invertedStylus:
                    return buttons | kPrimaryButton;
                default:
                    // We have no information about the device but we know we never want
                    // buttons to be 0 when the pointer is down.
                    return buttons == 0 ? kPrimaryButton : buttons;
            }
        }
    }


    public static class PointerEventConverter {
        public static IEnumerable<PointerEvent> expand(IEnumerable<PointerData> data, float devicePixelRatio) {
            foreach (PointerData datum in data) {
                var position = new Offset(datum.physicalX, datum.physicalY) / devicePixelRatio;
                Offset delta = new Offset(datum.physicalDeltaX, datum.physicalDeltaY) / devicePixelRatio;
                var radiusMinor = _toLogicalPixels(datum.radiusMinor, devicePixelRatio);
                var radiusMajor = _toLogicalPixels(datum.radiusMajor, devicePixelRatio);
                var radiusMin = _toLogicalPixels(datum.radiusMin, devicePixelRatio);
                var radiusMax = _toLogicalPixels(datum.radiusMax, devicePixelRatio);
                var timeStamp = datum.timeStamp;
                var kind = datum.kind;
                // TODO: datum.signalKind is not nullable, "else" could not be reached
                if (datum.signalKind == ui.PointerSignalKind.none) {
                    switch (datum.change) {
                        case PointerChange.add: {
                            yield return new PointerAddedEvent(
                                timeStamp: timeStamp,
                                kind: kind,
                                device: datum.device,
                                position: position,
                                obscured: datum.obscured,
                                pressureMin: datum.pressureMin,
                                pressureMax: datum.pressureMax,
                                distance: datum.distance,
                                distanceMax: datum.distanceMax,
                                radiusMin: radiusMin,
                                radiusMax: radiusMax,
                                orientation: datum.orientation,
                                tilt: datum.tilt
                            );
                            break;
                        }

                        case PointerChange.hover: {
                            yield return new PointerHoverEvent(
                                timeStamp: timeStamp,
                                kind: kind,
                                device: datum.device,
                                position: position,
                                delta: delta,
                                buttons: datum.buttons,
                                obscured: datum.obscured,
                                pressureMin: datum.pressureMin,
                                pressureMax: datum.pressureMax,
                                distance: datum.distance,
                                distanceMax: datum.distanceMax,
                                size: datum.size,
                                radiusMajor: radiusMajor,
                                radiusMinor: radiusMinor,
                                radiusMin: radiusMin,
                                radiusMax: radiusMax,
                                orientation: datum.orientation,
                                tilt: datum.tilt,
                                synthesized: datum.synthesized
                            );
                            break;
                        }
                        case PointerChange.down: {
                            yield return new PointerDownEvent(
                                timeStamp: timeStamp,
                                pointer: datum.pointerIdentifier,
                                kind: kind,
                                device: datum.device,
                                position: position,
                                buttons: gesture_._synthesiseDownButtons(datum.buttons, kind),
                                obscured: datum.obscured,
                                pressure: datum.pressure,
                                pressureMin: datum.pressureMin,
                                pressureMax: datum.pressureMax,
                                distanceMax: datum.distanceMax,
                                size: datum.size,
                                radiusMajor: radiusMajor,
                                radiusMinor: radiusMinor,
                                radiusMin: radiusMin,
                                radiusMax: radiusMax,
                                orientation: datum.orientation,
                                tilt: datum.tilt
                            );
                        }
                            break;

                        case PointerChange.move: {
                            yield return new PointerMoveEvent(
                                timeStamp: timeStamp,
                                pointer: datum.pointerIdentifier,
                                kind: kind,
                                device: datum.device,
                                position: position,
                                delta: delta,
                                buttons: gesture_._synthesiseDownButtons(datum.buttons, kind),
                                obscured: datum.obscured,
                                pressure: datum.pressure,
                                pressureMin: datum.pressureMin,
                                pressureMax: datum.pressureMax,
                                distanceMax: datum.distanceMax,
                                size: datum.size,
                                radiusMajor: radiusMajor,
                                radiusMinor: radiusMinor,
                                radiusMin: radiusMin,
                                radiusMax: radiusMax,
                                orientation: datum.orientation,
                                tilt: datum.tilt,
                                platformData: datum.platformData,
                                synthesized: datum.synthesized
                            );
                        }
                            break;
                        case PointerChange.up:
                            yield return new PointerUpEvent(
                                timeStamp: timeStamp,
                                pointer: datum.pointerIdentifier,
                                kind: kind,
                                device: datum.device,
                                position: position,
                                buttons: datum.buttons,
                                obscured: datum.obscured,
                                pressure: datum.pressure,
                                pressureMin: datum.pressureMin,
                                pressureMax: datum.pressureMax,
                                distance: datum.distance,
                                distanceMax: datum.distanceMax,
                                size: datum.size,
                                radiusMajor: radiusMajor,
                                radiusMinor: radiusMinor,
                                radiusMin: radiusMin,
                                radiusMax: radiusMax,
                                orientation: datum.orientation,
                                tilt: datum.tilt
                            );
                            break;
                        case PointerChange.cancel: {
                            yield return new PointerCancelEvent(
                                timeStamp: timeStamp,
                                pointer: datum.pointerIdentifier,
                                kind: kind,
                                device: datum.device,
                                position: position,
                                buttons: datum.buttons,
                                obscured: datum.obscured,
                                pressureMin: datum.pressureMin,
                                pressureMax: datum.pressureMax,
                                distance: datum.distance,
                                distanceMax: datum.distanceMax,
                                size: datum.size,
                                radiusMajor: radiusMajor,
                                radiusMinor: radiusMinor,
                                radiusMin: radiusMin,
                                radiusMax: radiusMax,
                                orientation: datum.orientation,
                                tilt: datum.tilt
                            );
                        }
                            break;
                        case ui.PointerChange.remove:
                            yield return new PointerRemovedEvent(
                                timeStamp: timeStamp,
                                kind: kind,
                                device: datum.device,
                                position: position,
                                obscured: datum.obscured,
                                pressureMin: datum.pressureMin,
                                pressureMax: datum.pressureMax,
                                distanceMax: datum.distanceMax,
                                radiusMin: radiusMin,
                                radiusMax: radiusMax
                            );
                            break;
                        default:
                        //TODO: PUT KEYBOARD TO A PROPRER POSITION
                            if (datum.kind == PointerDeviceKind.keyboard) {
                                var keyBoardEvent = new Event();
                                if (datum.change == PointerChange.kMouseDown) {
                                    keyBoardEvent.type = EventType.KeyDown;
                                }else if (datum.change == PointerChange.kMouseUp) {
                                    keyBoardEvent.type = EventType.KeyUp;
                                }
                                keyBoardEvent.keyCode = (KeyCode)datum.buttons;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                                keyBoardEvent.shift = (datum.modifier & (1 << (int) FunctionKey.shift)) > 0;
                                keyBoardEvent.alt = (datum.modifier & (1 << (int) FunctionKey.alt)) > 0;
                                keyBoardEvent.command = (datum.modifier & (1 << (int) FunctionKey.command)) > 0;
                                keyBoardEvent.control = (datum.modifier & (1 << (int) FunctionKey.control)) > 0;
#endif
                                TextInput.OnGUI();
                                RawKeyboard.instance._handleKeyEvent(keyBoardEvent);
                            }
                            break;
                    }
                }
                else {
                    switch (datum.signalKind) {
                        case ui.PointerSignalKind.scroll:
                            Offset scrollDelta =
                                new Offset(datum.scrollDeltaX, datum.scrollDeltaY) / devicePixelRatio;
                            yield return new PointerScrollEvent(
                                timeStamp: timeStamp,
                                kind: kind,
                                device: datum.device,
                                position: position,
                                scrollDelta: scrollDelta
                            );
                            break;
                        case ui.PointerSignalKind.none:
                            D.assert(false); // This branch should already have 'none' filtered out.
                            break;
                        case ui.PointerSignalKind.editorDragMove: {
                            yield return new PointerDragFromEditorHoverEvent(
                                timeStamp: timeStamp,
                                pointer: datum.pointerIdentifier,
                                kind: kind,
                                device: datum.device,
                                position: position
                            );
                            break;
                        }
                        case ui.PointerSignalKind.editorDragRelease: {
                            yield return new PointerDragFromEditorReleaseEvent(
                                timeStamp: timeStamp,
                                pointer: datum.pointerIdentifier,
                                kind: kind,
                                device: datum.device,
                                position: position
                            );
                            break;
                        }
                        case ui.PointerSignalKind.unknown:
                            // Ignore unknown signals.
                            break;
                    }
                }
            }
        }

        public static Queue<Event> KeyEvent = new Queue<Event>();
        static float _toLogicalPixels(float physicalPixels, float devicePixelRatio) {
            return physicalPixels / devicePixelRatio;
        }
    }
}