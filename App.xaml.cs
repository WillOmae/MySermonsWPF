using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MySermonsWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTU5Njc0QDMxMzcyZTMzMmUzMG5mVjNXeldUWmp4MjFaaHZnZitXU25za2hPSHR2YW5yWGFYbVF2M1FaREk9");
        }
    }
}
