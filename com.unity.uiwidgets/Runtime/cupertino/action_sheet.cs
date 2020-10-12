using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    class CupertinoActionSheetUtils {
        public static readonly TextStyle _kActionSheetActionStyle = new TextStyle(
            // fontFamily: ".SF UI Text",
            fontFamily: ".SF Pro Text",
            inherit: false,
            fontSize: 20.0f,
            fontWeight: FontWeight.w400,
            color: CupertinoColors.activeBlue,
            textBaseline: TextBaseline.alphabetic
        );

        public static readonly TextStyle _kActionSheetContentStyle = new TextStyle(
            // fontFamily: ".SF UI Text",
            fontFamily: ".SF Pro Text",
            inherit: false,
            fontSize: 13.0f,
            fontWeight: FontWeight.w400,
            color: _kContentTextColor,
            textBaseline: TextBaseline.alphabetic
        );

        public static readonly BoxDecoration _kAlertBlurOverlayDecoration = new BoxDecoration(
            color: CupertinoColors.white,
            backgroundBlendMode: BlendMode.overlay
        );

        public static readonly Color _kBackgroundColor = new Color(0xD1F8F8F8);
        public static readonly Color _kPressedColor = new Color(0xA6E5E5EA);
        public static readonly Color _kButtonDividerColor = new Color(0x403F3F3F);
        public static readonly Color _kContentTextColor = new Color(0xFF8F8F8F);
        public static readonly Color _kCancelButtonPressedColor = new Color(0xFFEAEAEA);

        public const float _kBlurAmount = 20.0f;
        public const float _kEdgeHorizontalPadding = 8.0f;
        public const float _kCancelButtonPadding = 8.0f;
        public const float _kEdgeVerticalPadding = 10.0f;
        public const float _kContentHorizontalPadding = 40.0f;
        public const float _kContentVerticalPadding = 14.0f;
        public const float _kButtonHeight = 56.0f;
        public const float _kCornerRadius = 14.0f;
        public const float _kDividerThickness = 1.0f;
    }

    public class CupertinoActionSheet : StatelessWidget {
        public CupertinoActionSheet(
            Key key = null,
            Widget title = null,
            Widget message = null,
            List<Widget> actions = null,
            ScrollController messageScrollController = null,
            ScrollController actionScrollController = null,
            Widget cancelButton = null
        ) : base(key: key) {
            D.assert(actions != null || title != null || message != null || cancelButton != null,
                () =>
                    "An action sheet must have a non-null value for at least one of the following arguments: actions, title, message, or cancelButton");
            this.title = title;
            this.message = message;
            this.actions = actions ?? new List<Widget>();
            this.messageScrollController = messageScrollController;
            this.actionScrollController = actionScrollController;
            this.cancelButton = cancelButton;
        }

        public readonly Widget title;
        public readonly Widget message;
        public readonly List<Widget> actions;
        public readonly ScrollController messageScrollController;
        public readonly ScrollController actionScrollController;
        public readonly Widget cancelButton;

        Widget _buildContent() {
            List<Widget> content = new List<Widget>();
            if (title != null || message != null) {
                Widget titleSection = new _CupertinoAlertContentSection(
                    title: title,
                    message: message,
                    scrollController: messageScrollController
                );
                content.Add(new Flexible(child: titleSection));
            }

            return new Container(
                color: CupertinoActionSheetUtils._kBackgroundColor,
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: content
                )
            );
        }

        Widget _buildActions() {
            if (actions == null || actions.isEmpty()) {
                return new Container(height: 0.0f);
            }

            return new Container(
                child: new _CupertinoAlertActionSection(
                    children: actions,
                    scrollController: actionScrollController,
                    hasCancelButton: cancelButton != null
                )
            );
        }

        Widget _buildCancelButton() {
            float cancelPadding = (actions != null || message != null || title != null)
                ? CupertinoActionSheetUtils._kCancelButtonPadding
                : 0.0f;
            return new Padding(
                padding: EdgeInsets.only(top: cancelPadding),
                child: new _CupertinoActionSheetCancelButton(
                    child: cancelButton
                )
            );
        }

        public override Widget build(BuildContext context) {
            List<Widget> children = new List<Widget> {
                new Flexible(child: new ClipRRect(
                        borderRadius: BorderRadius.circular(12.0f),
                        child: new BackdropFilter(
                            filter: ImageFilter.blur(sigmaX: CupertinoActionSheetUtils._kBlurAmount,
                                sigmaY: CupertinoActionSheetUtils._kBlurAmount),
                            child: new Container(
                                decoration: CupertinoActionSheetUtils._kAlertBlurOverlayDecoration,
                                child: new _CupertinoAlertRenderWidget(
                                    contentSection: _buildContent(),
                                    actionsSection: _buildActions()
                                )
                            )
                        )
                    )
                ),
            };

            if (cancelButton != null) {
                children.Add(_buildCancelButton()
                );
            }

            Orientation orientation = MediaQuery.of(context).orientation;

            float actionSheetWidth;
            if (orientation == Orientation.portrait) {
                actionSheetWidth = MediaQuery.of(context).size.width -
                                   (CupertinoActionSheetUtils._kEdgeHorizontalPadding * 2);
            }
            else {
                actionSheetWidth = MediaQuery.of(context).size.height -
                                   (CupertinoActionSheetUtils._kEdgeHorizontalPadding * 2);
            }

            return new SafeArea(
                child: new Container(
                    width: actionSheetWidth,
                    margin: EdgeInsets.symmetric(
                        horizontal: CupertinoActionSheetUtils._kEdgeHorizontalPadding,
                        vertical: CupertinoActionSheetUtils._kEdgeVerticalPadding
                    ),
                    child: new Column(
                        children: children,
                        mainAxisSize: MainAxisSize.min,
                        crossAxisAlignment: CrossAxisAlignment.stretch
                    )
                )
            );
        }
    }


    public class CupertinoActionSheetAction : StatelessWidget {
        public CupertinoActionSheetAction(
            Widget child,
            VoidCallback onPressed,
            bool isDefaultAction = false,
            bool isDestructiveAction = false
        ) {
            D.assert(child != null);
            D.assert(onPressed != null);
            this.child = child;
            this.onPressed = onPressed;
            this.isDefaultAction = isDefaultAction;
            this.isDestructiveAction = isDestructiveAction;
        }

        public readonly VoidCallback onPressed;
        public readonly bool isDefaultAction;
        public readonly bool isDestructiveAction;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            TextStyle style = CupertinoActionSheetUtils._kActionSheetActionStyle;

            if (isDefaultAction) {
                style = style.copyWith(fontWeight: FontWeight.w600);
            }

            if (isDestructiveAction) {
                style = style.copyWith(color: CupertinoColors.destructiveRed);
            }

            return new GestureDetector(
                onTap: () => onPressed(),
                behavior: HitTestBehavior.opaque,
                child: new ConstrainedBox(
                    constraints: new BoxConstraints(
                        minHeight: CupertinoActionSheetUtils._kButtonHeight
                    ),
                    child: new Container(
                        alignment: Alignment.center,
                        padding: EdgeInsets.symmetric(
                            vertical: 16.0f,
                            horizontal: 10.0f
                        ),
                        child: new DefaultTextStyle(
                            style: style,
                            child: child,
                            textAlign: TextAlign.center
                        )
                    )
                )
            );
        }
    }

    class _CupertinoActionSheetCancelButton : StatefulWidget {
        public _CupertinoActionSheetCancelButton(
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _CupertinoActionSheetCancelButtonState();
        }
    }

    class _CupertinoActionSheetCancelButtonState : State<_CupertinoActionSheetCancelButton> {
        Color _backgroundColor;

        public override void initState() {
            _backgroundColor = CupertinoColors.white;
            base.initState();
        }

        void _onTapDown(TapDownDetails evt) {
            setState(() => { _backgroundColor = CupertinoActionSheetUtils._kCancelButtonPressedColor; });
        }

        void _onTapUp(TapUpDetails evt) {
            setState(() => { _backgroundColor = CupertinoColors.white; });
        }

        void _onTapCancel() {
            setState(() => { _backgroundColor = CupertinoColors.white; });
        }

        public override Widget build(BuildContext context) {
            return new GestureDetector(
                onTapDown: _onTapDown,
                onTapUp: _onTapUp,
                onTapCancel: _onTapCancel,
                child: new Container(
                    decoration: new BoxDecoration(
                        color: _backgroundColor,
                        borderRadius: BorderRadius.circular(CupertinoActionSheetUtils._kCornerRadius)
                    ),
                    child: widget.child
                )
            );
        }
    }

    class _CupertinoAlertRenderWidget : RenderObjectWidget {
        public _CupertinoAlertRenderWidget(
            Widget contentSection,
            Widget actionsSection,
            Key key = null
        ) : base(key: key) {
            this.contentSection = contentSection;
            this.actionsSection = actionsSection;
        }

        public readonly Widget contentSection;
        public readonly Widget actionsSection;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoAlert(
                dividerThickness: CupertinoActionSheetUtils._kDividerThickness /
                                  MediaQuery.of(context).devicePixelRatio
            );
        }

        public override Element createElement() {
            return new _CupertinoAlertRenderElement(this);
        }
    }

    class _CupertinoAlertRenderElement : RenderObjectElement {
        public _CupertinoAlertRenderElement(_CupertinoAlertRenderWidget widget) : base(widget) {
        }

        Element _contentElement;
        Element _actionsElement;

        public new _CupertinoAlertRenderWidget widget {
            get { return base.widget as _CupertinoAlertRenderWidget; }
        }

        public new _RenderCupertinoAlert renderObject {
            get { return base.renderObject as _RenderCupertinoAlert; }
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
                _AlertSections.contentSection);
            _actionsElement = updateChild(_actionsElement, widget.actionsSection,
                _AlertSections.actionsSection);
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            _placeChildInSlot(child, slot);
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            _placeChildInSlot(child, slot);
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            _contentElement = updateChild(_contentElement, widget.contentSection,
                _AlertSections.contentSection);
            _actionsElement = updateChild(_actionsElement, widget.actionsSection,
                _AlertSections.actionsSection);
        }

        protected override void forgetChild(Element child) {
            D.assert(child == _contentElement || child == _actionsElement);
            if (_contentElement == child) {
                _contentElement = null;
            }
            else if (_actionsElement == child) {
                _actionsElement = null;
            }
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(child == renderObject.contentSection || child == renderObject.actionsSection);
            if (renderObject.contentSection == child) {
                renderObject.contentSection = null;
            }
            else if (renderObject.actionsSection == child) {
                renderObject.actionsSection = null;
            }
        }

        void _placeChildInSlot(RenderObject child, object slot) {
            switch ((_AlertSections) slot) {
                case _AlertSections.contentSection:
                    renderObject.contentSection = child as RenderBox;
                    break;
                case _AlertSections.actionsSection:
                    renderObject.actionsSection = child as RenderBox;
                    ;
                    break;
            }
        }
    }


    class _RenderCupertinoAlert : RenderBox {
        public _RenderCupertinoAlert(
            RenderBox contentSection = null,
            RenderBox actionsSection = null,
            float dividerThickness = 0.0f
        ) {
            _contentSection = contentSection;
            _actionsSection = actionsSection;
            _dividerThickness = dividerThickness;
        }

        public RenderBox contentSection {
            get { return _contentSection; }
            set {
                if (value != _contentSection) {
                    if (null != _contentSection) {
                        dropChild(_contentSection);
                    }

                    _contentSection = value;
                    if (null != _contentSection) {
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

        readonly float _dividerThickness;

        readonly Paint _dividerPaint = new Paint() {
            color = CupertinoActionSheetUtils._kButtonDividerColor,
            style = PaintingStyle.fill
        };

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
            if (!(child.parentData is MultiChildLayoutParentData)) {
                child.parentData = new MultiChildLayoutParentData();
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

        protected override float computeMinIntrinsicWidth(float height) {
            return constraints.minWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return constraints.maxWidth;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            float contentHeight = contentSection.getMinIntrinsicHeight(width);
            float actionsHeight = actionsSection.getMinIntrinsicHeight(width);
            bool hasDivider = contentHeight > 0.0f && actionsHeight > 0.0f;
            float height = contentHeight + (hasDivider ? _dividerThickness : 0.0f) + actionsHeight;

            if (actionsHeight > 0 || contentHeight > 0) {
                height -= 2 * CupertinoActionSheetUtils._kEdgeVerticalPadding;
            }

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

            if (actionsHeight > 0 || contentHeight > 0) {
                height -= 2 * CupertinoActionSheetUtils._kEdgeVerticalPadding;
            }

            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected override void performLayout() {
            bool hasDivider = contentSection.getMaxIntrinsicHeight(constraints.maxWidth) > 0.0f
                              && actionsSection.getMaxIntrinsicHeight(constraints.maxWidth) > 0.0f;
            float dividerThickness = hasDivider ? _dividerThickness : 0.0f;

            float minActionsHeight = actionsSection.getMinIntrinsicHeight(constraints.maxWidth);


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


            float actionSheetHeight = contentSize.height + dividerThickness + actionsSize.height;


            size = new Size(constraints.maxWidth, actionSheetHeight);


            D.assert(actionsSection.parentData is MultiChildLayoutParentData);
            MultiChildLayoutParentData actionParentData = actionsSection.parentData as MultiChildLayoutParentData;
            actionParentData.offset = new Offset(0.0f, contentSize.height + dividerThickness);
        }

        public override void paint(PaintingContext context, Offset offset) {
            MultiChildLayoutParentData contentParentData = contentSection.parentData as MultiChildLayoutParentData;
            contentSection.paint(context, offset + contentParentData.offset);

            bool hasDivider = contentSection.size.height > 0.0f && actionsSection.size.height > 0.0f;
            if (hasDivider) {
                _paintDividerBetweenContentAndActions(context.canvas, offset);
            }

            MultiChildLayoutParentData actionsParentData = actionsSection.parentData as MultiChildLayoutParentData;
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
            MultiChildLayoutParentData contentSectionParentData =
                contentSection.parentData as MultiChildLayoutParentData;
            MultiChildLayoutParentData actionsSectionParentData =
                actionsSection.parentData as MultiChildLayoutParentData;
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


    enum _AlertSections {
        contentSection,
        actionsSection,
    }


    class _CupertinoAlertContentSection : StatelessWidget {
        public _CupertinoAlertContentSection(
            Key key = null,
            Widget title = null,
            Widget message = null,
            ScrollController scrollController = null
        ) : base(key: key) {
            this.title = title;
            this.message = message;
            this.scrollController = scrollController;
        }

        public readonly Widget title;
        public readonly Widget message;
        public readonly ScrollController scrollController;

        public override Widget build(BuildContext context) {
            List<Widget> titleContentGroup = new List<Widget>();
            if (title != null) {
                titleContentGroup.Add(new Padding(
                    padding: EdgeInsets.only(
                        left: CupertinoActionSheetUtils._kContentHorizontalPadding,
                        right: CupertinoActionSheetUtils._kContentHorizontalPadding,
                        bottom: CupertinoActionSheetUtils._kContentVerticalPadding,
                        top: CupertinoActionSheetUtils._kContentVerticalPadding
                    ),
                    child: new DefaultTextStyle(
                        style: message == null
                            ? CupertinoActionSheetUtils._kActionSheetContentStyle
                            : CupertinoActionSheetUtils._kActionSheetContentStyle.copyWith(fontWeight: FontWeight.w600),
                        textAlign: TextAlign.center,
                        child: title
                    )
                ));
            }

            if (message != null) {
                titleContentGroup.Add(
                    new Padding(
                        padding: EdgeInsets.only(
                            left: CupertinoActionSheetUtils._kContentHorizontalPadding,
                            right: CupertinoActionSheetUtils._kContentHorizontalPadding,
                            bottom: title == null ? CupertinoActionSheetUtils._kContentVerticalPadding : 22.0f,
                            top: title == null ? CupertinoActionSheetUtils._kContentVerticalPadding : 0.0f
                        ),
                        child: new DefaultTextStyle(
                            style: title == null
                                ? CupertinoActionSheetUtils._kActionSheetContentStyle.copyWith(
                                    fontWeight: FontWeight.w600)
                                : CupertinoActionSheetUtils._kActionSheetContentStyle,
                            textAlign: TextAlign.center,
                            child: message
                        )
                    )
                );
            }

            if (titleContentGroup.isEmpty()) {
                return new SingleChildScrollView(
                    controller: scrollController,
                    child: new Container(
                        width: 0.0f,
                        height: 0.0f
                    )
                );
            }


            if (titleContentGroup.Count > 1) {
                titleContentGroup.Insert(1, new Padding(padding: EdgeInsets.only(top: 8.0f)));
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


    class _CupertinoAlertActionSection : StatefulWidget {
        public _CupertinoAlertActionSection(
            List<Widget> children,
            Key key = null,
            ScrollController scrollController = null,
            bool hasCancelButton = false
        ) : base(key: key) {
            D.assert(children != null);
            this.children = children;
            this.scrollController = scrollController;
            this.hasCancelButton = hasCancelButton;
        }

        public readonly List<Widget> children;
        public readonly ScrollController scrollController;
        public readonly bool hasCancelButton;

        public override State createState() {
            return new _CupertinoAlertActionSectionState();
        }
    }

    class _CupertinoAlertActionSectionState : State<_CupertinoAlertActionSection> {
        public override Widget build(BuildContext context) {
            float devicePixelRatio = MediaQuery.of(context).devicePixelRatio;

            List<Widget> interactiveButtons = new List<Widget>();
            for (int i = 0; i < widget.children.Count; i += 1) {
                interactiveButtons.Add(new _PressableActionSheetActionButton(
                        child: widget.children[i]
                    )
                );
            }

            return new CupertinoScrollbar(
                child: new SingleChildScrollView(
                    controller: widget.scrollController,
                    child: new _CupertinoAlertActionsRenderWidget(
                        actionButtons: interactiveButtons,
                        dividerThickness: CupertinoActionSheetUtils._kDividerThickness / devicePixelRatio,
                        hasCancelButton: widget.hasCancelButton
                    )
                )
            );
        }
    }

    class _PressableActionSheetActionButton : StatefulWidget {
        public _PressableActionSheetActionButton(
            Widget child
        ) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _PressableActionSheetActionButtonState();
        }
    }

    class _PressableActionSheetActionButtonState : State<_PressableActionSheetActionButton> {
        bool _isPressed = false;

        public override Widget build(BuildContext context) {
            return new _ActionSheetActionButtonParentDataWidget(
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

    class _ActionSheetActionButtonParentDataWidget : ParentDataWidget<_CupertinoAlertActionsRenderWidget> {
        public _ActionSheetActionButtonParentDataWidget(
            Widget child,
            bool isPressed = false,
            Key key = null
        ) : base(key: key, child: child) {
            this.isPressed = isPressed;
        }

        public readonly bool isPressed;

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is _ActionSheetActionButtonParentData);
            _ActionSheetActionButtonParentData parentData =
                renderObject.parentData as _ActionSheetActionButtonParentData;
            if (parentData.isPressed != isPressed) {
                parentData.isPressed = isPressed;
                AbstractNodeMixinDiagnosticableTree targetParent = renderObject.parent;
                if (targetParent is RenderObject) {
                    ((RenderObject) targetParent).markNeedsPaint();
                }
            }
        }
    }

    class _ActionSheetActionButtonParentData : MultiChildLayoutParentData {
        public _ActionSheetActionButtonParentData(
            bool isPressed = false
        ) {
            this.isPressed = isPressed;
        }

        public bool isPressed;
    }

    class _CupertinoAlertActionsRenderWidget : MultiChildRenderObjectWidget {
        public _CupertinoAlertActionsRenderWidget(
            List<Widget> actionButtons,
            Key key = null,
            float dividerThickness = 0.0f,
            bool hasCancelButton = false
        ) : base(key: key, children: actionButtons) {
            _dividerThickness = dividerThickness;
            _hasCancelButton = hasCancelButton;
        }

        readonly float _dividerThickness;
        readonly bool _hasCancelButton;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoAlertActions(
                dividerThickness: _dividerThickness,
                hasCancelButton: _hasCancelButton
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((_RenderCupertinoAlertActions) renderObject).dividerThickness = _dividerThickness;
            ((_RenderCupertinoAlertActions) renderObject).hasCancelButton = _hasCancelButton;
        }
    }

    class _RenderCupertinoAlertActions : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox,
        MultiChildLayoutParentData> {
        public _RenderCupertinoAlertActions(
            List<RenderBox> children = null,
            float dividerThickness = 0.0f,
            bool hasCancelButton = false
        ) {
            _dividerThickness = dividerThickness;
            _hasCancelButton = hasCancelButton;
            addAll(children ?? new List<RenderBox>());
        }

        public float dividerThickness {
            get { return _dividerThickness; }
            set {
                if (value == _dividerThickness) {
                    return;
                }

                _dividerThickness = value;
                markNeedsLayout();
            }
        }

        float _dividerThickness;

        bool _hasCancelButton;

        public bool hasCancelButton {
            get { return _hasCancelButton; }
            set {
                if (value == _hasCancelButton) {
                    return;
                }

                _hasCancelButton = value;
                markNeedsLayout();
            }
        }


        readonly Paint _buttonBackgroundPaint = new Paint() {
            color = CupertinoActionSheetUtils._kBackgroundColor,
            style = PaintingStyle.fill
        };

        readonly Paint _pressedButtonBackgroundPaint = new Paint() {
            color = CupertinoActionSheetUtils._kPressedColor,
            style = PaintingStyle.fill
        };

        readonly Paint _dividerPaint = new Paint() {
            color = CupertinoActionSheetUtils._kButtonDividerColor,
            style = PaintingStyle.fill
        };

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is _ActionSheetActionButtonParentData)) {
                child.parentData = new _ActionSheetActionButtonParentData();
            }
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return constraints.minWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return constraints.maxWidth;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            if (childCount == 0) {
                return 0.0f;
            }

            if (childCount == 1) {
                return firstChild.computeMaxIntrinsicHeight(width) + dividerThickness;
            }

            if (hasCancelButton && childCount < 4) {
                return _computeMinIntrinsicHeightWithCancel(width);
            }

            return _computeMinIntrinsicHeightWithoutCancel(width);
        }

        float _computeMinIntrinsicHeightWithCancel(float width) {
            D.assert(childCount == 2 || childCount == 3);
            if (childCount == 2) {
                return firstChild.getMinIntrinsicHeight(width)
                       + childAfter(firstChild).getMinIntrinsicHeight(width)
                       + dividerThickness;
            }

            return firstChild.getMinIntrinsicHeight(width)
                   + childAfter(firstChild).getMinIntrinsicHeight(width)
                   + childAfter(childAfter(firstChild)).getMinIntrinsicHeight(width)
                   + (dividerThickness * 2);
        }

        float _computeMinIntrinsicHeightWithoutCancel(float width) {
            D.assert(childCount >= 2);
            return firstChild.getMinIntrinsicHeight(width)
                   + dividerThickness
                   + (0.5f * childAfter(firstChild).getMinIntrinsicHeight(width));
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (childCount == 0) {
                return 0.0f;
            }

            if (childCount == 1) {
                return firstChild.computeMaxIntrinsicHeight(width) + dividerThickness;
            }

            return _computeMaxIntrinsicHeightStacked(width);
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

        protected override void performLayout() {
            BoxConstraints perButtonConstraints = constraints.copyWith(
                minHeight: 0.0f,
                maxHeight: float.PositiveInfinity
            );
            RenderBox child = firstChild;
            int index = 0;
            float verticalOffset = 0.0f;
            while (child != null) {
                child.layout(
                    perButtonConstraints,
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
                new Size(constraints.maxWidth, verticalOffset)
            );
        }

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;
            _drawButtonBackgroundsAndDividersStacked(canvas, offset);
            _drawButtons(context, offset);
        }

        void _drawButtonBackgroundsAndDividersStacked(Canvas canvas, Offset offset) {
            Offset dividerOffset = new Offset(0.0f, dividerThickness);
            Path backgroundFillPath = new Path();
            // fillType = PathFillType.evenOdd
            backgroundFillPath.addRect(Rect.fromLTWH(0.0f, 0.0f, size.width, size.height));

            Path pressedBackgroundFillPath = new Path();
            Path dividersPath = new Path();
            Offset accumulatingOffset = offset;
            RenderBox child = firstChild;
            RenderBox prevChild = null;
            while (child != null) {
                D.assert(child.parentData is _ActionSheetActionButtonParentData);
                _ActionSheetActionButtonParentData currentButtonParentData =
                    child.parentData as _ActionSheetActionButtonParentData;
                bool isButtonPressed = currentButtonParentData.isPressed;
                bool isPrevButtonPressed = false;
                if (prevChild != null) {
                    D.assert(prevChild.parentData is _ActionSheetActionButtonParentData);
                    _ActionSheetActionButtonParentData previousButtonParentData =
                        prevChild.parentData as _ActionSheetActionButtonParentData;
                    isPrevButtonPressed = previousButtonParentData.isPressed;
                }

                bool isDividerPresent = child != firstChild;
                bool isDividerPainted = isDividerPresent && !(isButtonPressed || isPrevButtonPressed);
                Rect dividerRect = Rect.fromLTWH(
                    accumulatingOffset.dx,
                    accumulatingOffset.dy, size.width, _dividerThickness
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

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            return defaultHitTestChildren(result, position: position);
        }
    }
}