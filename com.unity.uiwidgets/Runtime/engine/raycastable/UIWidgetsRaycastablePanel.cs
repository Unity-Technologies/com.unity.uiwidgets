using UnityEngine;
using Rect = UnityEngine.Rect;

namespace Unity.UIWidgets.engine {
    [RequireComponent(typeof(RectTransform))]
    public class UIWidgetsRaycastablePanel : UIWidgetsPanel, ICanvasRaycastFilter {
        int windowHashCode;

        public override void mainEntry() {
            base.mainEntry();
            windowHashCode = wrapper.isolate.GetHashCode();
            RaycastManager.NewWindow(windowHashCode);
        }

        protected override void OnDisable() {
            base.OnDisable();
            RaycastManager.DisposeWindow(windowHashCode);
        }

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();
            RaycastManager.OnScreenSizeChanged(windowHashCode);
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
            if (!enabled) {
                return true;
            }

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera,
                out local);

            Rect rect = rectTransform.rect;

            // Convert top left corner as reference origin point.
            local.x += rectTransform.pivot.x * rect.width;
            local.y -= rectTransform.pivot.y * rect.height;
            local.x = local.x / (_currentDevicePixelRatio / canvas.scaleFactor);
            local.y = -local.y / (_currentDevicePixelRatio / canvas.scaleFactor);

            return !RaycastManager.CheckCastThrough(windowHashCode, local);
        }
    }
}