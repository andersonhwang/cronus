using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cronus.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        object _locker = new();     // The locker
        ILogger _logger = LoggerFactory.Create(builder => builder.AddSerilog()).CreateLogger("Cronus");
        ObservableCollection<APInfor> ObcAPInfors = new();      // Observable collection of AP infor
        ObservableCollection<TagInfor> ObcTagInfors = new();    // Observable collection of tag infor
        CronusConfig _config = new() { APPort = 1234, DefaultStoreCode = "0001", OneStoreModel = true };

        /// <summary>
        /// Cosntructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Events Handler
        /// <summary>
        /// Window load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                lvAPs.ItemsSource = ObcAPInfors;
                dgTags.ItemsSource = ObcTagInfors;
                cobPage.SelectedIndex = 0;
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
                if (!int.TryParse(txtPort.Text, out int port) || port < 1000 || port > 0xFFFF)
                {
                    MessageBox.Show("Invalid AP port:" + txtPort.Text, "Error");
                    return;
                }

                _config.APPort = port;
                SendServer.Instance.APEventHandler += Instance_APEventHandler;
                SendServer.Instance.TaskEventHandler += Instance_TaskEventHandler;
                var result = SendServer.Instance.Start(_config, _logger);
                _logger.LogInformation("Try to start send server: " + result);
                if (result != Enum.Result.OK)
                {
                    MessageBox.Show("Start failed:" + result);
                }
                else
                {
                    btnStart.IsEnabled = false;
                    btnStart.Content = "Running...";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "btnStart_Click");
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Broadcast: switch page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSwitch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = SendServer.Instance.SwitchPageAll(_config.DefaultStoreCode, cobPage.SelectedIndex);
                if (result == Enum.Result.NoApOnline)
                {
                    MessageBox.Show("There is no AP online, please try again later.");
                    return;
                }
                else if (result == Enum.Result.APBusying)
                {
                    MessageBox.Show("AP is busying, please try again later.");
                    return;
                }
                else if (result == Enum.Result.OK)
                {
                    MessageBox.Show($"Switch page to #{cobPage.SelectedIndex}: OK", "Broadcast");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Switch_Page");
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Broadcast: display barcode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBarcode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = SendServer.Instance.DisplayBarcodeAll(_config.DefaultStoreCode);
                if (result == Enum.Result.NoApOnline)
                {
                    MessageBox.Show("There is no AP online, please try again later.");
                    return;
                }
                else if (result == Enum.Result.APBusying)
                {
                    MessageBox.Show("AP is busying, please try again later.");
                    return;
                }
                else if (result == Enum.Result.OK)
                {
                    MessageBox.Show("Display barcode: OK", "Broadcast");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Display_Barcode");
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void btnLED_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ObcTagInfors.Count == 0)
                {
                    MessageBox.Show("There is no tag to flashing LED light!");
                    return;
                }
                var ids = new List<string>();
                if (dgTags.SelectedItems.Count == 0)
                {
                    ids.AddRange(ObcTagInfors.Select(x => x.TagID).ToList());
                }
                else
                {
                    foreach (var item in dgTags.SelectedItems)
                    {
                        var tag = item as TagInfor;
                        ids.Add(tag.TagID);
                    }
                }
                LedWindow led = new LedWindow(ids, _logger);
                led.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Flashing_LED");
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void btnImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ObcTagInfors.Count == 0)
                {
                    MessageBox.Show("There is no tag to send image!");
                    return;
                }

                var ids = new List<string>();
                if (dgTags.SelectedItems.Count == 0)
                {
                    ids.AddRange(ObcTagInfors.Select(x => x.TagID).ToList());
                }
                else
                {
                    foreach (var item in dgTags.SelectedItems)
                    {
                        var tag = item as TagInfor;
                        ids.Add(tag.TagID);
                    }
                }
                ImageWindow image = new(ids, _logger);
                image.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send_Image");
                MessageBox.Show(ex.Message, "Error");
            }
        }

        #region Data grid menu events
        /// <summary>
        /// Menu: Add tag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miAddTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddTagWindow add = new AddTagWindow(ObcTagInfors.Select(x => x.TagID).ToList());
                add.UpdateTagIDListHandler += Add_UpdateTagIDListHandler;
                add.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Add_Tag");
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Menu: remove tag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTags.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please the tags which you want to remove.");
                    return;
                }
                if (MessageBox.Show("Are you sure to remove these tags?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                var needRemove = new List<TagInfor>();
                foreach (var item in dgTags.SelectedItems)
                {
                    var tag = item as TagInfor;
                    if (tag != null) needRemove.Add(tag);
                }
                if (needRemove.Count > 0)
                {
                    needRemove.ForEach(x => ObcTagInfors.Remove(x));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Add_Tag");
                MessageBox.Show(ex.Message, "Error");
            }
        }
        #endregion

        #region Cronus events
        /// <summary>
        /// Task event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_TaskEventHandler(object? sender, Events.TaskResultEventArgs e)
        {
            try
            {
                foreach (var item in e.TaskResults)
                {

                    var tag = ObcTagInfors.FirstOrDefault(x => x.TagID == item.TagID);
                    if (tag is null) continue;
                    tag.Token = item.Token;             // Token, please using TaskID(GUID) in your project
                    tag.RFPower = item.RfPower;         // RF power, -256 means empty
                    tag.Battery = item.Battery;
                    tag.Temperature = item.Temperature; // Temperature
                    tag.Status = item.Status;           // Tag status
                    tag.LastSend = item.LastSendTime;   // Last send time
                    tag.LastRecv = item.LastRecvTime;   // Last receive time
                    tag.LastAP = item.RouteRecord.Count > 0 ? item.RouteRecord[0] : ""; // Last AP ID
                    if (tag.Status == Enum.TaskStatus.Sending) tag.SendCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Instance_TaskEventHandler");
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// AP event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_APEventHandler(object? sender, Events.APStatusEventArgs e)
        {
            try
            {
                lock (_locker)
                {
                    var add = !ObcAPInfors.Any(x => x.StoreCode == e.StoreCode && x.APID == e.APID);
                    var ap = add
                        ? new APInfor { StoreCode = e.StoreCode, APID = e.APID, APStatus = e.Status }
                        : ObcAPInfors.First(x => x.StoreCode == e.StoreCode && x.APID == e.APID);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (add)
                        {
                            ObcAPInfors.Add(ap);
                        }
                        else
                        {
                            ap.APStatus = e.Status;
                        }
                        ObcAPInfors.Sort(); // Sort for better view
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Instance_APEventHandler");
                MessageBox.Show(ex.Message, "Error");
            }
        }
        #endregion

        #endregion

        #region Prviate Methods
        /// <summary>
        /// Update tag ID list
        /// </summary>
        /// <param name="tagIDList">Tag ID list</param>
        /// <exception cref="NotImplementedException"></exception>
        private void Add_UpdateTagIDListHandler(List<string> tagIDList)
        {
            ObcTagInfors.Clear();
            tagIDList.ForEach(x => ObcTagInfors.Add(new TagInfor { TagID = x, TagType = x[..2] }));
        }
        #endregion
    }
}
