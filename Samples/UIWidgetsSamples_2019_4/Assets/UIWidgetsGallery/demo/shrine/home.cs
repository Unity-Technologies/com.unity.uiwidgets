using System.Collections.Generic;
using UIWidgetsGallery.demo.shrine.model;
using UIWidgetsGallery.demo.shrine.supplemental;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.shrine
{
    public class ProductPage : StatelessWidget {
        public ProductPage(Category category = Category.all)
        {
            this.category = category;
        }

        public readonly Category category;

    
        public override Widget build(BuildContext context) {
            return new ScopedModelDescendant<AppStateModel>(
                builder: (BuildContext context1, Widget child, AppStateModel model) => {
                    return new AsymmetricView(products: model.getProducts());
            });
        }
    }

    public class HomePage : StatelessWidget {
        public HomePage(
            ExpandingBottomSheet expandingBottomSheet = null,
            Backdrop backdrop = null,
            Key key = null
        ) : base(key: key)
        {
            this.expandingBottomSheet = expandingBottomSheet;
            this.backdrop = backdrop;
        }

        public readonly ExpandingBottomSheet expandingBottomSheet;
        public readonly Backdrop backdrop;

    
        public override Widget build(BuildContext context) {
            return new Stack(
            children: new List<Widget>
            {
                backdrop,
                new Align(child: expandingBottomSheet, alignment: Alignment.bottomRight),
            }
            );
    }
}

}