using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    class RouteNotificationMessages {
        public RouteNotificationMessages() {
        }

        /// When the engine is Web notify the platform for a route change.
        /*public static void maybeNotifyRouteChange(string routeName, string previousRouteName) {
            if(kIsWeb) {
                _notifyRouteChange(routeName, previousRouteName);
            } else {
            // No op.
            }
        }

        
        public static void _notifyRouteChange(string routeName, string previousRouteName) {
            SystemChannels.navigation.invokeMethod<void>(
            'routeUpdated',
            <string, dynamic>{
                'previousRouteName': previousRouteName,
                'routeName': routeName,
            },
            );
        }*/
        }


}