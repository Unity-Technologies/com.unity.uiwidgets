namespace Unity.UIWidgets.DevTools
{
    public class SnapshotArguments {
        public SnapshotArguments(string screenId)
        {
            this.screenId = screenId;
        }
        
        public readonly string screenId;
    }
}