using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PotatoCAD
{
    public partial class MainWindow : Window
    {
        private Point _startPoint;
        private Point _endPoint;
        private bool _isDrawingLine;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DrawLineButton_Click(object sender, RoutedEventArgs e)
        {
            _isDrawingLine = true;
            MyViewport3D.MouseDown += Viewport3D_MouseDown;
            MyViewport3D.MouseUp += Viewport3D_MouseUp;
        }

        private void Viewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawingLine)
            {
                _startPoint = e.GetPosition(MyViewport3D);
            }
        }

        private void Viewport3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawingLine)
            {
                _endPoint = e.GetPosition(MyViewport3D);
                DrawLine3D(_startPoint, _endPoint);
                _isDrawingLine = false;
                MyViewport3D.MouseDown -= Viewport3D_MouseDown;
                MyViewport3D.MouseUp -= Viewport3D_MouseUp;
            }
        }

        private void DrawLine3D(Point start, Point end)
        {
            Point3D start3D = ConvertScreenPointTo3D(start);
            Point3D end3D = ConvertScreenPointTo3D(end);

            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(start3D);
            mesh.Positions.Add(end3D);

            // Add line thickness by creating a small rectangle around the line
            mesh.Positions.Add(new Point3D(start3D.X, start3D.Y + 0.01, start3D.Z));
            mesh.Positions.Add(new Point3D(end3D.X, end3D.Y + 0.01, end3D.Z));

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);

            GeometryModel3D lineModel = new GeometryModel3D
            {
                Geometry = mesh,
                Material = new DiffuseMaterial(Brushes.Black)
            };

            ModelContainer.Content = lineModel;
        }

        private Point3D ConvertScreenPointTo3D(Point point)
        {
            return new Point3D(point.X / MyViewport3D.ActualWidth * 2 - 1, -(point.Y / MyViewport3D.ActualHeight * 2 - 1), 0);
        }

        private void ExtrudeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModelContainer.Content is GeometryModel3D lineModel)
            {
                MeshGeometry3D mesh = (MeshGeometry3D)lineModel.Geometry;

                // Extrude the 2D line to a 3D object
                Point3DCollection extrudedPositions = new Point3DCollection();
                foreach (Point3D point in mesh.Positions)
                {
                    extrudedPositions.Add(point);
                    extrudedPositions.Add(new Point3D(point.X, point.Y, point.Z + 1)); // Extrude along Z axis
                }

                mesh.Positions = extrudedPositions;
                mesh.TriangleIndices.Clear();

                // Add indices for the new extruded faces
                for (int i = 0; i < mesh.Positions.Count / 2 - 1; i++)
                {
                    int topLeft = i * 2;
                    int topRight = i * 2 + 2;
                    int bottomLeft = i * 2 + 1;
                    int bottomRight = i * 2 + 3;

                    mesh.TriangleIndices.Add(topLeft);
                    mesh.TriangleIndices.Add(bottomLeft);
                    mesh.TriangleIndices.Add(topRight);

                    mesh.TriangleIndices.Add(bottomLeft);
                    mesh.TriangleIndices.Add(bottomRight);
                    mesh.TriangleIndices.Add(topRight);
                }

                lineModel.Geometry = mesh;
            }
        }
    }
}
