using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.service {
    public enum DeviceOrientation {
        potraitUp,
        landscapeLeft,
        portraitDown,
        landscapeRight
    }


    public class ApplicationSwitcherDescription {
        public ApplicationSwitcherDescription(
            string label = null,
            int? primaryColor = null
        ) {
            this.label = label;
            this.primaryColor = primaryColor;
        }

        public readonly string label;

        public readonly int? primaryColor;
    }

    public enum SystemUiOverlay {
        top,
        bottom
    }

    public class SystemUiOverlayStyle {
        public SystemUiOverlayStyle(
            Color systemNavigationBarColor = null,
            Color systemNavigationBarDividerColor = null,
            Brightness? systemNavigationBarIconBrightness = null,
            Color statusBarColor = null,
            Brightness? statusBarBrightness = null,
            Brightness? statusBarIconBrightness = null
        ) {
            this.systemNavigationBarColor = systemNavigationBarColor;
            this.systemNavigationBarDividerColor = systemNavigationBarDividerColor;
            this.systemNavigationBarIconBrightness = systemNavigationBarIconBrightness;
            this.statusBarColor = statusBarColor;
            this.statusBarBrightness = statusBarBrightness;
            this.statusBarIconBrightness = statusBarIconBrightness;
        }

        public readonly Color systemNavigationBarColor;

        public readonly Color systemNavigationBarDividerColor;

        public readonly Brightness? systemNavigationBarIconBrightness;

        public readonly Color statusBarColor;

        public readonly Brightness? statusBarBrightness;

        public readonly Brightness? statusBarIconBrightness;

        public static readonly SystemUiOverlayStyle light = new SystemUiOverlayStyle(
            systemNavigationBarColor: new Color(0xFF000000),
            systemNavigationBarDividerColor: null,
            statusBarColor: null,
            systemNavigationBarIconBrightness: Brightness.light,
            statusBarIconBrightness: Brightness.light,
            statusBarBrightness: Brightness.dark
        );

        public static readonly SystemUiOverlayStyle dark = new SystemUiOverlayStyle(
            systemNavigationBarColor: new Color(0xFF000000),
            systemNavigationBarDividerColor: null,
            statusBarColor: null,
            systemNavigationBarIconBrightness: Brightness.light,
            statusBarIconBrightness: Brightness.dark,
            statusBarBrightness: Brightness.light
        );

        public Dictionary<string, object> _toMap() {
            return new Dictionary<string, object> {
                {"systemNavigationBarColor", systemNavigationBarColor?.value},
                {"systemNavigationBarDividerColor", systemNavigationBarDividerColor?.value},
                {"statusBarColor", statusBarColor?.value},
                {"statusBarBrightness", statusBarBrightness?.ToString()},
                {"statusBarIconBrightness", statusBarIconBrightness?.ToString()},
                {"systemNavigationBarIconBrightness", systemNavigationBarIconBrightness?.ToString()}
            };
        }

        public override string ToString() {
            return _toMap().ToString();
        }

        public SystemUiOverlayStyle copyWith(
            Color systemNavigationBarColor = null,
            Color systemNavigationBarDividerColor = null,
            Color statusBarColor = null,
            Brightness? statusBarBrightness = null,
            Brightness? statusBarIconBrightness = null,
            Brightness? systemNavigationBarIconBrightness = null
        ) {
            return new SystemUiOverlayStyle(
                systemNavigationBarColor: systemNavigationBarColor ?? this.systemNavigationBarColor,
                systemNavigationBarDividerColor:
                systemNavigationBarDividerColor ?? this.systemNavigationBarDividerColor,
                statusBarColor: statusBarColor ?? this.statusBarColor,
                statusBarIconBrightness: statusBarIconBrightness ?? this.statusBarIconBrightness,
                statusBarBrightness: statusBarBrightness ?? this.statusBarBrightness,
                systemNavigationBarIconBrightness: systemNavigationBarIconBrightness ??
                                                   this.systemNavigationBarIconBrightness
            );
        }

        public override int GetHashCode() {
            var hashCode = systemNavigationBarColor.GetHashCode();
            hashCode = (hashCode * 397) ^ (systemNavigationBarDividerColor != null
                           ? systemNavigationBarDividerColor.GetHashCode()
                           : 0);
            hashCode = (hashCode * 397) ^ (statusBarColor != null ? statusBarColor.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^
                       (statusBarBrightness != null ? statusBarBrightness.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^
                       (statusBarIconBrightness != null ? statusBarIconBrightness.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (systemNavigationBarIconBrightness != null
                           ? systemNavigationBarIconBrightness.GetHashCode()
                           : 0);
            return hashCode;
        }


        public bool Equals(SystemUiOverlayStyle other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.systemNavigationBarColor == systemNavigationBarColor &&
                   other.systemNavigationBarDividerColor == systemNavigationBarDividerColor &&
                   other.statusBarColor == statusBarColor &&
                   other.statusBarIconBrightness == statusBarIconBrightness &&
                   other.statusBarBrightness == statusBarIconBrightness &&
                   other.systemNavigationBarIconBrightness == systemNavigationBarIconBrightness;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((SystemUiOverlayStyle) obj);
        }

        public static bool operator ==(SystemUiOverlayStyle left, SystemUiOverlayStyle right) {
            return Equals(left, right);
        }

        public static bool operator !=(SystemUiOverlayStyle left, SystemUiOverlayStyle right) {
            return !Equals(left, right);
        }
    }
    public class SystemChrome {
        /*public static Future setPreferredOrientations(List<DeviceOrientation> orientations)  {
            return SystemChannels.platform.invokeMethod(
              "SystemChrome.setPreferredOrientations",
              _stringify(orientations)
            );
        }

        public static Future setApplicationSwitcherDescription(ApplicationSwitcherDescription description){
          return SystemChannels.platform.invokeMethod(
                "SystemChrome.setApplicationSwitcherDescription",
                new Dictionary<string, object>{
                  {"label",description.label},
                  {"primaryColor", description.primaryColor},
                }
            );
        }

        static Future setEnabledSystemUIOverlays(List<SystemUiOverlay> overlays) {
          return SystemChannels.platform.invokeMethod(
                "SystemChrome.setEnabledSystemUIOverlays",
                _stringify(overlays)
            );
        }
        public static Future restoreSystemUIOverlays()  {
          return SystemChannels.platform.invokeMethod(
            "SystemChrome.restoreSystemUIOverlays",
            null
            );
        }

        public static void setSystemUIOverlayStyle(SystemUiOverlayStyle style) {
            D.assert(style != null);
            if (_pendingStyle != null) {
                _pendingStyle = style;
                return;
            } 
            if (style == _latestStyle) {
                return;
            } 
            _pendingStyle = style;
            scheduleMicrotask(()=> { 
                D.assert(_pendingStyle != null); 
                if (_pendingStyle != _latestStyle) {
                  return SystemChannels.platform.invokeMethod(
                        "SystemChrome.setSystemUIOverlayStyle",
                        _pendingStyle._toMap()
                    );
                    _latestStyle = _pendingStyle;
                }
                _pendingStyle = null;
            });
        }*/
        public static SystemUiOverlayStyle _pendingStyle; 
        public static SystemUiOverlayStyle latestStyle {
            get {
                return _latestStyle;
            }
        }
        static SystemUiOverlayStyle _latestStyle;
    }

}