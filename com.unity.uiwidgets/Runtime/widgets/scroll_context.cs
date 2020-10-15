using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler2;

namespace Unity.UIWidgets.widgets {
    public interface ScrollContext {
        BuildContext notificationContext { get; }

        BuildContext storageContext { get; }

        TickerProvider vsync { get; }

        AxisDirection axisDirection { get; }

        void setIgnorePointer(bool value);

        void setCanDrag(bool value);
    }
}