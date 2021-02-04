using CovGASv2.Proc.Flow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WfComponent.External;
using WfComponent.External.Properties;
using WfComponent.Utils;
using static FluGASv25.Dao.DbCommon;
using static FluGASv25.Utils.ConstantValues;


namespace CovGASv2.Several
{
    public static class CreateTree
    {
        public static readonly string TreeReferenceDir = "TreeReference";
        public static void OutTree(string outTreePath, int sampleId, int topNo, ref string message)
        {
            // 当該サンプル情報
            var mes = string.Empty;
            var sample = Dao.SampleDao.GetSample(sampleId, ref mes);
            if (!string.IsNullOrEmpty(mes) || sample == null || sample.ID <= 0)
            {
                message += mes;
                return;
            }

            var nucName = sample.ViewName;
            var nucs = GetDbValue(sample.Cns_Nucs, topNo);
            var referenceName = GetDbValue(sample.Accession, topNo);

            var reference = SeveralUtils.GetCoronaReference(referenceName, ref mes);
            if (!string.IsNullOrEmpty(mes) ||
                string.IsNullOrEmpty(reference.Key) ||
                string.IsNullOrEmpty(reference.Value))
            {
                message += mes + Environment.NewLine;
                message += "create tree process error...";
                return;
            }

            // for tree base fasta, to add reference , add consensus
            var baseTreeFasta = FileUtils.FindFile(
                                                         Path.Combine(
                                                             AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                                                             referencesDir,
                                                             TreeReferenceDir),
                                                         CommonFlow.covBaseName,
                                                         FnaFooter);
            var coronaRefseq = baseTreeFasta.Any() ?
                                            baseTreeFasta.First():
                                            Path.Combine(    // mapping reference
                                                            FluGASv25.Proc.Flow.CommonFlow.GetMappingReferenceDir,
                                                            CommonFlow.covBaseName + FnaFooter);
            


            var coronaRefseqLines = FileUtils.ReadFile(coronaRefseq,ref mes);
            var coronaReference = new List<string>(coronaRefseqLines);
            coronaReference.Add(">" + reference.Key);
            coronaReference.Add( reference.Value);
            coronaReference.Add(">" + nucName);
            coronaReference.Add(nucs);


            var tmpFasta = outTreePath.EndsWith(".fasta") ?
                            Path.ChangeExtension(outTreePath, ".fna") :
                            Path.ChangeExtension(outTreePath, ".fasta");

            if (File.Exists(tmpFasta)) File.Delete(tmpFasta);
            // 一時Fasta
            FileUtils.WriteFile(tmpFasta, coronaReference, ref message);
            if (!string.IsNullOrEmpty(message)) return;


            var clustalO = new ClustalOmega(
                                    new ClustalOmegaOptions()
                                    {
                                        targetFile = tmpFasta,
                                        outGuidTreeFile = outTreePath
                                    });

            try { 
                var s = clustalO.StartProcess();
                if (!string.IsNullOrEmpty(s))
                    message += s;

                if (string.IsNullOrEmpty(clustalO.Message))
                    message += clustalO.Message;
            }
            catch (Exception e)
            {
                message += e.Message;
            }

            return;
        }



    }
}
