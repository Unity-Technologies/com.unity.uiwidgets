using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIWidgetsSample
{
    public class PickUp : MonoBehaviour
    {
        public bool isHolding = false;

        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            isHolding = true;
            Debug.Log("Mouse Down Detected !!!");
        }

        private void OnMouseUp()
        {
            isHolding = false;
            
        }
    }
}