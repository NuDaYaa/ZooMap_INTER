using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ZooMap.Database;
using ZooMap.Models;

namespace ZooMap
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper dbHelper;
        private User currentUser;
        private Point scrollStartPoint;
        private Point scrollStartOffset;
        private bool isPanning;
        private double currentZoom = 1.0;
        private const double ZoomStep = 0.1;
        private const double MinZoomBase = 0.5;
        private const double MaxZoom = 2.0;
        private double minZoomDynamic = MinZoomBase;
        private const double MapWidth = 2000;
        private const double MapHeight = 2000;

        public MainWindow()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadEnclosures();
            Loaded += MainWindow_Loaded;
            scrollViewer.SizeChanged += ScrollViewer_SizeChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMinZoom();
            ClampScrollOffsets();
        }

        private void LoadEnclosures()
        {
            iconsCanvas.Children.Clear();

            // Тестовые данные вместо БД
            CreateEnclosureIcon("Львиный вольер", 300, 400, Colors.Red);
            CreateEnclosureIcon("Обезьянник", 600, 700, Colors.Green);
            CreateEnclosureIcon("Аквариум", 800, 300, Colors.Blue);
        }

        private void CreateEnclosureIcon(string name, double x, double y, Color color)
        {
            Button iconButton = new Button
            {
                Width = 40,
                Height = 40,
                Background = new SolidColorBrush(Colors.Transparent),
                BorderBrush = new SolidColorBrush(color),
                BorderThickness = new Thickness(2),
                Content = new Ellipse
                {
                    Width = 30,
                    Height = 30,
                    Fill = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B)),
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 2
                },
                ToolTip = name
            };

            Canvas.SetLeft(iconButton, x - 20);
            Canvas.SetTop(iconButton, y - 20);

            iconButton.Click += (s, e) => ShowEnclosureInfo(name);
            iconsCanvas.Children.Add(iconButton);
        }

        private void ShowEnclosureInfo(string enclosureName)
        {
            EnclosureInfoWindow infoWindow = new EnclosureInfoWindow(enclosureName, currentUser);
            infoWindow.Owner = this;
            infoWindow.ShowDialog();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                currentUser = loginWindow.AuthenticatedUser;
                UpdateUI();
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow regWindow = new RegistrationWindow();
            regWindow.Owner = this;
            regWindow.ShowDialog();
        }

        private void BtnAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser?.Role == "Admin")
            {
                AdminPanelWindow adminWindow = new AdminPanelWindow();
                adminWindow.Owner = this;
                adminWindow.ShowDialog();
            }
        }

        private void UpdateUI()
        {
            if (currentUser != null)
            {
                tbUserInfo.Text = $"Пользователь: {currentUser.Username} ({currentUser.Role})";
                btnLogin.Content = "Выйти";

                if (currentUser.Role == "Admin" || currentUser.Role == "Worker")
                {
                    btnAdminPanel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                tbUserInfo.Text = "";
                btnLogin.Content = "Вход";
                btnAdminPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustZoom(ZoomStep);
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustZoom(-ZoomStep);
        }

        private void AdjustZoom(double delta)
        {
            UpdateMinZoom();

            double newZoom = Math.Clamp(currentZoom + delta, minZoomDynamic, MaxZoom);

            if (Math.Abs(newZoom - currentZoom) < double.Epsilon)
            {
                return;
            }

            double viewportCenterX = scrollViewer.HorizontalOffset + scrollViewer.ViewportWidth / 2;
            double viewportCenterY = scrollViewer.VerticalOffset + scrollViewer.ViewportHeight / 2;
            double relativeCenterX = viewportCenterX / (MapWidth * currentZoom);
            double relativeCenterY = viewportCenterY / (MapHeight * currentZoom);

            currentZoom = newZoom;
            mapScaleTransform.ScaleX = currentZoom;
            mapScaleTransform.ScaleY = currentZoom;

            double targetCenterX = relativeCenterX * MapWidth * currentZoom;
            double targetCenterY = relativeCenterY * MapHeight * currentZoom;

            ScrollToClampedOffsets(targetCenterX - scrollViewer.ViewportWidth / 2, targetCenterY - scrollViewer.ViewportHeight / 2);
        }

        private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsIconClick(e.OriginalSource))
            {
                return;
            }

            scrollStartPoint = e.GetPosition(scrollViewer);
            scrollStartOffset = new Point(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
            isPanning = true;
            scrollViewer.CaptureMouse();
            e.Handled = true;
        }

        private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!isPanning)
            {
                return;
            }

            Point currentPoint = e.GetPosition(scrollViewer);
            Vector delta = currentPoint - scrollStartPoint;

            ScrollToClampedOffsets(scrollStartOffset.X - delta.X, scrollStartOffset.Y - delta.Y);
            e.Handled = true;
        }

        private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopPanning();
            e.Handled = true;
        }

        private void ScrollViewer_MouseLeave(object sender, MouseEventArgs e)
        {
            StopPanning();
        }

        private void StopPanning()
        {
            if (!isPanning)
            {
                return;
            }

            isPanning = false;
            scrollViewer.ReleaseMouseCapture();
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMinZoom();
            ClampScrollOffsets();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ClampScrollOffsets();
        }

        private void ScrollToClampedOffsets(double desiredOffsetX, double desiredOffsetY)
        {
            double scaledWidth = MapWidth * currentZoom;
            double scaledHeight = MapHeight * currentZoom;

            double maxOffsetX = Math.Max(0, scaledWidth - scrollViewer.ViewportWidth);
            double maxOffsetY = Math.Max(0, scaledHeight - scrollViewer.ViewportHeight);

            double clampedX = Math.Clamp(desiredOffsetX, 0, maxOffsetX);
            double clampedY = Math.Clamp(desiredOffsetY, 0, maxOffsetY);

            scrollViewer.ScrollToHorizontalOffset(clampedX);
            scrollViewer.ScrollToVerticalOffset(clampedY);
        }

        private void ClampScrollOffsets()
        {
            ScrollToClampedOffsets(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
        }

        private void UpdateMinZoom()
        {
            if (scrollViewer.ViewportWidth <= 0 || scrollViewer.ViewportHeight <= 0)
            {
                return;
            }

            double widthRatio = scrollViewer.ViewportWidth / MapWidth;
            double heightRatio = scrollViewer.ViewportHeight / MapHeight;
            double minZoomByViewport = Math.Max(widthRatio, heightRatio);

            minZoomDynamic = Math.Max(MinZoomBase, minZoomByViewport);

            if (currentZoom < minZoomDynamic)
            {
                currentZoom = minZoomDynamic;
                mapScaleTransform.ScaleX = currentZoom;
                mapScaleTransform.ScaleY = currentZoom;
            }
        }

        private bool IsIconClick(object originalSource)
        {
            DependencyObject? current = originalSource as DependencyObject;

            while (current != null)
            {
                if (current is Button)
                {
                    return true;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }
    }
}
