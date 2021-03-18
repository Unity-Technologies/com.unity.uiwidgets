using engine2;
using UIWidgetsSample;
using Unity.UIWidgets.engine2;
using UnityEngine;

namespace Unity.UIWidgets.ui
{
    
    public class AndroidWorkAround : MonoBehaviour
    {
        UIWidgetsPanel panel;

        // Start is called before the first frame update
        void Start()
        {
            panel = GetComponent<UIWidgetsPanel>();
            panel.enabled = false;
            AndroidGLInit.Init();
        }

        // Update is called once per frame
        void Update()
        {
            if (!panel.enabled)
            {
                panel.enabled = true;
                enabled = false;
            }
        }
    }

}