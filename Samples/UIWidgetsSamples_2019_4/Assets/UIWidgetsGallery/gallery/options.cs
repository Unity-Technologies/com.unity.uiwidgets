using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgetsGallery.gallery
{
    public static class GalleryOptionUtils
    {
        public const float _kItemHeight = 48.0f;

        //TODO: uncomment this when fixes on EdgeInsetsDirectional lands
        //public static readonly EdgeInsetsDirectional _kItemPadding = EdgeInsetsDirectional.only(start: 56.0f);
        public static readonly EdgeInsets _kItemPadding = EdgeInsets.only(left: 56.0f);
    }

    public class GalleryOptions : IEquatable<GalleryOptions>
    {
        public GalleryOptions(
            ThemeMode? themeMode = null,
            GalleryTextScaleValue textScaleFactor = null,
            GalleryVisualDensityValue visualDensity = null,
            TextDirection? textDirection = null,
            float timeDilation = 1.0f,
            RuntimePlatform? platform = null,
            bool showOffscreenLayersCheckerboard = false,
            bool showRasterCacheImagesCheckerboard = false,
            bool showPerformanceOverlay = false
        )
        {
            textDirection = textDirection ?? TextDirection.ltr;
            this.themeMode = themeMode;
            this.textScaleFactor = textScaleFactor;
            this.visualDensity = visualDensity;
            this.textDirection = textDirection;
            this.timeDilation = timeDilation;
            this.platform = platform;
            this.showOffscreenLayersCheckerboard = showOffscreenLayersCheckerboard;
            this.showRasterCacheImagesCheckerboard = showRasterCacheImagesCheckerboard;
            this.showPerformanceOverlay = showPerformanceOverlay;
        }

        public readonly ThemeMode? themeMode;
        public readonly GalleryTextScaleValue textScaleFactor;
        public readonly GalleryVisualDensityValue visualDensity;
        public readonly TextDirection? textDirection;
        public readonly float timeDilation;
        public readonly RuntimePlatform? platform;
        public readonly bool showPerformanceOverlay;
        public readonly bool showRasterCacheImagesCheckerboard;
        public readonly bool showOffscreenLayersCheckerboard;

        internal GalleryOptions copyWith(
            ThemeMode? themeMode = null,
            GalleryTextScaleValue textScaleFactor = null,
            GalleryVisualDensityValue visualDensity = null,
            TextDirection? textDirection = null,
            float? timeDilation = null,
            RuntimePlatform? platform = null,
            bool? showPerformanceOverlay = null,
            bool? showRasterCacheImagesCheckerboard = null,
            bool? showOffscreenLayersCheckerboard = null
        )
        {
            return new GalleryOptions(
                themeMode: themeMode ?? this.themeMode,
                textScaleFactor: textScaleFactor ?? this.textScaleFactor,
                visualDensity: visualDensity ?? this.visualDensity,
                textDirection: textDirection ?? this.textDirection,
                timeDilation: timeDilation ?? this.timeDilation,
                platform: platform ?? this.platform,
                showPerformanceOverlay: showPerformanceOverlay ?? this.showPerformanceOverlay,
                showOffscreenLayersCheckerboard:
                showOffscreenLayersCheckerboard ?? this.showOffscreenLayersCheckerboard,
                showRasterCacheImagesCheckerboard: showRasterCacheImagesCheckerboard ??
                                                   this.showRasterCacheImagesCheckerboard
            );
        }

        public bool Equals(GalleryOptions other)
        {
            if (ReferenceEquals(this, other)) return true;

            if (ReferenceEquals(other, null)) return false;

            return this.themeMode.Equals(other.themeMode)
                   && this.textScaleFactor == other.textScaleFactor
                   && this.visualDensity == other.visualDensity
                   && this.textDirection == other.textDirection
                   && this.platform == other.platform
                   && this.showPerformanceOverlay == other.showPerformanceOverlay
                   && this.showRasterCacheImagesCheckerboard == other.showRasterCacheImagesCheckerboard
                   && this.showOffscreenLayersCheckerboard == other.showOffscreenLayersCheckerboard;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            if (ReferenceEquals(obj, null)) return false;

            if (obj.GetType() != this.GetType()) return false;

            return this.Equals((GalleryOptions) obj);
        }

        public static bool operator ==(GalleryOptions left, GalleryOptions right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GalleryOptions left, GalleryOptions right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashcode = this.themeMode?.GetHashCode() ?? 0;
                hashcode = (hashcode * 397) ^ this.textScaleFactor.GetHashCode();
                hashcode = (hashcode * 397) ^ this.visualDensity.GetHashCode();
                hashcode = (hashcode * 397) ^ (this.textDirection?.GetHashCode() ?? 0);
                hashcode = (hashcode * 397) ^ this.timeDilation.GetHashCode();
                hashcode = (hashcode * 397) ^ (this.platform?.GetHashCode() ?? 0);
                hashcode = (hashcode * 397) ^ this.showPerformanceOverlay.GetHashCode();
                hashcode = (hashcode * 397) ^ this.showRasterCacheImagesCheckerboard.GetHashCode();
                hashcode = (hashcode * 397) ^ this.showOffscreenLayersCheckerboard.GetHashCode();
                return hashcode;
            }
        }

        public override string ToString()
        {
            return $"{this.GetType()}{this.themeMode}";
        }
    }

    internal class _OptionsItem : StatelessWidget
    {
        public _OptionsItem(Key key = null, Widget child = null) : base(key: key)
        {
            this.child = child;
        }

        public readonly Widget child;

        public override Widget build(BuildContext context)
        {
            float textScaleFactor = MediaQuery.textScaleFactorOf(context);

            return new Container(
                constraints: new BoxConstraints(minHeight: GalleryOptionUtils._kItemHeight * textScaleFactor),
                padding: GalleryOptionUtils._kItemPadding,
                alignment: AlignmentDirectional.centerStart,
                child: new DefaultTextStyle(
                    style: DefaultTextStyle.of(context).style,
                    maxLines: 2,
                    overflow: TextOverflow.fade,
                    child: new IconTheme(
                        data: Theme.of(context).primaryIconTheme,
                        child: this.child
                    )
                )
            );
        }
    }

    internal class _BooleanItem : StatelessWidget
    {
        public _BooleanItem(string title, bool value, ValueChanged<bool?> onChanged, Key switchKey = null)
        {
            this.title = title;
            this.value = value;
            this.onChanged = onChanged;
            this.switchKey = switchKey;
        }

        public readonly string title;
        public readonly bool value;
        public readonly ValueChanged<bool?> onChanged;
        public readonly Key switchKey;

        public override Widget build(BuildContext context)
        {
            bool isDark = Theme.of(context).brightness == Brightness.dark;
            return new _OptionsItem(
                child: new Row(
                    children: new List<Widget>
                    {
                        new Expanded(child: new Text(this.title)),
                        new Switch(
                            key: this.switchKey,
                            value: this.value,
                            onChanged: this.onChanged,
                            activeColor: new Color(0xFF39CEFD),
                            activeTrackColor: isDark ? Colors.white30 : Colors.black26
                        )
                    }
                )
            );
        }
    }

    internal class _ActionItem : StatelessWidget
    {
        public _ActionItem(string text, VoidCallback onTap)
        {
            this.text = text;
            this.onTap = onTap;
        }

        public readonly string text;
        public readonly VoidCallback onTap;

        public override Widget build(BuildContext context)
        {
            return new _OptionsItem(
                child: new _FlatButton(
                    onPressed: this.onTap,
                    child: new Text(this.text)
                )
            );
        }
    }

    internal class _FlatButton : StatelessWidget
    {
        public _FlatButton(Key key = null, VoidCallback onPressed = null, Widget child = null) : base(key: key)
        {
            this.child = child;
            this.onPressed = onPressed;
        }

        public readonly VoidCallback onPressed;
        public readonly Widget child;

        public override Widget build(BuildContext context)
        {
            return new FlatButton(
                padding: EdgeInsets.zero,
                onPressed: this.onPressed,
                child: new DefaultTextStyle(
                    style: Theme.of(context).primaryTextTheme.subtitle1,
                    child: this.child
                )
            );
        }
    }

    internal class _Heading : StatelessWidget
    {
        public _Heading(string text)
        {
            this.text = text;
        }

        public readonly string text;


        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            return new _OptionsItem(
                child: new DefaultTextStyle(
                    style: theme.textTheme.headline6.copyWith(
                        color: theme.colorScheme.onPrimary,
                        fontWeight: FontWeight.w700
                    ),
                    child: new Text(this.text)
                )
            );
        }
    }

    internal class _ThemeModeItem : StatelessWidget
    {
        public _ThemeModeItem(GalleryOptions options, ValueChanged<GalleryOptions> onOptionsChanged)
        {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;

        public static readonly Dictionary<ThemeMode, string> modeLabels = new Dictionary<ThemeMode, string>
        {
            {ThemeMode.system, "System Default"},
            {ThemeMode.light, "Light"},
            {ThemeMode.dark, "Dark"},
        };

        public override Widget build(BuildContext context)
        {
            return new _OptionsItem(
                child: new Row(
                    children: new List<Widget>
                    {
                        new Expanded(
                            child: new Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget>
                                {
                                    new Text("Theme"),
                                    new Text(
                                        modeLabels[this.options.themeMode.Value],
                                        style: Theme.of(context).primaryTextTheme.bodyText2
                                    )
                                }
                            )
                        ),
                        new PopupMenuButton<ThemeMode>(
                            //TODO: uncomment this when fixes on EdgeInsetsDirectional lands
                            //padding: const EdgeInsetsDirectional.only(end: 16.0),
                            padding: EdgeInsets.only(right: 16.0f),
                            icon: new Icon(Icons.arrow_drop_down),
                            initialValue: this.options.themeMode.Value,
                            itemBuilder: (BuildContext subContext) =>
                            {
                                return modeLabels.Keys.Select<ThemeMode, PopupMenuEntry<ThemeMode>>((ThemeMode mode) =>
                                {
                                    return new PopupMenuItem<ThemeMode>(
                                        value: mode,
                                        child: new Text(modeLabels[mode])
                                    );
                                }).ToList();
                            },
                            onSelected: (ThemeMode mode) =>
                            {
                                this.onOptionsChanged(this.options.copyWith(themeMode: mode)
                                );
                            }
                        )
                    }
                )
            );
        }
    }

    internal class _TextScaleFactorItem : StatelessWidget
    {
        public _TextScaleFactorItem(GalleryOptions options, ValueChanged<GalleryOptions> onOptionsChanged)
        {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;

        public override Widget build(BuildContext context)
        {
            return new _OptionsItem(
                child: new Row(
                    children: new List<Widget>
                    {
                        new Expanded(
                            child: new Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget>
                                {
                                    new Text("Text size"),
                                    new Text(this.options.textScaleFactor.label,
                                        style: Theme.of(context).primaryTextTheme.bodyText2
                                    )
                                }
                            )
                        ),
                        new PopupMenuButton<GalleryTextScaleValue>(
                            //TODO: uncomment this when fixes on EdgeInsetsDirectional lands
                            //padding: EdgeInsetsDirectional.only(end: 16.0f),
                            padding: EdgeInsets.only(right: 16.0f),
                            icon: new Icon(Icons.arrow_drop_down),
                            itemBuilder: (BuildContext subContext) =>
                            {
                                return GalleryTextScaleValue.kAllGalleryTextScaleValues
                                    .Select<GalleryTextScaleValue, PopupMenuEntry<GalleryTextScaleValue>>(
                                        (GalleryTextScaleValue scaleValue) =>
                                        {
                                            return new PopupMenuItem<GalleryTextScaleValue>(
                                                value: scaleValue,
                                                child: new Text(scaleValue.label)
                                            );
                                        }).ToList();
                            },
                            onSelected: (GalleryTextScaleValue scaleValue) =>
                            {
                                this.onOptionsChanged(this.options.copyWith(textScaleFactor: scaleValue)
                                );
                            }
                        )
                    }
                )
            );
        }
    }

    internal class _VisualDensityItem : StatelessWidget
    {
        public _VisualDensityItem(GalleryOptions options, ValueChanged<GalleryOptions> onOptionsChanged)
        {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;

        public override Widget build(BuildContext context)
        {
            return new _OptionsItem(
                child: new Row(
                    children: new List<Widget>
                    {
                        new Expanded(
                            child: new Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget>
                                {
                                    new Text("Visual density"),
                                    new Text(this.options.visualDensity.label,
                                        style: Theme.of(context).primaryTextTheme.bodyText2
                                    )
                                }
                            )
                        ),
                        new PopupMenuButton<GalleryVisualDensityValue>(
                            //TODO: uncomment this when fixes on EdgeInsetsDirectional lands
                            //padding: EdgeInsetsDirectional.only(end: 16.0f),
                            padding: EdgeInsets.only(right: 16.0f),
                            icon: new Icon(Icons.arrow_drop_down),
                            itemBuilder: (BuildContext subContext) =>
                            {
                                return GalleryVisualDensityValue.kAllGalleryVisualDensityValues
                                    .Select<GalleryVisualDensityValue, PopupMenuEntry<GalleryVisualDensityValue>>(
                                        (GalleryVisualDensityValue densityValue) =>
                                        {
                                            return new PopupMenuItem<GalleryVisualDensityValue>(
                                                value: densityValue,
                                                child: new Text(densityValue.label)
                                            );
                                        }).ToList();
                            },
                            onSelected: (GalleryVisualDensityValue densityValue) =>
                            {
                                this.onOptionsChanged(this.options.copyWith(visualDensity: densityValue)
                                );
                            }
                        )
                    }
                )
            );
        }
    }

    internal class _TextDirectionItem : StatelessWidget
    {
        public _TextDirectionItem(GalleryOptions options, ValueChanged<GalleryOptions> onOptionsChanged)
        {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;

        public override Widget build(BuildContext context)
        {
            return new _BooleanItem(
                "Force RTL", this.options.textDirection == TextDirection.rtl,
                (bool? value) =>
                {
                    this.onOptionsChanged(this.options.copyWith(
                            textDirection: value == true ? TextDirection.rtl : TextDirection.ltr
                        )
                    );
                },
                switchKey: Key.key("text_direction")
            );
        }
    }

    internal class _TimeDilationItem : StatelessWidget
    {
        public _TimeDilationItem(GalleryOptions options, ValueChanged<GalleryOptions> onOptionsChanged)
        {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;


        public override Widget build(BuildContext context)
        {
            return new _BooleanItem(
                "Slow motion", this.options.timeDilation != 1.0f,
                (bool? value) =>
                {
                    this.onOptionsChanged(this.options.copyWith(
                            timeDilation: value == true ? 20.0f : 1.0f
                        )
                    );
                },
                switchKey: Key.key("slow_motion")
            );
        }
    }

    internal class _PlatformItem : StatelessWidget
    {
        public _PlatformItem(GalleryOptions options, ValueChanged<GalleryOptions> onOptionsChanged)
        {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;

        private string _platformLabel(RuntimePlatform? platform)
        {
            return platform.ToString();
        }

        private static List<RuntimePlatform> _platforms = new List<RuntimePlatform>
            {RuntimePlatform.Android, RuntimePlatform.IPhonePlayer, RuntimePlatform.WindowsPlayer};

        public override Widget build(BuildContext context)
        {
            return new _OptionsItem(
                child: new Row(
                    children: new List<Widget>
                    {
                        new Expanded(
                            child: new Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget>
                                {
                                    new Text("Platform mechanics"),
                                    new Text(this._platformLabel(this.options.platform),
                                        style: Theme.of(context).primaryTextTheme.bodyText2
                                    )
                                }
                            )
                        ),
                        new PopupMenuButton<RuntimePlatform>(
                            //TODO: uncomment this when fixes on EdgeInsetsDirectional lands
                            //padding: EdgeInsetsDirectional.only(end: 16.0f),
                            padding: EdgeInsets.only(right: 16.0f),
                            icon: new Icon(Icons.arrow_drop_down),
                            itemBuilder: (BuildContext subContext) =>
                            {
                                return _platforms.Select<RuntimePlatform, PopupMenuEntry<RuntimePlatform>>(
                                    (RuntimePlatform platform) =>
                                    {
                                        return new PopupMenuItem<RuntimePlatform>(
                                            value: platform,
                                            child: new Text(this._platformLabel(platform))
                                        );
                                    }).ToList();
                            },
                            onSelected: (RuntimePlatform platform) =>
                            {
                                this.onOptionsChanged(this.options.copyWith(platform: platform)
                                );
                            }
                        )
                    }
                )
            );
        }
    }

    internal class GalleryOptionsPage : StatelessWidget
    {
        public GalleryOptionsPage(
            Key key = null,
            GalleryOptions options = null,
            ValueChanged<GalleryOptions> onOptionsChanged = null,
            VoidCallback onSendFeedback = null
        ) : base(key: key)
        {
            this.options = options;
            this.onOptionsChanged = onOptionsChanged;
            this.onSendFeedback = onSendFeedback;
        }

        public readonly GalleryOptions options;
        public readonly ValueChanged<GalleryOptions> onOptionsChanged;
        public readonly VoidCallback onSendFeedback;

        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);

            return new DefaultTextStyle(
                style: theme.primaryTextTheme.subtitle1,
                child: new ListView(
                    padding: EdgeInsets.only(bottom: 124.0f),
                    children: new List<Widget>
                    {
                        new _Heading("Display"),
                        new _ThemeModeItem(this.options, this.onOptionsChanged),
                        new _TextScaleFactorItem(this.options, this.onOptionsChanged),
                        new _VisualDensityItem(this.options, this.onOptionsChanged),
                        new _TextDirectionItem(this.options, this.onOptionsChanged),
                        new _TimeDilationItem(this.options, this.onOptionsChanged),
                        new Divider(),
                        new _PlatformItem(this.options, this.onOptionsChanged),
                        new Divider(),
                        new _Heading("Flutter gallery"),
                        new _ActionItem("About Flutter Gallery",
                            () => { GalleryAboutUtils.showGalleryAboutDialog(context); }),
                        new _ActionItem("Send feedback", this.onSendFeedback)
                    }
                )
            );
        }
    }
}