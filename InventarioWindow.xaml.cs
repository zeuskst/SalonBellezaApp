using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace SalonBellezaApp
{
    public partial class InventarioWindow : Window
    {
        private InventarioService _inventarioService;
        private List<ProductoInventario> _productosActuales;

        public InventarioWindow()
        {
            InitializeComponent();
            _inventarioService = new InventarioService();
            CreateModernUI();
            _ = CargarDatosAsync();
        }

        private void CreateModernUI()
        {
            this.Title = "Bella Vista - Gestión de Inventario";
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

            Grid mainGrid = new Grid { Margin = new Thickness(0) };
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

            StackPanel titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(30, 0, 0, 0)
            };

            TextBlock titleIcon = new TextBlock
            {
                Text = "📦",
                FontSize = 32,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock titleText = new TextBlock
            {
                Text = "Control de Inventario",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                VerticalAlignment = VerticalAlignment.Center
            };

            titlePanel.Children.Add(titleIcon);
            titlePanel.Children.Add(titleText);

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
                Cursor = System.Windows.Input.Cursors.Hand
            };

            volverButton.Click += VolverButton_Click;
            volverBorder.Child = volverButton;

            Grid.SetColumn(titlePanel, 0);
            Grid.SetColumn(volverBorder, 1);
            headerGrid.Children.Add(titlePanel);
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

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Tab control para las pestañas
            TabControl tabControl = new TabControl
            {
                Margin = new Thickness(30),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0)
            };

            tabControl.Items.Add(CreateDashboardTab());
            tabControl.Items.Add(CreateMovimientosTab());
            tabControl.Items.Add(CreateHistorialTab());

            contentGrid.Children.Add(tabControl);
            Grid.SetRow(contentBorder, 1);
            mainGrid.Children.Add(contentBorder);
        }

        private TabItem CreateDashboardTab()
        {
            TabItem tab = new TabItem
            {
                Header = "📊 Dashboard"
            };

            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            StackPanel mainPanel = new StackPanel();
            scrollViewer.Content = mainPanel;

            // Tarjetas de estadísticas
            Grid statsGrid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition());

            Border cardTotal = CreateStatsCard("Total Productos", "0", "📦", Color.FromRgb(52, 152, 219));
            Border cardStockBajo = CreateStatsCard("Stock Bajo", "0", "⚠️", Color.FromRgb(243, 156, 18));
            Border cardSinStock = CreateStatsCard("Sin Stock", "0", "❌", Color.FromRgb(231, 76, 60));
            Border cardValor = CreateStatsCard("Valor Total", "$0", "💰", Color.FromRgb(46, 204, 113));

            cardTotal.Name = "cardTotalProductos";
            cardStockBajo.Name = "cardStockBajo";
            cardSinStock.Name = "cardSinStock";
            cardValor.Name = "cardValorTotal";

            this.RegisterName("cardTotalProductos", cardTotal);
            this.RegisterName("cardStockBajo", cardStockBajo);
            this.RegisterName("cardSinStock", cardSinStock);
            this.RegisterName("cardValorTotal", cardValor);

            Grid.SetColumn(cardTotal, 0);
            Grid.SetColumn(cardStockBajo, 1);
            Grid.SetColumn(cardSinStock, 2);
            Grid.SetColumn(cardValor, 3);

            statsGrid.Children.Add(cardTotal);
            statsGrid.Children.Add(cardStockBajo);
            statsGrid.Children.Add(cardSinStock);
            statsGrid.Children.Add(cardValor);

            mainPanel.Children.Add(statsGrid);

            // Productos con stock bajo
            Border stockBajoSection = CreateSection("⚠️ Productos con Stock Bajo");
            DataGrid gridStockBajo = CreateDataGrid();
            gridStockBajo.Name = "dgStockBajo";
            this.RegisterName("dgStockBajo", gridStockBajo);
            ((StackPanel)stockBajoSection.Child).Children.Add(gridStockBajo);
            mainPanel.Children.Add(stockBajoSection);

            // Productos próximos a vencer
            Border vencimientoSection = CreateSection("📅 Productos Próximos a Vencer (30 días)");
            DataGrid gridVencimiento = CreateDataGrid();
            gridVencimiento.Name = "dgVencimiento";
            this.RegisterName("dgVencimiento", gridVencimiento);
            ((StackPanel)vencimientoSection.Child).Children.Add(gridVencimiento);
            mainPanel.Children.Add(vencimientoSection);

            tab.Content = scrollViewer;
            return tab;
        }

        private TabItem CreateMovimientosTab()
        {
            TabItem tab = new TabItem
            {
                Header = "📝 Registrar Movimiento"
            };

            Grid grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Formulario
            Border formBorder = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(30),
                Margin = new Thickness(0, 0, 0, 20)
            };

            StackPanel formPanel = new StackPanel();

            // Producto
            TextBlock lblProducto = new TextBlock
            {
                Text = "Producto:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            ComboBox cmbProducto = new ComboBox
            {
                Name = "cmbProducto",
                Height = 35,
                Margin = new Thickness(0, 0, 0, 15)
            };
            this.RegisterName("cmbProducto", cmbProducto);

            // Tipo de movimiento
            TextBlock lblTipo = new TextBlock
            {
                Text = "Tipo de Movimiento:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            ComboBox cmbTipo = new ComboBox
            {
                Name = "cmbTipoMovimiento",
                Height = 35,
                Margin = new Thickness(0, 0, 0, 15)
            };
            cmbTipo.Items.Add(new ComboBoxItem { Content = "📥 Entrada", Tag = "entrada" });
            cmbTipo.Items.Add(new ComboBoxItem { Content = "📤 Salida", Tag = "salida" });
            cmbTipo.Items.Add(new ComboBoxItem { Content = "🔧 Ajuste", Tag = "ajuste" });
            cmbTipo.SelectedIndex = 0;
            this.RegisterName("cmbTipoMovimiento", cmbTipo);

            // Cantidad
            TextBlock lblCantidad = new TextBlock
            {
                Text = "Cantidad:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            TextBox txtCantidad = new TextBox
            {
                Name = "txtCantidad",
                Height = 35,
                Margin = new Thickness(0, 0, 0, 15)
            };
            this.RegisterName("txtCantidad", txtCantidad);

            // Motivo
            TextBlock lblMotivo = new TextBlock
            {
                Text = "Motivo:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            TextBox txtMotivo = new TextBox
            {
                Name = "txtMotivo",
                Height = 70,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 0, 0, 20)
            };
            this.RegisterName("txtMotivo", txtMotivo);

            // Botón
            Button btnRegistrar = new Button
            {
                Content = "✅ Registrar Movimiento",
                Height = 45,
                Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                Foreground = Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btnRegistrar.Click += BtnRegistrarMovimiento_Click;

            formPanel.Children.Add(lblProducto);
            formPanel.Children.Add(cmbProducto);
            formPanel.Children.Add(lblTipo);
            formPanel.Children.Add(cmbTipo);
            formPanel.Children.Add(lblCantidad);
            formPanel.Children.Add(txtCantidad);
            formPanel.Children.Add(lblMotivo);
            formPanel.Children.Add(txtMotivo);
            formPanel.Children.Add(btnRegistrar);

            formBorder.Child = formPanel;
            Grid.SetRow(formBorder, 0);
            grid.Children.Add(formBorder);

            tab.Content = grid;
            return tab;
        }

        private TabItem CreateHistorialTab()
        {
            TabItem tab = new TabItem
            {
                Header = "📜 Historial"
            };

            Grid grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Filtros
            StackPanel filterPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 15)
            };

            DatePicker dpInicio = new DatePicker
            {
                Name = "dpFechaInicio",
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
            };
            DatePicker dpFin = new DatePicker
            {
                Name = "dpFechaFin",
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
            };
            Button btnFiltrar = new Button
            {
                Content = "🔍 Filtrar",
                Width = 100,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            btnFiltrar.Click += BtnFiltrar_Click;

            this.RegisterName("dpFechaInicio", dpInicio);
            this.RegisterName("dpFechaFin", dpFin);

            filterPanel.Children.Add(new TextBlock { Text = "Desde:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            filterPanel.Children.Add(dpInicio);
            filterPanel.Children.Add(new TextBlock { Text = "Hasta:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            filterPanel.Children.Add(dpFin);
            filterPanel.Children.Add(btnFiltrar);

            DataGrid dgHistorial = CreateDataGrid();
            dgHistorial.Name = "dgHistorial";
            dgHistorial.AutoGenerateColumns = false;
            // CreateDataGrid() añade columnas por defecto (Producto, Categoría, Stock, etc.).
            // Limpiarlas para evitar duplicados y dejar solo las necesarias para el historial.
            dgHistorial.Columns.Clear();
            dgHistorial.Columns.Add(new DataGridTextColumn { Header = "Fecha", Binding = new System.Windows.Data.Binding("FechaMovimiento") { StringFormat = "dd/MM/yyyy HH:mm" }, Width = new DataGridLength(140) });
            dgHistorial.Columns.Add(new DataGridTextColumn { Header = "Producto", Binding = new System.Windows.Data.Binding("NombreProducto"), Width = new DataGridLength(180) });
            dgHistorial.Columns.Add(new DataGridTextColumn { Header = "Categoría", Binding = new System.Windows.Data.Binding("Categoria"), Width = new DataGridLength(140) });
            dgHistorial.Columns.Add(new DataGridTextColumn { Header = "Stock Actual", Binding = new System.Windows.Data.Binding("StockActual"), Width = new DataGridLength(100) });
            dgHistorial.Columns.Add(new DataGridTextColumn { Header = "Stock Mínimo", Binding = new System.Windows.Data.Binding("StockMinimo"), Width = new DataGridLength(100) });
            dgHistorial.Columns.Add(new DataGridTextColumn { Header = "Estado", Binding = new System.Windows.Data.Binding("Estado"), Width = new DataGridLength(100) });
            dgHistorial.Columns.Add(new DataGridTextColumn { Header = "Vencimiento", Binding = new System.Windows.Data.Binding("FechaCaducidadDisplay"), Width = new DataGridLength(120) });
            dgHistorial.Columns.Add(new DataGridTextColumn { Header = "Tipo", Binding = new System.Windows.Data.Binding("TipoDisplay"), Width = new DataGridLength(120) });
            dgHistorial.Columns.Add(new DataGridTextColumn { Header = "Cantidad", Binding = new System.Windows.Data.Binding("Cantidad"), Width = new DataGridLength(90) });
            dgHistorial.Columns.Add(new DataGridTextColumn { Header = "Motivo", Binding = new System.Windows.Data.Binding("Motivo"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgHistorial.Columns.Add(new DataGridTextColumn { Header = "Usuario", Binding = new System.Windows.Data.Binding("Usuario"), Width = new DataGridLength(120) });

            this.RegisterName("dgHistorial", dgHistorial);

            Grid.SetRow(filterPanel, 0);
            Grid.SetRow(dgHistorial, 1);
            grid.Children.Add(filterPanel);
            grid.Children.Add(dgHistorial);

            tab.Content = grid;
            return tab;
        }

        private Border CreateStatsCard(string title, string value, string icon, Color color)
        {
            Border card = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(5),
                Height = 140,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 270,
                    ShadowDepth = 3,
                    Opacity = 0.2,
                    BlurRadius = 8
                }
            };

            Grid cardGrid = new Grid { Margin = new Thickness(20) };
            // Filas y columnas: título+icon en la fila superior, valor en la inferior
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icono dentro de un Viewbox para evitar recorte y escalar correctamente
            TextBlock iconInner = new TextBlock
            {
                Text = icon,
                FontFamily = new FontFamily("Segoe UI Emoji"),
                FontSize = 40,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Viewbox iconBox = new Viewbox
            {
                Width = 56,
                Height = 56,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top
            };
            iconBox.Child = iconInner;

            TextBlock titleText = new TextBlock
            {
                Text = title,
                FontSize = 14,
                Foreground = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Top
            };

            TextBlock valueText = new TextBlock
            {
                Text = value,
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(color),
                VerticalAlignment = VerticalAlignment.Bottom,
                Name = "valueText"
            };

            Grid.SetRow(titleText, 0);
            Grid.SetColumn(titleText, 0);
            Grid.SetRow(iconBox, 0);
            Grid.SetColumn(iconBox, 1);
            Grid.SetRow(valueText, 1);
            Grid.SetColumn(valueText, 0);
            Grid.SetColumnSpan(valueText, 2);

            cardGrid.Children.Add(titleText);
            cardGrid.Children.Add(iconBox);
            cardGrid.Children.Add(valueText);

            card.Child = cardGrid;
            return card;
        }

        private Border CreateSection(string title)
        {
            Border section = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 20),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 270,
                    ShadowDepth = 3,
                    Opacity = 0.2,
                    BlurRadius = 8
                }
            };

            StackPanel panel = new StackPanel();

            TextBlock titleText = new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                Margin = new Thickness(0, 0, 0, 15)
            };

            panel.Children.Add(titleText);
            section.Child = panel;
            return section;
        }

        private DataGrid CreateDataGrid()
        {
            DataGrid grid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Height = 250,
                AlternatingRowBackground = new SolidColorBrush(Color.FromArgb(20, 102, 126, 234))
            };

            grid.Columns.Add(new DataGridTextColumn { Header = "Producto", Binding = new System.Windows.Data.Binding("Nombre"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            grid.Columns.Add(new DataGridTextColumn { Header = "Categoría", Binding = new System.Windows.Data.Binding("Categoria"), Width = new DataGridLength(120) });
            grid.Columns.Add(new DataGridTextColumn { Header = "Stock Actual", Binding = new System.Windows.Data.Binding("StockActual"), Width = new DataGridLength(100) });
            grid.Columns.Add(new DataGridTextColumn { Header = "Stock Mínimo", Binding = new System.Windows.Data.Binding("StockMinimo"), Width = new DataGridLength(110) });
            grid.Columns.Add(new DataGridTextColumn { Header = "Estado", Binding = new System.Windows.Data.Binding("EstadoStock"), Width = new DataGridLength(120) });
            grid.Columns.Add(new DataGridTextColumn { Header = "Vencimiento", Binding = new System.Windows.Data.Binding("FechaCaducidadDisplay"), Width = new DataGridLength(120) });

            return grid;
        }

        private async System.Threading.Tasks.Task CargarDatosAsync()
        {
            try
            {
                // Cargar estadísticas
                var stats = await _inventarioService.ObtenerEstadisticasAsync();
                var valorTotal = await _inventarioService.ObtenerValorTotalInventarioAsync();

                Border cardTotal = (Border)this.FindName("cardTotalProductos");
                Border cardStockBajo = (Border)this.FindName("cardStockBajo");
                Border cardSinStock = (Border)this.FindName("cardSinStock");
                Border cardValor = (Border)this.FindName("cardValorTotal");

                UpdateCardValue(cardTotal, stats.Total.ToString());
                UpdateCardValue(cardStockBajo, stats.StockBajo.ToString());
                UpdateCardValue(cardSinStock, stats.SinStock.ToString());
                UpdateCardValue(cardValor, $"${valorTotal:N2}");

                // Cargar productos con stock bajo
                var stockBajo = await _inventarioService.ObtenerProductosStockBajoAsync();
                DataGrid dgStockBajo = (DataGrid)this.FindName("dgStockBajo");
                dgStockBajo.ItemsSource = stockBajo;

                // Cargar productos próximos a vencer
                var proximosVencer = await _inventarioService.ObtenerProductosProximosVencerAsync();
                DataGrid dgVencimiento = (DataGrid)this.FindName("dgVencimiento");
                dgVencimiento.ItemsSource = proximosVencer;

                // Cargar productos para el combo
                var productos = await _inventarioService.ObtenerInventarioAsync();
                _productosActuales = productos;
                ComboBox cmbProducto = (ComboBox)this.FindName("cmbProducto");
                cmbProducto.ItemsSource = productos;
                cmbProducto.DisplayMemberPath = "Nombre";
                cmbProducto.SelectedValuePath = "IdProducto";

                // Cargar historial
                await CargarHistorialAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCardValue(Border card, string value)
        {
            if (card?.Child is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is TextBlock tb && tb.Name == "valueText")
                    {
                        tb.Text = value;
                        break;
                    }
                }
            }
        }

        private async void BtnRegistrarMovimiento_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBox cmbProducto = (ComboBox)this.FindName("cmbProducto");
                ComboBox cmbTipo = (ComboBox)this.FindName("cmbTipoMovimiento");
                TextBox txtCantidad = (TextBox)this.FindName("txtCantidad");
                TextBox txtMotivo = (TextBox)this.FindName("txtMotivo");

                if (cmbProducto.SelectedItem == null)
                {
                    MessageBox.Show("Seleccione un producto", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCantidad.Text) || !decimal.TryParse(txtCantidad.Text, out decimal cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("Ingrese una cantidad válida", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var producto = (ProductoInventario)cmbProducto.SelectedItem;
                var tipoItem = (ComboBoxItem)cmbTipo.SelectedItem;
                string tipoMovimiento = tipoItem.Tag.ToString();

                await _inventarioService.RegistrarMovimientoAsync(
                    producto.IdProducto,
                    tipoMovimiento,
                    cantidad,
                    txtMotivo.Text,
                    "Admin"
                );

                MessageBox.Show("Movimiento registrado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar formulario
                txtCantidad.Clear();
                txtMotivo.Clear();

                // Recargar datos
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            await CargarHistorialAsync();
        }

        private async System.Threading.Tasks.Task CargarHistorialAsync()
        {
            try
            {
                DatePicker dpInicio = (DatePicker)this.FindName("dpFechaInicio");
                DatePicker dpFin = (DatePicker)this.FindName("dpFechaFin");

                var movimientos = await _inventarioService.ObtenerMovimientosAsync(
                    null,
                    dpInicio.SelectedDate,
                    dpFin.SelectedDate
                );

                DataGrid dgHistorial = (DataGrid)this.FindName("dgHistorial");
                dgHistorial.ItemsSource = movimientos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
