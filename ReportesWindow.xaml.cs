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
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.IO;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.Collections;

namespace SalonBellezaApp
{
    /// <summary>
    /// Lógica de interacción para ReportesWindow.xaml
    /// </summary>
    public partial class ReportesWindow : Window
    {
        private readonly ReporteService _reporteService;
        private TabControl _tabControl;

        
        private Button _btnGenerar;
        private Button _btnExport;
        private TextBlock _lblStatus;
        private TextBlock _lblRange;
        private ProgressBar _progressBar;

       
        private static List<ReporteIngreso> _cachedIngresos;
        private static List<ReporteServicio> _cachedServicios;
        private static List<ReporteEmpleado> _cachedEmpleados;
        private static List<ReporteCliente> _cachedClientes;
        private static List<ReporteProductoUsado> _cachedProductos;
        private static List<ReporteCategoriaStock> _cachedStockCategorias;
        private static List<ReporteMovimientoInventario> _cachedMovimientos;
        private static DateTime? _cachedFechaInicio;
        private static DateTime? _cachedFechaFin;

        public ReportesWindow()
        {
            InitializeComponent();
            _reporteService = new ReporteService();
            CreateModernUI();

            
            RestoreCachedReports();
        }

        private void RestoreCachedReports()
        {
            try
            {
                if (_cachedFechaInicio.HasValue && _cachedFechaFin.HasValue)
                {
                    var dpInicio = (DatePicker)this.FindName("dpFechaInicio");
                    var dpFin = (DatePicker)this.FindName("dpFechaFin");
                    if (dpInicio != null) dpInicio.SelectedDate = _cachedFechaInicio.Value;
                    if (dpFin != null) dpFin.SelectedDate = _cachedFechaFin.Value;

                    if (_lblRange != null)
                        _lblRange.Text = $"Rango: {_cachedFechaInicio:dd/MM/yyyy} - {_cachedFechaFin:dd/MM/yyyy}";
                }

                if (_cachedIngresos != null)
                {
                    var dg = (DataGrid)this.FindName("dgIngresos");
                    if (dg != null) dg.ItemsSource = _cachedIngresos;
                }
                if (_cachedServicios != null)
                {
                    var dg = (DataGrid)this.FindName("dgServicios");
                    if (dg != null) dg.ItemsSource = _cachedServicios;
                }
                if (_cachedEmpleados != null)
                {
                    var dg = (DataGrid)this.FindName("dgEmpleados");
                    if (dg != null) dg.ItemsSource = _cachedEmpleados;
                }
                if (_cachedClientes != null)
                {
                    var dg = (DataGrid)this.FindName("dgClientes");
                    if (dg != null) dg.ItemsSource = _cachedClientes;
                }
                if (_cachedProductos != null)
                {
                    var dg = (DataGrid)this.FindName("dgProductos");
                    if (dg != null) dg.ItemsSource = _cachedProductos;
                }
                if (_cachedStockCategorias != null)
                {
                    var dg = (DataGrid)this.FindName("dgStockCategoria");
                    if (dg != null) dg.ItemsSource = _cachedStockCategorias;
                }
                if (_cachedMovimientos != null)
                {
                    var dg = (DataGrid)this.FindName("dgMovimientos");
                    if (dg != null) dg.ItemsSource = _cachedMovimientos.Take(500).ToList();
                }
            }
            catch { }
        }

        private void CreateModernUI()
        {
            this.Title = "Bella Vista - Reportes";
            this.Width = 1400;
            this.Height = 900;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.WindowState = WindowState.Maximized;

            LinearGradientBrush backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(1, 1);
            backgroundBrush.GradientStops.Add(new GradientStop(Color.FromRgb(102, 126, 234), 0));
            backgroundBrush.GradientStops.Add(new GradientStop(Color.FromRgb(118, 75, 162), 1));
            this.Background = backgroundBrush;

            Grid mainGrid = new Grid();
            this.Content = mainGrid;

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            CreateHeader(mainGrid);
            CreateContent(mainGrid);
        }

        private void CreateHeader(Grid mainGrid)
        {
            Border headerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(250, 255, 255, 255)),
                CornerRadius = new CornerRadius(20, 20, 0, 0),
                Margin = new Thickness(20, 20, 20, 0)
            };

            DropShadowEffect headerShadow = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 3,
                Opacity = 0.2,
                BlurRadius = 10
            };
            headerBorder.Effect = headerShadow;

            Grid headerGrid = new Grid();
            headerBorder.Child = headerGrid;

            
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(30, 0, 0, 0)
            };

            TextBlock titleIcon = new TextBlock
            {
                Text = "\uD83D\uDCCA",
                FontSize = 32,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock titleText = new TextBlock
            {
                Text = "Reportes",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                VerticalAlignment = VerticalAlignment.Center
            };

            titlePanel.Children.Add(titleIcon);
            titlePanel.Children.Add(titleText);

            TextBlock userText = new TextBlock
            {
                Text = $"Usuario: {Environment.UserName}",
                FontSize = 13,
                Foreground = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0,0,20,0)
            };

            Border volverBorder = new Border
            {
                CornerRadius = new CornerRadius(20),
                Background = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                Margin = new Thickness(0, 0, 30, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            Button volverButton = new Button
            {
                Content = "← Volver al Dashboard",
                Width = 200,
                Height = 40,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Cursor = Cursors.Hand
            };

            volverButton.Click += VolverButton_Click;
            volverBorder.Child = volverButton;

            Grid.SetColumn(titlePanel, 0);
            Grid.SetColumn(userText, 1);
            Grid.SetColumn(volverBorder, 2);
            headerGrid.Children.Add(titlePanel);
            headerGrid.Children.Add(userText);
            headerGrid.Children.Add(volverBorder);

            Grid.SetRow(headerBorder, 0);
            mainGrid.Children.Add(headerBorder);
        }

        private void CreateContent(Grid mainGrid)
        {
            Border contentBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(250, 255, 255, 255)),
                CornerRadius = new CornerRadius(0, 0, 20, 20),
                Margin = new Thickness(20, 0, 20, 20)
            };

            DropShadowEffect contentShadow = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 5,
                Opacity = 0.2,
                BlurRadius = 15
            };
            contentBorder.Effect = contentShadow;

            Grid contentGrid = new Grid();
            contentBorder.Child = contentGrid;

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // status bar

            UIElement filterPanel = CreateFilterPanel();
            Grid.SetRow(filterPanel, 0);
            contentGrid.Children.Add(filterPanel);

            _tabControl = new TabControl
            {
                Margin = new Thickness(20),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0)
            };

            _tabControl.Items.Add(CreateTabIngresos());
            _tabControl.Items.Add(CreateTabServicios());
            _tabControl.Items.Add(CreateTabEmpleados());
            _tabControl.Items.Add(CreateTabClientes());
            _tabControl.Items.Add(CreateTabProductos());
            _tabControl.Items.Add(CreateTabInventario());
            _tabControl.Items.Add(CreateTabComparacion());

            this.RegisterName("tabReportes", _tabControl);

            Grid.SetRow(_tabControl, 1);
            contentGrid.Children.Add(_tabControl);

          
            var statusBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(230, 250, 250, 250)),
                Padding = new Thickness(10),
                Margin = new Thickness(20, 10, 20, 20),
                CornerRadius = new CornerRadius(8)
            };
            var statusGrid = new Grid();
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _lblStatus = new TextBlock { Text = "Estado: listo", VerticalAlignment = VerticalAlignment.Center };
            _lblRange = new TextBlock { Text = "Rango: -", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10,0,10,0) };
            var lblUser = new TextBlock { Text = $"Usuario: {Environment.UserName}", VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.Gray };
            _progressBar = new ProgressBar { Width = 140, Height = 16, IsIndeterminate = true, Visibility = Visibility.Collapsed, Margin = new Thickness(10,0,0,0) };

            Grid.SetColumn(_lblStatus, 0);
            Grid.SetColumn(_lblRange, 1);
            Grid.SetColumn(lblUser, 2);
            Grid.SetColumn(_progressBar, 3);

            statusGrid.Children.Add(_lblStatus);
            statusGrid.Children.Add(_lblRange);
            statusGrid.Children.Add(lblUser);
            statusGrid.Children.Add(_progressBar);

            statusBorder.Child = statusGrid;
            Grid.SetRow(statusBorder, 2);
            contentGrid.Children.Add(statusBorder);

            Grid.SetRow(contentBorder, 1);
            mainGrid.Children.Add(contentBorder);
        }

        private Border CreateFilterPanel()
        {
            Border container = new Border
            {
                Background = Brushes.White,
                Padding = new Thickness(15),
                Margin = new Thickness(20, 15, 20, 0),
                CornerRadius = new CornerRadius(8)
            };

            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock lblFechaInicio = new TextBlock
            {
                Text = "Desde:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.Medium
            };

            DatePicker dpFechaInicio = new DatePicker
            {
                Name = "dpFechaInicio",
                Width = 150,
                Height = 35,
                Margin = new Thickness(0, 0, 30, 0),
                SelectedDate = DateTime.Now.AddMonths(-1)
            };
            this.RegisterName("dpFechaInicio", dpFechaInicio);

            TextBlock lblFechaFin = new TextBlock
            {
                Text = "Hasta:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.Medium
            };

            DatePicker dpFechaFin = new DatePicker
            {
                Name = "dpFechaFin",
                Width = 150,
                Height = 35,
                Margin = new Thickness(0, 0, 20, 0),
                SelectedDate = DateTime.Now
            };
            this.RegisterName("dpFechaFin", dpFechaFin);

            _btnGenerar = new Button
            {
                Content = "\uD83D\uDD04 Generar Reportes",
                Width = 160,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Margin = new Thickness(0,0,10,0)
            };
            _btnGenerar.Click += BtnGenerar_Click;

            _btnExport = new Button
            {
                Content = "\uD83D\uDCC4 Exportar a Excel",
                Width = 160,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand
            };
            _btnExport.Click += BtnExport_Click;

            panel.Children.Add(lblFechaInicio);
            panel.Children.Add(dpFechaInicio);
            panel.Children.Add(lblFechaFin);
            panel.Children.Add(dpFechaFin);
            panel.Children.Add(_btnGenerar);
            panel.Children.Add(_btnExport);

            container.Child = panel;
            return container;
        }

        private void AttachFilterBox(StackPanel parentPanel, DataGrid grid, string name)
        {
            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            TextBlock lbl = new TextBlock { Text = "Filtro:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,8,0) };
            TextBox txt = new TextBox { Name = name, Width = 250, Height = 28 };
            txt.TextChanged += (s, e) => ApplyFilter(grid, txt.Text);
            this.RegisterName(name, txt);
            sp.Children.Add(lbl);
            sp.Children.Add(txt);
            parentPanel.Children.Insert(0, sp);
        }

        private void ApplyFilter(DataGrid grid, string filterText)
        {
            if (grid.ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
            if (string.IsNullOrWhiteSpace(filterText))
            {
                view.Filter = null;
                return;
            }

            string term = filterText.Trim().ToLowerInvariant();
            view.Filter = obj =>
            {
                if (obj == null) return false;
                foreach (var prop in obj.GetType().GetProperties())
                {
                    var val = prop.GetValue(obj);
                    if (val != null && val.ToString().ToLowerInvariant().Contains(term))
                        return true;
                }
                return false;
            };
        }

        private TabItem CreateTabIngresos()
        {
            TabItem tab = new TabItem { Header = "\uD83D\uDCB0 Ingresos por Período" };

            ScrollViewer scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel panel = new StackPanel { Margin = new Thickness(15) };
            scrollViewer.Content = panel;

            DataGrid dgIngresos = new DataGrid
            {
                Name = "dgIngresos",
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Height = 400
            };

            dgIngresos.Columns.Add(new DataGridTextColumn { Header = "Fecha", Binding = new Binding("Fecha") { StringFormat = "dd/MM/yyyy" }, Width = new DataGridLength(120) });
            dgIngresos.Columns.Add(new DataGridTextColumn { Header = "Total Pagos", Binding = new Binding("TotalPagos"), Width = new DataGridLength(100) });
            dgIngresos.Columns.Add(new DataGridTextColumn { Header = "Monto Total", Binding = new Binding("MontoTotal") { StringFormat = "C2" }, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

            this.RegisterName("dgIngresos", dgIngresos);
            panel.Children.Add(dgIngresos);

            
            AttachFilterBox(panel, dgIngresos, "txtFilterIngresos");

            tab.Content = scrollViewer;
            return tab;
        }

        private TabItem CreateTabServicios()
        {
            TabItem tab = new TabItem { Header = "✂️ Servicios Populares" };

            ScrollViewer scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel panel = new StackPanel { Margin = new Thickness(15) };
            scrollViewer.Content = panel;

            DataGrid dgServicios = new DataGrid
            {
                Name = "dgServicios",
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Height = 400
            };

            dgServicios.Columns.Add(new DataGridTextColumn { Header = "Servicio", Binding = new Binding("NombreServicio"), Width = new DataGridLength(200) });
            dgServicios.Columns.Add(new DataGridTextColumn { Header = "Total Ventas", Binding = new Binding("TotalVentas"), Width = new DataGridLength(100) });
            dgServicios.Columns.Add(new DataGridTextColumn { Header = "Precio", Binding = new Binding("Precio") { StringFormat = "C2" }, Width = new DataGridLength(100) });
            dgServicios.Columns.Add(new DataGridTextColumn { Header = "Monto Total", Binding = new Binding("MontoTotal") { StringFormat = "C2" }, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

            this.RegisterName("dgServicios", dgServicios);
            panel.Children.Add(dgServicios);

            AttachFilterBox(panel, dgServicios, "txtFilterServicios");

            tab.Content = scrollViewer;
            return tab;
        }

        private TabItem CreateTabEmpleados()
        {
            TabItem tab = new TabItem { Header = "\uD83D\uDC64 Empleados" };

            ScrollViewer scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel panel = new StackPanel { Margin = new Thickness(15) };
            scrollViewer.Content = panel;

            DataGrid dgEmpleados = new DataGrid
            {
                Name = "dgEmpleados",
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Height = 400
            };

            dgEmpleados.Columns.Add(new DataGridTextColumn { Header = "Nombre", Binding = new Binding("NombreCompleto"), Width = new DataGridLength(200) });
            dgEmpleados.Columns.Add(new DataGridTextColumn { Header = "Total Citas", Binding = new Binding("TotalCitas"), Width = new DataGridLength(100) });
            dgEmpleados.Columns.Add(new DataGridTextColumn { Header = "Completadas", Binding = new Binding("CitasCompletadas"), Width = new DataGridLength(100) });
            dgEmpleados.Columns.Add(new DataGridTextColumn { Header = "Ingresos", Binding = new Binding("MontoTotal") { StringFormat = "C2" }, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

            this.RegisterName("dgEmpleados", dgEmpleados);
            panel.Children.Add(dgEmpleados);

            AttachFilterBox(panel, dgEmpleados, "txtFilterEmpleados");

            tab.Content = scrollViewer;
            return tab;
        }

        private TabItem CreateTabClientes()
        {
            TabItem tab = new TabItem { Header = "\uD83E\uDDD1 Clientes" };

            ScrollViewer scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel panel = new StackPanel { Margin = new Thickness(15) };
            scrollViewer.Content = panel;

            DataGrid dgClientes = new DataGrid
            {
                Name = "dgClientes",
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Height = 400
            };

            dgClientes.Columns.Add(new DataGridTextColumn { Header = "Nombre", Binding = new Binding("NombreCompleto"), Width = new DataGridLength(200) });
            dgClientes.Columns.Add(new DataGridTextColumn { Header = "Total Citas", Binding = new Binding("TotalCitas"), Width = new DataGridLength(100) });
            dgClientes.Columns.Add(new DataGridTextColumn { Header = "Monto Gastado", Binding = new Binding("MontoGastado") { StringFormat = "C2" }, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

            this.RegisterName("dgClientes", dgClientes);
            panel.Children.Add(dgClientes);

            AttachFilterBox(panel, dgClientes, "txtFilterClientes");

            tab.Content = scrollViewer;
            return tab;
        }

        private TabItem CreateTabProductos()
        {
            TabItem tab = new TabItem { Header = "\uD83D\uDCE6 Productos" };

            ScrollViewer scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel panel = new StackPanel { Margin = new Thickness(15) };
            scrollViewer.Content = panel;

            DataGrid dgProductos = new DataGrid
            {
                Name = "dgProductos",
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Height = 400
            };

            dgProductos.Columns.Add(new DataGridTextColumn { Header = "Producto", Binding = new Binding("NombreProducto"), Width = new DataGridLength(200) });
            dgProductos.Columns.Add(new DataGridTextColumn { Header = "Categoría", Binding = new Binding("Categoria"), Width = new DataGridLength(120) });
            dgProductos.Columns.Add(new DataGridTextColumn { Header = "Servicios", Binding = new Binding("ServiciosQueUsan"), Width = new DataGridLength(100) });
            dgProductos.Columns.Add(new DataGridTextColumn { Header = "Cantidad Usada", Binding = new Binding("CantidadUsada") { StringFormat = "N2" }, Width = new DataGridLength(100) });
            dgProductos.Columns.Add(new DataGridTextColumn { Header = "Stock Actual", Binding = new Binding("StockActual"), Width = new DataGridLength(100) });

            this.RegisterName("dgProductos", dgProductos);
            panel.Children.Add(dgProductos);

            AttachFilterBox(panel, dgProductos, "txtFilterProductos");

            tab.Content = scrollViewer;
            return tab;
        }

        private TabItem CreateTabInventario()
        {
            TabItem tab = new TabItem { Header = "\uD83D\uDCCA Inventario" };

            ScrollViewer scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel panel = new StackPanel { Margin = new Thickness(15) };
            scrollViewer.Content = panel;

            TextBlock lblStockCategoria = new TextBlock
            {
                Text = "\uD83D\uDCE6 Stock por Categoría",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(lblStockCategoria);

            DataGrid dgStockCategoria = new DataGrid
            {
                Name = "dgStockCategoria",
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Height = 200
            };

            dgStockCategoria.Columns.Add(new DataGridTextColumn { Header = "Categoría", Binding = new Binding("Categoria"), Width = new DataGridLength(150) });
            dgStockCategoria.Columns.Add(new DataGridTextColumn { Header = "Productos", Binding = new Binding("TotalProductos"), Width = new DataGridLength(100) });
            dgStockCategoria.Columns.Add(new DataGridTextColumn { Header = "Stock Total", Binding = new Binding("StockTotal"), Width = new DataGridLength(100) });
            dgStockCategoria.Columns.Add(new DataGridTextColumn { Header = "Bajo Stock", Binding = new Binding("ProductosBajoStock"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

            this.RegisterName("dgStockCategoria", dgStockCategoria);
            panel.Children.Add(dgStockCategoria);

            panel.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            TextBlock lblMovimientos = new TextBlock
            {
                Text = "\uD83D\uDCCB Movimientos de Inventario",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(lblMovimientos);

            DataGrid dgMovimientos = new DataGrid
            {
                Name = "dgMovimientos",
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Height = 300
            };

            dgMovimientos.Columns.Add(new DataGridTextColumn { Header = "Fecha", Binding = new Binding("FechaMovimiento") { StringFormat = "dd/MM/yyyy HH:mm" }, Width = new DataGridLength(150) });
            dgMovimientos.Columns.Add(new DataGridTextColumn { Header = "Producto", Binding = new Binding("NombreProducto"), Width = new DataGridLength(150) });
            dgMovimientos.Columns.Add(new DataGridTextColumn { Header = "Tipo", Binding = new Binding("TipoMovimiento"), Width = new DataGridLength(100) });
            dgMovimientos.Columns.Add(new DataGridTextColumn { Header = "Cantidad", Binding = new Binding("Cantidad") { StringFormat = "N2" }, Width = new DataGridLength(80) });
            dgMovimientos.Columns.Add(new DataGridTextColumn { Header = "Motivo", Binding = new Binding("Motivo"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

            this.RegisterName("dgMovimientos", dgMovimientos);
            panel.Children.Add(dgMovimientos);

            AttachFilterBox(panel, dgMovimientos, "txtFilterMovimientos");

            tab.Content = scrollViewer;
            return tab;
        }

        private TabItem CreateTabComparacion()
        {
            TabItem tab = new TabItem { Header = "\uD83D\uDCC8 Comparación" };

            ScrollViewer scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel panel = new StackPanel { Margin = new Thickness(15) };
            scrollViewer.Content = panel;

            StackPanel panelSeleccion = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 20) };

            TextBlock lbl1 = new TextBlock { Text = "Período 1:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            DatePicker dpPeriodo1Inicio = new DatePicker { Name = "dpPeriodo1Inicio", Width = 120, Margin = new Thickness(0, 0, 5, 0), SelectedDate = DateTime.Now.AddMonths(-2) };
            DatePicker dpPeriodo1Fin = new DatePicker { Name = "dpPeriodo1Fin", Width = 120, Margin = new Thickness(0, 0, 20, 0), SelectedDate = DateTime.Now.AddMonths(-1) };

            TextBlock lbl2 = new TextBlock { Text = "Período 2:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            DatePicker dpPeriodo2Inicio = new DatePicker { Name = "dpPeriodo2Inicio", Width = 120, Margin = new Thickness(0, 0, 5, 0), SelectedDate = DateTime.Now.AddMonths(-1) };
            DatePicker dpPeriodo2Fin = new DatePicker { Name = "dpPeriodo2Fin", Width = 120, Margin = new Thickness(0, 0, 20, 0), SelectedDate = DateTime.Now };

            Button btnCompara = new Button
            {
                Content = "\uD83D\uDCCA Comparar",
                Width = 120,
                Height = 35,
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            btnCompara.Click += BtnCompara_Click;

            this.RegisterName("dpPeriodo1Inicio", dpPeriodo1Inicio);
            this.RegisterName("dpPeriodo1Fin", dpPeriodo1Fin);
            this.RegisterName("dpPeriodo2Inicio", dpPeriodo2Inicio);
            this.RegisterName("dpPeriodo2Fin", dpPeriodo2Fin);

            panelSeleccion.Children.Add(lbl1);
            panelSeleccion.Children.Add(dpPeriodo1Inicio);
            panelSeleccion.Children.Add(dpPeriodo1Fin);
            panelSeleccion.Children.Add(lbl2);
            panelSeleccion.Children.Add(dpPeriodo2Inicio);
            panelSeleccion.Children.Add(dpPeriodo2Fin);
            panelSeleccion.Children.Add(btnCompara);

            panel.Children.Add(panelSeleccion);

            TextBlock lblResultado = new TextBlock
            {
                Name = "lblComparacion",
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 10, 0, 0)
            };
            this.RegisterName("lblComparacion", lblResultado);
            panel.Children.Add(lblResultado);

            tab.Content = scrollViewer;
            return tab;
        }

        private async void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                _btnGenerar.IsEnabled = false;
                _btnExport.IsEnabled = false;
                _progressBar.Visibility = Visibility.Visible;
                _lblStatus.Text = "Generando reportes...";
                Mouse.OverrideCursor = Cursors.Wait;

                DatePicker dpInicio = (DatePicker)this.FindName("dpFechaInicio");
                DatePicker dpFin = (DatePicker)this.FindName("dpFechaFin");

                DateTime fechaInicio = dpInicio.SelectedDate ?? DateTime.Now.AddMonths(-1);
                DateTime fechaFin = dpFin.SelectedDate ?? DateTime.Now;

                var ingresos = await _reporteService.ObtenerIngresosPorPeriodoAsync(fechaInicio, fechaFin);
                DataGrid dgIngresos = (DataGrid)this.FindName("dgIngresos");
                if (dgIngresos != null) dgIngresos.ItemsSource = ingresos;

                var servicios = await _reporteService.ObtenerServiciosMasPopularesAsync(fechaInicio, fechaFin);
                DataGrid dgServicios = (DataGrid)this.FindName("dgServicios");
                if (dgServicios != null) dgServicios.ItemsSource = servicios;

                var empleados = await _reporteService.ObtenerDesempenoEmpleadosAsync(fechaInicio, fechaFin);
                DataGrid dgEmpleados = (DataGrid)this.FindName("dgEmpleados");
                if (dgEmpleados != null) dgEmpleados.ItemsSource = empleados;

                var clientes = await _reporteService.ObtenerClientesFrecuentesAsync(fechaInicio, fechaFin, 10);
                DataGrid dgClientes = (DataGrid)this.FindName("dgClientes");
                if (dgClientes != null) dgClientes.ItemsSource = clientes;

                var productos = await _reporteService.ObtenerProductosMasUsadosAsync(fechaInicio, fechaFin, 10);
                DataGrid dgProductos = (DataGrid)this.FindName("dgProductos");
                if (dgProductos != null) dgProductos.ItemsSource = productos;

                var stockCategoria = await _reporteService.ObtenerStockPorCategoriaAsync();
                DataGrid dgStockCategoria = (DataGrid)this.FindName("dgStockCategoria");
                if (dgStockCategoria != null) dgStockCategoria.ItemsSource = stockCategoria;

                var movimientos = await _reporteService.ObtenerMovimientosInventarioAsync(fechaInicio, fechaFin);
                DataGrid dgMovimientos = (DataGrid)this.FindName("dgMovimientos");
                if (dgMovimientos != null) dgMovimientos.ItemsSource = movimientos.Take(500).ToList();

                
                _cachedIngresos = ingresos;
                _cachedServicios = servicios;
                _cachedEmpleados = empleados;
                _cachedClientes = clientes;
                _cachedProductos = productos;
                _cachedStockCategorias = stockCategoria;
                _cachedMovimientos = movimientos;
                _cachedFechaInicio = fechaInicio;
                _cachedFechaFin = fechaFin;

                _lblStatus.Text = "Reportes generados correctamente.";
                _lblRange.Text = $"Rango: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";

                MessageBox.Show("Reportes generados exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reportes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _lblStatus.Text = "Error al generar reportes";
            }
            finally
            {
                _btnGenerar.IsEnabled = true;
                _btnExport.IsEnabled = true;
                _progressBar.Visibility = Visibility.Collapsed;
                Mouse.OverrideCursor = null;
            }
        }

        private async void BtnCompara_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DatePicker dpP1I = (DatePicker)this.FindName("dpPeriodo1Inicio");
                DatePicker dpP1F = (DatePicker)this.FindName("dpPeriodo1Fin");
                DatePicker dpP2I = (DatePicker)this.FindName("dpPeriodo2Inicio");
                DatePicker dpP2F = (DatePicker)this.FindName("dpPeriodo2Fin");

                DateTime periodo1Inicio = dpP1I.SelectedDate ?? DateTime.Now.AddMonths(-2);
                DateTime periodo1Fin = dpP1F.SelectedDate ?? DateTime.Now.AddMonths(-1);
                DateTime periodo2Inicio = dpP2I.SelectedDate ?? DateTime.Now.AddMonths(-1);
                DateTime periodo2Fin = dpP2F.SelectedDate ?? DateTime.Now;

                var comparacion = await _reporteService.ObtenerComparacionPeriodosAsync(periodo1Inicio, periodo1Fin, periodo2Inicio, periodo2Fin);

                TextBlock lblComparacion = (TextBlock)this.FindName("lblComparacion");
                if (lblComparacion != null)
                {
                    lblComparacion.Text = $"\n\uD83D\uDCCA COMPARACIÓN DE PERÍODOS\n\n▶ Período 1: {comparacion.Periodo1?.FechaInicio:dd/MM/yyyy} - {comparacion.Periodo1?.FechaFin:dd/MM/yyyy}\n   • Ingresos: {comparacion.Periodo1?.IngresoTotal:C2}\n   • Total Pagos: {comparacion.Periodo1?.TotalPagos}\n   • Total Citas: {comparacion.Periodo1?.TotalCitas}\n\n▶ Período 2: {comparacion.Periodo2?.FechaInicio:dd/MM/yyyy} - {comparacion.Periodo2?.FechaFin:dd/MM/yyyy}\n   • Ingresos: {comparacion.Periodo2?.IngresoTotal:C2}\n   • Total Pagos: {comparacion.Periodo2?.TotalPagos}\n   • Total Citas: {comparacion.Periodo2?.TotalCitas}\n\n\uD83D\uDCC8 VARIACIÓN\n   • Diferencia en Ingresos: {comparacion.VariacionIngreso:C2} ({comparacion.PorcentajeVariacionIngreso:F2}%)\n   • Diferencia en Citas: {comparacion.VariacionCitas} citas\n";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al comparar períodos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_tabControl == null) return;
                var selected = _tabControl.SelectedItem as TabItem;
                if (selected == null) return;

                
                var choice = MessageBox.Show("¿Exportar todas las tablas de todas las pestañas? (Sí = todas, No = solo pestaña actual, Cancelar = cancelar)",
                    "Exportar", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (choice == MessageBoxResult.Cancel) return;

                List<DataGrid> gridsToExport = new List<DataGrid>();

                if (choice == MessageBoxResult.Yes)
                {
                    
                    foreach (var item in _tabControl.Items)
                    {
                        if (item is TabItem ti && ti.Content is DependencyObject root)
                        {
                            gridsToExport.AddRange(FindVisualChildren<DataGrid>(root));
                        }
                    }
                }
                else
                {
                    
                    if (selected.Content is DependencyObject root)
                    {
                        gridsToExport.AddRange(FindVisualChildren<DataGrid>(root));
                    }
                }

                gridsToExport = gridsToExport.Where(g => g != null && g.ItemsSource != null).ToList();

                if (gridsToExport.Count == 0)
                {
                    MessageBox.Show("No hay tablas visibles para exportar.", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                
                if (choice == MessageBoxResult.Yes)
                {
                    string baseFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BellaVistaReports", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                    Directory.CreateDirectory(baseFolder);

                    _btnExport.IsEnabled = false;
                    _progressBar.Visibility = Visibility.Visible;
                    _lblStatus.Text = "Exportando archivos...";
                    Mouse.OverrideCursor = Cursors.Wait;

                    try
                    {
                        int idx = 1;
                        foreach (var dg in gridsToExport.Where(g => g.ItemsSource != null))
                        {
                            string fileName = $"reporte_{idx}.xlsx";
                            
                            if (!string.IsNullOrWhiteSpace(dg.Name))
                            {
                                string safe = MakeValidFileName(dg.Name);
                                fileName = $"reporte_{idx}_{safe}.xlsx";
                            }

                            string fullPath = System.IO.Path.Combine(baseFolder, fileName);
                            ExportGridToExcelFile(dg, fullPath);
                            idx++;
                        }

                        MessageBox.Show($"Exportación completada. Archivos guardados en: {baseFolder}", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
                        _lblStatus.Text = "Exportación completada";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al exportar: {ex.Message}", "Exportar", MessageBoxButton.OK, MessageBoxImage.Error);
                        _lblStatus.Text = "Error al exportar";
                    }
                    finally
                    {
                        _btnExport.IsEnabled = true;
                        _progressBar.Visibility = Visibility.Collapsed;
                        Mouse.OverrideCursor = null;
                    }

                    return;
                }

                
                SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = "reportes.xlsx" };
                if (sfd.ShowDialog() != true) return;

                _btnExport.IsEnabled = false;
                _progressBar.Visibility = Visibility.Visible;
                _lblStatus.Text = "Exportando...";
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    ExportMultipleDataGridsToExcel(gridsToExport, sfd.FileName);
                    MessageBox.Show("Exportación completada.", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
                    _lblStatus.Text = "Exportación completada";
                }
                finally
                {
                    _btnExport.IsEnabled = true;
                    _progressBar.Visibility = Visibility.Collapsed;
                    Mouse.OverrideCursor = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Exportar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportMultipleDataGridsToExcel(List<DataGrid> grids, string path)
        {
           
            DatePicker dpInicio = (DatePicker)this.FindName("dpFechaInicio");
            DatePicker dpFin = (DatePicker)this.FindName("dpFechaFin");
            DateTime? fechaInicio = dpInicio?.SelectedDate ?? _cachedFechaInicio;
            DateTime? fechaFin = dpFin?.SelectedDate ?? _cachedFechaFin;
            string usuario = Environment.UserName;

            using (var wb = new XLWorkbook())
            {
                int sheetIndex = 1;
                foreach (var dg in grids)
                {
                    string sheetName = !string.IsNullOrWhiteSpace(dg.Name) ? dg.Name : $"Sheet{sheetIndex}";
                    sheetName = MakeValidSheetName(sheetName);
                   
                    string baseName = sheetName;
                    int suffix = 1;
                    while (wb.Worksheets.Any(ws => ws.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase)))
                    {
                        string candidate = baseName.Length > 25 ? baseName.Substring(0, 25) : baseName;
                        sheetName = candidate + "_" + suffix;
                        suffix++;
                    }

                    var ws = wb.Worksheets.Add(sheetName);

                   
                    int totalCols = dg.Columns.Count;
                    if (totalCols < 1) totalCols = 1;
                    ws.Cell(1, 1).Value = "Bella Vista - Reportes";
                    ws.Range(1, 1, 1, totalCols).Merge();
                    ws.Cell(2, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    ws.Range(2, 1, 2, totalCols).Merge();

                    ws.Cell(3, 1).Value = fechaInicio.HasValue && fechaFin.HasValue ? $"Rango: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}" : "Rango: -";
                    ws.Range(3, 1, 3, totalCols).Merge();

                    ws.Cell(4, 1).Value = $"Usuario: {usuario}";
                    ws.Range(4, 1, 4, totalCols).Merge();

                    int startRow = 6;
                    int col = 1;

                    
                    foreach (var c in dg.Columns)
                    {
                        ws.Cell(startRow, col).Value = c.Header?.ToString() ?? string.Empty;
                        ws.Cell(startRow, col).Style.Font.Bold = true;
                        col++;
                    }

                    
                    IEnumerable rowsEnumerable = null;
                    var view = CollectionViewSource.GetDefaultView(dg.ItemsSource);
                    if (view != null)
                        rowsEnumerable = view.Cast<object>();
                    else
                        rowsEnumerable = dg.ItemsSource as IEnumerable;

                    int rowIdx = startRow + 1;
                    if (rowsEnumerable != null)
                    {
                        foreach (var item in rowsEnumerable)
                        {
                            col = 1;
                            foreach (var c in dg.Columns)
                            {
                                string bindingPath = null;
                                if (c is DataGridBoundColumn boundCol && boundCol.Binding is Binding b && b.Path != null)
                                    bindingPath = b.Path.Path;

                                object value = null;
                                if (bindingPath != null && item != null)
                                {
                                    var prop = item.GetType().GetProperty(bindingPath);
                                    if (prop != null) value = prop.GetValue(item);
                                }

                             
                                if (value == null)
                                {
                                    ws.Cell(rowIdx, col).Value = string.Empty;
                                }
                                else if (value is DateTime dt)
                                {
                                    ws.Cell(rowIdx, col).Value = dt;
                                    ws.Cell(rowIdx, col).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                                }
                                else if (IsNumericType(value.GetType()))
                                {
                                    ws.Cell(rowIdx, col).Value = Convert.ToDouble(value);
                                }
                                else if (value is bool bval)
                                {
                                    ws.Cell(rowIdx, col).Value = bval;
                                }
                                else
                                {
                                    ws.Cell(rowIdx, col).Value = value.ToString();
                                }

                                col++;
                            }
                            rowIdx++;
                        }
                    }

                    
                    ws.Columns().AdjustToContents();
                    sheetIndex++;
                }

                wb.SaveAs(path);
            }
        }

        private void ExportGridToExcelFile(DataGrid dg, string path)
        {
            DatePicker dpInicio = (DatePicker)this.FindName("dpFechaInicio");
            DatePicker dpFin = (DatePicker)this.FindName("dpFechaFin");
            DateTime? fechaInicio = dpInicio?.SelectedDate ?? _cachedFechaInicio;
            DateTime? fechaFin = dpFin?.SelectedDate ?? _cachedFechaFin;
            string usuario = Environment.UserName;

            using (var wb = new XLWorkbook())
            {
                string sheetName = MakeValidSheetName(!string.IsNullOrWhiteSpace(dg.Name) ? dg.Name : "Reporte");
                var ws = wb.Worksheets.Add(sheetName);

                int totalCols = dg.Columns.Count;
                if (totalCols < 1) totalCols = 1;
                ws.Cell(1, 1).Value = "Bella Vista - Reportes";
                ws.Range(1, 1, 1, totalCols).Merge();
                ws.Cell(2, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Range(2, 1, 2, totalCols).Merge();
                ws.Cell(3, 1).Value = fechaInicio.HasValue && fechaFin.HasValue ? $"Rango: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}" : "Rango: -";
                ws.Range(3, 1, 3, totalCols).Merge();
                ws.Cell(4, 1).Value = $"Usuario: {usuario}";
                ws.Range(4, 1, 4, totalCols).Merge();

                int startRow = 6;
                int col = 1;
                foreach (var c in dg.Columns)
                {
                    ws.Cell(startRow, col).Value = c.Header?.ToString() ?? string.Empty;
                    ws.Cell(startRow, col).Style.Font.Bold = true;
                    col++;
                }

                IEnumerable rowsEnumerable = null;
                var view = CollectionViewSource.GetDefaultView(dg.ItemsSource);
                if (view != null)
                    rowsEnumerable = view.Cast<object>();
                else
                    rowsEnumerable = dg.ItemsSource as IEnumerable;

                int rowIdx = startRow + 1;
                if (rowsEnumerable != null)
                {
                    foreach (var item in rowsEnumerable)
                    {
                        col = 1;
                        foreach (var c in dg.Columns)
                        {
                            string bindingPath = null;
                            if (c is DataGridBoundColumn boundCol && boundCol.Binding is Binding b && b.Path != null)
                                bindingPath = b.Path.Path;

                            object value = null;
                            if (bindingPath != null && item != null)
                            {
                                var prop = item.GetType().GetProperty(bindingPath);
                                if (prop != null) value = prop.GetValue(item);
                            }

                            if (value == null)
                                ws.Cell(rowIdx, col).Value = string.Empty;
                            else if (value is DateTime dt)
                            {
                                ws.Cell(rowIdx, col).Value = dt;
                                ws.Cell(rowIdx, col).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                            }
                            else if (IsNumericType(value.GetType()))
                                ws.Cell(rowIdx, col).Value = Convert.ToDouble(value);
                            else if (value is bool bval)
                                ws.Cell(rowIdx, col).Value = bval;
                            else
                                ws.Cell(rowIdx, col).Value = value.ToString();

                            col++;
                        }
                        rowIdx++;
                    }
                }

                ws.Columns().AdjustToContents();
                wb.SaveAs(path);
            }
        }

        private static string MakeValidFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "file";
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var clean = new string(name.Where(ch => !invalid.Contains(ch)).ToArray());
            clean = clean.Replace(' ', '_');
            if (clean.Length == 0) clean = "file";
            return clean.Length > 120 ? clean.Substring(0, 120) : clean;
        }

        private static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        private static string MakeValidSheetName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Sheet1";
            
            var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            var clean = new string(name.Where(ch => !invalid.Contains(ch)).ToArray());
            clean = clean.Trim();
            if (clean.Length == 0) clean = "Sheet1";
            return clean.Length > 31 ? clean.Substring(0, 31) : clean;
        }

        
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    yield return t;

                foreach (var desc in FindVisualChildren<T>(child))
                    yield return desc;
            }
        }
    }
}
