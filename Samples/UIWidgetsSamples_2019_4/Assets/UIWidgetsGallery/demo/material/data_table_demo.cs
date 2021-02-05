using System;
using System.Collections.Generic;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    internal delegate T getDessertField<T>(Dessert d);

    internal class Dessert
    {
        public Dessert(string name, int calories, float fat, int carbs, float protein, int sodium, int calcium,
            int iron)
        {
            this.name = name;
            this.calories = calories;
            this.fat = fat;
            this.carbs = carbs;
            this.protein = protein;
            this.sodium = sodium;
            this.calcium = calcium;
            this.iron = iron;
        }

        public readonly string name;
        public readonly int calories;
        public readonly float fat;
        public readonly int carbs;
        public readonly float protein;
        public readonly int sodium;
        public readonly int calcium;
        public readonly int iron;

        internal bool selected = false;
    }

    internal class DessertDataSource : DataTableSource
    {
        public readonly List<Dessert> _desserts = new List<Dessert>
        {
            new Dessert("Frozen yogurt", 159, 6.0f, 24, 4.0f, 87, 14, 1),
            new Dessert("Ice cream sandwich", 237, 9.0f, 37, 4.3f, 129, 8, 1),
            new Dessert("Eclair", 262, 16.0f, 24, 6.0f, 337, 6, 7),
            new Dessert("Cupcake", 305, 3.7f, 67, 4.3f, 413, 3, 8),
            new Dessert("Gingerbread", 356, 16.0f, 49, 3.9f, 327, 7, 16),
            new Dessert("Jelly bean", 375, 0.0f, 94, 0.0f, 50, 0, 0),
            new Dessert("Lollipop", 392, 0.2f, 98, 0.0f, 38, 0, 2),
            new Dessert("Honeycomb", 408, 3.2f, 87, 6.5f, 562, 0, 45),
            new Dessert("Donut", 452, 25.0f, 51, 4.9f, 326, 2, 22),
            new Dessert("KitKat", 518, 26.0f, 65, 7.0f, 54, 12, 6),

            new Dessert("Frozen yogurt with sugar", 168, 6.0f, 26, 4.0f, 87, 14, 1),
            new Dessert("Ice cream sandwich with sugar", 246, 9.0f, 39, 4.3f, 129, 8, 1),
            new Dessert("Eclair with sugar", 271, 16.0f, 26, 6.0f, 337, 6, 7),
            new Dessert("Cupcake with sugar", 314, 3.7f, 69, 4.3f, 413, 3, 8),
            new Dessert("Gingerbread with sugar", 345, 16.0f, 51, 3.9f, 327, 7, 16),
            new Dessert("Jelly bean with sugar", 364, 0.0f, 96, 0.0f, 50, 0, 0),
            new Dessert("Lollipop with sugar", 401, 0.2f, 100, 0.0f, 38, 0, 2),
            new Dessert("Honeycomb with sugar", 417, 3.2f, 89, 6.5f, 562, 0, 45),
            new Dessert("Donut with sugar", 461, 25.0f, 53, 4.9f, 326, 2, 22),
            new Dessert("KitKat with sugar", 527, 26.0f, 67, 7.0f, 54, 12, 6),

            new Dessert("Frozen yogurt with honey", 223, 6.0f, 36, 4.0f, 87, 14, 1),
            new Dessert("Ice cream sandwich with honey", 301, 9.0f, 49, 4.3f, 129, 8, 1),
            new Dessert("Eclair with honey", 326, 16.0f, 36, 6.0f, 337, 6, 7),
            new Dessert("Cupcake with honey", 369, 3.7f, 79, 4.3f, 413, 3, 8),
            new Dessert("Gingerbread with honey", 420, 16.0f, 61, 3.9f, 327, 7, 16),
            new Dessert("Jelly bean with honey", 439, 0.0f, 106, 0.0f, 50, 0, 0),
            new Dessert("Lollipop with honey", 456, 0.2f, 110, 0.0f, 38, 0, 2),
            new Dessert("Honeycomb with honey", 472, 3.2f, 99, 6.5f, 562, 0, 45),
            new Dessert("Donut with honey", 516, 25.0f, 63, 4.9f, 326, 2, 22),
            new Dessert("KitKat with honey", 582, 26.0f, 77, 7.0f, 54, 12, 6),

            new Dessert("Frozen yogurt with milk", 262, 8.4f, 36, 12.0f, 194, 44, 1),
            new Dessert("Ice cream sandwich with milk", 339, 11.4f, 49, 12.3f, 236, 38, 1),
            new Dessert("Eclair with milk", 365, 18.4f, 36, 14.0f, 444, 36, 7),
            new Dessert("Cupcake with milk", 408, 6.1f, 79, 12.3f, 520, 33, 8),
            new Dessert("Gingerbread with milk", 459, 18.4f, 61, 11.9f, 434, 37, 16),
            new Dessert("Jelly bean with milk", 478, 2.4f, 106, 8.0f, 157, 30, 0),
            new Dessert("Lollipop with milk", 495, 2.6f, 110, 8.0f, 145, 30, 2),
            new Dessert("Honeycomb with milk", 511, 5.6f, 99, 14.5f, 669, 30, 45),
            new Dessert("Donut with milk", 555, 27.4f, 63, 12.9f, 433, 32, 22),
            new Dessert("KitKat with milk", 621, 28.4f, 77, 15.0f, 161, 42, 6),

            new Dessert("Coconut slice and frozen yogurt", 318, 21.0f, 31, 5.5f, 96, 14, 7),
            new Dessert("Coconut slice and ice cream sandwich", 396, 24.0f, 44, 5.8f, 138, 8, 7),
            new Dessert("Coconut slice and eclair", 421, 31.0f, 31, 7.5f, 346, 6, 13),
            new Dessert("Coconut slice and cupcake", 464, 18.7f, 74, 5.8f, 422, 3, 14),
            new Dessert("Coconut slice and gingerbread", 515, 31.0f, 56, 5.4f, 316, 7, 22),
            new Dessert("Coconut slice and jelly bean", 534, 15.0f, 101, 1.5f, 59, 0, 6),
            new Dessert("Coconut slice and lollipop", 551, 15.2f, 105, 1.5f, 47, 0, 8),
            new Dessert("Coconut slice and honeycomb", 567, 18.2f, 94, 8.0f, 571, 0, 51),
            new Dessert("Coconut slice and donut", 611, 40.0f, 58, 6.4f, 335, 2, 28),
            new Dessert("Coconut slice and KitKat", 677, 41.0f, 72, 8.5f, 63, 12, 12),
        };

        internal void _sort<T>(getDessertField<T> getField, bool isAscending) where T : IComparable
        {
            this._desserts.Sort((Dessert a, Dessert b) =>
            {
                if (!isAscending)
                {
                    Dessert c = a;
                    a = b;
                    b = c;
                }

                T aValue = getField(a);
                T bValue = getField(b);
                return aValue.CompareTo(bValue);
            });

            this.notifyListeners();
        }

        private int _selectedCount = 0;

        public override DataRow getRow(int index)
        {
            D.assert(index >= 0);
            if (index >= this._desserts.Count)
                return null;

            Dessert dessert = this._desserts[index];
            return DataRow.byIndex(
                index: index,
                selected: dessert.selected,
                onSelectChanged: (bool value) =>
                {
                    if (dessert.selected != value)
                    {
                        this._selectedCount += value ? 1 : -1;
                        D.assert(this._selectedCount >= 0);
                        dessert.selected = value;
                        this.notifyListeners();
                    }
                },
                cells: new List<DataCell>
                {
                    new DataCell(new Text(dessert.name)),
                    new DataCell(new Text($"{dessert.calories}")),
                    new DataCell(new Text($"{dessert.fat:F1}")),
                    new DataCell(new Text($"{dessert.carbs}")),
                    new DataCell(new Text($"{dessert.protein:F1}")),
                    new DataCell(new Text($"{dessert.sodium}")),
                    new DataCell(new Text($"{dessert.calcium}%")),
                    new DataCell(new Text($"{dessert.iron}%")),
                }
            );
        }

        public override int rowCount => this._desserts.Count;

        public override bool isRowCountApproximate => false;

        public override int selectedRowCount => this._selectedCount;

        public void _selectAll(bool isChecked)
        {
            foreach (Dessert dessert in this._desserts)
                dessert.selected = isChecked;
            this._selectedCount = isChecked ? this._desserts.Count : 0;
            this.notifyListeners();
        }
    }

    internal class DataTableDemo : StatefulWidget
    {
        public const string routeName = "/material/data-table";


        public override State createState()
        {
            return new _DataTableDemoState();
        }
    }

    internal class _DataTableDemoState : State<DataTableDemo>
    {
        private int _rowsPerPage = PaginatedDataTable.defaultRowsPerPage;
        private int _sortColumnIndex;
        private bool _sortAscending = true;
        private DessertDataSource _dessertsDataSource = new DessertDataSource();

        private void _sort<T>(getDessertField<T> getField, int columnIndex, bool ascending) where T : IComparable
        {
            this._dessertsDataSource._sort<T>(getField, ascending);
            this.setState(() =>
            {
                this._sortColumnIndex = columnIndex;
                this._sortAscending = ascending;
            });
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Data tables"),
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(DataTableDemo.routeName)
                    }
                ),
                body: new Scrollbar(
                    child: new ListView(
                        padding: EdgeInsets.all(20.0f),
                        children: new List<Widget>
                        {
                            new PaginatedDataTable(
                                header: new Text("Nutrition"),
                                rowsPerPage: this._rowsPerPage,
                                onRowsPerPageChanged: (int value) =>
                                {
                                    this.setState(() => { this._rowsPerPage = value; });
                                },
                                sortColumnIndex: this._sortColumnIndex,
                                sortAscending: this._sortAscending,
                                onSelectAll: this._dessertsDataSource._selectAll,
                                columns: new List<DataColumn>
                                {
                                    new DataColumn(
                                        label: new Text("Dessert (100g serving)"),
                                        onSort: (int columnIndex, bool ascending) =>
                                            this._sort<string>((Dessert d) => d.name, columnIndex, ascending)
                                    ),
                                    new DataColumn(
                                        label: new Text("Calories"),
                                        tooltip: "The total amount of food energy in the given serving size.",
                                        numeric: true,
                                        onSort: (int columnIndex, bool ascending) =>
                                            this._sort<int>((Dessert d) => d.calories, columnIndex, ascending)
                                    ),
                                    new DataColumn(
                                        label: new Text("Fat (g)"),
                                        numeric: true,
                                        onSort: (int columnIndex, bool ascending) =>
                                            this._sort<float>((Dessert d) => d.fat, columnIndex, ascending)
                                    ),
                                    new DataColumn(
                                        label: new Text("Carbs (g)"),
                                        numeric: true,
                                        onSort: (int columnIndex, bool ascending) =>
                                            this._sort<int>((Dessert d) => d.carbs, columnIndex, ascending)
                                    ),
                                    new DataColumn(
                                        label: new Text("Protein (g)"),
                                        numeric: true,
                                        onSort: (int columnIndex, bool ascending) =>
                                            this._sort<float>((Dessert d) => d.protein, columnIndex, ascending)
                                    ),
                                    new DataColumn(
                                        label: new Text("Sodium (mg)"),
                                        numeric: true,
                                        onSort: (int columnIndex, bool ascending) =>
                                            this._sort<int>((Dessert d) => d.sodium, columnIndex, ascending)
                                    ),
                                    new DataColumn(
                                        label: new Text("Calcium (%)"),
                                        tooltip:
                                        "The amount of calcium as a percentage of the recommended daily amount.",
                                        numeric: true,
                                        onSort: (int columnIndex, bool ascending) =>
                                            this._sort<int>((Dessert d) => d.calcium, columnIndex, ascending)
                                    ),
                                    new DataColumn(
                                        label: new Text("Iron (%)"),
                                        numeric: true,
                                        onSort: (int columnIndex, bool ascending) =>
                                            this._sort<int>((Dessert d) => d.iron, columnIndex, ascending)
                                    )
                                },
                                source: this._dessertsDataSource
                            )
                        }
                    )
                )
            );
        }
    }
}