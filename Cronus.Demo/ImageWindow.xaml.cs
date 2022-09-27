using Cronus.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Cronus.Demo
{
    /// <summary>
    /// Interaction logic for ImageWindow.xaml
    /// </summary>
    public partial class ImageWindow : Window
    {
        ILogger _logger;        // Logger
        List<string> TagIDs;    // Tag ID list
        SKImage _imgOriginal;
        SKImage _imgBlack;
        SKImage _imgBlackRed;
        SKPoint _point = new(0, 0);
        SKColor _red = new(255, 0, 0);
        SKColor _black = new(0, 0, 0);
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
            skContainer.PaintSurface += skContainer_PaintSurface;
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
                OpenFileDialog dialog = new() { Filter = "Image Files|*.bmp;*.gif;*.jpeg;*.jpg;*.png" };
                if (!(dialog.ShowDialog(this) ?? false)) return;

                txtFilePath.Text = dialog.FileName;
                _update = true;
                skContainer?.InvalidateVisual();
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
                var bitmap = SKBitmap.FromImage(_imgOriginal);
                TagIDs.ForEach(x =>{ tasks.Add(new TaskData(x, bitmap, Cronos.SDK.Enum.Pattern.UpdateDisplay, Cronos.SDK.Enum.PageIndex.P0)); });
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
                skContainer?.InvalidateVisual();
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
                skContainer?.InvalidateVisual();
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
                skContainer?.InvalidateVisual();
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skContainer_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (!File.Exists(txtFilePath.Text)) return;

            if (_update)
            {
                _imgOriginal = SKImage.FromEncodedData(txtFilePath.Text);   // Original
                var bitmap = SKBitmap.FromImage(_imgOriginal);
                var W = bitmap.Width;
                var H = bitmap.Height;
                var bitmapBlack = new SKBitmap(W, H);
                var bitmapBlackRed = new SKBitmap(W, H);
                var Scan0 = bitmap.GetPixels();
                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    for (int h = 0; h < H; ++h)
                    {
                        for (int w = 0; w < W; ++w)
                        {
                            if (((byte)((p[2] * 19595 + p[1] * 38469 + p[0] * 7472) >> 16)) <= 125)
                            {
                                bitmapBlack.SetPixel(w, h, _black);
                                bitmapBlackRed.SetPixel(w, h, _black);
                            }

                            if ((p[2] > 200) && (p[1] < 70) && (p[0] < 70))
                            {
                                bitmapBlackRed.SetPixel(w, h, _red);
                            }
                            p += 4;
                        }
                    }
                }
                _imgBlack = SKImage.FromBitmap(bitmapBlack);      // Black
                _imgBlackRed = SKImage.FromBitmap(bitmapBlackRed);   // Black&Red
            }

            DrawPreviewImage(e.Surface.Canvas);
        }

        /// <summary>
        /// Draw preview image
        /// </summary>
        /// <param name="canvas">SKCanvas</param>
        private void DrawPreviewImage(SKCanvas canvas)
        {
            canvas.Clear();
            if (rbtnOriginal.IsChecked ?? false)
            {
                canvas.DrawImage(_imgOriginal, _point);
            }
            else if (rbtnBlack.IsChecked ?? false)
            {
                canvas.DrawImage(_imgBlack, _point);
            }
            else
            {
                canvas.DrawImage(_imgBlackRed, _point);
            }
        }
    }
}
