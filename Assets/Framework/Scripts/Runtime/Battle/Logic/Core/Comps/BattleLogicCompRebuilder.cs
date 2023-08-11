using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace My.Framework.Battle.Logic
{

    /// <summary>
    /// 通过装配驱动力 
    /// 以及不同的上层process 可以实现不同类型的重放（有head 或无head
    /// </summary>
    public class BattleLogicCompRebuilder : BattleLogicCompBase
    {
        public BattleLogicCompRebuilder(IBattleLogicCompOwnerBase owner, List<BattleOpt> battleOptList) : base(owner)
        {
            if(battleOptList == null || battleOptList.Count == 0)
            {
                m_isRebuildEnd = true;
            }
            else
            {
                m_isRebuildEnd = false;
                m_battleOptRecordQueue.Clear();
                foreach (var opt in battleOptList)
                {
                    m_battleOptRecordQueue.Enqueue(opt);
                }
            }
        }


        public override string CompName { get { return GamePlayerCompNames.Rebuilder; } }

        public override bool Initialize()
        {
            if(!base.Initialize())
            {
                return false;
            }

            //eventDispatcher.EventOnControllerWait4Input += OnControllerWait4Input;

            return true;
        }


        /// <summary>
        /// tick
        /// </summary>
        /// <param name="dt"></param>
        public override void Tick(float dt)
        {
            // 1. 已经rebuild完成
            if (m_isRebuildEnd)
            {
                return;
            }

            // 2. 检查是否RebuildEnd
            if (RebuildEndCheck())
            {
                RebuildEnd();
                return;
            }

            // 3. 执行OptCmdInput
            OptCmdInputExec();
        }

        /// <summary>
        /// 检查 是否Rebuild结束
        /// </summary>
        /// <returns></returns>
        protected virtual bool RebuildEndCheck()
        {
            // 1. 检查Rebuild操作是否完成
            if (!IsOptComplete())
            {
                return false;
            }

            // 2. 否则Rebuild结束检查通过
            return true;
        }

        /// <summary>
        /// Rebuild完成事件
        /// </summary>
        protected void RebuildEnd()
        {
            Debug.Log("RebuildEnd !");

            // 1. 设置BuildEnd标记，注销自己关注的事件
            m_isRebuildEnd = true;

            // 2. 触发Rebuild结束事件
            EventOnRebuildEnd?.Invoke();
        }

        /// <summary>
        /// 是否操作结束
        /// </summary>
        /// <returns></returns>
        protected bool IsOptComplete()
        {
            if (m_totalOptCount >= m_completeOptCount)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 执行指令输入
        /// </summary>
        private void OptCmdInputExec()
        {
            // 将当前tickSeq一致的cmd都push到队列中
            int currTickSeq = TickCount;
            // 防止一帧input过多cmd，设置一个上限
            int maxInputCount = 10000;
            int currInputCount = 0;
            while (true)
            {
                currInputCount++;
                if (currInputCount >= maxInputCount)
                {
                    RebuildFailExec(default, -100);
                    return;
                }

                // 检查队列是否合法
                if (m_battleOptRecordQueue.Count == 0)
                {
                    break;
                }

                // 检查指令是否合法
                var cmd = m_battleOptRecordQueue.Peek();
                if (cmd.m_seq < currTickSeq)
                {
                    RebuildFailExec(cmd, -120);
                    return;
                }

                // 检查指令是否是当前Tick
                if (cmd.m_seq > currTickSeq)
                {
                    break;
                }

                // 检查actor状态是否和cmd的actor状态一致
                // 获取当前的actor
                BattleController currCtrl = null;
                // 直接Push指令
                OptCmdInputDirectly(cmd, currCtrl);
            }
        }

        /// <summary>
        /// 执行Rebuild失败流程
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="errorCode"></param>
        protected void RebuildFailExec(BattleOpt opt, int errorCode)
        {
            m_rebuildErrorCode = errorCode;

            // todo:抛出rebuild失败的事件
        }

        /// <summary>
        /// 根据Type直接Input指令
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="currTurnActionActor"></param>
        private void OptCmdInputDirectly(BattleOpt opt, BattleController controller)
        {
            // 真正取出需要执行的指令 需要补充每个指令的检查判断
            switch (opt.m_type)
            {
                case BattleOptType.SkillCast:
                    opt = m_battleOptRecordQueue.Dequeue();
                    controller.ExecOptCmd(opt);
                    break;
                default:
                    opt = m_battleOptRecordQueue.Dequeue();
                    break;
            }
        }

        #region 监听事件

        /// <summary>
        /// 事件 - 下层等待传入数据
        /// </summary>
        protected void OnControllerWait4Input(BattleController controller)
        {
            // 已结束或未开始
            if(m_isRebuildEnd)
            {
                return;
            }
        }

        #endregion


        /// <summary>
        /// Rebuild结束事件
        /// </summary>
        public event Action EventOnRebuildEnd;

        /// <summary>
        /// 有效tick数量
        /// </summary>
        public int TickCount;

        /// <summary>
        /// 是否已经Rebuild完成
        /// </summary>
        protected bool m_isRebuildEnd = false;

        /// <summary>
        /// 操作记录队列
        /// </summary>
        protected Queue<BattleOpt> m_battleOptRecordQueue = new Queue<BattleOpt>();

        /// <summary>
        /// 操作总数
        /// </summary>
        protected int m_totalOptCount;
        /// <summary>
        /// 操作完成个数
        /// </summary>
        protected int m_completeOptCount;

        /// <summary>
        /// 重建的错误代码
        /// </summary>
        protected int m_rebuildErrorCode;
    }
}
