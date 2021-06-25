using System;
using System.Collections.Generic;
using NUnit.Framework.Api;

namespace Unity.UIWidgets.DevTools
{
    public static class Globals
    {
        public static Dictionary<string, object> offlineDataJson = new Dictionary<string, object>(){};
        public static bool offlineMode = false;
        
        static Dictionary<Type, object> globals = new Dictionary<Type, object>();

        public static FrameworkController frameworkController => globals[typeof(FrameworkController)] as FrameworkController;
        public static ServiceConnectionManager serviceManager =>
        globals[typeof(ServiceConnectionManager)] as ServiceConnectionManager;
        
        public static void setGlobal(Type clazz, dynamic instance) {
            globals[clazz] = instance;
        }
    }
}