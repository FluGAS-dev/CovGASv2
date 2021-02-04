using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CovGASv2.Several
{

    public static class ReadCsvFile
    {
        /// <summary>
        /// CSVファイル読込
        /// </summary>
        /// <param name="filePath">CSVファイル</param>
        /// <returns>データリスト</returns>
        public static List<string[]> GatLines(string filePath, ref string message)
        {

            var resCvs = new List<string[]>();
            StreamReader sr = new StreamReader(filePath);
            try
            {
                while (sr.EndOfStream == false)
                {
                    string line = sr.ReadLine();

                    Regex reg = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                    resCvs.Add(reg.Split(line));
                    ;
                }
            }
            catch (Exception e)
            {
                message += "cvs read error, " + e.Message + Environment.NewLine ;
            }
            finally
            {
                sr.Close();
            }
            return resCvs;
        }
    }
}
