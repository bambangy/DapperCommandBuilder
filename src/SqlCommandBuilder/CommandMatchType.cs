using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlCommandBuilder
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
        NotContains,
        IsNull,
        IsNotNull,
        IsIn,
        IsNotIn
    }
}
