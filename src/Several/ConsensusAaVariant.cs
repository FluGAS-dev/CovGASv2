using CovGASv2.Proc.Flow;
using System;
using System.Collections.Generic;
using System.Linq;
using WfComponent.Utils;

namespace CovGASv2.Several
{
    // Aa変異を取得する。PEND. 2020.12.15 使っていない
    public static class ConsensusAaVariant
    {
        // Aa変異をvcf形式で取得する
        public static (string vcfString, IEnumerable<VcfAnnotation> aaVariableList) GetAaVariation(IDictionary<string, string> fastaWithAnnotations, ref string message)
        {
            // var message = string.Empty;
            // vcf annotation strings, top3 consensus fasta.... 
            var resAaVcfList = new List<string>();
            var aaChangesList = new List<VcfAnnotation>();
            foreach (var seqName2annotNuc in fastaWithAnnotations)
            {
                // mapping reference name. -> Genbank Sequence 
                var referenceProperty = GetReferenceProperties(seqName2annotNuc.Key, ref message);
                if (referenceProperty == null)
                {
                    message += "error, ncbi genbank infomations is not found.";
                    resAaVcfList.Add(string.Empty);  //  Top1 取れなかった。エラーはmessageに格納
                    continue;
                }

                // amino properties 
                var aaChangeVcf = new List<string>();
                foreach (var aminoProp in referenceProperty.aminoProps)
                {
                    var aaChanges = GetAmminoChanges(
                                                aminoProp.AAseq,
                                                seqName2annotNuc.Value,
                                                aminoProp.SubLocations,
                                                ref message);
                    if (!aaChanges.Any())
                        continue;   // 対象のgene/CDS で変異が無い。
                    aaChanges.ForEach(a => a.referenceName = aminoProp.Name);

                    // DB格納文字列
                    aaChangeVcf.Add(aminoProp.Name + "[" +
                                    string.Join(",", aaChanges.Select(s => s.GetVcfString)) +
                                    "[");
                    // aaChangeVcf.Add( aminoProp.Name + "[" + string.Join(",",  aaChanges.g + "]");

                    // すべてのAa変異をTablerで保存
                    aaChangesList.AddRange(aaChanges);

                }
                resAaVcfList.Add(string.Join(",", aaChangeVcf));  // top1 分。

            }

            // 表形式でファイルへ書き込み対象

            return (CommonFlow.DbDefaultTop3(resAaVcfList),
                      aaChangesList);
        }

        //
        private static SequenceProperties GetReferenceProperties(string consensusName, ref string message)
        {

            // 
            // var consensusSequenceName = ">" + sampleName + "|" + subType + Mpileup2ConsensusFooter + cnsCnt +
            // " " + pileupItem.ReferenceName +
            // " cover=" + pileupItem.ReferenceMappingCoverageRate +
            // " depth=" + pileupItem.ReferenceMappingCoverageAvg;


            var seqNames = consensusName.Split(" ");
            if (seqNames.Length <= 1)
            {
                message += "Error, reference name is not found.  " + Environment.NewLine + consensusName;
                return null;   // FluGAS内ではありえない。
            }

            var refName = seqNames[1];  // pileupItem.ReferenceName
            if (!refName.StartsWith("NC"))
            {
                message += "caution,  reference is no NCBI-refseq. " + refName;
                // return null;  // mapping reference  CovGAS は Refseq を使用しているはず
                return GetGenbankInfo(refName, ref message); // 救済処置へ

            }
            var refseqAcc = refName.Split(".").First();
            var referenceFiles = FileUtils.FindFile(
                                                FluGASv25.Proc.Flow.CommonFlow.GetMappingReferenceDir,
                                                refseqAcc,
                                                Genbank.genbankFooter);

            if (!referenceFiles.Any())
            {
                message += "caution, not found refseq-genbank file. Please check in Data mapping-reference directory";
                // Genbank file ないので取得する
                return GetGenbankInfo(refseqAcc, ref message); // 救済措置

            }

            var gbSeqProperties = Genbank.GetInfluenzaProperties(referenceFiles.First(), ref message);
            if (!gbSeqProperties.Any())
            {
                message += "caution, reference sequence info is not found from genbank file." + referenceFiles.First();
                // return null;  // Genbank file から情報とれなかった。
                return GetGenbankInfo(refseqAcc, ref message); // 救済措置
            }

            return gbSeqProperties.First(); // 中務は一個だけ
        }

        // コンセンサスAa と、リファレンスAaの比較　　　　　
        private static List<VcfAnnotation> GetAmminoChanges(string referenceAa, string consensusNucs, IDictionary<int, int> startEndPos, ref string message)
        {
            // 保持List
            var aaChanges = new List<VcfAnnotation>();
            referenceAa = referenceAa.Replace("\"", "");

            // annotated consensus seq -> consensus Aa
            var cnsAas = new List<string>();
            foreach (var startEnd in startEndPos)  // join 情報があるので結合する
                cnsAas.AddRange(WfArrangement.Specific.AnnotatedConsensus2Aa.Fasta2Aa(consensusNucs, startEnd.Key, startEnd.Value));

            if (!cnsAas.Any())  // 当該箇所のコンセンサスAaが取れなかった。
            {
                message += "Error, varid consensus Aa ." + Environment.NewLine +
                                  startEndPos.ToString() + Environment.NewLine +
                                  consensusNucs;
                return aaChanges;
            }

            if (cnsAas.Count != referenceAa.Length + 1) // 終止コドン分 -1
                message += "Aa length to deffer...  cns:" + cnsAas.Count + "  ref:" + referenceAa.Length +
                                    "  start:" + string.Join(",", startEndPos.Keys) + " end:" + string.Join(",", startEndPos.Values) +
                                    Environment.NewLine;
            message += string.Join(Environment.NewLine, cnsAas) + Environment.NewLine;

            var aaPos = 0; // array は 0-orogin
            foreach (var cnsAa in cnsAas)
            {
                if (referenceAa.Length <= aaPos)
                    continue;  // リファレンスAaが短いとき。通常はあり得ない。

                var refAa = referenceAa[aaPos].ToString();  // reference Aa

                aaPos++;  // 表記のAa位置は1-origin
                if (!cnsAa.Equals(refAa))
                    aaChanges.Add(new VcfAnnotation()
                    {
                        referenceAa = refAa,
                        aaPosition = aaPos.ToString(),
                        consensusAa = cnsAa
                    });
            }

            return aaChanges;
        }

        // GenBank情報が取れない場合の救済処置。
        private static SequenceProperties GetGenbankInfo(string accession, ref string message)
        {
            // accession
            var searchAcc = accession.Split(".").First();  // MT259281.1 -> MT259281

            try
            {

                return Genbank.GetSequenceProperties(searchAcc, ref message);

            }
            catch (Exception e)  // net connection error とか。
            {
                message += e.StackTrace;
                message += e.Message;
            }

            return null;
        }


    }

    public class VcfAnnotation
    {
        public string referenceName { get; set; }
        public string referenceAa { get; set; }
        public string consensusAa { get; set; }
        public string aaPosition { get; set; }

        public string GetVcfString
            => referenceAa + aaPosition + consensusAa;

        public string GetTablerTable
            => string.Join("\t", new string[] { referenceName, aaPosition, referenceAa, consensusAa });
    }

}
