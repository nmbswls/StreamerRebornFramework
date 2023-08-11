using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace My.Framework.Runtime.Director
{
    public class CutsceneTrackBase : TrackAsset
    {
        /// <summary>
        /// 执行绑定
        /// </summary>
        public virtual void InitializeBinding(PlayableDirector director, DirectorCutscene cutscene)
        {

        }
    }
}
