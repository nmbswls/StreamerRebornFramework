using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace My.Framework.Runtime.Saving
{
    /// <summary>
    /// 基础存档数据
    /// </summary>
    public class SavingData
    {

    }

    /// <summary>
    /// 存储单元
    /// </summary>
    public abstract class SavingUnitBase
    {
        /// <summary>
        /// 获取存储单元名称
        /// </summary>
        public abstract string SavingUnitName { get; }

        /// <summary>
        /// 获取data bytes
        /// </summary>
        /// <returns></returns>
        public SavingData GetSavingData()
        {
            return m_savingData;
        }

        /// <summary>
        /// 持有数据
        /// </summary>
        protected SavingData m_savingData;

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void InitEmpty()
        {

        }

        /// <summary>
        /// 通过saving信息重建
        /// </summary>
        /// <param name="savingStr">存档</param>
        public abstract void ReconstructFromData(string savingStr);

    }

    /// <summary>
    /// 具体实现类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SavingUnitBase<T> : SavingUnitBase where T : SavingData
    {
        public override void ReconstructFromData(string savingStr)
        {
            m_savingData = JsonConvert.DeserializeObject<T>(savingStr);
            if (m_savingData == null)
            {
                Debug.LogError("RestructFromPersistent Error. Try Init");
            }

            // 触发回调
            OnReconstruct();
        }

        /// <summary>
        /// 创建完毕回调
        /// </summary>
        protected virtual void OnReconstruct()
        {

        }

        /// <summary>
        /// 获取真实类型
        /// </summary>
        public T SavingData { get { return (T)m_savingData; } }
    }

}

