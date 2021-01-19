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
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;

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
        int startKoordsX = 0;
        int startKoordsY = 0;
        int endKoordsX = 0;
        int endKoordsY = 0;
        bool starPunkt = false;
        bool endPunkt = false;
        private List<int[]> weissePixel = new List<int[]>();
        private List<int[]> schwarzePixel = new List<int[]>();

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
            leseBereicheAus();

        }

        private void leseBereicheAus()
        {
            int w = 0;
            for (int i = 0; i < bitmapReal.Width; i++)
            {
                for (int j = 0; j < bitmapReal.Height; j++)
                {
                    Color pixel = bitmapReal.GetPixel(i, j);

                    if (pixel.B == 255)
                    {
                       weissePixel.Add(new int[2] {i, j});
                    }
                    else
                    {
                        schwarzePixel.Add(new int[2] {i, j});
                    }
                }
            }
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

            Image img = Image.FromFile("V:/Schule/M426/M426-Navigationsprojekt/Navigationsprojekt/Karten/Welle_rund_rechts_bw.bmp");
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                staticImageArray = ms.ToArray();
            }
            ToImage(staticImageArray);
            bitmapReal = BitmapImage2Bitmap(staticImage);

        }

        public void reloadImage()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmapReal.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
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
               
                CarSize.Background = Brushes.Green;
                start.IsEnabled = false;
                CarSize.IsEnabled = false;
                Canvas.SetLeft(car, startKoordsX);
                Canvas.SetTop(car, startKoordsY);
                isRunning = true;
                car.Visibility = Visibility.Visible;
                map.Focus();
                
            }
            else
            {
                CarSize.Background = Brushes.Red;
            }
            
        }
        /*private void Mouse_Click(object sender, MouseEventArgs e)
        {
            System.Windows.Point start = Mouse.GetPosition(map);
            bitmapReal.SetPixel(Convert.ToInt32(start.X),Convert.ToInt32(start.Y), System.Drawing.Color.Red);
            
        }*/

        private void RideTimerEvent(object sender, EventArgs e)
        {
            if (isRunning)
            {
                if (FuelBar.Value == 0)
                {
                    rideTimer.Stop();
                    OutOfFuelPopup.IsOpen = true;
                }
                else
                {
                    if (bitmapReal.GetPixel(Convert.ToInt32(Canvas.GetLeft(car)), Convert.ToInt32(Canvas.GetTop(car))).R == 255 &&
                        bitmapReal.GetPixel(Convert.ToInt32(Canvas.GetLeft(car)), Convert.ToInt32(Canvas.GetTop(car))).B == 0)
                    {
                        rideTimer.Stop();
                        OutOfFuelPopup.IsOpen = true;
                    }
                    else
                    {
                        test.Content = endKoordsX + " " + endKoordsY;

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
            //SetPixel und Image aktualisieren
            starPunkt = true;
            landMap.MouseLeftButtonDown += new MouseButtonEventHandler(landMap_MouseLeftButtonDown);

            
        }

        private void landMap_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (starPunkt)
            {
                Point pointtest = e.GetPosition(landMap);
                Console.WriteLine(pointtest.ToString());
                startKoordsX = Convert.ToInt32(pointtest.X);
                startKoordsY = Convert.ToInt32(pointtest.Y);

                if (startKoordsX != 0)
                {
                    Rectangle yourRectangle = new Rectangle(startKoordsX, startKoordsY, 9, 9);
                    SolidBrush greenBrush = new SolidBrush(Color.Green);

                    using (Graphics G = Graphics.FromImage(bitmapReal))
                    {
                        G.FillRectangle(greenBrush, yourRectangle);
                    }

                    reloadImage();
                    Startpunkt_Button.IsEnabled = false;
                    starPunkt = false;
                }
            }else if (endPunkt)
            {
                Point pointtest = e.GetPosition(landMap);
                Console.WriteLine(pointtest.ToString());
                endKoordsX = Convert.ToInt32(pointtest.X);
                endKoordsY = Convert.ToInt32(pointtest.Y);

                if (endKoordsX != 0)
                {
                    Rectangle yourRectangle = new Rectangle(endKoordsX, endKoordsY, 9, 9);
                    SolidBrush greenBrush = new SolidBrush(Color.Red);

                    using (Graphics G = Graphics.FromImage(bitmapReal))
                    {
                        G.FillRectangle(greenBrush, yourRectangle);
                    }

                    reloadImage();
                    Endpunkt_Button.IsEnabled = false;
                    endPunkt = false;
                }
            }
        }

     

        private void Endpunkt_Button_Click(object sender, RoutedEventArgs e)
        {
            //SetPixel und Image aktualisieren
            endPunkt = true;
            landMap.MouseLeftButtonDown += new MouseButtonEventHandler(landMap_MouseLeftButtonDown);

            
        }
    }
}
