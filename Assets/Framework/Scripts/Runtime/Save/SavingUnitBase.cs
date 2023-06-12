using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace My.Framework.Runtime.Saving
{
    /// <summary>
    /// �����浵����
    /// </summary>
    public class SavingData
    {

    }

    /// <summary>
    /// �洢��Ԫ
    /// </summary>
    public abstract class SavingUnitBase
    {
        /// <summary>
        /// ��ȡ�洢��Ԫ����
        /// </summary>
        public abstract string SavingUnitName { get; }

        /// <summary>
        /// ��ȡdata bytes
        /// </summary>
        /// <returns></returns>
        public SavingData GetSavingData()
        {
            return m_savingData;
        }

        /// <summary>
        /// ��������
        /// </summary>
        protected SavingData m_savingData;

        /// <summary>
        /// ��ʼ��
        /// </summary>
        public virtual void InitEmpty()
        {

        }

        /// <summary>
        /// ͨ��saving��Ϣ�ؽ�
        /// </summary>
        /// <param name="savingStr">�浵</param>
        public abstract void ReconstructFromData(string savingStr);

    }

    /// <summary>
    /// ����ʵ����
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

            // �����ص�
            OnReconstruct();
        }

        /// <summary>
        /// ������ϻص�
        /// </summary>
        protected virtual void OnReconstruct()
        {

        }

        /// <summary>
        /// ��ȡ��ʵ����
        /// </summary>
        public T SavingData { get { return (T)m_savingData; } }
    }

}

