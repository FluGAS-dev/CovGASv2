using System;
using System.IO;
using System.Linq;
using static FluGASv25.Dao.DbCommon;

namespace CovGASv2.Several
{
    public static class CreateReport
    {

        public static readonly string htaFile = "report.hta";
        public static readonly string samplename = "%SAMPLENAME%";
        public static readonly string reference = "%REFERENCE%";
        public static readonly string description = "%DESCRIPTION%";
        public static readonly string spices = "%SPICES%";
        public static readonly string host = "%HOST%";
        public static readonly string type = "%TYPE%";
        public static readonly string geolocation = "%GEOLOCATION%";
        public static readonly string collectiondate = "%COLLECTIONDATE%";
        public static readonly string releasedate = "%RELEASEDATE%";
        public static readonly string length = "%LENGTH%";
        public static readonly string ratio = "%RATIO%";
        public static readonly string ave = "%AVE%";
        public static readonly string seq = "%SEQ%";

        public static readonly int dbTop1 = 0;

        public static void OutReport(string outHtaPath, int sampleId, int topNo, ref string message)
        {
            // 当該サンプル情報
            var mes = string.Empty;
            var sample = Dao.SampleDao.GetSample(sampleId, ref mes);
            if(! string.IsNullOrEmpty(mes) || sample == null || sample.ID <= 0)
            {
                message += mes;
                return;
            }


            // read hta file...
            var htaPath = Path.Combine(
                                    AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                                    "data",
                                    htaFile);
            if (!File.Exists(htaPath))
            {
                message = "report file is not found. check data directory.... " + htaFile;
                return;
            }

            // hta file read...
            var htaLines = WfComponent.Utils.FileUtils.ReadFile(htaPath, ref message);
            if (!string.IsNullOrEmpty(message)) return;


            var htaLine = string.Join(Environment.NewLine, htaLines);
            // 情報の入れ替え。
            htaLine = htaLine.Replace(samplename, sample.ViewName);
            htaLine = htaLine.Replace(reference, GetDbValue(sample.Accession, topNo));
            htaLine = htaLine.Replace(description, GetDbValue(sample.Genbank_Title, topNo));
            htaLine = htaLine.Replace(spices, GetDbValue(sample.Species, topNo));
            htaLine = htaLine.Replace(host, GetDbValue(sample.Host, topNo));
            htaLine = htaLine.Replace(type, GetDbValue(sample.Type, topNo));
            htaLine = htaLine.Replace(geolocation, GetDbValue(sample.Geo_Location, topNo));
            htaLine = htaLine.Replace(collectiondate, GetDbValue(sample.Collection_Date, topNo));
            htaLine = htaLine.Replace(releasedate, GetDbValue(sample.Release_Date, topNo));
            htaLine = htaLine.Replace(length, GetDbValue(sample.Length, topNo));
            htaLine = htaLine.Replace(ratio, GetDbValue(sample.Cover_Ratio, topNo));
            htaLine = htaLine.Replace(ave, GetDbValue(sample.Cover_Ave, topNo));
            htaLine = htaLine.Replace(seq, GetDbValue(sample.Cns_Nucs, topNo));

            // file wite
            WfComponent.Utils.FileUtils.WriteFileFromString(outHtaPath,  htaLine,  ref message);

        }//



    }
}
