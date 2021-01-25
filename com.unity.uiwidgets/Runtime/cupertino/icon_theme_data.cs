using System.Runtime.CompilerServices;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Brightness = Unity.UIWidgets.ui.Brightness;

namespace Unity.UIWidgets.cupertino {
    class CupertinoIconThemeData : IconThemeData  {
        
        public CupertinoIconThemeData(
            Color color = null,
            float? opacity = null,
            float? size = null
        ) : base(color: color, opacity: opacity, size: size) {
           
        }

        public new IconThemeData resolve(BuildContext context) {
            Color resolvedColor = CupertinoDynamicColor.resolve(color, context);
            return resolvedColor == color ? this : copyWith(color: resolvedColor);
        }
        public new CupertinoIconThemeData copyWith(
            Color color = null , 
            float? opacity = null, 
            float? size = null) 
        {
            return new CupertinoIconThemeData(
                color: color ?? this.color,
                opacity: opacity ?? this.opacity,
                size: size ?? this.size
            );
        }

   
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(CupertinoDynamicColor.createCupertinoColorProperty("color", color, defaultValue: null));
        }
    }

    
    
    
    
    
    
}