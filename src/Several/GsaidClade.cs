using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static FluGASv25.Utils.ConstantValues;
using static WfArrangement.Specific.NucVcf2AaVariant;
using static WfArrangement.Specific.PileupUtils;


namespace CovGASv2.Several
{
    public static class GsaidClade
    {
        public static string GetGisaidClade(string bamFilePath, ref string message)
        {
            // bam -> vcf 
            var nucvcf = Path.ChangeExtension(bamFilePath, vcfFooter);
            var aavcf = Path.ChangeExtension(bamFilePath, aaVcfFooter);

            // https://www.gisaid.org/references/statements-clarifications/clade-and-lineage-nomenclature-aids-in-genomic-epidemiology-of-active-hcov-19-viruses/
            // Clade で アミノ酸変異ないのはL だけ　Lは別に設けて
            if (!File.Exists(aavcf)) return string.Empty;

            var sampleClades = new List<Clade>();

            var nucVariants = GetVcfVariant(nucvcf, ref message);
            if (!string.IsNullOrEmpty(message))
                return string.Empty;

            var aaVariants = GetVcfVariant(aavcf, ref message, ":", 1, false);
            if (!string.IsNullOrEmpty(message))
                return string.Empty;


            var gisaidClades = GetGisaidClade();
            foreach (var clade in gisaidClades)
            {
                var isClade = true;
                foreach (var variant in clade.variants)
                {
                    if (variant.isNucreotide)
                    {
                        if (!IsVariantContain(variant, nucVariants))
                            isClade = false; // 一個でも
                    }
                    else
                    {
                        if (!IsVariantContain(variant, aaVariants))
                            isClade = false; // 一個でも
                    }
                }
                if (isClade)
                    sampleClades.Add(clade); // 全部そろっている

            }  // Clade end

            if (!sampleClades.Any())
                return string.Empty;

            // G は GH,GR に含まれるため表示しない
            if (sampleClades.Count(s => s.clage.StartsWith("G")) > 1)
                sampleClades = sampleClades.Where(s => s.clage != "G").ToList();
            // sampleClades.Remove(VariantAnnotations.CladeG());

            return string.Join(
                                 Environment.NewLine,
                               sampleClades.Select(s => s.clage + ":" + s.discription));

        }

        // https://www.gisaid.org/references/statements-clarifications/clade-and-lineage-nomenclature-aids-in-genomic-epidemiology-of-active-hcov-19-viruses/
        // この条件を満たす場合に文字列を返す。
        public static IEnumerable<Clade> GetGisaidClade()
        {
            return new List<Clade>()
            {
                CladeS(),
                CladeV(),
                CladeG(),
                CladeGH(),
                CladeGR(),
                CladeGV(),
            };
        }



        public static Clade CladeS() =>
                            new Clade() //S:    C8782T,T28144C includes NS8-L84S
                            {
                                clage = "S",
                                discription = "C8782T,T28144C includes NS8-L84S",  // ORF8 = NS8
                                variants = new List<Variant>()
                                {
                                    NucreotideNotation2Variant("C8782T"),
                                    NucreotideNotation2Variant("T28144C"),
                                    AminoacidNotation2Variant("ORF8-L84S"),
                                }
                            };

        public static Clade CladeL() =>   // CladeLは、この場所にRef塩基があれば良い。
                            new Clade() //L:    C241,C3037,A23403,C8782,G11083,G26144,T28144 
                            {
                                clage = "L",
                                discription = "C241,C3037,A23403,C8782,G11083,G26144,T28144 ",
                                variants = new List<Variant>()
                                {
                                    NucreotideNotation2Variant("C241C"),
                                    NucreotideNotation2Variant("C3037C"),
                                    NucreotideNotation2Variant("A23403A"),
                                    NucreotideNotation2Variant("C8782G"),
                                    NucreotideNotation2Variant("G11083G"),
                                    NucreotideNotation2Variant("G26144G"),
                                    NucreotideNotation2Variant("T28144T"),
                                }
                            };

        public static Clade CladeV() =>
                            new Clade() //V:    G11083T,G26144T NSP6-L37F + NS3-G251V
                            {
                                clage = "V",
                                discription = "G11083T,G26144T NSP6-L37F + NS3-G251V",
                                variants = new List<Variant>()
                                {
                                    NucreotideNotation2Variant("G11083T"),
                                    NucreotideNotation2Variant("G26144T"),
                                    // AminoacidNotation2Variant("NSP6-L37F"),     // NSP6 -> ORF1ab + 3559
                                    AminoacidNotation2Variant("ORF1ab-L3594F"),  // 
                                    AminoacidNotation2Variant("ORF3a-G251V"),// NS3 -> ORF3a
                                }
                            };

        public static Clade CladeG() =>
                            new Clade() //G:    C241T,C3037T,A23403G includes S-D614G
                            {
                                clage = "G",
                                discription = "C241T,C3037T,A23403G includes S-D614G",
                                variants = new List<Variant>()
                                {
                                    NucreotideNotation2Variant("C241T"),
                                    NucreotideNotation2Variant("C3037T"),
                                    NucreotideNotation2Variant("A23403G"),
                                    AminoacidNotation2Variant("S-D614G"),
                                }
                            };

        public static Clade CladeGH() =>
                            new Clade() //GH:  C241T,C3037T,A23403G,G25563T includes S-D614G + NS3-Q57H
                            {
                                clage = "GH",
                                discription = "C241T,C3037T,A23403G,G25563T includes S-D614G + NS3-Q57H",
                                variants = new List<Variant>()
                                {
                                    NucreotideNotation2Variant("C241T"),
                                    NucreotideNotation2Variant("C3037T"),
                                    NucreotideNotation2Variant("A23403G"),
                                        NucreotideNotation2Variant("G25563T"),
                                    AminoacidNotation2Variant("S-D614G"),
                                    AminoacidNotation2Variant("ORF3a-Q57H"),  // NS3 -> ORF3a
                                }
                            };

        public static Clade CladeGR() =>
                            new Clade() //GR:  C241T,C3037T,A23403G,G28882A includes S-D614G + N-G204R
                            {
                                clage = "GR",
                                discription = "C241T,C3037T,A23403G,G28882A includes S-D614G + N-G204R",
                                variants = new List<Variant>()
                                {
                                    NucreotideNotation2Variant("C241T"),
                                    NucreotideNotation2Variant("C3037T"),
                                    NucreotideNotation2Variant("A23403G"),
                                    NucreotideNotation2Variant("G28882A"),
                                    AminoacidNotation2Variant("S-D614G"),
                                    AminoacidNotation2Variant("N-G204R"),
                                }
                            };

        public static Clade CladeGV() =>
                            new Clade() // GV:  C241T,C3037T,A23403G,C22227T includes S-D614G + S-A222V
                            {
                                clage = "GV",
                                discription = "C241T,C3037T,A23403G,C22227T includes S-D614G + S-A222V",
                                variants = new List<Variant>()
                                {
                                    NucreotideNotation2Variant("C241T"),
                                    NucreotideNotation2Variant("C3037T"),
                                    NucreotideNotation2Variant("A23403G"),
                                    NucreotideNotation2Variant("C22227T"),
                                    AminoacidNotation2Variant("S-D614G"),
                                    AminoacidNotation2Variant("S-A222V"),
                                }
                            };

        public static string vcfnotationRegex = @"(?<reference>[a-zA-Z]+)(?<position>\d+)(?<alternate>[a-zA-Z]+)";
        public static Variant NucreotideNotation2Variant(string vcfnotation)
        {
            var variant = Regex.Match(vcfnotation, vcfnotationRegex, RegexOptions.IgnoreCase);

            if (!variant.Success || variant == null)
                return null;

            var pos = 0;
            if (!int.TryParse(variant.Groups["position"].Value, out pos))
                return null;

            return new Variant()
            {
                isNucreotide = true,
                gene = string.Empty,
                reference = variant.Groups["reference"].Value,
                alternate = variant.Groups["alternate"].Value,
                position = pos,
            };
        }

        // aminoacid change  NS3-G251V とか
        public static Variant AminoacidNotation2Variant(string vcfnotation)
        {
            var notations = vcfnotation.Split('-');
            if (notations.Length < 2)
                return null;

            // 変化パターン文字列は同じ形式。
            var variant = NucreotideNotation2Variant(notations.Last());
            variant.isNucreotide = false;
            variant.gene = notations.First();
            return variant;
        }

        // vcf -> Variants-object List
        public static IEnumerable<Variant> GetVcfVariant(string vcfFile, ref string message, string referenceDelimiter = ":", int referenceDelimiterIndex = 0, bool isNucleotide = true)
        {

            var vcfVariants = new List<Variant>();

            // var message = string.Empty;
            var vcflines = WfComponent.Utils.FileUtils.ReadFile(vcfFile, ref message);
            if (!string.IsNullOrEmpty(message))
                return vcfVariants;     // empty list...

            foreach (var vcfline in vcflines)
            {
                if (vcfline.StartsWith("#")) continue;  // vcf comment line
                var vcfitems = vcfline.Split(vcfDelimiter);

                var pos = 0;
                if (!int.TryParse(vcfitems.ElementAt(vcfPositionIdx), out pos))
                    continue;   // position が int 変換出来なかった

                var gene = isNucleotide ?
                                    string.Empty :
                                    vcfitems.ElementAt(vcfChromIdx)
                                                    .Split(referenceDelimiter)
                                                    .ElementAt(referenceDelimiterIndex);

                vcfVariants.Add(new Variant()
                {
                    isNucreotide = isNucleotide,
                    gene = gene,
                    reference = vcfitems.ElementAt(vcfReferenceIdx),
                    alternate = vcfitems.ElementAt(vcfAltIdx),
                    position = pos
                });
            }

            return vcfVariants;
        }

        // target が含まれているか
        public static bool IsVariantContain(Variant targetVariant, IEnumerable<Variant> vcfVariants)
        {
            var inc = vcfVariants.Where(s => s.alternate == targetVariant.alternate
                                                         && s.gene == targetVariant.gene
                                                         && s.position == targetVariant.position
                                                         && s.reference == targetVariant.reference);
            return inc.Any();
        }

        // CladeLは変異個所ではなく、Genomeの位置と塩基をみる
        public static bool IsGisaidCladeL(string fstAnnotatedConsensus)
        {
            // Annotation の削除。
            // コンセンサス配列
            var consNuc = fstAnnotatedConsensus
                                    .Replace(DelAnnotation, string.Empty)
                                    .Replace(SnpAnnotation, string.Empty)
                                    .Replace(InsStartAnnotation, string.Empty)
                                    .Replace(InsEndAnnotation, string.Empty);

            var isCladeLRequirement = true;
            foreach (var cladeL in CladeL().variants)
            {
                var consensusPositionNuc = consNuc.ElementAt(cladeL.position + 1); // crade is 1-orign
                if (consensusPositionNuc.ToString().ToUpper() != cladeL.reference)
                    isCladeLRequirement = false;
            }

            return isCladeLRequirement;
        }
    }

    public class Clade
    {
        public string clage { get; set; }
        public string discription { get; set; }
        public int clageRank { get; set; }

        public string GetCladeDiscription =>
                    clage + ":" + discription;

        public IEnumerable<Variant> variants { get; set; }
        public IEnumerable<Variant> requiredVariants { get; set; }

    }

    public class Variant
    {
        public bool isNucreotide { get; set; } = true; // default .
        public string gene { get; set; } = string.Empty;  // 

        public string reference { get; set; }
        public string alternate { get; set; }
        public int position { get; set; }

    }

}

