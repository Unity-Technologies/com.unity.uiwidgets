using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.painting;
namespace Unity.UIWidgets.cupertino {
    public class CupertinoColors {
        public static readonly CupertinoDynamicColor activeBlue = systemBlue;
        public static readonly CupertinoDynamicColor activeGreen = systemGreen;
        public static readonly CupertinoDynamicColor activeOrange = systemOrange;

        public static Color white = new Color(0xFFFFFFFF);
        public static Color black = new Color(0xFF000000);
        public static Color lightBackgroundGray = new Color(0xFFE5E5EA);
        public static Color extraLightBackgroundGray = new Color(0xFFEFEFF4);
        public static Color darkBackgroundGray = new Color(0xFF171717);

        public static readonly CupertinoDynamicColor inactiveGray = CupertinoDynamicColor.withBrightness(
            debugLabel: "inactiveGray",
            color: new Color(0xFF999999),
            darkColor: new Color(0xFF757575)
        );

        public static readonly CupertinoDynamicColor destructiveRed = systemRed;

        public static readonly CupertinoDynamicColor systemBlue = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemBlue",
            color: Color.fromARGB(255, 0, 122, 255),
            darkColor: Color.fromARGB(255, 10, 132, 255),
            highContrastColor: Color.fromARGB(255, 0, 64, 221),
            darkHighContrastColor: Color.fromARGB(255, 64, 156, 255)
        );

        public static readonly CupertinoDynamicColor systemGreen = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemGreen",
            color: Color.fromARGB(255, 52, 199, 89),
            darkColor: Color.fromARGB(255, 48, 209, 88),
            highContrastColor: Color.fromARGB(255, 36, 138, 61),
            darkHighContrastColor: Color.fromARGB(255, 48, 219, 91)
        );

        public static readonly CupertinoDynamicColor systemIndigo = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemIndigo",
            color: Color.fromARGB(255, 88, 86, 214),
            darkColor: Color.fromARGB(255, 94, 92, 230),
            highContrastColor: Color.fromARGB(255, 54, 52, 163),
            darkHighContrastColor: Color.fromARGB(255, 125, 122, 255)
        );

        public static readonly CupertinoDynamicColor systemOrange = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemOrange",
            color: Color.fromARGB(255, 255, 149, 0),
            darkColor: Color.fromARGB(255, 255, 159, 10),
            highContrastColor: Color.fromARGB(255, 201, 52, 0),
            darkHighContrastColor: Color.fromARGB(255, 255, 179, 64)
        );

        public static readonly CupertinoDynamicColor systemPink = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemPink",
            color: Color.fromARGB(255, 255, 45, 85),
            darkColor: Color.fromARGB(255, 255, 55, 95),
            highContrastColor: Color.fromARGB(255, 211, 15, 69),
            darkHighContrastColor: Color.fromARGB(255, 255, 100, 130)
        );

        public static readonly CupertinoDynamicColor systemPurple = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemPurple",
            color: Color.fromARGB(255, 175, 82, 222),
            darkColor: Color.fromARGB(255, 191, 90, 242),
            highContrastColor: Color.fromARGB(255, 137, 68, 171),
            darkHighContrastColor: Color.fromARGB(255, 218, 143, 255)
        );

        public static readonly CupertinoDynamicColor systemRed = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemRed",
            color: Color.fromARGB(255, 255, 59, 48),
            darkColor: Color.fromARGB(255, 255, 69, 58),
            highContrastColor: Color.fromARGB(255, 215, 0, 21),
            darkHighContrastColor: Color.fromARGB(255, 255, 105, 97)
        );

        public static readonly CupertinoDynamicColor systemTeal = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemTeal",
            color: Color.fromARGB(255, 90, 200, 250),
            darkColor: Color.fromARGB(255, 100, 210, 255),
            highContrastColor: Color.fromARGB(255, 0, 113, 164),
            darkHighContrastColor: Color.fromARGB(255, 112, 215, 255)
        );

        public static readonly CupertinoDynamicColor systemYellow = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemYellow",
            color: Color.fromARGB(255, 255, 204, 0),
            darkColor: Color.fromARGB(255, 255, 214, 10),
            highContrastColor: Color.fromARGB(255, 160, 90, 0),
            darkHighContrastColor: Color.fromARGB(255, 255, 212, 38)
        );

        public static readonly CupertinoDynamicColor systemGrey = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemGrey",
            color: Color.fromARGB(255, 142, 142, 147),
            darkColor: Color.fromARGB(255, 142, 142, 147),
            highContrastColor: Color.fromARGB(255, 108, 108, 112),
            darkHighContrastColor: Color.fromARGB(255, 174, 174, 178)
        );

        public static readonly CupertinoDynamicColor systemGrey2 = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemGrey2",
            color: Color.fromARGB(255, 174, 174, 178),
            darkColor: Color.fromARGB(255, 99, 99, 102),
            highContrastColor: Color.fromARGB(255, 142, 142, 147),
            darkHighContrastColor: Color.fromARGB(255, 124, 124, 128)
        );

        public static readonly CupertinoDynamicColor systemGrey3 = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemGrey3",
            color: Color.fromARGB(255, 199, 199, 204),
            darkColor: Color.fromARGB(255, 72, 72, 74),
            highContrastColor: Color.fromARGB(255, 174, 174, 178),
            darkHighContrastColor: Color.fromARGB(255, 84, 84, 86)
        );

        public static readonly CupertinoDynamicColor systemGrey4 = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemGrey4",
            color: Color.fromARGB(255, 209, 209, 214),
            darkColor: Color.fromARGB(255, 58, 58, 60),
            highContrastColor: Color.fromARGB(255, 188, 188, 192),
            darkHighContrastColor: Color.fromARGB(255, 68, 68, 70)
        );

        public static readonly CupertinoDynamicColor systemGrey5 = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemGrey5",
            color: Color.fromARGB(255, 229, 229, 234),
            darkColor: Color.fromARGB(255, 44, 44, 46),
            highContrastColor: Color.fromARGB(255, 216, 216, 220),
            darkHighContrastColor: Color.fromARGB(255, 54, 54, 56)
        );

        public static readonly CupertinoDynamicColor systemGrey6 = CupertinoDynamicColor.withBrightnessAndContrast(
            debugLabel: "systemGrey6",
            color: Color.fromARGB(255, 242, 242, 247),
            darkColor: Color.fromARGB(255, 28, 28, 30),
            highContrastColor: Color.fromARGB(255, 235, 235, 240),
            darkHighContrastColor: Color.fromARGB(255, 36, 36, 38)
        );

        public static readonly CupertinoDynamicColor label = new CupertinoDynamicColor(
            debugLabel: "label",
            effectiveColor:Color.fromARGB(255, 0, 0, 0),
            color: Color.fromARGB(255, 0, 0, 0),
            darkColor: Color.fromARGB(255, 255, 255, 255),
            highContrastColor: Color.fromARGB(255, 0, 0, 0),
            darkHighContrastColor: Color.fromARGB(255, 255, 255, 255),
            elevatedColor: Color.fromARGB(255, 0, 0, 0),
            darkElevatedColor: Color.fromARGB(255, 255, 255, 255),
            highContrastElevatedColor: Color.fromARGB(255, 0, 0, 0),
            darkHighContrastElevatedColor: Color.fromARGB(255, 255, 255, 255)
        );

        public static readonly CupertinoDynamicColor secondaryLabel = new CupertinoDynamicColor(
            debugLabel: "secondaryLabel",
            effectiveColor:Color.fromARGB(153, 60, 60, 67),
            color: Color.fromARGB(153, 60, 60, 67),
            darkColor: Color.fromARGB(153, 235, 235, 245),
            highContrastColor: Color.fromARGB(173, 60, 60, 67),
            darkHighContrastColor: Color.fromARGB(173, 235, 235, 245),
            elevatedColor: Color.fromARGB(153, 60, 60, 67),
            darkElevatedColor: Color.fromARGB(153, 235, 235, 245),
            highContrastElevatedColor: Color.fromARGB(173, 60, 60, 67),
            darkHighContrastElevatedColor: Color.fromARGB(173, 235, 235, 245)
        );

        /// The color for text labels containing tertiary content, equivalent to
        /// [UIColor.tertiaryLabel](https://developer.apple.com/documentation/uikit/uicolor/3173153-tertiarylabel).
        public static readonly CupertinoDynamicColor tertiaryLabel = new CupertinoDynamicColor(
            debugLabel: "tertiaryLabel",
            effectiveColor: Color.fromARGB(76, 60, 60, 67),
            color: Color.fromARGB(76, 60, 60, 67),
            darkColor: Color.fromARGB(76, 235, 235, 245),
            highContrastColor: Color.fromARGB(96, 60, 60, 67),
            darkHighContrastColor: Color.fromARGB(96, 235, 235, 245),
            elevatedColor: Color.fromARGB(76, 60, 60, 67),
            darkElevatedColor: Color.fromARGB(76, 235, 235, 245),
            highContrastElevatedColor: Color.fromARGB(96, 60, 60, 67),
            darkHighContrastElevatedColor: Color.fromARGB(96, 235, 235, 245)
        );

        public static readonly CupertinoDynamicColor quaternaryLabel = new CupertinoDynamicColor(
            debugLabel: "quaternaryLabel",
            effectiveColor:Color.fromARGB(45, 60, 60, 67),
            color: Color.fromARGB(45, 60, 60, 67),
            darkColor: Color.fromARGB(40, 235, 235, 245),
            highContrastColor: Color.fromARGB(66, 60, 60, 67),
            darkHighContrastColor: Color.fromARGB(61, 235, 235, 245),
            elevatedColor: Color.fromARGB(45, 60, 60, 67),
            darkElevatedColor: Color.fromARGB(40, 235, 235, 245),
            highContrastElevatedColor: Color.fromARGB(66, 60, 60, 67),
            darkHighContrastElevatedColor: Color.fromARGB(61, 235, 235, 245)
        );

        public static readonly CupertinoDynamicColor systemFill = new CupertinoDynamicColor(
            debugLabel: "systemFill",
            effectiveColor:Color.fromARGB(51, 120, 120, 128),
            color: Color.fromARGB(51, 120, 120, 128),
            darkColor: Color.fromARGB(91, 120, 120, 128),
            highContrastColor: Color.fromARGB(71, 120, 120, 128),
            darkHighContrastColor: Color.fromARGB(112, 120, 120, 128),
            elevatedColor: Color.fromARGB(51, 120, 120, 128),
            darkElevatedColor: Color.fromARGB(91, 120, 120, 128),
            highContrastElevatedColor: Color.fromARGB(71, 120, 120, 128),
            darkHighContrastElevatedColor: Color.fromARGB(112, 120, 120, 128)
        );

        public static readonly CupertinoDynamicColor secondarySystemFill = new CupertinoDynamicColor(
            debugLabel: "secondarySystemFill",
            effectiveColor: Color.fromARGB(40, 120, 120, 128),
            color: Color.fromARGB(40, 120, 120, 128),
            darkColor: Color.fromARGB(81, 120, 120, 128),
            highContrastColor: Color.fromARGB(61, 120, 120, 128),
            darkHighContrastColor: Color.fromARGB(102, 120, 120, 128),
            elevatedColor: Color.fromARGB(40, 120, 120, 128),
            darkElevatedColor: Color.fromARGB(81, 120, 120, 128),
            highContrastElevatedColor: Color.fromARGB(61, 120, 120, 128),
            darkHighContrastElevatedColor: Color.fromARGB(102, 120, 120, 128)
        );

        public static readonly CupertinoDynamicColor tertiarySystemFill = new CupertinoDynamicColor(
            debugLabel: "tertiarySystemFill",
            effectiveColor:Color.fromARGB(30, 118, 118, 128),
            color: Color.fromARGB(30, 118, 118, 128),
            darkColor: Color.fromARGB(61, 118, 118, 128),
            highContrastColor: Color.fromARGB(51, 118, 118, 128),
            darkHighContrastColor: Color.fromARGB(81, 118, 118, 128),
            elevatedColor: Color.fromARGB(30, 118, 118, 128),
            darkElevatedColor: Color.fromARGB(61, 118, 118, 128),
            highContrastElevatedColor: Color.fromARGB(51, 118, 118, 128),
            darkHighContrastElevatedColor: Color.fromARGB(81, 118, 118, 128)
        );

        public static readonly CupertinoDynamicColor quaternarySystemFill = new CupertinoDynamicColor(
            debugLabel: "quaternarySystemFill",
            effectiveColor:Color.fromARGB(20, 116, 116, 128),
            color: Color.fromARGB(20, 116, 116, 128),
            darkColor: Color.fromARGB(45, 118, 118, 128),
            highContrastColor: Color.fromARGB(40, 116, 116, 128),
            darkHighContrastColor: Color.fromARGB(66, 118, 118, 128),
            elevatedColor: Color.fromARGB(20, 116, 116, 128),
            darkElevatedColor: Color.fromARGB(45, 118, 118, 128),
            highContrastElevatedColor: Color.fromARGB(40, 116, 116, 128),
            darkHighContrastElevatedColor: Color.fromARGB(66, 118, 118, 128)
        );

        /// The color for placeholder text in controls or text views, equivalent to
        /// [UIColor.placeholderText](https://developer.apple.com/documentation/uikit/uicolor/3173134-placeholdertext).
        public static readonly CupertinoDynamicColor placeholderText = new CupertinoDynamicColor(
            debugLabel: "placeholderText",
            effectiveColor:Color.fromARGB(76, 60, 60, 67),
            color: Color.fromARGB(76, 60, 60, 67),
            darkColor: Color.fromARGB(76, 235, 235, 245),
            highContrastColor: Color.fromARGB(96, 60, 60, 67),
            darkHighContrastColor: Color.fromARGB(96, 235, 235, 245),
            elevatedColor: Color.fromARGB(76, 60, 60, 67),
            darkElevatedColor: Color.fromARGB(76, 235, 235, 245),
            highContrastElevatedColor: Color.fromARGB(96, 60, 60, 67),
            darkHighContrastElevatedColor: Color.fromARGB(96, 235, 235, 245)
        );

        public static readonly CupertinoDynamicColor systemBackground = new CupertinoDynamicColor(
            debugLabel: "systemBackground",
            effectiveColor:Color.fromARGB(255, 255, 255, 255),
            color: Color.fromARGB(255, 255, 255, 255),
            darkColor: Color.fromARGB(255, 0, 0, 0),
            highContrastColor: Color.fromARGB(255, 255, 255, 255),
            darkHighContrastColor: Color.fromARGB(255, 0, 0, 0),
            elevatedColor: Color.fromARGB(255, 255, 255, 255),
            darkElevatedColor: Color.fromARGB(255, 28, 28, 30),
            highContrastElevatedColor: Color.fromARGB(255, 255, 255, 255),
            darkHighContrastElevatedColor: Color.fromARGB(255, 36, 36, 38)
        );

        public static readonly CupertinoDynamicColor secondarySystemBackground = new CupertinoDynamicColor(
            debugLabel: "secondarySystemBackground",
            effectiveColor:Color.fromARGB(255, 242, 242, 247),
            color: Color.fromARGB(255, 242, 242, 247),
            darkColor: Color.fromARGB(255, 28, 28, 30),
            highContrastColor: Color.fromARGB(255, 235, 235, 240),
            darkHighContrastColor: Color.fromARGB(255, 36, 36, 38),
            elevatedColor: Color.fromARGB(255, 242, 242, 247),
            darkElevatedColor: Color.fromARGB(255, 44, 44, 46),
            highContrastElevatedColor: Color.fromARGB(255, 235, 235, 240),
            darkHighContrastElevatedColor: Color.fromARGB(255, 54, 54, 56)
        );

        public static readonly CupertinoDynamicColor tertiarySystemBackground = new CupertinoDynamicColor(
            debugLabel: "tertiarySystemBackground",
            effectiveColor:Color.fromARGB(255, 255, 255, 255),
            color: Color.fromARGB(255, 255, 255, 255),
            darkColor: Color.fromARGB(255, 44, 44, 46),
            highContrastColor: Color.fromARGB(255, 255, 255, 255),
            darkHighContrastColor: Color.fromARGB(255, 54, 54, 56),
            elevatedColor: Color.fromARGB(255, 255, 255, 255),
            darkElevatedColor: Color.fromARGB(255, 58, 58, 60),
            highContrastElevatedColor: Color.fromARGB(255, 255, 255, 255),
            darkHighContrastElevatedColor: Color.fromARGB(255, 68, 68, 70)
        );

        public static readonly CupertinoDynamicColor systemGroupedBackground = new CupertinoDynamicColor(
            debugLabel: "systemGroupedBackground",
            effectiveColor:Color.fromARGB(255, 242, 242, 247),
            color: Color.fromARGB(255, 242, 242, 247),
            darkColor: Color.fromARGB(255, 0, 0, 0),
            highContrastColor: Color.fromARGB(255, 235, 235, 240),
            darkHighContrastColor: Color.fromARGB(255, 0, 0, 0),
            elevatedColor: Color.fromARGB(255, 242, 242, 247),
            darkElevatedColor: Color.fromARGB(255, 28, 28, 30),
            highContrastElevatedColor: Color.fromARGB(255, 235, 235, 240),
            darkHighContrastElevatedColor: Color.fromARGB(255, 36, 36, 38)
        );

        public static readonly CupertinoDynamicColor secondarySystemGroupedBackground = new CupertinoDynamicColor(
            debugLabel: "secondarySystemGroupedBackground",
            effectiveColor:Color.fromARGB(255, 255, 255, 255),
            color: Color.fromARGB(255, 255, 255, 255),
            darkColor: Color.fromARGB(255, 28, 28, 30),
            highContrastColor: Color.fromARGB(255, 255, 255, 255),
            darkHighContrastColor: Color.fromARGB(255, 36, 36, 38),
            elevatedColor: Color.fromARGB(255, 255, 255, 255),
            darkElevatedColor: Color.fromARGB(255, 44, 44, 46),
            highContrastElevatedColor: Color.fromARGB(255, 255, 255, 255),
            darkHighContrastElevatedColor: Color.fromARGB(255, 54, 54, 56)
        );

        public static readonly CupertinoDynamicColor tertiarySystemGroupedBackground = new CupertinoDynamicColor(
            debugLabel: "tertiarySystemGroupedBackground",
            effectiveColor:Color.fromARGB(255, 242, 242, 247),
            color: Color.fromARGB(255, 242, 242, 247),
            darkColor: Color.fromARGB(255, 44, 44, 46),
            highContrastColor: Color.fromARGB(255, 235, 235, 240),
            darkHighContrastColor: Color.fromARGB(255, 54, 54, 56),
            elevatedColor: Color.fromARGB(255, 242, 242, 247),
            darkElevatedColor: Color.fromARGB(255, 58, 58, 60),
            highContrastElevatedColor: Color.fromARGB(255, 235, 235, 240),
            darkHighContrastElevatedColor: Color.fromARGB(255, 68, 68, 70)
        );

        public static readonly CupertinoDynamicColor separator = new CupertinoDynamicColor(
            debugLabel: "separator",
            effectiveColor:Color.fromARGB(73, 60, 60, 67),
            color: Color.fromARGB(73, 60, 60, 67),
            darkColor: Color.fromARGB(153, 84, 84, 88),
            highContrastColor: Color.fromARGB(94, 60, 60, 67),
            darkHighContrastColor: Color.fromARGB(173, 84, 84, 88),
            elevatedColor: Color.fromARGB(73, 60, 60, 67),
            darkElevatedColor: Color.fromARGB(153, 84, 84, 88),
            highContrastElevatedColor: Color.fromARGB(94, 60, 60, 67),
            darkHighContrastElevatedColor: Color.fromARGB(173, 84, 84, 88)
        );

        /// The color for borders or divider lines that hide any underlying content,
        /// equivalent to [UIColor.opaqueSeparator](https://developer.apple.com/documentation/uikit/uicolor/3173133-opaqueseparator).
        public static readonly CupertinoDynamicColor opaqueSeparator = new CupertinoDynamicColor(
            debugLabel: "opaqueSeparator",
            effectiveColor:Color.fromARGB(255, 198, 198, 200),
            color: Color.fromARGB(255, 198, 198, 200),
            darkColor: Color.fromARGB(255, 56, 56, 58),
            highContrastColor: Color.fromARGB(255, 198, 198, 200),
            darkHighContrastColor: Color.fromARGB(255, 56, 56, 58),
            elevatedColor: Color.fromARGB(255, 198, 198, 200),
            darkElevatedColor: Color.fromARGB(255, 56, 56, 58),
            highContrastElevatedColor: Color.fromARGB(255, 198, 198, 200),
            darkHighContrastElevatedColor: Color.fromARGB(255, 56, 56, 58)
        );

        public static readonly CupertinoDynamicColor link = 
            new CupertinoDynamicColor(
            debugLabel: "link",
            effectiveColor:Color.fromARGB(255, 0, 122, 255),
            color: Color.fromARGB(255, 0, 122, 255),
            darkColor: Color.fromARGB(255, 9, 132, 255),
            highContrastColor: Color.fromARGB(255, 0, 122, 255),
            darkHighContrastColor: Color.fromARGB(255, 9, 132, 255),
            elevatedColor: Color.fromARGB(255, 0, 122, 255),
            darkElevatedColor: Color.fromARGB(255, 9, 132, 255),
            highContrastElevatedColor: Color.fromARGB(255, 0, 122, 255),
            darkHighContrastElevatedColor: Color.fromARGB(255, 9, 132, 255)
        );
    }

    public class CupertinoDynamicColor : DiagnosticableMixinColor {
        public CupertinoDynamicColor(
            string debugLabel = null,
            Color effectiveColor = null,
            Color color = null,
            Color darkColor = null,
            Color highContrastColor = null,
            Color darkHighContrastColor = null,
            Color elevatedColor = null,
            Color darkElevatedColor = null,
            Color highContrastElevatedColor = null,
            Color darkHighContrastElevatedColor = null,
            Element debugResolveContext = null 
            ): base(0)
        {
            D.assert(color != null,()=>"color8 == null");
            D.assert(darkColor != null,()=>"color7 == null");
            D.assert(highContrastColor != null,()=>"color6 == null");
            D.assert(darkHighContrastColor != null,()=>"color5 == null");
            D.assert(elevatedColor != null,()=>"color4 == null");
            D.assert(darkElevatedColor != null,()=>"color3 == null");
            D.assert(highContrastElevatedColor != null,()=>"color2 == null");
            D.assert(darkHighContrastElevatedColor != null,()=>"color1 == null");
          

            _effectiveColor = effectiveColor ?? color;
            this.color = color;
            this.darkColor = darkColor;
            this.highContrastColor = highContrastColor;
            this.darkHighContrastColor = darkHighContrastColor;
            this.elevatedColor = elevatedColor;
            this.darkElevatedColor = darkElevatedColor;
            this.highContrastElevatedColor = highContrastElevatedColor;
            this.darkHighContrastElevatedColor = darkHighContrastElevatedColor;
            _debugResolveContext = debugResolveContext;
            _debugLabel = debugLabel;
        }

        

        public static CupertinoDynamicColor withBrightnessAndContrast(
            string debugLabel ,
            Color color ,
            Color darkColor ,
            Color highContrastColor ,
            Color darkHighContrastColor 
        ) {
            return new CupertinoDynamicColor(
                debugLabel: debugLabel,
                color: color,
                darkColor: darkColor,
                highContrastColor: highContrastColor,
                darkHighContrastColor: darkHighContrastColor,
                elevatedColor: color,
                darkElevatedColor: darkColor,
                highContrastElevatedColor: highContrastColor,
                darkHighContrastElevatedColor: darkHighContrastColor,
                debugResolveContext: null
            );
        }

        public static CupertinoDynamicColor withBrightness(
            string debugLabel = null,
            Color color = null,
            Color darkColor = null
        ) {
            return new CupertinoDynamicColor(
                debugLabel: debugLabel,
                color: color,
                darkColor: darkColor,
                highContrastColor: color,
                darkHighContrastColor: darkColor,
                elevatedColor: color,
                darkElevatedColor: darkColor,
                highContrastElevatedColor: color,
                darkHighContrastElevatedColor: darkColor,
                debugResolveContext: null
                );
        }

        public readonly Color _effectiveColor; 
        public new uint value {
            get {
                return _effectiveColor.value;
            }
        }


        public readonly string _debugLabel;
        public readonly Element _debugResolveContext;
        
        public readonly Color color;

        public readonly Color darkColor;

        public readonly Color highContrastColor;
        public readonly Color darkHighContrastColor;

        public readonly Color elevatedColor;

        public readonly Color darkElevatedColor;
        public readonly Color highContrastElevatedColor;
        public readonly Color darkHighContrastElevatedColor;

        
        public static Color resolve(Color resolvable, BuildContext context, bool nullOk = true) {
            if (resolvable == null)
                return null;
            D.assert(context != null);
            
            Color resolveColor = null;
            if (resolvable is CupertinoDynamicColor) {
                resolveColor = ((CupertinoDynamicColor) resolvable).resolveFrom(context, nullOk: nullOk)._effectiveColor;
            }
            else {
                resolveColor =  resolvable;
            }
            return resolveColor;
        }

        public bool _isPlatformBrightnessDependent {
            get {
                return color != darkColor
                       || elevatedColor != darkElevatedColor
                       || highContrastColor != darkHighContrastColor
                       || highContrastElevatedColor != darkHighContrastElevatedColor;
            }

        }

        public bool _isHighContrastDependent {
            get {
                return color != highContrastColor
                       || darkColor != darkHighContrastColor
                       || elevatedColor != highContrastElevatedColor
                       || darkElevatedColor != darkHighContrastElevatedColor;
            }
        }

        public bool _isInterfaceElevationDependent {
            get {
                return color != elevatedColor
                       || darkColor != darkElevatedColor
                       || highContrastColor != highContrastElevatedColor
                       || darkHighContrastColor != darkHighContrastElevatedColor;
            }
        }

        public CupertinoDynamicColor resolveFrom(BuildContext context, bool nullOk = true) {
            
            Brightness brightness = _isPlatformBrightnessDependent 
                ? CupertinoTheme.brightnessOf(context, nullOk: nullOk) ?? Brightness.light 
                : Brightness.light;

            bool isHighContrastEnabled = _isHighContrastDependent
                                         && (MediaQuery.of(context, nullOk: nullOk)?.highContrast ?? false);

            CupertinoUserInterfaceLevelData level = _isInterfaceElevationDependent
                ? CupertinoUserInterfaceLevel.of(context, nullOk: nullOk) ?? CupertinoUserInterfaceLevelData.baselayer
                : CupertinoUserInterfaceLevelData.baselayer;

            Color resolved = null;
            switch (brightness) {
                case Brightness.light:
                    switch (level) {
                        case CupertinoUserInterfaceLevelData.baselayer:
                            resolved = isHighContrastEnabled ? highContrastColor : color;
                            break;
                        case CupertinoUserInterfaceLevelData.elevatedlayer:
                            resolved = isHighContrastEnabled ? highContrastElevatedColor : elevatedColor;
                            break;
                    }

                    break;
                case Brightness.dark:
                    switch (level) {
                        case CupertinoUserInterfaceLevelData.baselayer:
                            resolved = isHighContrastEnabled ? darkHighContrastColor : darkColor;
                            break;
                        case CupertinoUserInterfaceLevelData.elevatedlayer:
                            resolved = isHighContrastEnabled ? darkHighContrastElevatedColor : darkElevatedColor;
                            break;
                    }

                    break;
            }

            Element _debugContext = null;
            D.assert(() => {
                    _debugContext = context as Element;
                    return true;
                }
            );
           
            return new CupertinoDynamicColor(
                debugLabel: _debugLabel,
                effectiveColor: resolved,
                color: color,
                darkColor: darkColor,
                highContrastColor: highContrastColor,
                darkHighContrastColor: darkHighContrastColor,
                elevatedColor: color,
                darkElevatedColor: darkColor,
                highContrastElevatedColor: highContrastColor,
                darkHighContrastElevatedColor: darkHighContrastColor,
                debugResolveContext: null
                );
        }

        public bool Equals(CupertinoDynamicColor other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(value, other.value) 
                   && Equals(color, other.color) 
                   && Equals(darkColor, other.darkColor) 
                   && Equals(highContrastColor, other.highContrastColor)
                   && Equals(darkHighContrastColor, other.darkHighContrastColor)
                   && Equals(elevatedColor, other.elevatedColor)
                   && Equals(darkElevatedColor, other.darkElevatedColor)
                   && Equals(highContrastElevatedColor, other.highContrastElevatedColor)
                   && Equals(darkHighContrastElevatedColor, other.darkHighContrastElevatedColor);
        }

        public override bool Equals(object obj) {

            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((CupertinoDynamicColor) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (value.GetHashCode());
                hashCode = (hashCode * 397) ^(color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (darkColor != null ? darkColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (highContrastColor != null ? highContrastColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (elevatedColor != null ? elevatedColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (darkElevatedColor != null ? darkElevatedColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (darkHighContrastColor != null ? darkHighContrastColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (darkHighContrastElevatedColor != null
                    ? darkHighContrastElevatedColor.GetHashCode()
                    : 0);
                hashCode = (hashCode * 397) ^
                           (highContrastElevatedColor != null ? highContrastElevatedColor.GetHashCode() : 0);
                return hashCode;
            }

        }

        public static bool operator ==(CupertinoDynamicColor left, CupertinoDynamicColor right) {
            return Equals(left, right);
        }

        public static bool operator !=(CupertinoDynamicColor left, CupertinoDynamicColor right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return toString();
        }

        public override string toString(DiagnosticLevel minLevel = DiagnosticLevel.debug) {
            string toStringColor(string name, Color color) {
                string marker = color == _effectiveColor ? "*" : "";
                return marker+ name+" = " + color + marker;
            }
            List<string> xs = new List<string>();
            xs.Add(toStringColor("color",color));
            if (_isPlatformBrightnessDependent)
                xs.Add(toStringColor("darkColor", darkColor));
            if (_isHighContrastDependent)
                xs.Add( toStringColor("highContrastColor", highContrastColor));
            if (_isPlatformBrightnessDependent && _isHighContrastDependent)
                xs.Add(toStringColor("darkHighContrastColor", darkHighContrastColor));
            if (_isInterfaceElevationDependent)
                xs.Add( toStringColor("elevatedColor", elevatedColor));
            if (_isPlatformBrightnessDependent && _isInterfaceElevationDependent)
                xs.Add(toStringColor("darkElevatedColor", darkElevatedColor));
            if (_isHighContrastDependent && _isInterfaceElevationDependent)
                xs.Add(toStringColor("highContrastElevatedColor", highContrastElevatedColor));
            if (_isPlatformBrightnessDependent && _isHighContrastDependent && _isInterfaceElevationDependent)
                xs.Add(toStringColor("darkHighContrastElevatedColor", darkHighContrastElevatedColor));
            
            string xsStr = "";
            foreach (var xss in xs) {
                xsStr += xss;
            }
            var debugContext = _debugResolveContext?.widget;
            if (_debugResolveContext?.widget == null) {
                return $"[{_debugLabel ?? GetType().ToString()}({xsStr}, resolved by: UNRESOLVED)]";
            }
            else {
                return $"[{_debugLabel ?? GetType().ToString()}({xsStr}, resolved by: {_debugResolveContext?.widget })]";
            }


        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            if (_debugLabel != null)
                properties.add(new MessageProperty("debugLabel", _debugLabel));
            properties.add(createCupertinoColorProperty("color", color));
            if (_isPlatformBrightnessDependent)
                properties.add(createCupertinoColorProperty("darkColor", darkColor));
            if (_isHighContrastDependent)
                properties.add(createCupertinoColorProperty("highContrastColor", highContrastColor));
            if (_isPlatformBrightnessDependent && _isHighContrastDependent)
                properties.add(createCupertinoColorProperty("darkHighContrastColor", darkHighContrastColor));
            if (_isInterfaceElevationDependent)
                properties.add(createCupertinoColorProperty("elevatedColor", elevatedColor));
            if (_isPlatformBrightnessDependent && _isInterfaceElevationDependent)
                properties.add(createCupertinoColorProperty("darkElevatedColor", darkElevatedColor));
            if (_isHighContrastDependent && _isInterfaceElevationDependent)
                properties.add(createCupertinoColorProperty("highContrastElevatedColor", highContrastElevatedColor));
            if (_isPlatformBrightnessDependent && _isHighContrastDependent && _isInterfaceElevationDependent)
                properties.add(createCupertinoColorProperty("darkHighContrastElevatedColor",
                    darkHighContrastElevatedColor));

            if (_debugResolveContext != null)
                properties.add(new DiagnosticsProperty<Element>("last resolved", _debugResolveContext));

        }

        public static DiagnosticsProperty<Color> createCupertinoColorProperty(
            string name ,
            Color value ,
            bool showName = true,
            object defaultValue = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) {

            if (value is CupertinoDynamicColor) {
                return new DiagnosticsProperty<Color>(
                    name: name,
                    value: value,
                    description: ((CupertinoDynamicColor)value)._debugLabel,
                    showName: showName,
                    defaultValue: defaultValue,
                    style: style,
                    level: level
                );
            }
            else {
                return new ColorProperty(
                    name,
                    value,
                    showName: showName,
                    defaultValue: defaultValue,
                    style: style,
                    level: level
                );
            }
        }
    }

   

}
