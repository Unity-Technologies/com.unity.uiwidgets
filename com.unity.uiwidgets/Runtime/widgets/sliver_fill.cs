using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class SliverFillViewport : StatelessWidget { 
        public SliverFillViewport(
            Key key = null, 
            SliverChildDelegate _delegate = null, 
            float viewportFraction = 1.0f, 
            bool padEnds = true
            ) : base(key: key) {
            D.assert(viewportFraction > 0.0);
            this._delegate = _delegate;
            this.viewportFraction = viewportFraction;
            this.padEnds = padEnds;
        }

        public readonly float viewportFraction;
        public readonly bool padEnds;
        public readonly SliverChildDelegate _delegate;

        public override Widget build(BuildContext context) { 
            return new _SliverFractionalPadding(
                viewportFraction: padEnds ? ((int)(1 - viewportFraction).clamp(0f, 1f) / 2) : 0, 
                sliver: new _SliverFillViewportRenderObjectWidget(
                    viewportFraction: viewportFraction, 
                    _delegate: _delegate
                )
            );
        }
    }
    public class _SliverFillViewportRenderObjectWidget : SliverMultiBoxAdaptorWidget { 
        public _SliverFillViewportRenderObjectWidget(
            Key key = null, 
            SliverChildDelegate _delegate = null, 
            float viewportFraction = 1.0f
            ) :base(key: key, del: _delegate) {
            D.assert(viewportFraction > 0.0);
            this.viewportFraction = viewportFraction;

        }

        public readonly float viewportFraction;
        public override RenderObject createRenderObject(BuildContext context) {
            SliverMultiBoxAdaptorElement element = context as SliverMultiBoxAdaptorElement;
            return new RenderSliverFillViewport(childManager: element, viewportFraction: viewportFraction);
        }
        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (RenderSliverFillViewport) renderObject;
            ((RenderSliverFillViewport)renderObject).viewportFraction = viewportFraction;
        }
    }
    public class _SliverFractionalPadding : SingleChildRenderObjectWidget {
        public _SliverFractionalPadding(
            int viewportFraction = 0,
            Widget sliver = null
        ) : base(child: sliver) {
            D.assert(viewportFraction >= 0);
            D.assert(viewportFraction <= 0.5);
            this.viewportFraction = viewportFraction;
        }

        public readonly float viewportFraction;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSliverFractionalPadding(viewportFraction: viewportFraction);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (_RenderSliverFractionalPadding) renderObject;
            ((_RenderSliverFractionalPadding)renderObject).viewportFraction = viewportFraction;
        }
    }
    public class _RenderSliverFractionalPadding : RenderSliverEdgeInsetsPadding {
        public _RenderSliverFractionalPadding(
            float? viewportFraction = 0
        ) {
            D.assert(viewportFraction != null);
            D.assert(viewportFraction <= 0.5);
            D.assert(viewportFraction >= 0);
            _viewportFraction = viewportFraction.Value;
        }

        SliverConstraints _lastResolvedConstraints;

        public float viewportFraction {
            get {return _viewportFraction; }
            set {
                if (_viewportFraction == value)
                    return;
                _viewportFraction = value;
                _markNeedsResolution();
            }
        }
        float _viewportFraction;


        protected override EdgeInsets resolvedPadding {
            get {
                return _resolvedPadding;
            }
        }
        EdgeInsets _resolvedPadding;
        void _markNeedsResolution() {
            _resolvedPadding = null;
            markNeedsLayout();
        }
        void _resolve() {
            if (_resolvedPadding != null && _lastResolvedConstraints == constraints)
              return;
            
            float paddingValue = constraints.viewportMainAxisExtent * viewportFraction;
            _lastResolvedConstraints = constraints;
            switch (constraints.axis) {
              case Axis.horizontal:
                _resolvedPadding = EdgeInsets.symmetric(horizontal: paddingValue);
                break;
              case Axis.vertical:
                _resolvedPadding = EdgeInsets.symmetric(vertical: paddingValue);
                break;
            }
        }

        protected override void performLayout() {
            _resolve();
            base.performLayout();
        }
    }
    public class SliverFillRemaining : StatelessWidget {
        public SliverFillRemaining(
            Key key = null,
            Widget child = null,
            bool hasScrollBody = true,
            bool fillOverscroll = false) : base(key: key) {
            this.child = child;
            this.hasScrollBody = hasScrollBody;
            this.fillOverscroll = fillOverscroll;
        }
        public readonly Widget child;
        public readonly bool hasScrollBody;
        public readonly bool fillOverscroll;
        public override Widget build(BuildContext context) { 
            if (hasScrollBody) 
                return new _SliverFillRemainingWithScrollable(child: child);
            if (!fillOverscroll) 
                return new _SliverFillRemainingWithoutScrollable(child: child);
            return new _SliverFillRemainingAndOverscroll(child: child);
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties); 
            properties.add(new DiagnosticsProperty<Widget>("child", child));
            List<string> flags = new List<string>();
            if (hasScrollBody)
                flags.Add("scrollable"); 
            if (fillOverscroll) 
                flags.Add("fillOverscroll");
            if (flags.isEmpty()) 
                flags.Add("nonscrollable"); 
            properties.add(new EnumerableProperty<string>("mode", flags));
        }
    }

    public class _SliverFillRemainingWithScrollable : SingleChildRenderObjectWidget {
        public _SliverFillRemainingWithScrollable(
            Key key = null,
            Widget child = null) : base(key: key, child: child) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverFillRemainingWithScrollable();
        }
    }

    class _SliverFillRemainingWithoutScrollable : SingleChildRenderObjectWidget { 
        public _SliverFillRemainingWithoutScrollable(
            Key key = null,
            Widget child = null) : base(key: key, child: child) {
        }
        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverFillRemaining();
        }
    }
    class _SliverFillRemainingAndOverscroll : SingleChildRenderObjectWidget {
        public _SliverFillRemainingAndOverscroll(
            Key key = null,
            Widget child = null
        ) : base(key: key, child: child) {
        
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverFillRemainingAndOverscroll();
        }
    }

}