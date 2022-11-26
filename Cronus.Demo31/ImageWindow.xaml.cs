using Cronos.SDK.Enum;
using Cronus.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Drawing2D = System.Drawing.Drawing2D;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cronus.Demo
{
    /// <summary>
    /// Interaction logic for ImageWindow.xaml
    /// </summary>
    public partial class ImageWindow : Window
    {
        ILogger _logger;        // Logger
        List<string> TagIDs;    // Tag ID list
        BitmapImage _img;
        BitmapImage _imgB;
        BitmapImage _imgBR;
        bool _update = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tagID"></param>
        /// <param name="logger"></param>
        public ImageWindow(List<string> tagID, ILogger logger)
        {
            InitializeComponent();
            TagIDs = tagID;
            _logger = logger;
            Title = $"Send a image({TagIDs.Count})";
        }

        /// <summary>
        /// Browse image file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog() { Filter = "Image Files|*.bmp;*.gif;*.jpeg;*.jpg;*.png" };
                if (!(dialog.ShowDialog(this) ?? false)) return;

                _update = true;
                txtFilePath.Text = dialog.FileName;
                ConvertImageSource(dialog.FileName);
                if (rbtnOriginal.IsChecked ?? false) imgPreview.Source = _img;
                else if (rbtnBlack.IsChecked ?? false) imgPreview.Source = _imgB;
                else imgPreview.Source = _imgBR;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Brose_Image_File");
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Send image file to tags
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtFilePath.Text))
                {
                    MessageBox.Show("Please select a image to send.");
                    return;
                }
                if (!File.Exists(txtFilePath.Text))
                {
                    MessageBox.Show("File not exist.");
                    return;
                }

                var tasks = new List<TaskData>();
                var bitmap = Image.FromFile(txtFilePath.Text);
                TagIDs.ForEach(x => { tasks.Add(new TaskData(x, new Bitmap(bitmap))); });
                var result = SendServer.Instance.Push(tasks);
                MessageBox.Show($"Send image: {result}", "Result");
                this.Close();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Send_Image_File");
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Image preview: original
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnOriginal_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_img != null) imgPreview.Source = _img;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Preivew_Image");
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Image preview: black
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnBlack_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_imgB != null) imgPreview.Source = _imgB;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Preivew_Image");
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Image preview: black and red
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnBlackRed_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_imgBR != null) imgPreview.Source = _imgBR;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Preivew_Image");
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// SKContainer paint surface
        /// </summary>
        /// <param name="path">Image file path</param>
        private void ConvertImageSource(string path)
        {
            if (!File.Exists(path)) return;

            if (_update)
            {
                var bitmap = new Bitmap(Bitmap.FromFile(path));
                var W = bitmap.Width;
                var H = bitmap.Height;
                var bitmapB = new Bitmap(W, H);
                var bitmapBR = new Bitmap(W, H);
                BitmapData DATA = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                IntPtr Scan0 = DATA.Scan0;
                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    for (int h = 0; h < H; ++h)
                    {
                        for (int w = 0; w < W; ++w)
                        {
                            if (((byte)((p[0] * 19595 + p[1] * 38469 + p[2] * 7472) >> 16)) <= 125)
                            {
                                bitmapB.SetPixel(w, h, System.Drawing.Color.Black);
                            }
                            else
                            {
                                bitmapB.SetPixel(w, h, System.Drawing.Color.White);
                            }

                            if ((p[2] > 180) && (p[1] < 80) && (p[0] < 80))
                            {
                                bitmapBR.SetPixel(w, h, System.Drawing.Color.Red);
                            }
                            else if (((byte)((p[0] * 19595 + p[1] * 38469 + p[2] * 7472) >> 16)) <= 125)
                            {
                                bitmapBR.SetPixel(w, h, System.Drawing.Color.Black);
                            }
                            else
                            {
                                bitmapBR.SetPixel(w, h, System.Drawing.Color.White);
                            }
                            p += 3;
                        }
                    }
                }

                bitmap.UnlockBits(DATA);
                var pathBack = path.Insert(path.LastIndexOf('.'), "B");
                var pathBackRed = path.Insert(path.LastIndexOf('.'), "R");
                bitmapB.Save(pathBack, ImageFormat.Bmp);
                bitmapBR.Save(pathBackRed, ImageFormat.Bmp);

                _img = new BitmapImage(new Uri(path));
                _imgB = new BitmapImage(new Uri(pathBack));      // Black
                _imgBR = new BitmapImage(new Uri(pathBackRed));   // Black&Red

            }

        }
    }
}
