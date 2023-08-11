using My.Framework.Runtime.Director;
using StreamerReborn.World;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace My.Runtime.Director
{
    [TrackColor(0.855f, 0.8623f, 0.870f)]
    [TrackClipType(typeof(ShowBubbleClip))]
    [TrackClipType(typeof(DoExpressionClip))]
    [TrackBindingType(typeof(ActorControllerBase))]
    public class MyActorTrack : CutsceneTrackBase
    {
        /// <summary>
        /// 绑定actor id
        /// </summary>
        public int BindingActorId;


        /// <summary>
        /// 初始化绑定信息
        /// </summary>
        public override void InitializeBinding(PlayableDirector director, DirectorCutscene cutscene)
        {
            // 初始化绑定关系UISceneRoot
            var bindingTargetGo = cutscene.GetActorById(BindingActorId);
            var ctrl = bindingTargetGo.GetComponent<ActorControllerBase>();
            if(ctrl == null)
            {
                Debug.LogError("MyActorTrack InitializeBinding Error");
                return;
            }

            director.SetGenericBinding(this, ctrl);

            // 初始化每个 Clip
            foreach (var clip in GetClips())
            {
                
            }
        }

        #region 处理clip绑定信息

        #endregion


        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
#if UNITY_EDITOR
            var comp = director.GetGenericBinding(this) as Transform;
            if (comp == null)
                return;
            var so = new UnityEditor.SerializedObject(comp);
            var iter = so.GetIterator();
            while (iter.NextVisible(true))
            {
                if (iter.hasVisibleChildren)
                    continue;
                driver.AddFromName<Transform>(comp.gameObject, iter.propertyPath);
            }
#endif
            base.GatherProperties(director, driver);
        }
    }
}


