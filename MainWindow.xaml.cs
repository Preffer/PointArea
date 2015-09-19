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
                bool convex = IsConvex();

                double area = 0;
                if (convex) {
                    area = AreaQuad();
                } else {
                    area = AreaTriangle();
                }

                Area.Text = string.Format("Area: {0}", area);
            } catch (Exception e) {
                MessageBox.Show(e.Message, e.Message, MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private int FindDiagonal() {
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

        private bool IsConvex() {
            Vector vec01 = Contour[1] - Contour[0];
            Vector vec03 = Contour[3] - Contour[0];
            Vector vec21 = Contour[1] - Contour[2];
            Vector vec23 = Contour[3] - Contour[2];

            if ((Vector.AngleBetween(vec01, vec03) <= 90) && (Vector.AngleBetween(vec21, vec23) <= 90)) {
                return true;
            } else {
                return false;
            }
        }

        private double AreaQuad() {
            return AreaPoly(Contour);
        }

        private double AreaTriangle() {
            return AreaPoly(Contour.Where(x => x != Contour[2]).ToArray());
        }

        private double AreaPoly(Point[] vertex) {
            int length = vertex.Length;
            double area = 0;

            for (int i = 0; i < length; i++) {
                int j = (i + 1) % length;
                area += vertex[i].X * vertex[j].Y - vertex[j].X * vertex[i].Y;
            }

            return Math.Abs(area / 2);
        }
    }
}
