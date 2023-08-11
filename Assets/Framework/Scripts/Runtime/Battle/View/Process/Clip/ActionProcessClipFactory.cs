using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.UI;
using UnityEngine;

namespace My.Framework.Battle.View
{

    public class WaitCallbackProcessClip : ProcessClip
    {
        public bool IsCallbacked = false;

    }



    public static class ProcessClipFactory
    {
        public static ProcessClip CreateProcessClip(BattleShowProcess process)
        {

            switch (process.Type)
            {
                case ProcessTypes.Announce:
                {
                    var cbClip = new WaitCallbackProcessClip();

                    UIControllerBattlePerform.Instance.ShowAnnounce(Vector3.zero, "我是一条提示",
                        () => { cbClip.IsCallbacked = true;});
                    
                    return cbClip;
                }
            }

            //ActionProcessClip actionClip = CreateActionProcessClip(process.Type);
            //if (actionClip != null)
            //{
            //    //actionClip.SetActionProcess(process);
            //    return actionClip;
            //}
            //Debug.LogError("Error: Cannot Create ActionProcess of Type " + process.Type);
            return null;
        }

    }
}
