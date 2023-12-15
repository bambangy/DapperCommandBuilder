using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public enum CommandMatchType
    {
        Equal,
        NotEqual,
        Lesser,
        Greater,
        LesserOrEqual,
        GreaterOrEqual,
        Contains,
        ContainsStart,
        ContainsEnd,
        NotContains,
        NotContainsStart,
        NotContainsEnd,
        IsNull,
        IsNotNull,
        IsIn,
        IsNotIn
    }
}
