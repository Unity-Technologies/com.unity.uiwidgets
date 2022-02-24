using UnityEngine;
using UnityEngine.UI;

namespace UIWidgetsSample
{
    public class TestFei : MonoBehaviour
    {
        private float curTime = 0.0f;

        private int totalNum = 10;

        private bool isOk = false;

        public static bool needwaiting = false;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                this.isOk = true;
            }

            if (!this.isOk)
            {
                return;
            }

            //if (needwaiting)
            //{
            //    return;
            //}
            
            if (this.totalNum <= 0)
            {
                return;
            }
            
            this.curTime += Time.deltaTime;
            if (this.curTime < 0)
            {
                return;
            }

            this.curTime = 0;
            this.totalNum--;

            //needwaiting = true;
            //for (int i = 0; i < 10; i++)
            {
                GameObject gameobject = new GameObject();
                GameObject newCanvas = new GameObject();

                Canvas c = newCanvas.AddComponent<Canvas>();
                c.renderMode = RenderMode.WorldSpace;

                newCanvas.AddComponent<CanvasScaler>();
                newCanvas.AddComponent<GraphicRaycaster>();

                var canvasTransform = c.GetComponent<RectTransform>();
                canvasTransform.anchorMin = new Vector2(0, 0);
                canvasTransform.anchorMax = new Vector2(0, 0);
                canvasTransform.sizeDelta = new Vector2(800, 800);

                GameObject panel = new GameObject("RawImage");
                var rectTransform = panel.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.offsetMin = new Vector2(0, 0);
                rectTransform.offsetMax = new Vector2(0, 0);

                panel.AddComponent<CanvasRenderer>();
                panel.AddComponent<HoverSample>();
                panel.transform.SetParent(newCanvas.transform, false);
                newCanvas.transform.SetParent(gameobject.transform, true);
            }
        }
    }
}