using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace Cronus.Demo
{
    /// <summary>
    /// Interaction logic for LedWindow.xaml
    /// </summary>
    public partial class LedWindow : Window
    {
        ILogger _logger;    // Logger

        /// <summary>
        /// Constructor
        /// </summary>
        public LedWindow(ILogger logger)
        {
            InitializeComponent();
            _logger = logger;
        }

        /// <summary>
        /// Button send click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtTimes.Text))
                {
                    MessageBox.Show("Flashing times is mandatory.");
                    return;
                }

                if (!Regex.IsMatch(txtTimes.Text, "^\\d{1,4}$"))
                {
                    MessageBox.Show("Invalid flashing times.");
                    return;
                }

                int.TryParse(txtTimes.Text, out int times);
                var result = SendServer.Instance.LED(chkRed.IsChecked ?? false, chkGreen.IsChecked ?? false, chkBlue.IsChecked ?? false, times);
                _logger.LogInformation($"LED Flashing:{result}.");
                Close();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Led_Send_Error");
                MessageBox.Show(ex.Message);
            }
        }
    }
}
