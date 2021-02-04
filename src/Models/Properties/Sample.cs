using System;
using System.Linq;

namespace CovGASv2.Models.Properties
{
    public class Sample
    {
        public Int32 ID { get; set; }
        public string Name { get; set; }
        public string ViewName { get; set; }
        public string File1 { get; set; }
        public string File2 { get; set; }
        public string Cover_Ratio { get; set; }
        public string Cover_Ave { get; set; }
        public string Cns_AaVariation { get; set; }
        public string Cns_Nucs { get; set; }
        public string Accession { get; set; }
        public string Release_Date { get; set; }
        public string Species { get; set; }
        public string Length { get; set; }
        public string Type { get; set; }
        public string Completeness { get; set; }
        public string Geo_Location { get; set; }
        public string Host { get; set; }
        public string Collection_Date { get; set; }
        public string Genbank_Title { get; set; }
        public string Gisaid_Clade { get; set; }
        public string Nextstrain_Clade { get; set; }
        public int Is_Delete { get; set; }
        public int Pram_ID { get; set; }
        public string Date { get; set; }
        public string Date_Only
        {
            get
            {
                var date = (string.IsNullOrEmpty(Date)) ? string.Empty : Date.Split(' ').First();
                return date;
            }
            set { }
        }
        public bool IsSelected { get; set; }
        public string MEMO { get; set; }
    }

}
