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

        private bool goNorth, goWest, goSouth, goEast;
        private bool manuel = true;
        private bool auto = false;
        private int carSpeed = 2;
        private int carSpeedAutomatically = 1;
        private bool isRunning = false;
        private Bitmap bitmapReal;
        private byte[] staticImageArray;
        private BitmapImage staticImage;
        private int startKoordsX = 0;
        private int startKoordsY = 0;
        private int endKoordsX = 0;
        private int endKoordsY = 0;
        private bool startPunkt = false;
        private bool endPunkt = false;
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

        /*
        Liest den gültigen und ungültigen Bereich der Bitmap anhand der Farbe der Pixel heraus
        und speichert jeden Pixel in einer int-Array Liste
        */
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
                        weissePixel.Add(new int[2] { i, j });
                    }
                    else
                    {
                        schwarzePixel.Add(new int[2] { i, j });
                    }
                }
            }
        }

        //Lädt die Karte von einem bmp File
        public void loadImage()
        {
            Image img = Image.FromFile("../../Karten/Welle_rund_rechts_bw.bmp");
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                staticImageArray = ms.ToArray();
            }
            ToImage(staticImageArray);
            bitmapReal = BitmapImage2Bitmap(staticImage);

        }

        //Lädt die Bitmap neu
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

        //Transformiert ein BitmapImage in eine Bitmap
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

        //Transformiert ein byte-Array in ein Bild
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
            manuel = true;
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
            if (CarSize.Text != "")
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
                    else if (bitmapReal.GetPixel(Convert.ToInt32(Canvas.GetLeft(car) - 1), Convert.ToInt32(Canvas.GetTop(car) - 1)).R == 0 &&
                        bitmapReal.GetPixel(Convert.ToInt32(Canvas.GetLeft(car) - 1), Convert.ToInt32(Canvas.GetTop(car) - 1)).G == 0 &&
                        bitmapReal.GetPixel(Convert.ToInt32(Canvas.GetLeft(car) - 1), Convert.ToInt32(Canvas.GetTop(car) - 1)).B == 0)
                    {

                        manuel = false;
                        Canvas.SetLeft(car, Canvas.GetLeft(car) + 5);
                        Canvas.SetTop(car, Canvas.GetTop(car) + 5);
                        OutOfFuelPopup.IsOpen = true;



                    }
                    else if (bitmapReal.GetPixel(Convert.ToInt32(Canvas.GetLeft(car) + 1), Convert.ToInt32(Canvas.GetTop(car) + 1)).R == 0 &&
                       bitmapReal.GetPixel(Convert.ToInt32(Canvas.GetLeft(car) + 1), Convert.ToInt32(Canvas.GetTop(car) + 1)).G == 0 &&
                       bitmapReal.GetPixel(Convert.ToInt32(Canvas.GetLeft(car) + 1), Convert.ToInt32(Canvas.GetTop(car) + 1)).B == 0)
                    {
                        manuel = false;
                        Canvas.SetLeft(car, Canvas.GetLeft(car) - 5);
                        Canvas.SetTop(car, Canvas.GetTop(car) - 5);
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
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                goWest = true;
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
            if (e.Key == Key.M)
            {
                AlertPopup.IsOpen = true;
                rideTimer.Stop();
            }
            if (e.Key == Key.A)
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
            endPunkt = false;
            startPunkt = true;
            landMap.MouseLeftButtonDown += new MouseButtonEventHandler(landMap_MouseLeftButtonDown);
        }

        private void landMap_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (startPunkt || endPunkt)
            {
                Point pointtest = e.GetPosition(landMap);
                int x = Convert.ToInt32(pointtest.X);
                int y = Convert.ToInt32(pointtest.Y);

                Color clickedPixel = bitmapReal.GetPixel(x, y);
                if (!clickedPixel.Name.Equals("ffffffff"))
                {
                    endPunkt = false;
                    startPunkt = false;
                    MessageBox.Show("Out of bounds. Only placeable on route.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (x == 0)
                {
                    return;
                }
                Rectangle yourRectangle = new Rectangle(x, y, 9, 9);
                SolidBrush solidBrush;
                if (startPunkt)
                {
                    solidBrush = new SolidBrush(Color.Green);
                    startKoordsX = x;
                    startKoordsY = y;
                    Startpunkt_Button.IsEnabled = false;
                    startPunkt = false;
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Red);
                    endKoordsX = x;
                    endKoordsY = y;
                    Endpunkt_Button.IsEnabled = false;
                    endPunkt = false;
                }

                using (Graphics G = Graphics.FromImage(bitmapReal))
                {
                    G.FillRectangle(solidBrush, yourRectangle);
                }

                reloadImage();
            }
        }



        private void Endpunkt_Button_Click(object sender, RoutedEventArgs e)
        {
            //SetPixel und Image aktualisieren
            endPunkt = true;
            startPunkt = false;
            landMap.MouseLeftButtonDown += new MouseButtonEventHandler(landMap_MouseLeftButtonDown);
        }
    }
}
