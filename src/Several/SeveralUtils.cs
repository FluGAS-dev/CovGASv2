using CovGASv2.Proc.Flow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WfComponent.Utils;
using static FluGASv25.Utils.ConstantValues;

namespace CovGASv2.Several
{
    public static class SeveralUtils
    {
        public static SequenceProperties GetNcbiGenbank(string gid, ref string message)
        {
            SequenceProperties ncbidat = null;
            try
            {
                ncbidat = WfComponent.Utils.Genbank.GetSequenceProperties(gid, ref message);
                if (!string.IsNullOrEmpty(message) ||
                     ncbidat == null ||
                     string.IsNullOrEmpty(ncbidat.accession))
                {
                    message += "get genbank sequence data error. ";
                    return ncbidat;  // genbank error 
                }
            }
            catch (Exception e)
            {
                message += e.Message;

            }
            return ncbidat;
        }

        // local の BLASTDB に対して Accession で検索します。
        public static KeyValuePair<string, string> GetCoronaReference(string accession, ref string message)
        {

            var localReference = Path.Combine(
                                            FluGASv25.Proc.Flow.CommonFlow.GetBlastReferenceDir,
                                            CommonFlow.covBaseName + FnaFooter);

            var fastaDic = Fasta.FastaFile2Dic(localReference);
            var targetFastas = fastaDic.Where(s => s.Key.Split(".").First() == accession);
            if (targetFastas.Any())
                return targetFastas.First(); // 正常取得

            // error...
            message += "not found accession, " + accession;
            return new KeyValuePair<string, string>(string.Empty, string.Empty);

        }

    }
}
