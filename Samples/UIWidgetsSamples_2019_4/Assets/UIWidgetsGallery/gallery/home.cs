using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgetsGallery.gallery
{
    public static class GalleryHomeUtils
    {
        public static readonly string _kGalleryAssetsPackage = "uiwdigets_gallery_assets";
        public static readonly Color _kFlutterBlue = new Color(0xFF003D75);
        public const float _kDemoItemHeight = 64.0f;
        public static readonly TimeSpan _kFrontLayerSwitchDuration = new TimeSpan(0, 0, 0, 0, 300);
    }
    
    class _CategoryItem : StatelessWidget {
    public _CategoryItem(
        Key key = null,
        GalleryDemoCategory category = null,
        VoidCallback onTap = null
    ) : base (key: key)
    {
        this.category = category;
        this.onTap = onTap;
    }

    public readonly GalleryDemoCategory category;
    public readonly VoidCallback onTap;

    public override Widget build(BuildContext context) {
         ThemeData theme = Theme.of(context);
         bool isDark = theme.brightness == Brightness.dark;

        // This repaint boundary prevents the entire _CategoriesPage from being
        // repainted when the button's ink splash animates.
        return new RepaintBoundary(
            child: new RawMaterialButton(
                padding: EdgeInsets.zero,
                hoverColor: theme.primaryColor.withOpacity(0.05f),
                splashColor: theme.primaryColor.withOpacity(0.12f),
                highlightColor: Colors.transparent,
                onPressed: onTap,
                child: new Column(
                           mainAxisAlignment: MainAxisAlignment.end,
                           crossAxisAlignment: CrossAxisAlignment.center,
                           children: new List<Widget>{
                               new Padding(
                    padding: EdgeInsets.all(6.0f),
        child: new Icon(
                category.icon,
                size: 60.0f,
                color: isDark ? Colors.white : GalleryHomeUtils._kFlutterBlue
            )
            ),
                               new  SizedBox(height: 10.0f),
                               new Container(
                height: 48.0f,
                alignment: Alignment.center,
                child: new Text(
                    category.name,
                    textAlign: TextAlign.center,
                    style: theme.textTheme.subtitle1.copyWith(
                        color: isDark ? Colors.white : GalleryHomeUtils._kFlutterBlue
                    )
                )
            )
            }
            )
            )
            );
    }
    }

    class _CategoriesPage : StatelessWidget
    {
        public _CategoriesPage(
            Key key = null,
            IEnumerable<GalleryDemoCategory> categories = null,
            ValueChanged<GalleryDemoCategory> onCategoryTap = null
        ) : base(key: key)
        {
            this.categories = categories;
            this.onCategoryTap = onCategoryTap;
        }

        public readonly IEnumerable<GalleryDemoCategory> categories;
        public readonly ValueChanged<GalleryDemoCategory> onCategoryTap;

        public override Widget build(BuildContext context)
        {
            float aspectRatio = 160.0f / 180.0f;
            List<GalleryDemoCategory> categoriesList = categories.ToList();
            int columnCount = (MediaQuery.of(context).orientation == Orientation.portrait) ? 2 : 3;

            return new SingleChildScrollView(
                key: PageStorageKey<String>.key("categories"),
                child: new LayoutBuilder(
                    builder: (BuildContext subContext, BoxConstraints constraints) =>
                    {
                        float columnWidth = constraints.biggest.width / columnCount;
                        float rowHeight = Mathf.Min(225.0f, columnWidth * aspectRatio);
                        int rowCount = (categories.Count() + columnCount - 1) / columnCount;

                        var children = new List<Widget>();

                        for (var i = 0; i < rowCount; i++)
                        {
                            var rowIndex = i;
                            int columnCountForRow = rowIndex == rowCount - 1
                                ? categories.Count() - columnCount * Mathf.Max(0, rowCount - 1)
                                : columnCount;

                            var subChildren = new List<Widget>();

                            for (var j = 0; j < columnCountForRow; j++)
                            {
                                var columnIndex = j;
                                int index = rowIndex * columnCount + columnIndex;
                                GalleryDemoCategory category = categoriesList[index];

                                subChildren.Add(new SizedBox(width: columnWidth,
                                    height: rowHeight,
                                    child: new _CategoryItem(
                                        category: category,
                                        onTap: () => { onCategoryTap(category); }
                                    )
                                ));
                            }

                            children.Add(new Row(
                                children: subChildren
                            ));
                        }

                        return new RepaintBoundary(
                            child: new Column(
                                mainAxisSize: MainAxisSize.min,
                                crossAxisAlignment: CrossAxisAlignment.stretch,
                                children: children
                            )
                        );
                    }));
        }
    }
    
    class _DemoItem : StatelessWidget {
        public _DemoItem(Key key = null, GalleryDemo demo = null) : base(key: key)
        {
            this.demo = demo;
        }

  public readonly GalleryDemo demo;

  void _launchDemo(BuildContext context) {
    if (demo.routeName != null) {
        Navigator.pushNamed(context, demo.routeName);
    }
  }

  @override
  Widget build(BuildContext context) {
    final ThemeData theme = Theme.of(context);
    final bool isDark = theme.brightness == Brightness.dark;
    final double textScaleFactor = MediaQuery.textScaleFactorOf(context);
    return RawMaterialButton(
      padding: EdgeInsets.zero,
      splashColor: theme.primaryColor.withOpacity(0.12),
      highlightColor: Colors.transparent,
      onPressed: () {
        _launchDemo(context);
      },
      child: Container(
        constraints: BoxConstraints(minHeight: _kDemoItemHeight * textScaleFactor),
        child: Row(
          children: <Widget>[
            Container(
              width: 56.0,
              height: 56.0,
              alignment: Alignment.center,
              child: Icon(
                demo.icon,
                size: 24.0,
                color: isDark ? Colors.white : _kFlutterBlue,
              ),
            ),
            Expanded(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: <Widget>[
                  Text(
                    demo.title,
                    style: theme.textTheme.subtitle1.copyWith(
                      color: isDark ? Colors.white : const Color(0xFF202124),
                    ),
                  ),
                  if (demo.subtitle != null)
                    Text(
                      demo.subtitle,
                      style: theme.textTheme.bodyText2.copyWith(
                        color: isDark ? Colors.white : const Color(0xFF60646B)
                      ),
                    ),
                ],
              ),
            ),
            const SizedBox(width: 44.0),
          ],
        ),
      ),
    );
  }
}
}