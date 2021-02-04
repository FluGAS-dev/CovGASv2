
using CovGASv2.Models;
using CovGASv2.Models.Properties;
using CovGASv2.Proc.Flow;
using FluGASv25.Proc.Flow.Properties;
using FluGASv25.Proc.Process;
using FluGASv25.Utils;
using FluGASv25.ViewModels;
using FluGASv25.ViewModels.Base;
using Livet.Commands;
using Livet.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CovGASv2.ViewModels
{
    public partial class MainWindowViewModel : FluGASv25.ViewModels.MainWindowViewModel
    {
        public string Title => "CovGAS -Corona virus Genome Assembly and Typing-";

        public ViewModelCommand OpenVcfNucCommand { get; set; }
        public ViewModelCommand OpenVcfAaCommand { get; set; }
        public ViewModelCommand OpenMapViewCommand { get; set; }


        // constractor
        public MainWindowViewModel() :base()
        {
            // Analysisボタン
            this.AnarysisExecuteCommand = new ViewModelCommand(AnalysisExecute);
            this.ChangeViewNameCommand = new ViewModelCommand(ViewNameUpdate);
            // results view ボタン
            this.OpenHtaCommand = new ViewModelCommand(OpenHta);
            this.OpenTreeCommand = new ViewModelCommand(OpenTree);
            this.OpenAlignmentCommand = new ViewModelCommand(OpenAlignment);
            // ResultsView sample detail
            this._sampleTabs = new ObservableCollection<SampleModel>();

            this.SampleEditCommand = new ViewModelCommand(SampleEdit);
            this.SampleDeleteCommand = new ViewModelCommand(SampleDelete);

            this.OpenVcfNucCommand = new ViewModelCommand(OpenVcfNuc);
            this.OpenVcfAaCommand = new ViewModelCommand(OpenVcfAa);
            this.OpenMapViewCommand = new ViewModelCommand(OpenMappingView);

            // decoy
            this.SetDefaultReferenceParameterCommand = new ViewModelCommand(DummyCommand);
            SetSampleResuts();
        }

        public new void InitializeActivated()
        {
            base.InitializeActivated();
        }

        // Execute.
        protected new void AnalysisExecute()
        {
            System.Diagnostics.Debug.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);

            switch (this._analysisButton)
            {
                case buttonAnalysis:
                    Anarysis();
                    return;

                case buttonCancel:
                    ProcessCancel();
                    return;

                default:
                    ShowErrorDialog("Fatal error !!");
                    System.Windows.Application.Current.Shutdown();
                    return;
            }

        }

        private IFlow flow = null;
        protected void Anarysis()
        {
            if (!IsAnalysisOK()) return;   // 必要なものがない。

            // minion ?
            if (IsMinion && SetMinionFlow())
                CallFlow();

            // Miseq flow
            if (IsMiseq && SetMiseqFlow())
                CallFlow();
        }

        private void CallFlow()
        {

            // Select folder cancel.
            if (viewParameters == null) return;

            // flow properties が 正常　= ユーザが save Dir を指定した。
            if (flow != null)
            {
                AnalysisButton = buttonCancel;
                // タスクマネージャ起動
                System.Diagnostics.Process.Start(
                                                new System.Diagnostics.ProcessStartInfo("taskmgr")
                                                { UseShellExecute = true });

                _ = ProcessAsync(flow);
                // 
            }
        }

        // 各Process の 非同期処理
        private string ProcessResultMessage;
        protected async Task<string> ProcessAsync(IFlow flow)
        {
            // Log puts
            mainLog.Report(ConstantValues.MainLogClear);  // 処理の開始時に下部に表示されているログをクリア
            mainLog.Report("CovGAS version : " + Properties.Settings.Default.version);
            mainLog.Report("----- start analysis. -----");
            mainLog.Report(WfComponent.Utils.FileUtils.LogDateString());

            // 非同期
            ProcessResultMessage = await flow.CallFlowAsync().ConfigureAwait(true);
            mainLog.Report("Prosess return code:" +  ProcessResultMessage);
            ProcessEnd(ProcessResultMessage);

            return ProcessResultMessage;
        }

        // 通常は此処に戻る。
        internal void ProcessEnd(string resValue)
        {
            // 作業終了
            mainLog.Report(ConstantValues.EndAnalysis);

            var message = string.Empty;
            var logfile = WfComponent.Utils.FileUtils.GetUniqDateLogFile(ConstantValues.EndAnalysisLog);
            WfComponent.Utils.FileUtils.WriteFileFromString(logfile, LogMessage, ref message);
            if (!string.IsNullOrEmpty(message))
                mainLog.Report("error report , file write error. " + message);


            // 出力ディレクトリ先にも。
            if (Directory.Exists(userOutDir))
            {
                var outlog = Path.Combine(userOutDir,
                                    Path.GetFileName(logfile));
                WfComponent.Utils.FileUtils.WriteFileFromString(outlog, LogMessage, ref message);
            }
            if (!string.IsNullOrEmpty(message))
                mainLog.Report("error report , file write error. " + message);

            // 実行ボタンを戻す
            AnalysisButton = buttonAnalysis;

            // Results 再取得
            SetSampleResuts();
            this.SampleTabs.Clear(); // 一旦全てクリア

            // 終了ダイアログ。
            MessageBox.Show("Processing finished" + Environment.NewLine + resValue,
                            flow.GetType().ToString(),
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            // ダイアログ出す前に削除されていたらException
            if (Directory.Exists(userOutDir))
                System.Diagnostics.Process.Start(
                                new System.Diagnostics.ProcessStartInfo(userOutDir)
                                { UseShellExecute = true });
        }


        private AnalysisViewParameters viewParameters = null;
        private bool SetMinionFlow()
        {
            if (!isFast5Analysis()) return false;

            // 2020.01.20 混合は認めない。
            if (InputFolderFileCheck(SelectDataList)) return false;


            var analysisProperties = string.Empty;
            IEnumerable<Barcode2Name> barcodeList = null;
            if (SelectDataList.Where(s => Directory.Exists(s) || s.EndsWith(".fast5")).Any())
            {

                bool isBarcodeComit = false;
                using (var barcodeView = new BarcodeManagementViewModel(this.SelectDataList, this.IsOneSample, this.IsBarcode))
                {
                    Messenger.Raise(new TransitionMessage(barcodeView, "BarcodeManagementCommand"));
                    barcodeList = barcodeView.BarcodeList;  // user input sample-names
                    analysisProperties = barcodeView.SelectedConfig;
                    isBarcodeComit = barcodeView.IsCommand;
                };

                // barcode view で、ボタン押下以外でCloseした。
                if (!isBarcodeComit) return false;

            }
            viewParameters = GetFlowProperties(barcodeList, analysisProperties);
            if (viewParameters == null)
                return false;

            this.flow = new MinionFlow(viewParameters, mainLog);
            return true;
        }

        private bool SetMiseqFlow() 
        {
            viewParameters = GetFlowProperties();
            if (viewParameters == null)
                return false;

            this.flow = new MiseqFlow(viewParameters, mainLog);
            return true;
        }


        // 名前を付ける DB.Sample.name update
        private void ViewNameUpdate()
        {
            if (this._selectSample == null) return;


            var updId = this._selectSample.ID;
            var updateSample = new Sample()
            {
                ID = updId,
                Pram_ID = this._selectSample.Pram_ID,
                ViewName = this.SampleName
            };

            var resId = Dao.SampleDao.UpdateSample(updateSample);
            SetViewTabProperties();
            SelectSample = Dao.SampleDao.GetSamples().Where(s => s.ID == updId).First();
        }

        // FluGASにあってCovGASにナイコマンド実装
        private void DummyCommand()
        {
            System.Diagnostics.Debug.WriteLine("dummy comannd call " );
        }

        protected void SampleEdit()
        {
            var hideSampleList = SampleList.Where(s => !s.IsSelected).ToList();
            this.SampleList = hideSampleList;
            //this.SampleList.Remove(selectSample);

            SetDetailClear();
            // FooterMessage = "Select sample : " + this.SampleList.Count();
        }

        protected void SampleDelete()
        {
            string messageBoxText = "Are you sure you want to delete selected data?";
            string caption = "Delete sample";

            // Display message box
            if (ShowConfirmDialog(messageBoxText, caption))
            {

                var deleteSampleList = SampleList.Where(s => s.IsSelected)
                                                                 .Select(s => (long)s.ID)
                                                                 .ToArray();
                // Console.WriteLine("delete sample");

                // リスト更新(DBから更新すると、Hideのものが復活するのでViewの中で完結させる)
                var res = Dao.SampleDao.DeleteSample(deleteSampleList);
                SampleEdit();
                return;
            }
            // Console.WriteLine("Delete Cancel");
        }

        // 現在表示している値のクリア。
        private void SetDetailClear()
        {
            RaisePropertyChanged(nameof(SampleList));
        }
    }
}
