using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    class BannerConstants {
        public const float _kOffset = 40.0f;
        public const float _kHeight = 12.0f;
        public const float _kBottomOffset = _kOffset + 0.707f * _kHeight;
        public static readonly Rect _kRect = Rect.fromLTWH(-_kOffset, _kOffset - _kHeight, _kOffset * 2.0f, _kHeight);

        public static readonly Color _kColor = new Color(0xA0B71C1C);

        public static readonly TextStyle _kTextStyle = new TextStyle(
            color: new Color(0xFFFFFFFF),
            fontSize: _kHeight * 0.85f,
            fontWeight: FontWeight.w700,
            height: 1.0f
        );
    }

    public enum BannerLocation {
        topStart,

        topEnd,

        bottomStart,

        bottomEnd,
    }

    public class BannerPainter : AbstractCustomPainter {
        public BannerPainter(
            string message = null,
            TextDirection? textDirection = null,
            BannerLocation? location = null,
            TextDirection? layoutDirection = null,
            Color color = null,
            TextStyle textStyle = null
        ) : base (repaint: PaintingBinding.instance.systemFonts) {
            D.assert(message != null);
            D.assert(textDirection != null);
            D.assert(location != null);

            this.message = message;
            this.textDirection = textDirection;
            this.location = location;
            this.layoutDirection = layoutDirection;
            this.color = color ?? BannerConstants._kColor;
            this.textStyle = textStyle ?? BannerConstants._kTextStyle;
        }

        public readonly string message;
        
        public readonly TextDirection? textDirection;

        public readonly BannerLocation? location;
        
        public readonly TextDirection? layoutDirection;

        public readonly Color color;

        public readonly TextStyle textStyle;

        readonly BoxShadow _shadow = new BoxShadow(
            color: new Color(0x7F000000),
            blurRadius: 6.0f
        );

        bool _prepared = false;
        TextPainter _textPainter;
        Paint _paintShadow;
        Paint _paintBanner;

        void _prepare() {
            _paintShadow = _shadow.toPaint();
            _paintBanner = new Paint();
            _paintBanner.color = color;
            _textPainter = new TextPainter(
                text: new TextSpan(style: textStyle, text: message),
                textAlign: TextAlign.center,
                textDirection: textDirection ?? TextDirection.ltr
            );
            _prepared = true;
        }

        public override void paint(Canvas canvas, Size size) {
            if (!_prepared) {
                _prepare();
            }

            canvas.translate(_translationX(size.width), _translationY(size.height));
            canvas.rotate(_rotation);
            canvas.drawRect(BannerConstants._kRect, _paintShadow);
            canvas.drawRect(BannerConstants._kRect, _paintBanner);
            const float width = BannerConstants._kOffset * 2.0f;
            _textPainter.layout(minWidth: width, maxWidth: width);
            _textPainter.paint(canvas,
                BannerConstants._kRect.topLeft + new Offset(0.0f,
                    (BannerConstants._kRect.height - _textPainter.height) / 2.0f));
        }

        public override bool shouldRepaint(CustomPainter _oldDelegate) {
            BannerPainter oldDelegate = _oldDelegate as BannerPainter;
            return message != oldDelegate.message
                   || location != oldDelegate.location
                   || color != oldDelegate.color
                   || textStyle != oldDelegate.textStyle;
        }

        public override bool? hitTest(Offset position) {
            return false;
        }

        float _translationX(float width) {
            D.assert(location != null);
            D.assert(layoutDirection != null);
            switch (layoutDirection) {
                case TextDirection.rtl:
                    switch (location) {
                        case BannerLocation.bottomEnd:
                            return BannerConstants._kBottomOffset;
                        case BannerLocation.topEnd:
                            return 0.0f;
                        case BannerLocation.bottomStart:
                            return width - BannerConstants._kBottomOffset;
                        case BannerLocation.topStart:
                            return width;
                    }
                    break;
                case TextDirection.ltr:
                    switch (location) {
                        case BannerLocation.bottomEnd:
                            return width - BannerConstants._kBottomOffset;
                        case BannerLocation.topEnd:
                            return width;
                        case BannerLocation.bottomStart:
                            return BannerConstants._kBottomOffset;
                        case BannerLocation.topStart:
                            return 0.0f;
                    }
                    break;
                
            }
            return 0.0f;
        }

        float _translationY(float height) {
            D.assert(location != null);
            switch (location) {
                case BannerLocation.bottomStart:
                case BannerLocation.bottomEnd:
                    return height - BannerConstants._kBottomOffset;
                case BannerLocation.topStart:
                case BannerLocation.topEnd:
                    return 0.0f;
                default:
                    throw new Exception("Unknown location: " + location);
            }
        }

        float _rotation {
            get {
                D.assert(location != null);
                D.assert(layoutDirection != null);
                switch (layoutDirection) {
                    case TextDirection.rtl:
                        switch (location) {
                            case BannerLocation.bottomStart:
                            case BannerLocation.topEnd:
                                return -Mathf.PI / 4.0f;
                            case BannerLocation.bottomEnd:
                            case BannerLocation.topStart:
                                return Mathf.PI / 4.0f;
                        }
                        break;
                    case TextDirection.ltr:
                        switch (location) {
                            case BannerLocation.bottomStart:
                            case BannerLocation.topEnd:
                                return Mathf.PI / 4.0f;
                            case BannerLocation.bottomEnd:
                            case BannerLocation.topStart:
                                return -Mathf.PI / 4.0f;
                        }
                        break;
                }
                return 0.0f;
            }
        }
    }

    public class Banner : StatelessWidget {
        public Banner(
            Key key = null,
            Widget child = null,
            string message = null,
            TextDirection? textDirection = null,
            BannerLocation? location = null,
            TextDirection? layoutDirection = null,
            Color color = null,
            TextStyle textStyle = null
        ) : base(key: key) {
            D.assert(message != null);
            D.assert(location != null);
            this.child = child;
            this.message = message;
            this.layoutDirection = layoutDirection;
            this.textDirection = textDirection;
            this.location = location;
            this.color = color ?? BannerConstants._kColor;
            this.textStyle = textStyle ?? BannerConstants._kTextStyle;
        }

        public readonly Widget child;

        public readonly string message;
        
        public readonly TextDirection? textDirection;
        
        public readonly TextDirection? layoutDirection;

        public readonly BannerLocation? location;

        public readonly Color color;

        public readonly TextStyle textStyle;

        public override Widget build(BuildContext context) {
            D.assert((textDirection != null && layoutDirection != null)|| WidgetsD.debugCheckHasDirectionality(context));
            return new CustomPaint(
                foregroundPainter: new BannerPainter(
                    message: message,
                    textDirection: textDirection ?? Directionality.of(context),
                    location: location,
                    layoutDirection: layoutDirection ?? Directionality.of(context),
                    color: color,
                    textStyle: textStyle
                ),
                child: child
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("message", message, showName: false));
            properties.add(new EnumProperty<TextDirection?>("textDirection", textDirection, defaultValue: null));
            properties.add(new EnumProperty<BannerLocation?>("location", location));
            properties.add(new EnumProperty<TextDirection?>("layoutDirection", layoutDirection, defaultValue: null));
            properties.add(new ColorProperty("color", color, showName: false));
            textStyle?.debugFillProperties(properties, prefix: "text ");
        }
    }

    public class CheckedModeBanner : StatelessWidget {
        public CheckedModeBanner(
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(child != null);
            this.child = child;
        }


        public readonly Widget child;

        public override Widget build(BuildContext context) {
            Widget result = child;
            D.assert(() => {
                result = new Banner(
                    child: result,
                    message: "DEBUG",
                    textDirection: TextDirection.ltr,
                    location: BannerLocation.topEnd
                );
                return true;
            });
            return result;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            string message = "disabled";
            D.assert(() => {
                message = "'DEBUG'";
                return true;
            });
            properties.add(DiagnosticsNode.message(message));
        }
    }
}