using System;
using System.Collections.Generic;
using System.Text;

namespace CovGASv2.ViewModels
{
    public class MiseqParameterViewModel : FluGASv25.ViewModels.MiseqParameterViewModel
    {

        // constractor
        public MiseqParameterViewModel()
        {
            System.Diagnostics.Debug.WriteLine(nameof(this.ToString));
            CommandInit();
        }

    }
}
