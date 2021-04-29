using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class ButtonBar : StatelessWidget {
        public ButtonBar(
            Key key = null,
            MainAxisAlignment alignment = MainAxisAlignment.end,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            ButtonTextTheme? buttonTextTheme = null,
            float? buttonMinWidth = null,
            float? buttonHeight = null,
            EdgeInsetsGeometry buttonPadding = null,
            bool? buttonAlignedDropdown = null,
            ButtonBarLayoutBehavior? layoutBehavior = null,
            VerticalDirection? overflowDirection = null,
            float? overflowButtonSpacing = null,
            List<Widget> children = null
        ) : base(key: key) {
            D.assert(buttonMinWidth == null || buttonMinWidth >= 0.0f);
            D.assert(buttonHeight == null || buttonHeight >= 0.0f);
            D.assert(overflowButtonSpacing == null || overflowButtonSpacing >= 0.0f);
            this.alignment = alignment;
            this.mainAxisSize = mainAxisSize;
            this.buttonTextTheme = buttonTextTheme;
            this.buttonMinWidth = buttonMinWidth;
            this.buttonHeight = buttonHeight;
            this.buttonPadding = buttonPadding;
            this.buttonAlignedDropdown = buttonAlignedDropdown;
            this.layoutBehavior = layoutBehavior;
            this.overflowDirection = overflowDirection;
            this.overflowButtonSpacing = overflowButtonSpacing;
            this.children = children ?? new List<Widget>();
        }

        public readonly MainAxisAlignment? alignment;

        public readonly MainAxisSize? mainAxisSize;

        public readonly ButtonTextTheme? buttonTextTheme;

        public readonly float? buttonMinWidth;

        public readonly float? buttonHeight;

        public readonly EdgeInsetsGeometry buttonPadding;

        public readonly bool? buttonAlignedDropdown;

        public readonly ButtonBarLayoutBehavior? layoutBehavior;

        public readonly VerticalDirection? overflowDirection;

        public readonly float? overflowButtonSpacing;


        public readonly List<Widget> children;


        public override Widget build(BuildContext context) {
            ButtonThemeData parentButtonTheme = ButtonTheme.of(context);
            ButtonBarThemeData barTheme = ButtonBarTheme.of(context);

            ButtonThemeData buttonTheme = parentButtonTheme.copyWith(
                textTheme: buttonTextTheme ?? barTheme?.buttonTextTheme ?? ButtonTextTheme.primary,
                minWidth: buttonMinWidth ?? barTheme?.buttonMinWidth ?? 64.0f,
                height: buttonHeight ?? barTheme?.buttonHeight ?? 36.0f,
                padding: buttonPadding ?? barTheme?.buttonPadding ?? EdgeInsets.symmetric(horizontal: 8.0f),
                alignedDropdown: buttonAlignedDropdown ?? barTheme?.buttonAlignedDropdown ?? false,
                layoutBehavior: layoutBehavior ?? barTheme?.layoutBehavior ?? ButtonBarLayoutBehavior.padded
            );

            float paddingUnit = buttonTheme.padding.horizontal / 4.0f;
            Widget child = ButtonTheme.fromButtonThemeData(
                data: buttonTheme,
                child: new _ButtonBarRow(
                    mainAxisAlignment: alignment ?? barTheme?.alignment ?? MainAxisAlignment.end,
                    mainAxisSize: mainAxisSize ?? barTheme?.mainAxisSize ?? MainAxisSize.max,
                    overflowDirection: overflowDirection ?? barTheme?.overflowDirection ?? VerticalDirection.down,
                    children: LinqUtils<Widget>.SelectList(children,((Widget childWidget) => {
                        return (Widget) new Padding(
                            padding: EdgeInsets.symmetric(horizontal: paddingUnit),
                            child: childWidget
                        );
                    })),
                    overflowButtonSpacing: overflowButtonSpacing
                )
            );

            switch (buttonTheme.layoutBehavior) {
                case ButtonBarLayoutBehavior.padded:
                    return new Padding(
                        padding: EdgeInsets.symmetric(
                            vertical: 2.0f * paddingUnit,
                            horizontal: paddingUnit
                        ),
                        child: child
                    );
                case ButtonBarLayoutBehavior.constrained:
                    return new Container(
                        padding: EdgeInsets.symmetric(horizontal: paddingUnit),
                        constraints: new BoxConstraints(minHeight: 52.0f),
                        alignment: Alignment.center,
                        child: child
                    );
            }

            D.assert(false);
            return null;
        }
    }

    internal class _ButtonBarRow : Flex {
        internal _ButtonBarRow(
            List<Widget> children,
            Axis direction = Axis.horizontal,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.start,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.center,
            TextDirection? textDirection = null,
            VerticalDirection overflowDirection = VerticalDirection.down,
            TextBaseline? textBaseline = null,
            float? overflowButtonSpacing = null
        ) : base(
            children: children,
            direction: direction,
            mainAxisSize: mainAxisSize,
            mainAxisAlignment: mainAxisAlignment,
            crossAxisAlignment: crossAxisAlignment,
            textDirection: textDirection,
            verticalDirection: overflowDirection,
            textBaseline: textBaseline
        ) {
            this.overflowButtonSpacing = overflowButtonSpacing;
        }

        public readonly float? overflowButtonSpacing;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderButtonBarRow(
                direction: direction,
                mainAxisAlignment: mainAxisAlignment,
                mainAxisSize: mainAxisSize,
                crossAxisAlignment: crossAxisAlignment,
                textDirection: getEffectiveTextDirection(context),
                verticalDirection: verticalDirection,
                textBaseline: textBaseline ?? TextBaseline.alphabetic,
                overflowButtonSpacing: overflowButtonSpacing
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            if (renderObject is _RenderButtonBarRow renderButtonBarRow) {
                renderButtonBarRow.direction = direction;
                renderButtonBarRow.mainAxisAlignment = mainAxisAlignment;
                renderButtonBarRow.mainAxisSize = mainAxisSize;
                renderButtonBarRow.crossAxisAlignment = crossAxisAlignment;
                renderButtonBarRow.textDirection = getEffectiveTextDirection(context);
                renderButtonBarRow.verticalDirection = verticalDirection;
                renderButtonBarRow.textBaseline = textBaseline ?? TextBaseline.alphabetic;
                renderButtonBarRow.overflowButtonSpacing = overflowButtonSpacing;
            }
        }
    }

    internal class _RenderButtonBarRow : RenderFlex {
        internal _RenderButtonBarRow(
            List<RenderBox> children = null,
            Axis direction = Axis.horizontal,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.start,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.center,
            TextDirection textDirection = TextDirection.ltr,
            VerticalDirection verticalDirection = VerticalDirection.down,
            TextBaseline textBaseline = TextBaseline.alphabetic,
            float? overflowButtonSpacing = null
        ) : base(
            children: children,
            direction: direction,
            mainAxisSize: mainAxisSize,
            mainAxisAlignment: mainAxisAlignment,
            crossAxisAlignment: crossAxisAlignment,
            textDirection: textDirection,
            verticalDirection: verticalDirection,
            textBaseline: textBaseline
        ) {
            D.assert(overflowButtonSpacing == null || overflowButtonSpacing >= 0);
            this.overflowButtonSpacing = overflowButtonSpacing;
        }

        bool _hasCheckedLayoutWidth = false;
        public float? overflowButtonSpacing;

        public new BoxConstraints constraints {
            get {
                if (_hasCheckedLayoutWidth)
                    return base.constraints;
                return base.constraints.copyWith(maxWidth: float.PositiveInfinity);
            }
        }


        protected override void performLayout() {
            _hasCheckedLayoutWidth = false;

            base.performLayout();
            _hasCheckedLayoutWidth = true;

            if (size.width <= constraints.maxWidth) {
                base.performLayout();
            }
            else {
                BoxConstraints childConstraints = constraints.copyWith(minWidth: 0.0f);
                RenderBox child = null;
                float currentHeight = 0.0f;
                switch (verticalDirection) {
                    case VerticalDirection.down:
                        child = firstChild;
                        break;
                    case VerticalDirection.up:
                        child = lastChild;
                        break;
                }

                while (child != null) {
                    FlexParentData childParentData = child.parentData as FlexParentData;

                    child.layout(childConstraints, parentUsesSize: true);

                    switch (textDirection) {
                        case TextDirection.ltr:
                            switch (mainAxisAlignment) {
                                case MainAxisAlignment.center:
                                    float midpoint = (constraints.maxWidth - child.size.width) / 2.0f;
                                    childParentData.offset = new Offset(midpoint, currentHeight);
                                    break;
                                case MainAxisAlignment.end:
                                    childParentData.offset = new Offset(constraints.maxWidth - child.size.width,
                                        currentHeight);
                                    break;
                                default:
                                    childParentData.offset = new Offset(0, currentHeight);
                                    break;
                            }

                            break;
                        case TextDirection.rtl:
                            switch (mainAxisAlignment) {
                                case MainAxisAlignment.center:
                                    float midpoint = constraints.maxWidth / 2.0f - child.size.width / 2.0f;
                                    childParentData.offset = new Offset(midpoint, currentHeight);
                                    break;
                                case MainAxisAlignment.end:
                                    childParentData.offset = new Offset(0, currentHeight);
                                    break;
                                default:
                                    childParentData.offset = new Offset(constraints.maxWidth - child.size.width,
                                        currentHeight);
                                    break;
                            }

                            break;
                    }

                    currentHeight += child.size.height;
                    switch (verticalDirection) {
                        case VerticalDirection.down:
                            child = childParentData.nextSibling;
                            break;
                        case VerticalDirection.up:
                            child = childParentData.previousSibling;
                            break;
                    }

                    if (overflowButtonSpacing != null && child != null)
                        currentHeight += overflowButtonSpacing ?? 0;
                }

                size = constraints.constrain(new Size(constraints.maxWidth, currentHeight));
            }
        }
    }
}