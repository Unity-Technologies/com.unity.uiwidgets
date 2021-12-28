using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class TableRow {
        public TableRow(
            LocalKey key = null,
            Decoration decoration = null,
            List<Widget> children = null
        ) {
            this.key = key;
            this.decoration = decoration;
            this.children = children;
        }

        public readonly LocalKey key;

        public readonly Decoration decoration;

        public readonly List<Widget> children;

        public override string ToString() {
            return "TableRow("
                   + (key != null ? key.ToString() : "")
                   + (decoration != null ? decoration.toString() : "")
                   + (children == null ? "child list is null" :
                       children.isEmpty() ? "no children" :
                       children.ToString())
                   + ")";
        }
    }

    class _TableElementRow {
        public _TableElementRow(
            LocalKey key = null,
            List<Element> children = null
        ) {
            this.key = key;
            this.children = children;
        }

        public readonly LocalKey key;

        public readonly List<Element> children;
    }

    public class Table : RenderObjectWidget {
        public Table(
            Key key = null,
            List<TableRow> children = null,
            Dictionary<int, TableColumnWidth> columnWidths = null,
            TableColumnWidth defaultColumnWidth = null,
            TableBorder border = null,
            TableCellVerticalAlignment defaultVerticalAlignment = TableCellVerticalAlignment.top,
            TextBaseline? textBaseline = null
        ) : base(key: key) {
            children = children ?? new List<TableRow>();
            defaultColumnWidth = defaultColumnWidth ?? new FlexColumnWidth(1.0f);
            this.children = children;
            this.columnWidths = columnWidths;
            this.defaultColumnWidth = defaultColumnWidth;
            this.border = border;
            this.defaultVerticalAlignment = defaultVerticalAlignment;
            this.textBaseline = textBaseline;
            D.assert(() => {
                if (children.Any((TableRow row) => {
                    return row.children.Any((Widget cell) => { return cell == null; });
                })) {
                    throw new UIWidgetsError(
                        "One of the children of one of the rows of the table was null.\n" +
                        "The children of a TableRow must not be null."
                    );
                }

                return true;
            });
            D.assert(() => {
                if (children.Any((TableRow row1) => {
                    return row1.key != null &&
                           children.Any((TableRow row2) => { return row1 != row2 && row1.key == row2.key; });
                })) {
                    throw new UIWidgetsError(
                        "Two or more TableRow children of this Table had the same key.\n" +
                        "All the keyed TableRow children of a Table must have different Keys."
                    );
                }

                return true;
            });
            D.assert(() => {
                if (children.isNotEmpty()) {
                    int cellCount = this.children.First().children.Count;
                    if (children.Any((TableRow row) => { return row.children.Count != cellCount; })) {
                        throw new UIWidgetsError(
                            "Table contains irregular row lengths.\n" +
                            "Every TableRow in a Table must have the same number of children, so that every cell is filled. " +
                            "Otherwise, the table will contain holes."
                        );
                    }
                }

                return true;
            });
            _rowDecorations = null;
            if (children.Any((TableRow row) => { return row.decoration != null; })) {
                _rowDecorations = new List<Decoration>();
                foreach (TableRow row in children) {
                    _rowDecorations.Add(row.decoration);
                }
            }

            D.assert(() => {
                List<Widget> flatChildren = new List<Widget>();
                foreach (TableRow row in children) {
                    flatChildren.AddRange(row.children);
                }

                if (WidgetsD.debugChildrenHaveDuplicateKeys(this, flatChildren)) {
                    throw new UIWidgetsError(
                        "Two or more cells in this Table contain widgets with the same key.\n" +
                        "Every widget child of every TableRow in a Table must have different keys. The cells of a Table are " +
                        "flattened out for processing, so separate cells cannot have duplicate keys even if they are in " +
                        "different rows."
                    );
                }

                return true;
            });
        }


        public readonly List<TableRow> children;

        public readonly Dictionary<int, TableColumnWidth> columnWidths;

        public readonly TableColumnWidth defaultColumnWidth;

        public readonly TableBorder border;

        public readonly TableCellVerticalAlignment defaultVerticalAlignment;

        public readonly TextBaseline? textBaseline;

        public readonly List<Decoration> _rowDecorations;

        public override Element createElement() {
            return new _TableElement(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderTable(
                columns: children.isNotEmpty() ? children[0].children.Count : 0,
                rows: children.Count,
                columnWidths: columnWidths,
                defaultColumnWidth: defaultColumnWidth,
                border: border,
                rowDecorations: _rowDecorations,
                configuration: ImageUtils.createLocalImageConfiguration(context),
                defaultVerticalAlignment: defaultVerticalAlignment,
                textBaseline: textBaseline
            );
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderTable _renderObject = (RenderTable) renderObject;

            D.assert(_renderObject.columns == (children.isNotEmpty() ? children[0].children.Count : 0));
            D.assert(_renderObject.rows == children.Count);

            _renderObject.columnWidths = columnWidths;
            _renderObject.defaultColumnWidth = defaultColumnWidth;
            _renderObject.border = border;
            _renderObject.rowDecorations = _rowDecorations;
            _renderObject.configuration = ImageUtils.createLocalImageConfiguration(context);
            _renderObject.defaultVerticalAlignment = defaultVerticalAlignment;
            _renderObject.textBaseline = textBaseline;
        }
    }


    class _TableElement : RenderObjectElement {
        public _TableElement(Table widget) : base(widget) {
        }

        public new Table widget {
            get { return (Table) base.widget; }
        }

        public new RenderTable renderObject {
            get { return (RenderTable) base.renderObject; }
        }

        List<_TableElementRow> _children = new List<_TableElementRow>();

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);

            _children.Clear();
            foreach (TableRow row in widget.children) {
                List<Element> elements = new List<Element>();
                foreach (Widget child in row.children) {
                    D.assert(child != null);
                    elements.Add(inflateWidget(child, null));
                }

                _children.Add(
                    new _TableElementRow(
                        key: row.key,
                        children: elements)
                );
            }

            _updateRenderObjectChildren();
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            renderObject.setupParentData(child);
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
        }

        protected override void removeChildRenderObject(RenderObject child) {
            TableCellParentData childParentData = child.parentData as TableCellParentData;
            renderObject.setChild(childParentData.x, childParentData.y, null);
        }

        readonly HashSet<Element> _forgottenChildren = new HashSet<Element>();

        public override void update(Widget newWidget) {
            Table _newWidget = (Table) newWidget;
            Dictionary<LocalKey, List<Element>> oldKeyedRows = new Dictionary<LocalKey, List<Element>>();

            foreach (_TableElementRow row in _children) {
                if (row.key != null) {
                    oldKeyedRows[row.key] = row.children;
                }
            }

            List<_TableElementRow> oldUnkeyedRows = new List<_TableElementRow>();
            foreach (_TableElementRow row in _children) {
                if (row.key == null) {
                    oldUnkeyedRows.Add(row);
                }
            }

            List<_TableElementRow> newChildren = new List<_TableElementRow>();
            HashSet<List<Element>> taken = new HashSet<List<Element>>();
            int unkeyedRow = 0;

            foreach (TableRow row in _newWidget.children) {
                List<Element> oldChildren = null;
                if (row.key != null && oldKeyedRows.ContainsKey(row.key)) {
                    oldChildren = oldKeyedRows[row.key];
                    taken.Add(oldChildren);
                }
                else if (row.key == null && unkeyedRow < oldUnkeyedRows.Count) {
                    oldChildren = oldUnkeyedRows[unkeyedRow].children;
                    unkeyedRow++;
                }
                else {
                    oldChildren = new List<Element>();
                }

                newChildren.Add(new _TableElementRow(
                    key: row.key,
                    children: updateChildren(oldChildren, row.children,
                        forgottenChildren: _forgottenChildren))
                );
            }

            while (unkeyedRow < oldUnkeyedRows.Count) {
                updateChildren(oldUnkeyedRows[unkeyedRow].children, new List<Widget>(),
                    forgottenChildren: _forgottenChildren);
                unkeyedRow++;
            }

            foreach (List<Element> oldChildren in oldKeyedRows.Values) {
                if (taken.Contains(oldChildren)) {
                    continue;
                }

                updateChildren(oldChildren, new List<Widget>(), forgottenChildren: _forgottenChildren);
            }
            
            _children = newChildren;
            _updateRenderObjectChildren();
            _forgottenChildren.Clear();
            base.update(newWidget);
            D.assert(widget == newWidget);
        }

        void _updateRenderObjectChildren() {
            D.assert(renderObject != null);
            List<RenderBox> renderBoxes = new List<RenderBox>();
            foreach (_TableElementRow row in _children) {
                foreach (Element child in row.children) {
                    renderBoxes.Add((RenderBox) child.renderObject);
                }
            }

            renderObject.setFlatChildren(
                _children.isNotEmpty() ? _children[0].children.Count : 0,
                renderBoxes
            );
        }

        public override void visitChildren(ElementVisitor visitor) {
            foreach (_TableElementRow row in _children) {
                foreach (Element child in row.children) {
                    if (!_forgottenChildren.Contains(child)) {
                        visitor(child);
                    }
                }
            }
        }

        public override void forgetChild(Element child) {
            _forgottenChildren.Add(child);
        }
    }
    
    public class TableCell : ParentDataWidget<TableCellParentData> {
        public TableCell(
        Key key = null,
        TableCellVerticalAlignment verticalAlignment = default,
        Widget child = null) : base(key: key, child: child) {
            this.verticalAlignment = verticalAlignment;
        }
         public readonly TableCellVerticalAlignment verticalAlignment;
         public override void applyParentData(RenderObject renderObject) { 
            TableCellParentData parentData = renderObject.parentData as TableCellParentData;
            if (parentData.verticalAlignment != verticalAlignment) {
                parentData.verticalAlignment = verticalAlignment;
                AbstractNodeMixinDiagnosticableTree targetParent = renderObject.parent;
                if (targetParent is RenderObject)
                    ((RenderObject)targetParent).markNeedsLayout();
            }
         }

         public override Type debugTypicalAncestorWidgetClass {
             get { return typeof(Table);}
         }

         public override void debugFillProperties(DiagnosticPropertiesBuilder properties) { 
             base.debugFillProperties(properties); 
             properties.add(new EnumProperty<TableCellVerticalAlignment>("verticalAlignment", verticalAlignment));
        }
    }

}