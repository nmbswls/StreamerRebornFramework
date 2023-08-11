using My.Framework.Runtime.Storytelling;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.ConfigData
{
    public partial class ConfigDataStoryBlockInfo
    {
        /// <summary>
        /// 故事指令 - 配置列表
        /// </summary>
        public List<ConfigDataStoryCommandInfo> CommandList = new List<ConfigDataStoryCommandInfo>();

        /// <summary>
        /// 故事选项指令 - 列表
        /// </summary>
        public List<OptionCommand> CommandOptionList = new List<OptionCommand>();
    }

    public partial class ConfigDataStoryCommandInfo
    {
        public string GetCommandId()
        {
            return CommandId;
        }

        public string[] GetParamList()
        {
            var paramList = ParamString.Split('#');
            return paramList;
        }

        public string RawCommand { get { return ParamString; } }
    }

}
