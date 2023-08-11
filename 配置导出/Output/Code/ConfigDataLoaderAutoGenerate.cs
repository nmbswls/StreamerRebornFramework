//
// Auto Generated Code By excel2json
// 1. 定义Config Loader 提供基本加载方法


using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using My.Framework.Runtime.Config;
namespace My.ConfigData {

    public partial class ConfigDataLoader : ConfigDataLoaderBase
    {
        protected override void InitValidConfigDataName()
        {

            m_validConfigDataName.Add("CardBattleInfo");

            m_validConfigDataName.Add("CutsceneInfo");

            m_validConfigDataName.Add("StoryBlockInfo");

            m_validConfigDataName.Add("StoryCommandInfo");

            m_validConfigDataName.Add("MultiLangInfo_CN");

            m_validConfigDataName.Add("MultiLangInfo_EN");

        }
        protected override void InitDeserializFunc4ConfigData()
        {

            m_deserializFuncDict["CardBattleInfo"] = DeserializFunc4ConfigDataCardBattleInfo;

            m_deserializFuncDict["CutsceneInfo"] = DeserializFunc4ConfigDataCutsceneInfo;

            m_deserializFuncDict["StoryBlockInfo"] = DeserializFunc4ConfigDataStoryBlockInfo;

            m_deserializFuncDict["StoryCommandInfo"] = DeserializFunc4ConfigDataStoryCommandInfo;

            m_deserializFuncDict["MultiLangInfo_CN"] = DeserializFunc4ConfigDataMultiLangInfo_CN;

            m_deserializFuncDict["MultiLangInfo_EN"] = DeserializFunc4ConfigDataMultiLangInfo_EN;

        }
        public void DeserializFunc4ConfigDataCardBattleInfo(string content)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<int, ConfigDataCardBattleInfo>>(content);
            if(data != null)
            {
                foreach(var pair in data)
                {
                    m_ConfigDataCardBattleInfoData[pair.Key] = pair.Value;
                }
            }
        }
        public void DeserializFunc4ConfigDataCutsceneInfo(string content)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<int, ConfigDataCutsceneInfo>>(content);
            if(data != null)
            {
                foreach(var pair in data)
                {
                    m_ConfigDataCutsceneInfoData[pair.Key] = pair.Value;
                }
            }
        }
        public void DeserializFunc4ConfigDataStoryBlockInfo(string content)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<int, ConfigDataStoryBlockInfo>>(content);
            if(data != null)
            {
                foreach(var pair in data)
                {
                    m_ConfigDataStoryBlockInfoData[pair.Key] = pair.Value;
                }
            }
        }
        public void DeserializFunc4ConfigDataStoryCommandInfo(string content)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<int, ConfigDataStoryCommandInfo>>(content);
            if(data != null)
            {
                foreach(var pair in data)
                {
                    m_ConfigDataStoryCommandInfoData[pair.Key] = pair.Value;
                }
            }
        }
        public void DeserializFunc4ConfigDataMultiLangInfo_CN(string content)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<int, ConfigDataMultiLangInfo_CN>>(content);
            if(data != null)
            {
                foreach(var pair in data)
                {
                    m_ConfigDataMultiLangInfo_CNData[pair.Key] = pair.Value;
                }
            }
        }
        public void DeserializFunc4ConfigDataMultiLangInfo_EN(string content)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<int, ConfigDataMultiLangInfo_EN>>(content);
            if(data != null)
            {
                foreach(var pair in data)
                {
                    m_ConfigDataMultiLangInfo_ENData[pair.Key] = pair.Value;
                }
            }
        }


#region 获取方法


        public ConfigDataCardBattleInfo GetConfigDataCardBattleInfo(int key)
        {
            ConfigDataCardBattleInfo data;
            if(m_ConfigDataCardBattleInfoData.TryGetValue(key, out data))
            {
                return data;
            }
            return null;
        }


        public Dictionary<int,ConfigDataCardBattleInfo> GetAllConfigDataCardBattleInfo()
        {
            return m_ConfigDataCardBattleInfoData;
        }


        public void ClearConfigDataCardBattleInfo()
        {
            m_ConfigDataCardBattleInfoData.Clear();
        }


        public ConfigDataCutsceneInfo GetConfigDataCutsceneInfo(int key)
        {
            ConfigDataCutsceneInfo data;
            if(m_ConfigDataCutsceneInfoData.TryGetValue(key, out data))
            {
                return data;
            }
            return null;
        }


        public Dictionary<int,ConfigDataCutsceneInfo> GetAllConfigDataCutsceneInfo()
        {
            return m_ConfigDataCutsceneInfoData;
        }


        public void ClearConfigDataCutsceneInfo()
        {
            m_ConfigDataCutsceneInfoData.Clear();
        }


        public ConfigDataStoryBlockInfo GetConfigDataStoryBlockInfo(int key)
        {
            ConfigDataStoryBlockInfo data;
            if(m_ConfigDataStoryBlockInfoData.TryGetValue(key, out data))
            {
                return data;
            }
            return null;
        }


        public Dictionary<int,ConfigDataStoryBlockInfo> GetAllConfigDataStoryBlockInfo()
        {
            return m_ConfigDataStoryBlockInfoData;
        }


        public void ClearConfigDataStoryBlockInfo()
        {
            m_ConfigDataStoryBlockInfoData.Clear();
        }


        public ConfigDataStoryCommandInfo GetConfigDataStoryCommandInfo(int key)
        {
            ConfigDataStoryCommandInfo data;
            if(m_ConfigDataStoryCommandInfoData.TryGetValue(key, out data))
            {
                return data;
            }
            return null;
        }


        public Dictionary<int,ConfigDataStoryCommandInfo> GetAllConfigDataStoryCommandInfo()
        {
            return m_ConfigDataStoryCommandInfoData;
        }


        public void ClearConfigDataStoryCommandInfo()
        {
            m_ConfigDataStoryCommandInfoData.Clear();
        }


        public ConfigDataMultiLangInfo_CN GetConfigDataMultiLangInfo_CN(int key)
        {
            ConfigDataMultiLangInfo_CN data;
            if(m_ConfigDataMultiLangInfo_CNData.TryGetValue(key, out data))
            {
                return data;
            }
            return null;
        }


        public Dictionary<int,ConfigDataMultiLangInfo_CN> GetAllConfigDataMultiLangInfo_CN()
        {
            return m_ConfigDataMultiLangInfo_CNData;
        }


        public void ClearConfigDataMultiLangInfo_CN()
        {
            m_ConfigDataMultiLangInfo_CNData.Clear();
        }


        public ConfigDataMultiLangInfo_EN GetConfigDataMultiLangInfo_EN(int key)
        {
            ConfigDataMultiLangInfo_EN data;
            if(m_ConfigDataMultiLangInfo_ENData.TryGetValue(key, out data))
            {
                return data;
            }
            return null;
        }


        public Dictionary<int,ConfigDataMultiLangInfo_EN> GetAllConfigDataMultiLangInfo_EN()
        {
            return m_ConfigDataMultiLangInfo_ENData;
        }


        public void ClearConfigDataMultiLangInfo_EN()
        {
            m_ConfigDataMultiLangInfo_ENData.Clear();
        }



#endregion


#region 数据定义

        private Dictionary<int, ConfigDataCardBattleInfo> m_ConfigDataCardBattleInfoData = new Dictionary<int, ConfigDataCardBattleInfo>();

        private Dictionary<int, ConfigDataCutsceneInfo> m_ConfigDataCutsceneInfoData = new Dictionary<int, ConfigDataCutsceneInfo>();

        private Dictionary<int, ConfigDataStoryBlockInfo> m_ConfigDataStoryBlockInfoData = new Dictionary<int, ConfigDataStoryBlockInfo>();

        private Dictionary<int, ConfigDataStoryCommandInfo> m_ConfigDataStoryCommandInfoData = new Dictionary<int, ConfigDataStoryCommandInfo>();

        private Dictionary<int, ConfigDataMultiLangInfo_CN> m_ConfigDataMultiLangInfo_CNData = new Dictionary<int, ConfigDataMultiLangInfo_CN>();

        private Dictionary<int, ConfigDataMultiLangInfo_EN> m_ConfigDataMultiLangInfo_ENData = new Dictionary<int, ConfigDataMultiLangInfo_EN>();

#endregion
    }
}
