using System;
using System.Collections.Generic;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.ui
{

    public static class LabelUtils
    {
        public static bool _showLabelText(BuildContext context, double includeTextWidth) {
            return includeTextWidth == null ||
                   MediaQuery.of(context).size.width > includeTextWidth;
        }
    }
    
    class ImageIconLabel : StatelessWidget {
    public ImageIconLabel(Image icon, string text, float minIncludeTextWidth = 100)
    {
        this.icon = icon;
        this.text = text;
        this.minIncludeTextWidth = minIncludeTextWidth;
    }

    public readonly Image icon;
    public readonly string text;
    public readonly float minIncludeTextWidth;

   
    public override Widget build(BuildContext context)
    {
        List<Widget> temp = new List<Widget>();
        temp.Add(icon);
        if (LabelUtils._showLabelText(context, minIncludeTextWidth))
        {
            temp.Add(new Padding(
                padding: EdgeInsets.only(left: 8.0f),
                child: new Text(text)
                ));
        }
        return new Row(
            children: temp
        );
    }
}

class MaterialIconLabel : StatelessWidget {
    public MaterialIconLabel(IconData iconData, string text, float includeTextWidth = 10)
    {
        this.iconData = iconData;
        this.text = text;
        this.includeTextWidth = includeTextWidth;
    }

    public readonly IconData iconData;
    public readonly string text;
    public readonly float includeTextWidth;

    
    public override Widget build(BuildContext context)
    {
        List<Widget> temp = new List<Widget>();
        temp.Add(new Icon(iconData, size: 18.0f));
        if (LabelUtils._showLabelText(context, includeTextWidth))
        {
            temp.Add(
                new Padding(
                    padding: EdgeInsets.only(left: 8.0f),
                    child: new Text(text)
                )
            );
        }
        return new Row(
            children: temp
            );
    }
}



}