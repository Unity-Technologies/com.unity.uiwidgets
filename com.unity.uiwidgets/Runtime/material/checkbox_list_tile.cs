using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class CheckboxListTile : StatelessWidget {
        public CheckboxListTile(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color checkColor = null,
            Widget title = null,
            Widget subtitle = null,
            bool isThreeLine = false,
            bool? dense = null,
            Widget secondary = null,
            bool selected = false,
            ListTileControlAffinity controlAffinity = ListTileControlAffinity.platform
        ) : base(key: key) {
            D.assert(value != null);
            D.assert(!isThreeLine || subtitle != null);
            this.checkColor = checkColor;
            this.title = title;
            this.subtitle = subtitle;
            this.isThreeLine = isThreeLine;
            this.dense = dense;
            this.secondary = secondary;
            this.selected = selected;
            this.controlAffinity = controlAffinity;
            this.value = value;
            this.activeColor = activeColor;
            this.onChanged = onChanged;
        }

        public readonly bool? value;
        public readonly ValueChanged<bool?> onChanged;
        public readonly Color activeColor;
        public readonly Color checkColor;
        public readonly Widget title;
        public readonly Widget subtitle;
        public readonly Widget secondary;
        public readonly bool isThreeLine;
        public readonly bool? dense;
        public readonly bool selected;
        public readonly ListTileControlAffinity controlAffinity;

        public override Widget build(BuildContext context) {
            Widget control = new Checkbox(
                value: value,
                onChanged: onChanged,
                activeColor: activeColor,
                checkColor: checkColor,
                materialTapTargetSize: MaterialTapTargetSize.shrinkWrap
            );
            Widget leading = null;
            Widget trailing = null;
            switch (controlAffinity) {
                case ListTileControlAffinity.leading:
                    leading = control;
                    trailing = secondary;
                    break;
                case ListTileControlAffinity.trailing:
                case ListTileControlAffinity.platform:
                    leading = secondary;
                    trailing = control;
                    break;
            }

            return ListTileTheme.merge(
                selectedColor: activeColor ?? Theme.of(context).accentColor,
                child: new ListTile(
                    leading: leading,
                    title: title,
                    subtitle: subtitle,
                    trailing: trailing,
                    isThreeLine: isThreeLine,
                    dense: dense,
                    enabled: onChanged != null,
                    onTap: onChanged != null ? () => { onChanged(!value); } : (GestureTapCallback) null,
                    selected: selected
                )
            );
        }
    }
}