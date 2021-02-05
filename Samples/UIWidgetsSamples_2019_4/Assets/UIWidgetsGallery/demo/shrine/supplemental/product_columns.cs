using System.Collections.Generic;
using UIWidgetsGallery.demo.shrine.model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.shrine.supplemental
{
    public class TwoProductCardColumn : StatelessWidget {
      
        public TwoProductCardColumn(Product bottom = null, Product top = null)
        {
            D.assert(bottom != null);
            this.bottom = bottom;
            this.top = top;
        }

        public readonly Product bottom, top;

 
        public override Widget build(BuildContext context) {
            return new LayoutBuilder(builder: (BuildContext context1, BoxConstraints constraints) => {
                float spacerHeight = 44.0f;
                float heightOfCards = (constraints.biggest.height - spacerHeight) / 2.0f;
                float availableHeightForImages = heightOfCards - product_cardutils.kTextBoxHeight;
                float imageAspectRatio = availableHeightForImages >= 0.0
                ? constraints.biggest.width / availableHeightForImages
                : 49.0f / 33.0f;
                
                return new ListView(
                    physics:new ClampingScrollPhysics(),
                    children: new List<Widget>
                    {
                        new Padding(
                            padding: EdgeInsets.only(left: 28.0f),
                            child: top != null
                            ?new SizedBox(
                                child: new ProductCard(
                                    imageAspectRatio: imageAspectRatio,
                                    product: top
                                )
                            )
                            : new SizedBox(
                                height: heightOfCards > 0 ? heightOfCards : spacerHeight
                            )
                        ),
                        new SizedBox(height: spacerHeight),
                        new Padding(
                            padding: EdgeInsets.only(right: 28.0f),
                            child: new ProductCard(
                            imageAspectRatio: imageAspectRatio,
                            product: bottom
                            )
                        ),
                    }
                );
            });
        }
    }

    public class OneProductCardColumn : StatelessWidget {
        public OneProductCardColumn(Product product = null)
        {
            this.product = product;
        }

        public readonly Product product;

  
        public override Widget build(BuildContext context) {
            return new ListView(
                physics: new ClampingScrollPhysics(),
                reverse: true,
                children: new List<Widget>
                {
                    new SizedBox(
                        height: 40.0f
                    ),
                    new ProductCard(
                        product: product
                    ),
                }
            );
        }
    }
}