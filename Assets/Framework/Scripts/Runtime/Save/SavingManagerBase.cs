using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace My.Framework.Runtime.Saving
{
    public class SavingManagerBase
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
        /// <summary>
        /// 新存档
        /// </summary>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public void CreateNewSaving()
        {
            InitFromConfig();
        }

        public bool LoadSaving(int saveIdx)
        {
            var savePath = GetSavePathByIdx(saveIdx);
            if (!LoadSavingFiles(savePath, out var retList))
            {
                return false;
            }
            RestructFromPersistent(retList);
            return true;
        }

        public void WriteSavingData(int saveIdx = 0)
        {
            if (saveIdx == 0)
            {
                saveIdx = CurrSaveIndex;
            }
            var savingDatas = CollectAndFillSaveData();

            if(!Directory.Exists(SavePathRoot))
            {
                Directory.CreateDirectory(SavePathRoot);
            }

            FileStream stream = null;
            var savePath = GetSavePathByIdx(saveIdx);


            foreach(var pair in savingDatas)
            {
                try
                {
                    //获取保存文件路径
                    var saveFile = new FileInfo(savePath);
                    
                    stream = File.Open(saveFile.FullName, FileMode.Create);

                    BinaryWriter bw = new BinaryWriter(stream);
                    bw.Write(MagicNum);

                    var sum = GenerateSaveSammary();

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
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool LoadSavingFiles(string savePath, out Dictionary<string, byte[]> saveData)
        {
            saveData = new Dictionary<string, byte[]>();

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

                        saveData.Add(fileInfo.Name, dataBytes);
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
        /// 收集存档
        /// </summary>
        /// <returns></returns>
        public void CollectSaveFile()
        {
            CollectedSaveInfo = new Dictionary<int, Dictionary<string,string>>();
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
                    CollectedSaveInfo.Add(saveIdx, summary);
                }
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="saveIdx"></param>
        public void DeleteSaving(int saveIdx)
        {
            DirectoryInfo dir = new DirectoryInfo(GetSavePathByIdx(saveIdx));
            if (!dir.Exists)
            {
                return;
            }
            foreach(var file in dir.GetFiles())
            {
                file.Delete();
            }
            dir.Delete();
            CollectSaveFile();
        }


        #region 供重写

        /// <summary>
        /// 填充save
        /// </summary>
        protected virtual Dictionary<string, byte[]> CollectAndFillSaveData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 初始化存档数据
        /// </summary>
        /// <param name="savingData"></param>
        protected virtual void InitFromConfig()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 从文件数据中重建
        /// </summary>
        /// <param name="savingData"></param>
        /// <param name="savingObjs"></param>
        protected virtual void RestructFromPersistent(Dictionary<string, byte[]> savingObjs)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, string> GenerateSaveSammary()
        {
            return new Dictionary<string, string>();
        }

        #endregion


        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="savePath"></param>
        protected Dictionary<string, string> ReadSummaryInfo(string savePath)
        {
            var summary = new Dictionary<string, string>();

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
                    summary = formatter.Deserialize(ms) as Dictionary<string, string>;
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

        public const uint MagicNum = 0xC0C0BBBB;
        public const int MaxSaveCount = 10;
        protected string SavePathRoot
        {
            get { return Application.persistentDataPath + "/" + "save"; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int CurrSaveIndex = 0;

        ///// <summary>
        ///// 是否是单文件存档
        ///// </summary>
        //public virtual bool IsSingleFile { get { return true; } }

        /// <summary>
        /// 搜集存档缩略信息
        /// </summary>
        public Dictionary<int, Dictionary<string,string>> CollectedSaveInfo = new Dictionary<int, Dictionary<string, string>>();
    }

}


