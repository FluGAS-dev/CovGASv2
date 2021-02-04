using CovGASv2.Proc.Flow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using static CovGASv2.Several.GsaidClade;
using static FluGASv25.Utils.ConstantValues;

namespace CovGASv2.Several
{
    public class NextstrainClade
    {
        // https://github.com/nextstrain/ncov

        public static readonly string nextstrainCladeTsvUrl 
            = "https://raw.githubusercontent.com/nextstrain/ncov/master/defaults/clades.tsv";

        public static readonly string nextstrainCladeTsvFile = "clades.tsv";
        public static readonly char tsvDelimiter = '\t';
        public static readonly string nextstrainCladeTsv =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                                   "data",
                                   nextstrainCladeTsvFile);
        public static string CovReferenceGid = "NC_045512";  // 取り敢えず固定。

        public static string GetNextstrainClade(string bamFilePath, ref string message)
        {
            // ncbi refseq 
            var refseqNucs = GetRefseqNuc();

            // bam -> vcf 
            var nucVcfFile = Path.ChangeExtension(bamFilePath, vcfFooter);
            var nucVariants = GetVcfVariant(nucVcfFile, ref message);
            var nextstrainClade = GetNextstrainClade(ref message);

            var cladeGroup = nextstrainClade.OrderByDescending(s => s.clageRank)
                                                              .GroupBy(s => s.clageRank);

            // Rankの高い方から順番。　高い=新しい  Cladeが見つかれば、そのclade が返る
            foreach (var clades in cladeGroup)
            {
                // System.Diagnostics.Debug.WriteLine(clades.Key);
                var desc =  GetNextstranGroupClade(clades, nucVariants, refseqNucs); 

                if (!string.IsNullOrWhiteSpace(desc))
                    return desc;
            }

            // 見つからない。Nextstrain は無し。
            return string.Empty;

        }

        public static IEnumerable<Clade> GetNextstrainClade(ref string message)
        {
            // base clade = NC_045512
            var nextstrainClades = new List<Clade>();

            // tsv-text read -> clade class
            foreach(var cladeGroup in GetCladeTsv2Group(ref message))
            {
                if (cladeGroup.Key.StartsWith("clade")) continue;   // header

                var description = NextstrainDiscprition().ContainsKey(cladeGroup.Key) ?
                            NextstrainDiscprition()[cladeGroup.Key] :
                            baseNextstrainDiscription;

                int rank = NextstrainCladeRank().ContainsKey(cladeGroup.Key) ?
                                            NextstrainCladeRank()[cladeGroup.Key] :
                                            baseNextstrainCladeRank;

                var variants = new List<Variant>();
                foreach (var clade in cladeGroup)  // 1-clad　= 数行分
                {
                    var altpos = 0;
                    if(int.TryParse(clade.ElementAt(tsvSiteClm), out altpos))
                        variants.Add(
                            NucreotideNotation2Variant(clade));
                }

                if (variants.Any())
                    nextstrainClades.Add(
                        new Clade()
                        {
                            clage = cladeGroup.Key,
                            discription = description,
                            clageRank = rank,
                            variants = variants
                        });

            }

            return nextstrainClades;
        }

        public static string GetNextstranGroupClade(IEnumerable<Clade> nexestrainClades, IEnumerable<Variant> vcfVariants, string refseqNucs)
        {
            var applyclade = new List<string>();
            foreach (var clade in nexestrainClades)
            {
                var isClade = true;
                foreach (var cladeVariant in clade.variants)
                    if (!IsVariantContainNoReference(cladeVariant, vcfVariants, refseqNucs))
                        isClade = false;

                if (isClade)
                    applyclade.Add(clade.clage +":" +  clade.discription);
            }

            return string.Join(Environment.NewLine, applyclade);
        }

        // target が含まれているか
        public static bool IsVariantContainNoReference(Variant nextstrainVariant, IEnumerable<Variant> vcfVariants, string refseqNucs)
        {
            var inc = vcfVariants.Where(s => s.alternate == nextstrainVariant.alternate
                                                         && s.gene == nextstrainVariant.gene
                                                         && s.position == nextstrainVariant.position
                                                         ); // リファレンスはない。
            if (inc.Any()) return true;  // variant が存在する

            // 救済措置　Nextstrain Clade は Wuhan-Hu-1, の塩基でも指定されている（Wuhan-Hu-1,より前の株が基準?）
            if (refseqNucs.Length < nextstrainVariant.position)
                return false;     // position が refseq 範囲外

            var refseqNuc = refseqNucs.ElementAt(nextstrainVariant.position - 1); // 0-origin

            // debug // 
            System.Diagnostics.Debug.WriteLine("refseq pos=" + nextstrainVariant.position + " nuc =" + refseqNuc);

            if (refseqNuc.ToString().Equals(nextstrainVariant.alternate))
                return true;

            return false;
        }

        // tsv-text  -> group
        public static IEnumerable<IGrouping<string, string[]>> GetCladeTsv2Group(ref string message)
        {
            // file check.
            if (!File.Exists(nextstrainCladeTsv))
            {
                if (!Directory.Exists(Path.GetDirectoryName(nextstrainCladeTsv)))
                    Directory.CreateDirectory(Path.GetDirectoryName(nextstrainCladeTsv));
                using (var downloadClient = new WebClient())
                {
                    downloadClient.Encoding = System.Text.Encoding.UTF8;
                    var downloadUri = new Uri(nextstrainCladeTsvUrl);
                    downloadClient.DownloadFile(downloadUri, nextstrainCladeTsv);   // 非同期にしない。
                }
            }

            // 
            var tsvLine = WfComponent.Utils.FileUtils.ReadFile(nextstrainCladeTsv, ref message);
            if (!string.IsNullOrEmpty(message))
                return null;  // error...

            var tsv = tsvLine.Where(s => !string.IsNullOrEmpty(s.Trim()))
                                    .Select(s => s.Split(tsvDelimiter))
                                    .GroupBy(s => s.First());

            return tsv;
        }

        // ncbi reference nuc を取得する // Mapping Reference に入って居る（固定）
        public static string GetRefseqNuc()
        {
            // NCBI Refseq nuc
            var fastaPath = Path.Combine(
                                    FluGASv25.Proc.Flow.CommonFlow.GetMappingReferenceDir,
                                    CommonFlow.covBaseName + FnaFooter);

            var references = WfComponent.Utils.Fasta.FastaFile2Dic(fastaPath);  // 無ければ空で返ってくる
            var refseq = references.Where(s => s.Key.StartsWith(CovReferenceGid));
            if (refseq.Any())
                return refseq.First().Value;

            return string.Empty;

        }

        public static string baseNextstrainDiscription = "divergent mutation";
        public static IDictionary<string, string> NextstrainDiscprition()
        {
            return
                new Dictionary<string, string>()
                {
                    {"19A",  "basal pandemic mutation 19A" },
                    {"19B" , "basal pandemic mutation 19B"},
                    {"20A", "basal pandemic lineage bearing S 614G that’s globally distributed" },
                    {"20B", "derived from 20A bearing N 203K, N204R and ORF14 50N, also globally distributed" },
                    {"20C", "derived from 20A bearing ORF3a 57H and ORF1a 265I, also globally distributed" },
                    {"20D", "derived from 20B bearing ORF1a 1246I and ORF1a 3278S, concentrated in South America, southern Europe and South Africa" },
                    {"20E", "derived from 20A bearing N 220V, ORF10 30L, ORF14 67F and S 222V, concentrated in Europe" },
                    {"20F", "derived from 20B bearing ORF1a 300F and S 477N, concentrated in Australia" },
                    {"20G", "derived from 20C bearing ORF1b 1653D, ORF3a 172V, N 67S and N 199L, concentrated in the United States" },
                    {"20H/501Y.V2", "derived from 20C bearing S 80A, S 215G, S 484K, S 501Y, S 701V, concentrated in South Africa" },
                    {"20I/501Y.V1", "derived from 20B bearing S 501Y, S 570D, S 681H, ORF8 27*, concentrated in the United Kingdom" },
                    {"20J/501Y.V3", " contains spike mutations L18F, T20N, P26S, D138Y, R190S, K417T, E484K, N501Y, H655Y and T1027I " },
                };
        }

        public static readonly int baseNextstrainCladeRank = 0;
        public static IDictionary<string, int> NextstrainCladeRank()
        {
            return
                new Dictionary<string, int>()
                {
                    {"19A", 0 },
                    {"19B", 0 },
                    {"20A", 1 },
                    {"20B", 2},
                    {"20C", 2},
                    {"20D", 3},
                    {"20E", 3},
                    {"20F", 3},
                    {"20G", 3},
                    {"20H/501Y.V2",3},
                    {"20I/501Y.V1", 3},
                    {"20J/501Y.V3", 3},

                };
        }

        public static int cladeRank0 = 0;
        public static Clade Clade19A =>
            new Clade()
            {
                clage = "19A",
                discription = "basal pandemic mutation 19A",
                clageRank = cladeRank0,
                variants = new List<Variant>()
                {
                            NucreotideNotation2Variant( new string[]{"19A", "nuc", "8782", "C"}),
                            NucreotideNotation2Variant( new string[]{"19A", "nuc", "14408", "C"}),
                }
            };

        public static Clade Clade19B =>
            new Clade()
            {
                clage = "19B",
                discription = "basal pandemic mutation 19B",
                clageRank = cladeRank0,
                variants = new List<Variant>()
                {
                            NucreotideNotation2Variant( new string[]{"19B", "nuc", "8782", "T"}),
                            NucreotideNotation2Variant( new string[]{"19B", "nuc", "28144", "C"}),
                }
            };

        public static readonly int tsvCladeClm = 0;
        public static readonly int tsvGeneClm = 1;
        public static readonly int tsvSiteClm = 2;   // alt-position
        public static readonly int tsvAltClm = 3;     // alt-nuc
        public static Variant NucreotideNotation2Variant(string[] tsvLine, int cladeRank = 0)
        {

            var altpos = 0;
            int.TryParse(tsvLine.ElementAt(tsvSiteClm), out altpos);
            return
                new Variant()
                {
                    isNucreotide = true,
                    gene = string.Empty,
                    reference = string.Empty,
                    alternate = tsvLine.ElementAt(tsvAltClm),
                    position = altpos,
                };

        }

    }
}
