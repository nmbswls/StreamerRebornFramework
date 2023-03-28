using My.Framework.Runtime.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace My.Framework.Runtime.Scene
{
    public class SceneLayerManager
    {

        #region �������

        private SceneLayerManager() { }


        public static SceneLayerManager CreateSceneManager()
        {
            if (m_instance == null)
            {
                m_instance = new SceneLayerManager();
            }
            return m_instance;
        }

        /// <summary>
        /// ����������
        /// </summary>
        public static SceneLayerManager Instance { get { return m_instance; } }
        private static SceneLayerManager m_instance;

        /// <summary>
        /// corutine����
        /// </summary>
        private SimpleCoroutineWrapper m_corutineHelper = new SimpleCoroutineWrapper();
        #endregion

        public void Tick()
        {
            m_corutineHelper.Tick();
            UpdateLayerStack();
            if (m_isSortingOrderModifierDirty)
            {
                m_isSortingOrderModifierDirty = false;
                UpdateAllLayerSortingOrder();
            }
        }

        public bool Initialize()
        {
            // ����sceneRoot
            if (!CreateSceneRoot())
            {
                Debug.LogError("SceneLayerManager.Initlize CreateSceneRoot fail");
                return false;
            }
            return true;
        }

        private bool CreateSceneRoot()
        {
            Debug.Log("SceneManager.CreateSceneRoot start");
            var sceneRootPrefab = Resources.Load<GameObject>("SceneRoot");
            m_sceneRootGo = GameObject.Instantiate(sceneRootPrefab);
            if (m_sceneRootGo == null)
            {
                m_sceneRootGo = new GameObject("SceneRoot");
                m_sceneRootGo.transform.localPosition = Vector3.zero;
                m_sceneRootGo.transform.localRotation = Quaternion.identity;
                m_sceneRootGo.transform.localScale = Vector3.one;

                {
                    GameObject goUISceneRoot = new GameObject("UISceneRoot");
                    goUISceneRoot.transform.SetParent(m_sceneRootGo.transform);

                    GameObject go3DSceneRoot = new GameObject("3DSceneRoot");
                    go3DSceneRoot.transform.SetParent(m_sceneRootGo.transform);

                    GameObject goUnusedLayerRoot = new GameObject("UnusedLayerRoot");
                    goUnusedLayerRoot.transform.SetParent(m_sceneRootGo.transform);

                    GameObject goLoadingLayerRoot = new GameObject("LoadingLayerRoot");
                    goLoadingLayerRoot.transform.SetParent(m_sceneRootGo.transform);

                    GameObject goEventSystem = new GameObject("EventSystem");
                    goEventSystem.AddComponent<EventSystem>();
                    goEventSystem.AddComponent<StandaloneInputModule>();
                    goUISceneRoot.transform.SetParent(m_sceneRootGo.transform);
                }
            }

            // ����������õĽڵ�
            m_3DSceneRootGo = m_sceneRootGo.transform.Find("3DSceneRoot").gameObject;
            m_unusedLayerRootGo = m_sceneRootGo.transform.Find("UnusedLayerRoot").gameObject;
            m_loadingLayerRootGo = m_sceneRootGo.transform.Find("LoadingLayerRoot").gameObject;
            m_uiSceneRootGo = m_sceneRootGo.transform.Find("UISceneRoot").gameObject;

            m_uiCameraTemplate = m_sceneRootGo.transform.Find("UICameraTemplate").gameObject;

            return true;
        }

        /// <summary>
        /// ������ui layer
        /// </summary>
        /// <returns></returns>
        private GameObject CreateUILayerRoot()
        {
            GameObject layerGo = new GameObject();
            layerGo.layer = LayerMask.NameToLayer("UI");
            layerGo.name = "UILayerRoot";
            var canvas = layerGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var scaler = layerGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            layerGo.AddComponent<GraphicRaycaster>();
            layerGo.AddComponent<CanvasGroup>();

            layerGo.AddComponent<SceneLayerUI>();

            return layerGo;
        }

        /// <summary>
        /// ������ui layer
        /// </summary>
        /// <returns></returns>
        private GameObject Create3DLayerRoot()
        {
            GameObject layerGo = new GameObject();
            layerGo.name = "3DLayerRoot";

            layerGo.AddComponent<SceneLayer3D>();
            return layerGo;
        }

        private GameObject CreateDefaultCamera()
        {
            GameObject cameraGo;
            if (m_uiCameraTemplate != null)
            {
                cameraGo = GameObject.Instantiate<GameObject>(m_uiCameraTemplate);
                cameraGo.SetActive(true);
                cameraGo.name = "UICamera";
                return cameraGo;
            }

            cameraGo = new GameObject();
            cameraGo.name = "UICamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Depth;
            camera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.depth = 50;

            return cameraGo;
        }

        /// <summary>
        /// ����һ����
        /// </summary>
        /// <param name="layerType"></param>
        /// <param name="name"></param>
        /// <param name="resPath"></param>
        /// <param name="onComplete"></param>
        public void CreateLayer(Type layerType, string name, string resPath, Action<SceneLayerBase> onComplete)
        {
            // ����Ҫ������
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("CreateLayer need a name");
            }

            // ����Ǳ���layer��ֱ�ӷ��ػ���
            var oldLayer = FindLayerByName(name);
            if (oldLayer != null)
            {
                if (oldLayer.IsReserve() && oldLayer.State == SceneLayerBase.LayerState.Unused)
                {
                    onComplete(oldLayer);
                }
                else
                {
                    Debug.LogError(
                        $"CreateLayer oldLayer state error. LayerName:{name},LayerType:{layerType},respath:{resPath},IsReserve:{oldLayer.IsReserve()},LayerState:{oldLayer.State}");
                    onComplete(null);
                }
                return;
            }

            // ���ֲ��ܳ�ͻ
            if (m_layerDict.ContainsKey(name))
            {
                throw new Exception(string.Format("CreateLayer name conflict {0}", name));
            }

            if (layerType == typeof(SceneLayerUI))
            {
                CreateSceneLayerUI(name, resPath, onComplete);
            }
            else if (layerType == typeof(SceneLayer3D))
            {
                CreateSceneLayer3D(name, resPath, onComplete);
            }
            else if (layerType == typeof(SceneLayerUnity))
            {
                CreateSceneLayerUnity(name, resPath, onComplete);
            }
        }

        /// <summary>
        /// ����һ��uilayer
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="resPath"></param>
        /// <param name="onComplete"></param>
        /// <param name="enableReserve"></param>
        private void CreateSceneLayerUI(string layerName, string resPath, Action<SceneLayerBase> onComplete)
        {
            // ��prefab��¡layerroot
            var layerRootGo = CreateUILayerRoot();

            // ����GO������
            layerRootGo.name = layerName + "_LayerRoot";

            // ��ȡlayer���
            var layer = layerRootGo.GetComponent<SceneLayerUI>();
            layer.Init();
            //layer.LayerCanvas.worldCamera = m_defaultUiCamera;
            layer.SetName(layerName);

            // ��layer���뵽�ֵ�
            m_layerDict.Add(layerName, layer);

            // ������Դ����
            if (resPath == null)
            {
                // ��layer�ҵ�δʹ���б�
                AddLayerToUnused(layer);
                onComplete(layer);
                return;
            }
            // ��layer���뵽�������б�
            AddLayerToLoading(layer);
            //todo ·��

            var iter = SimpleResourceManager.Instance.LoadAsset<GameObject>(resPath, (lpath, lasset) =>
            {
                // ��layer�Ѿ��ͷ�
                if (layer.State == SceneLayerBase.LayerState.WaitForFree)
                {
                    FreeLayer(layer);
                    return;
                }
                // ��Դ�������
                OnLayerLoadAssetComplete(layer, lasset);
                onComplete(lasset == null ? null : layer);
            });
            m_corutineHelper.StartCoroutine(iter);
        }

        /// <summary>
        /// ����һ��3dlayer
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="resPath"></param>
        /// <param name="onComplete"></param>
        /// <param name="enableReserve">�Ƿ���������</param>
        private void CreateSceneLayer3D(string layerName, string resPath, Action<SceneLayerBase> onComplete, bool enableReserve = false)
        {
            // ��prefab��¡layerroot
            var layerRootGo = Create3DLayerRoot();
            if (layerRootGo == null)
            {
                Debug.LogError(string.Format("CreateUILayer fail to clone m_3DLayerRootPrefab name={0}", layerName));
                return;
            }

            // ��ȡlayer���
            var layer = layerRootGo.GetComponent<SceneLayer3D>();
            layer.Init();
            layer.SetName(layerName);

            // ��layer���뵽�ֵ�
            m_layerDict.Add(layerName, layer);

            // ��layer���뵽�������б�
            AddLayerToLoading(layer);
            string resFullPath = $"Assets/RuntimeAssets/Main/{resPath}.prefab";
            var iter = SimpleResourceManager.Instance.LoadAsset<GameObject>(resFullPath, (lpath, lasset) =>
            {
                // ��layer�Ѿ��ͷ�
                if (layer.State == SceneLayerBase.LayerState.WaitForFree)
                {
                    FreeLayer(layer);
                    return;
                }
                // ��Դ�������
                OnLayerLoadAssetComplete(layer, lasset);
                onComplete(lasset == null ? null : layer);
            });
            m_corutineHelper.StartCoroutine(iter);
        }

        /// <summary>
        /// ����һ��������
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resPath"></param>
        /// <param name="onComplete"></param>
        private void CreateSceneLayerUnity(string name, string resPath, Action<SceneLayerBase> onComplete)
        {
            // ������Դ����
            UnityEngine.SceneManagement.Scene? scene;
            var iter = SimpleResourceManager.Instance.LoadUnityScene(resPath, (lpath, lscene) =>
            {
                // ����ʧ��
                if (lscene == null)
                {
                    onComplete(null);
                    return;
                }

                scene = lscene;

                var rootGoList = scene.Value.GetRootGameObjects();
                if (rootGoList == null || rootGoList.Length == 0)
                {
                    onComplete(null);
                    return;
                }

                // ��ȡlayer���
                GameObject layerObj = new GameObject(name);
                GameObject.DontDestroyOnLoad(layerObj);
                var layer = layerObj.AddComponent<SceneLayerUnity>();
                layer.Init();
                layer.SetName(name);
                layer.SetScene(scene.Value);

                foreach (var go in rootGoList)
                {
                    if (go.name == "Root")
                    {
                        layer.SetLayerPrefabRoot(go);
                        break;
                    }
                }

                // ��layer���뵽�ֵ�
                m_layerDict.Add(name, layer);

                // ��layer���뵽δʹ���б�
                layer.State = SceneLayerBase.LayerState.Unused;
                m_unusedLayerList.Add(layer);

                // ��Դ�������,�Զ�push��unityscenelayer�������в���ջ�е����
                PushLayer(layer);
                //Ĭ�ϰ����¼��صĳ�����Ϊcurrent scene������뻻������updateview��active
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene() != layer.Scene)
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(layer.Scene);
                onComplete(layer);
            });
            m_corutineHelper.StartCoroutine(iter);
        }

        /// <summary>
        /// �ͷŲ�
        /// </summary>
        /// <param name="layer"></param>
        public void FreeLayer(SceneLayerBase layer)
        {
            if (layer == null)
            {
                return;
            }
            // ��layer����loading�����У���Ҫ��loading���
            if (layer.State == SceneLayerBase.LayerState.Loading)
            {
                layer.State = SceneLayerBase.LayerState.WaitForFree;
                return;
            }

            // ��layer��ջ�У���Ҫ�ȵ�ջ
            if (layer.State == SceneLayerBase.LayerState.InStack)
            {
                PopLayer(layer);

                // ����layer������������
                if (layer.IsReserve())
                {
                    return;
                }
            }

            // ��layer�����ݽṹ�Ƴ�
            layer.UnInit();
            m_layerDict.Remove(layer.LayerName);
            m_unusedLayerList.Remove(layer);
            m_loadingLayerList.Remove(layer);

            GameObject.Destroy(layer.gameObject);
        }

        /// <summary>
        /// ��layerѹջ
        /// </summary>
        /// <param name="layer"></param>
        public bool PushLayer(SceneLayerBase layer)
        {
            // ����ֻ��Unused״̬����ѹջ
            if (layer.State != SceneLayerBase.LayerState.Unused)
            {
                Debug.LogError(string.Format("PushLayer in wrong state layer={0} state={1}", layer.name, layer.State));
                return false;
            }

            // ��layer�ƶ���ջ���ݽṹ
            m_unusedLayerList.Remove(layer);
            m_layerStack.Add(layer);


            // ����״̬
            layer.State = SceneLayerBase.LayerState.InStack;

            // ����Ĭ�ϵ�go root, �������Ҫ UpdateLayerStack ������ϸ����
            if (layer.GetType() == typeof(SceneLayer3D))
            {
                layer.transform.SetParent(m_3DSceneRootGo.transform, false);
            }
            else if (layer.GetType() == typeof(SceneLayerUnity))
            {
                // do nothing
            }

            if (layer.GetType() != typeof(SceneLayerUnity))
            {
                layer.gameObject.SetActive(true);
            }

            // �������ǣ����ᵼ��UpdateLayerStack
            m_stackDirty = true;

            return true;
        }

        /// <summary>
        /// ��layer��ջ
        /// </summary>
        /// <param name="layer"></param>
        public void PopLayer(SceneLayerBase layer)
        {
            // ֻ��InStack״̬֮�²���PopLayer
            if (layer.State != SceneLayerBase.LayerState.InStack)
            {
                Debug.LogError(string.Format("PopLayer in wrong state layer={0} state={1}", layer.name, layer.State));
                return;
            }

            // ����layer
            if (layer.GetType() != typeof(SceneLayerUnity))
            {
                layer.gameObject.SetActive(false);
            }

            // ��layer�ƶ���ջ���ݽṹ
            m_layerStack.Remove(layer);
            m_unusedLayerList.Add(layer);

            if (layer.GetType() != typeof(SceneLayerUnity))
            {
                layer.gameObject.transform.SetParent(m_unusedLayerRootGo.transform, false);
            }

            // ����״̬
            layer.State = SceneLayerBase.LayerState.Unused;

            m_stackDirty = true;
        }

        /// <summary>
        /// ��layer�ҵ������б�
        /// </summary>
        /// <param name="layer"></param>
        private void AddLayerToLoading(SceneLayerBase layer)
        {
            if (layer.GetType() == typeof(SceneLayerUnity))
            {
                return;
            }
            // ������ҵ�loadingLayerRoot֮��
            layer.gameObject.transform.SetParent(m_loadingLayerRootGo.transform, false);
            layer.gameObject.transform.localPosition = Vector3.zero;
            layer.gameObject.transform.localScale = Vector3.one;
            layer.gameObject.transform.localRotation = Quaternion.identity;
            // ������ʾ״̬Ϊ����ʾ
            layer.gameObject.SetActive(false);

            // ��layer���뵽�������б�
            m_loadingLayerList.Add(layer);

            // ����״̬
            layer.State = SceneLayerBase.LayerState.Loading;
        }

        /// <summary>
        /// ��layer�ҵ�δʹ���б�
        /// </summary>
        /// <param name="layer"></param>
        private void AddLayerToUnused(SceneLayerBase layer)
        {
            if (layer.GetType() == typeof(SceneLayerUnity))
            {
                return;
            }

            m_loadingLayerList.Remove(layer);
            m_layerStack.Remove(layer);

            // ������ҵ�loadingLayerRoot֮��
            layer.gameObject.transform.SetParent(m_unusedLayerRootGo.transform, false);
            layer.gameObject.transform.localPosition = Vector3.zero;
            layer.gameObject.transform.localScale = Vector3.one;
            layer.gameObject.transform.localRotation = Quaternion.identity;
            // ������ʾ״̬Ϊ����ʾ
            layer.gameObject.SetActive(false);

            // ��layer���뵽�������б�
            m_unusedLayerList.Add(layer);

            // ����״̬
            layer.State = SceneLayerBase.LayerState.Unused;
        }

        /// <summary>
        /// ��layer����Դ�������
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="asset"></param>
        private void OnLayerLoadAssetComplete(SceneLayerBase layer, GameObject asset)
        {
            if (layer.GetType() == typeof(SceneLayerUnity))
            {
                return;
            }

            // �������ʧ��
            if (asset == null)
            {
                Debug.LogError(string.Format("CreateUILayer LoadAsset fail name={0}", layer.LayerName));
                // �ͷ�layer
                FreeLayer(layer);
                return;
            }

            // ��¡prefab
            var layerPrefabGo = GameObject.Instantiate(asset);
            layerPrefabGo.name = layerPrefabGo.name.Replace("(Clone)", "");

            // ����¡��prefab��ӵ�layer
            layer.AttachGameObject(layerPrefabGo);

            // ��layer�ҵ�δʹ���б�
            AddLayerToUnused(layer);
        }

        public void UpdateAllLayerSortingOrder()
        {
            int count = m_layerStack.Count;
            for (int i = 0; i < count; ++i)
            {
                SceneLayerUI uiSceneLayer = m_layerStack[i] as SceneLayerUI;
                if (uiSceneLayer != null)
                {
                    uiSceneLayer.UpdateLayerSortingOrder();
                }
            }
        }

        /// <summary>
        /// ����ջ
        /// </summary>
        private void UpdateLayerStack()
        {
            if (!m_stackDirty)
            {
                return;
            }
            m_stackDirty = false;
            m_isSortingOrderModifierDirty = true;
            SortLayerStack(m_layerStack);

            // �������е�layer��1 ���� parent 2 ����offset 3 ���� camera��depth
            int cameraDepthMax = 80;
            int currLayerCameraDepth = cameraDepthMax;
            var next3DLayerOffset = Vector3.zero;

            SceneLayerUnity topSceneLayer = null;

            var cachedCameras = new List<Camera>();
            cachedCameras.AddRange(m_uiCameraList);
            m_uiCameraList.Clear();

            int canvasSortOrder = 0;
            bool isCameraGroupBlocked = false;
            for (int i = m_layerStack.Count - 1; i >= 0; i--)
            {
                var layer = m_layerStack[i];
                
                if (layer.GetType() == typeof(SceneLayerUI))
                {
                    SceneLayerUI uiSceneLayer = layer as SceneLayerUI;
                    Camera uiCamera;
                    if (isCameraGroupBlocked || m_uiCameraList.Count == 0)
                    {
                        isCameraGroupBlocked = false;
                        if (cachedCameras.Count == 0)
                        {
                            GameObject cameraGo = CreateDefaultCamera();
                            uiCamera = cameraGo.GetComponent<Camera>();
                        }
                        else
                        {
                            uiCamera = cachedCameras[0];
                            cachedCameras.RemoveAt(0);
                        }

                        uiCamera.transform.parent = m_uiSceneRootGo.transform;

                        uiCamera.enabled = true;
                        uiCamera.depth = currLayerCameraDepth;
                        //uiCamera.transform.localPosition = new Vector3(0f, 0f, (m_cameraDepthMax - currLayerCameraDepth) * -200f);
                        float cameraPosX = uiCamera.orthographicSize * 2f * Screen.width / Screen.height * (cameraDepthMax - currLayerCameraDepth) * 5f;
                        float cameraPosY = uiCamera.orthographicSize * 2f * (cameraDepthMax - currLayerCameraDepth) * 5f;
                        uiCamera.transform.localPosition = new Vector3(cameraPosX, cameraPosY, 0f);
                        m_uiCameraList.Add(uiCamera);
                        canvasSortOrder = cameraDepthMax;
                        currLayerCameraDepth--;
                    }
                    else
                    {
                        uiCamera = m_uiCameraList[m_uiCameraList.Count - 1];
                    }
                    Canvas canvas = uiSceneLayer.LayerCanvas;
                    canvas.worldCamera = uiCamera;
                    canvas.sortingOrder = canvasSortOrder * 100;
                    canvasSortOrder--;
                    layer.transform.SetParent(uiCamera.transform, false);

                    layer.transform.SetSiblingIndex(i);
                }
                else if (layer.GetType() == typeof(SceneLayer3D))
                {
                    layer.gameObject.transform.SetParent(m_3DSceneRootGo.transform, false);
                    // Ϊlayer����offset
                    layer.gameObject.transform.localPosition = next3DLayerOffset;
                    next3DLayerOffset = next3DLayerOffset + new Vector3(1000, 0, 0);
                    // ���������
                    if (layer.LayerCamera != null)
                    {
                        layer.LayerCamera.depth = currLayerCameraDepth;
                        currLayerCameraDepth--;
                        isCameraGroupBlocked = true;
                    }
                }
                else if(layer.GetType() == typeof(SceneLayerUnity))
                {
                    topSceneLayer = topSceneLayer == null ? (SceneLayerUnity)layer : topSceneLayer;
                    if (layer.LayerCamera != null)
                    {
                        layer.LayerCamera.depth = currLayerCameraDepth;
                        currLayerCameraDepth--;
                    }
                }
            }

            // û�õ���ֱ��������
            if (cachedCameras.Count > 0)
            {
                foreach (var camera in cachedCameras)
                {
                    GameObject.Destroy(camera.gameObject);
                }
            }

            if (topSceneLayer == null)
            {
                foreach (var t in m_unusedLayerList)
                {
                    if (t.GetType() == typeof(SceneLayerUnity))
                    {
                        topSceneLayer = (SceneLayerUnity)t;
                        break;
                    }
                }
            }

            // ������Ⱦ
            if(topSceneLayer != null)
            {
                //ApplyRenderSettingAsDefault(currRenderSetting);
            }

            AfterUpdateLayerStack();
        }

        protected virtual void AfterUpdateLayerStack()
        {

        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="layerStack"></param>
        private void SortLayerStack(List<SceneLayerBase> layerStack)
        {
            // �ȶ��Ĳ�������
            int i, j;
            // ��ǰ����Ԫ�ص���ʱ�洢
            SceneLayerBase target;
            for (i = 1; i < layerStack.Count; i++)
            {
                j = i;

                // ��ȡ��ǰ�����Ԫ��
                target = layerStack[i];

                // �����ǰ�ĵ�j��Ԫ�غ͵�j-1��Ԫ����������ıȽϱ�׼����ô��Ҫ����������Ԫ�ص�λ��
                // ����j��Ԫ�ز��뵽 ǰ��� �Ѿ��ź����Ԫ�� ��һ�����ʵ�λ�ã�ʹ�ò�����Ԫ��������Ȼ��������
                while (j > 0 && target.ComparePriority(layerStack[j - 1]) < 0)
                {
                    layerStack[j] = layerStack[j - 1];
                    j--;
                }

                layerStack[j] = target;
            }
        }

        /// <summary>
        /// �ҵ�ָ����layer
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SceneLayerBase FindLayerByName(string name)
        {
            m_layerDict.TryGetValue(name, out var ret);
            return ret;
        }

        public SceneLayerBase GetSceneLayer(GameObject go)
        {
            int count = m_layerStack.Count;
            for (int i = 0; i < count; ++i)
            {
                if (go.transform.IsChildOf(m_layerStack[i].transform))
                {
                    return m_layerStack[i];
                }

                if (go.transform == m_layerStack[i].transform)
                {
                    return m_layerStack[i];
                }
            }
            return null;
        }

        #region ����������
        /// <summary>
        /// ����sceneRoot��صĳ�������
        /// </summary>
        private GameObject m_sceneRootGo;
        private GameObject m_unusedLayerRootGo;
        private GameObject m_loadingLayerRootGo;
        private GameObject m_3DSceneRootGo;
        private GameObject m_uiSceneRootGo;

        private GameObject m_uiCameraTemplate;

        #endregion

        /// <summary>
        /// �����е�layer�б�
        /// </summary>
        private readonly List<SceneLayerBase> m_loadingLayerList = new List<SceneLayerBase>();

        /// <summary>
        /// δʹ�õ�layer�б�
        /// </summary>
        private readonly List<SceneLayerBase> m_unusedLayerList = new List<SceneLayerBase>();

        /// <summary>
        /// layer��ջ��������ʾ�㼶��ϵ,����β����ջ��
        /// </summary>
        private readonly List<SceneLayerBase> m_layerStack = new List<SceneLayerBase>();
        /// <summary>
        /// layer�������ֵ�
        /// </summary>
        private Dictionary<string, SceneLayerBase> m_layerDict = new Dictionary<string, SceneLayerBase>();

        // ������3d����ʱ ��Ҫ���ui���
        private List<Camera> m_uiCameraList = new List<Camera>();
        /// <summary>
        /// layer��ջ�Ƿ���Ҫ��������ͼ�����ʾ��ϵ
        /// </summary>
        private bool m_stackDirty;

        /// <summary>
        /// sorting order ����
        /// </summary>
        private bool m_isSortingOrderModifierDirty;

        /// <summary>
        /// �������ɵ�camera
        /// </summary>
        private List<Camera> m_extraCameraList = new List<Camera>();

        /// <summary>
        /// sceneroot ��Դ·�� �������ͷ����
        /// </summary>
        private static string m_sceneRootAssetPath = "SceneRoot";
    }
}