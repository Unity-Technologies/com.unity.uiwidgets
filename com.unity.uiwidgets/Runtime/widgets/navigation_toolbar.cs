using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public class NavigationToolbar : StatelessWidget {
        public NavigationToolbar(
            Key key = null,
            Widget leading = null, 
            Widget middle = null,
            Widget trailing = null, 
            bool centerMiddle = true, 
            float middleSpacing = kMiddleSpacing
            ) : base(key) {
            this.leading = leading;
            this.middle = middle;
            this.trailing = trailing;
            this.centerMiddle = centerMiddle;
            this.middleSpacing = middleSpacing;
        }

        public const float kMiddleSpacing = 16.0f;

        public readonly Widget leading;
        public readonly Widget middle;
        public readonly Widget trailing;
        public readonly bool centerMiddle;
        public readonly float middleSpacing;

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            List<Widget> children = new List<Widget>();

            if (leading != null) {
                children.Add(new LayoutId(id: _ToolbarSlot.leading, child: leading));
            }

            if (middle != null) {
                children.Add(new LayoutId(id: _ToolbarSlot.middle, child: middle));
            }

            if (trailing != null) {
                children.Add(new LayoutId(id: _ToolbarSlot.trailing, child: trailing));
            }

            TextDirection textDirection = Directionality.of(context);
            return new CustomMultiChildLayout(
                layoutDelegate: new _ToolbarLayout(
                    centerMiddle: centerMiddle,
                    middleSpacing: middleSpacing,
                    textDirection: textDirection
                ),
                children: children
            );
        }
    }

    enum _ToolbarSlot {
        leading,
        middle,
        trailing,
    }

    class _ToolbarLayout : MultiChildLayoutDelegate {
        public _ToolbarLayout(
            bool? centerMiddle = null,
            float? middleSpacing = null,
            TextDirection? textDirection = null
        ) {
            D.assert(textDirection != null);
            D.assert(middleSpacing != null);
            this.centerMiddle = centerMiddle ?? true;
            this.middleSpacing = middleSpacing  ?? 0.0f;
            this.textDirection = textDirection ?? TextDirection.ltr;
        }


        public readonly bool centerMiddle;

        public readonly float middleSpacing;

        public readonly TextDirection textDirection;

        public override void performLayout(Size size) {
            float leadingWidth = 0.0f;
            float trailingWidth = 0.0f;

            if (hasChild(_ToolbarSlot.leading)) {
                BoxConstraints constraints = new BoxConstraints(
                    minWidth: 0.0f,
                    maxWidth: size.width / 3.0f,
                    minHeight: size.height,
                    maxHeight: size.height
                );
                leadingWidth = layoutChild(_ToolbarSlot.leading, constraints).width;
                float leadingX = 0.0f;
                switch (textDirection) {
                    case TextDirection.rtl:
                        leadingX = size.width - leadingWidth;
                        break;
                    case TextDirection.ltr:
                        leadingX = 0.0f;
                        break;
                }

                positionChild(_ToolbarSlot.leading, new Offset(leadingX, 0.0f));
            }

            if (hasChild(_ToolbarSlot.trailing)) {
                BoxConstraints constraints = BoxConstraints.loose(size);
                Size trailingSize = layoutChild(_ToolbarSlot.trailing, constraints);
                float trailingX = 0.0f;
                switch (textDirection) {
                    case TextDirection.rtl:
                        trailingX = 0.0f;
                        break;
                    case TextDirection.ltr:
                        trailingX = size.width - trailingSize.width;
                        break;
                }

                float trailingY = (size.height - trailingSize.height) / 2.0f;
                trailingWidth = trailingSize.width;
                positionChild(_ToolbarSlot.trailing, new Offset(trailingX, trailingY));
            }

            if (hasChild(_ToolbarSlot.middle)) {
                float maxWidth = Mathf.Max(size.width - leadingWidth - trailingWidth - middleSpacing * 2.0f, 0.0f);
                BoxConstraints constraints = BoxConstraints.loose(size).copyWith(maxWidth: maxWidth);
                Size middleSize = layoutChild(_ToolbarSlot.middle, constraints);

                float middleStartMargin = leadingWidth + middleSpacing;
                float middleStart = middleStartMargin;
                float middleY = (size.height - middleSize.height) / 2.0f;
                
                if (centerMiddle) {
                    middleStart = (size.width - middleSize.width) / 2.0f;
                    if (middleStart + middleSize.width > size.width - trailingWidth) {
                        middleStart = size.width - trailingWidth - middleSize.width;
                    }
                    else if (middleStart < middleStartMargin) {
                        middleStart = middleStartMargin;
                    }
                }

                float middleX = 0.0f;
                switch (textDirection) {
                    case TextDirection.rtl:
                        middleX = size.width - middleSize.width - middleStart;
                        break;
                    case TextDirection.ltr:
                        middleX = middleStart;
                        break;
                }

                positionChild(_ToolbarSlot.middle, new Offset(middleX, middleY));
            }
        }

        public override bool shouldRelayout(MultiChildLayoutDelegate oldDelegate) {
            return ((_ToolbarLayout) oldDelegate).centerMiddle != centerMiddle
                   || ((_ToolbarLayout) oldDelegate).middleSpacing != middleSpacing
                   || ((_ToolbarLayout) oldDelegate).textDirection != textDirection;
        }
    }
}