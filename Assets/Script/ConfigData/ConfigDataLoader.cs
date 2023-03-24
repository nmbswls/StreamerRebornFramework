using My.Framework.Runtime;
using My.Framework.Runtime.Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamerReborn.Config
{
    public partial class ConfigDataLoader : ConfigDataLoaderBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ConfigDataLoader()
        {
            InitValidConfigDataName();

            InitDeserializFunc4ConfigData();
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="onEnd"></param>
        /// <returns></returns>
        public void TryLoadInitConfig(System.Action<bool> onEnd)
        {
            var pathSet = new HashSet<string>();
            foreach (var configName in m_validConfigDataName)
            {
                pathSet.Add($"{ConfigPrefex}/{configName}.json");
            }
            //加载在init阶段需要加载的配置数据
            GameManager.Instance.StartCoroutine(InitLoadConfigData(pathSet, 1, 30, onEnd));
        }

        public const string ConfigPrefex = "Assets/RuntimeAssets/ConfigData/";
        
    }
}


