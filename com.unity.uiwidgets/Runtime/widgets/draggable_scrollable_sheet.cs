using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidget.material {
    // TODO: complete this file
    public class DraggableScrollableNotification : ViewportNotificationMixinNotification {
        public DraggableScrollableNotification(
            float extent,
            float minExtent,
            float maxExtent,
            float initialExtent,
            BuildContext context
        ) {
            D.assert(0.0f <= minExtent);
            D.assert(maxExtent <= 1.0f);
            D.assert(minExtent <= extent);
            D.assert(minExtent <= initialExtent);
            D.assert(extent <= maxExtent);
            D.assert(initialExtent <= maxExtent);
            D.assert(context != null);
        }

        public readonly float extent;

        public readonly float minExtent;

        public readonly float maxExtent;

        public readonly float initialExtent;

        public readonly BuildContext context;

        protected override void debugFillDescription(List<String> description) {
            base.debugFillDescription(description);
            description.Add(
                $"minExtent: {minExtent}, extent: {extent}, maxExtent: {maxExtent}, initialExtent: {initialExtent}");
        }
    }
}