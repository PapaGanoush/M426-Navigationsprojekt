using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;

namespace Navigationsprojekt.Views
{
    /// <summary>
    /// Interaktionslogik für Karte_View.xaml
    /// </summary>
    public partial class Karte_View : UserControl
    {

        bool goNorth, goWest, goSouth, goEast;
        bool manuel = true;
        bool auto = false;
        int carSpeed = 2;
        int carSpeedAutomatically = 1;
        bool isRunning = false;
        Bitmap bitmapReal;
        byte[] staticImageArray;
        BitmapImage staticImage;

        string[] strings;

        DispatcherTimer rideTimer = new DispatcherTimer();

        public Karte_View()
        {
            InitializeComponent();

            map.Focus();

            rideTimer.Tick += RideTimerEvent;
            rideTimer.Interval = TimeSpan.FromMilliseconds(20);
            loadImage();
            rideTimer.Start();
           
        }
        
        public void loadImage()
        {
            /*BitmapImage myBitmapImage;
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri("C:/Users/vmadmin/Documents/426/M426-Navigationsprojekt/Navigationsprojekt/Karten/Welle_rund_rechts_bw.bmp");
            myBitmapImage.EndInit();
            landMap.Source = myBitmapImage;
            bitmapReal = BitmapImage2Bitmap(myBitmapImage);*/

            Image img = Image.FromFile("C:/Users/vmadmin/Documents/426/M426-Navigationsprojekt/Navigationsprojekt/Karten/Welle_rund_rechts_bw.bmp");
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                staticImageArray = ms.ToArray();
            }
            ToImage(staticImageArray);
            bitmapReal = BitmapImage2Bitmap(staticImage);

        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public void ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                staticImage = new BitmapImage();
                staticImage.BeginInit();
                staticImage.CacheOption = BitmapCacheOption.OnLoad; // here
                staticImage.StreamSource = ms;
                staticImage.EndInit();

                landMap.Source = staticImage;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            manuel = true;
            AlertPopup.IsOpen = false;
            map.Focus();
            rideTimer.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            auto = true;
            AlertPopup.IsOpen = false;
            map.Focus();
            rideTimer.Start();
        }

        private void OutOfFuelButton_Click(object sender, RoutedEventArgs e)
        {
            OutOfFuelPopup.IsOpen = false;
            map.Focus();
            FuelBar.Value = 100;
            rideTimer.Start();
        }

        private void CarSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex regex = new Regex(@"[\d]");

            regex = new Regex("[0-9]{1,2}?$");

            if (regex.IsMatch(CarSize.Text))
            {
                if (CarSize.Text == "")
                {
                    car.Height = 1;
                    car.Width = 1;
                }
                else
                {
                    car.Height = Convert.ToDouble(CarSize.Text);
                    car.Width = Convert.ToDouble(CarSize.Text);
                }
                
            }

            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if(CarSize.Text != "")
            {
               isRunning = true;
                CarSize.Background = Brushes.Green;
                start.IsEnabled = false;
                CarSize.IsEnabled = false;
                car.Visibility = Visibility.Visible;
                map.Focus();
            }
            else
            {
                CarSize.Background = Brushes.Red;
            }
            
        }
        private void Mouse_Click(object sender, MouseEventArgs e)
        {
            System.Windows.Point start = Mouse.GetPosition(map);
            bitmapReal.SetPixel(Convert.ToInt32(start.X),Convert.ToInt32(start.Y), System.Drawing.Color.Red);
            
        }

        private void RideTimerEvent(object sender, EventArgs e)
        {
            if (FuelBar.Value == 0)
            {
                rideTimer.Stop();
                OutOfFuelPopup.IsOpen = true;
            }
            else
            {

                if (manuel == true)
                {
                    if (goWest == true && Canvas.GetLeft(car) > 5)
                    {
                        Canvas.SetLeft(car, Canvas.GetLeft(car) - carSpeed);
                        FuelBar.Value -= 0.1;
                    }
                    if (goNorth == true && Canvas.GetTop(car) > 5)
                    {
                        Canvas.SetTop(car, Canvas.GetTop(car) - carSpeed);
                        FuelBar.Value -= 0.1;
                    }
                    if (goEast == true && Canvas.GetLeft(car) + (car.Width + 20) < 800)
                    {
                        Canvas.SetLeft(car, Canvas.GetLeft(car) + carSpeed);
                        FuelBar.Value -= 0.1;
                    }
                    if (goSouth == true && Canvas.GetTop(car) + (car.Height + 20) < 800)
                    {
                        Canvas.SetTop(car, Canvas.GetTop(car) + carSpeed);
                        FuelBar.Value -= 0.1;
                    }
                }
                else if (auto == true)
                {
                    Canvas.SetTop(car, Canvas.GetTop(car) - carSpeedAutomatically);
                    FuelBar.Value -= 0.1;
                }
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Left)
            {
                goWest = true;
                Console.WriteLine("hi");
            }
            if (e.Key == Key.Up)
            {
                goNorth = true;
            }
            if (e.Key == Key.Right)
            {
                goEast = true;
            }
            if (e.Key == Key.Down)
            {
                goSouth = true;
            }
            if(e.Key == Key.M)
            {
                AlertPopup.IsOpen = true;
                rideTimer.Stop();
            }
            if(e.Key == Key.A)
            {
                auto = true;
                manuel = false;
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                goWest = false;
            }
            if (e.Key == Key.Up)
            {
                goNorth = false;
            }
            if (e.Key == Key.Right)
            {
                goEast = false;
            }
            if (e.Key == Key.Down)
            {
                goSouth = false;
            }
        }

        private void Startpunkt_Button_Click(object sender, RoutedEventArgs e)
        {
            /*using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"C:\Users\vmadmin\Desktop\test.txt", true))
            {
                byte[] pixels = new byte[size];
                for (int b = 0; b < bitmapReal.Height; b++)
                {
                    for (int c = 0; c < bitmapReal.Width; c++)
                    {
                        Console.WriteLine("H: " + b + " W: " + c + " Color: " + bitmapReal.GetPixel(b, c));
                        file.WriteLine("H: " + b + " W: " + c + " Color: " + bitmapReal.GetPixel(b, c));
                    }
                }
            }*/

            //SetPixel und Image aktualisieren
            bitmapReal.SetPixel(276, 130, Color.Green);
            bitmapReal.SetPixel(276, 360, Color.Red);

            using (MemoryStream ms = new MemoryStream())
            {
                bitmapReal.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                staticImageArray = ms.ToArray();
            }
            ToImage(staticImageArray);
            bitmapReal = BitmapImage2Bitmap(staticImage);
        }

        public void SetPixel2(int x, int y, System.Drawing.Color color)
        {

        }

        private void Endpunkt_Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
