using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public partial class material_ {
        public delegate void DataColumnSortCallback(int columnIndex, bool ascending);
    }

    public class DataColumn {
        public DataColumn(
            Widget label,
            string tooltip = null,
            bool numeric = false,
            material_.DataColumnSortCallback onSort = null
        ) {
            D.assert(label != null);
            this.label = label;
            this.tooltip = tooltip;
            this.numeric = numeric;
            this.onSort = onSort;
        }

        public readonly Widget label;

        public readonly string tooltip;

        public readonly bool numeric;

        public readonly material_.DataColumnSortCallback onSort;

        internal bool _debugInteractive {
            get { return onSort != null; }
        }
    }

    public class DataRow {
        public DataRow(
            LocalKey key = null,
            bool selected = false,
            ValueChanged<bool> onSelectChanged = null,
            List<DataCell> cells = null
        ) {
            D.assert(cells != null);
            this.key = key;
            this.selected = selected;
            this.onSelectChanged = onSelectChanged;
            this.cells = cells;
        }

        public static DataRow byIndex(
            int index = 0,
            bool selected = false,
            ValueChanged<bool> onSelectChanged = null,
            List<DataCell> cells = null
        ) {
            D.assert(cells != null);
            return new DataRow(
                new ValueKey<int>(index),
                selected,
                onSelectChanged,
                cells
            );
        }


        public readonly LocalKey key;

        public readonly ValueChanged<bool> onSelectChanged;

        public readonly bool selected;

        public readonly List<DataCell> cells;

        internal bool _debugInteractive {
            get { return onSelectChanged != null || cells.Any((DataCell cell) => cell._debugInteractive); }
        }
    }


    public class DataCell {
        public DataCell(
            Widget child,
            bool placeholder = false,
            bool showEditIcon = false,
            VoidCallback onTap = null
        ) {
            D.assert(child != null);
            this.child = child;
            this.placeholder = placeholder;
            this.showEditIcon = showEditIcon;
            this.onTap = onTap;
        }

        public static readonly DataCell empty = new DataCell(new Container(width: 0.0f, height: 0.0f));

        public readonly Widget child;

        public readonly bool placeholder;

        public readonly bool showEditIcon;

        public readonly VoidCallback onTap;

        internal bool _debugInteractive {
            get { return onTap != null; }
        }
    }

    public class DataTable : StatelessWidget {
        public DataTable(
            Key key = null,
            List<DataColumn> columns = null,
            int? sortColumnIndex = 0,
            bool sortAscending = true,
            ValueSetter<bool> onSelectAll = null,
            float dataRowHeight = material_.kMinInteractiveDimension,
            float headingRowHeight = 56.0f,
            float horizontalMargin = 24.0f,
            float columnSpacing = 56.0f,
            bool showCheckboxColumn = true,
            float dividerThickness = 1.0f,
            List<DataRow> rows = null
        ) : base(key: key) {
            D.assert(columns != null);
            D.assert(columns.isNotEmpty);
            D.assert(sortColumnIndex == null || (sortColumnIndex >= 0 && sortColumnIndex < columns.Count));
            D.assert(rows != null);
            D.assert(!rows.Any((DataRow row) => row.cells.Count != columns.Count));
            D.assert(dividerThickness >= 0);
            this.columns = columns;
            this.sortColumnIndex = sortColumnIndex;
            this.sortAscending = sortAscending;
            this.onSelectAll = onSelectAll;
            this.dataRowHeight = dataRowHeight;
            this.headingRowHeight = headingRowHeight;
            this.horizontalMargin = horizontalMargin;
            this.columnSpacing = columnSpacing;
            this.showCheckboxColumn = showCheckboxColumn;
            this.dividerThickness = dividerThickness;
            this.rows = rows;
            _onlyTextColumn = _initOnlyTextColumn(columns);
        }

        public readonly List<DataColumn> columns;
        public readonly int? sortColumnIndex;
        public readonly bool sortAscending;
        public readonly ValueSetter<bool> onSelectAll;
        public readonly float dataRowHeight;
        public readonly float headingRowHeight;
        public readonly float horizontalMargin;
        public readonly float columnSpacing;
        public readonly bool showCheckboxColumn;
        public readonly List<DataRow> rows;

        public readonly int? _onlyTextColumn;

        static int? _initOnlyTextColumn(List<DataColumn> columns) {
            int? result = null;
            for (int index = 0; index < columns.Count; index += 1) {
                DataColumn column = columns[index];
                if (!column.numeric) {
                    if (result != null) {
                        return null;
                    }

                    result = index;
                }
            }

            return result;
        }

        bool _debugInteractive {
            get {
                return columns.Any((DataColumn column) => column._debugInteractive)
                       || rows.Any((DataRow row) => row._debugInteractive);
            }
        }

        static readonly LocalKey _headingRowKey = new UniqueKey();

        void _handleSelectAll(bool isChecked) {
            if (onSelectAll != null) {
                onSelectAll(isChecked);
            }
            else {
                foreach (DataRow row in rows) {
                    if ((row.onSelectChanged != null) && (row.selected != isChecked))
                        row.onSelectChanged(isChecked);
                }
            }
        }

        static readonly float _sortArrowPadding = 2.0f;
        static readonly float _headingFontSize = 12.0f;
        static readonly TimeSpan _sortArrowAnimationDuration = new TimeSpan(0, 0, 0, 0, 150);
        static readonly Color _grey100Opacity = new Color(0x0A000000);
        static readonly Color _grey300Opacity = new Color(0x1E000000);
        public readonly float dividerThickness;

        Widget _buildCheckbox(
            Color color = null,
            bool isChecked = false,
            VoidCallback onRowTap = null,
            ValueChanged<bool?> onCheckboxChanged = null
        ) {
            Widget contents = new Padding(
                padding: EdgeInsetsDirectional.only(start: horizontalMargin,
                    end: horizontalMargin / 2.0f),
                child: new Center(
                    child: new Checkbox(
                        activeColor: color,
                        value: isChecked,
                        onChanged: onCheckboxChanged
                    )
                )
            );
            if (onRowTap != null) {
                contents = new TableRowInkWell(
                    onTap: () => onRowTap(),
                    child: contents
                );
            }

            return new TableCell(
                verticalAlignment: TableCellVerticalAlignment.fill,
                child: contents
            );
        }

        Widget _buildHeadingCell(
            BuildContext context = null,
            EdgeInsetsGeometry padding = null,
            Widget label = null,
            string tooltip = null,
            bool? numeric = null,
            VoidCallback onSort = null,
            bool? sorted = null,
            bool? ascending = null
        ) {
            List<Widget> arrowWithPadding() {
                return onSort == null
                    ? new List<Widget>()
                    : new List<
                        Widget>() {
                        new _SortArrow(
                            visible: sorted,
                            down: sorted ?? false ? ascending : null,
                            duration: _sortArrowAnimationDuration
                        ),

                        new SizedBox(width: _sortArrowPadding)
                    };
            }

            var rowChild = new List<Widget>();
            rowChild.Add(label);
            rowChild.AddRange(arrowWithPadding());
            label = new Row(
                textDirection: numeric ?? false ? TextDirection.rtl : (TextDirection?) null,
                children: rowChild
            );
            label = new Container(
                padding: padding,
                height: headingRowHeight,
                alignment: numeric ?? false
                    ? Alignment.centerRight
                    : (AlignmentGeometry) AlignmentDirectional.centerStart,
                child: new AnimatedDefaultTextStyle(
                    style: new TextStyle(
                        fontWeight: FontWeight.w500,
                        fontSize: _headingFontSize,
                        height: Mathf.Min(1.0f, headingRowHeight / _headingFontSize),
                        color: (Theme.of(context).brightness == Brightness.light)
                            ? ((onSort != null && (sorted ?? false)) ? Colors.black87 : Colors.black54)
                            : ((onSort != null && (sorted ?? false)) ? Colors.white : Colors.white70)
                    ),
                    softWrap: false,
                    duration: _sortArrowAnimationDuration,
                    child: label
                )
            );
            if (tooltip != null) {
                label = new Tooltip(
                    message: tooltip,
                    child: label
                );
            }

            // TODO(dkwingsmt): Only wrap Inkwell if onSort != null. Blocked by
            // https://github.com/flutter/flutter/issues/51152
            label = new InkWell(
                onTap: () => onSort?.Invoke(),
                child: label
            );
            return label;
        }

        Widget _buildDataCell(
            BuildContext context,
            EdgeInsetsGeometry padding,
            Widget label,
            bool numeric,
            bool placeholder,
            bool showEditIcon,
            VoidCallback onTap,
            VoidCallback onSelectChanged
        ) {
            bool isLightTheme = Theme.of(context).brightness == Brightness.light;
            if (showEditIcon) {
                Widget icon = new Icon(Icons.edit, size: 18.0f);
                label = new Expanded(child: label);
                label = new Row(
                    textDirection: numeric ? TextDirection.rtl : (TextDirection?) null,
                    children: new List<Widget> {label, icon}
                );
            }

            label = new Container(
                padding: padding,
                height: dataRowHeight,
                alignment: numeric ? Alignment.centerRight : (AlignmentGeometry) AlignmentDirectional.centerStart,
                child: new DefaultTextStyle(
                    style: new TextStyle(
                        // TODO(ianh): font family should be Roboto; see https://github.com/flutter/flutter/issues/3116
                        fontSize: 13.0f,
                        color: isLightTheme
                            ? (placeholder ? Colors.black38 : Colors.black87)
                            : (placeholder ? Colors.white38 : Colors.white70)
                    ),
                    child: IconTheme.merge(
                        data: new IconThemeData(
                            color: isLightTheme ? Colors.black54 : Colors.white70
                        ),
                        child: new DropdownButtonHideUnderline(child: label)
                    )
                )
            );
            if (onTap != null) {
                label = new InkWell(
                    onTap: () => onTap(),
                    child: label
                );
            }
            else if (onSelectChanged != null) {
                label = new TableRowInkWell(
                    onTap: () => onSelectChanged(),
                    child: label
                );
            }

            return label;
        }

        public override Widget build(BuildContext context) {
            D.assert(!_debugInteractive || material_.debugCheckHasMaterial(context));

            ThemeData theme = Theme.of(context);
            BoxDecoration _kSelectedDecoration = new BoxDecoration(
                border: new Border(bottom: Divider.createBorderSide(context, width: dividerThickness)),
                // The backgroundColor has to be transparent so you can see the ink on the material
                color: (Theme.of(context).brightness == Brightness.light) ? _grey100Opacity : _grey300Opacity
            );
            BoxDecoration _kUnselectedDecoration = new BoxDecoration(
                border: new Border(bottom: Divider.createBorderSide(context, width: dividerThickness))
            );

            bool displayCheckboxColumn =
                showCheckboxColumn && rows.Any((DataRow row) => row.onSelectChanged != null);
            bool allChecked = displayCheckboxColumn &&
                              !rows.Any((DataRow row) => row.onSelectChanged != null && !row.selected);

            List<TableColumnWidth> tableColumns =
                new List<TableColumnWidth>(new TableColumnWidth[columns.Count + (displayCheckboxColumn ? 1 : 0)]);
            
            List<TableRow> tableRows = LinqUtils<TableRow,int>.SelectList(Enumerable.Range(0, rows.Count + 1), (index) => {
                    return new TableRow(
                        key: index == 0 ? _headingRowKey : rows[index - 1].key,
                        decoration: index > 0 && rows[index - 1].selected
                            ? _kSelectedDecoration
                            : _kUnselectedDecoration,
                        children: new List<Widget>(new Widget[tableColumns.Count])
                    );
                });

            int rowIndex;

            int displayColumnIndex = 0;
            if (displayCheckboxColumn) {
                tableColumns[0] = new FixedColumnWidth(horizontalMargin + Checkbox.width + horizontalMargin / 2.0f);
                tableRows[0].children[0] = _buildCheckbox(
                    color: theme.accentColor,
                    isChecked: allChecked,
                    onCheckboxChanged: _check => _handleSelectAll(_check ?? false)
                );
                rowIndex = 1;
                foreach (DataRow row in rows) {
                    tableRows[rowIndex].children[0] = _buildCheckbox(
                        color: theme.accentColor,
                        isChecked: row.selected,
                        onRowTap: () => {
                            if (row.onSelectChanged != null) {
                                row.onSelectChanged(!row.selected);
                            }
                        },
                        onCheckboxChanged: _select => row.onSelectChanged(_select ?? false)
                    );
                    rowIndex += 1;
                }

                displayColumnIndex += 1;
            }

            for (int dataColumnIndex = 0; dataColumnIndex < columns.Count; dataColumnIndex += 1) {
                DataColumn column = columns[dataColumnIndex];

                float paddingStart;
                if (dataColumnIndex == 0 && displayCheckboxColumn) {
                    paddingStart = horizontalMargin / 2.0f;
                }
                else if (dataColumnIndex == 0 && !displayCheckboxColumn) {
                    paddingStart = horizontalMargin;
                }
                else {
                    paddingStart = columnSpacing / 2.0f;
                }

                float paddingEnd;
                if (dataColumnIndex == columns.Count - 1) {
                    paddingEnd = horizontalMargin;
                }
                else {
                    paddingEnd = columnSpacing / 2.0f;
                }

                EdgeInsetsDirectional padding = EdgeInsetsDirectional.only(
                    start: paddingStart,
                    end: paddingEnd
                );
                if (dataColumnIndex == _onlyTextColumn) {
                    tableColumns[displayColumnIndex] = new IntrinsicColumnWidth(flex: 1.0f);
                }
                else {
                    tableColumns[displayColumnIndex] = new IntrinsicColumnWidth();
                }

                var currentColumnIndex = dataColumnIndex;
                tableRows[0].children[displayColumnIndex] = _buildHeadingCell(
                    context: context,
                    padding: padding,
                    label: column.label,
                    tooltip: column.tooltip,
                    numeric: column.numeric,
                    onSort: column.onSort != null
                        ? () => column.onSort(currentColumnIndex, sortColumnIndex != currentColumnIndex || !sortAscending)
                        : (VoidCallback) null,
                    sorted: dataColumnIndex == sortColumnIndex,
                    ascending: sortAscending
                );
                rowIndex = 1;
                foreach (DataRow row in rows) {
                    DataCell cell = row.cells[dataColumnIndex];
                    var curRow = row;
                    tableRows[rowIndex].children[displayColumnIndex] = _buildDataCell(
                        context: context,
                        padding: padding,
                        label: cell.child,
                        numeric: column.numeric,
                        placeholder: cell.placeholder,
                        showEditIcon: cell.showEditIcon,
                        onTap: cell.onTap,
                        onSelectChanged: () => {
                            if (curRow.onSelectChanged != null) {
                                curRow.onSelectChanged(!curRow.selected);
                            }
                        });
                    rowIndex += 1;
                }

                displayColumnIndex += 1;
            }
            return new Table(
                columnWidths: LinqUtils<int, TableColumnWidth>.SelectDictionary(tableColumns, ((TableColumnWidth x) => tableColumns.IndexOf(x))),
                children: tableRows
            );
        }
    }

    public class TableRowInkWell : InkResponse {
        public TableRowInkWell(
            Key key = null,
            Widget child = null,
            GestureTapCallback onTap = null,
            GestureTapCallback onDoubleTap = null,
            GestureLongPressCallback onLongPress = null,
            ValueChanged<bool> onHighlightChanged = null
        ) : base(
            key: key,
            child: child,
            onTap: onTap,
            onDoubleTap: onDoubleTap,
            onLongPress: onLongPress,
            onHighlightChanged: onHighlightChanged,
            containedInkWell: true,
            highlightShape: BoxShape.rectangle
        ) {
        }

        public override RectCallback getRectCallback(RenderBox referenceBox) {
            return () => {
                    RenderObject cell = referenceBox;
                    AbstractNodeMixinDiagnosticableTree table = cell.parent;
                    Matrix4 transform = Matrix4.identity();
                    while (table is RenderObject && !(table is RenderTable)) {
                        RenderObject parentBox = table as RenderObject;
                        parentBox.applyPaintTransform(cell, transform);
                        D.assert(table == cell.parent);
                        cell = parentBox;
                        table = table.parent;
                    }

                    if (table is RenderTable renderTable) {
                        TableCellParentData cellParentData = cell.parentData as TableCellParentData;
                        Rect rect = renderTable.getRowBox(cellParentData.y);
                        // The rect is in the table's coordinate space. We need to change it to the
                        // TableRowInkWell's coordinate space.
                        renderTable.applyPaintTransform(cell, transform);
                        Offset offset = MatrixUtils.getAsTranslation(transform);
                        if (offset != null)
                            return rect.shift(-offset);
                    }

                    return Rect.zero;
                }
                ;
        }

        public override bool debugCheckContext(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasTable(context));
            return base.debugCheckContext(context);
        }
    }

    internal class _SortArrow : StatefulWidget {
        internal _SortArrow(
            Key key = null,
            bool? visible = null,
            bool? down = null,
            TimeSpan? duration = null
        ) : base(key: key) {
            this.visible = visible;
            this.down = down;
            this.duration = duration;
        }

        public readonly bool? visible;
        public readonly bool? down;
        public readonly TimeSpan? duration;

        public override State createState() => new _SortArrowState();
    }

    class _SortArrowState : TickerProviderStateMixin<_SortArrow> {
        AnimationController _opacityController;
        Animation<float> _opacityAnimation;
        AnimationController _orientationController;
        Animation<float> _orientationAnimation;
        float _orientationOffset = 0.0f;
        bool _down;

        static readonly Animatable<float> _turnTween =
            new FloatTween(begin: 0.0f, end: Mathf.PI).chain(new CurveTween(curve: Curves.easeIn));

        public override void initState() {
            base.initState();
            _opacityAnimation = new CurvedAnimation(
                parent: _opacityController = new AnimationController(
                    duration: widget.duration,
                    vsync: this
                ),
                curve: Curves.fastOutSlowIn
            );
            _opacityAnimation.addListener(_rebuild);
            _opacityController.setValue(widget.visible ?? false ? 1.0f : 0.0f);
            _orientationController = new AnimationController(
                duration: widget.duration,
                vsync: this
            );
            _orientationAnimation = _orientationController.drive(_turnTween);
            _orientationAnimation.addListener(_rebuild);
            _orientationAnimation.addStatusListener(_resetOrientationAnimation);
            if (widget.visible ?? false) {
                _orientationOffset = widget.down ?? false ? 0.0f : Mathf.PI;
            }
        }

        void _rebuild() {
            setState(() => {
                // The animations changed, so we need to rebuild.
            });
        }

        void _resetOrientationAnimation(AnimationStatus status) {
            if (status == AnimationStatus.completed) {
                D.assert(_orientationAnimation.value == Mathf.PI);
                _orientationOffset += Mathf.PI;
                _orientationController.setValue(0.0f); // TODO(ianh): This triggers a pointless rebuild.
            }
        }

        public override void didUpdateWidget(StatefulWidget oldStatefullWidget) {
            var oldWidget = oldStatefullWidget as _SortArrow;
            if (oldWidget == null) {
                return;
            }

            base.didUpdateWidget(oldWidget);

            bool skipArrow = false;
            bool newDown = widget.down ?? _down;
            if (oldWidget.visible != widget.visible) {
                if ((widget.visible ?? false) && (_opacityController.status == AnimationStatus.dismissed)) {
                    _orientationController.stop();
                    _orientationController.setValue(0.0f);
                    _orientationOffset = newDown ? 0.0f : Mathf.PI;
                    skipArrow = true;
                }

                if (widget.visible ?? false) {
                    _opacityController.forward();
                }
                else {
                    _opacityController.reverse();
                }
            }

            if ((_down != newDown) && !skipArrow) {
                if (_orientationController.status == AnimationStatus.dismissed) {
                    _orientationController.forward();
                }
                else {
                    _orientationController.reverse();
                }
            }

            _down = newDown;
        }

        public override void dispose() {
            _opacityController.dispose();
            _orientationController.dispose();
            base.dispose();
        }

        const float _arrowIconBaselineOffset = -1.5f;
        const float _arrowIconSize = 16.0f;

        public override Widget build(BuildContext context) {
            var transform = Matrix4.rotationZ(_orientationOffset + _orientationAnimation.value);
            transform.setTranslationRaw(0.0f, _arrowIconBaselineOffset, 0.0f);
            return new Opacity(
                opacity: _opacityAnimation.value,
                child: new widgets.Transform(
                    transform: transform,
                    alignment: Alignment.center,
                    child: new Icon(
                        Icons.arrow_downward,
                        size: _arrowIconSize,
                        color: (Theme.of(context).brightness == Brightness.light) ? Colors.black87 : Colors.white70
                    )
                )
            );
        }
    }
}