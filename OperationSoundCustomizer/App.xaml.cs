using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.IO;

namespace OperationSoundCustomizer {

    public partial class App : Application {
        public App() {
            this.InitializeComponent();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args) {
            try {
                blankWindow1 = new();
                blankWindow1.Activate();
            }
            catch (Exception e) {
                Debug.WriteLine(e.Message);
                File.AppendAllText(@"myerrorlog.txt", e.Message);
            }
            //main.Activate();
        }

        BlankWindow1 blankWindow1;
    }
}
