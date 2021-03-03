using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using StrutStyle = Unity.UIWidgets.painting.StrutStyle;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    
    public class DefaultTextStyle : InheritedTheme { 
        public DefaultTextStyle(
            Widget child ,
            TextStyle style ,
            Key key = null,
            TextAlign? textAlign = null,
            bool softWrap = true,
            TextOverflow overflow = TextOverflow.clip,
            int? maxLines = null,
            TextWidthBasis textWidthBasis = TextWidthBasis.parent,
            ui.TextHeightBehavior textHeightBehavior = null
            
        ) : base(key: key, child: child) {
            D.assert(style != null);
            D.assert(maxLines == null || maxLines > 0);
            D.assert(child != null);
            
            this.style = style;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.maxLines = maxLines;
            this.textWidthBasis = textWidthBasis;
            this.textHeightBehavior = textHeightBehavior;
        }

        public DefaultTextStyle (
            Key key = null) : base(key: key, child: null) {
            style = new TextStyle();
            textAlign = null;
            softWrap = true;
            maxLines = null;
            overflow = TextOverflow.clip;
            textWidthBasis = TextWidthBasis.parent;
            textHeightBehavior = null;
           
        }


        public static Widget merge(
            Widget child,
            Key key = null,
            TextStyle style = null,
            TextAlign? textAlign = null,
            bool? softWrap = null,
            TextOverflow? overflow = null,
            int? maxLines = null,
            TextWidthBasis? textWidthBasis = null,
            ui.TextHeightBehavior textHeightBehavior = null
        ) {
            D.assert(child != null);
            return new Builder(
                builder: (BuildContext context) => {
                    DefaultTextStyle parent = DefaultTextStyle.of(context);
                    return new DefaultTextStyle(
                        key: key,
                        style: parent.style.merge(style),
                        textAlign: textAlign ?? parent.textAlign,
                        softWrap: softWrap ?? parent.softWrap,
                        overflow: overflow ?? parent.overflow,
                        maxLines: maxLines ?? parent.maxLines,
                        textWidthBasis: textWidthBasis ?? parent.textWidthBasis,
                        child: child
                    );
                }
            );
        }

        public readonly TextStyle style;
        public readonly TextAlign? textAlign;
        public readonly bool softWrap;
        public readonly TextOverflow overflow;
        public readonly int? maxLines;
        public readonly TextWidthBasis textWidthBasis;
        public readonly ui.TextHeightBehavior textHeightBehavior;

        public static DefaultTextStyle of(BuildContext context) {
            return context.dependOnInheritedWidgetOfExactType<DefaultTextStyle>() ?? new DefaultTextStyle();
        }
        public override bool updateShouldNotify(InheritedWidget oldWidget) { 
            DefaultTextStyle _oldWidget = (DefaultTextStyle) oldWidget;
            return style !=  _oldWidget.style ||
                   textAlign != _oldWidget.textAlign ||
                   softWrap != _oldWidget.softWrap ||
                   overflow != _oldWidget.overflow ||
                   maxLines != _oldWidget.maxLines ||
                   textWidthBasis != _oldWidget.textWidthBasis ||
                   textHeightBehavior != _oldWidget.textHeightBehavior;
        }
        public override Widget wrap(BuildContext context, Widget child) {
    
            DefaultTextStyle defaultTextStyle = context.findAncestorWidgetOfExactType<DefaultTextStyle>();
            return this == defaultTextStyle ? child : new DefaultTextStyle(
              style: style,
              textAlign: textAlign,
              softWrap: softWrap,
              overflow: overflow,
              maxLines: maxLines,
              textWidthBasis: textWidthBasis,
              textHeightBehavior: textHeightBehavior,
              child: child
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            style?.debugFillProperties(properties);
            properties.add(new EnumProperty<TextAlign?>("textAlign", textAlign, defaultValue: null));
            properties.add(new FlagProperty("softWrap", value: softWrap, ifTrue: "wrapping at box width", ifFalse: "no wrapping except at line break characters", showName: true));
            properties.add(new EnumProperty<TextOverflow>("overflow", overflow, defaultValue: null));
            properties.add(new IntProperty("maxLines", maxLines, defaultValue: null));
            properties.add(new EnumProperty<TextWidthBasis>("textWidthBasis", textWidthBasis, defaultValue: TextWidthBasis.parent));
            properties.add(new DiagnosticsProperty<ui.TextHeightBehavior>("textHeightBehavior", textHeightBehavior, defaultValue: null));
        }

        
    }

    public class Text : StatelessWidget {
        public Text(string data,
            Key key = null,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            TextAlign? textAlign = null,
            bool? softWrap = null,
            TextOverflow? overflow = null,
            float? textScaleFactor = null,
            int? maxLines = null,
            TextWidthBasis textWidthBasis = TextWidthBasis.parent,
            ui.TextHeightBehavior textHeightBehavior = null) : base(key) {
            D.assert(data != null, () => "A non-null string must be provided to a Text widget.");
            textSpan = null;
            this.data = data;
            this.style = style;
            this.strutStyle = strutStyle;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
            this.maxLines = maxLines;
            this.textWidthBasis = textWidthBasis;
            this.textHeightBehavior = textHeightBehavior;
        }

        Text(TextSpan textSpan,
            Key key = null,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            TextAlign? textAlign = null,
            bool? softWrap = null,
            TextOverflow? overflow = null,
            float? textScaleFactor = null,
            int? maxLines = null,
            TextWidthBasis textWidthBasis = default,
            ui.TextHeightBehavior textHeightBehavior = null) : base(key) {
            D.assert(textSpan != null, () => "A non-null TextSpan must be provided to a Text.rich widget.");
            this.textSpan = textSpan;
            data = null;
            this.style = style;
            this.strutStyle = strutStyle;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
            this.maxLines = maxLines;
            this.textHeightBehavior = textHeightBehavior;
            this.textWidthBasis = textWidthBasis;
        }

        public static Text rich(TextSpan textSpan,
            Key key = null,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            TextAlign? textAlign = null,
            bool? softWrap = null,
            TextOverflow? overflow = null,
            float? textScaleFactor = null,
            int? maxLines = null) {
            return new Text(
                textSpan, key,
                style,
                strutStyle,
                textAlign,
                softWrap,
                overflow,
                textScaleFactor,
                maxLines);
        }

        public readonly string data;

        public readonly InlineSpan textSpan;

        public readonly TextStyle style;

        public readonly StrutStyle strutStyle;

        public readonly TextAlign? textAlign;

        public readonly bool? softWrap;

        public readonly TextOverflow? overflow;

        public readonly float? textScaleFactor;

        public readonly int? maxLines;
        
        public readonly TextWidthBasis? textWidthBasis;
        
        public readonly ui.TextHeightBehavior textHeightBehavior;

        public override Widget build(BuildContext context) {
            DefaultTextStyle defaultTextStyle = DefaultTextStyle.of(context);
            TextStyle effectiveTextStyle = style;
            if (style == null || style.inherit) {
                effectiveTextStyle = defaultTextStyle.style.merge(style);
            }

            return new RichText(
                textAlign: textAlign ?? defaultTextStyle.textAlign ?? TextAlign.left,
                softWrap: softWrap ?? defaultTextStyle.softWrap,
                overflow: overflow ?? defaultTextStyle.overflow,
                textScaleFactor: textScaleFactor ?? MediaQuery.textScaleFactorOf(context),
                maxLines: maxLines ?? defaultTextStyle.maxLines,
                strutStyle: strutStyle,
                textWidthBasis: textWidthBasis ?? defaultTextStyle.textWidthBasis,
                textHeightBehavior: textHeightBehavior ?? defaultTextStyle.textHeightBehavior,
                text: new TextSpan(
                    style: effectiveTextStyle,
                    text: data,
                    children: textSpan != null ? new List<InlineSpan> {textSpan} : null
                )
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("data", data, showName: false));
            if (textSpan != null) {
                properties.add(
                    textSpan.toDiagnosticsNode(name: "textSpan", style: DiagnosticsTreeStyle.transition));
            }

            if (style != null) {
                style.debugFillProperties(properties);
            }

            properties.add(new EnumProperty<TextAlign?>("textAlign", textAlign,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new FlagProperty("softWrap", value: softWrap, ifTrue: "wrapping at box width",
                ifFalse: "no wrapping except at line break characters", showName: true));
            properties.add(new EnumProperty<TextOverflow?>("overflow", overflow,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new FloatProperty("textScaleFactor", textScaleFactor,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new IntProperty("maxLines", maxLines, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<TextWidthBasis?>("textWidthBasis", textWidthBasis, defaultValue: null));
            properties.add(new DiagnosticsProperty<ui.TextHeightBehavior>("textHeightBehavior", textHeightBehavior, defaultValue: null));
        }
    }
}