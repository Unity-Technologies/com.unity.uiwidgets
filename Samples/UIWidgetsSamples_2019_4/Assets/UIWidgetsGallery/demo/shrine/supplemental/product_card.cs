using System.Collections.Generic;
using UIWidgetsGallery.demo.shrine.model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.demo.shrine.supplemental
{

    public static class product_cardutils
    {
        public static readonly float kTextBoxHeight = 65.0f;
    }
    public class ProductCard : StatelessWidget {
        public ProductCard(float imageAspectRatio = 33f / 49f, Product product = null)
        {
            D.assert(imageAspectRatio > 0);
            this.imageAspectRatio = imageAspectRatio;
            this.product = product;
        }

        public readonly float imageAspectRatio;
        public readonly Product product;

        

  
        public override Widget build(BuildContext context) {
            
            ThemeData theme = Theme.of(context);

            Image imageWidget = Image.file(
                product.assetName,
                fit: BoxFit.cover
            );

            return new ScopedModelDescendant<AppStateModel>(
                builder: (BuildContext context2, Widget child, AppStateModel model) =>
                {
                    return new GestureDetector(
                        onTap: () =>
                        {
                            model.addProductToCart(product.id);
                        },
                        child: child
                    );
                },
                child: new Stack(
                    children: new List<Widget>
                    {
                        new Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            crossAxisAlignment: CrossAxisAlignment.center,
                            children: new List<Widget>
                            {
                                new AspectRatio(
                                    aspectRatio: imageAspectRatio,
                                    child: imageWidget
                                ),
                                new SizedBox(
                                    height: product_cardutils.kTextBoxHeight * MediaQuery.of(context).textScaleFactor,
                                    width: 121.0f,
                                    child: new Column(
                                        mainAxisAlignment: MainAxisAlignment.end,
                                        crossAxisAlignment: CrossAxisAlignment.center,
                                        children: new List<Widget>
                                        {
                                            new Text(
                                                product == null ? "" : product.name,
                                                style: theme.textTheme.button,
                                                softWrap: false,
                                                overflow: TextOverflow.ellipsis,
                                                maxLines: 1
                                            ),
                                            new SizedBox(height: 4.0f),
                                            new Text(
                                                $" $ { product.price:F}",
                                                style: theme.textTheme.caption
                                            ),
                                        }
                                    )
                                )
                            }
                        ),
                        new Padding(
                            padding: EdgeInsets.all(16.0f),
                            child: new Icon(Icons.add_shopping_cart)
                        ),
                    }
                )
            );
        }
    }

}