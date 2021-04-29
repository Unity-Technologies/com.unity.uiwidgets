using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.demo.shrine.model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsGallery.demo.shrine
{

  public class shopping_cartUtils
  {
      public static readonly float _leftColumnWidth = 60.0f;
  }
    

public class ShoppingCartPage : StatefulWidget {
  
  public override State createState() => new _ShoppingCartPageState();
}

public class _ShoppingCartPageState : State<ShoppingCartPage> {
  List<Widget> _createShoppingCartRows(AppStateModel model)
  {
    List<Widget> widgets = new List<Widget>();
    foreach (var product in model.productsInCart.Keys)
    {
      int id = product;
        widgets.Add(new ShoppingCartRow(
          product: model.getProductById(id),
          quantity: model.productsInCart[id],
          onPressed: ()=> {
            model.removeItemFromCart(id);
          }
        ));
    }

    return widgets;
  }
  
  public override Widget build(BuildContext context) {
    ThemeData localTheme = Theme.of(context);

    return new Scaffold(
      backgroundColor: shrineColorsUtils.kShrinePink50,
      body: new SafeArea(
        child: new Container(
          child: new ScopedModelDescendant<AppStateModel>(
            builder: (BuildContext context2, Widget child, AppStateModel model) => {
              return new Stack(
                children: new List<Widget>{
                  new ListView(
                    children: new List<Widget>{
                      new Row(
                        children: new List<Widget>{
                          new SizedBox(
                            width: shopping_cartUtils._leftColumnWidth,
                            child: new IconButton(
                              icon: new Icon(Icons.keyboard_arrow_down),
                              onPressed: () => ExpandingBottomSheet.of(context).close()
                            )
                          ),
                          new Text(
                            "CART",
                            style: localTheme.textTheme.subtitle1.copyWith(fontWeight: FontWeight.w600)
                          ),
                          new SizedBox(width: 16.0f),
                          new Text($"{model.totalCartQuantity} ITEMS")
                        }
                      ),
                      new SizedBox(height: 16.0f),
                      new Column(
                        children: _createShoppingCartRows(model)
                      ),
                      new ShoppingCartSummary(model: model),
                      new SizedBox(height: 100.0f)
                    }
                  ),
                  new Positioned(
                    bottom: 16.0f,
                    left: 16.0f,
                    right: 16.0f,
                    child: new RaisedButton(
                      shape: new BeveledRectangleBorder(
                        borderRadius: BorderRadius.all(Radius.circular(7.0f))
                      ),
                      color: shrineColorsUtils.kShrinePink100,
                      splashColor: shrineColorsUtils.kShrineBrown600,
                      child: new Padding(
                        padding: EdgeInsets.symmetric(vertical: 12.0f),
                        child: new Text("CLEAR CART")
                      ),
                      onPressed: () => {
                        model.clearCart();
                        ExpandingBottomSheet.of(context).close();
                      }
                    )
                  )
                }
              );
            }
          )
        )
      )
    );
  }
}

public class ShoppingCartSummary : StatelessWidget {
  public ShoppingCartSummary(AppStateModel model)
  {
    this.model = model;
  }

  public readonly AppStateModel model;
  
  public override Widget build(BuildContext context) {
    TextStyle smallAmountStyle = Theme.of(context).textTheme.bodyText2.copyWith(color: shrineColorsUtils.kShrineBrown600);
    TextStyle largeAmountStyle = Theme.of(context).textTheme.headline4;

    return new Row(
      children: new List<Widget>
      {
        new SizedBox(width: shopping_cartUtils._leftColumnWidth),
        new Expanded(
          child: new Padding(
            padding: EdgeInsets.only(right: 16.0f),
            child: new Column(
              children: new List<Widget>
              {
                new Row(
                  crossAxisAlignment: CrossAxisAlignment.center,
                  children: new List<Widget>
                  {
                    new Expanded(
                      child: new Text("TOTAL")
                    ),
                    new Text(
                      $"${model.totalCost:F}",
                      style: largeAmountStyle
                    )
                  }
                ),
                new SizedBox(height: 16.0f),
                new Row(
                  children: new List<Widget>
                  {
                    new Expanded(
                      child: new Text("Subtotal:")
                    ),
                    new Text(
                      $" $ {model.subtotalCost:F}",
                      style: smallAmountStyle
                    )
                  }
                ),
                new SizedBox(height: 4.0f),
                new Row(
                  children: new List<Widget>
                  {
                    new Expanded(
                      child: new Text("Shipping:")
                    ),
                    new Text(
                      $" $ {model.shippingCost:F}",
                      style: smallAmountStyle
                    )
                  }
                ),
                new SizedBox(height: 4.0f),
                new Row(
                  children: new List<Widget>
                  {

                    new Expanded(
                    child: new Text("Tax:")
                    ),
                    new Text(
                      $"${model.tax:F}",
                      style: smallAmountStyle
                    )
                  }
                )
              }
            )
          )
        )
      }
    );
  }
}

public class ShoppingCartRow : StatelessWidget {
  public ShoppingCartRow(
    Product product = null,
    int quantity = 1,
    VoidCallback onPressed = null
  )
  {
    this.product = product;
    this.quantity = quantity;
    this.onPressed = onPressed;
  }

  public readonly Product product;
  public readonly int quantity;
  public readonly VoidCallback onPressed;

  
  public override Widget build(BuildContext context) {
    
    ThemeData localTheme = Theme.of(context);

    return new Padding(
      padding: EdgeInsets.only(bottom: 16.0f),
      child: new Row(
        key: new ValueKey<int>(product.id),
        crossAxisAlignment: CrossAxisAlignment.start,
        children: new List<Widget>{
          new SizedBox(
            width: shopping_cartUtils._leftColumnWidth,
            child: new IconButton(
              icon: new Icon(Icons.remove_circle_outline),
              onPressed: onPressed
            )
          ),
          new Expanded(
            child: new Padding(
              padding: EdgeInsets.only(right: 16.0f),
              child: new Column(
                children: new List<Widget>{
                  new Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget>{
                      Image.file(
                        product.assetName,
                        //package: product.assetPackage,
                        fit: BoxFit.cover,
                        width: 75.0f,
                        height: 75.0f
                      ),
                      new SizedBox(width: 16.0f),
                      new Expanded(
                        child: new Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: new List<Widget>{
                            new Row(
                              children: new List<Widget>{
                                new Expanded(
                                  child: new Text($"Quantity: {quantity}")
                                ),
                                new Text($"x  $ {product.price :F} ")
                              }
                            ),
                            new Text(
                              product.name,
                              style: localTheme.textTheme.subtitle1.copyWith(fontWeight: FontWeight.w600)
                            )
                          }
                        )
                      )
                    }
                  ),
                  new SizedBox(height: 16.0f),
                  new Divider(
                    color: shrineColorsUtils.kShrineBrown900,
                    height: 10.0f
                  )
                }
              )
            )
          )
        }
      )
    );
  }
}

}