using My.Framework.Runtime;
using My.Framework.Runtime.Config;
using My.Framework.Runtime.Storytelling;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace My.ConfigData
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

        public override void PreInitConfig()
        {
            {
                var storyBlockConfs = GetAllConfigDataStoryBlockInfo();
                foreach(var pair in storyBlockConfs)
                {
                    foreach(var commandId in pair.Value.CommandIdList)
                    {
                        var commandConf = GetConfigDataStoryCommandInfo(commandId);
                        pair.Value.CommandList.Add(commandConf);
                    }
                    foreach (string optionCommand in pair.Value.OptionIdList)
                    {
                        pair.Value.CommandOptionList.Add(new OptionCommand(optionCommand));
                    }
                }
            }
        }

        /// <summary>
        /// 通过配置名获取加载路径
        /// </summary>
        /// <returns></returns>
        protected override string GetConfigPathByName(string configName)
        {
            return $"{ConfigPrefex}/{configName}.json";
        }

        public const string ConfigPrefex = "Assets/RuntimeAssets/ConfigData/";
        
    }
}


