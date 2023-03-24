using My.Framework.Runtime.Saving;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StreamerReborn.Saving
{
    public class SavingManager : SavingManagerBase
    {

        
        /// <summary>
        /// ÷ÿ–¥∑Ω∑®
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, byte[]> CollectAndFillSaveData()
        {
            var gamePlayer = GameStatic.GamePlayer;
            Dictionary<string, byte[]> savingObjs = new Dictionary<string, byte[]>();
            savingObjs["main"] = System.Text.Encoding.UTF8.GetBytes(gamePlayer.StringData);
            return savingObjs;
        }

        protected override void InitFromConfig()
        {
            var gamePlayer = GameStatic.GamePlayer;
            gamePlayer.datas["Level"] = "1";
        }

        protected override void RestructFromPersistent(Dictionary<string, byte[]> savingObjs)
        {
            if(savingObjs.Count == 0)
            {
                return;
            }
            var data = savingObjs.First().Value;

            var gamePlayer = GameStatic.GamePlayer;

            var str = System.Text.Encoding.UTF8.GetString(data);
            gamePlayer.StringData = str;
        }
    }

}

