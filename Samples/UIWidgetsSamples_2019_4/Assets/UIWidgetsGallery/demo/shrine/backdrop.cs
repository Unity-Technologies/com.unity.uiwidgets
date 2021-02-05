using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.shrine
{
    public class backdropUtils
    {
        public static readonly Cubic _kAccelerateCurve = new Cubic(0.548f, 0.0f, 0.757f, 0.464f);
        public static readonly Cubic _kDecelerateCurve = new Cubic(0.23f, 0.94f, 0.41f, 1.0f);
        public static readonly float _kPeakVelocityTime = 0.248210f;
        public static readonly float _kPeakVelocityProgress = 0.379146f;
    }
    

    public class _TappableWhileStatusIs : StatefulWidget {
        public _TappableWhileStatusIs(
            AnimationStatus status,
            Key key = null,
            AnimationController controller = null,
            Widget child = null
        ) : base(key: key)
        {
            this.status = status;
            this.controller = controller;
            this.child = child;
        }

        public readonly AnimationController controller;
        public readonly AnimationStatus status;
        public readonly Widget child;
        
        public override State createState() => new _TappableWhileStatusIsState();
    }

    public class _TappableWhileStatusIsState : State<_TappableWhileStatusIs> {
        bool _active;
        
        public override void initState() {
            base.initState();
            widget.controller.addStatusListener(_handleStatusChange);
            _active = widget.controller.status == widget.status;
        }

      
        public override void dispose() {
            widget.controller.removeStatusListener(_handleStatusChange);
            base.dispose();
        }

        void _handleStatusChange(AnimationStatus status) {
            bool value = widget.controller.status == widget.status;
            if (_active != value) {
                setState(() => {
                    _active = value;
                });
            }
        }

        
        public override Widget build(BuildContext context) {
            Widget child = new AbsorbPointer(
                absorbing: !_active,
                child: widget.child
            );

            if (!_active) {
                child = new FocusScope(
                    canRequestFocus: false,
                    debugLabel: "_TappableWhileStatusIs",
                    child: child
                );
            }
            return child;
        }
    }

    public class _FrontLayer : StatelessWidget {
        public _FrontLayer(
            Key key = null,
            VoidCallback onTap = null,
            Widget child = null
        ) : base(key: key)
        {
            this.onTap = onTap;
            this.child = child;
        }

        public readonly VoidCallback onTap;
        public readonly Widget child;

        
        public override Widget build(BuildContext context) {
            return new Material(
                elevation: 16.0f,
                shape: new BeveledRectangleBorder(
                    borderRadius: BorderRadius.only(topLeft: Radius.circular(46.0f))
                ),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: new List<Widget>{
                        new GestureDetector(
                            behavior: HitTestBehavior.opaque,
                            onTap: () => { onTap?.Invoke();},
                            child: new Container(
                            height: 40.0f,
                            alignment: AlignmentDirectional.centerStart
                            )
                        ),
                        new Expanded(
                          child: child
                        ),
                    }
                )
            );
        }
    }
    public delegate void  OnPress();

    public class _BackdropTitle : AnimatedWidget {
        public _BackdropTitle(
            Key key = null,
            Animation<float> listenable = null,
            OnPress onPress = null,
            Widget frontTitle = null,
            Widget backTitle = null
        ) : base(key: key, listenable: listenable)
        {
            D.assert(frontTitle != null);
            D.assert(backTitle != null);
            this.backTitle = backTitle;
            this.frontTitle = frontTitle;
            this.onPress = onPress;
        }

        public readonly OnPress onPress;
        public readonly Widget frontTitle;
        public readonly Widget backTitle;

      
        protected override Widget build(BuildContext context) {
            Animation<float> animation = new CurvedAnimation(
              parent: listenable as Animation<float>,
              curve: new Interval(0.0f, 0.78f)
            );

            return new DefaultTextStyle(
                style: Theme.of(context).primaryTextTheme.headline6,
                softWrap: false,
                overflow: TextOverflow.ellipsis,
                child: new Row(children: new List<Widget>{
                new SizedBox(
                    width: 72.0f,
                    child: new IconButton(
                        padding: EdgeInsets.only(right: 8.0f),
                        onPressed: ()=> { onPress?.Invoke();},
                        icon: new Stack(children: new List<Widget>{
                            new Opacity(
                              opacity: animation.value,
                              child: new Container(
                                  width: 20.0f,
                                  height: 20.0f,
                                  decoration: new BoxDecoration(
                                      image: new DecorationImage(
                                          image: new FileImage(
                                              file: "shrine_images/slanted_menu.png"
                                          )
                                      ),
                                      shape: BoxShape.circle
                                  )
                              )
                            ),
                            new FractionalTranslation(
                              translation: new OffsetTween(
                              begin: Offset.zero,
                              end: new Offset(1.0f, 0.0f)).evaluate(animation),
                              child: new Container(
                                  width: 20.0f,
                                  height: 20.0f,
                                  decoration: new BoxDecoration(
                                      image: new DecorationImage(
                                          image: new FileImage(
                                              file: "shrine_images/diamond.png"
                                          )
                                      ),
                                      shape: BoxShape.circle
                                  )
                              )
                            )
                        })
                    )
                ),
                new Stack(
                    children: new List<Widget>{
                        new Opacity(
                            opacity: new CurvedAnimation(
                                parent: new ReverseAnimation(animation),
                                curve: new Interval(0.5f, 1.0f)
                            ).value,
                            child: new FractionalTranslation(
                                translation: new OffsetTween(
                                begin: Offset.zero,
                                end: new Offset(0.5f, 0.0f)).evaluate(animation),
                                child: backTitle
                            )
                        ),
                        new Opacity(
                            opacity: new CurvedAnimation(
                                parent: animation,
                                curve: new Interval(0.5f, 1.0f)).value,
                            child: new FractionalTranslation(
                            translation: new OffsetTween(
                                begin: new Offset(-0.25f, 0.0f),
                                end: Offset.zero).evaluate(animation),
                            child: frontTitle
                            )
                        ),
                    }
                ),
            })
          );
        }
    }

    public class Backdrop : StatefulWidget {
        public Backdrop(
            Widget frontLayer,
            Widget backLayer,
            Widget frontTitle,
            Widget backTitle,
            AnimationController controller
        ) {
            D.assert(frontLayer != null);
            D.assert(backLayer != null);
            D.assert(frontTitle != null);
            D.assert(backTitle != null);
            D.assert(controller != null);
            this.frontLayer = frontLayer;
            this.backLayer = backLayer;
            this.frontTitle = frontTitle;
            this.backTitle = backTitle;
            this.controller = controller;
        }

        public readonly Widget frontLayer;
        public readonly Widget backLayer;
        public readonly Widget frontTitle;
        public readonly Widget backTitle;
        public readonly AnimationController controller;

        
        public override State createState() => new _BackdropState();
    }

    public class _BackdropState : SingleTickerProviderStateMixin<Backdrop> {
        public readonly GlobalKey _backdropKey =  GlobalKey.key(debugLabel: "Backdrop");
        AnimationController _controller;
        Animation<RelativeRect> _layerAnimation;

  
        public override void initState() {
            base.initState();
            _controller = widget.controller;
        }
  
        public override void dispose() {
            _controller.dispose();
            base.dispose();
        }

        bool _frontLayerVisible {
            get
            {
                AnimationStatus status = _controller.status;
                return status == AnimationStatus.completed || status == AnimationStatus.forward;
            }
        }

        void _toggleBackdropLayerVisibility() { //[!!!]
            setState(() => {
                var visible = _frontLayerVisible ? _controller.reverse() : _controller.forward();
            });
        }
        
        Animation<RelativeRect> _getLayerAnimation(Size layerSize, float layerTop) {
            Curve firstCurve;
            Curve secondCurve;
            float firstWeight;
            float secondWeight;
            Animation<float> animation;

            if (_frontLayerVisible) {
                firstCurve = backdropUtils._kAccelerateCurve;
                secondCurve = backdropUtils._kDecelerateCurve;
                firstWeight = backdropUtils._kPeakVelocityTime;
                secondWeight = 1.0f - backdropUtils._kPeakVelocityTime;
                animation = new CurvedAnimation(
                parent: _controller.view,
                curve: new Interval(0.0f, 0.78f)
                );
            } else {
                firstCurve = backdropUtils._kDecelerateCurve.flipped;
                secondCurve = backdropUtils._kAccelerateCurve.flipped;
                firstWeight = 1.0f - backdropUtils._kPeakVelocityTime;
                secondWeight = backdropUtils._kPeakVelocityTime;
                animation = _controller.view;
            }

            return new TweenSequence<RelativeRect>(
                new List<TweenSequenceItem<RelativeRect>>{
                    new TweenSequenceItem<RelativeRect>(
                        tween: new RelativeRectTween(
                            begin: RelativeRect.fromLTRB(
                              0.0f,
                              layerTop,
                              0.0f,
                              layerTop - layerSize.height
                            ),
                            end: RelativeRect.fromLTRB(
                              0.0f,
                              layerTop * backdropUtils._kPeakVelocityProgress,
                              0.0f,
                              (layerTop - layerSize.height) * backdropUtils._kPeakVelocityProgress
                            )
                        ).chain(new CurveTween(curve: firstCurve)),
                        weight: firstWeight
                    ),
                    new TweenSequenceItem<RelativeRect>(
                        tween: new RelativeRectTween(
                            begin: RelativeRect.fromLTRB(
                              0.0f,
                              layerTop * backdropUtils._kPeakVelocityProgress,
                              0.0f,
                              (layerTop - layerSize.height) * backdropUtils._kPeakVelocityProgress
                            ),
                            end: RelativeRect.fill
                        ).chain(new CurveTween(curve: secondCurve)),
                        weight: secondWeight
                    ),
                }
            ).animate(animation);
        }

        Widget _buildStack(BuildContext context, BoxConstraints constraints) {
             float layerTitleHeight = 48.0f;
             Size layerSize = constraints.biggest;
             float layerTop = layerSize.height - layerTitleHeight;

            _layerAnimation = _getLayerAnimation(layerSize, layerTop);

            return new Stack(
                key: _backdropKey,
                children: new List<Widget>{
                    new _TappableWhileStatusIs(
                        AnimationStatus.dismissed,
                        controller: _controller,
                        child: widget.backLayer
                    ),
                    new PositionedTransition(
                        rect: _layerAnimation,
                        child: new _FrontLayer(
                            onTap: _toggleBackdropLayerVisibility,
                            child: new _TappableWhileStatusIs(
                                AnimationStatus.completed,
                                controller: _controller,
                                child: widget.frontLayer
                            )
                        )
                    ),
                }
            );
        }
        
        public override Widget build(BuildContext context) {
            AppBar appBar = new AppBar(
            brightness: Brightness.light,
            elevation: 0.0f,
            titleSpacing: 0.0f,
            title: new _BackdropTitle(
                listenable: _controller.view,
                onPress: _toggleBackdropLayerVisibility,
                frontTitle: widget.frontTitle,
                backTitle: widget.backTitle
                ),
                actions: new List<Widget>{
                    new IconButton(
                        icon: new Icon(Icons.search),
                        onPressed: () => {
                            Navigator.push<object>(
                                context,
                                new MaterialPageRoute(builder: (BuildContext context2) => new LoginPage())
                            );
                        }
                    ),
                    new IconButton(
                        icon: new Icon(Icons.tune),
                        onPressed: () => {
                            Navigator.push<object>(
                            context,
                            new MaterialPageRoute(builder: (BuildContext context2) => new LoginPage())
                            );
                        }
                    ),
                }
            );
            return new Scaffold(
                appBar: appBar,
                body: new LayoutBuilder(
                    builder: _buildStack
                )
            );
        }
    }

}