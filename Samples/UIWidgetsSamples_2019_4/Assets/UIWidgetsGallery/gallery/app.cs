using System;
using System.Collections.Generic;
using uiwidgets;
using UIWidgetsGallery.demo.shrine.model;
using Unity.UIWidgets.async;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.gallery
{
    public class GalleryApp : StatefulWidget
    { public GalleryApp(
            Key key = null,
            UpdateUrlFetcher updateUrlFetcher = null,
            bool enablePerformanceOverlay = true,
            bool enableRasterCacheImagesCheckerboard = true,
            bool enableOffscreenLayersCheckerboard = true,
            VoidCallback onSendFeedback = null,
            bool testMode = false
        ) : base(key: key)
        {
            this.updateUrlFetcher = updateUrlFetcher;
            this.enablePerformanceOverlay = enablePerformanceOverlay;
            this.enableRasterCacheImagesCheckerboard = enableRasterCacheImagesCheckerboard;
            this.enableOffscreenLayersCheckerboard = enableOffscreenLayersCheckerboard;
            this.onSendFeedback = onSendFeedback;
            this.testMode = testMode;
        }

        public readonly UpdateUrlFetcher updateUrlFetcher;
        public readonly bool enablePerformanceOverlay;
        public readonly bool enableRasterCacheImagesCheckerboard;
        public readonly bool enableOffscreenLayersCheckerboard;
        public readonly VoidCallback onSendFeedback;
        public readonly bool testMode;

        public override State createState()
        {
            return new _GalleryAppState();
        }
    }

    internal class _GalleryAppState : State<GalleryApp>
    {
        private GalleryOptions _options;
        private Timer _timeDilationTimer;
        private AppStateModel model;

        private Dictionary<string, WidgetBuilder> _buildRoutes()
        {
            Dictionary<string, WidgetBuilder> routeBulders = new Dictionary<string, WidgetBuilder>();

            foreach (var demo in GalleryDemo.kAllGalleryDemos) routeBulders.Add(demo.routeName, demo.buildRoute);

            return routeBulders;
        }

        private static readonly RuntimePlatform defaultTargetPlatform = RuntimePlatform.WindowsPlayer;


        public override void initState()
        {
            base.initState();
            this._options = new GalleryOptions(
                themeMode: ThemeMode.system,
                textScaleFactor: GalleryTextScaleValue.kAllGalleryTextScaleValues[0],
                visualDensity: GalleryVisualDensityValue.kAllGalleryVisualDensityValues[0],
                timeDilation: scheduler_.timeDilation,
                platform: defaultTargetPlatform
            );
            this.model = new AppStateModel();
            this.model.loadProducts();
        }

        public override void reassemble()
        {
            this._options = this._options.copyWith(platform: defaultTargetPlatform);
            base.reassemble();
        }

        public override void dispose()
        {
            this._timeDilationTimer?.cancel();
            this._timeDilationTimer = null;
            base.dispose();
        }

        private void _handleOptionsChanged(GalleryOptions newOptions)
        {
            this.setState(() =>
            {
                if (this._options.timeDilation != newOptions.timeDilation)
                {
                    this._timeDilationTimer?.cancel();
                    this._timeDilationTimer = null;
                    if (newOptions.timeDilation > 1.0f
                        ) // We delay the time dilation change long enough that the user can see
                        // that UI has started reacting and then we slam on the brakes so that
                        // they see that the time is in fact now dilated.
                        this._timeDilationTimer = Timer.create(new TimeSpan(0, 0, 0, 0, 150),
                            () => { scheduler_.timeDilation = newOptions.timeDilation; });
                    else
                        scheduler_.timeDilation = newOptions.timeDilation;
                }

                this._options = newOptions;
            });
        }

        private Widget _applyTextScaleFactor(Widget child)
        {
            return new Builder(
                builder: (BuildContext context) =>
                {
                    return new MediaQuery(
                        data: MediaQuery.of(context).copyWith(
                            textScaleFactor: this._options.textScaleFactor.scale
                        ),
                        child: child
                    );
                }
            );
        }

        private static void defaultSendFeedback()
        {
            Debug.Log("hello UIWidgets !");
        }

        public override Widget build(BuildContext context)
        {
            Widget home = new GalleryHome(
                testMode: this.widget.testMode,
                optionsPage: new GalleryOptionsPage(
                    options: this._options,
                    onOptionsChanged: this._handleOptionsChanged,
                    onSendFeedback: this.widget.onSendFeedback ?? defaultSendFeedback
                )
            );

            if (this.widget.updateUrlFetcher != null)
                home = new Updater(
                    updateUrlFetcher: this.widget.updateUrlFetcher,
                    child: home
                );

            return new ScopedModel<AppStateModel>(
                model: this.model,
                child: new MaterialApp(
                    theme: GalleyThemes.kLightGalleryTheme.copyWith(platform: this._options.platform,
                        visualDensity: this._options.visualDensity.visualDensity),
                    darkTheme: GalleyThemes.kDarkGalleryTheme.copyWith(platform: this._options.platform,
                        visualDensity: this._options.visualDensity.visualDensity),
                    themeMode: this._options.themeMode.Value,
                    title: "Flutter Gallery",
                    color: Colors.grey,
                    showPerformanceOverlay: this._options.showPerformanceOverlay,
                    checkerboardOffscreenLayers: this._options.showOffscreenLayersCheckerboard,
                    checkerboardRasterCacheImages: this._options.showRasterCacheImagesCheckerboard,
                    routes: this._buildRoutes(),
                    builder: (BuildContext subContext, Widget child) =>
                    {
                        return new Directionality(
                            textDirection: this._options.textDirection.Value,
                            child: this._applyTextScaleFactor(
                                // Specifically use a blank Cupertino theme here and do not transfer
                                // over the Material primary color etc except the brightness to
                                // showcase standard iOS looks.
                                new Builder(builder: (BuildContext context1) =>
                                {
                                    return new CupertinoTheme(
                                        data: new CupertinoThemeData(
                                            brightness: Theme.of(context1).brightness
                                        ),
                                        child: child
                                    );
                                })
                            )
                        );
                    },
                    home: home
                )
            );
        }
    }
}