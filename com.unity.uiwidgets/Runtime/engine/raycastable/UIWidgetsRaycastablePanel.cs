using UnityEngine;
using Rect = UnityEngine.Rect;

namespace Unity.UIWidgets.engine {
    [RequireComponent(typeof(RectTransform))]
    public class UIWidgetsRaycastablePanel : UIWidgetsPanel, ICanvasRaycastFilter {
        int windowHashCode;

        public override void mainEntry() {
            base.mainEntry();
            this.windowHashCode = wrapper.isolate.GetHashCode();
            RaycastManager.NewWindow(this.windowHashCode);
        }

        protected override void OnDisable() {
            base.OnDisable();
            RaycastManager.DisposeWindow(this.windowHashCode);
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
            if (!this.enabled) {
                return true;
            }

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, screenPoint, eventCamera,
                out local);

            Rect rect = this.rectTransform.rect;

            // Convert top left corner as reference origin point.
            local.x += this.rectTransform.pivot.x * rect.width;
            local.y -= this.rectTransform.pivot.y * rect.height;
            local.x = local.x / this._currentDevicePixelRatio;
            local.y = -local.y / this._currentDevicePixelRatio;

            return !RaycastManager.CheckCastThrough(this.windowHashCode, local);
        }
    }
}