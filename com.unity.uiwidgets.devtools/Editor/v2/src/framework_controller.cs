namespace Unity.UIWidgets.DevTools
{
    public class FrameworkController
    {
        public FrameworkController()
        {
            _init();
        }
        
        // StreamController<string> _pageChangeController =
        // StreamController.broadcast();
        
        void _init() 
        {
            
        }
        
        public void notifyPageChange(string pageId) {
            // _pageChangeController.add(pageId);
        }
        
    }
    
}