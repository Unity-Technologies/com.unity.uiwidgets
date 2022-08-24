using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.UIWidgets.gestures {
    public partial class gesture_ {
        public const int kPrimaryButton = 0x01;

        public const int kSecondaryButton = 0x02;

        public const int kPrimaryMouseButton = kPrimaryButton;

        public const int kSecondaryMouseButton = kSecondaryButton;

        public const int kStylusContact = kPrimaryButton;

        public const int kPrimaryStylusButton = kSecondaryButton;

        public const int kMiddleMouseButton = 0x04;

        public const int kSecondaryStylusButton = 0x04;

        public const int kBackMouseButton = 0x08;

        public const int kForwardMouseButton = 0x10;

        public const int kTouchContact = kPrimaryButton;

        public static int kMaxUnsignedSMI {
            get {
                Debug.LogError("Update this for io and web");
                return -1;
            }
        }

        public int nthMouseButton(int number) => (kPrimaryMouseButton << (number - 1)) & kMaxUnsignedSMI;

        public int nthStylusButton(int number) => (kPrimaryStylusButton << (number - 1)) & kMaxUnsignedSMI;

        public int smallestButton(int buttons) => buttons & (-buttons);

        public bool isSingleButton(int buttons) => buttons != 0 && (smallestButton(buttons) == buttons);
    }

    public abstract class PointerEvent : Diagnosticable {
        public PointerEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            Offset delta = null,
            Offset localDelta = null,
            int buttons = 0,
            bool down = false,
            bool obscured = false,
            float pressure = 1.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
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
            bool synthesized = false,
            Matrix4 transform = null,
            PointerEvent original = null
        ) {
            this.timeStamp = timeStamp;
            this.pointer = pointer;
            this.kind = kind;
            this.device = device;
            this.position = position ?? Offset.zero;
            this.localPosition = localPosition ?? this.position;
            this.delta = delta ?? Offset.zero;
            this.localDelta = localDelta ?? this.delta;
            this.buttons = buttons;
            this.down = down;
            this.obscured = obscured;
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
            this.synthesized = synthesized;
            this.transform = transform;
            this.original = original;
        }

        public readonly TimeSpan timeStamp;

        public readonly int pointer;

        public PointerDeviceKind kind;

        public int device;

        public readonly Offset position;

        public readonly Offset localPosition;

        public readonly Offset delta;

        public readonly Offset localDelta;

        public readonly int buttons;

        public readonly bool down;

        public readonly bool obscured;

        public readonly float pressure;

        public readonly float pressureMin;

        public readonly float pressureMax;

        public readonly float distance;

        public float distanceMin {
            get { return 0.0f; }
        }

        public readonly float distanceMax;

        public readonly float size;

        public readonly float radiusMajor;

        public readonly float radiusMinor;

        public readonly float radiusMin;

        public readonly float radiusMax;

        public readonly float orientation;

        public readonly float tilt;

        public readonly int platformData;

        public readonly bool synthesized;

        public readonly Matrix4 transform;

        public readonly PointerEvent original;

        public abstract PointerEvent transformed(Matrix4 transform);

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Offset>("position", position));
            properties.add(new DiagnosticsProperty<Offset>("localPosition", localPosition));
            properties.add(new DiagnosticsProperty<Offset>("delta", delta, defaultValue: Offset.zero,
                level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Offset>("localDelta", localDelta, defaultValue: Offset.zero,
                level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<TimeSpan>("timeStamp", timeStamp, defaultValue: TimeSpan.Zero,
                level: DiagnosticLevel.debug));
            properties.add(new IntProperty("pointer", pointer, level: DiagnosticLevel.debug));
            properties.add(new EnumProperty<PointerDeviceKind>("kind", kind, level: DiagnosticLevel.debug));
            properties.add(new IntProperty("device", device, defaultValue: 0, level: DiagnosticLevel.debug));
            properties.add(new IntProperty("buttons", buttons, defaultValue: 0, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<bool>("down", down, level: DiagnosticLevel.debug));
            properties.add(
                new FloatProperty("pressure", pressure, defaultValue: 1.0f, level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("pressureMin", pressureMin, defaultValue: 1.0f,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("pressureMax", pressureMax, defaultValue: 1.0f,
                level: DiagnosticLevel.debug));
            properties.add(
                new FloatProperty("distance", distance, defaultValue: 0.0f, level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("distanceMin", distanceMin, defaultValue: 0.0f,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("distanceMax", distanceMax, defaultValue: 0.0f,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("size", size, defaultValue: 0.0f, level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("radiusMajor", radiusMajor, defaultValue: 0.0f,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("radiusMinor", radiusMinor, defaultValue: 0.0f,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("radiusMin", radiusMin, defaultValue: 0.0f,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("radiusMax", radiusMax, defaultValue: 0.0f,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("orientation", orientation, defaultValue: 0.0f,
                level: DiagnosticLevel.debug));
            properties.add(new FloatProperty("tilt", tilt, defaultValue: 0.0f, level: DiagnosticLevel.debug));
            properties.add(new IntProperty("platformData", platformData, defaultValue: 0,
                level: DiagnosticLevel.debug));
            properties.add(new FlagProperty("obscured", value: obscured, ifTrue: "obscured",
                level: DiagnosticLevel.debug));
            properties.add(new FlagProperty("synthesized", value: synthesized, ifTrue: "synthesized",
                level: DiagnosticLevel.debug));
        }

        public static Offset transformPosition(Matrix4 transform, Offset position) {
            if (transform == null) {
                return position;
            }

            Vector3 position3 = new Vector3(position.dx, position.dy, 0.0f);
            Vector3 transformed3 = transform.perspectiveTransform(position3);
            return new Offset(transformed3.x, transformed3.y);
        }

        public static Offset transformDeltaViaPositions(
            Offset untransformedEndPosition,
            Offset untransformedDelta,
            Matrix4 transform,
            Offset transformedEndPosition = null
        ) {
            if (transform == null) {
                return untransformedDelta;
            }

            transformedEndPosition = transformedEndPosition ?? transformPosition(transform, untransformedEndPosition);
            Offset transformedStartPosition =
                transformPosition(transform, untransformedEndPosition - untransformedDelta);
            return transformedEndPosition - transformedStartPosition;
        }

        public static Matrix4 removePerspectiveTransform(Matrix4 transform) {
            Vector4 vector = new Vector4(0, 0, 1, 0);
            var result = transform.clone();
            result.setColumn(2, vector);
            result.setRow(2, vector);
            return result;
        }
    }

    public class PointerAddedEvent : PointerEvent {
        public PointerAddedEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            bool obscured = false,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            Matrix4 transform = null,
            PointerAddedEvent original = null
        ) : base(
            timeStamp: timeStamp,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            obscured: obscured,
            pressure: 0,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            transform: transform,
            original: original
        ) {
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }

            return new PointerAddedEvent(
                timeStamp: timeStamp,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformPosition(transform, position),
                obscured: obscured,
                pressureMin: pressureMin,
                pressureMax: pressureMax,
                distance: distance,
                distanceMax: distanceMax,
                radiusMin: radiusMin,
                radiusMax: radiusMax,
                orientation: orientation,
                tilt: tilt,
                transform: this.transform,
                original: original as PointerAddedEvent ?? this
            );
        }
    }

    public class PointerRemovedEvent : PointerEvent {
        public PointerRemovedEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            bool obscured = false,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distanceMax = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            Matrix4 transform = null,
            PointerRemovedEvent original = null
        ) : base(
            timeStamp: timeStamp,
            kind: kind,
            position: position,
            localPosition: localPosition,
            device: device,
            obscured: obscured,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            transform: transform,
            original: original
        ) {
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }

            return new PointerRemovedEvent(
                timeStamp: timeStamp,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformPosition(transform, position),
                obscured: obscured,
                pressureMin: pressureMin,
                pressureMax: pressureMax,
                distanceMax: distanceMax,
                radiusMin: radiusMin,
                radiusMax: radiusMax,
                transform: transform,
                original: original as PointerRemovedEvent ?? this
            );
        }
    }

    public class PointerHoverEvent : PointerEvent {
        public PointerHoverEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            Offset delta = null,
            Offset localDelta = null,
            int buttons = 0,
            bool obscured = false,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            bool synthesized = false,
            Matrix4 transform = null,
            PointerHoverEvent original = null) : base(
            timeStamp: timeStamp,
            kind: kind,
            device: device,
            position: position,
            localPosition: localDelta,
            delta: delta,
            localDelta: localDelta,
            buttons: buttons,
            obscured: obscured,
            pressure: 0,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            synthesized: synthesized,
            transform: transform,
            original: original) {
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }

            Offset transformedPosition = transformPosition(transform, position);
            return new PointerHoverEvent(
                timeStamp: timeStamp,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformedPosition,
                delta: delta,
                localDelta: transformDeltaViaPositions(
                    transform: transform,
                    untransformedDelta: delta,
                    untransformedEndPosition: position,
                    transformedEndPosition: transformedPosition
                ),
                buttons: buttons,
                obscured: obscured,
                pressureMin: pressureMin,
                pressureMax: pressureMax,
                distance: distance,
                distanceMax: distanceMax,
                size: size,
                radiusMajor: radiusMajor,
                radiusMinor: radiusMinor,
                radiusMin: radiusMin,
                radiusMax: radiusMax,
                orientation: orientation,
                tilt: tilt,
                synthesized: synthesized,
                transform: transform,
                original: original as PointerHoverEvent ?? this);
        }
    }

    public class PointerEnterEvent : PointerEvent {
        public PointerEnterEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            Offset delta = null,
            Offset localDelta = null,
            int buttons = 0,
            bool obscured = false,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            bool synthesized = false,
            bool down = false,
            Matrix4 transform = null,
            PointerEnterEvent original = null) : base(
            timeStamp: timeStamp,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            delta: delta,
            localDelta: localDelta,
            buttons: buttons,
            down: down,
            obscured: obscured,
            pressure: 0,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            synthesized: synthesized,
            transform: transform,
            original: original) {
        }

        public static PointerEnterEvent fromHoverEvent(PointerHoverEvent e) {
            return fromMouseEvent(e);
        }

        public static PointerEnterEvent fromMouseEvent(PointerEvent hover) {
            return new PointerEnterEvent(
                timeStamp: hover?.timeStamp ?? TimeSpan.Zero,
                kind: hover?.kind ?? PointerDeviceKind.touch,
                device: hover?.device ?? 0,
                position: hover?.position,
                localPosition: hover?.localPosition,
                delta: hover?.delta,
                localDelta: hover?.localDelta,
                buttons: hover?.buttons ?? 0,
                down: hover?.down ?? false,
                obscured: hover?.obscured ?? false,
                pressureMin: hover?.pressureMin ?? 1.0f,
                pressureMax: hover?.pressureMax ?? 1.0f,
                distance: hover?.distance ?? 0.0f,
                distanceMax: hover?.distanceMax ?? 0.0f,
                size: hover?.size ?? 0.0f,
                radiusMajor: hover?.radiusMajor ?? 0.0f,
                radiusMinor: hover?.radiusMinor ?? 0.0f,
                radiusMin: hover?.radiusMin ?? 0.0f,
                radiusMax: hover?.radiusMax ?? 0.0f,
                orientation: hover?.orientation ?? 0.0f,
                tilt: hover?.tilt ?? 0.0f,
                synthesized: hover?.synthesized ?? false,
                transform: hover?.transform,
                original: hover?.original as PointerEnterEvent
            );
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }

            Offset transformedPosition = transformPosition(transform, position);
            return new PointerEnterEvent(
                timeStamp: timeStamp,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformedPosition,
                delta: delta,
                localDelta: transformDeltaViaPositions(
                    transform: transform,
                    untransformedDelta: delta,
                    untransformedEndPosition: position,
                    transformedEndPosition: transformedPosition
                ),
                buttons: buttons,
                obscured: obscured,
                pressureMin: pressureMin,
                pressureMax: pressureMax,
                distance: distance,
                distanceMax: distanceMax,
                size: size,
                radiusMajor: radiusMajor,
                radiusMinor: radiusMinor,
                radiusMin: radiusMin,
                radiusMax: radiusMax,
                orientation: orientation,
                tilt: tilt,
                down: down,
                synthesized: synthesized,
                transform: transform,
                original: original as PointerEnterEvent ?? this
            );
        }
    }

    public class PointerExitEvent : PointerEvent {
        public PointerExitEvent(
            TimeSpan timeStamp,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            Offset delta = null,
            Offset localDelta = null,
            int buttons = 0,
            bool obscured = false,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            bool synthesized = false,
            bool down = false,
            Matrix4 transform = null,
            PointerExitEvent original = null) : base(
            timeStamp: timeStamp,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            delta: delta,
            localDelta: localDelta,
            buttons: buttons,
            down: down,
            obscured: obscured,
            pressure: 0,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            synthesized: synthesized,
            transform: transform,
            original: original) {
        }


        public static PointerExitEvent fromHoverEvent(PointerHoverEvent e) {
            return fromMouseEvent(e);
        }

        public static PointerExitEvent fromMouseEvent(PointerEvent hover) {
            return new PointerExitEvent(
                timeStamp: hover?.timeStamp ?? TimeSpan.Zero,
                kind: hover?.kind ?? PointerDeviceKind.touch,
                device: hover?.device ?? 0,
                position: hover?.position,
                localPosition: hover?.localPosition,
                delta: hover?.delta,
                localDelta: hover?.localDelta,
                buttons: hover?.buttons ?? 0,
                down: hover?.down ?? false,
                obscured: hover?.obscured ?? false,
                pressureMin: hover?.pressureMin ?? 1.0f,
                pressureMax: hover?.pressureMax ?? 1.0f,
                distance: hover?.distance ?? 0.0f,
                distanceMax: hover?.distanceMax ?? 0.0f,
                size: hover?.size ?? 0.0f,
                radiusMajor: hover?.radiusMajor ?? 0.0f,
                radiusMinor: hover?.radiusMinor ?? 0.0f,
                radiusMin: hover?.radiusMin ?? 0.0f,
                radiusMax: hover?.radiusMax ?? 0.0f,
                orientation: hover?.orientation ?? 0.0f,
                tilt: hover?.tilt ?? 0.0f,
                synthesized: hover?.synthesized ?? false,
                transform: hover?.transform,
                original: hover?.original as PointerExitEvent
            );
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }

            Offset transformedPosition = transformPosition(transform, position);
            return new PointerExitEvent(
                timeStamp: timeStamp,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformedPosition,
                delta: delta,
                localDelta: transformDeltaViaPositions(
                    transform: transform,
                    untransformedDelta: delta,
                    untransformedEndPosition: position,
                    transformedEndPosition: transformedPosition
                ),
                buttons: buttons,
                obscured: obscured,
                pressureMin: pressureMin,
                pressureMax: pressureMax,
                distance: distance,
                distanceMax: distanceMax,
                size: size,
                radiusMajor: radiusMajor,
                radiusMinor: radiusMinor,
                radiusMin: radiusMin,
                radiusMax: radiusMax,
                orientation: orientation,
                tilt: tilt,
                down: down,
                synthesized: synthesized,
                transform: transform,
                original: original as PointerExitEvent ?? this
            );
        }
    }

    public class PointerDownEvent : PointerEvent {
        public PointerDownEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            int buttons = gesture_.kPrimaryButton,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            Matrix4 transform = null,
            PointerDownEvent original = null
        ) : base(
            timeStamp: timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            buttons: buttons,
            down: true,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            transform: transform,
            original: original) {
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }

            return new PointerDownEvent(
                timeStamp: timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformPosition(transform, position),
                buttons: buttons,
                obscured: obscured,
                pressure: pressure,
                pressureMin: pressureMin,
                pressureMax: pressureMax,
                distanceMax: distanceMax,
                size: size,
                radiusMajor: radiusMajor,
                radiusMinor: radiusMinor,
                radiusMin: radiusMin,
                radiusMax: radiusMax,
                orientation: orientation,
                tilt: tilt,
                transform: transform,
                original: original as PointerDownEvent ?? this
            );
        }
    }

    public class PointerMoveEvent : PointerEvent {
        public PointerMoveEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            Offset delta = null,
            Offset localDelta = null,
            int buttons = gesture_.kPrimaryButton,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
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
            bool synthesized = false,
            Matrix4 transform = null,
            PointerMoveEvent original = null
        ) : base(
            timeStamp: timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            delta: delta,
            localDelta: localDelta,
            buttons: buttons,
            down: true,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            platformData: platformData,
            synthesized: synthesized,
            transform: transform,
            original: original) {
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }
            Offset transformedPosition = transformPosition(transform, position);

            return new PointerMoveEvent(
                timeStamp: timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformedPosition,
                delta: delta,
                localDelta: transformDeltaViaPositions(
                    transform: transform,
                    untransformedDelta: delta,
                    untransformedEndPosition: position,
                    transformedEndPosition: transformedPosition
                ),
                buttons: buttons,
                obscured: obscured,
                pressure: pressure,
                pressureMin: pressureMin,
                pressureMax: pressureMax,
                distanceMax: distanceMax,
                size: size,
                radiusMajor: radiusMajor,
                radiusMinor: radiusMinor,
                radiusMin: radiusMin,
                radiusMax: radiusMax,
                orientation: orientation,
                tilt: tilt,
                platformData: platformData,
                synthesized: synthesized,
                transform: transform,
                original: original as PointerMoveEvent ?? this
            );
        }
    }

    public class PointerUpEvent : PointerEvent {
        public PointerUpEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            int buttons = 0,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            Matrix4 transform = null,
            PointerUpEvent original = null
        ) : base(
            timeStamp: timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            buttons: buttons,
            down: false,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            transform: transform,
            original: original) {
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }
            return new PointerUpEvent(
                timeStamp: timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformPosition(transform, position),
                buttons: buttons,
                obscured: obscured,
                pressure: pressure,
                pressureMin: pressureMin,
                pressureMax: pressureMax,
                distance: distance,
                distanceMax: distanceMax,
                size: size,
                radiusMajor: radiusMajor,
                radiusMinor: radiusMinor,
                radiusMin: radiusMin,
                radiusMax: radiusMax,
                orientation: orientation,
                tilt: tilt,
                transform: transform,
                original: original as PointerUpEvent ?? this
            );
        }
    }

    public abstract class PointerSignalEvent : PointerEvent {
        public PointerSignalEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            Matrix4 transform = null,
            PointerSignalEvent original = null
        ) : base(
            timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            transform: transform,
            original: original
        ) {
        }
    }

    public class PointerScrollEvent : PointerSignalEvent {
        public PointerScrollEvent(
            TimeSpan? timeStamp = null,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            Offset scrollDelta = null,
            Matrix4 transform = null,
            PointerScrollEvent original = null)
            : base(
                timeStamp ?? TimeSpan.Zero,
                kind: kind,
                device: device,
                position: position,
                localPosition: localPosition,
                transform: transform,
                original: original) {
            D.assert(position != null);
            D.assert(scrollDelta != null);
            this.scrollDelta = scrollDelta;
        }

        public readonly Offset scrollDelta;

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }

            return new PointerScrollEvent(
                timeStamp: timeStamp,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformPosition(transform, position),
                scrollDelta: scrollDelta,
                transform: transform,
                original: original as PointerScrollEvent ?? this
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Offset>("scrollDelta", scrollDelta));
        }
    }

    public class PointerCancelEvent : PointerEvent {
        public PointerCancelEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            int buttons = 0,
            bool obscured = false,
            float pressure = 0.0f,
            float pressureMin = 1.0f,
            float pressureMax = 1.0f,
            float distance = 0.0f,
            float distanceMax = 0.0f,
            float size = 0.0f,
            float radiusMajor = 0.0f,
            float radiusMinor = 0.0f,
            float radiusMin = 0.0f,
            float radiusMax = 0.0f,
            float orientation = 0.0f,
            float tilt = 0.0f,
            Matrix4 transform = null,
            PointerCancelEvent original = null
        ) : base(
            timeStamp: timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            buttons: buttons,
            down: false,
            obscured: obscured,
            pressure: pressure,
            pressureMin: pressureMin,
            pressureMax: pressureMax,
            size: size,
            radiusMajor: radiusMajor,
            radiusMinor: radiusMinor,
            distance: distance,
            distanceMax: distanceMax,
            radiusMin: radiusMin,
            radiusMax: radiusMax,
            orientation: orientation,
            tilt: tilt,
            transform: transform,
            original: original) {
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }
            return new PointerCancelEvent(
                timeStamp: timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformPosition(transform, position),
                buttons: buttons,
                obscured: obscured,
                pressureMin: pressureMin,
                pressureMax: pressureMax,
                distance: distance,
                distanceMax: distanceMax,
                size: size,
                radiusMajor: radiusMajor,
                radiusMinor: radiusMinor,
                radiusMin: radiusMin,
                radiusMax: radiusMax,
                orientation: orientation,
                tilt: tilt,
                transform: transform,
                original: original as PointerCancelEvent ?? this
            );
        }
    }

    public class PointerDragFromEditorEnterEvent : PointerEvent {
        public PointerDragFromEditorEnterEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            Matrix4 transform = null,
            Object[] objectReferences = null,
            string[] paths = null,
            PointerDragFromEditorEnterEvent original = null
        ) : base(
            timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            transform: transform,
            original: original
        ) {
            this.objectReferences = objectReferences;
            this.paths = paths;
        }
        
        public Object[] objectReferences;
        public string[] paths;

        public static PointerDragFromEditorEnterEvent fromDragFromEditorEvent(PointerEvent evt,
            Object[] objectReferences, string[] paths) {
            return new PointerDragFromEditorEnterEvent(
                timeStamp: evt.timeStamp,
                pointer: evt.pointer,
                kind: evt.kind,
                device: evt.device,
                position: evt.position,
                localPosition: evt.localPosition,
                transform: evt.transform,
                objectReferences: objectReferences,
                paths: paths,
                original: evt.original as PointerDragFromEditorEnterEvent
            );
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }
            return new PointerDragFromEditorEnterEvent(
                timeStamp: timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformPosition(transform, position),
                transform: transform,
                objectReferences: objectReferences,
                paths: paths,
                original: original as PointerDragFromEditorEnterEvent ?? this
            );
        }
    }

    public class PointerDragFromEditorExitEvent : PointerEvent {
        public PointerDragFromEditorExitEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            Matrix4 transform = null,
            PointerDragFromEditorExitEvent original = null
        ) : base(
            timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            transform: transform,
            original: original
        ) {
        }

        public static PointerDragFromEditorExitEvent fromDragFromEditorEvent(PointerEvent evt) {
            return new PointerDragFromEditorExitEvent(
                timeStamp: evt.timeStamp,
                pointer: evt.pointer,
                kind: evt.kind,
                device: evt.device,
                position: evt.position,
                localPosition: evt.localPosition,
                transform: evt.transform,
                original: evt.original as PointerDragFromEditorExitEvent
            );
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }
            return new PointerDragFromEditorExitEvent(
                timeStamp: timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformPosition(transform, position),
                transform: transform,
                original: original as PointerDragFromEditorExitEvent ?? this
            );
        }
    }

    public class PointerDragFromEditorHoverEvent : PointerEvent {
        public PointerDragFromEditorHoverEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            Matrix4 transform = null,
            PointerDragFromEditorHoverEvent original = null
        ) : base(
            timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            transform: transform,
            original: original
        ) {
        }

        public static PointerDragFromEditorHoverEvent fromDragFromEditorEvent(PointerEvent evt) {
            return new PointerDragFromEditorHoverEvent(
                timeStamp: evt.timeStamp,
                pointer: evt.pointer,
                kind: evt.kind,
                device: evt.device,
                position: evt.position,
                localPosition: evt.localPosition,
                transform: evt.transform,
                original: evt.original as PointerDragFromEditorHoverEvent
            );
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }
            return new PointerDragFromEditorHoverEvent(
                timeStamp: timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformPosition(transform, position),
                transform: this.transform,
                original: original as PointerDragFromEditorHoverEvent ?? this
            );
        }
    }

    public class PointerDragFromEditorReleaseEvent : PointerEvent {
        public PointerDragFromEditorReleaseEvent(
            TimeSpan timeStamp,
            int pointer = 0,
            PointerDeviceKind kind = PointerDeviceKind.mouse,
            int device = 0,
            Offset position = null,
            Offset localPosition = null,
            Object[] objectReferences = null,
            string[] paths = null,
            Matrix4 transform = null,
            PointerDragFromEditorReleaseEvent original = null
        ) : base(
            timeStamp,
            pointer: pointer,
            kind: kind,
            device: device,
            position: position,
            localPosition: localPosition,
            transform: transform,
            original: original
        ) {
            this.objectReferences = objectReferences;
            this.paths = paths;
        }

        public Object[] objectReferences;
        public string[] paths;

        public static PointerDragFromEditorReleaseEvent fromDragFromEditorEvent(PointerEvent evt,
            Object[] objectReferences, string[] paths) {
            return new PointerDragFromEditorReleaseEvent(
                timeStamp: evt.timeStamp,
                pointer: evt.pointer,
                kind: evt.kind,
                device: evt.device,
                position: evt.position,
                localPosition: evt.localPosition,
                objectReferences: objectReferences,
                paths: paths,
                transform: evt.transform,
                original: evt.original as PointerDragFromEditorReleaseEvent
            );
        }

        public override PointerEvent transformed(Matrix4 transform) {
            if (transform == null || transform == this.transform) {
                return this;
            }
            return new PointerDragFromEditorReleaseEvent(
                timeStamp: timeStamp,
                pointer: pointer,
                kind: kind,
                device: device,
                position: position,
                localPosition: transformPosition(transform, position),
                objectReferences: objectReferences,
                transform: this.transform,
                original: original as PointerDragFromEditorReleaseEvent ?? this
            );
        }
    }
}