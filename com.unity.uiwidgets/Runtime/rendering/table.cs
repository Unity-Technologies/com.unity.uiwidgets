using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public enum TableCellVerticalAlignment {
        top,
        middle,
        bottom,
        baseline,
        fill
    }

    public class TableCellParentData : BoxParentData {
        public TableCellVerticalAlignment? verticalAlignment;

        public int x;

        public int y;

        public override string ToString() {
            return
                base.ToString() + "; " + (verticalAlignment == null
                    ? "default vertical alignment"
                    : verticalAlignment.ToString());
        }
    }

    public abstract class TableColumnWidth {
        protected TableColumnWidth() {
        }

        public abstract float minIntrinsicWidth(List<RenderBox> cells, float containerWidth);

        public abstract float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth);

        public virtual float? flex(List<RenderBox> cells) {
            return null;
        }

        public override string ToString() {
            return GetType().ToString();
        }
    }


    public class IntrinsicColumnWidth : TableColumnWidth {
        public IntrinsicColumnWidth(
            float? flex = null) {
            _flex = flex;
        }

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            float result = 0.0f;
            foreach (RenderBox cell in cells) {
                result = Mathf.Max(result, cell.getMinIntrinsicWidth(float.PositiveInfinity));
            }

            return result;
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            float result = 0.0f;
            foreach (RenderBox cell in cells) {
                result = Mathf.Max(result, cell.getMaxIntrinsicWidth(float.PositiveInfinity));
            }

            return result;
        }

        public override float? flex(List<RenderBox> cells) {
            return _flex;
        }

        readonly float? _flex;

        public override string ToString() {
            return $"${GetType()}(flex: {_flex})";
        }
    }


    public class FixedColumnWidth : TableColumnWidth {
        public FixedColumnWidth(float? value = null) {
            D.assert(value != null);
            this.value = value.Value;
        }

        public readonly float value;

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return value;
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return value;
        }

        public override string ToString() {
            return $"{GetType()}({value})";
        }
    }


    public class FractionColumnWidth : TableColumnWidth {
        public FractionColumnWidth(float? value = null) {
            D.assert(value != null);
            this.value = value.Value;
        }

        public readonly float value;

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            if (!containerWidth.isFinite()) {
                return 0.0f;
            }

            return value * containerWidth;
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            if (!containerWidth.isFinite()) {
                return 0.0f;
            }

            return value * containerWidth;
        }

        public override string ToString() {
            return $"{GetType()}({value})";
        }
    }

    public class FlexColumnWidth : TableColumnWidth {
        public FlexColumnWidth(float value = 1.0f) {
            this.value = value;
        }

        public readonly float value;

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return 0.0f;
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return 0.0f;
        }

        public override float? flex(List<RenderBox> cells) {
            return value;
        }

        public override string ToString() {
            return $"{GetType()}({value})";
        }
    }

    public class MaxColumnWidth : TableColumnWidth {
        public MaxColumnWidth(
            TableColumnWidth a, TableColumnWidth b) {
            this.a = a;
            this.b = b;
        }

        public readonly TableColumnWidth a;

        public readonly TableColumnWidth b;

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return Mathf.Max(
                a.minIntrinsicWidth(cells, containerWidth),
                b.minIntrinsicWidth(cells, containerWidth)
            );
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return Mathf.Max(
                a.maxIntrinsicWidth(cells, containerWidth),
                b.maxIntrinsicWidth(cells, containerWidth)
            );
        }

        public override float? flex(List<RenderBox> cells) {
            float? aFlex = a.flex(cells);
            if (aFlex == null) {
                return b.flex(cells);
            }

            float? bFlex = b.flex(cells);
            if (bFlex == null) {
                return null;
            }

            return Mathf.Max(aFlex.Value, bFlex.Value);
        }

        public override string ToString() {
            return $"{GetType()}({a}, {b})";
        }
    }

    public class MinColumnWidth : TableColumnWidth {
        public MinColumnWidth(
            TableColumnWidth a, TableColumnWidth b) {
            this.a = a;
            this.b = b;
        }

        public readonly TableColumnWidth a;

        public readonly TableColumnWidth b;

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return Mathf.Min(
                a.minIntrinsicWidth(cells, containerWidth),
                b.minIntrinsicWidth(cells, containerWidth)
            );
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return Mathf.Min(
                a.maxIntrinsicWidth(cells, containerWidth),
                b.maxIntrinsicWidth(cells, containerWidth)
            );
        }

        public override float? flex(List<RenderBox> cells) {
            float? aFlex = a.flex(cells);
            if (aFlex == null) {
                return b.flex(cells);
            }

            float? bFlex = b.flex(cells);
            if (bFlex == null) {
                return null;
            }

            return Mathf.Min(aFlex.Value, bFlex.Value);
        }

        public override string ToString() {
            return $"{GetType()}({a}, {b})";
        }
    }

    public class RenderTable : RenderBox {
        public RenderTable(
            int? columns = null,
            int? rows = null,
            Dictionary<int, TableColumnWidth> columnWidths = null,
            TableColumnWidth defaultColumnWidth = null,
            TextDirection? textDirection = null,
            TableBorder border = null,
            List<Decoration> rowDecorations = null,
            ImageConfiguration configuration = null,
            TableCellVerticalAlignment defaultVerticalAlignment = TableCellVerticalAlignment.top,
            TextBaseline? textBaseline = null,
            List<List<RenderBox>> children = null
        ) {
            D.assert(columns == null || columns >= 0);
            D.assert(rows == null || rows >= 0);
            D.assert(rows == null || children == null);
            _textDirection = textDirection ?? TextDirection.ltr;
            _columns = columns ?? (children != null && children.isNotEmpty() ? children[0].Count : 0);
            _rows = rows ?? 0;
            _children = new List<RenderBox>();
            for (int i = 0; i < _columns * _rows; i++) {
                _children.Add(null);
            }

            _columnWidths = columnWidths ?? new Dictionary<int, TableColumnWidth>();
            _defaultColumnWidth = defaultColumnWidth ?? new FlexColumnWidth(1.0f);
            _border = border;
            this.rowDecorations = rowDecorations;
            _configuration = configuration ?? ImageConfiguration.empty;
            _defaultVerticalAlignment = defaultVerticalAlignment;
            _textBaseline = textBaseline;

            if (children != null) {
                foreach (List<RenderBox> row in children) {
                    addRow(row);
                }
            }
        }

        List<RenderBox> _children = new List<RenderBox>();

        public int columns {
            get { return _columns; }
            set {
                D.assert(value >= 0);
                if (value == columns) {
                    return;
                }

                int oldColumns = columns;
                List<RenderBox> oldChildren = _children;
                _columns = value;
                _children = new List<RenderBox>();
                for (int i = 0; i < columns * rows; i++) {
                    _children.Add(null);
                }

                int columnsToCopy = Mathf.Min(columns, oldColumns);
                for (int y = 0; y < rows; y++) {
                    for (int x = 0; x < columnsToCopy; x++) {
                        _children[x + y * columns] = oldChildren[x + y * oldColumns];
                    }
                }

                if (oldColumns > columns) {
                    for (int y = 0; y < rows; y++) {
                        for (int x = columns; x < oldColumns; x++) {
                            int xy = x + y * oldColumns;
                            if (oldChildren[xy] != null) {
                                dropChild(oldChildren[xy]);
                            }
                        }
                    }
                }

                markNeedsLayout();
            }
        }

        int _columns;

        public int rows {
            get { return _rows; }
            set {
                D.assert(value >= 0);
                if (value == rows) {
                    return;
                }

                if (_rows > value) {
                    for (int xy = columns * value; xy < _children.Count; xy++) {
                        if (_children[xy] != null) {
                            dropChild(_children[xy]);
                        }
                    }
                }

                _rows = value;
                if (_children.Count > columns * rows) {
                    _children.RemoveRange(columns * rows,
                        _children.Count - columns * rows);
                }
                else if (_children.Count < columns * rows) {
                    while (_children.Count < columns * rows) {
                        _children.Add(null);
                    }
                }

                D.assert(_children.Count == columns * rows);

                markNeedsLayout();
            }
        }

        int _rows;

        public Dictionary<int, TableColumnWidth> columnWidths {
            get { return _columnWidths; }
            set {
                value = value ?? new Dictionary<int, TableColumnWidth>();
                if (_columnWidths == value) {
                    return;
                }

                _columnWidths = value;
                markNeedsLayout();
            }
        }

        Dictionary<int, TableColumnWidth> _columnWidths;

        public void setColumnWidth(int column, TableColumnWidth value) {
            if (_columnWidths.getOrDefault(column) == value) {
                return;
            }

            _columnWidths[column] = value;
            markNeedsLayout();
            ;
        }

        public TableColumnWidth defaultColumnWidth {
            get { return _defaultColumnWidth; }
            set {
                D.assert(value != null);
                if (defaultColumnWidth == value) {
                    return;
                }

                _defaultColumnWidth = value;
                markNeedsLayout();
            }
        }

        TableColumnWidth _defaultColumnWidth;

        TextDirection textDirection {
            get {
                return _textDirection;
            }
            set {
                if (_textDirection == value)
                    return;
                _textDirection = value;
                markNeedsLayout();
            }
        }

        TextDirection _textDirection;
        
        
        public TableBorder border {
            get { return _border; }
            set {
                if (border == value) {
                    return;
                }

                _border = value;
                markNeedsPaint();
            }
        }

        TableBorder _border;


        public List<Decoration> rowDecorations {
            get { return _rowDecorations ?? new List<Decoration>(); }
            set {
                if (_rowDecorations == value) {
                    return;
                }

                _rowDecorations = value;
                if (_rowDecorationPainters != null) {
                    foreach (BoxPainter painter in _rowDecorationPainters) {
                        painter?.Dispose();
                    }
                }

                if (_rowDecorations != null) {
                    _rowDecorationPainters = new List<BoxPainter>();
                    for (int i = 0; i < _rowDecorations.Count; i++) {
                        _rowDecorationPainters.Add(null);
                    }
                }
                else {
                    _rowDecorationPainters = null;
                }
            }
        }

        List<Decoration> _rowDecorations;
        List<BoxPainter> _rowDecorationPainters;

        public ImageConfiguration configuration {
            get { return _configuration; }
            set {
                D.assert(value != null);
                if (value == _configuration) {
                    return;
                }

                _configuration = value;
                markNeedsPaint();
            }
        }

        ImageConfiguration _configuration;

        public TableCellVerticalAlignment defaultVerticalAlignment {
            get { return _defaultVerticalAlignment; }
            set {
                if (_defaultVerticalAlignment == value) {
                    return;
                }

                _defaultVerticalAlignment = value;
                markNeedsLayout();
            }
        }

        TableCellVerticalAlignment _defaultVerticalAlignment;

        public TextBaseline? textBaseline {
            get { return _textBaseline; }
            set {
                if (_textBaseline == value) {
                    return;
                }

                _textBaseline = value;
                markNeedsLayout();
            }
        }

        TextBaseline? _textBaseline;

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is TableCellParentData)) {
                child.parentData = new TableCellParentData();
            }
        }

        public void setFlatChildren(int columns, List<RenderBox> cells) {
            if (cells == _children && columns == _columns) {
                return;
            }

            D.assert(columns >= 0);

            if (columns == 0 || cells.isEmpty()) {
                D.assert(cells == null || cells.isEmpty());
                _columns = columns;
                if (_children.isEmpty()) {
                    D.assert(_rows == 0);
                    return;
                }

                foreach (RenderBox oldChild in _children) {
                    if (oldChild != null) {
                        dropChild(oldChild);
                    }
                }

                _rows = 0;
                _children.Clear();
                markNeedsLayout();
                return;
            }

            D.assert(cells != null);
            D.assert(cells.Count % columns == 0);

            HashSet<RenderBox> lostChildren = new HashSet<RenderBox>();
            int y, x;
            for (y = 0; y < _rows; y++) {
                for (x = 0; x < _columns; x++) {
                    int xyOld = x + y * _columns;
                    int xyNew = x + y * columns;
                    if (_children[xyOld] != null &&
                        (x >= columns || xyNew >= cells.Count || _children[xyOld] != cells[xyNew])) {
                        lostChildren.Add(_children[xyOld]);
                    }
                }
            }

            y = 0;
            while (y * columns < cells.Count) {
                for (x = 0; x < columns; x++) {
                    int xyNew = x + y * columns;
                    int xyOld = x + y * _columns;
                    if (cells[xyNew] != null &&
                        (x >= _columns || y >= _rows || _children[xyOld] != cells[xyNew])) {
                        if (lostChildren.Contains(cells[xyNew])) {
                            lostChildren.Remove(cells[xyNew]);
                        }
                        else {
                            adoptChild(cells[xyNew]);
                        }
                    }
                }

                y += 1;
            }

            foreach (RenderBox child in lostChildren) {
                dropChild(child);
            }

            _columns = columns;
            _rows = cells.Count / columns;
            _children = cells;
            D.assert(_children.Count == rows * this.columns);
            markNeedsLayout();
        }


        void setChildren(List<List<RenderBox>> cells) {
            if (cells == null) {
                setFlatChildren(0, null);
                return;
            }

            foreach (RenderBox oldChild in _children) {
                if (oldChild != null) {
                    dropChild(oldChild);
                }
            }

            _children.Clear();
            _columns = cells.isNotEmpty() ? cells[0].Count : 0;
            _rows = 0;
            foreach (List<RenderBox> row in cells) {
                addRow(row);
            }

            D.assert(_children.Count == rows * columns);
        }


        void addRow(List<RenderBox> cells) {
            D.assert(cells.Count == columns);
            D.assert(_children.Count == rows * columns);

            _rows += 1;
            _children.AddRange(cells);
            foreach (RenderBox cell in cells) {
                if (cell != null) {
                    adoptChild(cell);
                }
            }

            markNeedsLayout();
        }

        public void setChild(int x, int y, RenderBox value) {
            D.assert(x >= 0 && x < columns && y >= 0 && y < rows);
            D.assert(_children.Count == rows * columns);

            int xy = x + y * columns;
            RenderBox oldChild = _children[xy];
            if (oldChild == value) {
                return;
            }

            if (oldChild != null) {
                dropChild(oldChild);
            }

            _children[xy] = value;
            if (value != null) {
                adoptChild(value);
            }
        }


        public override void attach(object owner) {
            base.attach(owner);
            foreach (RenderBox child in _children) {
                child?.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            if (_rowDecorationPainters != null) {
                foreach (BoxPainter painter in _rowDecorationPainters) {
                    painter?.Dispose();
                }

                _rowDecorationPainters = new List<BoxPainter>();
                for (int i = 0; i < _rowDecorations.Count; i++) {
                    _rowDecorationPainters.Add(null);
                }
            }

            foreach (RenderBox child in _children) {
                child?.detach();
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            D.assert(_children.Count == rows * columns);
            foreach (RenderBox child in _children) {
                if (child != null) {
                    visitor(child);
                }
            }
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            D.assert(_children.Count == rows * columns);
            float totalMinWidth = 0.0f;
            for (int x = 0; x < columns; x++) {
                TableColumnWidth columnWidth = _columnWidths.getOrDefault(x) ?? defaultColumnWidth;
                List<RenderBox> columnCells = column(x);
                totalMinWidth += columnWidth.minIntrinsicWidth(columnCells, float.PositiveInfinity);
            }

            return totalMinWidth;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            D.assert(_children.Count == rows * columns);
            float totalMaxWidth = 0.0f;
            for (int x = 0; x < columns; x++) {
                TableColumnWidth columnWidth = _columnWidths.getOrDefault(x) ?? defaultColumnWidth;
                List<RenderBox> columnCells = column(x);
                totalMaxWidth += columnWidth.maxIntrinsicWidth(columnCells, float.PositiveInfinity);
            }

            return totalMaxWidth;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            D.assert(_children.Count == rows * columns);
            List<float> widths = _computeColumnWidths(BoxConstraints.tightForFinite(width: width));
            float rowTop = 0.0f;
            for (int y = 0; y < rows; y++) {
                float rowHeight = 0.0f;
                for (int x = 0; x < columns; x++) {
                    int xy = x + y * columns;
                    RenderBox child = _children[xy];
                    if (child != null) {
                        rowHeight = Mathf.Max(rowHeight, child.getMaxIntrinsicHeight(widths[x]));
                    }
                }

                rowTop += rowHeight;
            }

            return rowTop;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return computeMinIntrinsicHeight(width);
        }

        float? _baselineDistance;

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            D.assert(!debugNeedsLayout);
            return _baselineDistance;
        }

        List<RenderBox> column(int x) {
            List<RenderBox> ret = new List<RenderBox>();
            for (int y = 0; y < rows; y++) {
                int xy = x + y * columns;
                RenderBox child = _children[xy];
                if (child != null) {
                    ret.Add(child);
                }
            }

            return ret;
        }

        List<RenderBox> row(int y) {
            List<RenderBox> ret = new List<RenderBox>();
            int start = y * columns;
            int end = (y + 1) * columns;
            for (int xy = start; xy < end; xy++) {
                RenderBox child = _children[xy];
                if (child != null) {
                    ret.Add(child);
                }
            }

            return ret;
        }

        List<float> _computeColumnWidths(BoxConstraints constraints) {
            D.assert(constraints != null);
            D.assert(_children.Count == rows * columns);

            List<float> widths = new List<float>();
            List<float> minWidths = new List<float>();
            List<float?> flexes = new List<float?>();

            for (int i = 0; i < columns; i++) {
                widths.Add(0.0f);
                minWidths.Add(0.0f);
                flexes.Add(null);
            }

            float tableWidth = 0.0f;
            float? unflexedTableWidth = 0.0f;
            float totalFlex = 0.0f;

            for (int x = 0; x < columns; x++) {
                TableColumnWidth columnWidth = _columnWidths.getOrDefault(x) ?? defaultColumnWidth;
                List<RenderBox> columnCells = column(x);

                float maxIntrinsicWidth = columnWidth.maxIntrinsicWidth(columnCells, constraints.maxWidth);
                D.assert(maxIntrinsicWidth.isFinite());
                D.assert(maxIntrinsicWidth >= 0.0f);
                widths[x] = maxIntrinsicWidth;
                tableWidth += maxIntrinsicWidth;

                float minIntrinsicWidth = columnWidth.minIntrinsicWidth(columnCells, constraints.maxWidth);
                D.assert(minIntrinsicWidth.isFinite());
                D.assert(minIntrinsicWidth >= 0.0f);
                minWidths[x] = minIntrinsicWidth;
                D.assert(maxIntrinsicWidth >= minIntrinsicWidth);

                float? flex = columnWidth.flex(columnCells);
                if (flex != null) {
                    D.assert(flex.Value.isFinite());
                    D.assert(flex.Value > 0.0f);
                    flexes[x] = flex;
                    totalFlex += flex.Value;
                }
                else {
                    unflexedTableWidth += maxIntrinsicWidth;
                }
            }

            float maxWidthConstraint = constraints.maxWidth;
            float minWidthConstraint = constraints.minWidth;

            if (totalFlex > 0.0f) {
                float targetWidth = 0.0f;
                if (maxWidthConstraint.isFinite()) {
                    targetWidth = maxWidthConstraint;
                }
                else {
                    targetWidth = minWidthConstraint;
                }

                if (tableWidth < targetWidth) {
                    float remainingWidth = targetWidth - unflexedTableWidth.Value;
                    D.assert(remainingWidth.isFinite());
                    D.assert(remainingWidth >= 0.0f);
                    for (int x = 0; x < columns; x++) {
                        if (flexes[x] != null) {
                            float flexedWidth = remainingWidth * flexes[x].Value / totalFlex;
                            D.assert(flexedWidth.isFinite());
                            D.assert(flexedWidth >= 0.0f);
                            if (widths[x] < flexedWidth) {
                                float delta = flexedWidth - widths[x];
                                tableWidth += delta;
                                widths[x] = flexedWidth;
                            }
                        }
                    }

                    D.assert(tableWidth + foundation_.precisionErrorTolerance >= targetWidth);
                }
            }
            else if (tableWidth < minWidthConstraint) {
                float delta = (minWidthConstraint - tableWidth) / columns;
                for (int x = 0; x < columns; x++) {
                    widths[x] += delta;
                }

                tableWidth = minWidthConstraint;
            }

            D.assert(() => {
                unflexedTableWidth = null;
                return true;
            });

            if (tableWidth > maxWidthConstraint) {
                float deficit = tableWidth - maxWidthConstraint;

                int availableColumns = columns;

                //(Xingwei Zhu) this deficit is double and set to be 0.00000001f in flutter.
                //since we use float by default, making it larger should make sense in most cases
                while (deficit > foundation_.precisionErrorTolerance && totalFlex > foundation_.precisionErrorTolerance) {
                    float newTotalFlex = 0.0f;
                    for (int x = 0; x < columns; x++) {
                        if (flexes[x] != null) {
                            //(Xingwei Zhu) in case deficit * flexes[x].Value / totalFlex => 0 if deficit is really small, leading to dead loop,
                            //we amend it with a default larger value to ensure that this loop will eventually end
                            float newWidth =
                                widths[x] - Mathf.Max(foundation_.precisionErrorTolerance, deficit * flexes[x].Value / totalFlex);
                            D.assert(newWidth.isFinite());
                            if (newWidth <= minWidths[x]) {
                                deficit -= widths[x] - minWidths[x];
                                widths[x] = minWidths[x];
                                flexes[x] = null;
                                availableColumns -= 1;
                            }
                            else {
                                deficit -= widths[x] - newWidth;
                                widths[x] = newWidth;
                                newTotalFlex += flexes[x].Value;
                            }

                            D.assert(widths[x] >= 0.0f);
                        }
                    }

                    totalFlex = newTotalFlex;
                }

                while (deficit > foundation_.precisionErrorTolerance && availableColumns > 0) {
                    float delta = deficit / availableColumns;
                    D.assert(delta != 0);
                    int newAvailableColumns = 0;
                    for (int x = 0; x < columns; x++) {
                        float availableDelta = widths[x] - minWidths[x];
                        if (availableDelta > 0.0f) {
                            if (availableDelta <= delta) {
                                deficit -= widths[x] - minWidths[x];
                                widths[x] = minWidths[x];
                            }
                            else {
                                deficit -= availableDelta;
                                widths[x] -= availableDelta;
                                newAvailableColumns += 1;
                            }
                        }
                    }
                    availableColumns = newAvailableColumns;
                }
            }
            

            return widths;
        }

        readonly List<float> _rowTops = new List<float>();
        List<float> _columnLefts;

       public Rect getRowBox(int row) {
            D.assert(row >= 0);
            D.assert(row < rows);
            D.assert(!debugNeedsLayout);
            return Rect.fromLTRB(0.0f, _rowTops[row], size.width, _rowTops[row + 1]);
        }

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            int rows = this.rows;
            int columns = this.columns;
            D.assert(_children.Count == rows * columns);
            if (rows * columns == 0) {
                size = constraints.constrain(new Size(0.0f, 0.0f));
                return;
            }

            List<float> widths = _computeColumnWidths(constraints);
            List<float> positions = new List<float>();
            for (int i = 0; i < columns; i++) {
                positions.Add(0.0f);
            }
            float tableWidth = 0.0f;
            switch (textDirection) {
                case TextDirection.rtl:
                    positions[columns - 1] = 0.0f;
                    for (int x = columns - 2; x >= 0; x -= 1)
                        positions[x] = positions[x+1] + widths[x+1];
                    _columnLefts = positions;
                    tableWidth = positions[0] + widths[0];
                    break;
                case TextDirection.ltr:
                    positions[0] = 0.0f;
                    for (int x = 1; x < columns; x += 1)
                        positions[x] = positions[x-1] + widths[x-1];
                    _columnLefts = positions;
                    tableWidth = positions[columns - 1] + widths[columns - 1];
                    break;
            }

            _rowTops.Clear();
            _baselineDistance = null;

            float rowTop = 0.0f;
            for (int y = 0; y < rows; y++) {
                _rowTops.Add(rowTop);
                float rowHeight = 0.0f;
                bool haveBaseline = false;
                float beforeBaselineDistance = 0.0f;
                float afterBaselineDistance = 0.0f;
                List<float?> baselines = new List<float?>();
                for (int i = 0; i < columns; i++) {
                    baselines.Add(null);
                }

                for (int x = 0; x < columns; x++) {
                    int xy = x + y * columns;
                    RenderBox child = _children[xy];
                    if (child != null) {
                        TableCellParentData childParentData = (TableCellParentData) child.parentData;
                        D.assert(childParentData != null);
                        childParentData.x = x;
                        childParentData.y = y;
                        switch (childParentData.verticalAlignment ?? defaultVerticalAlignment) {
                            case TableCellVerticalAlignment.baseline: {
                                D.assert(textBaseline != null);
                                child.layout(BoxConstraints.tightFor(width: widths[x]), parentUsesSize: true);
                                float? childBaseline =
                                    child.getDistanceToBaseline(textBaseline.Value, onlyReal: true);
                                if (childBaseline != null) {
                                    beforeBaselineDistance = Mathf.Max(beforeBaselineDistance, childBaseline.Value);
                                    afterBaselineDistance = Mathf.Max(afterBaselineDistance,
                                        child.size.height - childBaseline.Value);
                                    baselines[x] = childBaseline.Value;
                                    haveBaseline = true;
                                }
                                else {
                                    rowHeight = Mathf.Max(rowHeight, child.size.height);
                                    childParentData.offset = new Offset(positions[x], rowTop);
                                }

                                break;
                            }
                            case TableCellVerticalAlignment.top:
                            case TableCellVerticalAlignment.middle:
                            case TableCellVerticalAlignment.bottom: {
                                child.layout(BoxConstraints.tightFor(width: widths[x]), parentUsesSize: true);
                                rowHeight = Mathf.Max(rowHeight, child.size.height);
                                break;
                            }
                            case TableCellVerticalAlignment.fill: {
                                break;
                            }
                        }
                    }
                }

                if (haveBaseline) {
                    if (y == 0) {
                        _baselineDistance = beforeBaselineDistance;
                    }

                    rowHeight = Mathf.Max(rowHeight, beforeBaselineDistance + afterBaselineDistance);
                }

                for (int x = 0; x < columns; x++) {
                    int xy = x + y * columns;
                    RenderBox child = _children[xy];
                    if (child != null) {
                        TableCellParentData childParentData = (TableCellParentData) child.parentData;
                        switch (childParentData.verticalAlignment ?? defaultVerticalAlignment) {
                            case TableCellVerticalAlignment.baseline: {
                                if (baselines[x] != null) {
                                    childParentData.offset = new Offset(positions[x],
                                        rowTop + beforeBaselineDistance - baselines[x].Value);
                                }

                                break;
                            }
                            case TableCellVerticalAlignment.top: {
                                childParentData.offset = new Offset(positions[x], rowTop);
                                break;
                            }
                            case TableCellVerticalAlignment.middle: {
                                childParentData.offset = new Offset(positions[x],
                                    rowTop + (rowHeight - child.size.height) / 2.0f);
                                break;
                            }
                            case TableCellVerticalAlignment.bottom: {
                                childParentData.offset =
                                    new Offset(positions[x], rowTop + rowHeight - child.size.height);
                                break;
                            }
                            case TableCellVerticalAlignment.fill: {
                                child.layout(BoxConstraints.tightFor(width: widths[x], height: rowHeight));
                                childParentData.offset = new Offset(positions[x], rowTop);
                                break;
                            }
                        }
                    }
                }

                rowTop += rowHeight;
            }

            _rowTops.Add(rowTop);
            size = constraints.constrain(new Size(tableWidth, rowTop));
            D.assert(_rowTops.Count == rows + 1);
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            D.assert(_children.Count == rows * columns);
            for (int index = _children.Count - 1; index >= 0; index--) {
                RenderBox child = _children[index];
                if (child != null) {
                    BoxParentData childParentData = (BoxParentData) child.parentData;
                    bool isHit = result.addWithPaintOffset(
                        offset: childParentData.offset,
                        position: position,
                        hitTest: (BoxHitTestResult resultIn, Offset transformed) => {
                            D.assert(transformed == position - childParentData.offset);
                            return child.hitTest(resultIn, position: transformed);
                        }
                    );
                    if (isHit) {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void paint(PaintingContext context, Offset offset) {
            D.assert(_children.Count == this.rows * this.columns);
            if (this.rows * this.columns == 0) {
                if (border != null) {
                    Rect borderRect = Rect.fromLTWH(offset.dx, offset.dy, size.width, 0.0f);
                    border.paint(context.canvas, borderRect, rows: new List<float>(), columns: new List<float>());
                }

                return;
            }

            D.assert(_rowTops.Count == this.rows + 1);
            if (_rowDecorations != null) {
                D.assert(_rowDecorations.Count == _rowDecorationPainters.Count);
                Canvas canvas = context.canvas;
                for (int y = 0; y < rows; y++) {
                    if (_rowDecorations.Count <= y) {
                        break;
                    }

                    if (_rowDecorations[y] != null) {
                        _rowDecorationPainters[y] = _rowDecorationPainters[y] ??
                                                         _rowDecorations[y].createBoxPainter(markNeedsPaint);
                        _rowDecorationPainters[y].paint(
                            canvas,
                            new Offset(offset.dx, offset.dy + _rowTops[y]),
                            configuration.copyWith(
                                size: new Size(size.width, _rowTops[y + 1] - _rowTops[y])
                            )
                        );
                    }
                }
            }

            for (int index = 0; index < _children.Count; index++) {
                RenderBox child = _children[index];
                if (child != null) {
                    BoxParentData childParentData = (BoxParentData) child.parentData;
                    context.paintChild(child, childParentData.offset + offset);
                }
            }

            D.assert(_rows == _rowTops.Count - 1);
            D.assert(_columns == _columnLefts.Count);
            if (border != null) {
                Rect borderRect = Rect.fromLTWH(offset.dx, offset.dy, size.width,
                    _rowTops[_rowTops.Count - 1]);
                List<float> rows = _rowTops.GetRange(1, _rowTops.Count - 2);
                List<float> columns = _columnLefts.GetRange(1, _columnLefts.Count - 1);
                border.paint(context.canvas, borderRect, rows: rows, columns: columns);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<TableBorder>("border", border, defaultValue: null));
            properties.add(new DiagnosticsProperty<Dictionary<int, TableColumnWidth>>("specified column widths",
                _columnWidths,
                level: _columnWidths.isEmpty() ? DiagnosticLevel.hidden : DiagnosticLevel.info));
            properties.add(new DiagnosticsProperty<TableColumnWidth>("default column width", defaultColumnWidth));
            properties.add(new MessageProperty("table size", $"{columns}*{rows}"));
            properties.add(new EnumerableProperty<float>("column offsets", _columnLefts, ifNull: "unknown"));
            properties.add(new EnumerableProperty<float>("row offsets", _rowTops, ifNull: "unknown"));
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            if (_children.isEmpty()) {
                return new List<DiagnosticsNode> {DiagnosticsNode.message("table is empty")};
            }

            List<DiagnosticsNode> children = new List<DiagnosticsNode>();
            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < columns; x++) {
                    int xy = x + y * columns;
                    RenderBox child = _children[xy];
                    string name = $"child ({x}, {y})";
                    if (child != null) {
                        children.Add(child.toDiagnosticsNode(name: name));
                    }
                    else {
                        children.Add(new DiagnosticsProperty<object>(name, null, ifNull: "is null",
                            showSeparator: false));
                    }
                }
            }

            return children;
        }
    }
}