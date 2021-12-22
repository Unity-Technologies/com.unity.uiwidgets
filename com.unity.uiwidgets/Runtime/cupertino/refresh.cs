using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.cupertino {
   class _CupertinoSliverRefresh : SingleChildRenderObjectWidget { 
       public _CupertinoSliverRefresh(
        Key key = null,
        float? refreshIndicatorLayoutExtent = 0.0f,
        bool? hasLayoutExtent = false,
        Widget child = null
        ) : base(key: key, child: child) {
        D.assert(refreshIndicatorLayoutExtent != null);
        D.assert(refreshIndicatorLayoutExtent >= 0.0);
        D.assert(hasLayoutExtent != null);
        this.refreshIndicatorLayoutExtent = refreshIndicatorLayoutExtent.Value;
        this.hasLayoutExtent = hasLayoutExtent.Value;
       }
       public readonly float refreshIndicatorLayoutExtent; 
       public readonly bool hasLayoutExtent;
       
       public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoSliverRefresh(
              refreshIndicatorExtent: refreshIndicatorLayoutExtent,
              hasLayoutExtent: hasLayoutExtent
            );
       }
       public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
           renderObject = (_RenderCupertinoSliverRefresh)renderObject;
           ((_RenderCupertinoSliverRefresh) renderObject).refreshIndicatorLayoutExtent = refreshIndicatorLayoutExtent;
           ((_RenderCupertinoSliverRefresh) renderObject).hasLayoutExtent = hasLayoutExtent;
       }
   }
   class _RenderCupertinoSliverRefresh : RenderObjectWithChildMixinRenderSliver<RenderBox> { 
       public _RenderCupertinoSliverRefresh(
           float refreshIndicatorExtent,
           bool hasLayoutExtent = false,
           RenderBox child = null) {
          
           D.assert(refreshIndicatorExtent >= 0.0f);
           _refreshIndicatorExtent = refreshIndicatorExtent;
           _hasLayoutExtent = hasLayoutExtent;
           this.child = child; 
       }


       public float refreshIndicatorLayoutExtent {
           get { return _refreshIndicatorExtent; }
           set {
               D.assert(value >= 0.0);
               if (value == _refreshIndicatorExtent)
                   return;
               _refreshIndicatorExtent = value;
               markNeedsLayout();
           }
       }

       float _refreshIndicatorExtent;

       public bool hasLayoutExtent {
           get { return _hasLayoutExtent; }
           set {
               if (value == _hasLayoutExtent)
                   return;
               _hasLayoutExtent = value;
               markNeedsLayout();
           }
       }

       bool _hasLayoutExtent;
       float layoutExtentOffsetCompensation = 0.0f;

       protected override void performLayout() { 
           SliverConstraints constraints = this.constraints;
           D.assert(constraints.axisDirection == AxisDirection.down);
           D.assert(constraints.growthDirection == GrowthDirection.forward);
           float layoutExtent =
            (_hasLayoutExtent ? 1.0f : 0.0f) * _refreshIndicatorExtent;
           if (layoutExtent != layoutExtentOffsetCompensation) {
               geometry = new SliverGeometry(
                scrollOffsetCorrection: layoutExtent - layoutExtentOffsetCompensation
              );
              layoutExtentOffsetCompensation = layoutExtent;
            
           }
           bool active = constraints.overlap < 0.0 || layoutExtent > 0.0;
           float overscrolledExtent = 
               constraints.overlap < 0.0f ? constraints.overlap.abs() : 0.0f;
           child.layout(
               constraints.asBoxConstraints(
                   maxExtent: layoutExtent + overscrolledExtent
                   ), 
               parentUsesSize: true
               ); 
           if (active) { 
               geometry = new SliverGeometry(
                   scrollExtent: layoutExtent,
                    paintOrigin: -overscrolledExtent - constraints.scrollOffset,
                    paintExtent: Mathf.Max(
                        Mathf.Max(child.size.height, layoutExtent) - constraints.scrollOffset, 0.0f), 
                   maxPaintExtent: Mathf.Max(
                       Mathf.Max(child.size.height, layoutExtent) - constraints.scrollOffset, 0.0f), 
                   layoutExtent: Mathf.Max(layoutExtent - constraints.scrollOffset, 0.0f)); 
           } else {
               geometry = SliverGeometry.zero; 
           } 
       }
       public override void paint(PaintingContext paintContext, Offset offset) { 
           if (constraints.overlap < 0.0 || constraints.scrollOffset + child.size.height > 0) { 
               paintContext.paintChild(child, offset);
           }
       }
       public override void applyPaintTransform(RenderObject child, Matrix4 transform) { }
   }

   public enum RefreshIndicatorMode {
      /// Initial state, when not being overscrolled into, or after the overscroll
      /// is canceled or after done and the sliver retracted away.
      inactive,

      /// While being overscrolled but not far enough yet to trigger the refresh.
      drag,

      /// Dragged far enough that the onRefresh callback will run and the dragged
      /// displacement is not yet at the final refresh resting state.
      armed,

      /// While the onRefresh task is running.
      refresh,

      /// While the indicator is animating away after refreshing.
      done,
   }
   public delegate Widget RefreshControlIndicatorBuilder(
        BuildContext context,
        RefreshIndicatorMode refreshState,
        float pulledExtent,
        float refreshTriggerPullDistance,
        float refreshIndicatorExtent
    );

   public delegate Future RefreshCallback();

   public class CupertinoSliverRefreshControl : StatefulWidget { 
       public CupertinoSliverRefreshControl(
           Key key = null,
           float? refreshTriggerPullDistance = _defaultRefreshTriggerPullDistance, 
           float? refreshIndicatorExtent = _defaultRefreshIndicatorExtent, 
           RefreshControlIndicatorBuilder  builder = null,
           RefreshCallback  onRefresh = null
           ) : base(key: key) {
              D.assert(refreshTriggerPullDistance != null);
              D.assert(refreshTriggerPullDistance > 0.0);
              D.assert(refreshIndicatorExtent != null);
              D.assert(refreshIndicatorExtent >= 0.0);
              D.assert(
                  refreshTriggerPullDistance >= refreshIndicatorExtent, () =>
                      "The refresh indicator cannot take more space in its final state " +
                      "than the amount initially created by overscrolling."
              );
              this.refreshIndicatorExtent = refreshIndicatorExtent.Value;
              this.refreshTriggerPullDistance = refreshTriggerPullDistance.Value;
              this.builder = builder ?? buildSimpleRefreshIndicator;
              this.onRefresh = onRefresh;
              

       }
       public readonly float refreshTriggerPullDistance;
       public readonly float refreshIndicatorExtent;
       public readonly RefreshControlIndicatorBuilder builder;
       public readonly RefreshCallback onRefresh;
       const float _defaultRefreshTriggerPullDistance = 100.0f;
       const float _defaultRefreshIndicatorExtent = 60.0f;
       
       static RefreshIndicatorMode state(BuildContext context) { 
           _CupertinoSliverRefreshControlState state = context.findAncestorStateOfType<_CupertinoSliverRefreshControlState>(); 
           return state.refreshState; 
       }
       public static Widget buildSimpleRefreshIndicator(
           BuildContext context,
           RefreshIndicatorMode refreshState,
           float pulledExtent,
           float refreshTriggerPullDistance,
           float refreshIndicatorExtent
           ) { 
           Curve opacityCurve = new Interval(0.4f, 0.8f, curve: Curves.easeInOut);
           
           return new Align(
              alignment: Alignment.bottomCenter,
              child: new Padding(
                padding: EdgeInsets.only(bottom: 16.0f),
                child: refreshState == RefreshIndicatorMode.drag
                    ? new Opacity(
                        opacity: opacityCurve.transform(Mathf.Min(pulledExtent / refreshTriggerPullDistance, 1.0f)
                        ),
                        child: new Icon(
                          CupertinoIcons.down_arrow,
                          color: CupertinoDynamicColor.resolve(CupertinoColors.inactiveGray, context),
                          size: 36.0f
                        )
                      )
                    : new Opacity(
                        opacity: opacityCurve.transform(
                            Mathf.Min(pulledExtent / refreshIndicatorExtent, 1.0f)
                        ),
                        child: new CupertinoActivityIndicator(radius: 14.0f)
                      )
                    )
                );
       }

       public override State createState() {
           return new _CupertinoSliverRefreshControlState();       

       }
   }
   public class _CupertinoSliverRefreshControlState : State<CupertinoSliverRefreshControl> {
       public const float _inactiveResetOverscrollFraction = 0.1f;
       public RefreshIndicatorMode refreshState;
       Future refreshTask;
       float latestIndicatorBoxExtent = 0.0f;
       bool hasSliverLayoutExtent = false;
       public override void initState() { 
           base.initState(); 
           refreshState = RefreshIndicatorMode.inactive;
        }
       RefreshIndicatorMode transitionNextState() { 
           RefreshIndicatorMode nextState = RefreshIndicatorMode.refresh;

           void goToDone() {
              nextState = RefreshIndicatorMode.done;
             
              if (SchedulerBinding.instance.schedulerPhase == SchedulerPhase.idle) {
                setState(() => hasSliverLayoutExtent = false);
              } else {
                SchedulerBinding.instance.addPostFrameCallback((timestamp)=> {
                  setState(() => hasSliverLayoutExtent = false);
                });
              }
           }
           switch (refreshState) {
              case RefreshIndicatorMode.inactive:
                if (latestIndicatorBoxExtent <= 0) {
                  return RefreshIndicatorMode.inactive;
                } else {
                  nextState = RefreshIndicatorMode.drag;
                }

                goto case RefreshIndicatorMode.drag;
              
              case RefreshIndicatorMode.drag:
                if (latestIndicatorBoxExtent == 0) {
                  return RefreshIndicatorMode.inactive;
                } else if (latestIndicatorBoxExtent < widget.refreshTriggerPullDistance) {
                  return RefreshIndicatorMode.drag;
                } else {
                  if (widget.onRefresh != null) { //HapticFeedback.mediumImpact();
                   
                    SchedulerBinding.instance.addPostFrameCallback((timestamp)=> {
                      refreshTask = widget.onRefresh().whenComplete(() =>{
                        if (mounted) {
                          setState(() => refreshTask = null);
                         
                          refreshState = transitionNextState();
                        }
                      });
                      setState(() => hasSliverLayoutExtent = true);
                    });
                  }
                  return RefreshIndicatorMode.armed;
                }
              
              case RefreshIndicatorMode.armed:
                if (refreshState == RefreshIndicatorMode.armed && refreshTask == null) {
                  goToDone();
                  goto case RefreshIndicatorMode.done;
                }

                if (latestIndicatorBoxExtent > widget.refreshIndicatorExtent) {
                  return RefreshIndicatorMode.armed;
                } else {
                  nextState = RefreshIndicatorMode.refresh;
                }

                goto case RefreshIndicatorMode.refresh;
             
              case RefreshIndicatorMode.refresh:
                if (refreshTask != null) {
                  return RefreshIndicatorMode.refresh;
                } else {
                  goToDone();
                }

                goto case RefreshIndicatorMode.done;
             
              case RefreshIndicatorMode.done:
                
                if (latestIndicatorBoxExtent >
                    widget.refreshTriggerPullDistance * _inactiveResetOverscrollFraction) {
                  return RefreshIndicatorMode.done;
                } else {
                  nextState = RefreshIndicatorMode.inactive;
                }
                break;
           }
           return nextState;
       }
       public override Widget build(BuildContext context) {
            return new _CupertinoSliverRefresh(
              refreshIndicatorLayoutExtent: widget.refreshIndicatorExtent,
              hasLayoutExtent: hasSliverLayoutExtent,
             
              child: new LayoutBuilder(
                builder: (BuildContext context1, BoxConstraints constraints) =>{
                  latestIndicatorBoxExtent = constraints.maxHeight;
                  refreshState = transitionNextState();
                  if (widget.builder != null && latestIndicatorBoxExtent > 0) {
                    return widget.builder(
                      context1,
                      refreshState,
                      latestIndicatorBoxExtent,
                      widget.refreshTriggerPullDistance,
                      widget.refreshIndicatorExtent
                    );
                  }
                  return new Container();
                }
              )
            );
        }
    }
}