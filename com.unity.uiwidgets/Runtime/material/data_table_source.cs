using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.material {
    public abstract class DataTableSource : ChangeNotifier {
        public abstract DataRow getRow(int index);

        public int rowCount { get; }

        public bool isRowCountApproximate { get; }

        public int selectedRowCount { get; }
    }
}