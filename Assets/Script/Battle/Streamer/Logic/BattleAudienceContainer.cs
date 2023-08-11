using My.Framework.Runtime.Prefab;
using My.Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static StreamerReborn.UIComponentBattleCard;

namespace StreamerReborn
{
    public class BattleAudienceContainer : UIComponentBase
    {

        /// <summary>
        /// 完成绑定回调
        /// </summary>
        protected override void OnBindFiledsCompleted()
        {
            base.OnBindFiledsCompleted();

            m_audiencePrefab = GameStatic.ResourceManager.LoadAssetSync<GameObject>("Assets/RuntimeAssets/UI/UIPrefab/Battle/Audience.prefab");
            RegisterEvent();

            GenerateSpawnCenters();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvent()
        {
        }

        /// <summary>
        /// 反注册事件
        /// </summary>
        private void UnRegisterEvent()
        {
        }


        protected void GenerateSpawnCenters()
        {
            float padding = 80;
            Vector2 regionSize = new Vector2(Root.rect.width - 2 * padding, Root.rect.height - 2 * padding);
            if(regionSize.x <= 0 || regionSize.y <= 0)
            {
                Debug.LogError("GenerateSpawnCenters Error.");
                return;
            }
            var points = GeneratePoints(150, regionSize);
            foreach(var point in points)
            {
                var validPos = point - new Vector2(regionSize.x * 0.5f, regionSize.y * 0.5f);
                GameObject go = new GameObject();
                go.transform.parent = SpawnCenter;
                go.transform.localPosition = validPos;
                var image = go.AddComponent<Image>();
                image.rectTransform.sizeDelta = new Vector2(20,20);

                AudienceRootList.Add(go.transform);
            }
            AudienceRootOccupiedMap = new bool[points.Count];
        }

        #region 观众删减

        /// <summary>
        /// TempAddAudience
        /// </summary>
        public void TempAddAudience()
        {
            var emptySlot = GetEmptySlot();
            if(emptySlot == -1)
            {
                // 弄到别处
                return;
            }
            AudienceRootOccupiedMap[emptySlot] = true;
            var newAudience = GameObject.Instantiate<GameObject>(m_audiencePrefab, Root);
            var elemt = newAudience.GetComponent<UIComponentAudience>();
            newAudience.transform.position = AudienceRootList[emptySlot].position;
        }

        #endregion

        #region 观众分布


        public int GetEmptySlot()
        {
            m_cacheList.Clear();
            for (int i=0;i< AudienceRootList.Count;i++)
            {
                if(!AudienceRootOccupiedMap[i])
                {
                    m_cacheList.Add(i);
                }
            }
            if(m_cacheList.Count == 0)
            {
                return -1;
            }
            int randVal = UnityEngine.Random.Range(0, m_cacheList.Count);
            return m_cacheList[randVal];
        }
        private List<int> m_cacheList = new List<int>();

        public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
        {
            float cellSize = radius / Mathf.Sqrt(2);

            int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
            List<Vector2> points = new List<Vector2>();
            List<Vector2> spawnPoints = new List<Vector2>();

            spawnPoints.Add(sampleRegionSize / 2);
            int iterCnt = 0;
            while (spawnPoints.Count > 0)
            {
                int spawnIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
                Vector2 spawnCentre = spawnPoints[spawnIndex];
                bool candidateAccepted = false;

                for (int i = 0; i < numSamplesBeforeRejection; i++)
                {
                    float angle = UnityEngine.Random.value * Mathf.PI * 2;
                    Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                    Vector2 candidate = spawnCentre + dir * UnityEngine.Random.Range(radius, 2 * radius);
                    if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                    {
                        points.Add(candidate);
                        spawnPoints.Add(candidate);
                        grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }

                if (!candidateAccepted)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                }
                iterCnt++;
                if(iterCnt > 1000000)
                {
                    Debug.LogError("iterCntiterCntiterCntiterCnt dead loop");
                    break;
                }
            }

            return points;
        }

        private static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
        {
            if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
            {
                int cellX = (int)(candidate.x / cellSize);
                int cellY = (int)(candidate.y / cellSize);
                int searchStartX = Mathf.Max(0, cellX - 2);
                int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
                int searchStartY = Mathf.Max(0, cellY - 2);
                int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

                for (int x = searchStartX; x <= searchEndX; x++)
                {
                    for (int y = searchStartY; y <= searchEndY; y++)
                    {
                        int pointIndex = grid[x, y] - 1;
                        if (pointIndex != -1)
                        {
                            float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                            if (sqrDst < radius * radius)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        #endregion



        #region 缓存

        private GameObject m_audiencePrefab;
        /// <summary>
        /// 缓存
        /// </summary>
        private Queue<GameObject> m_cachedAudienceCard = new Queue<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        public List<Transform> AudienceRootList = new List<Transform>();

        /// <summary>
        /// 
        /// </summary>
        public bool[] AudienceRootOccupiedMap;

        #endregion

        #region 绑定区域

        [AutoBind("./")]
        public RectTransform Root;

        [AutoBind("./SpawnCenter")]
        public RectTransform SpawnCenter;

        #endregion
    }
}

