using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    static class TabsDemoUtils
    {
        public static readonly string _kGalleryAssetsPackage = "gallery/";

        public static readonly Dictionary<_TabPage, List<_CardData>> _allPages =
            new Dictionary<_TabPage, List<_CardData>>
            {
                {
                    new _TabPage(label: "HOME"), new List<_CardData>
                    {
                        new _CardData(
                            title: "Flatwear",
                            imageAsset: "products/flatwear.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                        new _CardData(
                            title: "Pine Table",
                            imageAsset: "products/table.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                        new _CardData(
                            title: "Blue Cup",
                            imageAsset: "products/cup.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                        new _CardData(
                            title: "Tea Set",
                            imageAsset: "products/teaset.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                        new _CardData(
                            title: "Desk Set",
                            imageAsset: "products/deskset.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                        new _CardData(
                            title: "Blue Linen Napkins",
                            imageAsset: "products/napkins.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                        new _CardData(
                            title: "Planters",
                            imageAsset: "products/planters.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                        new _CardData(
                            title: "Kitchen Quattro",
                            imageAsset: "products/kitchen_quattro.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                        new _CardData(
                            title: "Platter",
                            imageAsset: "products/platter.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                    }
                },
                {
                    new _TabPage(label: "APPAREL"), new List<_CardData>
                    {
                        new _CardData(
                            title: "Cloud-White Dress",
                            imageAsset: "products/dress.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                        new _CardData(
                            title: "Ginger Scarf",
                            imageAsset: "products/scarf.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                        new _CardData(
                            title: "Blush Sweats",
                            imageAsset: "products/sweats.png",
                            imageAssetPackage: _kGalleryAssetsPackage
                        ),
                    }
                }
            };
    }

    internal class _TabPage {
        public _TabPage(string label)
        {
            this.label = label;
        }
       
    public readonly string label;
        public string id => label.Substring(0, 1);

    public override string ToString() => $"{GetType()}(\"{label}\")";
}

        internal class _CardData {
    public _CardData(string title, string imageAsset, string imageAssetPackage)
    {
        this.title = title;
        this.imageAsset = imageAsset;
        this.imageAssetPackage = imageAssetPackage;
    }
    
    public readonly string title;
    public readonly string imageAsset;
    public readonly string imageAssetPackage;
    
    
}
    
    class _CardDataItem : StatelessWidget {
        public _CardDataItem(_TabPage page, _CardData data)
        {
            this.page = page;
            this.data = data;
        }

        internal const float height = 272.0f;
    public readonly _TabPage page;
    public readonly _CardData data;

    public override Widget build(BuildContext context) {
    return new Card(
    child: new Padding(
    padding: EdgeInsets.all(16.0f),
    child: new Column(
    crossAxisAlignment: CrossAxisAlignment.stretch,
    mainAxisAlignment: MainAxisAlignment.start,
    children: new List<Widget>{
        new Align(
            alignment: page.id == "H"
                ? Alignment.centerLeft
                : Alignment.centerRight,
            child: new CircleAvatar(child: new Text(page.id))
        ),
        new SizedBox(
            width: 144.0f,
            height: 144.0f,
            child: Image.file(
                data.imageAssetPackage + data.imageAsset,
                fit: BoxFit.contain
            )
        ),
        new Center(
            child: new Text(
                data.title,
                style: Theme.of(context).textTheme.headline6
            )
        )
    }
    )
    )
    );
}
}
    
    class TabsDemo : StatelessWidget {
  public static readonly string routeName = "/material/tabs";

  public override Widget build(BuildContext context) {
    return new DefaultTabController(
      length: TabsDemoUtils._allPages.Count,
      child: new Scaffold(
        body: new NestedScrollView(
          headerSliverBuilder: (BuildContext subContext, bool innerBoxIsScrolled) => {
            return new List<Widget>{
              new SliverOverlapAbsorber(
                handle: NestedScrollView.sliverOverlapAbsorberHandleFor(subContext),
                sliver: new SliverAppBar(
                  title: new Text("Tabs and scrolling"),
                  actions: new List<Widget>{new MaterialDemoDocumentationButton(routeName)},
                  pinned: true,
                  expandedHeight: 150.0f,
                  forceElevated: innerBoxIsScrolled,
                  bottom: new TabBar(
                    tabs: TabsDemoUtils._allPages.Keys.Select<_TabPage, Widget>(
                      (_TabPage page) => new Tab(text: page.label)
                    ).ToList()
                  )
                )
              )
            };
          },
          body: new TabBarView(
            children: TabsDemoUtils._allPages.Keys.Select<_TabPage, Widget>((_TabPage page) => {
              return new SafeArea(
                top: false,
                bottom: false,
                child: new Builder(
                  builder: (BuildContext subContext) => {
                    return new CustomScrollView(
                      key: new PageStorageKey<_TabPage>(page),
                      slivers: new List<Widget>{
                        new SliverOverlapInjector(
                          handle: NestedScrollView.sliverOverlapAbsorberHandleFor(subContext)
                        ),
                        new SliverPadding(
                          padding: EdgeInsets.symmetric(
                            vertical: 8.0f,
                            horizontal: 16.0f
                          ),
                          sliver: new SliverFixedExtentList(
                            itemExtent: _CardDataItem.height,
                            del: new SliverChildBuilderDelegate(
                              (BuildContext subsubContext, int index) => {
                                _CardData data = TabsDemoUtils._allPages[page][index];
                                return new Padding(
                                  padding: EdgeInsets.symmetric(
                                    vertical: 8.0f
                                  ),
                                  child: new _CardDataItem(
                                    page: page,
                                    data: data
                                  )
                                );
                              },
                              childCount: TabsDemoUtils._allPages[page].Count
                            )
                          )
                        )
                      }
                    );
                  }
                )
              );
            }).ToList()
          )
        )
      )
    );
  }
}
}