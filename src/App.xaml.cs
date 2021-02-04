using Livet;
using System;
using System.Windows;

namespace CovGASv2
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            // ----- test  mode ----- 
            // Unit.AllTest.CallTest();
            // Unit.VariantAnnotationsTest.GisaidCladeTest();
            // Environment.Exit(1);
            // ----- test mode end ----- 

            DispatcherHelper.UIDispatcher = Dispatcher;
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            var message = string.Empty;
            if (!Dao.CreateSQL.DbCreate.CheckDb(ref message))
            {
                // DB Check 失敗
                MessageBox.Show("initialisation db check error. " + Environment.NewLine +
                                            message);
                Application.Current.Shutdown();
            }
        }

        // Application level error handling
        //private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    //TODO: Logging
        //    MessageBox.Show(
        //        "Something errors were occurred.",
        //        "Error",
        //        MessageBoxButton.OK,
        //        MessageBoxImage.Error);
        //
        //    Environment.Exit(1);
        //}
    }
}
