using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.Editor;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace UIWidgetsEditorWindowSample
{
    public class UnityObjectDraggingWindow : UIWidgetsEditorPanel
    {
        [MenuItem("UIWidgets/EditorSample/UnityObjectDragging")]
        public static void CountDemo()
        {
            CreateWindow<UnityObjectDraggingWindow>();
        }

        protected override void onEnable()
        {
            AddFont("Material Icons", new List<string> {"MaterialIcons-Regular.ttf"}, new List<int> {0});
            AddFont("CupertinoIcons", new List<string> {"CupertinoIcons.ttf"}, new List<int> {0});
            AddFont("GalleryIcons", new List<string> {"gallery/GalleryIcons.ttf"}, new List<int> {0});
        }

        protected override void main()
        {
            editor_ui_.runEditorApp(new MyApp());
        }
        
        public class MyApp : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return 
                    new Container(color: Colors.green, child:
                    new Center(child: 
                        new UnityObjectDetector(
                            child: new Container(color: Colors.blue, width: 200f, height: 200f, 
                                child: new Center(child: new Text("Drag UnityObject or file to me"))),
                                onEnter: (details) =>
                                {
                                    Debug.Log("enter");
                                },
                                onHover: (details =>
                                {
                                    Debug.Log("position = " + details.position);
                                }),
                                onExit: () =>
                                {
                                    Debug.Log("on exit");
                                },
                                onRelease: (details) =>
                                {
                                    Debug.Log("on release");
                                }
                        )
                        )
                    );
            }
    }
    }
}