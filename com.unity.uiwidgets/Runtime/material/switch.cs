using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using ImageUtils = Unity.UIWidgets.widgets.ImageUtils;

namespace Unity.UIWidgets.material {
    enum _SwitchType {
        material,
        adaptive
    }

    public class Switch : StatefulWidget {
        internal const float _kTrackHeight = 14.0f;
        internal const float _kTrackWidth = 33.0f;
        internal const float _kTrackRadius = _kTrackHeight / 2.0f;
        internal const float _kThumbRadius = 10.0f;
        internal const float _kSwitchWidth = _kTrackWidth - 2 * _kTrackRadius + 2 * Constants.kRadialReactionRadius;
        internal const float _kSwitchHeight = 2 * Constants.kRadialReactionRadius + 8.0f;
        internal const float _kSwitchHeightCollapsed = 2 * Constants.kRadialReactionRadius;

        public Switch(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color activeTrackColor = null,
            Color inactiveThumbColor = null,
            Color inactiveTrackColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : this(
            key: key,
            value: value,
            onChanged: onChanged,
            activeColor: activeColor,
            activeTrackColor: activeTrackColor,
            inactiveThumbColor: inactiveThumbColor,
            inactiveTrackColor: inactiveTrackColor,
            activeThumbImage: activeThumbImage,
            inactiveThumbImage: inactiveThumbImage,
            materialTapTargetSize: materialTapTargetSize,
            switchType: _SwitchType.material,
            dragStartBehavior: dragStartBehavior
        ) {
        }

        Switch(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color activeTrackColor = null,
            Color inactiveThumbColor = null,
            Color inactiveTrackColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            _SwitchType switchType = _SwitchType.material,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(value != null);
            this.value = value.Value;
            D.assert(onChanged != null);
            this.onChanged = onChanged;
            this.activeColor = activeColor;
            this.activeTrackColor = activeTrackColor;
            this.inactiveThumbColor = inactiveThumbColor;
            this.inactiveTrackColor = inactiveTrackColor;
            this.activeThumbImage = activeThumbImage;
            this.inactiveThumbImage = inactiveThumbImage;
            this.materialTapTargetSize = materialTapTargetSize;
            _switchType = switchType;
            this.dragStartBehavior = dragStartBehavior;
        }

        public static Switch adaptive(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color activeTrackColor = null,
            Color inactiveThumbColor = null,
            Color inactiveTrackColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.down
        ) {
            return new Switch(key: key,
                value: value,
                onChanged: onChanged,
                activeColor: activeColor,
                activeTrackColor: activeTrackColor,
                inactiveThumbColor: inactiveThumbColor,
                inactiveTrackColor: inactiveTrackColor,
                activeThumbImage: activeThumbImage,
                inactiveThumbImage: inactiveThumbImage,
                materialTapTargetSize: materialTapTargetSize,
                switchType: _SwitchType.adaptive
            );
        }

        public readonly bool value;

        public readonly ValueChanged<bool?> onChanged;

        public readonly Color activeColor;

        public readonly Color activeTrackColor;

        public readonly Color inactiveThumbColor;

        public readonly Color inactiveTrackColor;

        public readonly ImageProvider activeThumbImage;

        public readonly ImageProvider inactiveThumbImage;

        public readonly MaterialTapTargetSize? materialTapTargetSize;

        internal readonly _SwitchType _switchType;

        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _SwitchState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("value", value: value, ifTrue: "on", ifFalse: "off", showName: true));
            properties.add(
                new ObjectFlagProperty<ValueChanged<bool?>>("onChanged", onChanged, ifNull: "disabled"));
        }
    }

    class _SwitchState : TickerProviderStateMixin<Switch> {
        Size getSwitchSize(ThemeData theme) {
            switch (widget.materialTapTargetSize ?? theme.materialTapTargetSize) {
                case MaterialTapTargetSize.padded:
                    return new Size(Switch._kSwitchWidth, Switch._kSwitchHeight);
                case MaterialTapTargetSize.shrinkWrap:
                    return new Size(Switch._kSwitchWidth, Switch._kSwitchHeightCollapsed);
            }

            D.assert(false);
            return null;
        }

        Widget buildMaterialSwitch(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            ThemeData theme = Theme.of(context);
            bool isDark = theme.brightness == Brightness.dark;

            Color activeThumbColor = widget.activeColor ?? theme.toggleableActiveColor;
            Color activeTrackColor = widget.activeTrackColor ?? activeThumbColor.withAlpha(0x80);

            Color inactiveThumbColor;
            Color inactiveTrackColor;
            if (widget.onChanged != null) {
                Color black32 = new Color(0x52000000); // Black with 32% opacity
                inactiveThumbColor = widget.inactiveThumbColor ??
                                     (isDark ? Colors.grey.shade400 : Colors.grey.shade50);
                inactiveTrackColor = widget.inactiveTrackColor ?? (isDark ? Colors.white30 : black32);
            }
            else {
                inactiveThumbColor = widget.inactiveThumbColor ??
                                     (isDark ? Colors.grey.shade800 : Colors.grey.shade400);
                inactiveTrackColor = widget.inactiveTrackColor ?? (isDark ? Colors.white10 : Colors.black12);
            }

            return new _SwitchRenderObjectWidget(
                dragStartBehavior: widget.dragStartBehavior,
                value: widget.value,
                activeColor: activeThumbColor,
                inactiveColor: inactiveThumbColor,
                activeThumbImage: widget.activeThumbImage,
                inactiveThumbImage: widget.inactiveThumbImage,
                activeTrackColor: activeTrackColor,
                inactiveTrackColor: inactiveTrackColor,
                configuration: ImageUtils.createLocalImageConfiguration(context),
                onChanged: widget.onChanged,
                additionalConstraints: BoxConstraints.tight(getSwitchSize(theme)),
                vsync: this
            );
        }

//        Widget buildCupertinoSwitch(BuildContext context) {
//            Size size = this.getSwitchSize(Theme.of(context));
//            return new Container(
//                width: size.width, // Same size as the Material switch.
//                height: size.height,
//                alignment: Alignment.center,
//                child: CupertinoSwitch(
//                    value: this.widget.value,
//                    onChanged: this.widget.onChanged,
//                    activeColor: this.widget.activeColor
//                )
//            );
//        }

        public override Widget build(BuildContext context) {
            switch (widget._switchType) {
                case _SwitchType.material:
                    return buildMaterialSwitch(context);

                case _SwitchType.adaptive: {
                    return buildMaterialSwitch(context);
//                    ThemeData theme = Theme.of(context);
//                    D.assert(theme.platform != null);
//                    switch (theme.platform) {
//                        case TargetPlatform.android:
//                            return buildMaterialSwitch(context);
//                        case TargetPlatform.iOS:
//                            return buildCupertinoSwitch(context);
//                    }
//                    break;
                }
            }

            D.assert(false);
            return null;
        }
    }

    class _SwitchRenderObjectWidget : LeafRenderObjectWidget {
        public _SwitchRenderObjectWidget(
            Key key = null,
            bool? value = null,
            Color activeColor = null,
            Color inactiveColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            Color activeTrackColor = null,
            Color inactiveTrackColor = null,
            ImageConfiguration configuration = null,
            ValueChanged<bool?> onChanged = null,
            TickerProvider vsync = null,
            BoxConstraints additionalConstraints = null,
            DragStartBehavior? dragStartBehavior = null
        ) : base(key: key) {
            D.assert(value != null);
            this.value = value.Value;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
            this.activeThumbImage = activeThumbImage;
            this.inactiveThumbImage = inactiveThumbImage;
            this.activeTrackColor = activeTrackColor;
            this.inactiveTrackColor = inactiveTrackColor;
            this.configuration = configuration;
            this.onChanged = onChanged;
            this.vsync = vsync;
            this.additionalConstraints = additionalConstraints;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly bool value;
        public readonly Color activeColor;
        public readonly Color inactiveColor;
        public readonly ImageProvider activeThumbImage;
        public readonly ImageProvider inactiveThumbImage;
        public readonly Color activeTrackColor;
        public readonly Color inactiveTrackColor;
        public readonly ImageConfiguration configuration;
        public readonly ValueChanged<bool?> onChanged;
        public readonly TickerProvider vsync;
        public readonly BoxConstraints additionalConstraints;
        public readonly DragStartBehavior? dragStartBehavior;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSwitch(
                dragStartBehavior: dragStartBehavior,
                value: value,
                activeColor: activeColor,
                inactiveColor: inactiveColor,
                activeThumbImage: activeThumbImage,
                inactiveThumbImage: inactiveThumbImage,
                activeTrackColor: activeTrackColor,
                inactiveTrackColor: inactiveTrackColor,
                configuration: configuration,
                onChanged: onChanged,
                additionalConstraints: additionalConstraints,
                vsync: vsync
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            _RenderSwitch renderObject = (_RenderSwitch) renderObjectRaw;

            renderObject.value = value;
            renderObject.activeColor = activeColor;
            renderObject.inactiveColor = inactiveColor;
            renderObject.activeThumbImage = activeThumbImage;
            renderObject.inactiveThumbImage = inactiveThumbImage;
            renderObject.activeTrackColor = activeTrackColor;
            renderObject.inactiveTrackColor = inactiveTrackColor;
            renderObject.configuration = configuration;
            renderObject.onChanged = onChanged;
            renderObject.additionalConstraints = additionalConstraints;
            renderObject.dragStartBehavior = dragStartBehavior;
            renderObject.vsync = vsync;
        }
    }

    class _RenderSwitch : RenderToggleable {
        public _RenderSwitch(
            bool? value = null,
            Color activeColor = null,
            Color inactiveColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            Color activeTrackColor = null,
            Color inactiveTrackColor = null,
            ImageConfiguration configuration = null,
            BoxConstraints additionalConstraints = null,
            ValueChanged<bool?> onChanged = null,
            TickerProvider vsync = null,
            DragStartBehavior? dragStartBehavior = null
        ) : base(
            value: value,
            tristate: false,
            activeColor: activeColor,
            inactiveColor: inactiveColor,
            onChanged: onChanged,
            additionalConstraints: additionalConstraints,
            vsync: vsync
        ) {
            _activeThumbImage = activeThumbImage;
            _inactiveThumbImage = inactiveThumbImage;
            _activeTrackColor = activeTrackColor;
            _inactiveTrackColor = inactiveTrackColor;
            _configuration = configuration;
            _drag = new HorizontalDragGestureRecognizer {
                onStart = _handleDragStart,
                onUpdate = _handleDragUpdate,
                onEnd = _handleDragEnd,
                dragStartBehavior = dragStartBehavior ?? DragStartBehavior.down
            };
        }

        public ImageProvider activeThumbImage {
            get { return _activeThumbImage; }
            set {
                if (value == _activeThumbImage) {
                    return;
                }

                _activeThumbImage = value;
                markNeedsPaint();
            }
        }

        ImageProvider _activeThumbImage;

        public ImageProvider inactiveThumbImage {
            get { return _inactiveThumbImage; }
            set {
                if (value == _inactiveThumbImage) {
                    return;
                }

                _inactiveThumbImage = value;
                markNeedsPaint();
            }
        }

        ImageProvider _inactiveThumbImage;

        public Color activeTrackColor {
            get { return _activeTrackColor; }
            set {
                D.assert(value != null);
                if (value == _activeTrackColor) {
                    return;
                }

                _activeTrackColor = value;
                markNeedsPaint();
            }
        }

        Color _activeTrackColor;

        public Color inactiveTrackColor {
            get { return _inactiveTrackColor; }
            set {
                D.assert(value != null);
                if (value == _inactiveTrackColor) {
                    return;
                }

                _inactiveTrackColor = value;
                markNeedsPaint();
            }
        }

        Color _inactiveTrackColor;

        public ImageConfiguration configuration {
            get { return _configuration; }
            set {
                D.assert(value != null);
                if (value == _configuration) {
                    return;
                }

                _configuration = value;
                markNeedsPaint();
            }
        }

        ImageConfiguration _configuration;

        public DragStartBehavior? dragStartBehavior {
            get { return _drag.dragStartBehavior; }
            set { _drag.dragStartBehavior = value ?? DragStartBehavior.down; }
        }


        public override void detach() {
            _cachedThumbPainter?.Dispose();
            _cachedThumbPainter = null;
            base.detach();
        }

        float _trackInnerLength {
            get { return size.width - 2.0f * Constants.kRadialReactionRadius; }
        }

        HorizontalDragGestureRecognizer _drag;

        void _handleDragStart(DragStartDetails details) {
            if (isInteractive) {
                reactionController.forward();
            }
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (isInteractive) {
                position.curve = null;
                position.reverseCurve = null;
                float delta = details.primaryDelta.Value / _trackInnerLength;
                positionController.setValue(positionController.value + delta);
            }
        }

        void _handleDragEnd(DragEndDetails details) {
            if (position.value >= 0.5) {
                positionController.forward();
            }
            else {
                positionController.reverse();
            }

            reactionController.reverse();
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(debugHandleEvent(evt, entry));
            if (evt is PointerDownEvent && onChanged != null) {
                _drag.addPointer((PointerDownEvent) evt);
            }

            base.handleEvent(evt, entry);
        }

        Color _cachedThumbColor;
        ImageProvider _cachedThumbImage;
        BoxPainter _cachedThumbPainter;

        BoxDecoration _createDefaultThumbDecoration(Color color, ImageProvider image) {
            return new BoxDecoration(
                color: color,
                image: image == null ? null : new DecorationImage(image: image),
                shape: BoxShape.circle,
                boxShadow: ShadowConstants.kElevationToShadow[1]
            );
        }

        bool _isPainting = false;

        void _handleDecorationChanged() {
            if (!_isPainting) {
                markNeedsPaint();
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;

            bool isEnabled = onChanged != null;
            float currentValue = position.value;

            float visualPosition = currentValue;

            Color trackColor = isEnabled
                ? Color.lerp(inactiveTrackColor, activeTrackColor, currentValue)
                : inactiveTrackColor;

            Color thumbColor = isEnabled
                ? Color.lerp(inactiveColor, activeColor, currentValue)
                : inactiveColor;

            ImageProvider thumbImage = isEnabled
                ? (currentValue < 0.5 ? inactiveThumbImage : activeThumbImage)
                : inactiveThumbImage;

            // Paint the track
            Paint paint = new Paint {color = trackColor};
            float trackHorizontalPadding = Constants.kRadialReactionRadius - Switch._kTrackRadius;
            Rect trackRect = Rect.fromLTWH(
                offset.dx + trackHorizontalPadding,
                offset.dy + (size.height - Switch._kTrackHeight) / 2.0f,
                size.width - 2.0f * trackHorizontalPadding,
                Switch._kTrackHeight
            );
            RRect trackRRect = RRect.fromRectAndRadius(trackRect, Radius.circular(Switch._kTrackRadius));
            canvas.drawRRect(trackRRect, paint);

            Offset thumbPosition = new Offset(
                Constants.kRadialReactionRadius + visualPosition * _trackInnerLength,
                size.height / 2.0f
            );

            paintRadialReaction(canvas, offset, thumbPosition);

            try {
                _isPainting = true;
                BoxPainter thumbPainter;
                if (_cachedThumbPainter == null || thumbColor != _cachedThumbColor ||
                    thumbImage != _cachedThumbImage) {
                    _cachedThumbColor = thumbColor;
                    _cachedThumbImage = thumbImage;
                    _cachedThumbPainter = _createDefaultThumbDecoration(thumbColor, thumbImage)
                        .createBoxPainter(_handleDecorationChanged);
                }

                thumbPainter = _cachedThumbPainter;

                float inset = 1.0f - (currentValue - 0.5f).abs() * 2.0f;
                float radius = Switch._kThumbRadius - inset;
                thumbPainter.paint(
                    canvas,
                    thumbPosition + offset - new Offset(radius, radius),
                    configuration.copyWith(size: Size.fromRadius(radius))
                );
            }
            finally {
                _isPainting = false;
            }
        }
    }
}