using System.Collections.Generic;

namespace Unity.UIWidgets.DevTools
{
    public class ServiceConnectionManager
    {
        public ServiceConnectionManager()
        {
            _isolateManager = new IsolateManager();
            ServiceExtensionManager serviceExtensionManager = new ServiceExtensionManager();
            isolateManager._serviceExtensionManager = serviceExtensionManager;
            serviceExtensionManager._isolateManager = isolateManager;
            _isolateManager = isolateManager;
            _serviceExtensionManager = serviceExtensionManager;
        }
        
        public IsolateManager isolateManager => _isolateManager;
        IsolateManager _isolateManager;
        
        public ServiceExtensionManager serviceExtensionManager =>
        _serviceExtensionManager;
        ServiceExtensionManager _serviceExtensionManager;
        
    }

    public class IsolateManager
    {
        public ServiceExtensionManager _serviceExtensionManager;
    }

    public class ServiceExtensionManager
    {
        public IsolateManager _isolateManager;
        List<string> _serviceExtensions = new List<string>();
        List<string> _pendingServiceExtensions = new List<string>();
        
        public bool isServiceExtensionAvailable(string name) {
            return _serviceExtensions.Contains(name) ||
                   _pendingServiceExtensions.Contains(name);
        }
    }
}