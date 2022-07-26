using System;
using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace UIWidgetsSample
{
    public class ModelViewState
    {
        public RenderTexture _renderTexture;
        public GameObject _modelViewPrefab;
    }
    
    public class ModelViewHelper : MonoBehaviour
    {
        static ModelViewHelper _instance;

        public static ModelViewHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    //ensureModelViewHelperIfNeeded();
                }

                return _instance;
            }
        }
        
        /*internal static void ensureModelViewHelperIfNeeded() {
            if (_instance != null) {
                return;
            }
            var managerObj = new GameObject("__ModelViewHelper");
            var component = managerObj.AddComponent<ModelViewHelper>();
            _instance = component;
        }*/
        
        //private static bool applicationIsQuitting = false;

        public void Awake()
        {
            if (_instance != null)
            {
                Debug.Log("error 11");
            }
            _instance = this;
        }
        
        public void OnDestroy()
        {
            _instance = null;
        }

        //private void Awake()
        //{
        //    DontDestroyOnLoad(gameObject);
        //}

        public Dictionary<int, ModelViewState> modelStates = new Dictionary<int, ModelViewState>();

        
        public void UnloadAll()
        {
            
        }

        public bool Load(int id, string path)
        {
            if (modelStates.ContainsKey(id))
            {
                return true;
            }

            var modelViewPrefab = Resources.Load(path);
            if (modelViewPrefab == null)
            {
                Debug.Log("Load failed !");
            }
            
            var gameObj = (GameObject) Instantiate(modelViewPrefab, Vector3.zero, Quaternion.identity);
            
            var renderTexture = UIWidgetsExternalTextureHelper.createCompatibleExternalTexture(new RenderTextureDescriptor(
                width: 100, height: 100
                )
            {
                depthBufferBits = 32
            });

            gameObj.transform.Find("Camera").GetComponent<Camera>().targetTexture = renderTexture;
            
            this.modelStates.Add(id, new ModelViewState
            {
                _renderTexture = renderTexture,
                _modelViewPrefab = gameObj
            });

            return true;
        }

        public RenderTexture LoadModelViewRT(int id)
        {
            return this.modelStates[id]._renderTexture;
        }
    }
} 