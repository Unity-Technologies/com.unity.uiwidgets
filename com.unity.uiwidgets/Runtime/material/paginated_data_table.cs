using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class PaginatedDataTable : StatefulWidget {
        public PaginatedDataTable(
            Key key = null,
            Widget header = null,
            List<Widget> actions = null,
            List<DataColumn> columns = null,
            int? sortColumnIndex = null,
            bool sortAscending = false,
            ValueSetter<bool> onSelectAll = null,
            float dataRowHeight = material_.kMinInteractiveDimension,
            float headingRowHeight = 56.0f,
            float horizontalMargin = 24.0f,
            float columnSpacing = 56.0f,
            bool showCheckboxColumn = true,
            int? initialFirstRowIndex = 0,
            ValueChanged<int> onPageChanged = null,
            int rowsPerPage = defaultRowsPerPage,
            List<int> availableRowsPerPage = null,
            ValueChanged<int> onRowsPerPageChanged = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            DataTableSource source = null
        ) : base(key: key) {
            availableRowsPerPage = availableRowsPerPage ?? new List<int> {
                defaultRowsPerPage, defaultRowsPerPage * 2, defaultRowsPerPage * 5, defaultRowsPerPage * 10
            };
            D.assert(columns.isNotEmpty);
            D.assert(() => {
                if (onRowsPerPageChanged != null)
                    D.assert(availableRowsPerPage != null && availableRowsPerPage.Contains(rowsPerPage));
                return true;
            });
            D.assert(source != null);
            this.header = header;
            this.actions = actions;
            this.columns = columns;
            this.sortColumnIndex = sortColumnIndex;
            this.sortAscending = sortAscending;
            this.onSelectAll = onSelectAll;
            this.dataRowHeight = dataRowHeight;
            this.headingRowHeight = headingRowHeight;
            this.horizontalMargin = horizontalMargin;
            this.columnSpacing = columnSpacing;
            this.showCheckboxColumn = showCheckboxColumn;
            this.initialFirstRowIndex = initialFirstRowIndex;
            this.onPageChanged = onPageChanged;
            this.rowsPerPage = rowsPerPage;
            this.availableRowsPerPage = availableRowsPerPage;
            this.onRowsPerPageChanged = onRowsPerPageChanged;
            this.dragStartBehavior = dragStartBehavior;
            this.source = source;
        }


        public readonly Widget header;

        public readonly List<Widget> actions;

        public readonly List<DataColumn> columns;

        public readonly int? sortColumnIndex;

        public readonly bool sortAscending;

        public readonly ValueSetter<bool> onSelectAll;

        public readonly float dataRowHeight;

        public readonly float headingRowHeight;

        public readonly float horizontalMargin;

        public readonly float columnSpacing;

        public readonly bool showCheckboxColumn;

        public readonly int? initialFirstRowIndex;

        public readonly ValueChanged<int> onPageChanged;

        public readonly int rowsPerPage;

        public const int defaultRowsPerPage = 10;

        public readonly List<int> availableRowsPerPage;

        public readonly ValueChanged<int> onRowsPerPageChanged;

        public readonly DataTableSource source;

        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() => new PaginatedDataTableState();
    }

    class PaginatedDataTableState : State<PaginatedDataTable> {
        int _firstRowIndex;
        int _rowCount;
        bool _rowCountApproximate;
        int _selectedRowCount;
        public readonly Dictionary<int, DataRow> _rows = new Dictionary<int, DataRow>();

        public override void initState() {
            base.initState();
            _firstRowIndex = (PageStorage.of(context)?.readState(context)) as int? ?? widget.initialFirstRowIndex ?? 0;
            widget.source.addListener(_handleDataSourceChanged);
            _handleDataSourceChanged();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (oldWidget is PaginatedDataTable painPaginatedDataTable) {
                if (painPaginatedDataTable.source != widget.source) {
                    painPaginatedDataTable.source.removeListener(_handleDataSourceChanged);
                    widget.source.addListener(_handleDataSourceChanged);
                    _handleDataSourceChanged();
                }
            }
        }

        public override void dispose() {
            widget.source.removeListener(_handleDataSourceChanged);
            base.dispose();
        }

        void _handleDataSourceChanged() {
            setState(() => {
                _rowCount = widget.source.rowCount;
                _rowCountApproximate = widget.source.isRowCountApproximate;
                _selectedRowCount = widget.source.selectedRowCount;
                _rows.Clear();
            });
        }

        void pageTo(int rowIndex) {
            int oldFirstRowIndex = _firstRowIndex;
            setState(() => {
                int rowsPerPage = widget.rowsPerPage;
                _firstRowIndex = ((int) (1.0f * rowIndex / rowsPerPage)) * rowsPerPage;
            });
            if ((widget.onPageChanged != null) &&
                (oldFirstRowIndex != _firstRowIndex))
                widget.onPageChanged(_firstRowIndex);
        }

        DataRow _getBlankRowFor(int index) {
            return DataRow.byIndex(
                index: index,
                cells:  LinqUtils<DataCell,DataColumn>.SelectList(widget.columns,((DataColumn column) => DataCell.empty))
                );
        }

        DataRow _getProgressIndicatorRowFor(int index) {
            bool haveProgressIndicator = false;
            List<DataCell> cells = LinqUtils<DataCell, DataColumn>.SelectList(widget.columns,((DataColumn column) => {
                    if (!column.numeric) {
                        haveProgressIndicator = true;
                        return new DataCell(new CircularProgressIndicator());
                    }
                    
                    return DataCell.empty;
                }));
            if (!haveProgressIndicator) {
                haveProgressIndicator = true;
                cells[0] = new DataCell(new CircularProgressIndicator());
            }

            return DataRow.byIndex(
                index: index,
                cells: cells
            );
        }

        List<DataRow> _getRows(int firstRowIndex, int rowsPerPage) {
            List<DataRow> result = new List<DataRow>();
            int nextPageFirstRowIndex = firstRowIndex + rowsPerPage;

            bool haveProgressIndicator = false;
            for (int index = firstRowIndex;
                index < nextPageFirstRowIndex;
                index += 1) {
                DataRow row = null;
                if (index < _rowCount || _rowCountApproximate) {
                    row = _rows.putIfAbsent(index, () => widget.source.getRow(index));
                    if (row == null && !haveProgressIndicator) {
                        row = row ?? _getProgressIndicatorRowFor(index);
                        haveProgressIndicator = true;
                    }
                }

                row = row ?? _getBlankRowFor(index);
                result.Add(row);
            }

            return result;
        }

        void _handlePrevious() {
            pageTo(Mathf.Max(_firstRowIndex - widget.rowsPerPage, 0));
        }

        void _handleNext() {
            pageTo(_firstRowIndex + widget.rowsPerPage);
        }

        public readonly GlobalKey _tableKey = GlobalKey.key();

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            ThemeData themeData = Theme.of(context);
            MaterialLocalizations localizations = MaterialLocalizations.of(context);

            List<Widget> headerWidgets = new List<Widget>();

            float startPadding = 24.0f;
            if (_selectedRowCount == 0) {
                headerWidgets.Add(new Expanded(child: widget.header));
                if (widget.header is ButtonBar) {
                    startPadding = 12.0f;
                }
            }
            else {
                headerWidgets.Add(new Expanded(
                    child: new Text(localizations.selectedRowCountTitle(_selectedRowCount))
                ));
            }

            if (widget.actions != null) {
                headerWidgets.AddRange(
                    LinqUtils<Widget>.SelectList(widget.actions, ((Widget action) => {
                    return new Padding(
                        padding: EdgeInsetsDirectional.only(
                            start: 24.0f - 8.0f * 2.0f),
                        child: action
                    );
                }))
                );
            }

            TextStyle footerTextStyle = themeData.textTheme.caption;
            List<Widget> footerWidgets = new List<Widget>();
            if (widget.onRowsPerPageChanged != null) {
                List<Widget> availableRowsPerPage = LinqUtils<Widget,int>.SelectList(
                    LinqUtils<int>.WhereList(widget.availableRowsPerPage, ((int value) => value <= _rowCount || value == widget.rowsPerPage)), 
                    (int value) => {
                        return (Widget) new DropdownMenuItem<int>(
                            value: value,
                            child: new Text($"{value}")
                        );
                    });
                footerWidgets.AddRange(new List<Widget>() {
                    new Container(width: 14.0f), // to match trailing padding in case we overflow and end up scrolling
                    new Text(localizations.rowsPerPageTitle),
                    new ConstrainedBox(
                        constraints: new BoxConstraints(minWidth: 64.0f), // 40.0 for the text, 24.0 for the icon
                        child: new Align(
                            alignment: AlignmentDirectional.centerEnd,
                            child: new DropdownButtonHideUnderline(
                                child: new DropdownButton<int>(
                                    items: availableRowsPerPage.Cast<DropdownMenuItem<int>>().ToList(),
                                    value: widget.rowsPerPage,
                                    onChanged: widget.onRowsPerPageChanged,
                                    style: footerTextStyle,
                                    iconSize: 24.0f
                                )
                            )
                        )
                    ),
                });
            }

            footerWidgets.AddRange(new List<Widget>() {
                new Container(width: 32.0f),
                new Text(
                    localizations.pageRowsInfoTitle(
                        _firstRowIndex + 1,
                        _firstRowIndex + widget.rowsPerPage,
                        _rowCount,
                        _rowCountApproximate
                    )
                ),
                new Container(width: 32.0f),
                new IconButton(
                    icon: new Icon(Icons.chevron_left),
                    padding: EdgeInsets.zero,
                    tooltip:
                    localizations.previousPageTooltip,
                    onPressed: _firstRowIndex <= 0 ? (VoidCallback) null : _handlePrevious),
                new Container(width: 24.0f),
                new IconButton(
                    icon: new Icon(Icons.chevron_right),
                    padding: EdgeInsets.zero,
                    tooltip:
                    localizations.nextPageTooltip,
                    onPressed: (!_rowCountApproximate && (_firstRowIndex + widget.rowsPerPage >= _rowCount))
                        ? (VoidCallback) null
                        : _handleNext
                ),

                new Container(width: 14.0f),
            });

            return new LayoutBuilder(
                builder: (BuildContext _context, BoxConstraints constraints) => {
                    return new Card(
                        child: new Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: new List<Widget> {
                                new DefaultTextStyle(
                                    style: _selectedRowCount > 0
                                        ? themeData.textTheme.subtitle1.copyWith(color: themeData.accentColor)
                                        : themeData.textTheme.headline6.copyWith(fontWeight: FontWeight.w400),
                                    child: IconTheme.merge(
                                        data: new IconThemeData(
                                            opacity: 0.54f
                                        ),
                                        child: new Ink(
                                            height: 64.0f,
                                            color: _selectedRowCount > 0 ? themeData.secondaryHeaderColor : null,
                                            child: new Padding(
                                                padding: EdgeInsetsDirectional.only(
                                                    start: startPadding, end: 14.0f),
                                                child: new Row(
                                                    mainAxisAlignment: MainAxisAlignment.end,
                                                    children: headerWidgets
                                                )
                                            )
                                        )
                                    )
                                ),
                                new SingleChildScrollView(
                                    scrollDirection: Axis.horizontal,
                                    dragStartBehavior: widget.dragStartBehavior,
                                    child: new ConstrainedBox(
                                        constraints: new BoxConstraints(minWidth: constraints.minWidth),
                                        child: new DataTable(
                                            key: _tableKey,
                                            columns: widget.columns,
                                            sortColumnIndex: widget.sortColumnIndex,
                                            sortAscending: widget.sortAscending,
                                            onSelectAll: widget.onSelectAll,
                                            dataRowHeight: widget.dataRowHeight,
                                            headingRowHeight: widget.headingRowHeight,
                                            horizontalMargin: widget.horizontalMargin,
                                            columnSpacing: widget.columnSpacing,
                                            rows: _getRows(_firstRowIndex, widget.rowsPerPage)
                                        )
                                    )
                                ),
                                new DefaultTextStyle(
                                    style: footerTextStyle,
                                    child: IconTheme.merge(
                                        data: new IconThemeData(
                                            opacity: 0.54f
                                        ),
                                        child:
                                        new Container(
                                            height: 56.0f,
                                            child: new SingleChildScrollView(
                                                dragStartBehavior: widget.dragStartBehavior,
                                                scrollDirection: Axis.horizontal,
                                                reverse: true,
                                                child: new Row(
                                                    children: footerWidgets
                                                )
                                            )
                                        )
                                    )
                                )
                            }
                        )
                    );
                }
            );
        }
    }
}