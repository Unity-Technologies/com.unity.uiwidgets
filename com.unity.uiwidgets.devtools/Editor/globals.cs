/*using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.DevTools
{
    public class globals
    {
        public static bool offlineMode = false;

        // TODO(kenz): store this data in an inherited widget.
        Dictionary<string, object> offlineDataJson = new Dictionary<string, object>();

        public static readonly Dictionary<Type, object> _globals = new Dictionary<Type, object>();

        
        
        public static ServiceConnectionManager serviceManager
        {
            get
            {
                return _globals[ServiceConnectionManager];
            }
        }

        // MessageBus get messageBus => globals[MessageBus];
        //
        // FrameworkController get frameworkController => globals[FrameworkController];
        //
        // Storage get storage => globals[Storage];
        //
        // SurveyService get surveyService => globals[SurveyService];
        //
         public static PreferencesController preferences
         {
             get
             {
                 return _globals[PreferencesController];
             }
         }

         // string generateDevToolsTitle() {
        // if (!serviceManager.hasConnection) return " ";
        // return serviceManager.connectedApp.isFlutterAppNow
        // ? "Flutter DevTools"
        // : "Dart DevTools";
        // }
        //
        // void setGlobal(Type clazz, dynamic instance) {
        //     globals[clazz] = instance;
        // }
    }
}*/