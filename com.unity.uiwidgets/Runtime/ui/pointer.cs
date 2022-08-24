using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.ui{
    public enum PointerChange {
        cancel,
        add,
        remove,
        hover,
        down,
        move,
        up,
        kMouseDown,
        kMouseUp,
    }

    public enum PointerDeviceKind {
        touch = 0,
        mouse = 1,
        stylus,
        invertedStylus,
        keyboard = 4,
        unknown,
    }

    public enum PointerSignalKind {
        none,
        scroll,
        editorDragMove,
        editorDragRelease,
        unknown,
    }

    public enum FunctionKey {
        none = 0,
        shift = 1,
        alt = 2,
        command = 3,
        control = 4,
    }

    public readonly struct PointerData {
        public PointerData(
            TimeSpan? timeStamp = null,
            PointerChange change = PointerChange.cancel,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            PointerSignalKind signalKind = PointerSignalKind.none,
            int device = 0,
            int pointerIdentifier = 0,
            float physicalX = 0.0f,
            float physicalY = 0.0f,
            float physicalDeltaX = 0.0f,
            float physicalDeltaY = 0.0f,
            int buttons = 0,
            int modifier = 0,
            bool obscured = false,
            bool synthesized = false,
            float pressure = 0.0f,
            float pressureMin = 0.0f,
            float pressureMax = 0.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            int platformData = 0,
            float scrollDeltaX = 0.0f,
            float scrollDeltaY = 0.0f) {
            this.timeStamp = timeStamp ?? TimeSpan.Zero;
            this.change = change;
            this.kind = kind;
            this.signalKind = signalKind;
            this.device = device;
            this.pointerIdentifier = pointerIdentifier;
            this.physicalX = physicalX;
            this.physicalY = physicalY;
            this.physicalDeltaX = physicalDeltaX;
            this.physicalDeltaY = physicalDeltaY;
            this.buttons = buttons;
            this.modifier = modifier;
            this.obscured = obscured;
            this.synthesized = synthesized;
            this.pressure = pressure;
            this.pressureMin = pressureMin;
            this.pressureMax = pressureMax;
            this.distance = distance;
            this.distanceMax = distanceMax;
            this.size = size;
            this.radiusMajor = radiusMajor;
            this.radiusMinor = radiusMinor;
            this.radiusMin = radiusMin;
            this.radiusMax = radiusMax;
            this.orientation = orientation;
            this.tilt = tilt;
            this.platformData = platformData;
            this.scrollDeltaX = scrollDeltaX;
            this.scrollDeltaY = scrollDeltaY;
        }

        public readonly TimeSpan timeStamp;
        public readonly PointerChange change;
        public readonly PointerDeviceKind kind;
        public readonly PointerSignalKind signalKind;
        public readonly int device;
        public readonly int pointerIdentifier;
        public readonly float physicalX;
        public readonly float physicalY;
        public readonly float physicalDeltaX;
        public readonly float physicalDeltaY;
        public readonly int buttons;
        public readonly int modifier;
        public readonly bool obscured;
        public readonly bool synthesized;
        public readonly float pressure;
        public readonly float pressureMin;
        public readonly float pressureMax;
        public readonly float distance;
        public readonly float distanceMax;
        public readonly float size;
        public readonly float radiusMajor;
        public readonly float radiusMinor;
        public readonly float radiusMin;
        public readonly float radiusMax;
        public readonly float orientation;
        public readonly float tilt;
        public readonly int platformData;
        public readonly float scrollDeltaX;
        public readonly float scrollDeltaY;

        public override string ToString() => $"PointerData(x: {physicalX}, y: {physicalY})";
    }

    // public class ScrollData : PointerData {
    //     public ScrollData(
    //         TimeSpan timeStamp,
    //         PointerChange change,
    //         PointerDeviceKind kind,
    //         PointerSignalKind signalKind = PointerSignalKind.none,
    //         int device = 0,
    //         float physicalX = 0.0f,
    //         float physicalY = 0.0f,
    //         float scrollX = 0.0f,
    //         float scrollY = 0.0f) : base(timeStamp, change, kind, signalKind, device, physicalX, physicalY) {
    //         this.scrollX = scrollX;
    //         this.scrollY = scrollY;
    //     }
    //
    //     public float scrollX;
    //     public float scrollY;
    // }

    public struct PointerDataPacket {
        public PointerDataPacket(List<PointerData> data = null) {
            this.data = data ?? new List<PointerData>();
        }

        public readonly List<PointerData> data;
    }
}