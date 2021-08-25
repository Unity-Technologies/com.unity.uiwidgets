using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Debug = UnityEngine.Debug;
using Rect = Unity.UIWidgets.ui.Rect;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Unity.UIWidgets.foundation {
    
    
    public static class D {
        public static bool debugPrintGestureArenaDiagnostics = false;

        public static bool debugPrintHitTestResults = false;

        public static bool debugPaintPointersEnabled;

        public static bool debugPaintBaselinesEnabled;

        public static bool debugPrintRecognizerCallbacksTrace = false;

        public static bool debugPaintSizeEnabled;

        public static bool debugRepaintRainbowEnabled;

        public static bool debugRepaintTextRainbowEnabled = false;

        public static bool debugPaintLayerBordersEnabled;

        public static bool debugPrintMarkNeedsLayoutStacks = false;

        public static bool debugPrintLayouts = false;

        public static bool debugDisableClipLayers = false;

        public static bool debugDisableOpacityLayers = false;

        public static bool debugDisablePhysicalShapeLayers = false;

        public static bool debugPrintMarkNeedsPaintStacks = false;

        public static bool debugCheckIntrinsicSizes = false;

        public static bool debugPrintMouseHoverEvents = false;

        public static HSVColor debugCurrentRepaintColor =
            HSVColor.fromAHSV(0.4f, 60.0f, 1.0f, 1.0f);

        public static int? debugFloatPrecision;

        public static void logError(string message, Exception ex = null) {
            Debug.LogException(new AssertionError(message: message, innerException: ex));
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void assert(Func<bool> result, Func<string> message = null) {
            if ( enableDebug && !result() ) {
                throw new AssertionError(message != null ? message() : "");
            }
        }
        
        [Conditional("UNITY_ASSERTIONS")]
        public static void assert(bool result, Func<string> message = null) {
            if ( enableDebug && !result ) {
                throw new AssertionError(message != null ? message() : "");
            }
        }

#if UNITY_EDITOR
        static bool? _enableDebug = null;

        public static bool enableDebug {
            get {
                if (_enableDebug == null) {
                    _enableDebug = EditorPrefs.GetInt("UIWidgetsDebug") == 1;
                }
                return _enableDebug.Value;
            }
            set {
                if (_enableDebug == value) {
                    return;
                }
                _enableDebug = value;
                EditorPrefs.SetInt("UIWidgetsDebug",value ? 1 : 0);
            }
        }
#else
        //In the runtime, we use the Conditional decorator instead of this to enable/disable debug mode
        //The rule is simple: the debug mode is on for Debug/Development build, and is off for Release build
        public static bool enableDebug => true;
#endif

        public static void _debugDrawDoubleRect(Canvas canvas, Rect outerRect, Rect innerRect, Color color) { 
            var path = new Path();
            path.fillType = PathFillType.evenOdd;
            path.addRect(rect: outerRect);
            path.addRect(rect: innerRect);
            var paint = new Paint {
                color = color
            };
            canvas.drawPath(path: path, paint: paint);
        }

        public static void debugPaintPadding(Canvas canvas, Rect outerRect, Rect innerRect, float outlineWidth = 2.0f) {
            assert(() => {
                if (innerRect != null && !innerRect.isEmpty) {
                    _debugDrawDoubleRect(canvas: canvas, outerRect: outerRect, innerRect: innerRect,
                        new Color(0x900090FF));
                    _debugDrawDoubleRect(canvas: canvas,
                        innerRect.inflate(delta: outlineWidth).intersect(other: outerRect), innerRect: innerRect,
                        new Color(0xFF0090FF));
                }
                else {
                    var paint = new Paint();
                    paint.color = new Color(0x90909090);
                    canvas.drawRect(rect: outerRect, paint: paint);
                }

                return true;
            });
        }

        public static void setDebugPaint(bool? debugPaintSizeEnabled = null,
            bool? debugPaintBaselinesEnabled = null,
            bool? debugPaintPointersEnabled = null,
            bool? debugPaintLayerBordersEnabled = null,
            bool? debugRepaintRainbowEnabled = null) {
            var needRepaint = false;
            if (debugPaintSizeEnabled != null && debugPaintSizeEnabled != D.debugPaintSizeEnabled) {
                D.debugPaintSizeEnabled = debugPaintSizeEnabled.Value;
                needRepaint = true;
            }

            if (debugPaintBaselinesEnabled != null && debugPaintBaselinesEnabled != D.debugPaintBaselinesEnabled) {
                D.debugPaintBaselinesEnabled = debugPaintBaselinesEnabled.Value;
                needRepaint = true;
            }

            if (debugPaintPointersEnabled != null && debugPaintPointersEnabled != D.debugPaintPointersEnabled) {
                D.debugPaintPointersEnabled = debugPaintPointersEnabled.Value;
                needRepaint = true;
            }

            if (debugPaintLayerBordersEnabled != null &&
                debugPaintLayerBordersEnabled != D.debugPaintLayerBordersEnabled) {
                D.debugPaintLayerBordersEnabled = debugPaintLayerBordersEnabled.Value;
                needRepaint = true;
            }

            if (debugRepaintRainbowEnabled != null && debugRepaintRainbowEnabled != D.debugRepaintRainbowEnabled) {
                D.debugRepaintRainbowEnabled = debugRepaintRainbowEnabled.Value;
                needRepaint = true;
            }

            if (needRepaint) {
                /*foreach (var adapter in WindowAdapter.windowAdapters) {
                    adapter._forceRepaint();
                }*/
            }
        }

        public static string debugFormatFloat(float? value) {
            if (value == null) {
                return "null";
            }

            if (debugFloatPrecision != null) {
                return value.Value.ToString($"N{debugFloatPrecision}");
            }

            return value.Value.ToString("N1");
        }
    }

    [Serializable]
    public class AssertionError : Exception {
        readonly Exception innerException;

        public AssertionError(string message) : base(message: message) {
        }

        public AssertionError(string message, Exception innerException) : base(message: message) {
            this.innerException = innerException;
        }

        public override string StackTrace {
            get {
                if (innerException != null) {
                    return innerException.StackTrace;
                }

                var stackTrace = base.StackTrace;
                var lines = stackTrace.Split('\n');
                var strippedLines = lines.Skip(1);

                return string.Join("\n", values: strippedLines);
            }
        }
    }
}