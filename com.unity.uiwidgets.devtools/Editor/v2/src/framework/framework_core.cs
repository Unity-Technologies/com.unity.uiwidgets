namespace Unity.UIWidgets.DevTools.framework
{
    public class FrameworkCore
    {
        public static void initGlobals()
        {
            // setGlobal(ServiceConnectionManager, ServiceConnectionManager());
            // setGlobal(MessageBus, MessageBus());
            Globals.setGlobal(typeof(FrameworkController), new FrameworkController());
        }
    }
}