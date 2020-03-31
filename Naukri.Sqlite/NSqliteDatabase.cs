using System;
using System.Collections.Generic;
using System.Text;

namespace Naukri.Sqlite
{
    public sealed class NSqliteDatabase
    {
        private readonly string connectText;

        public NSqliteDatabase(string connectText)
        {
            this.connectText = $"data source={connectText}";
        }

        public void CreateTable<Table>()
        {
            NSqliteTableInfo.SetTableInfo<Table>(connectText);
        }
    }
}
