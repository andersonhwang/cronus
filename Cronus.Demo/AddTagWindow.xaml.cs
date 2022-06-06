using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Cronus.Demo
{
    /// <summary>
    /// Interaction logic for AddTagWindow.xaml
    /// </summary>
    public partial class AddTagWindow : Window
    {
        List<string> _tagIDList = new();

        public delegate void UpdateTagIDList(List<string> tagIDList);
        public event UpdateTagIDList UpdateTagIDListHandler;

        public AddTagWindow(List<string> list)
        {
            InitializeComponent();
            if (list != null) _tagIDList.AddRange(list);
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
                StringBuilder builder = new();
                foreach (var id in _tagIDList) builder.AppendLine(id);
                txtIDList.Text = builder.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Button save click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reg = new Regex("^[0-9A-F]{12}$");
                var hash = new HashSet<string>();
                var items = txtIDList.Text.Trim().ToUpper().Split('\r');
                foreach (var item in items)
                {
                    var id = item.Trim('\n');
                    if (!reg.IsMatch(id) || hash.Contains(id)) continue;
                    hash.Add(id);
                }
                UpdateTagIDListHandler?.Invoke(new List<string>(hash));
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtIDList_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
