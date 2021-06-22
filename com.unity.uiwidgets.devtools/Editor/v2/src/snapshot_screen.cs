using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    public class SnapshotArguments {
        public SnapshotArguments(string screenId)
        {
            this.screenId = screenId;
        }
        
        public readonly string screenId;
    }
    
    public class SnapshotScreenBody : StatefulWidget {
        public SnapshotScreenBody(SnapshotArguments args, List<Screen> possibleScreens)
        {
            this.args = args;
            this.possibleScreens = possibleScreens;
        }

        public readonly SnapshotArguments args;

        public readonly List<Screen> possibleScreens;
            
        public override State createState()
        {
            return new _SnapshotScreenBodyState();
        }
    }

    public class _SnapshotScreenBodyState : State<SnapshotScreenBody>
    {
        Screen _screen;
        public override Widget build(BuildContext context)
        {
            return new Column(
                children: new List<Widget>()
                {
                    new Expanded(
                        child: _screen != null ? _screen.build(context) : _buildSnapshotError()
                    )
                } 
            );
        }
        
        Widget _buildSnapshotError()
        {
            return new Container(child: new Text($"Cannot load snapshot for screen \'{widget.args?.screenId}\'"));
        }
    }

}