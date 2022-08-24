using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace UIWidgetsGallery.demo.shrine.supplemental
{
    public class CutCornersBorder : OutlineInputBorder {
        public CutCornersBorder(
        BorderSide borderSide = null,
        BorderRadius borderRadius = null,
        float cut = 7.0f,
        float gapPadding = 2.0f
        ) : base(
            borderSide: borderSide?? BorderSide.none,
            borderRadius: borderRadius?? BorderRadius.all(Radius.circular(2.0f)),
            gapPadding: gapPadding
        )
        {
            this.cut = cut;
        }

  
  public CutCornersBorder copyWith(
    BorderSide borderSide = null,
    BorderRadius borderRadius = null,
    float? gapPadding = null,
    float? cut = null
  ) {
    return new CutCornersBorder(
        borderSide: borderSide ?? this.borderSide,
        borderRadius: borderRadius ?? this.borderRadius,
        gapPadding: gapPadding ?? this.gapPadding,
        cut: cut ?? this.cut
    );
  }

  public readonly float cut;

  
  public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
    if (a is CutCornersBorder) {
      CutCornersBorder outline = (CutCornersBorder)a;
      return new CutCornersBorder(
        borderRadius: BorderRadius.lerp(outline.borderRadius, borderRadius, t),
        borderSide: BorderSide.lerp(outline.borderSide, borderSide, t),
        cut: cut,
        gapPadding: outline.gapPadding
      );
    }
    return base.lerpFrom(a, t);
  }

  
  public override ShapeBorder lerpTo(ShapeBorder b, float t) {
    if (b is CutCornersBorder) {
      CutCornersBorder outline = (CutCornersBorder)b;
      return new CutCornersBorder(
        borderRadius: BorderRadius.lerp(borderRadius, outline.borderRadius, t),
        borderSide: BorderSide.lerp(borderSide, outline.borderSide, t),
        cut: cut,
        gapPadding: outline.gapPadding
      );
    }
    return base.lerpTo(b, t);
  }

  Path _notchedCornerPath(Rect center, float start = 0.0f, float extent = 0.0f) {
    Path path = new Path();
    if (start > 0.0 || extent > 0.0) {
      path.relativeMoveTo(extent + start, center.top);
      _notchedSidesAndBottom(center, path);
      path.lineTo(center.left + cut, center.top);
      path.lineTo(start, center.top);
    } else {
      path.moveTo(center.left + cut, center.top);
      _notchedSidesAndBottom(center, path);
      path.lineTo(center.left + cut, center.top);
    }
    return path;
  }

  Path _notchedSidesAndBottom(Rect center, Path path)
  {
    path.lineTo(center.right - cut, center.top);
    path.lineTo(center.right, center.top + cut);
    path.lineTo(center.right, center.top + center.height - cut);
    path.lineTo(center.right - cut, center.top + center.height);
    path.lineTo(center.left + cut, center.top + center.height);
    path.lineTo(center.left, center.top + center.height - cut);
      path.lineTo(center.left, center.top + cut);
      return path;
  }

  
  public void paint(
    Canvas canvas,
    Rect rect, 
    float? gapStart = null,
    float gapExtent = 0.0f,
    float gapPercentage = 0.0f,
    TextDirection? textDirection = null
  ) {
    D.assert(gapPercentage >= 0.0 && gapPercentage <= 1.0);

    Paint paint = borderSide.toPaint();
    RRect outer = borderRadius.toRRect(rect);
    if (gapStart == null || gapExtent <= 0.0 || gapPercentage == 0.0) {
      canvas.drawPath(_notchedCornerPath(outer.middleRect), paint);
    } else {
      float extent = lerpFloat(0.0f, gapExtent + gapPadding * 2.0f, gapPercentage)?? 0.0f;
      switch (textDirection) {
        case TextDirection.rtl: {
          Path path = _notchedCornerPath(outer.middleRect, gapStart.Value + gapPadding - extent, extent);
          canvas.drawPath(path, paint);
          break;
        }
        case TextDirection.ltr: {
          Path path = _notchedCornerPath(outer.middleRect, gapStart.Value - gapPadding, extent);
          canvas.drawPath(path, paint);
          break;
        }
      }
    }
  }
  
  float? lerpFloat(float? a, float? b, float t) {
    if (a == null && b == null)
      return null;
    a = a?? 0.0f;
    b = b?? 0.0f;
    return a + (b - a) * t;
  }
  
}

}