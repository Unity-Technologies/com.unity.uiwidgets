using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    public class BackButtonIcon : StatelessWidget {
        public BackButtonIcon(
            Key key = null) : base(key: key) {
        }

        static IconData _getIconData(RuntimePlatform platform) {
            switch (platform) {
                case RuntimePlatform.Android:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return Icons.arrow_back;
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return Icons.arrow_back_ios;
                default:
                    return Icons.arrow_back;
            }
        }

        public override Widget build(BuildContext context) {
            return new Icon(_getIconData(Theme.of(context).platform));
        }
    }

    public class BackButton : StatelessWidget {
        public BackButton(
            Key key = null,
            Color color = null,
            VoidCallback onPressed = null) : base(key: key) {
            this.color = color;
            this.onPressed = onPressed;
        }

        public readonly Color color;
        public readonly VoidCallback onPressed;

        public override Widget build(BuildContext context) {
            return new IconButton(
                icon: new BackButtonIcon(),
                color: color,
                tooltip: MaterialLocalizations.of(context).backButtonTooltip,
                onPressed: () => {
                    if (onPressed != null) {
                        onPressed();
                    }
                    else {
                        Navigator.maybePop(context);
                    }
                });
        }
    }

    public class CloseButton : StatelessWidget {
        public CloseButton(
            Key key = null,
            Color color = null,
            VoidCallback onPressed = null) : base(key: key) {
            this.color = color;
            this.onPressed = onPressed;
        }

        public readonly Color color;

        public readonly VoidCallback onPressed;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            return new IconButton(
                icon: new Icon(Icons.close),
                color: color,
                tooltip: MaterialLocalizations.of(context).closeButtonTooltip,
                onPressed: () => {
                    if (onPressed != null) {
                        onPressed();
                    }
                    else {
                        Navigator.maybePop(context);
                    }
                });
        }
    }
}