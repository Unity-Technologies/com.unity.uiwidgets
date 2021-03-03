using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class Divider : StatelessWidget {
        public Divider(
            Key key = null,
            float? height = null,
            float? thickness = null,
            float? indent = null,
            float? endIndent = null,
            Color color = null) : base(key: key) {
            D.assert(height == null || height >= 0.0);
            D.assert(thickness == null || thickness >= 0.0);
            D.assert(indent == null || indent >= 0.0);
            D.assert(endIndent == null || endIndent >= 0.0);
            this.height = height;
            this.thickness = thickness;
            this.indent = indent;
            this.endIndent = endIndent;
            this.color = color;
        }

        public readonly float? height;

        public readonly float? thickness;

        public readonly float? indent;

        public readonly float? endIndent;

        public readonly Color color;

        public static BorderSide createBorderSide(BuildContext context, Color color = null, float? width = null) {
            Color effectiveColor = color
                                   ?? (context != null
                                       ? (DividerTheme.of(context)?.color ?? Theme.of(context).dividerColor)
                                       : null);
            float effectiveWidth = width
                                   ?? (context != null ? DividerTheme.of(context)?.thickness : null)
                                   ?? 0.0f;

            // Prevent assertion since it is possible that context is null and no color
            // is specified.
            if (effectiveColor == null) {
                return new BorderSide(
                    width: effectiveWidth
                );
            }

            return new BorderSide(
                color: effectiveColor,
                width: effectiveWidth
            );
        }

        public override Widget build(BuildContext context) {
            DividerThemeData dividerTheme = DividerTheme.of(context);
            float height = this.height ?? dividerTheme?.space ?? 16.0f;
            float thickness = this.thickness ?? dividerTheme?.thickness ?? 0.0f;
            float indent = this.indent ?? dividerTheme?.indent ?? 0.0f;
            float endIndent = this.endIndent ?? dividerTheme?.endIndent ?? 0.0f;

            return new SizedBox(
                height: height,
                child: new Center(
                    child: new Container(
                        height: thickness,
                        //TODO: update to EdgeInsetsGeometry
                        margin: EdgeInsetsDirectional.only(start: indent,
                            end: endIndent),
                        decoration: new BoxDecoration(
                            border: new Border(
                                bottom: createBorderSide(context, color: color, width: thickness))
                        )
                    )
                )
            );
        }
    }

    public class VerticalDivider : StatelessWidget {
        public VerticalDivider(
            Key key = null,
            float? width = null,
            float? thickness = null,
            float? indent = null,
            float? endIndent = null,
            Color color = null
        ) : base(key) {
            D.assert(width == null || width >= 0.0);
            D.assert(thickness == null || thickness >= 0.0);
            D.assert(indent == null || indent >= 0.0);
            D.assert(endIndent == null || endIndent >= 0.0);
            this.width = width;
            this.thickness = thickness;
            this.indent = indent;
            this.endIndent = endIndent;
            this.color = color;
        }

        public readonly float? width;
        public readonly float? thickness;
        public readonly float? indent;
        public readonly float? endIndent;
        public readonly Color color;

        public override Widget build(BuildContext context) {
            DividerThemeData dividerTheme = DividerTheme.of(context);
            float width = this.width ?? dividerTheme.space ?? 16.0f;
            float thickness = this.thickness ?? dividerTheme.thickness ?? 0.0f;
            float indent = this.indent ?? dividerTheme.indent ?? 0.0f;
            float endIndent = this.endIndent ?? dividerTheme.endIndent ?? 0.0f;

            return new SizedBox(
                width: width,
                child: new Center(
                    child: new Container(
                        width: thickness,
                        margin: EdgeInsetsDirectional.only(top: indent,
                            bottom: endIndent),
                        decoration: new BoxDecoration(
                            border: new Border(
                                left: Divider.createBorderSide(context, color: color, width: thickness)
                            )
                        )
                    )
                )
            );
        }
    }
}