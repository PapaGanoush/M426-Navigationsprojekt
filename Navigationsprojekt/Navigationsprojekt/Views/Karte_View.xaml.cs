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
using System.Drawing;
using System.IO;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;

namespace Navigationsprojekt.Views
{
    /// <summary>
    /// Interaktionslogik für Karte_View.xaml
    /// </summary>
    public partial class Karte_View : UserControl
    {
        private Bitmap image;
        public Karte_View()
        {
            InitializeComponent();
            generateImage();
            landMap.Source = generateImage();
            test();
        }

        public BitmapImage generateImage()
        {

            using (MemoryStream memory = new MemoryStream())
            {
                System.Net.WebRequest request =
                    System.Net.WebRequest.Create(
                        "https://www.duden.de/og-image/188355.png");
                System.Net.WebResponse response = request.GetResponse();
                System.IO.Stream responseStream =
                    response.GetResponseStream();


                image = new Bitmap(responseStream);
                image.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }


        }

        public void test()
        {
            /*byte[,] array = new byte[Convert.ToInt32(image.Width), Convert.ToInt32(image.Height)];

            for (int i = 0; i <= Convert.ToInt32(image.Width); i++)
            {

                for (int j = 0; j <= Convert.ToInt32(image.Height); j++)
                {
                    Console.WriteLine(image.GetPixel(i, j));
                }
            }*/
            image.SetPixel(100, 100, Color.Green);
        }
    }
}
