using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public enum _SwitchListTileType {
        material,
        adaptive
    }

    public class SwitchListTile : StatelessWidget {
        public SwitchListTile(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color activeTrackColor = null,
            Color inactiveThumbColor = null,
            Color inactiveTrackColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            Widget title = null,
            Widget subtitle = null,
            bool isThreeLine = false,
            bool? dense = null,
            EdgeInsetsGeometry contentPadding = null,
            Widget secondary = null,
            bool selected = false,
            _SwitchListTileType _switchListTileType = _SwitchListTileType.material
        ) : base(key: key) {
            D.assert(value != null);
            D.assert(!isThreeLine || subtitle != null);
            this.value = value.Value;
            this.onChanged = onChanged;
            this.activeColor = activeColor;
            this.activeTrackColor = activeTrackColor;
            this.inactiveThumbColor = inactiveThumbColor;
            this.inactiveTrackColor = inactiveTrackColor;
            this.activeThumbImage = activeThumbImage;
            this.inactiveThumbImage = inactiveThumbImage;
            this.title = title;
            this.subtitle = subtitle;
            this.isThreeLine = isThreeLine;
            this.dense = dense;
            this.contentPadding = contentPadding;
            this.secondary = secondary;
            this.selected = selected;
            this._switchListTileType = _switchListTileType;
        }

        public static SwitchListTile adaptive(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color activeTrackColor = null,
            Color inactiveThumbColor = null,
            Color inactiveTrackColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            Widget title = null,
            Widget subtitle = null,
            bool isThreeLine = false,
            bool? dense = null,
            EdgeInsets contentPadding = null,
            Widget secondary = null,
            bool selected = false) {
            return new SwitchListTile(
                key: key,
                value: value,
                onChanged: onChanged,
                activeColor: activeColor,
                activeTrackColor: activeTrackColor,
                inactiveThumbColor: inactiveThumbColor,
                inactiveTrackColor: inactiveTrackColor,
                activeThumbImage: activeThumbImage,
                inactiveThumbImage: inactiveThumbImage,
                title: title,
                subtitle: subtitle,
                isThreeLine: isThreeLine,
                dense: dense,
                contentPadding: contentPadding,
                secondary: secondary,
                selected: selected,
                _switchListTileType: _SwitchListTileType.adaptive
            );
        }


        public readonly bool value;

        public readonly ValueChanged<bool?> onChanged;


        public readonly Color activeColor;


        public readonly Color activeTrackColor;


        public readonly Color inactiveThumbColor;


        public readonly Color inactiveTrackColor;


        public readonly ImageProvider activeThumbImage;


        public readonly ImageProvider inactiveThumbImage;


        public readonly Widget title;


        public readonly Widget subtitle;


        public readonly Widget secondary;


        public readonly bool isThreeLine;


        public readonly bool? dense;


        public readonly EdgeInsetsGeometry contentPadding;


        public readonly bool selected;


        public readonly _SwitchListTileType _switchListTileType;

        public override Widget build(BuildContext context) {
            Widget control = null;
            switch (_switchListTileType) {
                case _SwitchListTileType.adaptive:
                    control = Switch.adaptive(
                        value: value,
                        onChanged: onChanged,
                        activeColor: activeColor,
                        activeThumbImage: activeThumbImage,
                        inactiveThumbImage: inactiveThumbImage,
                        materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                        activeTrackColor: activeTrackColor,
                        inactiveTrackColor: inactiveTrackColor,
                        inactiveThumbColor: inactiveThumbColor
                    );
                    break;

                case _SwitchListTileType.material:
                    control = new Switch(
                        value: value,
                        onChanged: onChanged,
                        activeColor: activeColor,
                        activeThumbImage: activeThumbImage,
                        inactiveThumbImage: inactiveThumbImage,
                        materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                        activeTrackColor: activeTrackColor,
                        inactiveTrackColor: inactiveTrackColor,
                        inactiveThumbColor: inactiveThumbColor
                    );
                    break;
            }

            return ListTileTheme.merge(
                selectedColor: activeColor ?? Theme.of(context).accentColor,
                child: new ListTile(
                    leading: secondary,
                    title: title,
                    subtitle: subtitle,
                    trailing: control,
                    isThreeLine: isThreeLine,
                    dense: dense,
                    contentPadding: contentPadding,
                    enabled: onChanged != null,
                    onTap: onChanged != null ? () => { onChanged(!value); } : (GestureTapCallback) null,
                    selected: selected
                )
            );
        }
    }
}