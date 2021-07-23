using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    class CupertinoDialogUtils {
        public static readonly TextStyle _kCupertinoDialogTitleStyle = new TextStyle(
            inherit:false,
            fontFamily: ".SF UI Display",
            fontSize: 18.0f,
            fontWeight: FontWeight.w600,
            letterSpacing: 0.48f,
            textBaseline: TextBaseline.alphabetic
        );

        public static readonly TextStyle _kCupertinoDialogContentStyle = new TextStyle(
            fontFamily: ".SF UI Text",
            inherit:false,
            fontSize: 13.4f,
            fontWeight: FontWeight.w400,
            height: 1.036f,
            letterSpacing: -0.25f,
            textBaseline: TextBaseline.alphabetic
        );

        public static readonly TextStyle _kCupertinoDialogActionStyle = new TextStyle(
            fontFamily: ".SF UI Text",
            inherit:false,
            fontSize: 16.8f,
            fontWeight: FontWeight.w400,
            color: CupertinoColors.activeBlue,
            textBaseline: TextBaseline.alphabetic
        );

        public const float _kCupertinoDialogWidth = 270.0f;
        public const float _kAccessibilityCupertinoDialogWidth = 310.0f;

        public static readonly BoxDecoration _kCupertinoDialogBlurOverlayDecoration = new BoxDecoration(
            color: CupertinoColors.white,
            backgroundBlendMode: BlendMode.overlay
        );

        public const float _kBlurAmount = 20.0f;
        public const float _kEdgePadding = 20.0f;
        public const float _kMinButtonHeight = 45.0f;
        public const float _kMinButtonFontSize = 10.0f;
        public const float _kDialogCornerRadius = 12.0f;
        public const float _kDividerThickness = 1.0f;
        

        public static readonly Color _kButtonDividerColor = new Color(0x40FFFFFF);

        public static readonly Color _kDialogColor = CupertinoDynamicColor.withBrightness(
            color: new Color(0xCCF2F2F2),
            darkColor: new Color(0xBF1E1E1E)
            );
        public static readonly Color _kDialogPressedColor = CupertinoDynamicColor.withBrightness(
            color: new Color(0xFFE1E1E1),
            darkColor: new Color(0xFF2E2E2E)
        );
        public const float _kMaxRegularTextScaleFactor = 1.4f;

        public static bool _isInAccessibilityMode(BuildContext context) {
            MediaQueryData data = MediaQuery.of(context, nullOk: true);
            return data != null && data.textScaleFactor > _kMaxRegularTextScaleFactor;
        }
    }


    public class CupertinoAlertDialog : StatelessWidget {
        public CupertinoAlertDialog(
            Key key = null,
            Widget title = null,
            Widget content = null,
            List<Widget> actions = null,
            ScrollController scrollController = null,
            ScrollController actionScrollController = null,
            TimeSpan? insetAnimationDuration = null,
            Curve insetAnimationCurve = null
        ) : base(key: key) {
            D.assert(actions != null);
            this.title = title;
            this.content = content;
            this.actions = actions ?? new List<Widget>();
            this.scrollController = scrollController;
            this.actionScrollController = actionScrollController;
            this.insetAnimationDuration = insetAnimationDuration ?? TimeSpan.FromMilliseconds(100);
            this.insetAnimationCurve = insetAnimationCurve ?? Curves.decelerate;
        }

        public readonly Widget title;
        public readonly Widget content;
        public readonly List<Widget> actions;
        public readonly ScrollController scrollController;
        public readonly ScrollController actionScrollController;
        readonly TimeSpan? m_InsetAnimationDuration;
        public readonly TimeSpan insetAnimationDuration;
        public readonly Curve insetAnimationCurve;

        Widget _buildContent(BuildContext context) {
            List<Widget> children = new List<Widget>();
            if (title != null || content != null) {
                Widget titleSection = new _CupertinoDialogAlertContentSection(
                    title: title,
                    content: content,
                    scrollController: scrollController
                );
                children.Add(new Flexible
                (
                    flex: 3, 
                    child: titleSection));
            }

            return new Container(
                color: CupertinoDynamicColor.resolve(CupertinoDialogUtils._kDialogColor, context),
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: children
                )
            );
        }

        Widget _buildActions() {
            Widget actionSection = new Container(
                height: 0.0f
            );
            if (actions.isNotEmpty()) {
                actionSection = new _CupertinoDialogAlertActionSection(
                    children: actions,
                    scrollController: actionScrollController
                );
            }

            return actionSection;
        }

        public override Widget build(BuildContext context) {
            CupertinoLocalizations localizations = CupertinoLocalizations.of(context);
            bool isInAccessibilityMode = CupertinoDialogUtils._isInAccessibilityMode(context);
            float textScaleFactor = MediaQuery.of(context).textScaleFactor;
            return  new CupertinoUserInterfaceLevel(
                data: CupertinoUserInterfaceLevelData.elevatedlayer,
                child: new MediaQuery(
                    data: MediaQuery.of(context).copyWith(
                        textScaleFactor: Mathf.Max(textScaleFactor, 1.0f)
                    ),
                child: new LayoutBuilder(
                    builder: (BuildContext _context, BoxConstraints constraints) => {
                        return new AnimatedPadding(
                            padding: MediaQuery.of(_context).viewInsets + EdgeInsets.symmetric(horizontal: 40.0f, vertical: 24.0f),
                            duration: insetAnimationDuration,
                            curve: insetAnimationCurve,
                            child: MediaQuery.removeViewInsets(
                                removeLeft: true,
                                removeTop: true,
                                removeRight: true,
                                removeBottom: true,
                                context: _context,
                                child: new Center(
                                    child: new Container(
                                        margin: EdgeInsets.symmetric(vertical: CupertinoDialogUtils._kEdgePadding),
                                        width: isInAccessibilityMode
                                            ? CupertinoDialogUtils._kAccessibilityCupertinoDialogWidth
                                            : CupertinoDialogUtils._kCupertinoDialogWidth,
                                        child: new CupertinoPopupSurface(
                                            isSurfacePainted: false,
                                            child: new _CupertinoDialogRenderWidget(
                                                contentSection: _buildContent(_context),
                                                actionsSection: _buildActions()
                                            )
                                        )
                                    )
                            )
                            )
                        );
                    }
                )
            )
                );
        }
    }

    public class CupertinoDialog : StatelessWidget {
        public CupertinoDialog(
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new Center(
                child: new SizedBox(
                    width: CupertinoDialogUtils._kCupertinoDialogWidth,
                    child: new CupertinoPopupSurface(
                        child: child
                    )
                )
            );
        }
    }

    public class CupertinoPopupSurface : StatelessWidget {
        public CupertinoPopupSurface(
            Key key = null,
            bool isSurfacePainted = true,
            Widget child = null
        ) : base(key: key) {
            this.isSurfacePainted = isSurfacePainted;
            this.child = child;
        }

        public readonly bool isSurfacePainted;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new ClipRRect(
                borderRadius: BorderRadius.circular(CupertinoDialogUtils._kDialogCornerRadius),
                child: new BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: CupertinoDialogUtils._kBlurAmount,
                        sigmaY: CupertinoDialogUtils._kBlurAmount),
                    child: new Container(
                        decoration: CupertinoDialogUtils._kCupertinoDialogBlurOverlayDecoration,
                        child: new Container(
                            color: isSurfacePainted ? CupertinoDynamicColor.resolve(CupertinoDialogUtils._kDialogColor, context) : null,
                            child: child
                        )
                    )
                )
            );
        }
    }

    class _CupertinoDialogRenderWidget : RenderObjectWidget {
        public _CupertinoDialogRenderWidget(
            Key key = null,
            Widget contentSection = null,
            Widget actionsSection = null
        ) : base(key: key) {
            this.contentSection = contentSection;
            this.actionsSection = actionsSection;
        }

        public readonly Widget contentSection;
        public readonly Widget actionsSection;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoDialog(
                dividerThickness: CupertinoDialogUtils._kDividerThickness / MediaQuery.of(context).devicePixelRatio,
                isInAccessibilityMode: CupertinoDialogUtils._isInAccessibilityMode(context),
                dividerColor: CupertinoDynamicColor.resolve(CupertinoColors.separator, context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((_RenderCupertinoDialog) renderObject).isInAccessibilityMode =
                CupertinoDialogUtils._isInAccessibilityMode(context);
            ((_RenderCupertinoDialog) renderObject).dividerColor = 
                CupertinoDynamicColor.resolve(CupertinoColors.separator, context);
        }

        public override Element createElement() {
            return new _CupertinoDialogRenderElement(this);
        }
    }

    class _CupertinoDialogRenderElement : RenderObjectElement {
        public _CupertinoDialogRenderElement(_CupertinoDialogRenderWidget widget) : base(widget) {
        }

        Element _contentElement;
        Element _actionsElement;


        public new _CupertinoDialogRenderWidget widget {
            get { return base.widget as _CupertinoDialogRenderWidget; }
        }

        public new _RenderCupertinoDialog renderObject {
            get { return base.renderObject as _RenderCupertinoDialog; }
        }

        public override void visitChildren(ElementVisitor visitor) {
            if (_contentElement != null) {
                visitor(_contentElement);
            }

            if (_actionsElement != null) {
                visitor(_actionsElement);
            }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            _contentElement = updateChild(_contentElement, widget.contentSection,
                _AlertDialogSections.contentSection);
            _actionsElement = updateChild(_actionsElement, widget.actionsSection,
                _AlertDialogSections.actionsSection);
        }


        protected override void insertChildRenderObject(RenderObject child, object slot) {
            D.assert(slot != null);
            switch ((_AlertDialogSections)slot) {
                case _AlertDialogSections.contentSection:
                    renderObject.contentSection = child as RenderBox;
                    break;
                case _AlertDialogSections.actionsSection:
                    renderObject.actionsSection = child as RenderBox;
                    ;
                    break;
            }
        }

        protected override void moveChildRenderObject(RenderObject child,object slot) {
            //D.assert(false);
        }

        public override void update(Widget newWidget) {
            newWidget = (RenderObjectWidget) newWidget;
            base.update(newWidget);
            _contentElement = updateChild(_contentElement, widget.contentSection,
                _AlertDialogSections.contentSection);
            _actionsElement = updateChild(_actionsElement, widget.actionsSection,
                _AlertDialogSections.actionsSection);
        }

        public override void forgetChild(Element child) {
            D.assert(child == _contentElement || child == _actionsElement);
            if (_contentElement == child) {
                _contentElement = null;
            }
            else {
                D.assert(_actionsElement == child);
                _actionsElement = null;
            }
            base.forgetChild(child);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(child == renderObject.contentSection || child == renderObject.actionsSection);
            if (renderObject.contentSection == child) {
                renderObject.contentSection = null;
            }
            else {
                D.assert(renderObject.actionsSection == child);
                renderObject.actionsSection = null;
            }
        }
    }

    class _RenderCupertinoDialog : RenderBox {
        public _RenderCupertinoDialog(
            RenderBox contentSection = null,
            RenderBox actionsSection = null,
            float dividerThickness = 0.0f,
            bool isInAccessibilityMode = false,
            Color dividerColor = null
        ) {
            _contentSection = contentSection;
            _actionsSection = actionsSection;
            _dividerThickness = dividerThickness;
            _isInAccessibilityMode = isInAccessibilityMode;
            _dividerPaint = new Paint() {
                color = dividerColor,
                style = PaintingStyle.fill
            };

        }

        public RenderBox contentSection {
            get { return _contentSection; }
            set {
                if (value != _contentSection) {
                    if (_contentSection != null) {
                        dropChild(_contentSection);
                    }

                    _contentSection = value;
                    if (_contentSection != null) {
                        adoptChild(_contentSection);
                    }
                }
            }
        }

        RenderBox _contentSection;


        public RenderBox actionsSection {
            get { return _actionsSection; }
            set {
                if (value != _actionsSection) {
                    if (null != _actionsSection) {
                        dropChild(_actionsSection);
                    }

                    _actionsSection = value;
                    if (null != _actionsSection) {
                        adoptChild(_actionsSection);
                    }
                }
            }
        }

        RenderBox _actionsSection;

        public bool isInAccessibilityMode {
            get { return _isInAccessibilityMode; }
            set {
                if (value != _isInAccessibilityMode) {
                    _isInAccessibilityMode = value;
                    markNeedsLayout();
                }
            }
        }

        bool _isInAccessibilityMode;

        float _dialogWidth {
            get {
                return isInAccessibilityMode
                    ? CupertinoDialogUtils._kAccessibilityCupertinoDialogWidth
                    : CupertinoDialogUtils._kCupertinoDialogWidth;
            }
        }

        readonly float _dividerThickness;


        public readonly Paint _dividerPaint;

        public Color dividerColor {
            get {
                return _dividerPaint.color;
            }
            set {
                if (dividerColor == value) {
                    return;
                }

                _dividerPaint.color = value;
                markNeedsPaint();
            }
        }
        public override void attach(object owner) {
            base.attach(owner);
            if (null != contentSection) {
                contentSection.attach(owner);
            }

            if (null != actionsSection) {
                actionsSection.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            if (null != contentSection) {
                contentSection.detach();
            }

            if (null != actionsSection) {
                actionsSection.detach();
            }
        }

        public override void redepthChildren() {
            if (null != contentSection) {
                redepthChild(contentSection);
            }

            if (null != actionsSection) {
                redepthChild(actionsSection);
            }
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is BoxParentData)) {
                child.parentData = new BoxParentData();
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            if (contentSection != null) {
                visitor(contentSection);
            }

            if (actionsSection != null) {
                visitor(actionsSection);
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            List<DiagnosticsNode> value = new List<DiagnosticsNode>();
            if (contentSection != null) {
                value.Add(contentSection.toDiagnosticsNode(name: "content"));
            }

            if (actionsSection != null) {
                value.Add(actionsSection.toDiagnosticsNode(name: "actions"));
            }

            return value;
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            return _dialogWidth;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            return _dialogWidth;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            float contentHeight = contentSection.getMinIntrinsicHeight(width);
            float actionsHeight = actionsSection.getMinIntrinsicHeight(width);
            bool hasDivider = contentHeight > 0.0f && actionsHeight > 0.0f;
            float height = contentHeight + (hasDivider ? _dividerThickness : 0.0f) + actionsHeight;
            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            float contentHeight = contentSection.getMaxIntrinsicHeight(width);
            float actionsHeight = actionsSection.getMaxIntrinsicHeight(width);
            bool hasDivider = contentHeight > 0.0f && actionsHeight > 0.0f;
            float height = contentHeight + (hasDivider ? _dividerThickness : 0.0f) + actionsHeight;
            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected override void performLayout() {
            if (isInAccessibilityMode) {
                performAccessibilityLayout();
            }
            else {
                performRegularLayout();
            }
        }

        void performRegularLayout() {
            bool hasDivider = contentSection.getMaxIntrinsicHeight(_dialogWidth) > 0.0f
                              && actionsSection.getMaxIntrinsicHeight(_dialogWidth) > 0.0f;
            float dividerThickness = hasDivider ? _dividerThickness : 0.0f;
            float minActionsHeight = actionsSection.getMinIntrinsicHeight(_dialogWidth);
            contentSection.layout(
                constraints.deflate(EdgeInsets.only(bottom: minActionsHeight + dividerThickness)),
                parentUsesSize: true
            );
            Size contentSize = contentSection.size;
            actionsSection.layout(
                constraints.deflate(EdgeInsets.only(top: contentSize.height + dividerThickness)),
                parentUsesSize: true
            );
            Size actionsSize = actionsSection.size;
            float dialogHeight = contentSize.height + dividerThickness + actionsSize.height;
            size = constraints.constrain(
                new Size(_dialogWidth, dialogHeight)
            );
            D.assert(actionsSection.parentData is BoxParentData);
            BoxParentData actionParentData = actionsSection.parentData as BoxParentData;
            actionParentData.offset = new Offset(0.0f, contentSize.height + dividerThickness);
        }

        void performAccessibilityLayout() {
            bool hasDivider = contentSection.getMaxIntrinsicHeight(_dialogWidth) > 0.0f
                              && actionsSection.getMaxIntrinsicHeight(_dialogWidth) > 0.0f;
            float dividerThickness = hasDivider ? _dividerThickness : 0.0f;
            float maxContentHeight = contentSection.getMaxIntrinsicHeight(_dialogWidth);
            float maxActionsHeight = actionsSection.getMaxIntrinsicHeight(_dialogWidth);
            Size contentSize;
            Size actionsSize;
            if (maxContentHeight + dividerThickness + maxActionsHeight > constraints.maxHeight) {
                actionsSection.layout(
                    constraints.deflate(EdgeInsets.only(top: constraints.maxHeight / 2.0f)),
                    parentUsesSize: true
                );
                actionsSize = actionsSection.size;
                contentSection.layout(
                    constraints.deflate(EdgeInsets.only(bottom: actionsSize.height + dividerThickness)),
                    parentUsesSize: true
                );
                contentSize = contentSection.size;
            }
            else {
                contentSection.layout(constraints,
                    parentUsesSize: true
                );
                contentSize = contentSection.size;
                actionsSection.layout(constraints.deflate(EdgeInsets.only(top: contentSize.height)),
                    parentUsesSize: true
                );
                actionsSize = actionsSection.size;
            }

            float dialogHeight = contentSize.height + dividerThickness + actionsSize.height;
            size = constraints.constrain(
                new Size(_dialogWidth, dialogHeight)
            );
            D.assert(actionsSection.parentData is BoxParentData);
            BoxParentData actionParentData = actionsSection.parentData as BoxParentData;
            actionParentData.offset = new Offset(0.0f, contentSize.height + dividerThickness);
        }

        public override void paint(PaintingContext context, Offset offset) {
            BoxParentData contentParentData = contentSection.parentData as BoxParentData;
            contentSection.paint(context, offset + contentParentData.offset);
            bool hasDivider = contentSection.size.height > 0.0f && actionsSection.size.height > 0.0f;
            if (hasDivider) {
                _paintDividerBetweenContentAndActions(context.canvas, offset);
            }

            BoxParentData actionsParentData = actionsSection.parentData as BoxParentData;
            actionsSection.paint(context, offset + actionsParentData.offset);
        }

        void _paintDividerBetweenContentAndActions(Canvas canvas, Offset offset) {
            canvas.drawRect(
                Rect.fromLTWH(
                    offset.dx,
                    offset.dy + contentSection.size.height, size.width, _dividerThickness
                ), _dividerPaint
            );
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            BoxParentData contentSectionParentData = contentSection.parentData as BoxParentData;
            BoxParentData actionsSectionParentData = actionsSection.parentData as BoxParentData;
            return result.addWithPaintOffset(
                offset: contentSectionParentData.offset,
                position: position,
                hitTest: (BoxHitTestResult resultIn, Offset transformed) => {
                    D.assert(transformed == position - contentSectionParentData.offset);
                    return contentSection.hitTest(resultIn, position: transformed);
                }
            ) || result.addWithPaintOffset(
                offset: actionsSectionParentData.offset,
                position: position,
                hitTest: (BoxHitTestResult resultIn, Offset transformed) => {
                    D.assert(transformed == position - actionsSectionParentData.offset);
                    return actionsSection.hitTest(resultIn, position: transformed);
                }
            );
        }
    }

    enum _AlertDialogSections {
        contentSection,
        actionsSection,
    }

    class _CupertinoDialogAlertContentSection : StatelessWidget {
        public _CupertinoDialogAlertContentSection(
            Key key = null,
            Widget title = null,
            Widget content = null,
            ScrollController scrollController = null
        ) : base(key: key) {
            this.title = title;
            this.content = content;
            this.scrollController = scrollController;
        }

        public readonly Widget title;
        public readonly Widget content;
        public readonly ScrollController scrollController;

        public override Widget build(BuildContext context) {
            if (title == null && content == null) {
                return new SingleChildScrollView(
                    controller: scrollController,
                    child: new Container(width: 0.0f, height: 0.0f)
                );
            }

            float textScaleFactor = MediaQuery.of(context).textScaleFactor;
            List<Widget> titleContentGroup = new List<Widget>();
            if (title != null) {
                titleContentGroup.Add(new Padding(
                    padding: EdgeInsets.only(
                        left: CupertinoDialogUtils._kEdgePadding,
                        right: CupertinoDialogUtils._kEdgePadding,
                        bottom: content == null ? CupertinoDialogUtils._kEdgePadding : 1.0f,
                        top: CupertinoDialogUtils._kEdgePadding * textScaleFactor
                    ),
                    child: new DefaultTextStyle(
                        style: CupertinoDialogUtils._kCupertinoDialogTitleStyle.copyWith(
                            color: CupertinoDynamicColor.resolve(CupertinoColors.label, context)
                        ),
                        textAlign: TextAlign.center,
                        child: title
                    )
                ));
            }

            if (content != null) {
                titleContentGroup.Add(
                    new Padding(
                        padding: EdgeInsets.only(
                            left: CupertinoDialogUtils._kEdgePadding,
                            right: CupertinoDialogUtils._kEdgePadding,
                            bottom: CupertinoDialogUtils._kEdgePadding * textScaleFactor,
                            top: title == null ? CupertinoDialogUtils._kEdgePadding : 1.0f
                        ),
                        child: new DefaultTextStyle(
                            style: CupertinoDialogUtils._kCupertinoDialogContentStyle.copyWith(
                                color: CupertinoDynamicColor.resolve(CupertinoColors.label, context)
                            ),
                            textAlign: TextAlign.center,
                            child: content
                        )
                    )
                );
            }

            if (titleContentGroup.isEmpty()) {
                return new SingleChildScrollView(
                    controller: scrollController,
                    child: new Container(width: 0.0f, height: 0.0f)
                );
            }

            return new CupertinoScrollbar(
                child: new SingleChildScrollView(
                    controller: scrollController,
                    child: new Column(
                        mainAxisSize: MainAxisSize.max,
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: titleContentGroup
                    )
                )
            );
        }
    }

    class _CupertinoDialogAlertActionSection : StatefulWidget {
        public _CupertinoDialogAlertActionSection(
            List<Widget> children,
            Key key = null,
            ScrollController scrollController = null
        ) : base(key: key) {
            D.assert(children != null);
            this.children = children;
            this.scrollController = scrollController;
        }

        public readonly List<Widget> children;
        public readonly ScrollController scrollController;

        public override State createState() {
            return new _CupertinoDialogAlertActionSectionState();
        }
    }

    class _CupertinoDialogAlertActionSectionState : State<_CupertinoDialogAlertActionSection> {
        public override Widget build(BuildContext context) {
            float devicePixelRatio = MediaQuery.of(context).devicePixelRatio;
            List<Widget> interactiveButtons = new List<Widget>();
            for (int i = 0; i < widget.children.Count; i += 1) {
                interactiveButtons.Add(
                    new _PressableDialogActionButton(
                        child: widget.children[i]
                    )
                );
            }

            return new CupertinoScrollbar(
                child: new SingleChildScrollView(
                    controller: widget.scrollController,
                    child: new _CupertinoDialogActionsRenderWidget(
                        actionButtons: interactiveButtons,
                        dividerThickness: CupertinoDialogUtils._kDividerThickness / devicePixelRatio
                    )
                )
            );
        }
    }

    class _PressableDialogActionButton : StatefulWidget {
        public _PressableDialogActionButton(
            Widget child
        ) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _PressableDialogActionButtonState();
        }
    }

    class _PressableDialogActionButtonState : State<_PressableDialogActionButton> {
        bool _isPressed = false;

        public override Widget build(BuildContext context) {
            return new _DialogActionButtonParentDataWidget(
                isPressed: _isPressed,
                child: new GestureDetector(
                    behavior: HitTestBehavior.opaque,
                    onTapDown: (TapDownDetails details) => setState(() => { _isPressed = true; }),
                    onTapUp: (TapUpDetails details) => setState(() => { _isPressed = false; }),
                    onTapCancel: () => setState(() => _isPressed = false),
                    child: widget.child
                )
            );
        }
    }

    class _DialogActionButtonParentDataWidget : ParentDataWidget<_ActionButtonParentData> {
        public _DialogActionButtonParentDataWidget(
            Key key = null,
            bool isPressed = false,
            Widget child = null
        ) : base(key: key, child: child) {
            this.isPressed = isPressed;
        }

        public readonly bool isPressed;

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is _ActionButtonParentData);
            _ActionButtonParentData parentData = renderObject.parentData as _ActionButtonParentData;
            if (parentData.isPressed != isPressed) {
                parentData.isPressed = isPressed;
                AbstractNodeMixinDiagnosticableTree targetParent = renderObject.parent;
                if (targetParent is RenderObject) {
                    ((RenderObject) targetParent).markNeedsPaint();
                }
            }
        }

        public override Type debugTypicalAncestorWidgetClass {
            get {
                return typeof(_CupertinoDialogActionsRenderWidget);
            }
        }
    }

    public class CupertinoDialogAction : StatelessWidget {
        public CupertinoDialogAction(
            Key key = null,
            VoidCallback onPressed = null,
            bool isDefaultAction = false,
            bool isDestructiveAction = false,
            TextStyle textStyle = null,
            Widget child = null
        ):base(key:key) {
            D.assert(child != null);
            this.onPressed = onPressed;
            this.isDefaultAction = isDefaultAction;
            this.isDestructiveAction = isDestructiveAction;
            this.textStyle = textStyle;
            this.child = child;
        }

        public readonly VoidCallback onPressed;
        public readonly bool isDefaultAction;
        public readonly bool isDestructiveAction;
        public readonly TextStyle textStyle;
        public readonly Widget child;

        public bool enabled {
            get { return onPressed != null; }
        }

        float _calculatePadding(BuildContext context) {
            return 8.0f * MediaQuery.textScaleFactorOf(context);
        }

        Widget _buildContentWithRegularSizingPolicy(
            BuildContext context,
            TextStyle textStyle,
            Widget content
        ) {
            bool isInAccessibilityMode = CupertinoDialogUtils._isInAccessibilityMode(context);
            float dialogWidth = isInAccessibilityMode
                ? CupertinoDialogUtils._kAccessibilityCupertinoDialogWidth
                : CupertinoDialogUtils._kCupertinoDialogWidth;
            float textScaleFactor = MediaQuery.textScaleFactorOf(context);
            float fontSizeRatio =
                (textScaleFactor * textStyle.fontSize) / CupertinoDialogUtils._kMinButtonFontSize ?? 0f;
            float padding = _calculatePadding(context);
            return new IntrinsicHeight(
                child: new SizedBox(
                    width: float.PositiveInfinity,
                    child: new FittedBox(
                        fit: BoxFit.scaleDown,
                        child: new ConstrainedBox(
                            constraints: new BoxConstraints(
                                maxWidth: fontSizeRatio * (dialogWidth - (2 * padding))
                            ),
                            child: new DefaultTextStyle(
                                style: textStyle,
                                textAlign: TextAlign.center,
                                overflow: TextOverflow.ellipsis,
                                maxLines: 1,
                                child: content
                            )
                        )
                    )
                )
            );
        }

        Widget _buildContentWithAccessibilitySizingPolicy(
            TextStyle textStyle,
            Widget content
        ) {
            return new DefaultTextStyle(
                style: textStyle,
                textAlign: TextAlign.center,
                child: content
            );
        }

        public override Widget build(BuildContext context) {
            TextStyle style = CupertinoDialogUtils._kCupertinoDialogActionStyle.copyWith(
                color: CupertinoDynamicColor.resolve(
                    isDestructiveAction ?  CupertinoColors.systemRed : CupertinoColors.systemBlue,
                    context
                )
            );
            style = style.merge(textStyle);
            
            if (isDefaultAction) {
                style = style.copyWith(fontWeight: FontWeight.w600);
            }


            if (!enabled) {
                style = style.copyWith(color: style.color.withOpacity(0.5f));
            }

            Widget sizedContent = CupertinoDialogUtils._isInAccessibilityMode(context)
                ? _buildContentWithAccessibilitySizingPolicy(
                    textStyle: style,
                    content: child
                )
                : _buildContentWithRegularSizingPolicy(
                    context: context,
                    textStyle: style,
                    content: child
                );
            return new GestureDetector(
                onTap: () => onPressed(),
                behavior: HitTestBehavior.opaque,
                child: new ConstrainedBox(
                    constraints: new BoxConstraints(
                        minHeight: CupertinoDialogUtils._kMinButtonHeight
                    ),
                    child: new Container(
                        alignment: Alignment.center,
                        padding: EdgeInsets.all(_calculatePadding(context)),
                        child: sizedContent
                    )
                )
            );
        }
    }

    class _CupertinoDialogActionsRenderWidget : MultiChildRenderObjectWidget {
        public _CupertinoDialogActionsRenderWidget(
            List<Widget> actionButtons,
            Key key = null,
            float dividerThickness = 0.0f
        ) : base(key: key, children: actionButtons) {
            _dividerThickness = dividerThickness;
        }

        public readonly float _dividerThickness;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoDialogActions(
                dialogWidth: CupertinoDialogUtils._isInAccessibilityMode(context)
                    ? CupertinoDialogUtils._kAccessibilityCupertinoDialogWidth
                    : CupertinoDialogUtils._kCupertinoDialogWidth,
                dividerThickness: _dividerThickness,
                dialogColor: CupertinoDynamicColor.resolve(CupertinoDialogUtils._kDialogColor, context),
                dialogPressedColor: CupertinoDynamicColor.resolve(CupertinoDialogUtils._kDialogPressedColor, context),
                dividerColor: CupertinoDynamicColor.resolve(CupertinoColors.separator, context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            (renderObject as _RenderCupertinoDialogActions).dialogWidth =
                CupertinoDialogUtils._isInAccessibilityMode(context)
                    ? CupertinoDialogUtils._kAccessibilityCupertinoDialogWidth
                    : CupertinoDialogUtils._kCupertinoDialogWidth;
            (renderObject as _RenderCupertinoDialogActions).dividerThickness = _dividerThickness;
            (renderObject as _RenderCupertinoDialogActions).dialogColor =
                CupertinoDynamicColor.resolve( CupertinoDialogUtils._kDialogColor, context);
            (renderObject as _RenderCupertinoDialogActions).dialogPressedColor =
                CupertinoDynamicColor.resolve( CupertinoDialogUtils._kDialogPressedColor, context);
            (renderObject as _RenderCupertinoDialogActions).dividerColor = 
                CupertinoDynamicColor.resolve(CupertinoColors.separator, context);
        }
    }

    class _RenderCupertinoDialogActions : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<
        RenderBox, MultiChildLayoutParentData> {
        public _RenderCupertinoDialogActions(
            List<RenderBox> children = null,
            float? dialogWidth = null,
            float dividerThickness = 0.0f,
            Color dialogColor = null,
            Color dialogPressedColor = null,
            Color dividerColor = null
        ) {
            _dialogWidth = dialogWidth;
            _buttonBackgroundPaint = new Paint() {
                color = dialogColor,
                style = PaintingStyle.fill
            };
            _pressedButtonBackgroundPaint = new Paint(){
                color = dialogPressedColor,
                style = PaintingStyle.fill
            };
            _dividerPaint = new Paint(){
                color = dividerColor,
                style = PaintingStyle.fill
            };
            _dividerThickness = dividerThickness;
            addAll(children);
        }

        public float? dialogWidth {
            get { return _dialogWidth; }
            set {
                if (value != _dialogWidth) {
                    _dialogWidth = value;
                    markNeedsLayout();
                }
            }
        }
        float? _dialogWidth;


        public float dividerThickness {
            get { return _dividerThickness; }
            set {
                if (value != _dividerThickness) {
                    _dividerThickness = value;
                    markNeedsLayout();
                }
            }
        }
        float _dividerThickness;

        public readonly  Paint _buttonBackgroundPaint;

        public Color dialogColor {
            set{
                if (value == _buttonBackgroundPaint.color)
                    return;

                _buttonBackgroundPaint.color = value;
                markNeedsPaint();
            }
         }

        public readonly Paint _pressedButtonBackgroundPaint;

        public Color dialogPressedColor {
            set{
                if (value == _pressedButtonBackgroundPaint.color)
                    return;

                _pressedButtonBackgroundPaint.color = value;
                markNeedsPaint();
            }
        }

        public readonly Paint _dividerPaint;

        public Color dividerColor{
            set {
                if (value == _dividerPaint.color)
                    return;

                _dividerPaint.color = value;
                markNeedsPaint();
            }
        }

        List<RenderBox> _pressedButtons {
            get {
                List<RenderBox> childList = new List<RenderBox>();

                RenderBox currentChild = firstChild;
                while (currentChild != null) {
                    D.assert(currentChild.parentData is _ActionButtonParentData);
                    _ActionButtonParentData parentData = currentChild.parentData as _ActionButtonParentData;
                    if (parentData.isPressed) {
                        childList.Add(currentChild);
                    }

                    currentChild = childAfter(currentChild);
                }

                return childList;
            }
        }

        bool _isButtonPressed {
            get {
                RenderBox currentChild = firstChild;
                while (currentChild != null) {
                    D.assert(currentChild.parentData is _ActionButtonParentData);
                    _ActionButtonParentData parentData = currentChild.parentData as _ActionButtonParentData;
                    if (parentData.isPressed) {
                        return true;
                    }

                    currentChild = childAfter(currentChild);
                }

                return false;
            }
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is _ActionButtonParentData)) {
                child.parentData = new _ActionButtonParentData();
            }
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            return dialogWidth ?? 0.0f;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            return dialogWidth ?? 0.0f;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            float minHeight;
            if (childCount == 0) {
                minHeight = 0.0f;
            }
            else if (childCount == 1) {
                minHeight = _computeMinIntrinsicHeightSideBySide(width);
            }
            else {
                if (childCount == 2 && _isSingleButtonRow(width)) {
                    minHeight = _computeMinIntrinsicHeightSideBySide(width);
                }
                else {
                    minHeight = _computeMinIntrinsicHeightStacked(width);
                }
            }

            return minHeight;
        }

        float _computeMinIntrinsicHeightSideBySide(float width) {
            D.assert(childCount >= 1 && childCount <= 2);
            float minHeight;
            if (childCount == 1) {
                minHeight = firstChild.getMinIntrinsicHeight(width);
            }
            else {
                float perButtonWidth = (width - dividerThickness) / 2.0f;
                minHeight = Mathf.Max(
                    firstChild.getMinIntrinsicHeight(perButtonWidth),
                    lastChild.getMinIntrinsicHeight(perButtonWidth)
                );
            }

            return minHeight;
        }

        float _computeMinIntrinsicHeightStacked(float width) {
            D.assert(childCount >= 2);
            return firstChild.getMinIntrinsicHeight(width)
                   + dividerThickness
                   + (0.5f * childAfter(firstChild).getMinIntrinsicHeight(width));
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            float maxHeight;
            if (childCount == 0) {
                maxHeight = 0.0f;
            }
            else if (childCount == 1) {
                maxHeight = firstChild.getMaxIntrinsicHeight(width);
            }
            else if (childCount == 2) {
                if (_isSingleButtonRow(width)) {
                    float perButtonWidth = (width - dividerThickness) / 2.0f;
                    maxHeight = Mathf.Max(
                        firstChild.getMaxIntrinsicHeight(perButtonWidth),
                        lastChild.getMaxIntrinsicHeight(perButtonWidth)
                    );
                }
                else {
                    maxHeight = _computeMaxIntrinsicHeightStacked(width);
                }
            }
            else {
                maxHeight = _computeMaxIntrinsicHeightStacked(width);
            }

            return maxHeight;
        }

        float _computeMaxIntrinsicHeightStacked(float width) {
            D.assert(childCount >= 2);
            float allDividersHeight = (childCount - 1) * dividerThickness;
            float heightAccumulation = allDividersHeight;
            RenderBox button = firstChild;
            while (button != null) {
                heightAccumulation += button.getMaxIntrinsicHeight(width);
                button = childAfter(button);
            }
            return heightAccumulation;
        }

        bool _isSingleButtonRow(float width) {
            bool isSingleButtonRow;
            if (childCount == 1) {
                isSingleButtonRow = true;
            }
            else if (childCount == 2) {
                float sideBySideWidth = firstChild.getMaxIntrinsicWidth(float.PositiveInfinity)
                                        + dividerThickness
                                        + lastChild.getMaxIntrinsicWidth(float.PositiveInfinity);
                isSingleButtonRow = sideBySideWidth <= width;
            }
            else {
                isSingleButtonRow = false;
            }

            return isSingleButtonRow;
        }

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            if (_isSingleButtonRow(dialogWidth ?? 0.0f)) {
                if (childCount == 1) {
                    firstChild.layout(
                        constraints,
                        parentUsesSize: true
                    );
                    size = constraints.constrain(
                        new Size(dialogWidth ?? 0.0f, firstChild.size.height)
                    );
                }
                else {
                    BoxConstraints perButtonnewraints = new BoxConstraints(
                        minWidth: (constraints.minWidth - dividerThickness) / 2.0f,
                        maxWidth: (constraints.maxWidth - dividerThickness) / 2.0f,
                        minHeight: 0.0f,
                        maxHeight: float.PositiveInfinity
                    );
                    firstChild.layout(
                        perButtonnewraints,
                        parentUsesSize: true
                    );
                    lastChild.layout(
                        perButtonnewraints,
                        parentUsesSize: true
                    );
                    D.assert(lastChild.parentData is MultiChildLayoutParentData);
                    MultiChildLayoutParentData secondButtonParentData =
                        lastChild.parentData as MultiChildLayoutParentData;
                    secondButtonParentData.offset =
                        new Offset(firstChild.size.width + dividerThickness, 0.0f);
                    size = constraints.constrain(
                        new Size(
                            dialogWidth ?? 0.0f,
                            Mathf.Max(
                                firstChild.size.height, 
                                lastChild.size.height
                            )
                        )
                    );
                }
            }
            else {
                BoxConstraints perButtonnewraints = constraints.copyWith(
                    minHeight: 0.0f,
                    maxHeight: float.PositiveInfinity
                );
                RenderBox child = firstChild;
                int index = 0;
                float verticalOffset = 0.0f;
                while (child != null) {
                    child.layout(
                        perButtonnewraints,
                        parentUsesSize: true
                    );
                    D.assert(child.parentData is MultiChildLayoutParentData);
                    MultiChildLayoutParentData parentData = child.parentData as MultiChildLayoutParentData;
                    parentData.offset = new Offset(0.0f, verticalOffset);
                    verticalOffset += child.size.height;
                    if (index < childCount - 1) {
                        verticalOffset += dividerThickness;
                    }

                    index += 1;
                    child = childAfter(child);
                }

                size = constraints.constrain(
                    new Size(dialogWidth ?? 0.0f, verticalOffset)
                );
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;
            if (_isSingleButtonRow(size.width)) {
                _drawButtonBackgroundsAndDividersSingleRow(canvas, offset);
            }
            else {
                _drawButtonBackgroundsAndDividersStacked(canvas, offset);
            }

            _drawButtons(context, offset);
        }

        void _drawButtonBackgroundsAndDividersSingleRow(Canvas canvas, Offset offset) {
            Rect verticalDivider = childCount == 2 && !_isButtonPressed
                ? Rect.fromLTWH(
                    offset.dx + firstChild.size.width,
                    offset.dy, dividerThickness,
                    Mathf.Max(firstChild.size.height, lastChild.size.height
                    )
                )
                : Rect.zero;
            List<Rect> pressedButtonRects = new List<Rect>();

            foreach (var item in _pressedButtons) {
                MultiChildLayoutParentData buttonParentData = item.parentData as MultiChildLayoutParentData;
                pressedButtonRects.Add(
                    Rect.fromLTWH(
                        offset.dx + buttonParentData.offset.dx,
                        offset.dy + buttonParentData.offset.dy,
                        item.size.width,
                        item.size.height
                    ));
            }

            Path backgroundFillPath = new Path();
            backgroundFillPath.fillType = PathFillType.evenOdd;
            backgroundFillPath.addRect(Rect.fromLTWH(0.0f, 0.0f, size.width, size.height));
            backgroundFillPath.addRect(verticalDivider);

            for (int i = 0; i < pressedButtonRects.Count; i += 1) {
                backgroundFillPath.addRect(pressedButtonRects[i]);
            }

            canvas.drawPath(
                backgroundFillPath, _buttonBackgroundPaint
            );
            Path pressedBackgroundFillPath = new Path();
            for (int i = 0; i < pressedButtonRects.Count; i += 1) {
                pressedBackgroundFillPath.addRect(pressedButtonRects[i]);
            }

            canvas.drawPath(
                pressedBackgroundFillPath, 
                _pressedButtonBackgroundPaint
            );
            Path dividersPath = new Path();
            dividersPath.addRect(verticalDivider);
            canvas.drawPath(
                dividersPath, _dividerPaint
            );
        }

        void _drawButtonBackgroundsAndDividersStacked(Canvas canvas, Offset offset) {
            Offset dividerOffset = new Offset(0.0f, dividerThickness);
            Path backgroundFillPath = new Path();
            backgroundFillPath.fillType = PathFillType.evenOdd;
            backgroundFillPath.addRect(Rect.fromLTWH(0.0f, 0.0f, size.width, size.height));
            Path pressedBackgroundFillPath = new Path();
            Path dividersPath = new Path();
            Offset accumulatingOffset = offset;
            RenderBox child = firstChild;
            RenderBox prevChild = null;

            while (child != null) {
                D.assert(child.parentData is _ActionButtonParentData);
                _ActionButtonParentData currentButtonParentData =
                    child.parentData as _ActionButtonParentData;
                bool isButtonPressed = currentButtonParentData.isPressed;
                bool isPrevButtonPressed = false;
                if (prevChild != null) {
                    D.assert(prevChild.parentData is _ActionButtonParentData);
                    _ActionButtonParentData previousButtonParentData =
                        prevChild.parentData as _ActionButtonParentData;
                    isPrevButtonPressed = previousButtonParentData.isPressed;
                }

                bool isDividerPresent = child != firstChild;
                bool isDividerPainted = isDividerPresent && !(isButtonPressed || isPrevButtonPressed);
                Rect dividerRect = Rect.fromLTWH(
                    accumulatingOffset.dx,
                    accumulatingOffset.dy, size.width, dividerThickness
                );
                Rect buttonBackgroundRect = Rect.fromLTWH(
                    accumulatingOffset.dx,
                    accumulatingOffset.dy + (isDividerPresent ? dividerThickness : 0.0f), size.width,
                    child.size.height
                );
                if (isButtonPressed) {
                    backgroundFillPath.addRect(buttonBackgroundRect);
                    pressedBackgroundFillPath.addRect(buttonBackgroundRect);
                }

                if (isDividerPainted) {
                    backgroundFillPath.addRect(dividerRect);
                    dividersPath.addRect(dividerRect);
                }

                accumulatingOffset += (isDividerPresent ? dividerOffset : Offset.zero)
                                      + new Offset(0.0f, child.size.height);
                prevChild = child;
                child = childAfter(child);
            }

            canvas.drawPath(backgroundFillPath, _buttonBackgroundPaint);
            canvas.drawPath(pressedBackgroundFillPath, _pressedButtonBackgroundPaint);
            canvas.drawPath(dividersPath, _dividerPaint);
        }

        void _drawButtons(PaintingContext context, Offset offset) {
            RenderBox child = firstChild;
            while (child != null) {
                MultiChildLayoutParentData childParentData = child.parentData as MultiChildLayoutParentData;
                context.paintChild(child, childParentData.offset + offset);
                child = childAfter(child);
            }
        }

        protected override bool hitTestChildren(
            BoxHitTestResult result,
            Offset position = null
        ) {
            return defaultHitTestChildren(result, position: position);
        }
    }
}