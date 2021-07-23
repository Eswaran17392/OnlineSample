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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MatrixTransform ZoomMatrixTransform;
        public MainWindow()
        {
            InitializeComponent();
            grid.DataContext = this;
            updateZoom = true;
        }

        bool updateZoom;

        public int ZoomFactor
        {
            get { return (int)GetValue(ZoomFactorProperty); }
            set { SetValue(ZoomFactorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZoomFactor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomFactorProperty =
            DependencyProperty.Register("ZoomFactor", typeof(int), typeof(MainWindow), new PropertyMetadata(100, OnZoomFactorChanged));

        private static void OnZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as MainWindow;
            int factor = (int)e.NewValue;
            if (factor >= 50 && factor <= 400 && window.updateZoom)
                window.PerformZoom(e.NewValue);
        }

        private void PerformZoom(object newValue)
        {          

            float zoomValue = Convert.ToInt32(newValue) / 100f;

            Matrix matrix = Matrix.Identity;
            var scaleX = zoomValue;
            var scaleY = zoomValue;
            matrix.Scale(scaleX, scaleY);

            ZoomMatrixTransform = new MatrixTransform(matrix);

            foreach (UIElement child in panel.Children)
            {
                child.RenderTransformOrigin = new Point(0.5, 0.5);
                child.RenderTransform = ZoomMatrixTransform;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ZoomFactor += 10;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ZoomFactor -= 10;
        }

        private void Panel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var element = sender as UIElement;
            var position = e.GetPosition(panel.Children[0]);

            var matrix = ZoomMatrixTransform == null ? Matrix.Identity : ZoomMatrixTransform.Matrix;

            var scale = e.Delta >= 0 ? 1.1 : (1.0 / 1.1);

            matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
            var factor = (int)(matrix.M11 * 100);
            updateZoom = false;
            ZoomFactor = factor <= 50 ? 50 : factor >= 400 ? 400 : factor;
            updateZoom = true;

            if (factor >= 50 && factor <= 400)
            {
                ZoomMatrixTransform = new MatrixTransform(matrix);
                foreach (UIElement child in panel.Children)
                {
                    child.RenderTransform = ZoomMatrixTransform;
                }
              //  isZoomed = true;
            }
        }
        Point start;
        Point origin;
        private void Panel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
            if ( ZoomMatrixTransform != null && ZoomMatrixTransform.Matrix != Matrix.Identity)
            {
                start = e.GetPosition(panel);
                origin = new Point(ZoomMatrixTransform.Matrix.OffsetX, ZoomMatrixTransform.Matrix.OffsetY);
                editorImage.CaptureMouse();
            }

            Point position;
            if (this.editorImage != null)
                position = e.GetPosition(this.editorImage);
            else
                position = e.GetPosition(panel);
           // CheckValueIsInRange(position);
           // if (!isInRange) return;

            //if (e.GetPosition(this).X >= this.actualImageRect.Width ||
            //  e.GetPosition(this).Y >= this.actualImageRect.Height)
            //    this.ReleaseMouseCapture();
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
           
            Point position;
            if (this.editorImage != null)
                position = e.GetPosition(this.editorImage);
            else
                position = e.GetPosition(this.panel);
            //CheckValueIsInRange(position);
            //if (!isInRange) return;

            if (editorImage.IsMouseCaptured && ZoomMatrixTransform.Matrix != Matrix.Identity)
            {
                FrameworkElement frameworkElement;
                if (editorImage != null)
                    frameworkElement = editorImage;
                else
                    frameworkElement = panel;

                var elementBounds = new Rect(frameworkElement.RenderSize);
                var transformedBounds = editorImage.TransformToAncestor(panel).TransformBounds(elementBounds);

                var matrix = ZoomMatrixTransform.Matrix;
                Vector vector = start - e.GetPosition(panel);

                if (transformedBounds.Left < 0 && vector.X <= 0)
                    matrix.OffsetX = origin.X - vector.X;
                else if (vector.X >= 0 && transformedBounds.Right >= panel.ActualWidth)
                    matrix.OffsetX = origin.X - vector.X;

                if (transformedBounds.Top < 0 && vector.Y <= 0)
                    matrix.OffsetY = origin.Y - vector.Y;
                else if (vector.Y >= 0 && transformedBounds.Bottom >= panel.ActualHeight)
                    matrix.OffsetY = origin.Y - vector.Y;

                ZoomMatrixTransform.Matrix = matrix;
                foreach (UIElement child in panel.Children)
                {
                    child.RenderTransform = ZoomMatrixTransform;
                }
            }
        }

        private void Panel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            editorImage.ReleaseMouseCapture();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ZoomMatrixTransform.Matrix = Matrix.Identity;
            foreach (UIElement child in panel.Children)
            {
                child.RenderTransform = ZoomMatrixTransform;
            }
        }
    }
}
