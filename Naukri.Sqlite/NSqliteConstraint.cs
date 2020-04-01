using System;
using System.Collections.Generic;
using System.Text;

namespace Naukri.Sqlite
{
    [Flags]
    public enum NSqliteConstraint
    {
        None = 0,
        PrimaryKey = 1,
        Unique = 2,
        NotNull = 4,
        Default = 8,
        Check = 16,
        Autoincrement = 32
    }
}
