using My.Framework.Runtime.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace My.Framework.Runtime.UI
{
    /// <summary>
    /// UI ������ ����㼶
    /// </summary>
    public partial class UIManager
    {
        /// <summary>
        /// ���²㼶��Ϣ
        /// </summary>
        public void TickLayerStack()
        {
            UpdateLayerStack();
            if (m_isSortingOrderModifierDirty)
            {
                m_isSortingOrderModifierDirty = false;
                UpdateAllLayerSortingOrder();
            }
        }

        /// <summary>
        /// ��ʼ���㼶
        /// </summary>
        /// <returns></returns>
        public bool InitializeLayer()
        {
            // ����sceneRoot
            if (!CreateSceneRoot())
            {
                Debug.LogError("SceneLayerManager.Initlize CreateSceneRoot fail");
                return false;
            }
            return true;
        }

        #region ���ⷽ��

        /// <summary>
        /// ����һ����
        /// </summary>
        /// <param name="layerType"></param>
        /// <param name="name"></param>
        /// <param name="resPath"></param>
        /// <param name="onComplete"></param>
        public void CreateLayer(Type layerType, string name, string resPath, Action<UILayerBase> onComplete)
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
                if (oldLayer.IsReserve() && oldLayer.State == UILayerBase.LayerState.Unused)
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

            if (layerType == typeof(UILayerNormal))
            {
                CreateSceneLayerUI(name, resPath, onComplete);
            }
            else if (layerType == typeof(UILayer3D))
            {
                CreateSceneLayer3D(name, resPath, onComplete);
            }
            else if (layerType == typeof(UILayerUnity))
            {
                CreateSceneLayerUnity(name, resPath, onComplete);
            }
        }

        /// <summary>
        /// �ͷŲ�
        /// </summary>
        /// <param name="layer"></param>
        public void FreeLayer(UILayerBase layer)
        {
            if (layer == null)
            {
                return;
            }
            // ��layer����loading�����У���Ҫ��loading���
            if (layer.State == UILayerBase.LayerState.Loading)
            {
                layer.State = UILayerBase.LayerState.WaitForFree;
                return;
            }

            // ��layer��ջ�У���Ҫ�ȵ�ջ
            if (layer.State == UILayerBase.LayerState.InStack)
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

            // ����layer����
            if (layer.GetType() != typeof(UILayerUnity))
            {
                GameObject.Destroy(layer.gameObject);
            }
            else
            {
                // scene layer ������
                var sceneLayer = layer as UILayerUnity;
                System.Diagnostics.Debug.Assert(sceneLayer != null, "sceneLayer != null");
                GameObject.Destroy(sceneLayer.gameObject);
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneLayer.Scene);
            }
        }

        /// <summary>
        /// ��layerѹջ
        /// </summary>
        /// <param name="layer"></param>
        public bool PushLayer(UILayerBase layer)
        {
            // ����ֻ��Unused״̬����ѹջ
            if (layer.State != UILayerBase.LayerState.Unused)
            {
                Debug.LogError(string.Format("PushLayer in wrong state layer={0} state={1}", layer.name, layer.State));
                return false;
            }

            // ��layer�ƶ���ջ���ݽṹ
            m_unusedLayerList.Remove(layer);
            m_layerStack.Add(layer);


            // ����״̬
            layer.State = UILayerBase.LayerState.InStack;

            // ����Ĭ�ϵ�go root, �������Ҫ UpdateLayerStack ������ϸ����
            if (layer.GetType() == typeof(UILayer3D))
            {
                layer.transform.SetParent(m_3DSceneRootGo.transform, false);
            }
            else if (layer.GetType() == typeof(UILayerUnity))
            {
                // do nothing
            }

            if (layer.GetType() != typeof(UILayerUnity))
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
        public void PopLayer(UILayerBase layer)
        {
            // ֻ��InStack״̬֮�²���PopLayer
            if (layer.State != UILayerBase.LayerState.InStack)
            {
                Debug.LogError(string.Format("PopLayer in wrong state layer={0} state={1}", layer.name, layer.State));
                return;
            }

            // ����layer
            if (layer.GetType() != typeof(UILayerUnity))
            {
                layer.gameObject.SetActive(false);
            }

            // ��layer�ƶ���ջ���ݽṹ
            m_layerStack.Remove(layer);
            m_unusedLayerList.Add(layer);

            if (layer.GetType() != typeof(UILayerUnity))
            {
                layer.gameObject.transform.SetParent(m_unusedLayerRootGo.transform, false);
            }

            // ����״̬
            layer.State = UILayerBase.LayerState.Unused;

            m_stackDirty = true;
        }

        /// <summary>
        /// �ҵ�ָ����layer
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public UILayerBase FindLayerByName(string name)
        {
            m_layerDict.TryGetValue(name, out var ret);
            return ret;
        }

        /// <summary>
        /// ͨ��gameobject ��ȡ������layer����
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public UILayerBase GetSceneLayer(GameObject go)
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

        #endregion

        #region �������ɽṹ

        /// <summary>
        /// ����ui�����ĸ��ڵ�ṹ
        /// </summary>
        /// <returns></returns>
        private bool CreateSceneRoot()
        {
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

            layerGo.AddComponent<UILayerNormal>();

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

            layerGo.AddComponent<UILayer3D>();
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
            camera.clearFlags = CameraClearFlags.Nothing;
            camera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.depth = 50;

            //var additionalData = camera.GetComponent<UniversalAdditionalCameraData>();
            //additionalData.SetBackgroundOverrideParams
            return cameraGo;
        }

        #endregion

        #region ���ɲ㺯��

        /// <summary>
        /// ����һ��uilayer
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="resPath"></param>
        /// <param name="onComplete"></param>
        /// <param name="enableReserve"></param>
        private void CreateSceneLayerUI(string layerName, string resPath, Action<UILayerBase> onComplete)
        {
            // ��prefab��¡layerroot
            var layerRootGo = CreateUILayerRoot();

            // ����GO������
            layerRootGo.name = layerName + "_LayerRoot";

            // ��ȡlayer���
            var layer = layerRootGo.GetComponent<UILayerNormal>();
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
                if (layer.State == UILayerBase.LayerState.WaitForFree)
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
        private void CreateSceneLayer3D(string layerName, string resPath, Action<UILayerBase> onComplete, bool enableReserve = false)
        {
            // ��prefab��¡layerroot
            var layerRootGo = Create3DLayerRoot();
            if (layerRootGo == null)
            {
                Debug.LogError(string.Format("CreateUILayer fail to CreateSceneLayer3D name={0}", layerName));
                return;
            }

            // ��ȡlayer���
            var layer = layerRootGo.GetComponent<UILayer3D>();
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
                if (layer.State == UILayerBase.LayerState.WaitForFree)
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
        private void CreateSceneLayerUnity(string name, string resPath, Action<UILayerBase> onComplete)
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
                var layer = layerObj.AddComponent<UILayerUnity>();
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
                layer.State = UILayerBase.LayerState.Unused;
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

        #endregion

        #region �ڲ�����

        /// <summary>
        /// ��layer����Դ�������
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="asset"></param>
        private void OnLayerLoadAssetComplete(UILayerBase layer, GameObject asset)
        {
            if (layer.GetType() == typeof(UILayerUnity))
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

        #region �ƶ���transform

        /// <summary>
        /// ��layer�ҵ������б�
        /// </summary>
        /// <param name="layer"></param>
        private void AddLayerToLoading(UILayerBase layer)
        {
            if (layer.GetType() == typeof(UILayerUnity))
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
            layer.State = UILayerBase.LayerState.Loading;
        }

        /// <summary>
        /// ��layer�ҵ�δʹ���б�
        /// </summary>
        /// <param name="layer"></param>
        private void AddLayerToUnused(UILayerBase layer)
        {
            if (layer.GetType() == typeof(UILayerUnity))
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
            layer.State = UILayerBase.LayerState.Unused;
        }


        #endregion

        /// <summary>
        /// ��Layer��������
        /// </summary>
        /// <param name="layerStack"></param>
        private void SortLayerStack(List<UILayerBase> layerStack)
        {
            // �ȶ��Ĳ�������
            int i, j;
            // ��ǰ����Ԫ�ص���ʱ�洢
            UILayerBase target;
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
        /// ���¸���sorting order ����Ҫʱui�㣩
        /// </summary>
        private void UpdateAllLayerSortingOrder()
        {
            int count = m_layerStack.Count;
            for (int i = 0; i < count; ++i)
            {
                UILayerNormal uiSceneLayer = m_layerStack[i] as UILayerNormal;
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

            UILayerUnity topSceneLayer = null;

            var cachedCameras = new List<Camera>();
            cachedCameras.AddRange(m_uiCameraList);
            m_uiCameraList.Clear();

            int canvasSortOrder = 0;
            bool isCameraGroupBlocked = false;
            for (int i = m_layerStack.Count - 1; i >= 0; i--)
            {
                var layer = m_layerStack[i];

                if (layer.GetType() == typeof(UILayerNormal))
                {
                    UILayerNormal uiSceneLayer = layer as UILayerNormal;
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
                else if (layer.GetType() == typeof(UILayer3D))
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
                else if (layer.GetType() == typeof(UILayerUnity))
                {
                    topSceneLayer = topSceneLayer == null ? (UILayerUnity)layer : topSceneLayer;
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
                    if (t.GetType() == typeof(UILayerUnity))
                    {
                        topSceneLayer = (UILayerUnity)t;
                        break;
                    }
                }
            }

            // ������Ⱦ
            if (topSceneLayer != null)
            {
                //ApplyRenderSettingAsDefault(currRenderSetting);
            }

            AfterUpdateLayerStack();
        }

        protected virtual void AfterUpdateLayerStack()
        {

        }

        #endregion

        #region ���߷���

        /// <summary>
        /// ��ȡui�����Ӧcamera
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public Camera GetUICamera(GameObject go)
        {
            Canvas canvas = GetCanvas(go);
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                return canvas.worldCamera;
            }
            return null;
        }

        public Canvas GetCanvas(GameObject go)
        {
            Canvas canvas = null;
            UILayerNormal uiSceneLayer = GetSceneLayer(go) as UILayerNormal;
            if (uiSceneLayer != null)
            {
                canvas = uiSceneLayer.LayerCanvas;
            }
            return canvas;
        }

        #endregion

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
        private readonly List<UILayerBase> m_loadingLayerList = new List<UILayerBase>();

        /// <summary>
        /// δʹ�õ�layer�б�
        /// </summary>
        private readonly List<UILayerBase> m_unusedLayerList = new List<UILayerBase>();

        /// <summary>
        /// layer��ջ��������ʾ�㼶��ϵ,����β����ջ��
        /// </summary>
        private readonly List<UILayerBase> m_layerStack = new List<UILayerBase>();
        /// <summary>
        /// layer�������ֵ�
        /// </summary>
        private Dictionary<string, UILayerBase> m_layerDict = new Dictionary<string, UILayerBase>();

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