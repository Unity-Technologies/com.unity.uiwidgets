using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.DevTools.inspector;
using Unity.UIWidgets.DevTools.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.DevTools.inspector
{
    public class diagnosticsUtils
    {
      public static ColorIconMaker _colorIconMaker = new ColorIconMaker();
      public static CustomIconMaker _customIconMaker = new CustomIconMaker();
      public static CustomIcon defaultIcon = _customIconMaker.fromInfo("Default");

      public static readonly bool _showRenderObjectPropertiesAsLinks = false;
    }
    
public class DiagnosticsNodeDescription : StatelessWidget {
  public DiagnosticsNodeDescription(
    RemoteDiagnosticsNode diagnostic,
    bool isSelected = false,
    string errorText = null
  )
  {
    this.diagnostic = diagnostic;
    this.isSelected = isSelected;
    this.errorText = errorText;
  }

  public readonly RemoteDiagnosticsNode diagnostic;
  public readonly bool isSelected;
  public readonly string errorText;

  Widget _paddedIcon(Widget icon) {
    return new Padding(
      padding: EdgeInsets.only(right: iconPadding),
      child: icon
    );
  }

  IEnumerable<TextSpan> _buildDescriptionTextSpans(
    String description,
    TextStyle textStyle,
    ColorScheme colorScheme
  ) {
    if (diagnostic.isDiagnosticableValue) {
      var match = treeNodePrimaryDescriptionPattern.firstMatch(description);
      if (match != null) {
        yield return new TextSpan(text: match.group(1), style: textStyle);
        if (match.group(2).isNotEmpty()) {
          yield return new TextSpan(
            text: match.group(2),
            style: inspector_text_styles.unimportant(colorScheme)
          );
        }
        //return new List<TextSpan>();
      }
    } else if (diagnostic.type == "ErrorDescription") {
      var match = assertionThrownBuildingError.firstMatch(description);
      if (match != null) {
        yield return new TextSpan(text: match.group(1), style: textStyle);
        yield return new TextSpan(text: match.group(3), style: textStyle);
        // return;
      }
    }
    if (description?.isNotEmpty() == true) {
      yield return new TextSpan(text: description, style: textStyle);
    }
  }

  Widget buildDescription(
    string description,
    TextStyle textStyle,
    ColorScheme colorScheme, 
    bool isProperty = false
  ) {
    return new RichText(
      overflow: TextOverflow.ellipsis,
      text: new TextSpan(
        children: _buildDescriptionTextSpans(
          description,
          textStyle,
          colorScheme
        ).ToList()
      )
    );
  }
  
  public override Widget build(BuildContext context) {
    if (diagnostic == null) {
      return new SizedBox();
    }
    var colorScheme = Theme.of(context).colorScheme;
    Widget icon = diagnostic.icon;
    var children = new List<Widget>();

    if (icon != null) {
      children.Add(_paddedIcon(icon));
    }
    string name = diagnostic.name;

    TextStyle textStyle = DefaultTextStyle.of(context)
        .style
        .merge(InspectorControllerUtils.textStyleForLevel(diagnostic.level, colorScheme));
    if (diagnostic.isProperty) {
      // Display of inline properties.
      string propertyType = diagnostic.propertyType;
      Dictionary<string, object> properties = diagnostic.valuePropertiesJson;

      if (name?.isNotEmpty() == true && diagnostic.showName) {
        children.Add(new Text($"{name}{diagnostic.separator} ", style: textStyle));
      }

      if (diagnostic.isCreatedByLocalProject) {
        textStyle =
            textStyle.merge(inspector_text_styles.regularBold(colorScheme));
      }

      string description = diagnostic.description;
      if (propertyType != null && properties != null) {
        switch (propertyType) {
          case "Color":
            {
              int alpha = JsonUtils.getIntMember(properties, "alpha");
              int red = JsonUtils.getIntMember(properties, "red");
              int green = JsonUtils.getIntMember(properties, "green");
              int blue = JsonUtils.getIntMember(properties, "blue");
              string radix(int chan) => Convert.ToString(chan,16).PadLeft(2, '0');
              if (alpha == 255) {
                description = $"#{radix(red)}{radix(green)}{radix(blue)}";
              } else {
                description =
                    $"#{radix(alpha)}{radix(red)}{radix(green)}{radix(blue)}";
              }

              Color color = Color.fromARGB(alpha, red, green, blue);
              children.Add(_paddedIcon(diagnosticsUtils._colorIconMaker.getCustomIcon(color)));
              break;
            }

          case "IconData":
            {
              int codePoint =
                  JsonUtils.getIntMember(properties, "codePoint");
              if (codePoint > 0) {
                icon = FlutterMaterialIcons.getIconForCodePoint(
                  codePoint,
                  colorScheme
                );
                if (icon != null) {
                  children.Add(_paddedIcon(icon));
                }
              }
              break;
            }
        }
      }

      if (diagnosticsUtils._showRenderObjectPropertiesAsLinks &&
          propertyType == "RenderObject") {
        textStyle = textStyle.merge(inspector_text_styles.link(colorScheme));
      }

      // TODO(jacobr): custom display for units, iterables, and padding.
      children.Add(new Flexible(
        child: buildDescription(
          description,
          textStyle,
          colorScheme,
          isProperty: true
        )
      ));

      if (diagnostic.level == DiagnosticLevel.fine &&
          diagnostic.hasDefaultValue) {
        children.Add(new Text(" "));
        children.Add(_paddedIcon(diagnosticsUtils.defaultIcon));
      }
    } else {
      // Non property, regular node case.
      if (name?.isNotEmpty() == true && diagnostic.showName && name != "child") {
        if (name.StartsWith("child ")) {
          children.Add(new Text(
            name,
            style: inspector_text_styles.unimportant(colorScheme)
          ));
        } else {
          children.Add(new Text(name, style: textStyle));
        }

        if (diagnostic.showSeparator) {
          children.Add(new Text(
            diagnostic.separator,
            style: inspector_text_styles.unimportant(colorScheme)
          ));
          if (diagnostic.separator != " " &&
              diagnostic.description.isNotEmpty()) {
            children.Add(new Text(
              " ",
              style: inspector_text_styles.unimportant(colorScheme)
            ));
          }
        }
      }

      if (!diagnostic.isSummaryTree && diagnostic.isCreatedByLocalProject) {
        textStyle =
            textStyle.merge(inspector_text_styles.regularBold(colorScheme));
      }

      var diagnosticDescription = buildDescription(
        diagnostic.description,
        textStyle,
        colorScheme,
        isProperty: false
      );

      if (errorText != null) {
        // TODO(dantup): Find if there's a way to achieve this without
        //  the nested row.
        diagnosticDescription = new Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: new List<Widget>{
            diagnosticDescription,
            _buildErrorText(colorScheme),
          }
        );
      }

      children.Add(new Expanded(child: diagnosticDescription));
    }

    return new Row(mainAxisSize: MainAxisSize.min, children: children);
  }

  Flexible _buildErrorText(ColorScheme colorScheme) {
    return new Flexible(
      child: new RichText(
        textAlign: TextAlign.right,
        overflow: TextOverflow.ellipsis,
        text: new TextSpan(
          text: errorText,
          style: isSelected
              ? inspector_text_styles.regular
              : inspector_text_styles.error(colorScheme)
        )
      )
    );
  }
}
    
    
}