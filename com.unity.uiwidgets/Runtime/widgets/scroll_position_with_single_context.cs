using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class ScrollPositionWithSingleContext : ScrollPosition, ScrollActivityDelegate {
        public ScrollPositionWithSingleContext(
            ScrollPhysics physics = null,
            ScrollContext context = null,
            float? initialPixels = 0.0f,
            bool keepScrollOffset = true,
            ScrollPosition oldPosition = null,
            string debugLabel = null
        ) : base(
            physics: physics,
            context: context,
            keepScrollOffset: keepScrollOffset,
            oldPosition: oldPosition,
            debugLabel: debugLabel
        ) {
            if (_pixels == null && initialPixels != null) {
                correctPixels(initialPixels.Value);
            }

            if (activity == null) {
                goIdle();
            }

            D.assert(activity != null);
        }


        float _heldPreviousVelocity = 0.0f;

        public override AxisDirection axisDirection {
            get { return context.axisDirection; }
        }

        public override float setPixels(float newPixels) {
            D.assert(activity.isScrolling);
            return base.setPixels(newPixels);
        }

        protected override void absorb(ScrollPosition other) {
            base.absorb(other);
            if (!(other is ScrollPositionWithSingleContext)) {
                goIdle();
                return;
            }

            activity.updateDelegate(this);
            ScrollPositionWithSingleContext typedOther = (ScrollPositionWithSingleContext) other;
            _userScrollDirection = typedOther._userScrollDirection;
            D.assert(_currentDrag == null);
            if (typedOther._currentDrag != null) {
                _currentDrag = typedOther._currentDrag;
                _currentDrag.updateDelegate(this);
                typedOther._currentDrag = null;
            }
        }

        protected override void applyNewDimensions() {
            base.applyNewDimensions();
            context.setCanDrag(physics.shouldAcceptUserOffset(this));
        }

        public override void beginActivity(ScrollActivity newActivity) {
            _heldPreviousVelocity = 0.0f;
            if (newActivity == null) {
                return;
            }

            D.assert(newActivity.del == this);
            base.beginActivity(newActivity);
            if (_currentDrag != null) {
                _currentDrag.dispose();
                _currentDrag = null;
            }

            if (!activity.isScrolling) {
                updateUserScrollDirection(ScrollDirection.idle);
            }
        }

        public virtual void applyUserScrollOffset(float delta) {
            updateUserScrollDirection(delta > 0.0 ? ScrollDirection.forward : ScrollDirection.reverse);

            var pixel = pixels - physics.applyPhysicsToUserOffset(this, delta);
            if (pixel < minScrollExtent) {
                pixel = minScrollExtent;
            }

            if (pixel > maxScrollExtent) {
                pixel = maxScrollExtent;
            }

            setPixels(pixel);
        }

        public virtual void applyUserOffset(float delta) {
            updateUserScrollDirection(delta > 0.0 ? ScrollDirection.forward : ScrollDirection.reverse);
            setPixels(pixels - physics.applyPhysicsToUserOffset(this, delta));
        }

        public void goIdle() {
            beginActivity(new IdleScrollActivity(this));
        }

        public virtual void goBallistic(float velocity) {
            D.assert(_pixels != null);
            Simulation simulation = physics.createBallisticSimulation(this, velocity);
            if (simulation != null) {
                beginActivity(new BallisticScrollActivity(this, simulation, context.vsync));
            }
            else {
                goIdle();
            }
        }

        public override ScrollDirection userScrollDirection {
            get { return _userScrollDirection; }
        }

        ScrollDirection _userScrollDirection = ScrollDirection.idle;

        protected void updateUserScrollDirection(ScrollDirection value) {
            if (userScrollDirection == value) {
                return;
            }

            _userScrollDirection = value;
            didUpdateScrollDirection(value);
        }

        public override Future animateTo(
            float to,
            TimeSpan duration,
            Curve curve
        ) {
            if (PhysicsUtils.nearEqual(to, pixels, physics.tolerance.distance)) {
                jumpTo(to);
                return Future.value();
            }

            DrivenScrollActivity activity = new DrivenScrollActivity(
                this,
                from: pixels,
                to: to,
                duration: duration,
                curve: curve,
                vsync: context.vsync
            );
            beginActivity(activity);
            return activity.done;
        }

        public override void jumpTo(float value) {
            goIdle();
            if (pixels != value) {
                float oldPixels = pixels;
                forcePixels(value);
                // this.notifyListeners(); already in forcePixels, no need here.
                didStartScroll();
                didUpdateScrollPositionBy(pixels - oldPixels);
                didEndScroll();
            }

            goBallistic(0.0f);
        }

        public override ScrollHoldController hold(VoidCallback holdCancelCallback) {
            float previousVelocity = activity.velocity;
            HoldScrollActivity holdActivity = new HoldScrollActivity(
                del: this,
                onHoldCanceled: holdCancelCallback
            );
            beginActivity(holdActivity);
            _heldPreviousVelocity = previousVelocity;
            return holdActivity;
        }

        ScrollDragController _currentDrag;

        public override Drag drag(DragStartDetails details, VoidCallback dragCancelCallback) {
            ScrollDragController drag = new ScrollDragController(
                del: this,
                details: details,
                onDragCanceled: dragCancelCallback,
                carriedVelocity: physics.carriedMomentum(_heldPreviousVelocity),
                motionStartDistanceThreshold: physics.dragStartDistanceMotionThreshold
            );
            beginActivity(new DragScrollActivity(this, drag));
            D.assert(_currentDrag == null);
            _currentDrag = drag;
            return drag;
        }

        public override void dispose() {
            if (_currentDrag != null) {
                _currentDrag.dispose();
                _currentDrag = null;
            }

            base.dispose();
        }

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add(context.GetType().ToString());
            description.Add(physics.ToString());
            description.Add(activity?.ToString());
            description.Add(userScrollDirection.ToString());
        }
    }
}