// See https://aka.ms/new-console-template for more information
using Cronos.SDK;
using System.Drawing;

Console.WriteLine("Hello, World!");
Image image = Image.FromFile("I.bmp");
Bitmap bitmap = new Bitmap(image);
var data = Server.Instance.GetTagData("401234567890", 1, bitmap);