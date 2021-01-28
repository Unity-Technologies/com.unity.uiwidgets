using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    public class ElevationOverlay {
        private ElevationOverlay() {
        }

        public static Color applyOverlay(BuildContext context, Color color, float elevation) {
            ThemeData theme = Theme.of(context);
            if (elevation > 0.0f &&
                theme.applyElevationOverlayColor &&
                color == theme.colorScheme.surface) {
                return Color.alphaBlend(overlayColor(context, elevation), color);
            }

            return color;
        }

        public static Color overlayColor(BuildContext context, float elevation) {
            ThemeData theme = Theme.of(context);

            float opacity = (4.5f * Mathf.Log(elevation + 1) + 2) / 100.0f;
            return theme.colorScheme.onSurface.withOpacity(opacity);
        }
    }
}