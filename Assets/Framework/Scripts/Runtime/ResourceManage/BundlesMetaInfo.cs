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
        /// bundle的名称
        /// </summary>
        public string m_bundleName = null;

        /// <summary>
        /// 该 bundle的variant名称
        /// </summary>
        public string m_variant;

        /// <summary>
        /// bundle type
        /// </summary>
        public bool m_isResident;

        /// <summary>
        /// 资源列表
        /// </summary>
        public List<string> m_assetList = new List<string>();
    }

    /// <summary>
    /// 存放ab元信息
    /// </summary>
    public class BundlesMetaInfo : ScriptableObject
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int m_version = 1;

        /// <summary>
        /// bundle信息列表
        /// </summary>
        public List<SingleBundleInfo> m_bundleInfoList = new List<SingleBundleInfo>();

        /// <summary>
        /// 
        /// </summary>
        public List<string> m_DLCList = new List<string>();
    }
}
