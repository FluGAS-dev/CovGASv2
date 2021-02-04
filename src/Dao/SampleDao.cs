using CovGASv2.Models.Properties;
using FluGASv25.Dao;
using System.Collections.Generic;
using System.Linq;

namespace CovGASv2.Dao
{
    public class SampleDao 
    {
        private const string TableName = "SAMPLE";
        public static long InsertSample(Sample s)
        {
            var withoutClm = new string[] { "id" };  // Sample登録で ID は AutoInclimentだから指定しない。
            long insertId = DbCommon.InsertTable(TableName, s, withoutClm);
            return insertId;
        }

        public static Sample[] GetSamples()
        {
            var allData = DbCommon.SelectTableAll(
                                                        TableName,
                                                        typeof(Sample));

            var list = allData.Select(s => s)
                                        .Cast<Sample>()
                                        .Where(s => s.Is_Delete == 0)
                                        .ToArray();

            // 必ず1件は在るはず（初期データベースに入れている）
            return (Sample[])list;
        }

        public static Sample GetSample(int sampleId , ref string message)
        {
            var samples = GetSamples();
            if (!samples.Any())
            {
                message = "sample data is not found, sample id = " + sampleId;
                return null;
            }

            // 当該サンプル情報
            return samples.Where(s => s.ID == sampleId).First();
        }

        // Sample のdelete は ISDELETE　を 0 以外にする。
        public static IEnumerable<long> DeleteSample(long[] deleteSampleIds)
        {
            var deleteIds = DbCommon.DeleteRecord(TableName, deleteSampleIds, "Is_Delete");
            return deleteIds;
        }

        public static long UpdateSample(Sample s)
        {
            var withoutClm = new string[] { "id" };  // Sample登録で ID は AutoInclimentだから指定しない。
            var updId = DbCommon.UpdateRecodeById(TableName, s, withoutClm);
            return updId;
        }


    }
}
