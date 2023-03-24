using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace My.Framework.Runtime.Resource
{
    [Serializable]
    public class SingleBundleInfo
    {
        /// <summary>
        /// bundle������
        /// </summary>
        public string m_bundleName = null;

        /// <summary>
        /// �� bundle��variant����
        /// </summary>
        public string m_variant;

        /// <summary>
        /// bundle type
        /// </summary>
        public bool m_isResident;

        /// <summary>
        /// ��Դ�б�
        /// </summary>
        public List<string> m_assetList = new List<string>();
    }

    /// <summary>
    /// ���abԪ��Ϣ
    /// </summary>
    public class BundlesMetaInfo : ScriptableObject
    {
        /// <summary>
        /// �汾��
        /// </summary>
        public int m_version = 1;

        /// <summary>
        /// bundle��Ϣ�б�
        /// </summary>
        public List<SingleBundleInfo> m_bundleInfoList = new List<SingleBundleInfo>();

        /// <summary>
        /// 
        /// </summary>
        public List<string> m_DLCList = new List<string>();
    }
}
