using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.DevTools
{
    public static class Globals
    {
        public static bool offlineMode = false;
        
        static Dictionary<Type, object> globals = new Dictionary<Type, object>();

        public static ServiceConnectionManager serviceManager =>
        globals[typeof(ServiceConnectionManager)] as ServiceConnectionManager;
    }
}