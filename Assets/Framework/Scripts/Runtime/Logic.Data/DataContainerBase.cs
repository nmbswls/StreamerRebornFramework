using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Runtime.Logic
{
    public abstract class DataBlock
    {
        
    }

    public interface IDataContainerBase
    {
        string Name { get; }
    }

    public abstract class DataContainerBase : IDataContainerBase
    {
        public abstract string Name { get; }

        public virtual void InitData(IDataSourceBase dataSource)
        {

        }
    }

}
