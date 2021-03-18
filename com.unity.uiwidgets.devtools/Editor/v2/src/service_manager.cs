namespace Unity.UIWidgets.DevTools
{
    public class ServiceConnectionManager
    {
        public ServiceConnectionManager()
        {
            _isolateManager = new IsolateManager();
        }
        
        public IsolateManager isolateManager => _isolateManager;
        IsolateManager _isolateManager;
    }

    public class IsolateManager
    {
        
    }
}