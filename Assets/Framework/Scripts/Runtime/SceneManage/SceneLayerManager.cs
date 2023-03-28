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

        #region 单例相关

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
        /// 单例访问器
        /// </summary>
        public static SceneLayerManager Instance { get { return m_instance; } }
        private static SceneLayerManager m_instance;

        /// <summary>
        /// corutine管理
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
            // 创建sceneRoot
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

            // 缓存各个有用的节点
            m_3DSceneRootGo = m_sceneRootGo.transform.Find("3DSceneRoot").gameObject;
            m_unusedLayerRootGo = m_sceneRootGo.transform.Find("UnusedLayerRoot").gameObject;
            m_loadingLayerRootGo = m_sceneRootGo.transform.Find("LoadingLayerRoot").gameObject;
            m_uiSceneRootGo = m_sceneRootGo.transform.Find("UISceneRoot").gameObject;

            m_uiCameraTemplate = m_sceneRootGo.transform.Find("UICameraTemplate").gameObject;

            return true;
        }

        /// <summary>
        /// 创建空ui layer
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
        /// 创建空ui layer
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
        /// 创建一个层
        /// </summary>
        /// <param name="layerType"></param>
        /// <param name="name"></param>
        /// <param name="resPath"></param>
        /// <param name="onComplete"></param>
        public void CreateLayer(Type layerType, string name, string resPath, Action<SceneLayerBase> onComplete)
        {
            // 必须要有名字
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("CreateLayer need a name");
            }

            // 如果是保留layer，直接返回缓存
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

            // 名字不能冲突
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
        /// 创建一个uilayer
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="resPath"></param>
        /// <param name="onComplete"></param>
        /// <param name="enableReserve"></param>
        private void CreateSceneLayerUI(string layerName, string resPath, Action<SceneLayerBase> onComplete)
        {
            // 从prefab克隆layerroot
            var layerRootGo = CreateUILayerRoot();

            // 设置GO的名称
            layerRootGo.name = layerName + "_LayerRoot";

            // 获取layer组件
            var layer = layerRootGo.GetComponent<SceneLayerUI>();
            layer.Init();
            //layer.LayerCanvas.worldCamera = m_defaultUiCamera;
            layer.SetName(layerName);

            // 将layer加入到字典
            m_layerDict.Add(layerName, layer);

            // 启动资源加载
            if (resPath == null)
            {
                // 将layer挂到未使用列表
                AddLayerToUnused(layer);
                onComplete(layer);
                return;
            }
            // 将layer加入到加载中列表
            AddLayerToLoading(layer);
            //todo 路径

            var iter = SimpleResourceManager.Instance.LoadAsset<GameObject>(resPath, (lpath, lasset) =>
            {
                // 当layer已经释放
                if (layer.State == SceneLayerBase.LayerState.WaitForFree)
                {
                    FreeLayer(layer);
                    return;
                }
                // 资源加载完成
                OnLayerLoadAssetComplete(layer, lasset);
                onComplete(lasset == null ? null : layer);
            });
            m_corutineHelper.StartCoroutine(iter);
        }

        /// <summary>
        /// 创建一个3dlayer
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="resPath"></param>
        /// <param name="onComplete"></param>
        /// <param name="enableReserve">是否允许被留用</param>
        private void CreateSceneLayer3D(string layerName, string resPath, Action<SceneLayerBase> onComplete, bool enableReserve = false)
        {
            // 从prefab克隆layerroot
            var layerRootGo = Create3DLayerRoot();
            if (layerRootGo == null)
            {
                Debug.LogError(string.Format("CreateUILayer fail to clone m_3DLayerRootPrefab name={0}", layerName));
                return;
            }

            // 获取layer组件
            var layer = layerRootGo.GetComponent<SceneLayer3D>();
            layer.Init();
            layer.SetName(layerName);

            // 将layer加入到字典
            m_layerDict.Add(layerName, layer);

            // 将layer加入到加载中列表
            AddLayerToLoading(layer);
            string resFullPath = $"Assets/RuntimeAssets/Main/{resPath}.prefab";
            var iter = SimpleResourceManager.Instance.LoadAsset<GameObject>(resFullPath, (lpath, lasset) =>
            {
                // 当layer已经释放
                if (layer.State == SceneLayerBase.LayerState.WaitForFree)
                {
                    FreeLayer(layer);
                    return;
                }
                // 资源加载完成
                OnLayerLoadAssetComplete(layer, lasset);
                onComplete(lasset == null ? null : layer);
            });
            m_corutineHelper.StartCoroutine(iter);
        }

        /// <summary>
        /// 创建一个场景层
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resPath"></param>
        /// <param name="onComplete"></param>
        private void CreateSceneLayerUnity(string name, string resPath, Action<SceneLayerBase> onComplete)
        {
            // 启动资源加载
            UnityEngine.SceneManagement.Scene? scene;
            var iter = SimpleResourceManager.Instance.LoadUnityScene(resPath, (lpath, lscene) =>
            {
                // 加载失败
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

                // 获取layer组件
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

                // 将layer加入到字典
                m_layerDict.Add(name, layer);

                // 将layer加入到未使用列表
                layer.State = SceneLayerBase.LayerState.Unused;
                m_unusedLayerList.Add(layer);

                // 资源加载完成,自动push，unityscenelayer，不会有不在栈中的情况
                PushLayer(layer);
                //默认把最新加载的场景作为current scene，如果想换可以在updateview里active
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene() != layer.Scene)
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(layer.Scene);
                onComplete(layer);
            });
            m_corutineHelper.StartCoroutine(iter);
        }

        /// <summary>
        /// 释放层
        /// </summary>
        /// <param name="layer"></param>
        public void FreeLayer(SceneLayerBase layer)
        {
            if (layer == null)
            {
                return;
            }
            // 当layer处于loading过程中，需要等loading完成
            if (layer.State == SceneLayerBase.LayerState.Loading)
            {
                layer.State = SceneLayerBase.LayerState.WaitForFree;
                return;
            }

            // 当layer在栈中，需要先弹栈
            if (layer.State == SceneLayerBase.LayerState.InStack)
            {
                PopLayer(layer);

                // 保留layer不用真正销毁
                if (layer.IsReserve())
                {
                    return;
                }
            }

            // 将layer从数据结构移除
            layer.UnInit();
            m_layerDict.Remove(layer.LayerName);
            m_unusedLayerList.Remove(layer);
            m_loadingLayerList.Remove(layer);

            GameObject.Destroy(layer.gameObject);
        }

        /// <summary>
        /// 将layer压栈
        /// </summary>
        /// <param name="layer"></param>
        public bool PushLayer(SceneLayerBase layer)
        {
            // 首先只有Unused状态才能压栈
            if (layer.State != SceneLayerBase.LayerState.Unused)
            {
                Debug.LogError(string.Format("PushLayer in wrong state layer={0} state={1}", layer.name, layer.State));
                return false;
            }

            // 将layer移动到栈数据结构
            m_unusedLayerList.Remove(layer);
            m_layerStack.Add(layer);


            // 设置状态
            layer.State = SceneLayerBase.LayerState.InStack;

            // 设置默认的go root, 随后还是需要 UpdateLayerStack 进行详细计算
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

            // 设置脏标记，将会导致UpdateLayerStack
            m_stackDirty = true;

            return true;
        }

        /// <summary>
        /// 将layer弹栈
        /// </summary>
        /// <param name="layer"></param>
        public void PopLayer(SceneLayerBase layer)
        {
            // 只有InStack状态之下才能PopLayer
            if (layer.State != SceneLayerBase.LayerState.InStack)
            {
                Debug.LogError(string.Format("PopLayer in wrong state layer={0} state={1}", layer.name, layer.State));
                return;
            }

            // 隐藏layer
            if (layer.GetType() != typeof(SceneLayerUnity))
            {
                layer.gameObject.SetActive(false);
            }

            // 将layer移动到栈数据结构
            m_layerStack.Remove(layer);
            m_unusedLayerList.Add(layer);

            if (layer.GetType() != typeof(SceneLayerUnity))
            {
                layer.gameObject.transform.SetParent(m_unusedLayerRootGo.transform, false);
            }

            // 设置状态
            layer.State = SceneLayerBase.LayerState.Unused;

            m_stackDirty = true;
        }

        /// <summary>
        /// 将layer挂到加载列表
        /// </summary>
        /// <param name="layer"></param>
        private void AddLayerToLoading(SceneLayerBase layer)
        {
            if (layer.GetType() == typeof(SceneLayerUnity))
            {
                return;
            }
            // 将对象挂到loadingLayerRoot之下
            layer.gameObject.transform.SetParent(m_loadingLayerRootGo.transform, false);
            layer.gameObject.transform.localPosition = Vector3.zero;
            layer.gameObject.transform.localScale = Vector3.one;
            layer.gameObject.transform.localRotation = Quaternion.identity;
            // 设置显示状态为不显示
            layer.gameObject.SetActive(false);

            // 将layer加入到加载中列表
            m_loadingLayerList.Add(layer);

            // 设置状态
            layer.State = SceneLayerBase.LayerState.Loading;
        }

        /// <summary>
        /// 将layer挂到未使用列表
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

            // 将对象挂到loadingLayerRoot之下
            layer.gameObject.transform.SetParent(m_unusedLayerRootGo.transform, false);
            layer.gameObject.transform.localPosition = Vector3.zero;
            layer.gameObject.transform.localScale = Vector3.one;
            layer.gameObject.transform.localRotation = Quaternion.identity;
            // 设置显示状态为不显示
            layer.gameObject.SetActive(false);

            // 将layer加入到加载中列表
            m_unusedLayerList.Add(layer);

            // 设置状态
            layer.State = SceneLayerBase.LayerState.Unused;
        }

        /// <summary>
        /// 当layer的资源加载完成
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="asset"></param>
        private void OnLayerLoadAssetComplete(SceneLayerBase layer, GameObject asset)
        {
            if (layer.GetType() == typeof(SceneLayerUnity))
            {
                return;
            }

            // 如果加载失败
            if (asset == null)
            {
                Debug.LogError(string.Format("CreateUILayer LoadAsset fail name={0}", layer.LayerName));
                // 释放layer
                FreeLayer(layer);
                return;
            }

            // 克隆prefab
            var layerPrefabGo = GameObject.Instantiate(asset);
            layerPrefabGo.name = layerPrefabGo.name.Replace("(Clone)", "");

            // 将克隆的prefab添加到layer
            layer.AttachGameObject(layerPrefabGo);

            // 将layer挂到未使用列表
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
        /// 更新栈
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

            // 遍历所有的layer，1 设置 parent 2 设置offset 3 设置 camera的depth
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
                    // 为layer设置offset
                    layer.gameObject.transform.localPosition = next3DLayerOffset;
                    next3DLayerOffset = next3DLayerOffset + new Vector3(1000, 0, 0);
                    // 设置摄像机
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

            // 没用掉的直接销毁了
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

            // 设置渲染
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
        /// 排序
        /// </summary>
        /// <param name="layerStack"></param>
        private void SortLayerStack(List<SceneLayerBase> layerStack)
        {
            // 稳定的插入排序
            int i, j;
            // 当前排序元素的临时存储
            SceneLayerBase target;
            for (i = 1; i < layerStack.Count; i++)
            {
                j = i;

                // 获取当前排序的元素
                target = layerStack[i];

                // 如果当前的第j个元素和第j-1个元素满足下面的比较标准，那么需要交换这两个元素的位置
                // 将第j个元素插入到 前面的 已经排好序的元素 中一个合适的位置，使得插入后的元素序列仍然保持有序
                while (j > 0 && target.ComparePriority(layerStack[j - 1]) < 0)
                {
                    layerStack[j] = layerStack[j - 1];
                    j--;
                }

                layerStack[j] = target;
            }
        }

        /// <summary>
        /// 找到指定的layer
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

        #region 场景树对象
        /// <summary>
        /// 缓存sceneRoot相关的场景对象
        /// </summary>
        private GameObject m_sceneRootGo;
        private GameObject m_unusedLayerRootGo;
        private GameObject m_loadingLayerRootGo;
        private GameObject m_3DSceneRootGo;
        private GameObject m_uiSceneRootGo;

        private GameObject m_uiCameraTemplate;

        #endregion

        /// <summary>
        /// 加载中的layer列表
        /// </summary>
        private readonly List<SceneLayerBase> m_loadingLayerList = new List<SceneLayerBase>();

        /// <summary>
        /// 未使用的layer列表
        /// </summary>
        private readonly List<SceneLayerBase> m_unusedLayerList = new List<SceneLayerBase>();

        /// <summary>
        /// layer的栈，保存显示层级关系,链表尾部是栈顶
        /// </summary>
        private readonly List<SceneLayerBase> m_layerStack = new List<SceneLayerBase>();
        /// <summary>
        /// layer的名称字典
        /// </summary>
        private Dictionary<string, SceneLayerBase> m_layerDict = new Dictionary<string, SceneLayerBase>();

        // 当穿插3d物体时 需要多个ui相机
        private List<Camera> m_uiCameraList = new List<Camera>();
        /// <summary>
        /// layer的栈是否需要重新排序和计算显示关系
        /// </summary>
        private bool m_stackDirty;

        /// <summary>
        /// sorting order 更新
        /// </summary>
        private bool m_isSortingOrderModifierDirty;

        /// <summary>
        /// 额外生成的camera
        /// </summary>
        private List<Camera> m_extraCameraList = new List<Camera>();

        /// <summary>
        /// sceneroot 资源路径 如无则从头创建
        /// </summary>
        private static string m_sceneRootAssetPath = "SceneRoot";
    }
}