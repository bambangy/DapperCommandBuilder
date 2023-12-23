using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlCommandBuilder
{
    public enum CommandReferenceType
    {
        Join,
        InnerJoin,
        LeftJoin,
        RightJoin,
        OuterJoin
    }
}
