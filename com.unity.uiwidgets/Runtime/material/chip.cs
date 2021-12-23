using System;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    class ChipUtils {
        public const float _kChipHeight = 32.0f;
        public const float _kDeleteIconSize = 18.0f;
        public const int _kCheckmarkAlpha = 0xde; // 87%
        public const int _kDisabledAlpha = 0x61; // 38%
        public const float _kCheckmarkStrokeWidth = 2.0f;
        public static readonly TimeSpan _kSelectDuration = new TimeSpan(0, 0, 0, 0, 195);
        public static readonly TimeSpan _kCheckmarkDuration = new TimeSpan(0, 0, 0, 0, 150);
        public static readonly TimeSpan _kCheckmarkReverseDuration = new TimeSpan(0, 0, 0, 0, 50);
        public static readonly TimeSpan _kDrawerDuration = new TimeSpan(0, 0, 0, 0, 150);
        public static readonly TimeSpan _kReverseDrawerDuration = new TimeSpan(0, 0, 0, 0, 100);
        public static readonly TimeSpan _kDisableDuration = new TimeSpan(0, 0, 0, 0, 75);
        public static readonly Color _kSelectScrimColor = new Color(0x60191919);
        public static readonly Icon _kDefaultDeleteIcon = new Icon(Icons.cancel, size: _kDeleteIconSize);
    }

    public interface ChipAttributes {
        Widget label { get; }

        Widget avatar { get; }

        TextStyle labelStyle { get; }

        ShapeBorder shape { get; }

        Clip clipBehavior { get; }

        FocusNode focusNode { get; }

        bool autofocus { get; }

        Color backgroundColor { get; }

        EdgeInsetsGeometry padding { get; }

        VisualDensity visualDensity { get; }

        EdgeInsetsGeometry labelPadding { get; }

        MaterialTapTargetSize? materialTapTargetSize { get; }

        float? elevation { get; }

        Color shadowColor { get; }
    }

    public interface DeletableChipAttributes {
        Widget deleteIcon { get; }

        VoidCallback onDeleted { get; }

        Color deleteIconColor { get; }

        string deleteButtonTooltipMessage { get; }
    }

    public interface CheckmarkableChipAttributes {
        bool? showCheckmark { get; }

        Color checkmarkColor { get; }
    }

    public interface SelectableChipAttributes {
        bool? selected { get; }

        ValueChanged<bool> onSelected { get; }

        float? pressElevation { get; }

        Color selectedColor { get; }

        string tooltip { get; }

        ShapeBorder avatarBorder { get; }

        Color selectedShadowColor { get; }
    }

    public interface DisabledChipAttributes {
        bool? isEnabled { get; }

        Color disabledColor { get; }
    }

    public interface TappableChipAttributes {
        VoidCallback onPressed { get; }

        float? pressElevation { get; }

        string tooltip { get; }
    }

    public class Chip : StatelessWidget, ChipAttributes, DeletableChipAttributes {
        public Chip(
            Key key = null,
            Widget avatar = null,
            Widget label = null,
            TextStyle labelStyle = null,
            EdgeInsetsGeometry labelPadding = null,
            Widget deleteIcon = null,
            VoidCallback onDeleted = null,
            Color deleteIconColor = null,
            string deleteButtonTooltipMessage = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            Color backgroundColor = null,
            EdgeInsetsGeometry padding = null,
            VisualDensity visualDensity = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            float? elevation = null,
            Color shadowColor = null
        ) : base(key: key) {
            D.assert(label != null);
            D.assert(elevation == null || elevation >= 0.0f);
            _avatar = avatar;
            _label = label;
            _labelStyle = labelStyle;
            _labelPadding = labelPadding;
            _deleteIcon = deleteIcon;
            _onDeleted = onDeleted;
            _deleteIconColor = deleteIconColor;
            _deleteButtonTooltipMessage = deleteButtonTooltipMessage;
            _shape = shape;
            _clipBehavior = clipBehavior;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            _backgroundColor = backgroundColor;
            _padding = padding;
            _materialTapTargetSize = materialTapTargetSize;
            _elevation = elevation;
            _shadowColor = shadowColor;
        }

        public Widget avatar {
            get { return _avatar; }
        }

        Widget _avatar;

        public Widget label {
            get { return _label; }
        }

        Widget _label;

        public TextStyle labelStyle {
            get { return _labelStyle; }
        }

        TextStyle _labelStyle;

        public EdgeInsetsGeometry labelPadding {
            get { return _labelPadding; }
        }

        EdgeInsetsGeometry _labelPadding;

        public ShapeBorder shape {
            get { return _shape; }
        }

        ShapeBorder _shape;

        public Clip clipBehavior {
            get { return _clipBehavior; }
        }

        Clip _clipBehavior;

        public FocusNode focusNode { get; }

        public bool autofocus { get; }

        public Color backgroundColor {
            get { return _backgroundColor; }
        }

        Color _backgroundColor;

        public EdgeInsetsGeometry padding {
            get { return _padding; }
        }

        public VisualDensity visualDensity { get; }

        EdgeInsetsGeometry _padding;

        public Widget deleteIcon {
            get { return _deleteIcon; }
        }

        Widget _deleteIcon;

        public VoidCallback onDeleted {
            get { return _onDeleted; }
        }

        VoidCallback _onDeleted;

        public Color deleteIconColor {
            get { return _deleteIconColor; }
        }

        Color _deleteIconColor;

        public string deleteButtonTooltipMessage {
            get { return _deleteButtonTooltipMessage; }
        }

        string _deleteButtonTooltipMessage;

        public MaterialTapTargetSize? materialTapTargetSize {
            get { return _materialTapTargetSize; }
        }

        MaterialTapTargetSize? _materialTapTargetSize;

        public float? elevation {
            get { return _elevation; }
        }

        float? _elevation;

        public Color shadowColor {
            get { return _shadowColor; }
        }

        Color _shadowColor;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            return new RawChip(
                avatar: avatar,
                label: label,
                labelStyle: labelStyle,
                labelPadding: labelPadding,
                deleteIcon: deleteIcon,
                onDeleted: onDeleted,
                deleteIconColor: deleteIconColor,
                deleteButtonTooltipMessage: deleteButtonTooltipMessage,
                tapEnabled: false,
                shape: shape,
                clipBehavior: clipBehavior,
                focusNode: focusNode,
                autofocus: autofocus,
                backgroundColor: backgroundColor,
                padding: padding,
                visualDensity: visualDensity,
                materialTapTargetSize: materialTapTargetSize,
                elevation: elevation,
                shadowColor: shadowColor,
                isEnabled: true
            );
        }
    }

    public class InputChip : StatelessWidget,
        ChipAttributes,
        DeletableChipAttributes,
        SelectableChipAttributes,
        CheckmarkableChipAttributes,
        DisabledChipAttributes,
        TappableChipAttributes {
        public InputChip(
            Key key = null,
            Widget avatar = null,
            Widget label = null,
            TextStyle labelStyle = null,
            EdgeInsetsGeometry labelPadding = null,
            bool selected = false,
            bool isEnabled = true,
            ValueChanged<bool> onSelected = null,
            Widget deleteIcon = null,
            VoidCallback onDeleted = null,
            Color deleteIconColor = null,
            string deleteButtonTooltipMessage = null,
            VoidCallback onPressed = null,
            float? pressElevation = null,
            Color disabledColor = null,
            Color selectedColor = null,
            string tooltip = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            Color backgroundColor = null,
            EdgeInsetsGeometry padding = null,
            VisualDensity visualDensity = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            float? elevation = null,
            Color shadowColor = null,
            Color selectedShadowColor = null,
            ShapeBorder avatarBorder = null,
            bool showCheckmark = false,
            Color checkmarkColor = null
        ) : base(key: key) {
            D.assert(label != null);
            D.assert(pressElevation == null || pressElevation >= 0.0f);
            D.assert(elevation == null || elevation >= 0.0f);
            _avatarBorder = avatarBorder ?? new CircleBorder();
            _avatar = avatar;
            _label = label;
            _labelStyle = labelStyle;
            _labelPadding = labelPadding;
            _selected = selected;
            _isEnabled = isEnabled;
            _onSelected = onSelected;
            _deleteIcon = deleteIcon;
            _onDeleted = onDeleted;
            _deleteIconColor = deleteIconColor;
            _deleteButtonTooltipMessage = deleteButtonTooltipMessage;
            _onPressed = onPressed;
            _pressElevation = pressElevation;
            _disabledColor = disabledColor;
            _selectedColor = selectedColor;
            _tooltip = tooltip;
            _shape = shape;
            _clipBehavior = clipBehavior;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            _backgroundColor = backgroundColor;
            _padding = padding;
            this.visualDensity = visualDensity;
            _materialTapTargetSize = materialTapTargetSize;
            _elevation = elevation;
            _shadowColor = shadowColor;
            _selectedShadowColor = selectedShadowColor;
            this.showCheckmark = showCheckmark;
            this.checkmarkColor = checkmarkColor;
        }

        public Widget avatar {
            get { return _avatar; }
        }

        Widget _avatar;

        public Widget label {
            get { return _label; }
        }

        Widget _label;

        public TextStyle labelStyle {
            get { return _labelStyle; }
        }

        TextStyle _labelStyle;

        public EdgeInsetsGeometry labelPadding {
            get { return _labelPadding; }
        }

        EdgeInsetsGeometry _labelPadding;

        public bool? selected {
            get { return _selected; }
        }

        bool _selected;

        public bool? isEnabled {
            get { return _isEnabled; }
        }

        bool _isEnabled;

        public ValueChanged<bool> onSelected {
            get { return _onSelected; }
        }

        ValueChanged<bool> _onSelected;

        public Widget deleteIcon {
            get { return _deleteIcon; }
        }

        Widget _deleteIcon;

        public VoidCallback onDeleted {
            get { return _onDeleted; }
        }

        VoidCallback _onDeleted;

        public Color deleteIconColor {
            get { return _deleteIconColor; }
        }

        Color _deleteIconColor;

        public string deleteButtonTooltipMessage {
            get { return _deleteButtonTooltipMessage; }
        }

        string _deleteButtonTooltipMessage;

        public VoidCallback onPressed {
            get { return _onPressed; }
        }

        VoidCallback _onPressed;

        public float? pressElevation {
            get { return _pressElevation; }
        }

        float? _pressElevation;

        public Color disabledColor {
            get { return _disabledColor; }
        }

        Color _disabledColor;

        public Color selectedColor {
            get { return _selectedColor; }
        }

        Color _selectedColor;

        public string tooltip {
            get { return _tooltip; }
        }

        string _tooltip;

        public ShapeBorder shape {
            get { return _shape; }
        }

        ShapeBorder _shape;

        public Clip clipBehavior {
            get { return _clipBehavior; }
        }

        Clip _clipBehavior;

        public FocusNode focusNode { get; }

        public bool autofocus { get; }

        public Color backgroundColor {
            get { return _backgroundColor; }
        }

        Color _backgroundColor;

        public EdgeInsetsGeometry padding {
            get { return _padding; }
        }

        EdgeInsetsGeometry _padding;

        public VisualDensity visualDensity { get; }

        public MaterialTapTargetSize? materialTapTargetSize {
            get { return _materialTapTargetSize; }
        }

        MaterialTapTargetSize? _materialTapTargetSize;

        public float? elevation {
            get { return _elevation; }
        }

        float? _elevation;

        public Color shadowColor {
            get { return _shadowColor; }
        }

        Color _shadowColor;

        public Color selectedShadowColor {
            get { return _selectedShadowColor; }
        }

        Color _selectedShadowColor;

        public ShapeBorder avatarBorder {
            get { return _avatarBorder; }
        }

        public bool? showCheckmark { get; }

        public Color checkmarkColor { get; }

        ShapeBorder _avatarBorder;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            return new RawChip(
                avatar: avatar,
                label: label,
                labelStyle: labelStyle,
                labelPadding: labelPadding,
                deleteIcon: deleteIcon,
                onDeleted: onDeleted,
                deleteIconColor: deleteIconColor,
                deleteButtonTooltipMessage: deleteButtonTooltipMessage,
                onSelected: onSelected,
                onPressed: onPressed,
                pressElevation: pressElevation,
                selected: selected,
                tapEnabled: true,
                disabledColor: disabledColor,
                selectedColor: selectedColor,
                tooltip: tooltip,
                shape: shape,
                clipBehavior: clipBehavior,
                focusNode: focusNode,
                autofocus: autofocus,
                backgroundColor: backgroundColor,
                padding: padding,
                visualDensity: visualDensity,
                materialTapTargetSize: materialTapTargetSize,
                elevation: elevation,
                shadowColor: shadowColor,
                selectedShadowColor: selectedShadowColor,
                showCheckmark: showCheckmark,
                checkmarkColor: checkmarkColor,
                isEnabled: isEnabled == true &&
                           (onSelected != null || onDeleted != null || onPressed != null),
                avatarBorder: avatarBorder
            );
        }
    }

    public class ChoiceChip : StatelessWidget,
        ChipAttributes,
        SelectableChipAttributes,
        DisabledChipAttributes {
        public ChoiceChip(
            Key key,
            Widget avatar = null,
            Widget label = null,
            TextStyle labelStyle = null,
            EdgeInsetsGeometry labelPadding = null,
            ValueChanged<bool> onSelected = null,
            float? pressElevation = null,
            bool? selected = null,
            Color selectedColor = null,
            Color disabledColor = null,
            string tooltip = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            Color backgroundColor = null,
            EdgeInsetsGeometry padding = null,
            VisualDensity visualDensity = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            float? elevation = null,
            Color shadowColor = null,
            Color selectedShadowColor = null,
            ShapeBorder avatarBorder = null
        ) : base(key: key) {
            D.assert(selected != null);
            D.assert(label != null);
            D.assert(pressElevation == null || pressElevation >= 0.0f);
            D.assert(elevation == null || elevation >= 0.0f);
            _avatarBorder = avatarBorder ?? new CircleBorder();
            _avatar = avatar;
            _label = label;
            _labelStyle = labelStyle;
            _labelPadding = labelPadding;
            _onSelected = onSelected;
            _pressElevation = pressElevation;
            _selected = selected;
            _selectedColor = selectedColor;
            _disabledColor = disabledColor;
            _tooltip = tooltip;
            _shape = shape;
            _clipBehavior = clipBehavior;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            _backgroundColor = backgroundColor;
            _padding = padding;
            this.visualDensity = visualDensity;
            _materialTapTargetSize = materialTapTargetSize;
            _elevation = elevation;
            _shadowColor = shadowColor;
            _selectedShadowColor = selectedShadowColor;
        }

        public Widget avatar {
            get { return _avatar; }
        }

        Widget _avatar;

        public Widget label {
            get { return _label; }
        }

        Widget _label;

        public TextStyle labelStyle {
            get { return _labelStyle; }
        }

        TextStyle _labelStyle;

        public EdgeInsetsGeometry labelPadding {
            get { return _labelPadding; }
        }

        EdgeInsetsGeometry _labelPadding;

        public ValueChanged<bool> onSelected {
            get { return _onSelected; }
        }

        ValueChanged<bool> _onSelected;

        public float? pressElevation {
            get { return _pressElevation; }
        }

        float? _pressElevation;

        public bool? selected {
            get { return _selected; }
        }

        bool? _selected;

        public Color disabledColor {
            get { return _disabledColor; }
        }

        Color _disabledColor;

        public Color selectedColor {
            get { return _selectedColor; }
        }

        Color _selectedColor;

        public string tooltip {
            get { return _tooltip; }
        }

        string _tooltip;

        public ShapeBorder shape {
            get { return _shape; }
        }

        ShapeBorder _shape;

        public Clip clipBehavior {
            get { return _clipBehavior; }
        }

        Clip _clipBehavior;

        public FocusNode focusNode { get; }
        public bool autofocus { get; }

        public Color backgroundColor {
            get { return _backgroundColor; }
        }

        Color _backgroundColor;

        public EdgeInsetsGeometry padding {
            get { return _padding; }
        }

        EdgeInsetsGeometry _padding;

        public VisualDensity visualDensity { get; }

        public MaterialTapTargetSize? materialTapTargetSize {
            get { return _materialTapTargetSize; }
        }

        MaterialTapTargetSize? _materialTapTargetSize;

        public float? elevation {
            get { return _elevation; }
        }

        float? _elevation;

        public Color shadowColor {
            get { return _shadowColor; }
        }

        Color _shadowColor;

        public Color selectedShadowColor {
            get { return _selectedShadowColor; }
        }

        Color _selectedShadowColor;

        public ShapeBorder avatarBorder {
            get { return _avatarBorder; }
        }

        ShapeBorder _avatarBorder;

        public bool? isEnabled {
            get { return onSelected != null; }
        }

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            ChipThemeData chipTheme = ChipTheme.of(context);
            return new RawChip(
                avatar: avatar,
                label: label,
                labelStyle: labelStyle ?? (selected == true ? chipTheme.secondaryLabelStyle : null),
                labelPadding: labelPadding,
                onSelected: onSelected,
                pressElevation: pressElevation,
                selected: selected,
                showCheckmark: false,
                onDeleted: null,
                tooltip: tooltip,
                shape: shape,
                clipBehavior: clipBehavior,
                focusNode: focusNode,
                autofocus: autofocus,
                disabledColor: disabledColor,
                selectedColor: selectedColor ?? chipTheme.secondarySelectedColor,
                backgroundColor: backgroundColor,
                padding: padding,
                visualDensity: visualDensity,
                isEnabled: isEnabled,
                materialTapTargetSize: materialTapTargetSize,
                elevation: elevation,
                shadowColor: shadowColor,
                selectedShadowColor: selectedShadowColor,
                avatarBorder: avatarBorder
            );
        }
    }

    public class FilterChip : StatelessWidget,
        ChipAttributes,
        SelectableChipAttributes,
        CheckmarkableChipAttributes,
        DisabledChipAttributes {
        public FilterChip(
            Key key = null,
            Widget avatar = null,
            Widget label = null,
            TextStyle labelStyle = null,
            EdgeInsetsGeometry labelPadding = null,
            bool selected = false,
            ValueChanged<bool> onSelected = null,
            float? pressElevation = null,
            Color disabledColor = null,
            Color selectedColor = null,
            string tooltip = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            Color backgroundColor = null,
            EdgeInsetsGeometry padding = null,
            VisualDensity visualDensity = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            float? elevation = null,
            Color shadowColor = null,
            Color selectedShadowColor = null,
            ShapeBorder avatarBorder = null,
            bool showCheckmark = false,
            Color checkmarkColor = null
        ) : base(key: key) {
            D.assert(label != null);
            D.assert(pressElevation == null || pressElevation >= 0.0f);
            D.assert(elevation == null || elevation >= 0.0f);
            _avatarBorder = avatarBorder ?? new CircleBorder();
            _avatar = avatar;
            _label = label;
            _labelStyle = labelStyle;
            _labelPadding = labelPadding;
            _selected = selected;
            _onSelected = onSelected;
            _pressElevation = pressElevation;
            _disabledColor = disabledColor;
            _selectedColor = selectedColor;
            _tooltip = tooltip;
            _shape = shape;
            _clipBehavior = clipBehavior;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            _backgroundColor = backgroundColor;
            _padding = padding;
            this.visualDensity = visualDensity;
            _materialTapTargetSize = materialTapTargetSize;
            _elevation = elevation;
            _shadowColor = shadowColor;
            _selectedShadowColor = selectedShadowColor;
            this.showCheckmark = showCheckmark;
            this.checkmarkColor = checkmarkColor;
        }

        public Widget avatar {
            get { return _avatar; }
        }

        Widget _avatar;

        public Widget label {
            get { return _label; }
        }

        Widget _label;

        public TextStyle labelStyle {
            get { return _labelStyle; }
        }

        TextStyle _labelStyle;

        public EdgeInsetsGeometry labelPadding {
            get { return _labelPadding; }
        }

        EdgeInsetsGeometry _labelPadding;

        public bool? selected {
            get { return _selected; }
        }

        bool _selected;

        public ValueChanged<bool> onSelected {
            get { return _onSelected; }
        }

        ValueChanged<bool> _onSelected;

        public float? pressElevation {
            get { return _pressElevation; }
        }

        float? _pressElevation;

        public Color disabledColor {
            get { return _disabledColor; }
        }

        Color _disabledColor;

        public Color selectedColor {
            get { return _selectedColor; }
        }

        Color _selectedColor;

        public string tooltip {
            get { return _tooltip; }
        }

        string _tooltip;

        public ShapeBorder shape {
            get { return _shape; }
        }

        ShapeBorder _shape;

        public Clip clipBehavior {
            get { return _clipBehavior; }
        }

        Clip _clipBehavior;

        public FocusNode focusNode { get; }

        public bool autofocus { get; }

        public Color backgroundColor {
            get { return _backgroundColor; }
        }

        Color _backgroundColor;

        public EdgeInsetsGeometry padding {
            get { return _padding; }
        }

        EdgeInsetsGeometry _padding;

        public VisualDensity visualDensity { get; }

        public MaterialTapTargetSize? materialTapTargetSize {
            get { return _materialTapTargetSize; }
        }

        MaterialTapTargetSize? _materialTapTargetSize;

        public float? elevation {
            get { return _elevation; }
        }

        float? _elevation;

        public Color shadowColor {
            get { return _shadowColor; }
        }

        Color _shadowColor;

        public Color selectedShadowColor {
            get { return _selectedShadowColor; }
        }

        Color _selectedShadowColor;

        public bool? showCheckmark { get; }

        public Color checkmarkColor { get; }

        public ShapeBorder avatarBorder {
            get { return _avatarBorder; }
        }

        ShapeBorder _avatarBorder;

        public bool? isEnabled {
            get { return onSelected != null; }
        }

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            return new RawChip(
                avatar: avatar,
                label: label,
                labelStyle: labelStyle,
                labelPadding: labelPadding,
                onSelected: onSelected,
                pressElevation: pressElevation,
                selected: selected,
                tooltip: tooltip,
                shape: shape,
                clipBehavior: clipBehavior,
                focusNode: focusNode,
                autofocus: autofocus,
                backgroundColor: backgroundColor,
                disabledColor: disabledColor,
                selectedColor: selectedColor,
                padding: padding,
                visualDensity: visualDensity,
                isEnabled: isEnabled,
                materialTapTargetSize: materialTapTargetSize,
                elevation: elevation,
                shadowColor: shadowColor,
                selectedShadowColor: selectedShadowColor,
                showCheckmark: showCheckmark,
                checkmarkColor: checkmarkColor,
                avatarBorder: avatarBorder
            );
        }
    }

    public class ActionChip : StatelessWidget, ChipAttributes, TappableChipAttributes {
        public ActionChip(
            Key key = null,
            Widget avatar = null,
            Widget label = null,
            TextStyle labelStyle = null,
            EdgeInsetsGeometry labelPadding = null,
            VoidCallback onPressed = null,
            float? pressElevation = null,
            string tooltip = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            Color backgroundColor = null,
            EdgeInsetsGeometry padding = null,
            VisualDensity visualDensity = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            float? elevation = null,
            Color shadowColor = null
        ) : base(key: key) {
            D.assert(label != null);
            D.assert(
                onPressed != null,
                () => "Rather than disabling an ActionChip by setting onPressed to null, " +
                      "remove it from the interface entirely."
            );
            D.assert(pressElevation == null || pressElevation >= 0.0f);
            D.assert(elevation == null || elevation >= 0.0f);
            _avatar = avatar;
            _label = label;
            _labelStyle = labelStyle;
            _labelPadding = labelPadding;
            _onPressed = onPressed;
            _pressElevation = pressElevation;
            _tooltip = tooltip;
            _shape = shape;
            _clipBehavior = clipBehavior;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            _backgroundColor = backgroundColor;
            _padding = padding;
            this.visualDensity = visualDensity;
            _materialTapTargetSize = materialTapTargetSize;
            _elevation = elevation;
            _shadowColor = shadowColor;
        }


        public Widget avatar {
            get { return _avatar; }
        }

        Widget _avatar;

        public Widget label {
            get { return _label; }
        }

        Widget _label;

        public TextStyle labelStyle {
            get { return _labelStyle; }
        }

        TextStyle _labelStyle;

        public EdgeInsetsGeometry labelPadding {
            get { return _labelPadding; }
        }

        EdgeInsetsGeometry _labelPadding;

        public VoidCallback onPressed {
            get { return _onPressed; }
        }

        VoidCallback _onPressed;

        public float? pressElevation {
            get { return _pressElevation; }
        }

        float? _pressElevation;

        public string tooltip {
            get { return _tooltip; }
        }

        string _tooltip;

        public ShapeBorder shape {
            get { return _shape; }
        }

        ShapeBorder _shape;

        public Clip clipBehavior {
            get { return _clipBehavior; }
        }

        Clip _clipBehavior;

        public FocusNode focusNode { get; }

        public bool autofocus { get; }

        public Color backgroundColor {
            get { return _backgroundColor; }
        }

        Color _backgroundColor;

        public EdgeInsetsGeometry padding {
            get { return _padding; }
        }

        EdgeInsetsGeometry _padding;

        public VisualDensity visualDensity { get; }

        public MaterialTapTargetSize? materialTapTargetSize {
            get { return _materialTapTargetSize; }
        }

        MaterialTapTargetSize? _materialTapTargetSize;

        public float? elevation {
            get { return _elevation; }
        }

        float? _elevation;

        public Color shadowColor {
            get { return _shadowColor; }
        }

        Color _shadowColor;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            return new RawChip(
                avatar: avatar,
                label: label,
                onPressed: onPressed,
                pressElevation: pressElevation,
                tooltip: tooltip,
                labelStyle: labelStyle,
                backgroundColor: backgroundColor,
                shape: shape,
                clipBehavior: clipBehavior,
                focusNode: focusNode,
                autofocus: autofocus,
                padding: padding,
                visualDensity: visualDensity,
                labelPadding: labelPadding,
                isEnabled: true,
                materialTapTargetSize: materialTapTargetSize,
                elevation: elevation,
                shadowColor: _shadowColor
            );
        }
    }

    public class RawChip : StatefulWidget,
        ChipAttributes,
        DeletableChipAttributes,
        SelectableChipAttributes,
        CheckmarkableChipAttributes,
        DisabledChipAttributes,
        TappableChipAttributes {
        public RawChip(
            Key key = null,
            Widget avatar = null,
            Widget label = null,
            TextStyle labelStyle = null,
            EdgeInsetsGeometry padding = null,
            VisualDensity visualDensity = null,
            EdgeInsetsGeometry labelPadding = null,
            Widget deleteIcon = null,
            VoidCallback onDeleted = null,
            Color deleteIconColor = null,
            string deleteButtonTooltipMessage = null,
            VoidCallback onPressed = null,
            ValueChanged<bool> onSelected = null,
            float? pressElevation = null,
            bool? tapEnabled = true,
            bool? selected = null,
            bool? isEnabled = true,
            Color disabledColor = null,
            Color selectedColor = null,
            string tooltip = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            Color backgroundColor = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            float? elevation = null,
            Color shadowColor = null,
            Color selectedShadowColor = null,
            bool? showCheckmark = true,
            Color checkmarkColor = null,
            ShapeBorder avatarBorder = null
        ) : base(key: key) {
            D.assert(label != null);
            D.assert(isEnabled != null);
            D.assert(pressElevation == null || pressElevation >= 0.0f);
            D.assert(elevation == null || elevation >= 0.0f);
            deleteIcon = deleteIcon ?? ChipUtils._kDefaultDeleteIcon;
            _avatarBorder = avatarBorder ?? new CircleBorder();
            _avatar = avatar;
            _label = label;
            _labelStyle = labelStyle;
            _padding = padding;
            this.visualDensity = visualDensity;
            _labelPadding = labelPadding;
            _deleteIcon = deleteIcon;
            _onDeleted = onDeleted;
            _deleteIconColor = deleteIconColor;
            _deleteButtonTooltipMessage = deleteButtonTooltipMessage;
            _onPressed = onPressed;
            _onSelected = onSelected;
            _pressElevation = pressElevation;
            _tapEnabled = tapEnabled;
            _selected = selected;
            _isEnabled = isEnabled;
            _disabledColor = disabledColor;
            _selectedColor = selectedColor;
            _tooltip = tooltip;
            _shape = shape;
            _clipBehavior = clipBehavior;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            _backgroundColor = backgroundColor;
            _materialTapTargetSize = materialTapTargetSize;
            _elevation = elevation;
            _shadowColor = shadowColor;
            _selectedShadowColor = selectedShadowColor;
            this.showCheckmark = showCheckmark;
            this.checkmarkColor = checkmarkColor;
        }


        public Widget avatar {
            get { return _avatar; }
        }

        Widget _avatar;

        public Widget label {
            get { return _label; }
        }

        Widget _label;

        public TextStyle labelStyle {
            get { return _labelStyle; }
        }

        TextStyle _labelStyle;

        public EdgeInsetsGeometry labelPadding {
            get { return _labelPadding; }
        }

        EdgeInsetsGeometry _labelPadding;

        public Widget deleteIcon {
            get { return _deleteIcon; }
        }

        Widget _deleteIcon;

        public VoidCallback onDeleted {
            get { return _onDeleted; }
        }

        VoidCallback _onDeleted;

        public Color deleteIconColor {
            get { return _deleteIconColor; }
        }

        Color _deleteIconColor;

        public string deleteButtonTooltipMessage {
            get { return _deleteButtonTooltipMessage; }
        }

        string _deleteButtonTooltipMessage;

        public ValueChanged<bool> onSelected {
            get { return _onSelected; }
        }

        ValueChanged<bool> _onSelected;

        public VoidCallback onPressed {
            get { return _onPressed; }
        }

        VoidCallback _onPressed;

        public float? pressElevation {
            get { return _pressElevation; }
        }

        float? _pressElevation;

        public bool? selected {
            get { return _selected; }
        }

        bool? _selected;

        public bool? isEnabled {
            get { return _isEnabled; }
        }

        bool? _isEnabled;

        public Color disabledColor {
            get { return _disabledColor; }
        }

        Color _disabledColor;

        public Color selectedColor {
            get { return _selectedColor; }
        }

        Color _selectedColor;

        public string tooltip {
            get { return _tooltip; }
        }

        string _tooltip;

        public ShapeBorder shape {
            get { return _shape; }
        }

        ShapeBorder _shape;

        public Clip clipBehavior {
            get { return _clipBehavior; }
        }

        Clip _clipBehavior;

        public FocusNode focusNode { get; }

        public bool autofocus { get; }

        public Color backgroundColor {
            get { return _backgroundColor; }
        }

        Color _backgroundColor;

        public EdgeInsetsGeometry padding {
            get { return _padding; }
        }

        EdgeInsetsGeometry _padding;

        public VisualDensity visualDensity { get; }

        public MaterialTapTargetSize? materialTapTargetSize {
            get { return _materialTapTargetSize; }
        }

        MaterialTapTargetSize? _materialTapTargetSize;

        public float? elevation {
            get { return _elevation; }
        }

        float? _elevation;

        public Color shadowColor {
            get { return _shadowColor; }
        }

        Color _shadowColor;

        public Color selectedShadowColor {
            get { return _selectedShadowColor; }
        }

        Color _selectedShadowColor;

        public bool? showCheckmark { get; }

        public Color checkmarkColor { get; }

        public ShapeBorder avatarBorder {
            get { return _avatarBorder; }
        }

        ShapeBorder _avatarBorder;

        public bool? tapEnabled {
            get { return _tapEnabled; }
        }

        bool? _tapEnabled;

        public override State createState() {
            return new _RawChipState();
        }
    }

    class _RawChipState : TickerProviderStateMixin<RawChip> {
        static readonly TimeSpan pressedAnimationDuration = new TimeSpan(0, 0, 0, 0, 75);

        AnimationController selectController;
        AnimationController avatarDrawerController;
        AnimationController deleteDrawerController;
        AnimationController enableController;
        Animation<float> checkmarkAnimation;
        Animation<float> avatarDrawerAnimation;
        Animation<float> deleteDrawerAnimation;
        Animation<float> enableAnimation;
        Animation<float> selectionFade;


        readonly HashSet<MaterialState> _states = new HashSet<MaterialState>();

        public readonly GlobalKey deleteIconKey = GlobalKey.key();


        public bool hasDeleteButton {
            get { return widget.onDeleted != null; }
        }

        public bool hasAvatar {
            get { return widget.avatar != null; }
        }

        public bool canTap {
            get {
                return widget.isEnabled == true
                       && widget.tapEnabled == true
                       && (widget.onPressed != null || widget.onSelected != null);
            }
        }

        bool _isTapping = false;

        public bool isTapping {
            get { return canTap && _isTapping; }
        }

        public override void initState() {
            D.assert(widget.onSelected == null || widget.onPressed == null);
            base.initState();
            _updateState(MaterialState.disabled, !(widget.isEnabled != null && widget.isEnabled.Value));
            _updateState(MaterialState.selected, widget.selected != null && widget.selected.Value);
            selectController = new AnimationController(
                duration: ChipUtils._kSelectDuration,
                value: widget.selected == true ? 1.0f : 0.0f,
                vsync: this
            );
            selectionFade = new CurvedAnimation(
                parent: selectController,
                curve: Curves.fastOutSlowIn
            );
            avatarDrawerController = new AnimationController(
                duration: ChipUtils._kDrawerDuration,
                value: hasAvatar || widget.selected == true ? 1.0f : 0.0f,
                vsync: this
            );
            deleteDrawerController = new AnimationController(
                duration: ChipUtils._kDrawerDuration,
                value: hasDeleteButton ? 1.0f : 0.0f,
                vsync: this
            );
            enableController = new AnimationController(
                duration: ChipUtils._kDisableDuration,
                value: widget.isEnabled == true ? 1.0f : 0.0f,
                vsync: this
            );

            float checkmarkPercentage = (float) (ChipUtils._kCheckmarkDuration.TotalMilliseconds /
                                                 ChipUtils._kSelectDuration.TotalMilliseconds);
            float checkmarkReversePercentage = (float) (ChipUtils._kCheckmarkReverseDuration.TotalMilliseconds /
                                                        ChipUtils._kSelectDuration.TotalMilliseconds);
            float avatarDrawerReversePercentage = (float) (ChipUtils._kReverseDrawerDuration.TotalMilliseconds /
                                                           ChipUtils._kSelectDuration.TotalMilliseconds);
            checkmarkAnimation = new CurvedAnimation(
                parent: selectController,
                curve: new Interval(1.0f - checkmarkPercentage, 1.0f, curve: Curves.fastOutSlowIn),
                reverseCurve: new Interval(
                    1.0f - checkmarkReversePercentage,
                    1.0f,
                    curve: Curves.fastOutSlowIn
                )
            );
            deleteDrawerAnimation = new CurvedAnimation(
                parent: deleteDrawerController,
                curve: Curves.fastOutSlowIn
            );
            avatarDrawerAnimation = new CurvedAnimation(
                parent: avatarDrawerController,
                curve: Curves.fastOutSlowIn,
                reverseCurve: new Interval(
                    1.0f - avatarDrawerReversePercentage,
                    1.0f,
                    curve: Curves.fastOutSlowIn
                )
            );
            enableAnimation = new CurvedAnimation(
                parent: enableController,
                curve: Curves.fastOutSlowIn
            );
        }

        public override void dispose() {
            selectController.dispose();
            avatarDrawerController.dispose();
            deleteDrawerController.dispose();
            enableController.dispose();
            base.dispose();
        }

        void _updateState(MaterialState state, bool value) {
            if (value) {
                _states.Add(state);
            }
            else {
                _states.Remove(state);
            }
        }

        void _handleTapDown(TapDownDetails details) {
            if (!canTap) {
                return;
            }

            setState(() => {
                _isTapping = true;
                _updateState(MaterialState.pressed, true);
            });
        }

        void _handleTapCancel() {
            if (!canTap) {
                return;
            }

            setState(() => {
                _isTapping = false;
                _updateState(MaterialState.pressed, false);
            });
        }

        void _handleTap() {
            if (!canTap) {
                return;
            }

            setState(() => {
                _isTapping = false;
                _updateState(MaterialState.pressed, false);
            });
            widget.onSelected?.Invoke(!widget.selected == true);
            widget.onPressed?.Invoke();
        }

        void _handleFocus(bool isFocused) {
            setState(() => { _updateState(MaterialState.focused, isFocused); });
        }

        void _handleHover(bool isHovered) {
            setState(() => { _updateState(MaterialState.hovered, isHovered); });
        }

        Color getBackgroundColor(ChipThemeData theme) {
            ColorTween backgroundTween = new ColorTween(
                begin: widget.disabledColor ?? theme.disabledColor,
                end: widget.backgroundColor ?? theme.backgroundColor
            );
            ColorTween selectTween = new ColorTween(
                begin: backgroundTween.evaluate(enableController),
                end: widget.selectedColor ?? theme.selectedColor
            );
            return selectTween.evaluate(selectionFade);
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            RawChip oldWidget = _oldWidget as RawChip;
            base.didUpdateWidget(oldWidget);
            if (oldWidget.isEnabled != widget.isEnabled) {
                setState(() => {
                    _updateState(MaterialState.disabled, !(widget.isEnabled != null && widget.isEnabled.Value));
                    if (widget.isEnabled == true) {
                        enableController.forward();
                    }
                    else {
                        enableController.reverse();
                    }
                });
            }

            if (oldWidget.avatar != widget.avatar || oldWidget.selected != widget.selected) {
                setState(() => {
                    if (hasAvatar || widget.selected == true) {
                        avatarDrawerController.forward();
                    }
                    else {
                        avatarDrawerController.reverse();
                    }
                });
            }

            if (oldWidget.selected != widget.selected) {
                setState(() => {
                    _updateState(MaterialState.selected, widget.selected != null && widget.selected.Value);
                    if (widget.selected == true) {
                        selectController.forward();
                    }
                    else {
                        selectController.reverse();
                    }
                });
            }

            if (oldWidget.onDeleted != widget.onDeleted) {
                setState(() => {
                    if (hasDeleteButton) {
                        deleteDrawerController.forward();
                    }
                    else {
                        deleteDrawerController.reverse();
                    }
                });
            }
        }

        Widget _wrapWithTooltip(string tooltip, VoidCallback callback, Widget child) {
            if (child == null || callback == null || tooltip == null) {
                return child;
            }

            return new Tooltip(
                message: tooltip,
                child: child
            );
        }

        Widget _buildDeleteIcon(
            BuildContext context,
            ThemeData theme,
            ChipThemeData chipTheme,
            GlobalKey deleteIconKey
        ) {
            if (!hasDeleteButton) {
                return null;
            }

            return _wrapWithTooltip(
                widget.deleteButtonTooltipMessage ?? MaterialLocalizations.of(context)?.deleteButtonTooltip,
                widget.onDeleted,
                new GestureDetector(
                    key: deleteIconKey,
                    behavior: HitTestBehavior.opaque,
                    onTap: widget.isEnabled != null && widget.isEnabled.Value
                        ? () => {
                            Feedback.forTap(context);
                            widget.onDeleted();
                        }
                        : (GestureTapCallback) null,
                    child: new IconTheme(
                        data: theme.iconTheme.copyWith(
                            color: widget.deleteIconColor ?? chipTheme.deleteIconColor
                        ),
                        child: widget.deleteIcon
                    )
                )
            );
        }

        const float _defaultElevation = 0.0f;
        const float _defaultPressElevation = 8.0f;
        static readonly Color _defaultShadowColor = Colors.black;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            D.assert(material_.debugCheckHasMaterialLocalizations(context));

            ThemeData theme = Theme.of(context);
            ChipThemeData chipTheme = ChipTheme.of(context);
            TextDirection textDirection = Directionality.of(context);
            ShapeBorder shape = widget.shape ?? chipTheme.shape;
            float elevation = widget.elevation ?? (chipTheme.elevation ?? _defaultElevation);
            float pressElevation = widget.pressElevation ?? (chipTheme.pressElevation ?? _defaultPressElevation);
            Color shadowColor = widget.shadowColor ?? chipTheme.shadowColor ?? _defaultShadowColor;
            Color selectedShadowColor =
                widget.selectedShadowColor ?? chipTheme.selectedShadowColor ?? _defaultShadowColor;
            Color checkmarkColor = widget.checkmarkColor ?? chipTheme.checkmarkColor;
            bool showCheckmark = widget.showCheckmark ?? chipTheme.showCheckmark ?? true;

            TextStyle effectiveLabelStyle = widget.labelStyle ?? chipTheme.labelStyle;
            Color resolvedLabelColor = MaterialStateProperty<Color>.resolveAsMaterialStateProperty(effectiveLabelStyle?.color, _states);
            TextStyle resolvedLabelStyle = effectiveLabelStyle?.copyWith(color: resolvedLabelColor);

            Widget result = new Material(
                elevation: isTapping ? pressElevation : elevation,
                shadowColor: widget.selected ?? false ? selectedShadowColor : shadowColor,
                animationDuration: pressedAnimationDuration,
                shape: shape,
                clipBehavior: widget.clipBehavior,
                child: new InkWell(
                    onFocusChange: _handleFocus,
                    focusNode: widget.focusNode,
                    autofocus: widget.autofocus,
                    canRequestFocus: widget.isEnabled ?? false,
                    onTap: canTap ? _handleTap : (GestureTapCallback) null,
                    onTapDown: canTap ? _handleTapDown : (GestureTapDownCallback) null,
                    onTapCancel: canTap ? _handleTapCancel : (GestureTapCancelCallback) null,
                    onHover: canTap ? _handleHover : (ValueChanged<bool>) null,
                    splashFactory: new _LocationAwareInkRippleFactory(
                        hasDeleteButton,
                        context,
                        deleteIconKey
                    ),
                    customBorder: shape,
                    child: new AnimatedBuilder(
                        animation: ListenableUtils.merge(new List<Listenable>
                            {selectController, enableController}),
                        builder: (BuildContext _context, Widget child) => {
                            return new Container(
                                decoration: new ShapeDecoration(
                                    shape: shape,
                                    color: getBackgroundColor(chipTheme)
                                ),
                                child: child
                            );
                        },
                        child: _wrapWithTooltip(widget.tooltip, widget.onPressed,
                            new _ChipRenderWidget(
                                theme: new _ChipRenderTheme(
                                    label: new DefaultTextStyle(
                                        overflow: TextOverflow.fade,
                                        textAlign: TextAlign.left,
                                        maxLines: 1,
                                        softWrap: false,
                                        style: resolvedLabelStyle,
                                        child: widget.label
                                    ),
                                    avatar: new AnimatedSwitcher(
                                        child: widget.avatar,
                                        duration: ChipUtils._kDrawerDuration,
                                        switchInCurve: Curves.fastOutSlowIn
                                    ),
                                    deleteIcon: new AnimatedSwitcher(
                                        child: _buildDeleteIcon(context, theme, chipTheme, deleteIconKey),
                                        duration: ChipUtils._kDrawerDuration,
                                        switchInCurve: Curves.fastOutSlowIn
                                    ),
                                    brightness: chipTheme.brightness,
                                    padding: (widget.padding ?? chipTheme.padding).resolve(textDirection),
                                    visualDensity: widget.visualDensity ?? theme.visualDensity,
                                    labelPadding:
                                    (widget.labelPadding ?? chipTheme.labelPadding).resolve(textDirection),
                                    showAvatar: hasAvatar,
                                    showCheckmark: showCheckmark,
                                    checkmarkColor: checkmarkColor,
                                    canTapBody: canTap
                                ),
                                value: widget.selected,
                                checkmarkAnimation: checkmarkAnimation,
                                enableAnimation: enableAnimation,
                                avatarDrawerAnimation: avatarDrawerAnimation,
                                deleteDrawerAnimation: deleteDrawerAnimation,
                                isEnabled: widget.isEnabled,
                                avatarBorder: widget.avatarBorder
                            )
                        )
                    )
                )
            );
            BoxConstraints constraints;
            Offset densityAdjustment = (widget.visualDensity ?? theme.visualDensity).baseSizeAdjustment;
            switch (widget.materialTapTargetSize ?? theme.materialTapTargetSize) {
                case MaterialTapTargetSize.padded:
                    constraints =
                        new BoxConstraints(minHeight: material_.kMinInteractiveDimension + densityAdjustment.dy);
                    break;
                case MaterialTapTargetSize.shrinkWrap:
                    constraints = new BoxConstraints();
                    break;
                default:
                    throw new Exception("Unknown Material Tap Target Size: " + widget.materialTapTargetSize);
            }

            result = new _ChipRedirectingHitDetectionWidget(
                constraints: constraints,
                child: new Center(
                    child: result,
                    widthFactor: 1.0f,
                    heightFactor: 1.0f
                )
            );
            return result;
        }
    }

    class _ChipRedirectingHitDetectionWidget : SingleChildRenderObjectWidget {
        public _ChipRedirectingHitDetectionWidget(
            Key key = null,
            Widget child = null,
            BoxConstraints constraints = null
        ) : base(key: key, child: child) {
            this.constraints = constraints;
        }

        public readonly BoxConstraints constraints;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderChipRedirectingHitDetection(constraints);
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderChipRedirectingHitDetection renderObject = _renderObject as _RenderChipRedirectingHitDetection;
            renderObject.additionalConstraints = constraints;
        }
    }

    class _RenderChipRedirectingHitDetection : RenderConstrainedBox {
        public _RenderChipRedirectingHitDetection(BoxConstraints additionalConstraints) : base(
            additionalConstraints: additionalConstraints) {
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            if (!size.contains(position)) {
                return false;
            }

            Offset offset = new Offset(position.dx, size.height / 2);
            return result.addWithRawTransform(
                transform: MatrixUtils.forceToPoint(offset),
                position: position,
                hitTest: (BoxHitTestResult _result, Offset _position) => {
                    D.assert(_position == offset);
                    return child.hitTest(_result, position: offset);
                }
            );
        }
    }

    class _ChipRenderWidget : RenderObjectWidget {
        public _ChipRenderWidget(
            Key key = null,
            _ChipRenderTheme theme = null,
            bool? value = null,
            bool? isEnabled = null,
            Animation<float> checkmarkAnimation = null,
            Animation<float> avatarDrawerAnimation = null,
            Animation<float> deleteDrawerAnimation = null,
            Animation<float> enableAnimation = null,
            ShapeBorder avatarBorder = null
        ) : base(key: key) {
            D.assert(theme != null);
            this.theme = theme;
            this.value = value;
            this.isEnabled = isEnabled;
            this.checkmarkAnimation = checkmarkAnimation;
            this.avatarDrawerAnimation = avatarDrawerAnimation;
            this.deleteDrawerAnimation = deleteDrawerAnimation;
            this.enableAnimation = enableAnimation;
            this.avatarBorder = avatarBorder;
        }

        public readonly _ChipRenderTheme theme;
        public readonly bool? value;
        public readonly bool? isEnabled;
        public readonly Animation<float> checkmarkAnimation;
        public readonly Animation<float> avatarDrawerAnimation;
        public readonly Animation<float> deleteDrawerAnimation;
        public readonly Animation<float> enableAnimation;
        public readonly ShapeBorder avatarBorder;

        public override Element createElement() {
            return new _RenderChipElement(this);
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderChip renderObject = _renderObject as _RenderChip;
            renderObject.theme = theme;
            renderObject.textDirection = Directionality.of(context);
            renderObject.value = value ?? false;
            renderObject.isEnabled = isEnabled ?? false;
            renderObject.checkmarkAnimation = checkmarkAnimation;
            renderObject.avatarDrawerAnimation = avatarDrawerAnimation;
            renderObject.deleteDrawerAnimation = deleteDrawerAnimation;
            renderObject.enableAnimation = enableAnimation;
            renderObject.avatarBorder = avatarBorder;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderChip(
                theme: theme,
                textDirection: Directionality.of(context),
                value: value,
                isEnabled: isEnabled,
                checkmarkAnimation: checkmarkAnimation,
                avatarDrawerAnimation: avatarDrawerAnimation,
                deleteDrawerAnimation: deleteDrawerAnimation,
                enableAnimation: enableAnimation,
                avatarBorder: avatarBorder
            );
        }
    }

    enum _ChipSlot {
        label,
        avatar,
        deleteIcon
    }

    class _RenderChipElement : RenderObjectElement {
        public _RenderChipElement(_ChipRenderWidget chip) : base(chip) {
        }

        Dictionary<_ChipSlot, Element> slotToChild = new Dictionary<_ChipSlot, Element> { };
        Dictionary<Element, _ChipSlot> childToSlot = new Dictionary<Element, _ChipSlot> { };

        public new _ChipRenderWidget widget {
            get { return (_ChipRenderWidget) base.widget; }
        }

        public new _RenderChip renderObject {
            get { return (_RenderChip) base.renderObject; }
        }

        public override void visitChildren(ElementVisitor visitor) {
            slotToChild.Values.Each((value) => { visitor(value); });
        }

        public override void forgetChild(Element child) {
            D.assert(slotToChild.ContainsValue(child));
            D.assert(childToSlot.ContainsKey(child));
            _ChipSlot slot = childToSlot[child];
            childToSlot.Remove(child);
            slotToChild.Remove(slot);
            base.forgetChild(child);
        }

        void _mountChild(Widget widget, _ChipSlot slot) {
            Element oldChild = slotToChild.getOrDefault(slot);
            Element newChild = updateChild(oldChild, widget, slot);
            if (oldChild != null) {
                slotToChild.Remove(slot);
                childToSlot.Remove(oldChild);
            }

            if (newChild != null) {
                slotToChild[slot] = newChild;
                childToSlot[newChild] = slot;
            }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            _mountChild(widget.theme.avatar, _ChipSlot.avatar);
            _mountChild(widget.theme.deleteIcon, _ChipSlot.deleteIcon);
            _mountChild(widget.theme.label, _ChipSlot.label);
        }

        void _updateChild(Widget widget, _ChipSlot slot) {
            Element oldChild = slotToChild[slot];
            Element newChild = updateChild(oldChild, widget, slot);
            if (oldChild != null) {
                childToSlot.Remove(oldChild);
                slotToChild.Remove(slot);
            }

            if (newChild != null) {
                slotToChild[slot] = newChild;
                childToSlot[newChild] = slot;
            }
        }

        public override void update(Widget _newWidget) {
            _ChipRenderWidget newWidget = _newWidget as _ChipRenderWidget;
            base.update(newWidget);
            D.assert(widget == newWidget);
            _updateChild(widget.theme.label, _ChipSlot.label);
            _updateChild(widget.theme.avatar, _ChipSlot.avatar);
            _updateChild(widget.theme.deleteIcon, _ChipSlot.deleteIcon);
        }

        void _updateRenderObject(RenderObject child, _ChipSlot slot) {
            switch (slot) {
                case _ChipSlot.avatar:
                    renderObject.avatar = (RenderBox) child;
                    break;
                case _ChipSlot.label:
                    renderObject.label = (RenderBox) child;
                    break;
                case _ChipSlot.deleteIcon:
                    renderObject.deleteIcon = (RenderBox) child;
                    break;
            }
        }

        protected override void insertChildRenderObject(RenderObject child, object slotValue) {
            D.assert(child is RenderBox);
            D.assert(slotValue is _ChipSlot);
            _ChipSlot slot = (_ChipSlot) slotValue;
            _updateRenderObject(child, slot);
            D.assert(renderObject.childToSlot.ContainsKey((RenderBox) child));
            D.assert(renderObject.slotToChild.ContainsKey(slot));
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(child is RenderBox);
            D.assert(renderObject.childToSlot.ContainsKey((RenderBox) child));
            _updateRenderObject(null, renderObject.childToSlot[(RenderBox) child]);
            D.assert(!renderObject.childToSlot.ContainsKey((RenderBox) child));
            D.assert(!renderObject.slotToChild.ContainsKey((_ChipSlot) slot));
        }

        protected override void moveChildRenderObject(RenderObject child, object slotValue) {
            D.assert(false, () => "not reachable");
        }
    }

    class _ChipRenderTheme {
        public _ChipRenderTheme(
            Widget avatar = null,
            Widget label = null,
            Widget deleteIcon = null,
            Brightness? brightness = null,
            EdgeInsets padding = null,
            VisualDensity visualDensity = null,
            EdgeInsets labelPadding = null,
            bool? showAvatar = null,
            bool? showCheckmark = null,
            Color checkmarkColor = null,
            bool? canTapBody = null
        ) {
            this.avatar = avatar;
            this.label = label;
            this.deleteIcon = deleteIcon;
            this.brightness = brightness;
            this.padding = padding;
            this.visualDensity = visualDensity;
            this.labelPadding = labelPadding;
            this.showAvatar = showAvatar;
            this.showCheckmark = showCheckmark;
            this.checkmarkColor = checkmarkColor;
            this.canTapBody = canTapBody;
        }

        public readonly Widget avatar;
        public readonly Widget label;
        public readonly Widget deleteIcon;
        public readonly Brightness? brightness;
        public readonly EdgeInsets padding;
        public readonly VisualDensity visualDensity;
        public readonly EdgeInsets labelPadding;
        public readonly bool? showAvatar;
        public readonly bool? showCheckmark;
        public readonly Color checkmarkColor;
        public readonly bool? canTapBody;

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((_ChipRenderTheme) obj);
        }

        public bool Equals(_ChipRenderTheme other) {
            return avatar == other.avatar
                   && label == other.label
                   && deleteIcon == other.deleteIcon
                   && brightness == other.brightness
                   && padding == other.padding
                   && labelPadding == other.labelPadding
                   && showAvatar == other.showAvatar
                   && showCheckmark == other.showCheckmark
                   && checkmarkColor == other.checkmarkColor
                   && canTapBody == other.canTapBody;
        }

        public static bool operator ==(_ChipRenderTheme left, _ChipRenderTheme right) {
            return Equals(left, right);
        }

        public static bool operator !=(_ChipRenderTheme left, _ChipRenderTheme right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            var hashCode = avatar.GetHashCode();
            hashCode = (hashCode * 397) ^ label.GetHashCode();
            hashCode = (hashCode * 397) ^ deleteIcon.GetHashCode();
            hashCode = (hashCode * 397) ^ brightness.GetHashCode();
            hashCode = (hashCode * 397) ^ padding.GetHashCode();
            hashCode = (hashCode * 397) ^ labelPadding.GetHashCode();
            hashCode = (hashCode * 397) ^ showAvatar.GetHashCode();
            hashCode = (hashCode * 397) ^ showCheckmark.GetHashCode();
            hashCode = (hashCode * 397) ^ checkmarkColor.GetHashCode();
            hashCode = (hashCode * 397) ^ canTapBody.GetHashCode();
            return hashCode;
        }
    }

    class _RenderChip : RenderBox {
        public _RenderChip(
            _ChipRenderTheme theme = null,
            TextDirection textDirection = TextDirection.ltr,
            bool? value = null,
            bool? isEnabled = null,
            Animation<float> checkmarkAnimation = null,
            Animation<float> avatarDrawerAnimation = null,
            Animation<float> deleteDrawerAnimation = null,
            Animation<float> enableAnimation = null,
            ShapeBorder avatarBorder = null
        ) {
            D.assert(theme != null);
            _theme = theme;
            this.textDirection = textDirection;
            checkmarkAnimation.addListener(markNeedsPaint);
            avatarDrawerAnimation.addListener(markNeedsLayout);
            deleteDrawerAnimation.addListener(markNeedsLayout);
            enableAnimation.addListener(markNeedsPaint);
            this.value = value;
            this.isEnabled = isEnabled;
            this.checkmarkAnimation = checkmarkAnimation;
            this.avatarDrawerAnimation = avatarDrawerAnimation;
            this.deleteDrawerAnimation = deleteDrawerAnimation;
            this.enableAnimation = enableAnimation;
            this.avatarBorder = avatarBorder;
        }

        public Dictionary<_ChipSlot, RenderBox> slotToChild = new Dictionary<_ChipSlot, RenderBox> { };
        public Dictionary<RenderBox, _ChipSlot> childToSlot = new Dictionary<RenderBox, _ChipSlot> { };

        public bool? value;
        public bool? isEnabled;
        public Rect deleteButtonRect;
        public Rect pressRect;
        public Animation<float> checkmarkAnimation;
        public Animation<float> avatarDrawerAnimation;
        public Animation<float> deleteDrawerAnimation;
        public Animation<float> enableAnimation;
        public ShapeBorder avatarBorder;

        RenderBox _updateChild(RenderBox oldChild, RenderBox newChild, _ChipSlot slot) {
            if (oldChild != null) {
                dropChild(oldChild);
                childToSlot.Remove(oldChild);
                slotToChild.Remove(slot);
            }

            if (newChild != null) {
                childToSlot[newChild] = slot;
                slotToChild[slot] = newChild;
                adoptChild(newChild);
            }

            return newChild;
        }

        RenderBox _avatar;

        public RenderBox avatar {
            get { return _avatar; }
            set { _avatar = _updateChild(_avatar, value, _ChipSlot.avatar); }
        }

        RenderBox _deleteIcon;

        public RenderBox deleteIcon {
            get { return _deleteIcon; }
            set { _deleteIcon = _updateChild(_deleteIcon, value, _ChipSlot.deleteIcon); }
        }

        RenderBox _label;

        public RenderBox label {
            get { return _label; }
            set { _label = _updateChild(_label, value, _ChipSlot.label); }
        }

        public _ChipRenderTheme theme {
            get { return _theme; }
            set {
                if (_theme == value) {
                    return;
                }

                _theme = value;
                markNeedsLayout();
            }
        }

        _ChipRenderTheme _theme;

        IEnumerable<RenderBox> _children {
            get {
                if (avatar != null) {
                    yield return avatar;
                }

                if (label != null) {
                    yield return label;
                }

                if (deleteIcon != null) {
                    yield return deleteIcon;
                }
            }
        }

        public TextDirection textDirection {
            get => _textDirection;
            set {
                if (_textDirection == value) {
                    return;
                }

                _textDirection = value;
                markNeedsLayout();
            }
        }

        TextDirection _textDirection;

        public bool isDrawingCheckmark {
            get { return theme.showCheckmark == true && (!(checkmarkAnimation?.isDismissed ?? !value)) == true; }
        }

        public bool deleteIconShowing {
            get { return !deleteDrawerAnimation.isDismissed; }
        }

        public override void attach(object _owner) {
            PipelineOwner owner = _owner as PipelineOwner;
            base.attach(owner);
            foreach (RenderBox child in _children) {
                child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            foreach (RenderBox child in _children) {
                child.detach();
            }
        }

        public override void redepthChildren() {
            _children.Each(redepthChild);
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            _children.Each((value) => { visitor(value); });
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            List<DiagnosticsNode> value = new List<DiagnosticsNode> { };

            void add(RenderBox child, string name) {
                if (child != null) {
                    value.Add(child.toDiagnosticsNode(name: name));
                }
            }

            add(avatar, "avatar");
            add(label, "label");
            add(deleteIcon, "deleteIcon");
            return value;
        }

        protected override bool sizedByParent {
            get { return false; }
        }

        static float _minWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMinIntrinsicWidth(height);
        }

        static float _maxWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMaxIntrinsicWidth(height);
        }

        static float _minHeight(RenderBox box, float width) {
            return box == null ? 0.0f : box.getMinIntrinsicHeight(width);
        }

        static Size _boxSize(RenderBox box) {
            return box == null ? Size.zero : box.size;
        }

        static Rect _boxRect(RenderBox box) {
            return box == null ? Rect.zero : _boxParentData(box).offset & box.size;
        }

        static BoxParentData _boxParentData(RenderBox box) {
            return (BoxParentData) box.parentData;
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            float overallPadding = theme.padding.horizontal + theme.labelPadding.horizontal;
            return overallPadding +
                   _minWidth(avatar, height) +
                   _minWidth(label, height) +
                   _minWidth(deleteIcon, height);
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            float overallPadding = theme.padding.vertical + theme.labelPadding.horizontal;
            return overallPadding +
                   _maxWidth(avatar, height) +
                   _maxWidth(label, height) +
                   _maxWidth(deleteIcon, height);
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            return Mathf.Max(
                ChipUtils._kChipHeight,
                theme.padding.vertical + theme.labelPadding.vertical + _minHeight(label, width)
            );
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return computeMinIntrinsicHeight(width);
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return label.getDistanceToActualBaseline(baseline);
        }

        Size _layoutLabel(float iconSizes, Size size) {
            Size rawSize = _boxSize(label);
            if (constraints.maxWidth.isFinite()) {
                float maxWidth = Mathf.Max(
                    0.0f,
                    constraints.maxWidth
                    - iconSizes
                    - theme.labelPadding.horizontal
                    - theme.padding.horizontal
                );
                label.layout(constraints.copyWith(
                        minWidth: 0.0f,
                        maxWidth: maxWidth,
                        minHeight: rawSize.height,
                        maxHeight: size.height
                    ),
                    parentUsesSize: true
                );
                Size updatedSize = _boxSize(label);
                return new Size(
                    updatedSize.width + theme.labelPadding.horizontal,
                    updatedSize.height + theme.labelPadding.vertical
                );
            }

            label.layout(
                new BoxConstraints(
                    minHeight: rawSize.height,
                    maxHeight: size.height,
                    minWidth: 0.0f,
                    maxWidth: size.width
                ),
                parentUsesSize: true
            );

            return new Size(
                rawSize.width + theme.labelPadding.horizontal,
                rawSize.height + theme.labelPadding.vertical
            );
        }

        Size _layoutAvatar(BoxConstraints contentConstraints, float contentSize) {
            float requestedSize = Mathf.Max(0.0f, contentSize);
            BoxConstraints avatarConstraints = BoxConstraints.tightFor(
                width: requestedSize,
                height: requestedSize
            );
            avatar.layout(avatarConstraints, parentUsesSize: true);
            if (theme.showCheckmark != true && theme.showAvatar != true) {
                return new Size(0.0f, contentSize);
            }

            float avatarWidth = 0.0f;
            float avatarHeight = 0.0f;
            Size avatarBoxSize = _boxSize(avatar);
            if (theme.showAvatar == true) {
                avatarWidth += avatarDrawerAnimation.value * avatarBoxSize.width;
            }
            else {
                avatarWidth += avatarDrawerAnimation.value * contentSize;
            }

            avatarHeight += avatarBoxSize.height;
            return new Size(avatarWidth, avatarHeight);
        }

        Size _layoutDeleteIcon(BoxConstraints contentConstraints, float contentSize) {
            float requestedSize = Mathf.Max(0.0f, contentSize);
            BoxConstraints deleteIconConstraints = BoxConstraints.tightFor(
                width: requestedSize,
                height: requestedSize
            );
            deleteIcon.layout(deleteIconConstraints, parentUsesSize: true);
            if (!deleteIconShowing) {
                return new Size(0.0f, contentSize);
            }

            float deleteIconWidth = 0.0f;
            float deleteIconHeight = 0.0f;
            Size boxSize = _boxSize(deleteIcon);
            deleteIconWidth += deleteDrawerAnimation.value * boxSize.width;
            deleteIconHeight += boxSize.height;
            return new Size(deleteIconWidth, deleteIconHeight);
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            if (!size.contains(position)) {
                return false;
            }

            bool tapIsOnDeleteIcon = material_._tapIsOnDeleteIcon(
                hasDeleteButton: deleteIcon != null,
                tapPosition: position,
                chipSize: size,
                textDirection: textDirection
            );
            RenderBox hitTestChild = tapIsOnDeleteIcon
                ? (deleteIcon ?? label ?? avatar)
                : (label ?? avatar);

            if (hitTestChild != null) {
                Offset center = hitTestChild.size.center(Offset.zero);
                return result.addWithRawTransform(
                    transform: MatrixUtils.forceToPoint(center),
                    position: position,
                    hitTest: (BoxHitTestResult _result, Offset _position) => {
                        D.assert(_position == center);
                        return hitTestChild.hitTest(_result, position: center);
                    }
                );
            }

            return false;
        }

        protected override void performLayout() {
            BoxConstraints contentConstraints = constraints.loosen();
            Offset densityAdjustment = new Offset(0.0f, theme.visualDensity.baseSizeAdjustment.dy / 2.0f);
            label.layout(contentConstraints, parentUsesSize: true);
            float contentSize = Mathf.Max(
                ChipUtils._kChipHeight - theme.padding.vertical + theme.labelPadding.vertical,
                _boxSize(label).height + theme.labelPadding.vertical
            );
            Size avatarSize = _layoutAvatar(contentConstraints, contentSize);
            Size deleteIconSize = _layoutDeleteIcon(contentConstraints, contentSize);
            Size labelSize = new Size(_boxSize(label).width, contentSize);
            labelSize = _layoutLabel(avatarSize.width + deleteIconSize.width, labelSize);

            Size overallSize = new Size(
                avatarSize.width + labelSize.width + deleteIconSize.width,
                contentSize
            ) + densityAdjustment;


            const float left = 0.0f;
            float right = overallSize.width;

            Offset centerLayout(Size boxSize, float x) {
                D.assert(contentSize >= boxSize.height);
                Offset boxOffset = null;
                switch (textDirection) {
                    case TextDirection.rtl:
                        boxOffset = new Offset(x - boxSize.width,
                            (contentSize - boxSize.height + densityAdjustment.dy) / 2.0f);
                        break;
                    case TextDirection.ltr:
                        boxOffset = new Offset(x, (contentSize - boxSize.height + densityAdjustment.dy) / 2.0f);
                        break;
                }

                return boxOffset;
            }

            Offset avatarOffset = Offset.zero;
            Offset labelOffset = Offset.zero;
            Offset deleteIconOffset = Offset.zero;
            switch (textDirection) {
                case TextDirection.rtl:
                    float startRight = right;
                    if ((theme.showCheckmark ?? false) || (theme.showAvatar ?? false)) {
                        avatarOffset = centerLayout(avatarSize, startRight);
                        startRight -= avatarSize.width;
                    }

                    labelOffset = centerLayout(labelSize, startRight);
                    startRight -= labelSize.width;
                    if (deleteIconShowing) {
                        deleteButtonRect = Rect.fromLTWH(
                            0.0f,
                            0.0f,
                            deleteIconSize.width + theme.padding.right,
                            overallSize.height + theme.padding.vertical
                        );
                        deleteIconOffset = centerLayout(deleteIconSize, startRight);
                    }
                    else {
                        deleteButtonRect = Rect.zero;
                    }

                    startRight -= deleteIconSize.width;
                    if (theme.canTapBody ?? false) {
                        pressRect = Rect.fromLTWH(
                            deleteButtonRect.width,
                            0.0f,
                            overallSize.width - deleteButtonRect.width + theme.padding.horizontal,
                            overallSize.height + theme.padding.vertical
                        );
                    }
                    else {
                        pressRect = Rect.zero;
                    }

                    break;
                case TextDirection.ltr:
                    float startLeft = left;
                    if ((theme.showCheckmark ?? false) || (theme.showAvatar ?? false)) {
                        avatarOffset = centerLayout(avatarSize, startLeft - _boxSize(avatar).width + avatarSize.width);
                        startLeft += avatarSize.width;
                    }

                    labelOffset = centerLayout(labelSize, startLeft);
                    startLeft += labelSize.width;
                    if (theme.canTapBody ?? false) {
                        pressRect = Rect.fromLTWH(
                            0.0f,
                            0.0f,
                            deleteIconShowing
                                ? startLeft + theme.padding.left
                                : overallSize.width + theme.padding.horizontal,
                            overallSize.height + theme.padding.vertical
                        );
                    }
                    else {
                        pressRect = Rect.zero;
                    }

                    startLeft -= _boxSize(deleteIcon).width - deleteIconSize.width;
                    if (deleteIconShowing) {
                        deleteIconOffset = centerLayout(deleteIconSize, startLeft);
                        deleteButtonRect = Rect.fromLTWH(
                            startLeft + theme.padding.left,
                            0.0f,
                            deleteIconSize.width + theme.padding.right,
                            overallSize.height + theme.padding.vertical
                        );
                    }
                    else {
                        deleteButtonRect = Rect.zero;
                    }

                    break;
            }

            labelOffset = labelOffset +
                          new Offset(
                              0.0f,
                              ((labelSize.height - theme.labelPadding.vertical) - _boxSize(label).height) / 2.0f
                          );
            _boxParentData(avatar).offset = theme.padding.topLeft + avatarOffset;
            _boxParentData(label).offset =
                theme.padding.topLeft + labelOffset + theme.labelPadding.topLeft;
            _boxParentData(deleteIcon).offset = theme.padding.topLeft + deleteIconOffset;
            Size paddedSize = new Size(
                overallSize.width + theme.padding.horizontal,
                overallSize.height + theme.padding.vertical
            );
            size = constraints.constrain(paddedSize);
            D.assert(size.height == constraints.constrainHeight(paddedSize.height),
                () => $"Constrained height {size.height} doesn't match expected height " +
                      $"{constraints.constrainWidth(paddedSize.height)}");
            D.assert(size.width == constraints.constrainWidth(paddedSize.width),
                () => $"Constrained width {size.width} doesn't match expected width " +
                      $"{constraints.constrainWidth(paddedSize.width)}");
        }

        static ColorTween selectionScrimTween = new ColorTween(
            begin: Colors.transparent,
            end: ChipUtils._kSelectScrimColor
        );

        Color _disabledColor {
            get {
                if (enableAnimation == null || enableAnimation.isCompleted) {
                    return Colors.white;
                }

                ColorTween enableTween;
                switch (theme.brightness) {
                    case Brightness.light:
                        enableTween = new ColorTween(
                            begin: Colors.white.withAlpha(ChipUtils._kDisabledAlpha),
                            end: Colors.white
                        );
                        break;
                    case Brightness.dark:
                        enableTween = new ColorTween(
                            begin: Colors.black.withAlpha(ChipUtils._kDisabledAlpha),
                            end: Colors.black
                        );
                        break;
                    default:
                        throw new Exception("Unknown brightness: " + theme.brightness);
                }

                return enableTween.evaluate(enableAnimation);
            }
        }

        void _paintCheck(Canvas canvas, Offset origin, float size) {
            Color paintColor;
            if (theme.checkmarkColor != null) {
                paintColor = theme.checkmarkColor;
            }
            else {
                switch (theme.brightness) {
                    case Brightness.light:
                        paintColor = theme.showAvatar == true
                            ? Colors.white
                            : Colors.black.withAlpha(ChipUtils._kCheckmarkAlpha);
                        break;
                    case Brightness.dark:
                        paintColor = theme.showAvatar == true
                            ? Colors.black
                            : Colors.white.withAlpha(ChipUtils._kCheckmarkAlpha);
                        break;
                    default:
                        throw new Exception("Unknown brightness: " + theme.brightness);
                }
            }

            ColorTween fadeTween = new ColorTween(begin: Colors.transparent, end: paintColor);

            paintColor = checkmarkAnimation.status == AnimationStatus.reverse
                ? fadeTween.evaluate(checkmarkAnimation)
                : paintColor;

            Paint paint = new Paint();
            paint.color = paintColor;
            paint.style = PaintingStyle.stroke;
            paint.strokeWidth = ChipUtils._kCheckmarkStrokeWidth *
                                (avatar != null ? avatar.size.height / 24.0f : 1.0f);
            float t = checkmarkAnimation.status == AnimationStatus.reverse
                ? 1.0f
                : checkmarkAnimation.value;
            if (t == 0.0f) {
                return;
            }

            D.assert(t > 0.0f && t <= 1.0f);
            Path path = new Path();
            Offset start = new Offset(size * 0.15f, size * 0.45f);
            Offset mid = new Offset(size * 0.4f, size * 0.7f);
            Offset end = new Offset(size * 0.85f, size * 0.25f);
            if (t < 0.5f) {
                float strokeT = t * 2.0f;
                Offset drawMid = Offset.lerp(start, mid, strokeT);
                path.moveTo(origin.dx + start.dx, origin.dy + start.dy);
                path.lineTo(origin.dx + drawMid.dx, origin.dy + drawMid.dy);
            }
            else {
                float strokeT = (t - 0.5f) * 2.0f;
                Offset drawEnd = Offset.lerp(mid, end, strokeT);
                path.moveTo(origin.dx + start.dx, origin.dy + start.dy);
                path.lineTo(origin.dx + mid.dx, origin.dy + mid.dy);
                path.lineTo(origin.dx + drawEnd.dx, origin.dy + drawEnd.dy);
            }

            canvas.drawPath(path, paint);
        }

        void _paintSelectionOverlay(PaintingContext context, Offset offset) {
            if (isDrawingCheckmark) {
                if (theme.showAvatar == true) {
                    Rect avatarRect = _boxRect(avatar).shift(offset);
                    Paint darkenPaint = new Paint();
                    darkenPaint.color = selectionScrimTween.evaluate(checkmarkAnimation);
                    darkenPaint.blendMode = BlendMode.srcATop;
                    Path path = avatarBorder.getOuterPath(avatarRect);
                    context.canvas.drawPath(path, darkenPaint);
                }

                float checkSize = avatar.size.height * 0.75f;
                Offset checkOffset = _boxParentData(avatar).offset +
                                     new Offset(avatar.size.height * 0.125f, avatar.size.height * 0.125f);
                _paintCheck(context.canvas, offset + checkOffset, checkSize);
            }
        }

        void _paintAvatar(PaintingContext context, Offset offset) {
            void paintWithOverlay(PaintingContext _context, Offset _offset) {
                _context.paintChild(avatar, _boxParentData(avatar).offset + _offset);
                _paintSelectionOverlay(_context, _offset);
            }

            if (theme.showAvatar == false && avatarDrawerAnimation.isDismissed) {
                return;
            }

            Color disabledColor = _disabledColor;
            int disabledColorAlpha = disabledColor.alpha;
            if (needsCompositing) {
                context.pushLayer(new OpacityLayer(alpha: disabledColorAlpha), paintWithOverlay, offset);
            }
            else {
                Paint _paint = new Paint();
                _paint.color = _disabledColor;
                if (disabledColorAlpha != 0xff) {
                    context.canvas.saveLayer(_boxRect(avatar).shift(offset).inflate(20.0f), _paint);
                }

                paintWithOverlay(context, offset);
                if (disabledColorAlpha != 0xff) {
                    context.canvas.restore();
                }
            }
        }

        void _paintChild(PaintingContext context, Offset offset, RenderBox child, bool isEnabled) {
            if (child == null) {
                return;
            }

            int disabledColorAlpha = _disabledColor.alpha;
            if (!enableAnimation.isCompleted) {
                if (needsCompositing) {
                    context.pushLayer(
                        new OpacityLayer(alpha: disabledColorAlpha),
                        (PaintingContext _context, Offset _offset) => {
                            _context.paintChild(child, _boxParentData(child).offset + _offset);
                        },
                        offset
                    );
                }
                else {
                    Rect childRect = _boxRect(child).shift(offset);
                    Paint _paint = new Paint();
                    _paint.color = _disabledColor;
                    context.canvas.saveLayer(childRect.inflate(20.0f), _paint);
                    context.paintChild(child, _boxParentData(child).offset + offset);
                    context.canvas.restore();
                }
            }
            else {
                context.paintChild(child, _boxParentData(child).offset + offset);
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            _paintAvatar(context, offset);
            if (deleteIconShowing) {
                _paintChild(context, offset, deleteIcon, isEnabled == true);
            }

            _paintChild(context, offset, label, isEnabled == true);
        }

        const bool _debugShowTapTargetOutlines = false;

        public override void debugPaint(PaintingContext context, Offset offset) {
            bool visualizeTapTargets() {
                Paint outlinePaint = new Paint();
                outlinePaint.color = new Color(0xff800000);
                outlinePaint.strokeWidth = 1.0f;
                outlinePaint.style = PaintingStyle.stroke;
                if (deleteIconShowing) {
                    context.canvas.drawRect(deleteButtonRect.shift(offset), outlinePaint);
                }

                outlinePaint.color = new Color(0xff008000);
                context.canvas.drawRect(pressRect.shift(offset), outlinePaint);
                return true;
            }

            D.assert(!_debugShowTapTargetOutlines || visualizeTapTargets());
        }

        protected override bool hitTestSelf(Offset position) {
            return deleteButtonRect.contains(position) || pressRect.contains(position);
        }
    }

    class _LocationAwareInkRippleFactory : InteractiveInkFeatureFactory {
        internal _LocationAwareInkRippleFactory(
            bool? hasDeleteButton = null,
            BuildContext chipContext = null,
            GlobalKey deleteIconKey = null
        ) {
            this.hasDeleteButton = hasDeleteButton;
            this.chipContext = chipContext;
            this.deleteIconKey = deleteIconKey;
        }

        public readonly bool? hasDeleteButton;
        public readonly BuildContext chipContext;
        public readonly GlobalKey deleteIconKey;

        public override InteractiveInkFeature create(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Offset position = null,
            Color color = null,
            TextDirection? textDirection = null,
            bool containedInkWell = false,
            RectCallback rectCallback = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            float? radius = null,
            VoidCallback onRemoved = null
        ) {
            bool tapIsOnDeleteIcon = material_._tapIsOnDeleteIcon(
                hasDeleteButton: hasDeleteButton ?? false,
                tapPosition: position,
                chipSize: chipContext.size,
                textDirection: textDirection
            );

            BuildContext splashContext = tapIsOnDeleteIcon
                ? deleteIconKey.currentContext
                : chipContext;

            InteractiveInkFeatureFactory splashFactory = Theme.of(splashContext).splashFactory;

            if (tapIsOnDeleteIcon) {
                RenderBox currentBox = referenceBox;
                referenceBox = deleteIconKey.currentContext.findRenderObject() as RenderBox;
                position = referenceBox.globalToLocal(currentBox.localToGlobal(position));
                containedInkWell = false;
            }

            return splashFactory.create(
                controller: controller,
                referenceBox: referenceBox,
                position: position,
                color: color,
                textDirection: textDirection,
                containedInkWell: containedInkWell,
                rectCallback: rectCallback,
                borderRadius: borderRadius,
                customBorder: customBorder,
                radius: radius,
                onRemoved: onRemoved
            );
        }
    }

    public partial class material_ {
        internal static bool _tapIsOnDeleteIcon(
            bool hasDeleteButton = false,
            Offset tapPosition = null,
            Size chipSize = null,
            TextDirection? textDirection = null
        ) {
            bool tapIsOnDeleteIcon = false;
            if (!hasDeleteButton) {
                tapIsOnDeleteIcon = false;
            }
            else {
                switch (textDirection) {
                    case TextDirection.ltr:
                        tapIsOnDeleteIcon = tapPosition.dx / chipSize.width > 0.66;
                        break;
                    case TextDirection.rtl:
                        tapIsOnDeleteIcon = tapPosition.dx / chipSize.width < 0.33;
                        break;
                }
            }

            return tapIsOnDeleteIcon;
        }
    }
}