namespace Unity.UIWidgets.DevTools.analytics
{
    public abstract class AnalyticsProvider {
        private bool isGtagsEnabled
        {
            get;
        }

        private bool shouldPrompt
        {
            get;
        }

        private bool isEnabled
        {
            get;
        }

        public abstract void setUpAnalytics();
        public abstract void setAllowAnalytics();
        public abstract void setDontAllowAnalytics();
    }
}