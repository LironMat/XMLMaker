using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace XMLMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        static int[,] mat;
        static List<TextBox> tbList = new List<TextBox>();
        private void ScanB_Click(object sender, RoutedEventArgs e)
        {
            ReadPicture();
        }
        public void ReadPicture()
        {
            DColor CurrentColor;
            List<DColor> ColorList = new List<DColor>();
            Bitmap image1 = (Bitmap)System.Drawing.Image.FromFile(pictureTB.Text);
            Img1.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\" + pictureTB.Text));
            double maxSim = int.Parse(maxSimTB.Text);
            int maxIndex = 0;
            mat = new int[image1.Height, image1.Width];
            for (int i = 0; i < image1.Height; i++)
            {
                for (int j = 0; j < image1.Width; j++)
                {
                    CurrentColor = image1.GetPixel(j, i);
                    maxSim = double.Parse(maxSimTB.Text);
                    maxIndex = -1;
                    foreach (DColor c in ColorList)
                    {
                        if (((Math.Abs(CurrentColor.R - c.R) + Math.Abs(CurrentColor.B - c.B) + Math.Abs(CurrentColor.G - c.G)) / 3) <= maxSim)
                        {
                            maxSim = ((Math.Abs(CurrentColor.R - c.R) + Math.Abs(CurrentColor.B - c.B) + Math.Abs(CurrentColor.G - c.G)) / 3);
                            maxIndex = ColorList.IndexOf(c);
                        }
                    }
                    if (maxIndex == -1)
                    {
                        ColorList.Add(CurrentColor);
                        maxIndex = ColorList.Count - 1;
                    }
                    image1.SetPixel(j, i, ColorList[maxIndex]);
                    mat[i, j] = maxIndex;
                }
            }
            Img2.Source = BitmapToImageSource(image1);
            ColorsSP.Children.Clear();
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            tbList = new List<TextBox>();
            foreach (System.Drawing.Color color in ColorList)
            {
                sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle();
                r.Width = 40;
                r.Height = 40;
                r.Fill = new SolidColorBrush(MColor.FromArgb(color.A, color.R, color.G, color.B));
                TextBox tb = new TextBox();
                tb.Text = ColorList.IndexOf(color).ToString();
                tb.Width = 100;
                sp.Children.Add(r);
                sp.Children.Add(tb);
                tbList.Add(tb);
                ColorsSP.Children.Add(sp);
            }
            Button createB = new Button();
            createB.Click += new RoutedEventHandler(createB_Click);
            createB.Content = "create file";
            createB.Width = 100;
            createB.HorizontalAlignment = HorizontalAlignment.Left;
            Button savePictueB = new Button();
            savePictueB.Content = "save picture";
            savePictueB.Width = 100;
            savePictueB.HorizontalAlignment = HorizontalAlignment.Left;
            savePictueB.Click += SavePictueB_Click;
            ColorsSP.Children.Add(createB);
            ColorsSP.Children.Add(savePictueB);
        }

        private void SavePictueB_Click(object sender, RoutedEventArgs e)
        {
            Bitmap b = BitmapFromSource(Img2.Source as BitmapImage);
            string postDot = pictureTB.Text.Substring(pictureTB.Text.IndexOf('.'));
            b.Save(pictureTB.Text.Substring(0, pictureTB.Text.IndexOf('.')) + '_' + maxSimTB.Text + '_' + DateTime.Now.ToString("yyyyMMddHHmmss") + postDot);
        }

        void createB_Click(object sender, RoutedEventArgs e)
        {
            XmlTextWriter writer = new XmlTextWriter(pictureTB.Text.Substring(0, pictureTB.Text.IndexOf('.')) + '_' + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml", System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            writer.WriteStartElement("Map");
            for (int i = 1; i <= mat.GetLength(0); i++)
            {
                writer.WriteComment(String.Format("***************{0}*****************", i));
                writer.WriteStartElement("Line");
                for (int j = 1; j <= mat.GetLength(1); j++)
                {
                    writer.WriteStartElement("Area");
                    writer.WriteStartAttribute("value");
                    writer.WriteValue(tbList.ElementAt(mat[i - 1, j - 1]).Text);
                    writer.WriteEndElement();
                    if (j / 10 >= 1 && j % 10 == 0)
                        writer.WriteComment(String.Format("*****{0}*****", j));
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        public Bitmap BitmapFromSource(System.Windows.Media.Imaging.BitmapSource bitmapsource)
        {
            //convert image format
            var src = new System.Windows.Media.Imaging.FormatConvertedBitmap();
            src.BeginInit();
            src.Source = bitmapsource;
            src.DestinationFormat = System.Windows.Media.PixelFormats.Bgra32;
            src.EndInit();

            //copy to bitmap
            Bitmap bitmap = new Bitmap(src.PixelWidth, src.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            src.CopyPixels(System.Windows.Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bitmap.UnlockBits(data);

            return bitmap;
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
