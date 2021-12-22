using System.Collections.Generic;
using uiwidgets;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgetsGallery.demo.material
{
    internal class SliderDemo : StatefulWidget
    {
        public static readonly string routeName = "/material/slider";

        public override State createState()
        {
            return new _SliderDemoState();
        }
    }

    internal static class SliderDemoUtils
    {
        public static Path _downTriangle(float size, Offset thumbCenter, bool invert = false)
        {
            Path thumbPath = new Path();
            float height = Mathf.Sqrt(3.0f) / 2.0f;
            float centerHeight = size * height / 3.0f;
            float halfSize = size / 2.0f;
            float sign = invert ? -1.0f : 1.0f;
            thumbPath.moveTo(thumbCenter.dx - halfSize, thumbCenter.dy + sign * centerHeight);
            thumbPath.lineTo(thumbCenter.dx, thumbCenter.dy - 2.0f * sign * centerHeight);
            thumbPath.lineTo(thumbCenter.dx + halfSize, thumbCenter.dy + sign * centerHeight);
            thumbPath.close();
            return thumbPath;
        }

        public static Path _rightTriangle(float size, Offset thumbCenter, bool invert = false)
        {
            Path thumbPath = new Path();
            float halfSize = size / 2.0f;
            float sign = invert ? -1.0f : 1.0f;
            thumbPath.moveTo(thumbCenter.dx + halfSize * sign, thumbCenter.dy);
            thumbPath.lineTo(thumbCenter.dx - halfSize * sign, thumbCenter.dy - size);
            thumbPath.lineTo(thumbCenter.dx - halfSize * sign, thumbCenter.dy + size);
            thumbPath.close();
            return thumbPath;
        }

        public static Path _upTriangle(float size, Offset thumbCenter)
        {
            return _downTriangle(size, thumbCenter, invert: true);
        }

        public static Path _leftTriangle(float size, Offset thumbCenter)
        {
            return _rightTriangle(size, thumbCenter, invert: true);
        }
    }

    internal class _CustomRangeThumbShape : RangeSliderThumbShape
    {
        private const float _thumbSize = 4.0f;
        private const float _disabledThumbSize = 3.0f;

        public override Size getPreferredSize(bool isEnabled, bool isDiscrete)
        {
            return isEnabled ? Size.fromRadius(_thumbSize) : Size.fromRadius(_disabledThumbSize);
        }

        private static readonly Animatable<float> sizeTween = new FloatTween(
            begin: _disabledThumbSize,
            end: _thumbSize
        );


        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            bool isEnabled = false,
            bool? isOnTop = null,
            TextDirection? textDirection = null,
            SliderThemeData sliderTheme = null,
            Thumb? thumb = null
        )
        {
            Canvas canvas = context.canvas;
            ColorTween colorTween = new ColorTween(
                begin: sliderTheme.disabledThumbColor,
                end: sliderTheme.thumbColor
            );

            float size = _thumbSize * sizeTween.evaluate(enableAnimation);
            Path thumbPath = null;
            switch (textDirection)
            {
                case TextDirection.rtl:
                    switch (thumb)
                    {
                        case Thumb.start:
                            thumbPath = SliderDemoUtils._rightTriangle(size, center);
                            break;
                        case Thumb.end:
                            thumbPath = SliderDemoUtils._leftTriangle(size, center);
                            break;
                    }

                    break;
                case TextDirection.ltr:
                    switch (thumb)
                    {
                        case Thumb.start:
                            thumbPath = SliderDemoUtils._leftTriangle(size, center);
                            break;
                        case Thumb.end:
                            thumbPath = SliderDemoUtils._rightTriangle(size, center);
                            break;
                    }

                    break;
            }

            canvas.drawPath(thumbPath, new Paint {color = colorTween.evaluate(enableAnimation)});
        }
    }

    internal class _CustomThumbShape : SliderComponentShape
    {
        private const float _thumbSize = 4.0f;
        private const float _disabledThumbSize = 3.0f;

        public override Size getPreferredSize(bool isEnabled, bool isDiscrete, TextPainter textPainter)
        {
            return isEnabled ? Size.fromRadius(_thumbSize) : Size.fromRadius(_disabledThumbSize);
        }

        private static readonly Animatable<float> sizeTween = new FloatTween(
            begin: _disabledThumbSize,
            end: _thumbSize
        );

        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            TextDirection? textDirection = null,
            float? value = null)
        {
            Canvas canvas = context.canvas;
            ColorTween colorTween = new ColorTween(
                begin: sliderTheme.disabledThumbColor,
                end: sliderTheme.thumbColor
            );
            float size = _thumbSize * sizeTween.evaluate(enableAnimation);
            Path thumbPath = SliderDemoUtils._downTriangle(size, center);
            canvas.drawPath(thumbPath, new Paint {color = colorTween.evaluate(enableAnimation)});
        }
    }

    internal class _CustomValueIndicatorShape : SliderComponentShape
    {
        private const float _indicatorSize = 4.0f;
        private const float _disabledIndicatorSize = 3.0f;
        private const float _slideUpHeight = 40.0f;

        public override Size getPreferredSize(bool isEnabled, bool isDiscrete, TextPainter textPainter)
        {
            return Size.fromRadius(isEnabled ? _indicatorSize : _disabledIndicatorSize);
        }

        private static readonly Animatable<float> sizeTween = new FloatTween(
            begin: _disabledIndicatorSize,
            end: _indicatorSize
        );

        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool isDiscrete = false,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            TextDirection? textDirection = null,
            float? value = null)
        {
            Canvas canvas = context.canvas;
            ColorTween enableColor = new ColorTween(
                begin: sliderTheme.disabledThumbColor,
                end: sliderTheme.valueIndicatorColor
            );

            Tween<float> slideUpTween = new FloatTween(
                begin: 0.0f,
                end: _slideUpHeight
            );
            float size = _indicatorSize * sizeTween.evaluate(enableAnimation);
            Offset slideUpOffset = new Offset(0.0f, -slideUpTween.evaluate(activationAnimation));
            Path thumbPath = SliderDemoUtils._upTriangle(size, center + slideUpOffset);
            Color paintColor = enableColor.evaluate(enableAnimation)
                .withAlpha((255.0f * activationAnimation.value).round());
            canvas.drawPath(
                thumbPath,
                new Paint {color = paintColor}
            );
            canvas.drawLine(
                center,
                center + slideUpOffset,
                new Paint
                {
                    color = paintColor,
                    style = PaintingStyle.stroke,
                    strokeWidth = 2.0f
                });
            labelPainter.paint(canvas,
                center + slideUpOffset + new Offset(-labelPainter.width / 2.0f, -labelPainter.height - 4.0f));
        }
    }

    internal class _SliderDemoState : State<SliderDemo>
    {
        public override Widget build(BuildContext context)
        {
            List<ComponentDemoTabData> demos = new List<ComponentDemoTabData>
            {
                new ComponentDemoTabData(
                    tabName: "SINGLE",
                    description: "Sliders containing 1 thumb",
                    demoWidget: new _Sliders(),
                    documentationUrl: "https://docs.flutter.io/flutter/material/Slider-class.html"
                ),
                new ComponentDemoTabData(
                    tabName: "RANGE",
                    description: "Sliders containing 2 thumbs",
                    demoWidget: new _RangeSliders(),
                    documentationUrl: "https://docs.flutter.io/flutter/material/RangeSlider-class.html"
                )
            };

            return new TabbedComponentDemoScaffold(
                title: "Sliders",
                demos: demos,
                isScrollable: false,
                showExampleCodeAction: false
            );
        }
    }

    internal class _Sliders : StatefulWidget
    {
        public override State createState()
        {
            return new _SlidersState();
        }
    }

    internal class _SlidersState : State<_Sliders>
    {
        private float _continuousValue = 25.0f;
        private float _discreteValue = 20.0f;
        private float _discreteCustomValue = 25.0f;


        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            return new Padding(
                padding: EdgeInsets.symmetric(horizontal: 40.0f),
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.spaceAround,
                    children: new List<Widget>
                    {
                        new Column(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                new SizedBox(
                                    width: 64,
                                    height: 48,
                                    child: new TextField(
                                        textAlign: TextAlign.center,
                                        onSubmitted: (string value) =>
                                        {
                                            float? newValue = null;
                                            if (float.TryParse(value, out float convertValue))
                                            {
                                                newValue = convertValue;
                                            }
                                            if (newValue != null && newValue != _continuousValue)
                                                setState(() => { _continuousValue = newValue.Value.clamp(0.0f, 100.0f); });
                                        },
                                        keyboardType: TextInputType.number,
                                        controller: new TextEditingController(
                                            text: $"{_continuousValue:F0}"
                                        )
                                    )
                                ),
                                Slider.adaptive(
                                    value: _continuousValue,
                                    min: 0.0f,
                                    max: 100.0f,
                                    onChanged: (float value) => { setState(() => { _continuousValue = value; }); }
                                ),
                                new Text("Continuous with Editable Numerical Value")
                            }
                        ),
                        new Column(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                Slider.adaptive(value: 0.25f, onChanged: null),
                                new Text("Disabled")
                            }
                        ),
                        new Column(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                Slider.adaptive(
                                    value: _discreteValue,
                                    min: 0.0f,
                                    max: 200.0f,
                                    divisions: 5,
                                    label: $"{_discreteValue.round()}",
                                    onChanged: (float value) => { setState(() => { _discreteValue = value; }); }
                                ),
                                new Text("Discrete")
                            }
                        ),
                        new Column(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                new SliderTheme(
                                    data: theme.sliderTheme.copyWith(
                                        activeTrackColor: Colors.deepPurple,
                                        inactiveTrackColor: theme.colorScheme.onSurface.withOpacity(0.5f),
                                        activeTickMarkColor: theme.colorScheme.onSurface.withOpacity(0.7f),
                                        inactiveTickMarkColor: theme.colorScheme.surface.withOpacity(0.7f),
                                        overlayColor: theme.colorScheme.onSurface.withOpacity(0.12f),
                                        thumbColor: Colors.deepPurple,
                                        valueIndicatorColor: Colors.deepPurpleAccent,
                                        thumbShape: new _CustomThumbShape(),
                                        valueIndicatorShape: new _CustomValueIndicatorShape(),
                                        valueIndicatorTextStyle: theme.accentTextTheme.bodyText1.copyWith(
                                            color: theme.colorScheme.onSurface)
                                    ),
                                    child: new Slider(
                                        value: _discreteCustomValue,
                                        min: 0.0f,
                                        max: 200.0f,
                                        divisions: 5,
                                        label: $"{_discreteCustomValue.round()}",
                                        onChanged: (float value) =>
                                        {
                                            setState(() => { _discreteCustomValue = value; });
                                        }
                                    )
                                ),
                                new Text("Discrete with Custom Theme")
                            }
                        )
                    }
                )
            );
        }
    }

    internal class _RangeSliders : StatefulWidget
    {
        public override State createState()
        {
            return new _RangeSlidersState();
        }
    }

    internal class _RangeSlidersState : State<_RangeSliders>
    {
        private RangeValues _continuousValues = new RangeValues(25.0f, 75.0f);
        private RangeValues _discreteValues = new RangeValues(40.0f, 120.0f);
        private RangeValues _discreteCustomValues = new RangeValues(40.0f, 160.0f);

        public override Widget build(BuildContext context)
        {
            return new Padding(
                padding: EdgeInsets.symmetric(horizontal: 40.0f),
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.spaceAround,
                    children: new List<Widget>
                    {
                        new Column(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                new RangeSlider(
                                    values: _continuousValues,
                                    min: 0.0f,
                                    max: 100.0f,
                                    onChanged: (RangeValues values) =>
                                    {
                                        setState(() => { _continuousValues = values; });
                                    }
                                ),
                                new Text("Continuous")
                            }
                        ),
                        new Column(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                new RangeSlider(values: new RangeValues(0.25f, 0.75f), onChanged: null),
                                new Text("Disabled")
                            }
                        ),
                        new Column(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                new RangeSlider(
                                    values: _discreteValues,
                                    min: 0.0f,
                                    max: 200.0f,
                                    //divisions: 5,
                                    labels: new RangeLabels($"{_discreteValues.start.round()}",
                                        $"{_discreteValues.end.round()}"),
                                    onChanged: (RangeValues values) =>
                                    {
                                        setState(() => { _discreteValues = values; });
                                    }
                                ),
                                new Text("Discrete")
                            }
                        ),
                        new Column(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                new SliderTheme(
                                    data: new SliderThemeData(
                                        activeTrackColor: Colors.deepPurple,
                                        inactiveTrackColor: Colors.black26,
                                        activeTickMarkColor: Colors.white70,
                                        inactiveTickMarkColor: Colors.black,
                                        overlayColor: Colors.black12,
                                        thumbColor: Colors.deepPurple,
                                        rangeThumbShape: new _CustomRangeThumbShape(),
                                        showValueIndicator: ShowValueIndicator.never
                                    ),
                                    child: new RangeSlider(
                                        values: _discreteCustomValues,
                                        min: 0.0f,
                                        max: 200.0f,
                                        divisions: 5,
                                        labels: new RangeLabels($"{_discreteCustomValues.start.round()}",
                                            $"{_discreteCustomValues.end.round()}"),
                                        onChanged: (RangeValues values) =>
                                        {
                                            setState(() => { _discreteCustomValues = values; });
                                        }
                                    )
                                ),
                                new Text("Discrete with Custom Theme")
                            }
                        )
                    }
                )
            );
        }
    }
}