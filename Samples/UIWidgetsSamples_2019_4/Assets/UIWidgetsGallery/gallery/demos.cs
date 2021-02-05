using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.demo.material;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.gallery
{
    public class GalleryDemoCategory
    {
        public GalleryDemoCategory(
            string name, 
            IconData icon)
        {
            this.name = name;
            this.icon = icon;
        }
        
        public readonly string name;
        public readonly IconData icon;
        
        public bool Equals(GalleryDemoCategory other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return this.icon.Equals(other.icon) && this.name == other.name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            
            if (obj.GetType() != GetType())
            {
                return false;
            } 
            
            return Equals((GalleryDemoCategory)obj);
        }
        
        public static bool operator==(GalleryDemoCategory left, GalleryDemoCategory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GalleryDemoCategory left, GalleryDemoCategory right)
        {
            return !Equals(left, right);
        }
        
        public override int GetHashCode() {
            unchecked {
                return ((icon?.GetHashCode() ?? 0) * 397) ^ (name?.GetHashCode() ?? 0);
            }
        }

        public override string ToString()
        {
            return $"{GetType()}{name}";
        }
        
        public static readonly GalleryDemoCategory _kDemos = new GalleryDemoCategory(
            name: "Studies",
            icon: GalleryIcons.animation
        );

        public static readonly  GalleryDemoCategory _kStyle = new GalleryDemoCategory(
            name: "Style",
            icon: GalleryIcons.custom_typography
        );

        public static readonly  GalleryDemoCategory _kMaterialComponents = new GalleryDemoCategory(
            name: "Material",
            icon: GalleryIcons.category_mdc
        );

        public static readonly  GalleryDemoCategory _kCupertinoComponents = new GalleryDemoCategory(
            name: "Cupertino",
            icon: GalleryIcons.phone_iphone
        );

        public static readonly  GalleryDemoCategory _kMedia = new GalleryDemoCategory(
            name: "Media",
            icon: GalleryIcons.drive_video
        );
    }
    
    class GalleryDemo {
        public GalleryDemo(
            string title = null,
            IconData icon = null,
            string subtitle = null,
            GalleryDemoCategory category = null,
            string routeName = null,
            string documentationUrl = null,
            WidgetBuilder buildRoute = null
        )
        {
            D.assert(title != null);
            D.assert(category != null);
            D.assert(routeName != null);
            D.assert(buildRoute != null);

            this.title = title;
            this.icon = icon;
            this.subtitle = subtitle;
            this.category = category;
            this.routeName = routeName;
            this.documentationUrl = documentationUrl;
            this.buildRoute = buildRoute;
        }

        public readonly string title;
        public readonly  IconData icon;
        public readonly  string subtitle;
        public readonly  GalleryDemoCategory category;
        public readonly  string routeName;
        public readonly  WidgetBuilder buildRoute;
        public readonly  string documentationUrl;
        
        public override string ToString() {
            return $"{GetType()}({title} {routeName})";
        }

        public static List<GalleryDemo> _buildGalleryDemos()
        {
            List<GalleryDemo> galleryDemos = new List<GalleryDemo>
            {
                new GalleryDemo(
                    title: "Backdrop",
                    subtitle: $"Select a front layer from back layer",
                    icon: GalleryIcons.backdrop,
                    category: GalleryDemoCategory._kMaterialComponents,
                    routeName: BackdropDemo.routeName,
                    buildRoute: (BuildContext context) => new BackdropDemo()
                ),
                new GalleryDemo(
                    title: "Banner",
                    subtitle: "Displaying a banner within a list",
                    icon: GalleryIcons.lists_leave_behind,
                    category: GalleryDemoCategory._kMaterialComponents,
                    routeName: BannerDemo.routeName,
                    documentationUrl: "https://api.flutter.dev/flutter/material/MaterialBanner-class.html",
                    buildRoute: (BuildContext context) => new BannerDemo()
                ),
                new GalleryDemo(
                    title: "Bottom app bar",
                    subtitle: "Optional floating action button notch",
                    icon: GalleryIcons.bottom_app_bar,
                    category: GalleryDemoCategory._kMaterialComponents,
                    routeName: BottomAppBarDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/BottomAppBar-class.html",
                    buildRoute: (BuildContext context) => new BottomAppBarDemo()
                ),
                new GalleryDemo(
                    title: "Bottom navigation",
                    subtitle: "Bottom navigation with cross-fading views",
                    icon: GalleryIcons.bottom_navigation,
                    category: GalleryDemoCategory._kMaterialComponents,
                    routeName: BottomNavigationDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/BottomNavigationBar-class.html",
                    buildRoute: (BuildContext context) => new BottomNavigationDemo()
                ),
                new GalleryDemo(
                    title: "Buttons",
                    subtitle: "Flat, raised, dropdown, and more",
                    icon: GalleryIcons.generic_buttons,
                    category: GalleryDemoCategory._kMaterialComponents,
                    routeName: ButtonsDemo.routeName,
                    buildRoute: (BuildContext context) => new ButtonsDemo()
                ),
                new GalleryDemo(
                    title: "Cards",
                    subtitle: "Baseline cards with rounded corners",
                    icon: GalleryIcons.cards,
                    category: GalleryDemoCategory._kMaterialComponents,
                    routeName: CardsDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/Card-class.html",
                    buildRoute: (BuildContext context) => new CardsDemo()
                ),
                new GalleryDemo(
                    title: "Chips",
                    subtitle: "Labeled with delete buttons and avatars",
                    icon: GalleryIcons.chips,
                    category: GalleryDemoCategory._kMaterialComponents,
                    routeName: ChipDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/Chip-class.html",
                    buildRoute: (BuildContext context) => new ChipDemo()
                ),
                new GalleryDemo(
                    title: "Data tables",
                    subtitle: "Rows and columns",
                    icon: GalleryIcons.data_table,
                    category: GalleryDemoCategory._kMaterialComponents,
                    routeName: DataTableDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/PaginatedDataTable-class.html",
                    buildRoute: (BuildContext context) => new DataTableDemo()
                ),
            };

            return galleryDemos;
        }
        
        public static readonly List<GalleryDemo> kAllGalleryDemos = _buildGalleryDemos();

        public static readonly HashSet<GalleryDemoCategory> kAllGalleryDemoCategories =
        new HashSet<GalleryDemoCategory>(kAllGalleryDemos.Select<GalleryDemo, GalleryDemoCategory>((GalleryDemo demo) => demo.category).ToList());


        static Dictionary<GalleryDemoCategory, List<GalleryDemo>> _generateCategoryToDemos()
        {
            Dictionary<GalleryDemoCategory, List<GalleryDemo>> result =
                new Dictionary<GalleryDemoCategory, List<GalleryDemo>>();

            foreach (var category in kAllGalleryDemoCategories)
            {
                result.Add(category, kAllGalleryDemos.Where((GalleryDemo demo) =>
                {
                    return demo.category == category;
                }).ToList());
            }

            return result;
        }

        public static readonly Dictionary<GalleryDemoCategory, List<GalleryDemo>> kGalleryCategoryToDemos =
            _generateCategoryToDemos();

        static Dictionary<string, string> _generateDemoDocumentationUrls()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var demo in kAllGalleryDemos)
            {
                if (demo.documentationUrl != null)
                {
                    result.Add(demo.routeName, demo.documentationUrl);
                }
            }

            return result;
        }

        public static readonly Dictionary<string, string> kDemoDocumentationUrl = _generateDemoDocumentationUrls();
    }
    
    
}