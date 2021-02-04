using System;
using System.IO;
using System.Linq;
using System.Windows;
using FluDbCreate = FluGASv25.Dao.CreateSQL.DbCreate;

namespace CovGASv2.Dao.CreateSQL
{
    public static class DbCreate
    {
        private static readonly string[] DbTables = new string[]
        {
                    "/Dao/CreateSQL/CreateTableSample.txt",
                    "/Dao/CreateSQL/CreateTableMiseqParams.txt",
        };

        public static bool CheckDb(ref string message)
        {

            var resbool = true;

            // 初期状態 db-file が無ければテーブルを初期化する（初期化したいときはFile削除で）
            if (File.Exists(FluGASv25.Dao.DbCommon.sqliteFile))
                return resbool;

            var defaultableSqls = FluDbCreate.GetDefaultCreateTabels().ToList();
            if (!defaultableSqls.Any())
            {
                message += " initialisation Error,  no createable table list ";
                return false;
            }

            // FluGAS と同じTable が作られる
            foreach (var sql in defaultableSqls)
                FluDbCreate.ExecCreateDb(sql); // error でも取り敢えず実行

            // CovGAS 独自
            foreach (var sqlTxt in DbTables)
            {
                var res = CreateLocalDb(sqlTxt);
                if (! string.IsNullOrEmpty( res))   // error messge
                { 
                    message += res + System.Environment.NewLine;
                    resbool = false;   // Exception message があるとき
                }
            }

            return resbool;
        }

        private static string CreateLocalDb(string createSQL)
        {
            var createsql = string.Empty;
            var info = Application.GetResourceStream(new Uri(createSQL, UriKind.Relative));
            using (var sr = new StreamReader(info.Stream))
                createsql = sr.ReadToEnd();
            createsql = createsql.Replace(Environment.NewLine, string.Empty);

            return FluDbCreate.ExecCreateDb(createsql);
        }
    }
}