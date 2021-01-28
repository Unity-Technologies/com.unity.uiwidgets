using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public class DragDownDetails {
        public DragDownDetails(
            Offset globalPosition = null,
            Offset localPosition = null
        ) {
            this.globalPosition = globalPosition ?? Offset.zero;
            this.localPosition = localPosition ?? this.globalPosition;
        }

        public readonly Offset globalPosition;
        public readonly Offset localPosition;

        public override string ToString() {
            return GetType() + "(" + globalPosition + ")";
        }
    }

    public delegate void GestureDragDownCallback(DragDownDetails details);

    public class DragStartDetails {
        public DragStartDetails(
            TimeSpan? sourceTimeStamp = null, 
            Offset globalPosition = null,
            Offset localPosition = null
        ) {
            this.sourceTimeStamp = sourceTimeStamp;
            this.globalPosition = globalPosition ?? Offset.zero;
            this.localPosition = localPosition ?? this.globalPosition;
        }

        public readonly TimeSpan? sourceTimeStamp;

        public readonly Offset globalPosition;
        public readonly Offset localPosition;

        public override string ToString() {
            return GetType() + "(" + globalPosition + ")";
        }
    }

    public delegate void GestureDragStartCallback(DragStartDetails details);

    public class DragUpdateDetails {
        public DragUpdateDetails(
            TimeSpan? sourceTimeStamp = null,
            Offset delta = null,
            float? primaryDelta = null,
            Offset globalPosition = null,
            Offset localPosition = null,
            bool isScroll = false
            )
        {
            this.sourceTimeStamp = sourceTimeStamp;
            this.delta = delta ?? Offset.zero;
            this.primaryDelta = primaryDelta;
            this.globalPosition = globalPosition ?? Offset.zero;
            this.localPosition = localPosition ?? this.globalPosition;
            this.isScroll = isScroll;
            D.assert(primaryDelta == null
                     || primaryDelta == this.delta.dx && this.delta.dy == 0.0
                     || primaryDelta == this.delta.dy && this.delta.dx == 0.0);
        }

        public readonly TimeSpan? sourceTimeStamp;

        public readonly Offset delta;

        public readonly float? primaryDelta;

        public readonly Offset globalPosition;
        public readonly Offset localPosition;

        public readonly bool isScroll;

        public override string ToString() {
            return GetType() + "(" + delta + ")";
        }
    }

    public delegate void GestureDragUpdateCallback(DragUpdateDetails details);

    public class DragEndDetails {
        public DragEndDetails(
            Velocity velocity = null,
            float? primaryVelocity = null
        ) {
            this.velocity = velocity ?? Velocity.zero;
            this.primaryVelocity = primaryVelocity;

            D.assert(primaryVelocity == null
                     || primaryVelocity == this.velocity.pixelsPerSecond.dx
                     || primaryVelocity == this.velocity.pixelsPerSecond.dy);
        }

        public readonly Velocity velocity;

        public readonly float? primaryVelocity;

        public override string ToString() {
            return GetType() + "(" + velocity + ")";
        }
    }
}