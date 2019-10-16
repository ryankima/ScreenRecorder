using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Forms;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;
using System.IO;
using Button = System.Windows.Controls.Button;
using AForge.Video;
using AForge.Video.FFMPEG;
using System.Drawing.Imaging;

namespace ScreenCapture

{
    /*
     * Screen capture program
     *  -Uses WPF and AForge .net libraries
     *  -Exports video to Avi video format under name "Capture.avi"
     */
    public partial class MainWindow : Window
    {
        //file count number
        Int32 count;

        //Timer variable controls frame capture rate
        Timer refresh;

        VideoFileWriter vFWriter;
        

        //Main function; called on start of program
        public MainWindow()
        {
            //Initialize XAML window
            InitializeComponent();

            //Buttons to begin and end recording
            Button startRecording = StartRecording;
            Button endRecording = EndRecording;
            endRecording.Click += CreateVideo;
            startRecording.Click += BeginRecord;

        }

        //Function triggered by button press of begin record
        //This function sets timer variables and clears videoBitmapArr
        public void BeginRecord(object sender, RoutedEventArgs e)
        {
            refresh = new Timer();
            refresh.Tick += new EventHandler(timer_tick);
            refresh.Interval = 41;
            vFWriter = new VideoFileWriter();
            vFWriter.Open("Capture.mp4", Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height, 12, VideoCodec.MPEG4);
            refresh.Start();
            count = 1;
        }

        //Function triggered by button press of end record
        //This function uses the Aforge library to convery videoBitmapArr into video
        public void CreateVideo(object sender, RoutedEventArgs e)
        {
            refresh.Stop();
            refresh.Dispose();
            vFWriter.Close();
            vFWriter.Dispose();
        }

        //Function called every 42 ms
        //Captures monitor screen as bitmap
        public Bitmap ScreenCapture()
        {
            try
            {
                Bitmap captureBitmap = new Bitmap(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height, PixelFormat.Format24bppRgb);
                Rectangle captureRectangle = Screen.AllScreens[0].Bounds;
                Graphics captureGraphics = Graphics.FromImage(captureBitmap);

                captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);
                captureGraphics.Dispose();

                
                vFWriter.WriteVideoFrame(captureBitmap);


                return captureBitmap;
            } catch(Exception e)
            {
                return new Bitmap(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height, PixelFormat.Format24bppRgb);
            }
        }

        //Function called every 42 ms
        //Takes a bitmap image and displays on the main window
        public void DisplayImage(Bitmap screenCap)
        {
            screenCap.Save(System.Windows.Forms.Application.StartupPath + "\\Temp\\" + count.ToString() + ".jpeg");
         
            count++;
            System.Windows.Controls.Image dynamicImage = new System.Windows.Controls.Image();
            var bitmapImage = new BitmapImage();

            using (var memory = new MemoryStream())
            {
                screenCap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                screenCap.Dispose();
                memory.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
                memory.Dispose();
            }

            dynamicImage.Source = bitmapImage;

            dynamicImage.Stretch = Stretch.UniformToFill;
             

            LayoutRoot.Children.Add(dynamicImage);
            
        }

        //Function called every 42 ms
        //This function is called while timer is active and updates the screen and adds an image to videoBitmapArr
        private void timer_tick(object sender, EventArgs e)
        {
            try
            {
                DisplayImage(ScreenCapture());
            }
            catch (Exception error)
            {

            }
        }
    }
}
