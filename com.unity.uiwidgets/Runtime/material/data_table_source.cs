using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.material {
    public abstract class DataTableSource : ChangeNotifier {
        public abstract DataRow getRow(int index);

        public virtual int rowCount { get; }

        public virtual bool isRowCountApproximate { get; }

        public virtual int selectedRowCount { get; }
    }
}