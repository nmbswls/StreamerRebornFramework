//
// Auto Generated Code By excel2json
// https://neil3d.gitee.io/coding/excel2json.html
// 1. 每个 Sheet 形成一个 Struct 定义, Sheet 的名称作为 Struct 的名称
// 2. 表格约定：第一行是变量名称，第二行是变量类型

using System;
using System.Collections.Generic;

namespace My.ConfigData {

#region subtypes

    public class Tuple2
    {
        public Int32 P1; // 
        public Int32 P2; // 
    }


    public class Tuple3
    {
        public Int32 P1; // 
        public Int32 P2; // 
        public Int32 P3; // 
    }


    public class Tuple4
    {
        public Int32 P1; // 
        public Int32 P2; // 
        public Int32 P3; // 
        public Int32 P4; // 
    }


    public class ActorBornInfo
    {
        public Int32 ActorId; // 
        public String BornPoint; // 
    }

#endregion



#region enums


    public enum Sample
    {
    }


#endregion



#region datas

    public partial class ConfigDataCardBattleInfo
    {

        public Int32 ID; // 主键

        public Int32 CardType; // 类型

        public String Text; // 描述

        public String Image; // 图片

    }


    public partial class ConfigDataCutsceneInfo
    {

        public Int32 ID; // 主键

        public Int32 EntryStoryBlockId; // 入口剧情ID

        public String SetupSceneName; // 场景

        public List<ActorBornInfo> ActorBornInfos= new List<ActorBornInfo>(); // actor出生信息 actorid,出生点

    }


    public partial class ConfigDataStoryBlockInfo
    {

        public Int32 ID; // 主键

        public String JsonInfo; // 名称

        public List<int> CommandIdList= new List<int>(); // 命令id列表

        public List<String> OptionIdList= new List<String>(); // 描述

    }


    public partial class ConfigDataStoryCommandInfo
    {

        public Int32 ID; // 主键

        public String CommandId; // 命令类型

        public String ParamString; // 命令参数

        public List<int> ParamInts= new List<int>(); // 整型命令参数

    }


    public partial class ConfigDataMultiLangInfo_CN
    {

        public Int32 ID; // 主键

        public String StrKey; // 多语言键

        public String OriginContent; // 原始内容

        public String ReplaceContent; // 替换内容

    }


    public partial class ConfigDataMultiLangInfo_EN
    {

        public Int32 ID; // 主键

        public String StrKey; // 多语言键

        public String OriginContent; // 原始内容

        public String ReplaceContent; // 替换内容

    }

#endregion

}


// End of Auto Generated Code
