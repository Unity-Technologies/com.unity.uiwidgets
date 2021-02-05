using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.demo.shrine.model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.shrine.supplemental
{ 
    public class AsymmetricView : StatelessWidget {
        public AsymmetricView(Key key = null, List<Product> products = null) : base(key: key)
        {
          this.products = products;
        }

        public readonly List<Product> products;

        List<Widget> _buildColumns(BuildContext context) {
            if (products == null || products.isEmpty()) {
                return new List<Widget>();
            }

            List<Widget> list = new List<Widget>();
            var len = _listItemCount(products.Count());
            for (int index = 0; index < len; index++)
            {
                float width = 0.59f * MediaQuery.of(context).size.width;
                Widget column;
                if (index % 2 == 0) {
                    int bottom = _evenCasesIndex(index);
                    column = new TwoProductCardColumn(
                        bottom: products[bottom],
                        top: products.Count - 1 >= bottom + 1
                            ? products[bottom + 1]
                            : null
                    );
                    width += 32.0f;
                } else {
                    column = new OneProductCardColumn(
                        product: products[_oddCasesIndex(index)]
                    );
                }
                list.Add(new Container(
                    width: width,
                    child: new Padding(
                        padding: EdgeInsets.symmetric(horizontal: 16.0f),
                        child: column
                    )
                ));
            }

            return list;
        }

        int _evenCasesIndex(int input) {
            return input / 2 * 3;
        }

        int _oddCasesIndex(int input) {
            D.assert(input > 0);
            
            return ((input + 1) / 2) * 3 - 1;
        }

        int _listItemCount(int totalItems) {
            return (totalItems % 3 == 0)
            ? totalItems / 3 * 2
            : ((totalItems + 2) / 3) * 2 - 1;
        }

  
        public override Widget build(BuildContext context)
        {
            List<Widget> widgets = _buildColumns(context);
            return new ListView(
                scrollDirection: Axis.horizontal,
                padding: EdgeInsets.fromLTRB(0.0f, 34.0f, 16.0f, 44.0f),
                children: widgets,
                physics: new AlwaysScrollableScrollPhysics()
            );
        }
    }
    
}