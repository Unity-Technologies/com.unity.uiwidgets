using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public abstract class UniqueWidget<T> :StatefulWidget where T: State<StatefulWidget> {
        
        public UniqueWidget(
            GlobalKey<T> key
        ) : base(key: key) {
            D.assert(key != null);
        }
        public abstract override State createState();
        
        T currentState {
            get {
                GlobalKey<T> globalKey = key as GlobalKey<T>;
                return globalKey.currentState;
            }
        }
    }

}