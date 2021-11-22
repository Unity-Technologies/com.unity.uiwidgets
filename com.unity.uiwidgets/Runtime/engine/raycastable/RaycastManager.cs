using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.engine {
    public class RaycastableRect {
        bool _isDirty = true;

        public bool isDirty {
            get { return _isDirty; }
        }

        public float left;
        public float right;
        public float top;
        public float bottom;

        public void MarkDirty() {
            _isDirty = true;
        }

        public void UnmarkDirty() {
            _isDirty = false;
        }

        public void UpdateRect(float left, float top, float width, float height) {
            this.left = left;
            right = left + width;
            this.top = top;
            bottom = top + height;
        }

        public bool CheckInRect(Vector2 pos) {
            return pos.x >= left &&
                   pos.x < right &&
                   pos.y >= top &&
                   pos.y < bottom;
        }
    }

    public class RaycastManager {
        static RaycastManager _instance;

        public static RaycastManager instance {
            get {
                if (_instance == null) {
                    _instance = new RaycastManager();
                }

                return _instance;
            }
        }

        public readonly Dictionary<int, Dictionary<int, RaycastableRect>> raycastHandlerMap =
            new Dictionary<int, Dictionary<int, RaycastableRect>>();

        public static void NewWindow(int windowHashCode) {
            if (!instance.raycastHandlerMap.ContainsKey(windowHashCode)) {
                instance.raycastHandlerMap.Add(windowHashCode, new Dictionary<int, RaycastableRect>());
            }
        }

        public static void DisposeWindow(int windowHashCode) {
            if (instance.raycastHandlerMap.ContainsKey(windowHashCode)) {
                instance.raycastHandlerMap.Remove(windowHashCode);
            }
        }

        public static void AddToList(int widgetHashCode, int windowHashCode) {
            D.assert(instance.raycastHandlerMap.ContainsKey(windowHashCode), () =>
                $"Raycast Handler Map doesn't contain Window {windowHashCode}, " +
                $"Make sure using UIWidgetsRaycastablePanel instead of UIWidgetsPanel " +
                $"while using RaycastableContainer.");
            D.assert(!instance.raycastHandlerMap[windowHashCode].ContainsKey(widgetHashCode), () =>
                $"Raycast Handler Map already contains Widget {widgetHashCode} at Window {windowHashCode}");

            instance.raycastHandlerMap[windowHashCode][widgetHashCode] = new RaycastableRect();
        }

        public static void OnScreenSizeChanged(int windowHashCode) {
            if (!instance.raycastHandlerMap.ContainsKey(windowHashCode)) return;
            Dictionary<int, RaycastableRect> raycastableWidgets = RaycastManager.instance.raycastHandlerMap[windowHashCode];
            foreach (var item in raycastableWidgets) {
                MarkDirty(item.Key, windowHashCode);
            }
        }

        public static void MarkDirty(int widgetHashCode, int windowHashCode) {
            D.assert(instance.raycastHandlerMap.ContainsKey(windowHashCode), () =>
                $"Raycast Handler Map doesn't contain Window {windowHashCode}");
            D.assert(instance.raycastHandlerMap[windowHashCode].ContainsKey(widgetHashCode), () =>
                $"Raycast Handler Map doesn't contain Widget {widgetHashCode} at Window {windowHashCode}");

            instance.raycastHandlerMap[windowHashCode][widgetHashCode].MarkDirty();
        }

        public static void UpdateSizeOffset(int widgetHashCode, int windowHashCode, Size size, Offset offset) {
            D.assert(instance.raycastHandlerMap.ContainsKey(windowHashCode), () =>
                $"Raycast Handler Map doesn't contain Window {windowHashCode}");
            D.assert(instance.raycastHandlerMap[windowHashCode].ContainsKey(widgetHashCode), () =>
                $"Raycast Handler Map doesn't contain Widget {widgetHashCode} at Window {windowHashCode}");

            if (instance.raycastHandlerMap[windowHashCode][widgetHashCode].isDirty) {
                instance.raycastHandlerMap[windowHashCode][widgetHashCode]
                    .UpdateRect(offset.dx, offset.dy, size.width, size.height);
                instance.raycastHandlerMap[windowHashCode][widgetHashCode].UnmarkDirty();
            }
        }

        public static void RemoveFromList(int widgetHashCode, int windowHashCode) {
            D.assert(instance.raycastHandlerMap.ContainsKey(windowHashCode), () =>
                $"Raycast Handler Map doesn't contain Window {windowHashCode}");
            D.assert(instance.raycastHandlerMap[windowHashCode].ContainsKey(widgetHashCode), () =>
                $"Raycast Handler Map doesn't contain Widget {widgetHashCode} at Window {windowHashCode}");

            instance.raycastHandlerMap[windowHashCode].Remove(widgetHashCode);
        }

        public static bool CheckCastThrough(int windowHashCode, Vector2 pos) {
            D.assert(instance.raycastHandlerMap.ContainsKey(windowHashCode), () =>
                $"Raycast Handler Map doesn't contain Window {windowHashCode}");

            foreach (var item in instance.raycastHandlerMap[windowHashCode]) {
                if (item.Value.CheckInRect(pos)) {
                    return false;
                }
            }

            return true;
        }
    }
}