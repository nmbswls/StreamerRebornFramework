using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Battle
{
    /// <summary>
    /// 拖尾特效清理
    /// </summary>
    public class TrailEffectHelper : MonoBehaviour
    {
        public void ResetTrailEffect()
        {
            foreach (var item in m_trailRenderers)
            {
                if (item == null)
                    continue;
                item.enabled = !item.enabled;
                item.Clear();
                item.enabled = !item.enabled;
            }
        }
        public List<TrailRenderer> m_trailRenderers = new List<TrailRenderer>();
    }
}
