using System;
using System.Collections.Generic;
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

    public static class DiagnosticsUtils
    {
        public static ColorIconMaker _colorIconMaker = new ColorIconMaker();
        public static CustomIconMaker _customIconMaker = new CustomIconMaker();
        public static readonly bool _showRenderObjectPropertiesAsLinks = false;
        public static CustomIcon defaultIcon = _customIconMaker.fromInfo("Default");
    }
  
    class DiagnosticsNodeDescription : StatelessWidget {
      public DiagnosticsNodeDescription(RemoteDiagnosticsNode diagnostic)
      {
        this.diagnostic = diagnostic;
      }

      public readonly RemoteDiagnosticsNode diagnostic;

      Widget _paddedIcon(Widget icon) {
        return new Padding(
          padding: EdgeInsets.only(right: InspectorTreeUtils.iconPadding),
          child: icon
        );
      }

    void _addDescription(
      List<Widget> output,
      String description,
      TextStyle textStyle,
      ColorScheme colorScheme, 
      bool isProperty = false
    ) {
      if (diagnostic.isDiagnosticableValue) {
        var match = InspectorTreeUtils.treeNodePrimaryDescriptionPattern.Match(description);
        if (match != null) {
          output.Add(new Text(match.Groups[1].Value, style: textStyle));
          if (match.Groups[2].Value.isNotEmpty()) {
            output.Add(new Text(
              match.Groups[2].Value,
              style: inspector_text_styles.unimportant(colorScheme)
            ));
          }
          return;
        }
      } else if (diagnostic.type == "ErrorDescription") {
        var match = InspectorTreeUtils.assertionThrownBuildingError.Match(description);
        if (match != null) {
          output.Add(new Text(match.Groups[1].Value, style: textStyle));
          output.Add(new Text(match.Groups[3].Value, style: textStyle));
          return;
        }
      }
      if (description?.isNotEmpty() == true) {
        output.Add(new Text(description, style: textStyle));
      }
    }

  
  public override Widget build(BuildContext context) {
    if (diagnostic == null) {
      return new SizedBox();
    }
    var colorScheme = Theme.of(context).colorScheme;
    var icon = diagnostic.icon;
    var children = new List<Widget>();
    
    if (icon != null) {
      children.Add(_paddedIcon(icon));
    }
    string name = diagnostic.name;
    TextStyle textStyle = InspectorControllerUtils.textStyleForLevel(diagnostic.level, colorScheme);
    if (diagnostic.isProperty)
    {
      // Display of inline properties.
      string propertyType = diagnostic.propertyType;
      Dictionary<string, object> properties = diagnostic.valuePropertiesJson;

      if (name?.isNotEmpty() == true && diagnostic.showName)
      {
        children.Add(new Text($"{name}{diagnostic.separator} ", style: textStyle));
      }

      if (diagnostic.isCreatedByLocalProject) {
        textStyle =
            textStyle.merge(inspector_text_styles.regularBold(colorScheme));
      }
      
        string description = diagnostic.description;
        if (propertyType != null && properties != null)
        {
          switch (propertyType) {
            case "Color":
              {
                int alpha = JsonUtils.getIntMember(properties, "alpha");
                int red = JsonUtils.getIntMember(properties, "red");
                int green = JsonUtils.getIntMember(properties, "green");
                int blue = JsonUtils.getIntMember(properties, "blue");
                // string radix(int chan) => chan.toRadixString(16).padLeft(2, '0');
                string radix(int chan) => chan.ToString();
                if (alpha == 255) {
                  description = $"#{radix(red)}{radix(green)}{radix(blue)}";
                } else {
                  description =
                      $"#{radix(alpha)}{radix(red)}{radix(green)}{radix(blue)}";
                }
      
                Color color = Color.fromARGB(alpha, red, green, blue);
                children.Add(_paddedIcon(DiagnosticsUtils._colorIconMaker.getCustomIcon(color)));
                break;
              }
      
            case "IconData":
              {
                int codePoint =
                    JsonUtils.getIntMember(properties, "codePoint");
                if (codePoint > 0) {
                  var icon_ = FlutterMaterialIcons.getIconForCodePoint(
                    codePoint,
                    colorScheme
                  );
                  if (icon_ != null) {
                    children.Add(_paddedIcon(icon_));
                  }
                }
                break;
              }
          }
        }
      
        if (DiagnosticsUtils._showRenderObjectPropertiesAsLinks 
            && propertyType == "RenderObject") {
              // textStyle = textStyle..merge(inspector_text_styles.link(colorScheme));
        }
      
        // TODO(jacobr): custom display for units, iterables, and padding.
        _addDescription(
          children,
          description,
          textStyle,
          colorScheme,
          isProperty: true
        );
      
        if (diagnostic.level == DiagnosticLevel.fine &&
            diagnostic.hasDefaultValue) {
          children.Add(new Text(" "));
          children.Add(_paddedIcon(DiagnosticsUtils.defaultIcon));
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
      
        _addDescription(
          children,
          diagnostic.description,
          textStyle,
          colorScheme,
          isProperty: false
        );
      }

      return new Row(mainAxisSize: MainAxisSize.min, children: children);
    }
  }

}