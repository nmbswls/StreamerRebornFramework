using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace My.Framework.Runtime.Saving
{

    public class SavingManagerBase
    {
        /// <summary>
        /// ��ʼ��
        /// </summary>
        public virtual bool Initialize()
        {
            RegisterSavingUnit();
            return true;
        }

        /// <summary>
        /// tickһ��
        /// </summary>
        public void Tick()
        {

        }

        /// <summary>
        /// �´浵
        /// </summary>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public void CreateNewSaving()
        {
            InitFromConfig();
        }

        /// <summary>
        /// ����Ƿ��д浵
        /// </summary>
        /// <returns></returns>
        public bool HasAnySaving()
        {
            // check ÿ���浵��λ
            for(int saveIdx = 0; saveIdx < MaxSaveCount; saveIdx++)
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
        /// <returns></returns>
        public bool LoadSaving(int saveIdx)
        {
            var savePath = GetSavePathByIdx(saveIdx);
            if (!LoadSavingFiles(savePath, out byte[] content))
            {
                return false;
            }

            // ��ȡ�浵
            RestructFromSaving(content);

            return true;
        }

        public void WriteSavingData(int saveIdx = 0)
        {
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
                    //��ȡ�����ļ�·��
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
        public bool LoadSavingFiles(string savePath, out byte[] saveData)
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
        /// �ռ��浵
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
        /// ɾ��
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

        /// <summary>
        /// ���save�б�
        /// key�Ǵ浵��Ԫ��
        /// value�Ǵ浵��Ԫ����
        /// </summary>
        protected Dictionary<string, byte[]> CollectAndFillSaveData()
        {
            var savingObjs = new Dictionary<string, byte[]>();

            foreach (var unit in m_savingUnitList)
            {
                string jsonStr = JsonConvert.SerializeObject(unit.GetSavingData());
                savingObjs.Add(unit.SavingUnitName, Encoding.UTF8.GetBytes(jsonStr));
            }

            return savingObjs;
        }

        /// <summary>
        /// ��ʼ���浵����
        /// </summary>
        /// <param name="savingData"></param>
        protected void InitFromConfig()
        {
            m_savingUnitList.Clear();

            foreach (var regEntry in m_savingUnitRegEntryDict.Values)
            {
                var unit = Activator.CreateInstance(regEntry.UnitType) as SavingUnitBase;
                if (unit == null)
                {
                    continue;
                }
                unit.InitEmpty();
            }
        }

        /// <summary>
        /// ���ļ��������ؽ�
        /// </summary>
        /// <param name="savingData"></param>
        /// <param name="savingObjs"></param>
        protected void RestructFromSaving(byte[] saveBytes)
        {
            // ���
            m_savingUnitList.Clear();

            string jsonStr = Encoding.UTF8.GetString(saveBytes);

            var dataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);

            foreach (var regEntry in m_savingUnitRegEntryDict.Values)
            {
                var unit = Activator.CreateInstance(regEntry.UnitType) as SavingUnitBase;
                if(unit == null)
                {
                    continue;
                }

                if (!dataDict.TryGetValue(regEntry.UnitName, out var data))
                {
                    unit.InitEmpty();
                }
                else
                {
                    unit.ReconstructFromData(data);
                }
            }
        }


        #region ����д


        /// ����ժҪ��Ϣ
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, string> GenerateSaveSammary()
        {
            var sammary = new Dictionary<string, string>();

            return sammary;
        }

        #endregion

        /// <summary>
        /// ��ȡ
        /// </summary>
        /// <param name="savePath"></param>
        protected Dictionary<string, string> ReadSummaryInfo(string savePath)
        {
            var summary = new Dictionary<string, string>();

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

        public const uint MagicNum = 0xC0C0BBBB;

        public int MaxSaveCount { get { return 10; } }

        protected string SavePathRoot
        {
            get { return Application.persistentDataPath + "/" + "save"; }
        }

        /// <summary>
        /// �Ѽ��浵������Ϣ
        /// </summary>
        public Dictionary<int, Dictionary<string,string>> CollectedSaveInfo = new Dictionary<int, Dictionary<string, string>>();


        #region ע��

        /// <summary>
        /// ע��洢��Ԫ
        /// </summary>
        protected virtual void RegisterSavingUnit()
        {
            m_savingUnitRegEntryDict["Summary"] = new SavingUnitRegEntry() { UnitName = "Summary", UnitType = typeof(SavingUnitSummary), IsOpen = true };
        }

        /// <summary>
        /// ע����
        /// </summary>
        public class SavingUnitRegEntry
        {
            public string UnitName;
            public Type UnitType;
            public bool IsOpen; // ֧�ֿ����õ���������
        }

        /// <summary>
        /// ע���б�
        /// </summary>
        protected Dictionary<string, SavingUnitRegEntry> m_savingUnitRegEntryDict = new Dictionary<string, SavingUnitRegEntry>();

        #endregion

        /// <summary>
        /// ��ǰ�洢�б�
        /// </summary>
        protected List<SavingUnitBase> m_savingUnitList;
    }

}


