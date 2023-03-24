using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace My.Framework.Editor.Build
{
    /// <summary>
    /// 描述bundle信息的资源文件，需要固定名字为BundleDescription.Asset
    /// 放置在bundle目录下的第一级目录
    /// BundleDescription 不允许嵌套
    /// </summary>
    [CreateAssetMenu(menuName = "AssetBuild/BundleDescription", fileName = BundleDescription.BundleDescriptionAssetName)]
    public class BundleDescription : ScriptableObject
    {
        /// <summary>
        /// 替换bundle路径最后一个目录节点名字的字符串
        /// </summary>
        [Header("[bundle--替换最近一层目录的名字的字符串]")]
        public string m_replaceLastFolderNameStr;

        /// <summary>
        /// bundle变体的名字
        /// </summary>
        [Header("[bundle多语言配置--bundle变体名称]")]
        public string m_bundleVariantName;


        [Header("[Bundle是否持久]")]
        public bool mIsResident = false;

        [Header("[Manifest是否记录Prefab]")]
        public bool mbTogglePrefab = true;

        [Header("[Manifest是否记录Material]")]
        public bool mbToggleMaterial = true;

        [Header("[Manifest是否记录.asset]")]
        public bool mbToggleAsset = true;

        [Header("[Manifest是否记录Texture]")]
        public bool mbToggleTexture = true;

        [Header("[Manifest是否记录Timeline]")]
        public bool mbToggleTimeline = true;

        [Header("[Manifest是否记录Shader]")]
        public bool mbToggleShader = false;

        /// <summary>
        /// bundle名称，根据设置自动生成，不需要编辑
        /// </summary>
        [HideInInspector]
        public List<string> m_bundleNameList;

        // 资源名字
        public const string BundleDescriptionAssetName = "BundleDescription";
    }
}
