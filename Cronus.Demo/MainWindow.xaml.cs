using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cronus.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ILogger _logger = LoggerFactory.Create(builder => builder.AddSerilog()).CreateLogger("Cronus");

        /// <summary>
        /// Cosntructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Window load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Window_Loaded");
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Button start click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                SendServer.Instance.APEventHandler += Instance_APEventHandler;
                SendServer.Instance.TaskEventHandler += Instance_TaskEventHandler;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "btnStart_Click");
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void Instance_TaskEventHandler(object? sender, Events.TaskResultEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Instance_TaskEventHandler");
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void Instance_APEventHandler(object? sender, Events.APStatusEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Instance_APEventHandler");
                MessageBox.Show(ex.Message, "Error");
            }
        }
    }
}
