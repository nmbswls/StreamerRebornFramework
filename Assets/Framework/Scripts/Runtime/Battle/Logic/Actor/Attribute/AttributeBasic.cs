using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My.Framework.Battle.Actor
{
    public enum AttributeCombineRule
    {
        Add,
        OneMinusMulti,
    }

    public static class AttributeIdConsts
    {
        public const int Invalid = 0;
        public const int Hp = 100;
        public const int SpecialHp = 101;
        public const int ShieldHp = 102;
        public const int HpMax = 105;
    }
}
