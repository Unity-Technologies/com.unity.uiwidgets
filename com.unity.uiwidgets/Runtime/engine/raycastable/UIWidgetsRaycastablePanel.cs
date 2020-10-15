/*using Unity.UIWidgets.engine;
using UnityEngine;

namespace Unity.UIWidgets.engine.raycast {
    [RequireComponent(typeof(RectTransform))]
    public class UIWidgetsRaycastablePanel : UIWidgetsPanel, ICanvasRaycastFilter {
        int windowHashCode;

        protected override void InitWindowAdapter() {
            base.InitWindowAdapter();
            windowHashCode = window.GetHashCode();
            RaycastManager.NewWindow(windowHashCode);
        }

        protected override void OnDisable() {
            base.OnDisable();
            RaycastManager.DisposeWindow(windowHashCode);
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
            local.x = local.x / devicePixelRatio;
            local.y = -local.y / devicePixelRatio;

            return !RaycastManager.CheckCastThrough(windowHashCode, local);
        }
    }
}*/