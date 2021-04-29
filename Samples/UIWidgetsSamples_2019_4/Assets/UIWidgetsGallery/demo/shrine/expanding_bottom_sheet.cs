using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UIWidgetsGallery.demo.shrine.model;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.shrine
{
    public class expanding_buttom_sheetUtils
    {
        public static readonly Cubic _kAccelerateCurve = new Cubic(0.548f, 0.0f, 0.757f, 0.464f);
        public static readonly Cubic _kDecelerateCurve = new Cubic(0.23f, 0.94f, 0.41f, 1.0f);
        public static readonly float _kPeakVelocityTime = 0.248210f;
        public static readonly float _kPeakVelocityProgress = 0.379146f;
        public static readonly float _kCartHeight = 56.0f;
        public static readonly float _kCornerRadius = 24.0f;
        public static readonly float _kWidthForCartIcon = 64.0f;
        
        public static Animation<float> _getFloatEmphasizedEasingAnimation(
            float begin,
            float peak,
            float end,
            bool isForward,
            Animation<float> parent
        ) {
            Curve firstCurve;
            Curve secondCurve;
            float firstWeight;
            float secondWeight;

            if (isForward) {
                firstCurve = _kAccelerateCurve;
                secondCurve = _kDecelerateCurve;
                firstWeight = _kPeakVelocityTime;
                secondWeight = 1.0f - _kPeakVelocityTime;
            } else {
                firstCurve = _kDecelerateCurve.flipped;
                secondCurve = _kAccelerateCurve.flipped;
                firstWeight = 1.0f - _kPeakVelocityTime;
                secondWeight = _kPeakVelocityTime;
            }

            return new TweenSequence<float>(
                new List<TweenSequenceItem<float>>{
                    new TweenSequenceItem<float>(
                        weight: firstWeight,
                        tween: new FloatTween(
                          begin: begin,
                          end: peak
                        ).chain(new CurveTween(curve: firstCurve))
                    ),
                    new TweenSequenceItem<float>(
                        weight: secondWeight,
                        tween: new FloatTween(
                          begin: peak,
                          end: end
                        ).chain(new CurveTween(curve: secondCurve))
                    ),
                }
            ).animate(parent);
        }
        
        public static Animation<Offset> _getOffsetEmphasizedEasingAnimation(
          Offset begin,
          Offset peak,
          Offset end,
          bool isForward,
          Animation<float> parent
        ) {
          Curve firstCurve;
          Curve secondCurve;
          float firstWeight;
          float secondWeight;

          if (isForward) {
            firstCurve = _kAccelerateCurve;
            secondCurve = _kDecelerateCurve;
            firstWeight = _kPeakVelocityTime;
            secondWeight = 1.0f - _kPeakVelocityTime;
          } else {
            firstCurve = _kDecelerateCurve.flipped;
            secondCurve = _kAccelerateCurve.flipped;
            firstWeight = 1.0f - _kPeakVelocityTime;
            secondWeight = _kPeakVelocityTime;
          }

          return new TweenSequence<Offset>(
            new List<TweenSequenceItem<Offset>>{
              new TweenSequenceItem<Offset>(
                weight: firstWeight,
                tween: new OffsetTween(
                  begin: begin,
                  end: peak
                ).chain(new CurveTween(curve: firstCurve))
              ),
              new TweenSequenceItem<Offset>(
                weight: secondWeight,
                tween: new OffsetTween(
                  begin: peak,
                  end: end
                ).chain(new CurveTween(curve: secondCurve))
              ),
            }
          ).animate(parent);
        }
        
        
        public static float _getPeakPoint(float begin, float end) {
          return begin + (end - begin) * _kPeakVelocityProgress;
        }
    }
    

    public class ExpandingBottomSheet : StatefulWidget {
        public ExpandingBottomSheet(
          Key key = null, 
          AnimationController hideController = null)
        : base(key: key)
        {
            D.assert(hideController != null);
            this.hideController = hideController;
        }

        public readonly AnimationController hideController;
        
        public override State createState() => new _ExpandingBottomSheetState();

        public static _ExpandingBottomSheetState of(BuildContext context, bool isNullOk = false) {
          D.assert(context != null);
          _ExpandingBottomSheetState result = context.findAncestorStateOfType<_ExpandingBottomSheetState>();
          if (isNullOk || result != null) {
            return result;
          }
          throw new UIWidgetsError(
            "ExpandingBottomSheet.of() called with a context that does not contain a ExpandingBottomSheet.\n");
        }
    }



public class _ExpandingBottomSheetState : TickerProviderStateMixin<ExpandingBottomSheet> { 
  public readonly GlobalKey _expandingBottomSheetKey = GlobalKey.key(debugLabel: "Expanding bottom sheet");
  
  float _width = expanding_buttom_sheetUtils._kWidthForCartIcon;
  AnimationController _controller;
  
  Animation<float> _widthAnimation;
  Animation<float> _heightAnimation;
  Animation<float> _thumbnailOpacityAnimation;
  Animation<float> _cartOpacityAnimation;
  Animation<float> _shapeAnimation;
  Animation<Offset> _slideAnimation;

  
  public override void initState() {
    base.initState();
    _controller = new AnimationController(
      duration: TimeSpan.FromMilliseconds(500),
      vsync: this
    );
  }
  
  public override void dispose() {
    _controller.dispose();
    base.dispose();
  }

  Animation<float> _getWidthAnimation(float screenWidth) {
    if (_controller.status == AnimationStatus.forward) {
      return new FloatTween(begin: _width, end: screenWidth).animate(
        new CurvedAnimation(
          parent: _controller.view,
          curve: new Interval(0.0f, 0.3f, curve: Curves.fastOutSlowIn)
        )
      );
    } else {
      return expanding_buttom_sheetUtils._getFloatEmphasizedEasingAnimation(
        begin: _width,
        peak: expanding_buttom_sheetUtils._getPeakPoint(begin: _width, end: screenWidth),
        end: screenWidth,
        isForward: false,
        parent: new CurvedAnimation(parent: _controller.view, curve: new Interval(0.0f, 0.87f))
      );
    }
  }

  Animation<float> _getHeightAnimation(float screenHeight) {
    if (_controller.status == AnimationStatus.forward) {
      return expanding_buttom_sheetUtils._getFloatEmphasizedEasingAnimation(
        begin: expanding_buttom_sheetUtils._kCartHeight,
        peak: expanding_buttom_sheetUtils._kCartHeight + (screenHeight - expanding_buttom_sheetUtils._kCartHeight) * expanding_buttom_sheetUtils._kPeakVelocityProgress,
        end: screenHeight,
        isForward: true,
        parent: _controller.view
      );
    } else {
      return new FloatTween(
        begin: expanding_buttom_sheetUtils._kCartHeight,
        end: screenHeight
      ).animate(
        new CurvedAnimation(
          parent: _controller.view,
          curve: new Interval(0.434f, 1.0f, curve: Curves.linear),
          reverseCurve: new Interval(0.434f, 1.0f, curve: Curves.fastOutSlowIn.flipped)
        )
      );
    }
  }

  // Animation of the cut corner. It's cut when closed and not cut when open.
  Animation<float> _getShapeAnimation() {
    if (_controller.status == AnimationStatus.forward) {
      return new FloatTween(begin: expanding_buttom_sheetUtils._kCornerRadius, end: 0.0f).animate(
        new CurvedAnimation(
          parent: _controller.view,
          curve: new Interval(0.0f, 0.3f, curve: Curves.fastOutSlowIn)
        )
      );
    } else {
      return expanding_buttom_sheetUtils._getFloatEmphasizedEasingAnimation(
        begin: expanding_buttom_sheetUtils._kCornerRadius,
        peak: expanding_buttom_sheetUtils._getPeakPoint(begin: expanding_buttom_sheetUtils._kCornerRadius, end: 0.0f),
        end: 0.0f,
        isForward: false,
        parent: _controller.view
      );
    }
  }

  Animation<float> _getThumbnailOpacityAnimation() {
    return new FloatTween(begin: 1.0f, end: 0.0f).animate(
      new CurvedAnimation(
        parent: _controller.view,
        curve: _controller.status == AnimationStatus.forward
          ? new Interval(0.0f, 0.3f)
          : new Interval(0.532f, 0.766f)
      )
    );
  }

  Animation<float> _getCartOpacityAnimation() {
    return new CurvedAnimation(
      parent: _controller.view,
      curve: _controller.status == AnimationStatus.forward
        ? new Interval(0.3f, 0.6f)
        : new Interval(0.766f, 1.0f)
    );
  }

  // Returns the correct width of the ExpandingBottomSheet based on the number of
  // products in the cart.
  float _widthFor(int numProducts) {
    switch (numProducts) {
      case 0:
        return expanding_buttom_sheetUtils._kWidthForCartIcon;
      case 1:
        return 136.0f;
      case 2:
        return 192.0f;
      case 3:
        return 248.0f;
      default:
        return 278.0f;
    }
  }

  // Returns true if the cart is open or opening and false otherwise.
  bool _isOpen {
    get
    {
      AnimationStatus status = _controller.status;
      return status == AnimationStatus.completed || status == AnimationStatus.forward;
    }
  }

  // Opens the ExpandingBottomSheet if it's closed, otherwise does nothing.
  void open() {
    if (!_isOpen) {
      _controller.forward();
    }
  }

  // Closes the ExpandingBottomSheet if it's open or opening, otherwise does nothing.
  public void close() {
    if (_isOpen) {
      _controller.reverse();
    }
  }

  // Changes the padding between the start edge of the Material and the cart icon
  // based on the number of products in the cart (padding increases when > 0
  // products.)
  EdgeInsetsDirectional _cartPaddingFor(int numProducts) {
    return (numProducts == 0)
      ? EdgeInsetsDirectional.only(start: 20.0f, end: 8.0f)
      : EdgeInsetsDirectional.only(start: 32.0f, end: 8.0f);
  }

  bool _cartIsVisible
  {
    get
    {
      return _thumbnailOpacityAnimation.value == 0.0;
    }
  }

  Widget _buildThumbnails(int numProducts)
  {
    return new Opacity(
      opacity: _thumbnailOpacityAnimation.value,
      child: new Column(
        children: new List<Widget>
        {
          new Row(
            children: new List<Widget>
            {
              new AnimatedPadding(
                padding: _cartPaddingFor(numProducts),
                child: new Icon(Icons.shopping_cart),
                duration: TimeSpan.FromMilliseconds(225)
              ),
              new Container(
                // Accounts for the overflow number
                width: numProducts > 3 ? _width - 94.0f : _width - 64.0f,
                height: expanding_buttom_sheetUtils._kCartHeight,
                padding: EdgeInsets.symmetric(vertical: 8.0f),
                child: new ProductThumbnailRow()
              ), 
              new ExtraProductsNumber(),
            }
          ),
        }
      )
    );
  }

  Widget _buildShoppingCartPage() {
    return new Opacity(
      opacity: _cartOpacityAnimation.value,
      child: new ShoppingCartPage()
    );
  }

  Widget _buildCart(BuildContext context, Widget child) {

     AppStateModel model = ScopedModel<AppStateModel>.of(context);
     int numProducts = model.productsInCart.Keys.Count;
     int totalCartQuantity = model.totalCartQuantity;
     Size screenSize = MediaQuery.of(context).size;
     float screenWidth = screenSize.width;
     float screenHeight = screenSize.height;

    _width = _widthFor(numProducts);
    _widthAnimation = _getWidthAnimation(screenWidth);
    _heightAnimation = _getHeightAnimation(screenHeight);
    _shapeAnimation = _getShapeAnimation();
    _thumbnailOpacityAnimation = _getThumbnailOpacityAnimation();
    _cartOpacityAnimation = _getCartOpacityAnimation();

    return new Container(
        width: _widthAnimation.value,
        height: _heightAnimation.value,
        child: new Material(
          animationDuration: TimeSpan.FromMilliseconds(0),
          shape: new BeveledRectangleBorder(
            borderRadius: BorderRadius.only(
              topLeft: Radius.circular(_shapeAnimation.value)
            )
          ),
          elevation: 4.0f,
          color: shrineColorsUtils.kShrinePink50,
          child: _cartIsVisible
            ? _buildShoppingCartPage()
            : _buildThumbnails(numProducts)
        )
    );
  }

  // Builder for the hide and reveal animation when the backdrop opens and closes
  Widget _buildSlideAnimation(BuildContext context, Widget child) {
    _slideAnimation = expanding_buttom_sheetUtils._getOffsetEmphasizedEasingAnimation(
      begin: new Offset(1.0f, 0.0f),
      peak: new Offset(expanding_buttom_sheetUtils._kPeakVelocityProgress, 0.0f),
      end: new Offset(0.0f, 0.0f),
      isForward: widget.hideController.status == AnimationStatus.forward,
      parent: widget.hideController
    );

    return new SlideTransition(
      position: _slideAnimation,
      child: child
    );
  }

  // Closes the cart if the cart is open, otherwise exits the app (this should
  // only be relevant for Android).
  Future<bool> _onWillPop()  {
    if (!_isOpen)
    {
      //SystemNavigator.pop();
      return Future.value(true).to<bool>();
    }

    close();
    return Future.value(true).to<bool>();
  }

 
  public override Widget build(BuildContext context) {
    return new AnimatedSize(
      key: _expandingBottomSheetKey,
      duration: TimeSpan.FromMilliseconds(225),
      curve: Curves.easeInOut,
      vsync: this,
      alignment: FractionalOffset.topLeft,
      child: new WillPopScope(
        onWillPop: _onWillPop,
        child: new AnimatedBuilder(
          animation: widget.hideController,
          builder: _buildSlideAnimation,
          child: new GestureDetector(
            behavior: HitTestBehavior.opaque,
            onTap: open,
            child: new ScopedModelDescendant<AppStateModel>(
              builder: (BuildContext context2, Widget child, AppStateModel model)=> {
                return new AnimatedBuilder(
                  builder: _buildCart,
                  animation: _controller
                );
              }
            )
          )
        )
      )
    );
  }
}

public class ProductThumbnailRow : StatefulWidget {
  
  public override State createState() => new _ProductThumbnailRowState();
}

  public class _ProductThumbnailRowState : State<ProductThumbnailRow> {
      public readonly GlobalKey<AnimatedListState> _listKey = GlobalKey<AnimatedListState>.key();
  
      _ListModel _list;
      List<int> _internalList;

  
      public override void initState() {
        base.initState();
        _list = new _ListModel(
          listKey: _listKey,
          initialItems: ScopedModel<AppStateModel>.of(context).productsInCart.Keys.ToList(),
          removedItemBuilder: _buildRemovedThumbnail
        );
    _internalList = new List<int>(_list.list);
  }

  Product _productWithId(int productId) { 
    AppStateModel model = ScopedModel<AppStateModel>.of(context);
    Product product = model.getProductById(productId);
    D.assert(product != null);
    return product;
  }

  Widget _buildRemovedThumbnail(int item, BuildContext context, Animation<float> animation) {
    return new ProductThumbnail(animation, animation, _productWithId(item));
  }

  Widget _buildThumbnail(BuildContext context, int index, Animation<float> animation) {
     Animation<float> thumbnailSize = new FloatTween(begin: 0.8f, end: 1.0f).animate(
      new CurvedAnimation(
        curve: new Interval(0.33f, 1.0f, curve: Curves.easeIn),
        parent: animation
      )
    );

    Animation<float> opacity = new CurvedAnimation(
      curve: new Interval(0.33f, 1.0f, curve: Curves.linear),
      parent: animation
    );

    return new ProductThumbnail(thumbnailSize, opacity, _productWithId(_list.ElementAt(index)));
  }
  
  void _updateLists() {
    _internalList = ScopedModel<AppStateModel>.of(context).productsInCart.Keys.ToList();
    HashSet<int> internalSet = new HashSet<int>(_internalList);
    HashSet<int> listSet = new HashSet<int>(_list.list);

    HashSet<int> difference = new HashSet<int>();
    foreach (var _set in internalSet)
    {
      if (!listSet.Contains(_set))
      {
        difference.Add(_set);
      }
    }
    if (difference.isEmpty()) {
      return;
    }

    foreach (int product in difference) {
      if (_internalList.Count < _list.length) {
        _list.remove(product);
      } else if (_internalList.Count > _list.length) {
        _list.add(product);
      }
    }

    while (_internalList.Count != _list.length) {
      int index = 0;
      // Check bounds and that the list elements are the same
      while (_internalList.isNotEmpty() &&
          _list.length > 0 &&
          index < _internalList.Count &&
          index < _list.length &&
          _internalList[index] == _list.ElementAt(index)) {
        index++;
      }
    }
  }

  Widget _buildAnimatedList() {
    return new AnimatedList(
      key: _listKey,
      shrinkWrap: true,
      itemBuilder: _buildThumbnail,
      initialItemCount: _list.length,
      scrollDirection: Axis.horizontal,
      physics: new NeverScrollableScrollPhysics() // Cart shouldn't scroll
    );
  }
  
  public override Widget build(BuildContext context) {
    _updateLists();
    return new ScopedModelDescendant<AppStateModel>(
      builder: (BuildContext context2, Widget child, AppStateModel model) => _buildAnimatedList()
    );
  }
}

public class ExtraProductsNumber : StatelessWidget {
  int _calculateOverflow(AppStateModel model) {
    Dictionary<int, int> productMap = model.productsInCart;
    List<int> products = productMap.Keys.ToList();
    int overflow = 0;
    int numProducts = products.Count;
    if (numProducts > 3) {
      for (int i = 3; i < numProducts; i++) {
        overflow += productMap[products[i]];
      }
    }
    return overflow;
  }

  Widget _buildOverflow(AppStateModel model, BuildContext context) {
    if (model.productsInCart.Count <= 3)
      return new Container();

    int numOverflowProducts = _calculateOverflow(model);
    int displayedOverflowProducts = numOverflowProducts <= 99 ? numOverflowProducts : 99;
    return new Container(
      child: new Text(
        $"+{displayedOverflowProducts}",
        style: Theme.of(context).primaryTextTheme.button
      )
    );
  }
  
  public override Widget build(BuildContext context) {
    return new ScopedModelDescendant<AppStateModel>(
      builder: (BuildContext builder, Widget child, AppStateModel model) => _buildOverflow(model, context)
    );
  }
}

public class ProductThumbnail : StatelessWidget {
    public ProductThumbnail(Animation<float> animation, Animation<float> opacityAnimation, Product product)
    {
        this.animation = animation;
        this.opacityAnimation = opacityAnimation;
        this.product = product;
    }

    public readonly Animation<float> animation;
    public readonly Animation<float> opacityAnimation;
    public readonly Product product;

  
  public override Widget build(BuildContext context) {
    return new FadeTransition(
      opacity: opacityAnimation,
      child: new ScaleTransition(
        scale: animation,
        child: new Container(
          width: 40.0f,
          height: 40.0f,
          decoration: new BoxDecoration(
            image: new DecorationImage(
              image: new FileImage(
                product.assetName
              ),
              fit: BoxFit.cover
            ),
            borderRadius: BorderRadius.all(Radius.circular(10.0f))
          ),
          margin: EdgeInsets.only(left: 16.0f)
        )
      )
    );
  }
}
public delegate Widget RemovedItemBuilder(int item, BuildContext context, Animation<float> animation) ;
public class _ListModel {
  public _ListModel(
    GlobalKey<AnimatedListState> listKey,
    RemovedItemBuilder removedItemBuilder,
    IEnumerable<int> initialItems = null
  )
  {
    D.assert(listKey != null);
    D.assert(removedItemBuilder != null);
    _items = initialItems?.ToList() ?? new List<int>();
    this.listKey = listKey;
    this.removedItemBuilder = removedItemBuilder;

  }

  public readonly GlobalKey<AnimatedListState> listKey;
  public RemovedItemBuilder removedItemBuilder;
  public readonly List<int> _items;

  AnimatedListState _animatedList
  {
    get
    {
      return listKey.currentState;
    }
  }

  public void add(int product) {
    _insert(_items.Count, product);
  }

  public void _insert(int index, int item) {
    _items.Insert(index, item);
    _animatedList.insertItem(index, duration: TimeSpan.FromMilliseconds(225));
  }

  public void remove(int product) {
    int index = _items.IndexOf(product);
    if (index >= 0) {
      _removeAt(index);
    }
  }

  public void _removeAt(int index)
  {
    int removedItem = _items.ElementAt(index);
    _items.RemoveAt(index);
    _animatedList.removeItem(index, (BuildContext context, Animation<float> animation) =>{
        return removedItemBuilder(removedItem, context, animation);
      });
    
  }

  public int length
  {
    get
    {
      return _items.Count;
    }
  }

  public int ElementAt(int index) => _items[index];

  int indexOf(int item) => _items.IndexOf(item);

  public List<int> list
  {
    get
    {
      return _items;
    }
  }
}

}