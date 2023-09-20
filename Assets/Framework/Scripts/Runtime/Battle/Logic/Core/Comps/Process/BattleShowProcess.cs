using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.Framework.Runtime.UI;
using UnityEngine;

namespace My.Framework.Battle
{
    public class BattleShowProcessBar : BattleShowProcess
    {
        public override int Type
        {
            get { return ProcessTypes.Bar; }
        }
    }


    public static class ProcessTypes
    {
        public const int Invalid = 0;
        public const int Bar = 1;
        public const int Wait = 2;
        public const int Print = 3;
        public const int Show = 4;
        public const int Announce = 10;
    }

    public abstract class BattleShowProcess
    {
        /// <summary>
        /// 标记
        /// </summary>
        public bool IsEnd;

        /// <summary>
        /// process的id
        /// </summary>
        public uint m_instanceId;

        /// <summary>
        /// process的类型
        /// </summary>
        public abstract int Type { get; }
    }
}
