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


    public class Sample
    {
        public Int32 SampleField; // This is a int field
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
