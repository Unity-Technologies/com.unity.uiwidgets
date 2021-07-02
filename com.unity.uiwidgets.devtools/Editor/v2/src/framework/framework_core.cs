namespace Unity.UIWidgets.DevTools.framework
{
    public class FrameworkCore
    {
        public static void initGlobals()
        {
            Globals.setGlobal(typeof(ServiceConnectionManager), new ServiceConnectionManager());
            // Globals.setGlobal(MessageBus, MessageBus());
            Globals.setGlobal(typeof(FrameworkController), new FrameworkController());
        }
    }
}