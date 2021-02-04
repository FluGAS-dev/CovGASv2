using CovGASv2.Proc.Flow;
using System;
using System.IO;
using WfComponent.External;
using WfComponent.Utils;
using static FluGASv25.Dao.DbCommon;
using static FluGASv25.Utils.ConstantValues;

namespace CovGASv2.Several
{
    public static class CreateAgliment
    {

        public static void OutAgliment(string outAlignPath, int sampleId, int topNo, ref string message)
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

            // agliment作成と表示
            OutAgliment(referenceName, nucName, nucs, outAlignPath, ref message);

            var resTop = (topNo + 1).ToString();// tab が 0-orogin

            var bam = Path.Combine(
                                Path.GetDirectoryName(
                                    Path.GetDirectoryName(sample.MEMO)),
                                mappingResultDirName,
                                secMappingResultDirName,
                                CommonFlow.covBaseName + "-Top" + resTop + sortedBamFooter);

            var reference = Path.Combine(
                                        Path.GetDirectoryName(
                                            Path.GetDirectoryName(sample.MEMO)),
                                        mappingResultDirName,
                                        secMappingResultDirName,
                                        secMappingReferenceDirName,
                                        CommonFlow.covBaseName + "-Top" + resTop + FnaFooter);

            if(File.Exists(bam) && File.Exists(reference))
            {
                message =
                    Tablet.TabletStart(System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                                               reference,
                                               bam);
            }

            return;
        }

        /// <summary>
        /// NCBI Reference nuc 
        /// </summary>
        /// <param name="referenceName">NCBI Reference Accession </param>
        /// <param name="nucName">consensus name</param>
        /// <param name="nucs">consensus nucreotides</param>
        /// <param name="outAlignPath">output agliment file path</param>
        public static void OutAgliment(string referenceName, string nucName, string nucs, string outAlignPath, ref string message)
        {
            var mes = string.Empty;
            var reference = SeveralUtils.GetCoronaReference(referenceName, ref mes);
            if(!string.IsNullOrEmpty(mes) || 
                string.IsNullOrEmpty(reference.Key) ||
                string.IsNullOrEmpty(reference.Value))
            {
                message += mes + Environment.NewLine;
                message += "alignment process error...";
                return;
            }



            var tmpFasta = outAlignPath.EndsWith(".fasta") ?
                                        Path.ChangeExtension(outAlignPath, ".fna") :
                                        Path.ChangeExtension(outAlignPath, ".fasta");

            var fastaLines = new string[]
                                                {
                                                    ">" + reference.Key,
                                                    reference.Value,
                                                    ">" + nucName,
                                                    nucs
                                                };
            // 一時Fasta
            WfComponent.Utils.FileUtils.WriteFile(tmpFasta, fastaLines, ref message);
            if (!string.IsNullOrEmpty(message)) return;

            var alignProc = new WfComponent.External.Kalign(
                                    new WfComponent.External.KalignOptions()
                                    {
                                        fastaPath = tmpFasta,
                                        outAlign = outAlignPath,
                                        isFastaOut = true
                                    });

            // ほぼ一瞬で終わるはずなのでCancel 考えない。
            try
            {
                alignProc.StartProcess();
                message = alignProc.Message;

                if (File.Exists(outAlignPath))
                    message = AliView.AliViewStart(outAlignPath);

            }
            catch (Exception e)
            {
                message += e.Message;
                message += "execute agliment program error, " + alignProc.Message;
            }

            return;
        }


        /// <summary>
        /// NCBI Reference nuc 
        /// </summary>
        /// <param name="referenceName">NCBI Reference Accession </param>
        /// <param name="nucName">consensus name</param>
        /// <param name="nucs">consensus nucreotides</param>
        /// <param name="outAlignPath">output agliment file path</param>
        public static void OutAglimentNcbi(string referenceName, string nucName, string nucs, string outAlignPath, ref string message)
        {

            SequenceProperties ncbidat = null;
            try { 
                ncbidat = WfComponent.Utils.Genbank.GetSequenceProperties(referenceName, ref message);
                if (!string.IsNullOrEmpty(message) ||
                     ncbidat == null || 
                     string.IsNullOrEmpty(ncbidat.accession))
                {
                    message += "get genbank sequence data error. ";
                    return;  // genbank error 
                }
            } 
            catch (Exception e) 
            {
                message += e.Message;
                return;
            }

            var tmpFasta = outAlignPath.EndsWith(".fasta") ?
                                        Path.ChangeExtension(outAlignPath, ".fna") :
                                        Path.ChangeExtension(outAlignPath, ".fasta");

            var fastaLines = new string[]
                                                {
                                                    ">" + ncbidat.accession,
                                                    ncbidat.sequence,
                                                    ">" + nucName,
                                                    nucs
                                                };
            // 一時Fasta
            WfComponent.Utils.FileUtils.WriteFile(tmpFasta, fastaLines, ref message);
            if (!string.IsNullOrEmpty(message)) return;

            var alignProc = new WfComponent.External.Kalign(
                                    new WfComponent.External.KalignOptions()
                                    {
                                        fastaPath = tmpFasta,
                                        outAlign = outAlignPath,
                                        isFastaOut = true
                                    });

            // ほぼ一瞬で終わるはずなのでCancel 考えない。
            try { 
                alignProc.StartProcess();
                message = alignProc.Message;

                if (File.Exists(outAlignPath))
                    message = AliView.AliViewStart(outAlignPath);

            }
            catch (Exception e)
            {
                message += e.Message;
                message += "execute agliment program error, " + alignProc.Message;
            }

            return;
        }
    }
}
