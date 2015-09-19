using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PointArea {
    public partial class MainWindow : Window {
        private Point[] Contour = new Point[4];

        public MainWindow() {
            InitializeComponent();
        }

        private void CalcButton_Click(object sender, RoutedEventArgs e) {
            CalcPointArea();
        }

        private void CalcPointArea() {
            try {
                Contour[0].X = float.Parse(X1.Text);
                Contour[0].Y = float.Parse(Y1.Text);
                Contour[1].X = float.Parse(X2.Text);
                Contour[1].Y = float.Parse(Y2.Text);
                Contour[2].X = float.Parse(X3.Text);
                Contour[2].Y = float.Parse(Y3.Text);
                Contour[3].X = float.Parse(X4.Text);
                Contour[3].Y = float.Parse(Y4.Text);

                int diagonal = FindDiagonal();
                SwapPoint(2, diagonal);
                int inHull = InHull();

                Point[] vertex;
                if (inHull < 0) {
                    vertex = Contour;
                } else {
                    vertex = Contour.Where(x => x != Contour[inHull]).ToArray();
                }

                double area = PolygonArea(vertex);
                Area.Text = string.Format("Area: {0}, Mode: {1}", area, (inHull < 0) ? "Quad" : "Triangle");

                // draw to canvas
                double maxCanvasValue = Math.Min(PolyCanvas.ActualWidth, PolyCanvas.ActualHeight);
                double maxPointValue = Contour.Max(p => Math.Max(Math.Abs(p.X), Math.Abs(p.Y)));
                double scale = maxCanvasValue / maxPointValue / 2;

                Matrix transMatrix = new Matrix();
                transMatrix.Scale(scale, -scale);
                transMatrix.Translate(PolyCanvas.ActualWidth / 2, PolyCanvas.ActualHeight / 2);

                PolyCanvas.Children.Clear();
                DrawPolygon(new PointCollection(Contour.Select(p => p * transMatrix)), Brushes.Black, Brushes.Blue);
                DrawPolygon(new PointCollection(vertex.Select(p => p * transMatrix)), Brushes.Black, Brushes.Red);

            } catch (Exception e) {
                MessageBox.Show(e.Message, e.Message, MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private int FindDiagonal() {
            if (Contour.Distinct().ToArray().Length != Contour.Length) {
                throw new CollinearException("Duplicate points");
            }

            double k01 = Math.Atan2(Contour[1].Y - Contour[0].Y, Contour[1].X - Contour[0].X);
            double k02 = Math.Atan2(Contour[2].Y - Contour[0].Y, Contour[2].X - Contour[0].X);
            double k03 = Math.Atan2(Contour[3].Y - Contour[0].Y, Contour[3].X - Contour[0].X);

            Console.WriteLine("{0} {1} {2}", k01, k02, k03);

            if (k01 == k02 && k01 == k03) {
                throw new CollinearException("Four points collinear");
            }

            if (k01 == k02) {
                SwapPoint(0, 3);
                return FindDiagonal();
            }

            if (k01 == k03) {
                SwapPoint(0, 2);
                return FindDiagonal();
            }

            if (k02 == k03) {
                SwapPoint(0, 1);
                return FindDiagonal();
            }

            if ((k01 - k02) * (k01 - k03) < 0) {
                return 1;
            } else {
                if ((k02 - k01) * (k02 - k03) < 0) {
                    return 2;
                } else {
                    if ((k03 - k01) * (k03 - k02) < 0) {
                        return 3;
                    } else {
                        throw new CollinearException("Unexpected collinear case");
                    }
                }
            }
        }

        private void SwapPoint(int a, int b) {
            if (a == b) return;

            Point temp = Contour[a];
            Contour[a] = Contour[b];
            Contour[b] = temp;
        }

        private int InHull() {
            Vector vec02 = Contour[2] - Contour[0];
            Vector vec01 = Contour[1] - Contour[0];
            Vector vec03 = Contour[3] - Contour[0];

            Vector vec20 = Contour[0] - Contour[2];
            Vector vec21 = Contour[1] - Contour[2];
            Vector vec23 = Contour[3] - Contour[2];

            if ((Vector.AngleBetween(vec02, vec01) > 90) || (Vector.AngleBetween(vec02, vec03) > 90)) {
                return 0;
            } else {
                if ((Vector.AngleBetween(vec20, vec21) > 90) || (Vector.AngleBetween(vec20, vec23) > 90)) {
                    return 2;
                } else {
                    return -1;
                }
            }
        }

        private double PolygonArea(Point[] vertex) {
            int length = vertex.Length;
            double area = 0;

            for (int i = 0; i < length; i++) {
                int j = (i + 1) % length;
                area += vertex[i].X * vertex[j].Y - vertex[j].X * vertex[i].Y;
            }

            return Math.Abs(area / 2);
        }

        private void DrawPolygon(PointCollection points, Brush stroke, Brush fill) {
            Polygon p = new Polygon();
            p.Points = points;
            p.Stroke = stroke;
            p.Fill = fill;
            p.Opacity = 0.5;
            PolyCanvas.Children.Add(p);
        }
    }
}
