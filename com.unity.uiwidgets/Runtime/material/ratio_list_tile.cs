using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class RadioListTile<T> : StatelessWidget {
        public RadioListTile(
            Key key = null,
            T value = default,
            T groupValue = default,
            ValueChanged<T> onChanged = null,
            bool toggleable = false,
            Color activeColor = null,
            Widget title = null,
            Widget subtitle = null,
            bool isThreeLine = false,
            bool? dense = null,
            Widget secondary = null,
            bool selected = false,
            ListTileControlAffinity controlAffinity = ListTileControlAffinity.platform
        ) : base(key: key) {
            D.assert(!isThreeLine || subtitle != null);

            this.value = value;
            this.groupValue = groupValue;
            this.onChanged = onChanged;
            this.toggleable = toggleable;
            this.activeColor = activeColor;
            this.title = title;
            this.subtitle = subtitle;
            this.isThreeLine = isThreeLine;
            this.dense = dense;
            this.secondary = secondary;
            this.selected = selected;
            this.controlAffinity = controlAffinity;
        }

        public readonly T value;

        public readonly T groupValue;

        public readonly ValueChanged<T> onChanged;

        public readonly bool toggleable;

        public readonly Color activeColor;

        public readonly Widget title;

        public readonly Widget subtitle;

        public readonly Widget secondary;

        public readonly bool isThreeLine;

        public readonly bool? dense;

        public readonly bool selected;

        public readonly ListTileControlAffinity controlAffinity;

        bool isChecked {
            get { return value.Equals(groupValue); }
        }

        public override Widget build(BuildContext context) {
            Widget control = new Radio<T>(
                value: value,
                groupValue: groupValue,
                onChanged: onChanged,
                toggleable: toggleable,
                activeColor: activeColor,
                materialTapTargetSize: MaterialTapTargetSize.shrinkWrap
            );
            Widget leading = null;
            Widget trailing = null;
            switch (controlAffinity) {
                case ListTileControlAffinity.leading:
                case ListTileControlAffinity.platform:
                    leading = control;
                    trailing = secondary;
                    break;
                case ListTileControlAffinity.trailing:
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
                    onTap: onChanged != null
                        ? () => {
                            if (toggleable && isChecked) {
                                onChanged(default);
                                return;
                            }

                            if (!isChecked) {
                                onChanged(value);
                            }
                        }
                        : (GestureTapCallback) null,
                    selected: selected
                )
            );
        }
    }
}