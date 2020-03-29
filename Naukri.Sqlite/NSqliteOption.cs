using System;

namespace Naukri.Sqlite
{
    [Flags]
    public enum NSqliteOption
    {
        None = 0,
        CheckSchema = 1,

        Default = CheckSchema
    }
}
