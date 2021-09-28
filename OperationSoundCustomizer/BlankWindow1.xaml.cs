using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OperationSoundCustomizer {
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankWindow1 : Window {
        MainWindow main;
        bool isProcessing;
        bool isEnable;

        public BlankWindow1() {
            this.InitializeComponent();
            Title = "Operation sound customizer";
            Button_Click_Start(null, null);
        }


        private async void Button_Click_Start(object sender, RoutedEventArgs e) {
            if (isProcessing) {
                return;
            }
            isProcessing = true;
            StartStopButton.Content = "Wait...";

            if (main != null) {
                main.Dispose();
                main = null;
            }

            if (!isEnable) {
                try {
                    main = await MainWindow.CreateAsync();
                    StartStopButton.Content = "Stop";

                    isEnable = true;
                }
                catch (Exception error) {
                    StartStopButton.Content = "Error: " + error.Message;
                }
            }
            else {
                StartStopButton.Content = "Start";

                isEnable = false;
            }
            isProcessing = false;
        }

    }
}
