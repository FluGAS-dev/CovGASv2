using System;
using System.Collections.Generic;
using System.Text;

namespace CovGASv2.ViewModels
{
    public class MinionParameterViewModel : FluGASv25.ViewModels.MinionParameterViewModel
    {
        // constractor
        public MinionParameterViewModel()
        {
            CommandInit();
        }

        public MinionParameterViewModel(string selectedParameterName = null)
        {
            this.CurrentParameterName = selectedParameterName;
            CommandInit();
        }


    }
}
