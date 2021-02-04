using CovGASv2.Models.Properties;
using Livet.Commands;
using System;

namespace CovGASv2.Models
{
    public class SampleModel : Sample
    {
     
        // Sample-tab name
        public string TabName { get; set; }
        public ViewModelCommand OpenNcbiCommand { get; set; }

        public SampleModel()
        {
            System.Diagnostics.Debug.WriteLine("init sample model " );
            this.OpenNcbiCommand = new ViewModelCommand(OpenNcbi);
        }

        public void OpenNcbi()
        {
            System.Diagnostics.Debug.WriteLine("open url acc " + this.Accession);
            WfComponent.Utils.NcbiUtils.NcbiNcreotidePage(this.Accession);
        }

    }
}
