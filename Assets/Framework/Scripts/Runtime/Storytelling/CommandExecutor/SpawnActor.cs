using My.Framework.Runtime.Director;
using My.Framework.Runtime.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Runtime.Storytelling
{
    public class StoryCommandInfo_SpawnActor : StoryCommandInfoBase
    {
        /// <summary>
        /// actor刷新信息
        /// </summary>
        public Dictionary<int, string> SpawnActorInfo = new Dictionary<int, string>();

        /// <summary>
        /// 解析param
        /// </summary>
        public void ParseParamString(string paramString)
        {
            var paramList = paramString.Split(';');
            foreach (var item in paramList)
            {
                var itemPram = item.Split('#');
                if (itemPram.Length < 2)
                {
                    continue;
                }
                if (!int.TryParse(itemPram[0], out var actorId))
                {
                    continue;
                }

                if (SpawnActorInfo.ContainsKey(actorId))
                {
                    continue;
                }
                SpawnActorInfo[actorId] = itemPram[1];
            }
        }
    }


    /// <summary>
    /// 刷新actor
    /// </summary>
    public class StoryCommandExecutor_SpawnActor : StoryCommandExecutorBase
    {
        public override EnumCommandExecStatus Execute(StoryCommandInfoBase commandInfo, StoryContextBase ctx, IStoryCommandEnvBase env)
        {
            var runtimeData = ctx.CurrRuntimeData;

            var realCommandInfo = (StoryCommandInfo_SpawnActor)commandInfo;
            if(!runtimeData.Inited)
            {
                HashSet<string> resPathSet = new HashSet<string>();
                foreach (var actorInfo in realCommandInfo.SpawnActorInfo)
                {
                    var retPath = $"Assets/RuntimeAssets/Actors/{actorInfo.Key}.prefab";
                    resPathSet.Add(retPath);
                }

                Dictionary<string, UnityEngine.Object> resDict = new Dictionary<string, UnityEngine.Object>();
                SimpleResourceManager.Instance.StartLoadAssetsCorutine(resPathSet, resDict,
                () =>
                {
                    runtimeData.IsEnd = true;
                    if (resDict.Count == 0)
                    {
                        runtimeData.ErrorOccured = true;
                        return;
                    }
                    var cutscene = DirectorManager.Instance.m_currCutscene;
                    if (cutscene == null)
                    {
                        runtimeData.ErrorOccured = true;
                        return;
                    }
                    var root = cutscene.GetDynamicRoot();
                    if (root == null)
                    {
                        runtimeData.ErrorOccured = true;
                        return;
                    }

                    int successCount = 0;
                    foreach (var actorInfo in realCommandInfo.SpawnActorInfo)
                    {
                        var retPath = $"Assets/RuntimeAssets/Actors/{actorInfo.Key}.prefab";
                        if (!resDict.ContainsKey(retPath))
                        {
                            continue;
                        }
                        var newActorGo = GameObject.Instantiate(resDict[retPath], root);
                        if (newActorGo == null)
                        {
                            continue;
                        }
                        successCount += 1;
                    }
                    if (successCount < realCommandInfo.SpawnActorInfo.Count)
                    {
                        Debug.LogError($"Spawn Actor Not All Success. Success Count {successCount}");
                        return;
                    }

                }, loadAsync: true);

                runtimeData.Inited = true;
            }

            if(runtimeData.IsEnd)
            {
                return runtimeData.ErrorOccured ? EnumCommandExecStatus.Fail : EnumCommandExecStatus.Success;
            }
            return EnumCommandExecStatus.Running;
            
        }
    }
}
