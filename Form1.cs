using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;

namespace kznorm
{
    public partial class Form1 : Form
    {

        Mat etalon = Cv2.ImRead("C:\\Users\\nikita\\Desktop\\sadsada\\kznorm\\kznorm\\etalon.jpg", ImreadModes.Color);

        int k = 0;
        private VideoCapture videoCapture;
        private System.Windows.Forms.Timer timer;
        private int currentFrame = 0;
        int max=0;
        public Form1()
        {
            InitializeComponent();
            timer = new System.Windows.Forms.Timer();
            //timer.Interval = 40; // 25 FPS (1000ms / 25 = 40ms)
            timer.Interval = 20;
            timer.Tick += Timer_Tick;
            
/*            // Преобразуем изображение в HSV
            Mat hsv = new Mat();
            Cv2.CvtColor(etalon, hsv, ColorConversionCodes.BGR2HSV);

            // pictureBox1.Image = MatToBitmap(etalon);
            
            // Устанавливаем нижнюю и верхнюю границы для бинаризации
            Scalar lowerBound = new Scalar(0, 75, 100);
            Scalar upperBound = new Scalar(179, 255, 255);

            // Бинаризация изображения


            Mat binaryImage= new Mat();
           
            etalon = ResizeImage(hsv);
            Cv2.InRange(etalon, lowerBound,upperBound,binaryImage);
            Bitmap et;
            etalon = binaryImage;*/
            
            pictureBox1.Image = MatToBitmap(etalon);


        }

        private Mat ResizeImage(Mat image)
        {
            Mat resizedImage = new Mat();
            Cv2.Resize(image, resizedImage, new OpenCvSharp.Size(64, 64), 0, 0, InterpolationFlags.Linear);
            return resizedImage;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video Files (*.avi;*.mp4;*.mkv;*.flv;*.wmv)|*.avi;*.mp4;*.mkv;*.flv;*.wmv";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadVideoFrames(openFileDialog.FileName);
            }
        }
        /**/
        private void LoadVideoFrames(string videoFilePath)
        {
            videoCapture = new VideoCapture(videoFilePath);

            if (!videoCapture.IsOpened())
            {
                MessageBox.Show("Failed to open video file.");
                return;
            }

            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            using (var mat = new Mat())
            {
                if (videoCapture.Read(mat))
                {
                    Bitmap frame = MatToBitmap(mat);
                    Bitmap processedFrame = ProcessContours(frame);

                    pictureBox1.Image = processedFrame;
                }
                else
                {
                    timer.Stop();
                }
            }
        }
        private Bitmap MatToBitmap(Mat mat)
        {
            Bitmap bitmap = new Bitmap(mat.Width, mat.Height, PixelFormat.Format24bppRgb);
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, mat.Width, mat.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int dataSize = mat.Width * mat.Height * mat.ElemSize();
            byte[] bytes = new byte[dataSize];
            Marshal.Copy(mat.Data, bytes, 0, dataSize);
            Marshal.Copy(bytes, 0, data.Scan0, dataSize);

            bitmap.UnlockBits(data);
            return bitmap;
        }
        /*private Bitmap MatToBitmap(Mat mat)
        {
            // Создание Bitmap
            Bitmap bitmap = new Bitmap(mat.Width, mat.Height, PixelFormat.Format24bppRgb);

            // Блокировка Bitmap для записи
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, mat.Width, mat.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            // Копирование данных из Mat в Bitmap
            int dataSize = mat.Width * mat.Height * mat.NumberOfChannels;
            byte[] bytes = new byte[dataSize];
            Marshal.Copy(mat.DataPointer, bytes, 0, dataSize);
            Marshal.Copy(bytes, 0, data.Scan0, dataSize);

            // Разблокировка Bitmap
            bitmap.UnlockBits(data);

            return bitmap;
        }*/
        
        private Mat BitmapToMat(Bitmap bitmap)
        {
            Mat mat = new Mat(bitmap.Height, bitmap.Width, MatType.CV_8UC3);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int dataSize = bitmap.Width * bitmap.Height * 3;
            byte[] bytes = new byte[dataSize];
            Marshal.Copy(data.Scan0, bytes, 0, dataSize);

            Marshal.Copy(bytes, 0, mat.Data, dataSize);

            bitmap.UnlockBits(data);

            return mat;
        }



        private Bitmap ProcessContours(Bitmap image)
        {
            // Преобразуем Bitmap в Mat для использования в OpenCVSharp
            Mat mat = BitmapToMat(image);

            // Преобразуем изображение в HSV
            Mat hsv = new Mat();
            Cv2.CvtColor(mat, hsv, ColorConversionCodes.BGR2HSV);

            // Устанавливаем нижнюю и верхнюю границы для бинаризации
            Scalar lowerBound = new Scalar(0, 75, 100);
            Scalar upperBound = new Scalar(179, 255, 255);


           // hsv = ResizeImage(hsv);
            // Бинаризация изображения
            Mat binary = new Mat();
            Cv2.InRange(hsv, lowerBound, upperBound, binary);
            
            // Find contours
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            //Cv2.FindContours(hsv, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            var filteredContours = contours.Where(contour => Cv2.ContourArea(contour) >= 100 && Cv2.ContourArea(contour) <= 3000).ToArray();

            // Process each contour
            foreach (var contour in filteredContours)
            {

                //double area = Cv2.ContourArea(contour);
                //label1.Text = area.ToString();
                //if (area < 100 && area > 10000)
                //    break;

                // Get the bounding rectangle around the contour
                OpenCvSharp.Rect boundingRect = Cv2.BoundingRect(contour);

                // Draw the bounding rectangle on the original image
                Cv2.Rectangle(mat, boundingRect, Scalar.Red, 2);

                // Extract ROI from the image
                
                Mat roiMat = new Mat(mat, boundingRect);
               
                Bitmap roiBitmap = MatToBitmap(roiMat);

                // Check the ROI with a template
                int matchingPixels = MatchTemplate(roiMat);
                //label1.Text = matchingPixels.ToString();
                if (matchingPixels > 3000)
                {
                    /*k++;
                    label1.Text = k.ToString();*/

                    // Convert ROI to grayscale and apply thresholding
                    Mat roiGray = new Mat();
                    Cv2.CvtColor(roiMat, roiGray, ColorConversionCodes.BGR2GRAY);
                         Mat roiThresh = new Mat();
                    Cv2.Threshold(roiGray, roiThresh, 127, 255, ThresholdTypes.BinaryInv);






                       // Extract text using pytesseract
                       string text = ExtractText(MatToBitmap(roiThresh));
                       //string text = "0cxzxczc";




                       // Check text conditions and draw rectangle with text
                       if (!string.IsNullOrEmpty(text) && int.TryParse(text, out int speed) && speed % 10 == 0)
                       {
                           Cv2.Rectangle(mat, boundingRect, Scalar.Green, 2);
                           Cv2.PutText(mat, $"Speed limit {text}", new OpenCvSharp.Point(boundingRect.X, boundingRect.Y - 5),
                               HersheyFonts.HersheyDuplex, 0.6, Scalar.Green, 1, LineTypes.AntiAlias);
                       }
                       else
                       {
                           Cv2.Rectangle(mat, boundingRect, Scalar.Yellow, 2);
                       }
                   }
               }

               return MatToBitmap(mat);
           }



        private int MatchTemplate(Mat roiMat)
        {
            roiMat = ResizeImage(roiMat);

            if (roiMat.Size() != etalon.Size())
            {
                throw new Exception("Размеры изображений не совпадают");
            }

            int matchingPixels = 0;
            for (int y = 0; y < roiMat.Height; y++)
            {
                for (int x = 0; x < roiMat.Width; x++)
                {
                    byte roiPixel = roiMat.At<byte>(y, x);
                    byte etalonPixel = etalon.At<byte>(y, x);

                    if (roiPixel == etalonPixel)
                    {
                        matchingPixels++;
                    }
                }
            }
            if (matchingPixels > max)
            {
                max = matchingPixels;
                label1.Text = max.ToString();
            }
            //label1.Text = matchingPixels.ToString();
            /*if (matchingPixels > 3000)
            {
                label1.Text = matchingPixels.ToString();
            }*/
            return matchingPixels;
        }

        private string ExtractText(Bitmap roiThresh)
        {

            using (var engine = new TesseractEngine("./tessdata", "eng", EngineMode.Default))
            {

                using (var image = PixConverter.ToPix(roiThresh))
                {

                    engine.SetVariable("tessedit_char_whitelist", "0123456789");


                    using (var page = engine.Process(image))
                    {

                        return page.GetText();
                    }
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
