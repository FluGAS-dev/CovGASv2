using CovGASv2.Models;
using CovGASv2.Models.Properties;
using CovGASv2.Proc.Flow;
using Livet.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static FluGASv25.Utils.ConstantValues;
using static WfArrangement.Specific.NucVcf2AaVariant;

namespace CovGASv2.ViewModels
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {

        // butom button
        public ViewModelCommand OpenTreeCommand { protected set; get; }
        public ViewModelCommand OpenAlignmentCommand { protected set; get; }
        public ViewModelCommand OpenHtaCommand { protected set; get; }

        private ObservableCollection<SampleModel> _sampleTabs;
        public new ObservableCollection<SampleModel> SampleTabs
        {
            get { return _sampleTabs; }
            set { RaisePropertyChangedIfSet(ref _sampleTabs, value); }
        }

        private List<Sample> _sampleList;
        public new List<Sample> SampleList
        {
            get { return _sampleList; }
            set { RaisePropertyChangedIfSet(ref _sampleList, value); }
        }


        public void SetSampleResuts()
        {
            System.Diagnostics.Debug.WriteLine("Results List init...");
            SampleList = Dao.SampleDao.GetSamples().ToList();

            System.Diagnostics.Debug.WriteLine("sample count : " + _sampleList.Count());
        }

        private int _tabIndex = 0;
        public new int TabIndex
        {
            get { return _tabIndex; }
            set { RaisePropertyChangedIfSet(ref _tabIndex, value); }
        }
        private Sample _selectSample;
        public new Sample SelectSample
        {
            get { return _selectSample; }
            set
            {
                if (value == null || value.ID < 1) return;
                if (RaisePropertyChangedIfSet(ref _selectSample, value))
                {
                    // Listで選択されたサンプル情報を更新
                    this.sampleId = value.ID;
                    SampleName = value.ViewName;
                    ExecDate = value.Date;

                    // tab create
                    SetViewTabProperties();
                    // RaisePropertyChanged(nameof(TabIndex));
                    TabIndex = 0;  // TabItems が変更になったところで -1 が入る
                }
            }
        }

        private void SetViewTabProperties()
        {
            //　this.SampleTabs.Clear(); // 一旦全てクリア
            var selectSample = new ObservableCollection<SampleModel>();
            var accessions = _selectSample.Accession.Split(DbDelimiter);

            var resultscnt = 0;
            foreach (var accession in accessions)
            {
                if (string.IsNullOrEmpty(accession))  // Top1 ~ Top3 有るものだけ作る
                    continue;
                var topCnt = resultscnt + 1;
                selectSample.Add(new SampleModel()
                {
                    TabName = "Top" + topCnt,
                    Accession = accession,
                    Species = GetSamplePtopertyValue(nameof(_selectSample.Species), resultscnt),
                    Host = GetSamplePtopertyValue(nameof(_selectSample.Host), resultscnt),
                    Genbank_Title = GetSamplePtopertyValue(nameof(_selectSample.Genbank_Title), resultscnt),
                    Geo_Location = GetSamplePtopertyValue(nameof(_selectSample.Geo_Location), resultscnt),
                    Completeness = GetSamplePtopertyValue(nameof(_selectSample.Completeness), resultscnt),
                    Type = GetSamplePtopertyValue(nameof(_selectSample.Type), resultscnt),
                    Length = GetSamplePtopertyValue(nameof(_selectSample.Length), resultscnt),
                    Cover_Ratio = GetSamplePtopertyValue(nameof(_selectSample.Cover_Ratio), resultscnt),
                    Cover_Ave = GetSamplePtopertyValue(nameof(_selectSample.Cover_Ave), resultscnt),
                    Collection_Date = GetSamplePtopertyValue(nameof(_selectSample.Collection_Date), resultscnt),
                    Release_Date = GetSamplePtopertyValue(nameof(_selectSample.Release_Date), resultscnt),
                    Cns_Nucs = GetSamplePtopertyValue(nameof(_selectSample.Cns_Nucs), resultscnt),
                    Gisaid_Clade = GetSamplePtopertyValue(nameof(_selectSample.Gisaid_Clade), resultscnt),
                    Nextstrain_Clade = GetSamplePtopertyValue(nameof(_selectSample.Nextstrain_Clade), resultscnt),
                });
                resultscnt++;
            }
            SampleTabs = selectSample;  // 新しくセット
        }

        // gid から sequence.csv 情報が取れなかった時の対応
        private string GetSamplePtopertyValue(string name, int cnt)
        {
            // db->view カラムがないとException　DBの世代が違う。
            try { 
                var val = _selectSample.GetType().GetProperty(name).GetValue(_selectSample).ToString();
                if (val.Split(DbDelimiter).Count() >= cnt)
                    return val.Split(DbDelimiter)[cnt];
            } catch (Exception e)
            {
                mainLog.Report(e.Message);
                System.Diagnostics.Debug.WriteLine(e.Message);

            }
            return string.Empty;
        }


        private void OpenTree()
        {
            System.Diagnostics.Debug.WriteLine("call Alignment");
            if (_selectSample == null)
            {
                ShowErrorDialog("no seleced sample, Please select sample.");
                return;
            }

            var outName = _selectSample.ViewName;
            if (string.IsNullOrEmpty(outName))
                outName = "consensus.dnd";
            else
                outName += "-consensus.dnd";

            // save file
            var alignPath = SelectFileDialog("save tree ", false, false, outName);
            if (!alignPath.Any()) return; // Saveファイルの選択が無かった。

            var mes = string.Empty;
            Task.Run(() =>
            {
                Several.CreateTree.OutTree(alignPath.First(), sampleId, this._tabIndex, ref mes);
            });

            if (!string.IsNullOrEmpty(mes))
                ShowErrorDialog(mes);
        }

        private void OpenAlignment()
        {
            System.Diagnostics.Debug.WriteLine("call Alignment");
            if (_selectSample == null) {
                ShowErrorDialog("no seleced sample, Please select sample.");
                return;
            }

            var outName = _selectSample.ViewName;
            if (string.IsNullOrEmpty(outName))
                outName = "consensus.aglin";
            else
                outName += "-consensus.aglin";

            // save file
            var alignPath = SelectFileDialog("save aglinment ", false, false, outName);
            if (!alignPath.Any()) return; // Saveファイルの選択が無かった。

            var mes = string.Empty;
            Task.Run(() =>
            {
                Several.CreateAgliment.OutAgliment(alignPath.First(), sampleId, this._tabIndex, ref mes);
            });

            if (!string.IsNullOrEmpty(mes))
                ShowErrorDialog(mes);
        }

        private void OpenHta()
        {

            if (_selectSample == null)
            {
                ShowErrorDialog("no seleced sample, Please select sample.");
                return;
            }

            var  outName = _selectSample.ViewName;
            if (string.IsNullOrEmpty(outName))
                outName = "result.hta";
            else
                outName += "-result.hta";

            // save file
            var htaPath = SelectFileDialog("save result", false, false, outName);
            if (!htaPath.Any()) return; // Saveファイルの選択が無かった。



            var mes = string.Empty;
            Task.Run(() =>
            {
                Several.CreateReport.OutReport(htaPath.First(), this._selectSample.ID, this._tabIndex, ref mes);
            });

            if(!string.IsNullOrEmpty(mes))
                ShowErrorDialog(mes);
            else if (System.IO.File.Exists(htaPath.First()))
                System.Diagnostics.Process.Start(
                                                new System.Diagnostics.ProcessStartInfo(htaPath.First())
                                                { UseShellExecute = true });
        }

        // 1st-alignment vcf
        private void OpenVcfAa()
        {
            var bamFile = string.Empty;
            if(! GetSelectSampleOut1stBamPath(ref bamFile))
            {
                ShowErrorDialog(bamFile);
                return;
            }

            var aavcf = Path.ChangeExtension(bamFile, aaVcfFooter);
            OpenVcf(aavcf);  // Excel or NotePad
        }

        private void OpenVcfNuc()
        {
            var bamFile = string.Empty;
            if (!GetSelectSampleOut1stBamPath(ref bamFile))
            {
                ShowErrorDialog(bamFile);
                return;
            }

            var nucvcf = Path.ChangeExtension(bamFile, vcfFooter);
            OpenVcf(nucvcf);  // Excel or NotePad
        }

        private void OpenVcf(string vcfFilePath)
        {
            // vcf file 存在確認
            if (!File.Exists(vcfFilePath))
            {
                ShowErrorDialog("vcf-file is not found.");
                return;
            }

            // エクセルかメモ帳か
            var openApp = GetExcelPath();
            System.Diagnostics.Process.Start(
                                            new System.Diagnostics.ProcessStartInfo(openApp, vcfFilePath)
                                            { UseShellExecute = true });

        }

        private void OpenMappingView()
        {
            var bamFile = string.Empty;
            var referenceFile = string.Empty;
            if (!GetSelectSampleOut2ndBamPath(ref referenceFile, ref bamFile))
            {
                ShowErrorDialog( referenceFile + bamFile );
                return;
            }

            WfComponent.External.Tablet.TabletStart(string.Empty,
                                                                            referenceFile,
                                                                            bamFile);
        }

        private bool GetSelectSampleOut1stBamPath(ref string resPath)
        {
            if (_selectSample == null)
            {
                resPath = "no seleced sample, Please select sample.";
                return false;
            }
            var sampleOutDir = Path.GetDirectoryName(
                                            Path.GetDirectoryName(
                                                    _selectSample.MEMO));
            if (!Directory.Exists(sampleOutDir))
            {
                resPath = "not found results-out directory, removed directory? " + sampleOutDir;
                return false;
            }

            var bamFile = Path.Combine(
                            sampleOutDir,
                            mappingResultDirName,
                            fstMappingResultDirName,
                            CommonFlow.covBaseName + sortedBamFooter);
            if (!File.Exists(bamFile))
            {
                resPath = "not found result bam file, no-created sample? ";
                return false;
            }

            resPath = bamFile;
            return true;
        }

        private bool GetSelectSampleOut2ndBamPath(ref string referencePath, ref string bamPath)
        {
            if (_selectSample == null)
            {
                bamPath = "no seleced sample, Please select sample.";
                return false;
            }
            var sampleOutDir = Path.GetDirectoryName(
                                            Path.GetDirectoryName(
                                                    _selectSample.MEMO));
            if (!Directory.Exists(sampleOutDir))
            {
                bamPath = "not found results-out directory, removed directory? " + sampleOutDir;
                return false;
            }
            
            var bamFile = Path.Combine(
                            sampleOutDir,
                            mappingResultDirName,
                            secMappingResultDirName,
                            CommonFlow.covBaseName + secMappingFooter + 1 + sortedBamFooter);
            if (!File.Exists(bamFile))
            {
                bamPath = "not found result bam file, no-created sample? ";
                return false;
            }

            var referenceFile = Path.Combine(
                            sampleOutDir,
                            mappingResultDirName,
                            secMappingResultDirName,
                            secMappingReferenceDirName,
                            CommonFlow.covBaseName + FnaFooter);
            if (!File.Exists(referenceFile))
            {
                referencePath = "not found mappinf-reference file, no-created sample? ";
                return false;
            }

            referencePath = referenceFile;
            bamPath = bamFile;
            return true;
        }


        private string GetExcelPath()
        {
            // Programフォルダ内にある管理者権限フォルダがあるので回避するため
            var officeDirs = Directory.GetDirectories(
                                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                                    "Microsoft Office*");

            foreach(var officeDir in officeDirs)
            {
                var excelPath = WfComponent.Utils.FileUtils.FindFile(
                                             officeDir,
                                            "EXCEL.exe");

                if (excelPath.Any())
                    return excelPath.First();
            }

            return "notepad.exe"; // メモ帳はパスが切れている
        }

        private void OpenNcbi()
        {
            if (_selectSample == null)
            {
                ShowErrorDialog("no seleced sample, Please select sample.");
                return;
            }

            var dbAcc = _selectSample.Accession;
            if(string.IsNullOrEmpty(dbAcc))
            {
                ShowErrorDialog("not get results data, ");
                return;
            }

            var accs = dbAcc.Split(DbDelimiter);
            System.Diagnostics.Debug.WriteLine("open url acc " + accs[_tabIndex]);
            WfComponent.Utils.NcbiUtils.NcbiNcreotidePage(accs[_tabIndex]);
        }

    }
}
