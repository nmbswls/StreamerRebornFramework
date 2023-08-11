using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using My.Framework.Runtime.Logic;
using UnityEngine;

namespace My.Framework.Runtime.Saving
{

    public interface ISavingManager
    {
        /// <summary>
        /// 创建新存档
        /// </summary>
        /// <returns></returns>
        void CreateNewSaving(int savingIndex);

        /// <summary>
        /// 检查是否有存档
        /// </summary>
        /// <returns></returns>
        bool HasAnySaving();

        /// <summary>
        /// 加载存档
        /// </summary>
        /// <param name="saveIdx"></param>
        /// <returns></returns>
        bool LoadSavingData(int saveIdx, out SavingData savingData);

        /// <summary>
        /// 写入存档文件
        /// </summary>
        /// <param name="saveIdx"></param>
        void WriteSavingFile(int saveIdx = 0, string comment = "");


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="saveIdx"></param>
        void DeleteSavingFile(int saveIdx);
    }

    /// <summary>
    /// 存档管理
    /// </summary>
    public class SavingManager : ISavingManager
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public virtual bool Initialize()
        {
            return true;
        }

        /// <summary>
        /// tick一下
        /// </summary>
        public void Tick()
        {

        }

        #region 对外接口实现类

        /// <summary>
        /// 新存档
        /// </summary>
        /// <returns></returns>
        public void CreateNewSaving(int savingIndex)
        {
            //InitFromConfig();
        }

        /// <summary>
        /// 检查是否有存档
        /// </summary>
        /// <returns></returns>
        public bool HasAnySaving()
        {
            // check 每个存档栏位
            for (int saveIdx = 0; saveIdx < MaxSaveCount; saveIdx++)
            {
                var savePath = GetSavePathByIdx(saveIdx);
                FileInfo saveFileInfo = new FileInfo(savePath);
                if (saveFileInfo.Exists)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 加载存档
        /// </summary>
        /// <param name="saveIdx"></param>
        /// <param name="savingData"></param>
        /// <returns></returns>
        public bool LoadSavingData(int saveIdx, out SavingData savingData)
        {
            savingData = null;

            var savePath = GetSavePathByIdx(saveIdx);
            if (!LoadSavingFiles(savePath, out byte[] content))
            {
                return false;
            }

            // 读取存档
            savingData = RestructFromFileBytes(content);
            return true;
        }

        /// <summary>
        /// 写入存档文件
        /// </summary>
        /// <param name="saveIdx"></param>
        /// <param name="content"></param>
        public void WriteSavingFile(int saveIdx = 0, string comment = "")
        {
            var savingDatas = CollectAndFillSaveData();

            if (!Directory.Exists(SavePathRoot))
            {
                Directory.CreateDirectory(SavePathRoot);
            }

            FileStream stream = null;
            var savePath = GetSavePathByIdx(saveIdx);

            foreach (var pair in savingDatas)
            {
                try
                {
                    //获取保存文件路径
                    var saveFile = new FileInfo(savePath);

                    stream = File.Open(saveFile.FullName, FileMode.Create);

                    BinaryWriter bw = new BinaryWriter(stream);
                    bw.Write(MagicNum);

                    SavingSummary sum = GenerateSaveSammary(comment);

                    var binaryFormatter = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream();
                    binaryFormatter.Serialize(ms, sum);
                    bw.Write(ms.ToArray().Length);
                    bw.Write(ms.ToArray());

                    bw.Write(pair.Value.Length);
                    bw.Write(pair.Value);

                    Debug.Log($"File save success : {saveFile.FullName}");
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("File save Failed, Error={0}", e.Message);
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="saveIdx"></param>
        public void DeleteSavingFile(int saveIdx)
        {
            DirectoryInfo dir = new DirectoryInfo(GetSavePathByIdx(saveIdx));
            if (!dir.Exists)
            {
                return;
            }
            foreach (var file in dir.GetFiles())
            {
                file.Delete();
            }
            dir.Delete();
            CollectSaveSummaryInfo();
        }

        /// <summary>
        /// 收集存档缩略信息
        /// </summary>
        /// <returns></returns>
        public void CollectSaveSummaryInfo()
        {
            CollectedSavingSummary = new Dictionary<int, SavingSummary>();
            if (!Directory.Exists(SavePathRoot))
            {
                return;
            }
            for (int saveIdx = 1; saveIdx <= MaxSaveCount; saveIdx++)
            {
                var dirName = GetSavePathByIdx(saveIdx);
                if (!Directory.Exists(dirName))
                {
                    continue;
                }
                var summary = ReadSummaryInfo(dirName);
                if (summary != null)
                {
                    CollectedSavingSummary.Add(saveIdx, summary);
                }
            }
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 加载指定单一文件存档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected bool LoadSavingFileSingle(string savePath, out byte[] saveData)
        {
            saveData = null;
            var fileList = new List<FileInfo>();
            FileInfo saveFileInfo = new FileInfo(savePath);
            if (!saveFileInfo.Exists)
            {
                return false;
            }
            fileList.Add(saveFileInfo);

            foreach (var fileInfo in fileList)
            {
                // 进行文件的反序列化
                // 存档格式 魔数 + 缩略 + 内容
                using (var fstream = File.OpenRead(savePath))
                {
                    try
                    {
                        BinaryReader br = new BinaryReader(fstream);
                        uint magic = br.ReadUInt32();
                        if (magic != MagicNum)
                        {
                            continue;
                        }
                        int summaryLen = br.ReadInt32();
                        var summaryBytes = br.ReadBytes(summaryLen);

                        int dataLen = br.ReadInt32();
                        var dataBytes = br.ReadBytes(dataLen);

                        saveData = dataBytes;
                        return true;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("存档损坏");
                        return false;
                    }
                    finally
                    {
                        if (fstream != null)
                        {
                            fstream.Close();
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 加载指定目录下的所有文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected bool LoadSavingFiles(string savePath, out byte[] saveData)
        {
            saveData = null;
            var fileList = new List<FileInfo>();
            FileInfo saveFileInfo = new FileInfo(savePath);
            if (!saveFileInfo.Exists)
            {
                return false;
            }
            fileList.Add(saveFileInfo);

            foreach (var fileInfo in fileList)
            {
                // 进行文件的反序列化
                // 存档格式 魔数 + 缩略 + 内容
                using (var fstream = File.OpenRead(savePath))
                {
                    try
                    {
                        BinaryReader br = new BinaryReader(fstream);
                        uint magic = br.ReadUInt32();
                        if (magic != MagicNum)
                        {
                            continue;
                        }
                        int summaryLen = br.ReadInt32();
                        var summaryBytes = br.ReadBytes(summaryLen);

                        int dataLen = br.ReadInt32();
                        var dataBytes = br.ReadBytes(dataLen);

                        saveData = dataBytes;
                        return true;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("存档损坏");
                        return false;
                    }
                    finally
                    {
                        if (fstream != null)
                        {
                            fstream.Close();
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="savePath"></param>
        protected SavingSummary ReadSummaryInfo(string savePath)
        {
            SavingSummary summary = null;

            // 进行文件的反序列化
            using (var fstream = File.OpenRead(savePath))
            {
                try
                {
                    BinaryReader br = new BinaryReader(fstream);
                    uint magic = br.ReadUInt32();
                    if (magic != MagicNum)
                    {
                        return null;
                    }
                    int summaryLen = br.ReadInt32();
                    var bytes = br.ReadBytes(summaryLen);
                    var ms = new MemoryStream(bytes);
                    //加载缓存文件中的DataSection                    
                    var formatter = new BinaryFormatter();
                    summary = formatter.Deserialize(ms) as SavingSummary;
                    return summary;
                }
                catch (SerializationException de)
                {
                    Debug.LogError(de.Message);
                    return summary;
                }
                catch (Exception)
                {
                    Debug.LogError("存档非法");
                    return null;
                }
                finally
                {
                    if (fstream != null)
                    {
                        fstream.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 存档路径
        /// 单文件存档返回文件名
        /// 多文件存档返回存档根目录
        /// </summary>
        /// <param name="saveIdx"></param>
        /// <returns></returns>
        protected string GetSavePathByIdx(int saveIdx)
        {
            return $"{SavePathRoot}/save_{saveIdx}.save";
        }


        #endregion

        #region 供重写

        /// <summary>
        /// 通过文件数据重建saving对象
        /// </summary>
        /// <returns></returns>
        protected virtual SavingData RestructFromFileBytes(byte[] content)
        {
            return new SavingData();
        }

        /// <summary>
        /// 生成摘要信息
        /// </summary>
        /// <returns></returns>
        protected virtual SavingSummary GenerateSaveSammary(string comment)
        {
            var summary = new SavingSummary();
            summary.SavingName = comment;
            return summary;
        }

        /// <summary>
        /// 填充save列表
        /// key是存档单元名
        /// value是存档单元数据
        /// </summary>
        protected virtual Dictionary<string, byte[]> CollectAndFillSaveData()
        {
            var savingObjs = new Dictionary<string, byte[]>();

            

            return savingObjs;
        }



        #endregion

        #region 内部变量


        public const uint MagicNum = 0xC0C0BBBB;

        public static int MaxSaveCount { get { return 10; } }

        protected string SavePathRoot
        {
            get { return Application.persistentDataPath + "/" + "save"; }
        }

        /// <summary>
        /// 搜集存档缩略信息
        /// </summary>
        public Dictionary<int, SavingSummary> CollectedSavingSummary = new Dictionary<int, SavingSummary>();

        /// <summary>
        /// 当前存档内存数据
        /// </summary>
        protected SavingData m_currSavingData;
        public SavingData CurrSavingData
        {
            get { return m_currSavingData; }
        }

        #endregion

    }

}


