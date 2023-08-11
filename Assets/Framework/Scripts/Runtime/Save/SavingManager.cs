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
        /// �����´浵
        /// </summary>
        /// <returns></returns>
        void CreateNewSaving(int savingIndex);

        /// <summary>
        /// ����Ƿ��д浵
        /// </summary>
        /// <returns></returns>
        bool HasAnySaving();

        /// <summary>
        /// ���ش浵
        /// </summary>
        /// <param name="saveIdx"></param>
        /// <returns></returns>
        bool LoadSavingData(int saveIdx, out SavingData savingData);

        /// <summary>
        /// д��浵�ļ�
        /// </summary>
        /// <param name="saveIdx"></param>
        void WriteSavingFile(int saveIdx = 0, string comment = "");


        /// <summary>
        /// ɾ��
        /// </summary>
        /// <param name="saveIdx"></param>
        void DeleteSavingFile(int saveIdx);
    }

    /// <summary>
    /// �浵����
    /// </summary>
    public class SavingManager : ISavingManager
    {
        /// <summary>
        /// ��ʼ��
        /// </summary>
        public virtual bool Initialize()
        {
            return true;
        }

        /// <summary>
        /// tickһ��
        /// </summary>
        public void Tick()
        {

        }

        #region ����ӿ�ʵ����

        /// <summary>
        /// �´浵
        /// </summary>
        /// <returns></returns>
        public void CreateNewSaving(int savingIndex)
        {
            //InitFromConfig();
        }

        /// <summary>
        /// ����Ƿ��д浵
        /// </summary>
        /// <returns></returns>
        public bool HasAnySaving()
        {
            // check ÿ���浵��λ
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
        /// ���ش浵
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

            // ��ȡ�浵
            savingData = RestructFromFileBytes(content);
            return true;
        }

        /// <summary>
        /// д��浵�ļ�
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
                    //��ȡ�����ļ�·��
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
        /// ɾ��
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
        /// �ռ��浵������Ϣ
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

        #region �ڲ�����

        /// <summary>
        /// ����ָ����һ�ļ��浵
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
                // �����ļ��ķ����л�
                // �浵��ʽ ħ�� + ���� + ����
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
                        Debug.LogError("�浵��");
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
        /// ����ָ��Ŀ¼�µ������ļ�
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
                // �����ļ��ķ����л�
                // �浵��ʽ ħ�� + ���� + ����
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
                        Debug.LogError("�浵��");
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
        /// ��ȡ
        /// </summary>
        /// <param name="savePath"></param>
        protected SavingSummary ReadSummaryInfo(string savePath)
        {
            SavingSummary summary = null;

            // �����ļ��ķ����л�
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
                    //���ػ����ļ��е�DataSection                    
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
                    Debug.LogError("�浵�Ƿ�");
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
        /// �浵·��
        /// ���ļ��浵�����ļ���
        /// ���ļ��浵���ش浵��Ŀ¼
        /// </summary>
        /// <param name="saveIdx"></param>
        /// <returns></returns>
        protected string GetSavePathByIdx(int saveIdx)
        {
            return $"{SavePathRoot}/save_{saveIdx}.save";
        }


        #endregion

        #region ����д

        /// <summary>
        /// ͨ���ļ������ؽ�saving����
        /// </summary>
        /// <returns></returns>
        protected virtual SavingData RestructFromFileBytes(byte[] content)
        {
            return new SavingData();
        }

        /// <summary>
        /// ����ժҪ��Ϣ
        /// </summary>
        /// <returns></returns>
        protected virtual SavingSummary GenerateSaveSammary(string comment)
        {
            var summary = new SavingSummary();
            summary.SavingName = comment;
            return summary;
        }

        /// <summary>
        /// ���save�б�
        /// key�Ǵ浵��Ԫ��
        /// value�Ǵ浵��Ԫ����
        /// </summary>
        protected virtual Dictionary<string, byte[]> CollectAndFillSaveData()
        {
            var savingObjs = new Dictionary<string, byte[]>();

            

            return savingObjs;
        }



        #endregion

        #region �ڲ�����


        public const uint MagicNum = 0xC0C0BBBB;

        public static int MaxSaveCount { get { return 10; } }

        protected string SavePathRoot
        {
            get { return Application.persistentDataPath + "/" + "save"; }
        }

        /// <summary>
        /// �Ѽ��浵������Ϣ
        /// </summary>
        public Dictionary<int, SavingSummary> CollectedSavingSummary = new Dictionary<int, SavingSummary>();

        /// <summary>
        /// ��ǰ�浵�ڴ�����
        /// </summary>
        protected SavingData m_currSavingData;
        public SavingData CurrSavingData
        {
            get { return m_currSavingData; }
        }

        #endregion

    }

}


