using System.IO;
using static FluGASv25.Utils.Approbate;

namespace CovGASv2.ViewModels
{
    /**
    public class InformationViewModel : FluGASv25.ViewModels.DialogViewModel
    {
        private bool isActivate = false;
        public bool IsLicenceActivate => isActivate;

        public void Initialize() // ContentRendered.
        {
            System.Diagnostics.Debug.WriteLine("CovGAS : information ViewModel Initialize ");

        }

        public void CallOpenPdf()
        {
            System.Diagnostics.Debug.WriteLine("CovGAS call open manual.");
            var pdf = Path.Combine(
                                    System.AppDomain.CurrentDomain.BaseDirectory,
                                    "data",
                                    "CovGASv2.pdf");
            if (File.Exists(pdf))
                OpenApp(pdf);
            else
                OpenUrl("https://www.w-fusion.co.jp/J/productlist/Flugasn.html");
        }
    
    }
    */
}
