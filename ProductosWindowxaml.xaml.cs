using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Linq;
using System.Threading.Tasks;

namespace SalonBellezaApp
{
    public partial class ProductosWindow : Window
    {
        private readonly ProductoService _productoService;
        private readonly ServicioService _servicioService;
        private readonly InventarioService _inventarioService;
        private List<Producto> productos = new List<Producto>();
        private Producto productoEditando = null;

        public ProductosWindow()
        {
            InitializeComponent();
            _productoService = new ProductoService();
            _servicioService = new ServicioService();
            _inventarioService = new InventarioService();
            CreateModernProductosUI();
            _ = CargarProductosAsync();
        }

        private void CreateModernProductosUI()
        {
            this.Title = "Bella Vista - Gestión de Productos";
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
            CreateMainContent(mainGrid);
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
                Text = "Gestión de Productos",
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

        private void CreateMainContent(Grid mainGrid)
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

            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(450) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            CreateFormPanel(contentGrid);
            CreateProductosList(contentGrid);

            Grid.SetRow(contentBorder, 1);
            mainGrid.Children.Add(contentBorder);
        }

        private void CreateFormPanel(Grid contentGrid)
        {
            Border formBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(50, 102, 126, 234)),
                CornerRadius = new CornerRadius(15),
                Margin = new Thickness(30, 30, 15, 30)
            };

            ScrollViewer formScroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(25)
            };

            StackPanel formPanel = new StackPanel();
            formScroll.Content = formPanel;

            TextBlock formTitle = new TextBlock
            {
                Name = "lblTituloForm",
                Text = "🆕 Nuevo Producto",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                Margin = new Thickness(0, 0, 0, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            this.RegisterName("lblTituloForm", formTitle);
            formPanel.Children.Add(formTitle);

            CreateFormField(formPanel, "Nombre del Producto", "txtNombre");
            CreateFormField(formPanel, "Categoría", "txtCategoria");
            CreateFormFieldNumeric(formPanel, "Precio de Compra", "txtPrecioCompra");
            CreateFormFieldNumeric(formPanel, "Precio de Venta (opcional)", "txtPrecioVenta");
            CreateFormFieldNumeric(formPanel, "Stock Actual", "txtStockActual");
            CreateFormFieldNumeric(formPanel, "Stock Mínimo", "txtStockMinimo");
            CreateFormField(formPanel, "Unidad de Medida", "txtUnidadMedida");
            CreateFormField(formPanel, "Descripción", "txtDescripcion", true);
            CreateDatePickerField(formPanel, "Fecha de Caducidad (opcional)", "dpFechaCaducidad");
            CreateComboBoxField(formPanel, "Estado", "cmbEstado");

            CreateFormButtons(formPanel);

            formBorder.Child = formScroll;
            Grid.SetColumn(formBorder, 0);
            contentGrid.Children.Add(formBorder);
        }

        private void CreateFormField(StackPanel parent, string label, string name, bool isMultiline = false)
        {
            TextBlock labelText = new TextBlock
            {
                Text = label,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
            };

            Border inputBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 102, 126, 234)),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };

            TextBox textBox;

            if (isMultiline)
            {
                textBox = new TextBox
                {
                    Name = name,
                    Height = 80,
                    Padding = new Thickness(15, 10, 15, 10),
                    FontSize = 14,
                    BorderThickness = new Thickness(0),
                    Background = Brushes.Transparent,
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };
            }
            else
            {
                textBox = new TextBox
                {
                    Name = name,
                    Height = 45,
                    Padding = new Thickness(15, 10, 15, 10),
                    FontSize = 14,
                    BorderThickness = new Thickness(0),
                    Background = Brushes.Transparent,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
            }

            inputBorder.Child = textBox;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, textBox);
        }

        private void CreateFormFieldNumeric(StackPanel parent, string label, string name)
        {
            TextBlock labelText = new TextBlock
            {
                Text = label,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
            };

            Border inputBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 102, 126, 234)),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };

            TextBox textBox = new TextBox
            {
                Name = name,
                Height = 45,
                Padding = new Thickness(15, 10, 15, 10),
                FontSize = 14,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            textBox.PreviewTextInput += NumericTextBox_PreviewTextInput;

            inputBorder.Child = textBox;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, textBox);
        }

        private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private bool IsTextNumeric(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',')
                    return false;
            }
            return true;
        }

        private void CreateComboBoxField(StackPanel parent, string label, string name)
        {
            TextBlock labelText = new TextBlock
            {
                Text = label,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
            };

            Border inputBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 102, 126, 234)),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };

            ComboBox comboBox = new ComboBox
            {
                Name = name,
                Height = 45,
                Padding = new Thickness(15, 10, 15, 10),
                FontSize = 14,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            if (name == "cmbEstado")
            {
                comboBox.Items.Add("activo");
                comboBox.Items.Add("inactivo");
                comboBox.SelectedIndex = 0;
            }

            inputBorder.Child = comboBox;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, comboBox);
        }

        private void CreateDatePickerField(StackPanel parent, string label, string name)
        {
            TextBlock labelText = new TextBlock
            {
                Text = label,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
            };

            Border inputBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 102, 126, 234)),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };

            DatePicker datePicker = new DatePicker
            {
                Name = name,
                Height = 45,
                Padding = new Thickness(15, 10, 15, 10),
                FontSize = 14,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent
            };

            inputBorder.Child = datePicker;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, datePicker);
        }

        private void CreateFormButtons(StackPanel parent)
        {
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 20, 0, 0)
            };

            Border actualizarBorder = new Border
            {
                Name = "borderActualizar",
                CornerRadius = new CornerRadius(25),
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Margin = new Thickness(0, 0, 0, 10),
                Visibility = Visibility.Collapsed
            };

            Button actualizarButton = new Button
            {
                Name = "btnActualizar",
                Content = "✏️ Actualizar Producto",
                Height = 50,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            actualizarBorder.Child = actualizarButton;

            Border guardarBorder = new Border
            {
                Name = "borderGuardar",
                CornerRadius = new CornerRadius(25),
                Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                Margin = new Thickness(0, 0, 0, 10)
            };

            Button guardarButton = new Button
            {
                Name = "btnGuardar",
                Content = "💾 Guardar Producto",
                Height = 50,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            guardarBorder.Child = guardarButton;

            Border asignarServiciosBorder = new Border
            {
                CornerRadius = new CornerRadius(25),
                Background = new SolidColorBrush(Color.FromRgb(155, 89, 182)),
                Margin = new Thickness(0, 0, 0, 10)
            };

            Button asignarServiciosButton = new Button
            {
                Name = "btnAsignarServicios",
                Content = "🔗 Administrar Servicios",
                Height = 45,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            asignarServiciosButton.Click += AsignarServicios_Click;
            asignarServiciosBorder.Child = asignarServiciosButton;

            Border limpiarBorder = new Border
            {
                CornerRadius = new CornerRadius(22),
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60))
            };

            Button limpiarButton = new Button
            {
                Content = "🧹 Limpiar Campos",
                Height = 45,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            limpiarBorder.Child = limpiarButton;

            guardarButton.Click += GuardarProducto_Click;
            limpiarButton.Click += LimpiarCampos_Click;
            actualizarButton.Click += ActualizarProducto_Click;

            this.RegisterName("btnActualizar", actualizarButton);
            this.RegisterName("borderActualizar", actualizarBorder);
            this.RegisterName("btnGuardar", guardarButton);
            this.RegisterName("borderGuardar", guardarBorder);
            this.RegisterName("btnAsignarServicios", asignarServiciosButton);

            buttonPanel.Children.Add(actualizarBorder);
            buttonPanel.Children.Add(guardarBorder);
            buttonPanel.Children.Add(asignarServiciosBorder);
            buttonPanel.Children.Add(limpiarBorder);
            parent.Children.Add(buttonPanel);
        }

        private void CreateProductosList(Grid contentGrid)
        {
            Border listBorder = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(15),
                Margin = new Thickness(15, 30, 30, 30)
            };

            DropShadowEffect listShadow = new DropShadowEffect
            {
                Color = Colors.Gray,
                Direction = 270,
                ShadowDepth = 3,
                Opacity = 0.3,
                BlurRadius = 10
            };
            listBorder.Effect = listShadow;

            Grid listGrid = new Grid();
            listBorder.Child = listGrid;

            listGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
            listGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            CreateListHeader(listGrid);
            CreateProductosDataGrid(listGrid);

            Grid.SetColumn(listBorder, 1);
            contentGrid.Children.Add(listBorder);
        }

        private void CreateListHeader(Grid listGrid)
        {
            Border headerBorder = new Border
            {
                Background = new LinearGradientBrush(Color.FromRgb(102, 126, 234),
                                                   Color.FromRgb(118, 75, 162), 0),
                CornerRadius = new CornerRadius(15, 15, 0, 0)
            };

            StackPanel headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(25, 0, 25, 0)
            };

            TextBlock listTitle = new TextBlock
            {
                Text = "📋 Lista de Productos",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };

            StackPanel searchPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0)
            };

            Border searchBorder = new Border
            {
                CornerRadius = new CornerRadius(17),
                Background = Brushes.White
            };

            TextBox searchBox = new TextBox
            {
                Name = "txtBuscar",
                Width = 210,
                Height = 40,
                Padding = new Thickness(10),
                FontSize = 14,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Text = "🔍 Buscar producto...",
                Foreground = Brushes.Gray
            };

            searchBorder.Child = searchBox;
            searchBox.GotFocus += SearchBox_GotFocus;
            searchBox.LostFocus += SearchBox_LostFocus;
            searchBox.TextChanged += SearchBox_TextChanged;

            this.RegisterName("txtBuscar", searchBox);
            searchPanel.Children.Add(searchBorder);

            headerPanel.Children.Add(listTitle);
            headerPanel.Children.Add(searchPanel);

            headerBorder.Child = headerPanel;
            Grid.SetRow(headerBorder, 0);
            listGrid.Children.Add(headerBorder);
        }

        private void CreateProductosDataGrid(Grid listGrid)
        {
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 10, 0, 0)
            };

            StackPanel productosPanel = new StackPanel
            {
                Name = "productosPanel"
            };

            this.RegisterName("productosPanel", productosPanel);
            scrollViewer.Content = productosPanel;

            Grid.SetRow(scrollViewer, 1);
            listGrid.Children.Add(scrollViewer);
        }

        private void RefreshProductosList()
        {
            StackPanel productosPanel = (StackPanel)this.FindName("productosPanel");
            productosPanel.Children.Clear();

            foreach (var producto in productos)
            {
                CreateProductoCard(productosPanel, producto);
            }
        }

        private void CreateProductoCard(StackPanel parent, Producto producto)
        {
            Color estadoColor = producto.Estado == "activo" ?
                Color.FromRgb(46, 204, 113) : Color.FromRgb(149, 165, 166);

            Border cardBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30, estadoColor.R, estadoColor.G, estadoColor.B)),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(20, 5, 20, 5),
                Padding = new Thickness(20, 15, 20, 15)
            };

            Grid cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel infoPanel = new StackPanel();

            TextBlock nombreText = new TextBlock
            {
                Text = $"📦 {producto.Nombre}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock categoriaText = new TextBlock
            {
                Text = $"🏷️ {producto.Categoria}",
                FontSize = 14,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 3)
            };

            TextBlock precioText = new TextBlock
            {
                Text = $"💰 Compra: ${producto.PrecioCompra:F2} | Venta: ${producto.PrecioVenta:F2}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 3)
            };

            TextBlock stockText = new TextBlock
            {
                Text = $"📊 Stock: {producto.StockActual} (mín: {producto.StockMinimo})",
                FontSize = 13,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 3)
            };

            TextBlock estadoText = new TextBlock
            {
                Text = $"● {producto.Estado.ToUpper()}",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(estadoColor)
            };

            infoPanel.Children.Add(nombreText);
            infoPanel.Children.Add(categoriaText);
            infoPanel.Children.Add(precioText);
            infoPanel.Children.Add(stockText);
            infoPanel.Children.Add(estadoText);

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            Border editBorder = new Border
            {
                CornerRadius = new CornerRadius(17),
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Margin = new Thickness(0, 0, 10, 0)
            };

            Button editButton = new Button
            {
                Content = "✏️",
                Width = 35,
                Height = 35,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = producto
            };

            editBorder.Child = editButton;

            Border serviciosBorder = new Border
            {
                CornerRadius = new CornerRadius(17),
                Background = new SolidColorBrush(Color.FromRgb(155, 89, 182)),
                Margin = new Thickness(0, 0, 10, 0)
            };

            Button serviciosButton = new Button
            {
                Content = "🔗",
                Width = 35,
                Height = 35,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = producto
            };

            serviciosBorder.Child = serviciosButton;

            Border deleteBorder = new Border
            {
                CornerRadius = new CornerRadius(17),
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60))
            };

            Button deleteButton = new Button
            {
                Content = "🗑️",
                Width = 35,
                Height = 35,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = producto
            };

            deleteBorder.Child = deleteButton;

            editButton.Click += EditarProducto_Click;
            serviciosButton.Click += VerServiciosUso_Click;
            deleteButton.Click += EliminarProducto_Click;

            buttonPanel.Children.Add(editBorder);
            buttonPanel.Children.Add(serviciosBorder);
            buttonPanel.Children.Add(deleteBorder);

            Grid.SetColumn(infoPanel, 0);
            Grid.SetColumn(buttonPanel, 1);
            cardGrid.Children.Add(infoPanel);
            cardGrid.Children.Add(buttonPanel);

            cardBorder.Child = cardGrid;
            parent.Children.Add(cardBorder);
        }

        private async Task CargarProductosAsync()
        {
            try
            {
                productos = await _productoService.ObtenerTodosAsync();
                RefreshProductosList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void GuardarProducto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nombre = ((TextBox)this.FindName("txtNombre")).Text.Trim();
                var categoria = ((TextBox)this.FindName("txtCategoria")).Text.Trim();
                var precioCompraText = ((TextBox)this.FindName("txtPrecioCompra")).Text.Trim();
                var precioVentaText = ((TextBox)this.FindName("txtPrecioVenta")).Text.Trim();
                var stockActualText = ((TextBox)this.FindName("txtStockActual")).Text.Trim();
                var stockMinimoText = ((TextBox)this.FindName("txtStockMinimo")).Text.Trim();
                var unidadMedida = ((TextBox)this.FindName("txtUnidadMedida")).Text.Trim();
                var descripcion = ((TextBox)this.FindName("txtDescripcion")).Text.Trim();
                DatePicker dpFechaCaducidad = (DatePicker)this.FindName("dpFechaCaducidad");
                ComboBox cmbEstado = (ComboBox)this.FindName("cmbEstado");

                
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    MessageBox.Show("El nombre del producto es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(precioCompraText.Replace(',', '.'), out decimal precioCompra) || precioCompra <= 0)
                {
                    MessageBox.Show("El precio de compra debe ser un número válido mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int stockActual = 0;
                int stockMinimo = 0;
                if (!string.IsNullOrWhiteSpace(stockActualText) && !int.TryParse(stockActualText, out stockActual))
                {
                    MessageBox.Show("Stock actual debe ser un número entero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!string.IsNullOrWhiteSpace(stockMinimoText) && !int.TryParse(stockMinimoText, out stockMinimo))
                {
                    MessageBox.Show("Stock mínimo debe ser un número entero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal? precioVenta = null;
                if (!string.IsNullOrWhiteSpace(precioVentaText))
                {
                    if (decimal.TryParse(precioVentaText.Replace(',', '.'), out decimal pv))
                    {
                        precioVenta = pv;
                    }
                }

                var nuevoProducto = new Producto
                {
                    Nombre = nombre,
                    Categoria = categoria,
                    Descripcion = descripcion,
                    StockActual = stockActual,
                    StockMinimo = stockMinimo,
                    PrecioCompra = precioCompra,
                    PrecioVenta = precioVenta,
                    UnidadMedida = unidadMedida,
                    FechaEntrada = DateTime.Now,
                    FechaCaducidad = dpFechaCaducidad.SelectedDate,
                    Estado = cmbEstado.SelectedItem.ToString()
                };

                bool resultado = await _productoService.AgregarAsync(nuevoProducto);

                if (resultado)
                {
                    await CargarProductosAsync();
                    LimpiarCampos();
                    MessageBox.Show("Producto guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al guardar el producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ActualizarProducto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (productoEditando == null) return;

                var nombre = ((TextBox)this.FindName("txtNombre")).Text.Trim();
                var categoria = ((TextBox)this.FindName("txtCategoria")).Text.Trim();
                var precioCompraText = ((TextBox)this.FindName("txtPrecioCompra")).Text.Trim();
                var precioVentaText = ((TextBox)this.FindName("txtPrecioVenta")).Text.Trim();
                var stockActualText = ((TextBox)this.FindName("txtStockActual")).Text.Trim();
                var stockMinimoText = ((TextBox)this.FindName("txtStockMinimo")).Text.Trim();
                var unidadMedida = ((TextBox)this.FindName("txtUnidadMedida")).Text.Trim();
                var descripcion = ((TextBox)this.FindName("txtDescripcion")).Text.Trim();
                DatePicker dpFechaCaducidad = (DatePicker)this.FindName("dpFechaCaducidad");
                ComboBox cmbEstado = (ComboBox)this.FindName("cmbEstado");

                
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    MessageBox.Show("El nombre del producto es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(precioCompraText.Replace(',', '.'), out decimal precioCompra) || precioCompra <= 0)
                {
                    MessageBox.Show("El precio de compra debe ser un número válido mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int stockActual = 0;
                int stockMinimo = 0;
                if (!string.IsNullOrWhiteSpace(stockActualText) && !int.TryParse(stockActualText, out stockActual))
                {
                    MessageBox.Show("Stock actual debe ser un número entero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!string.IsNullOrWhiteSpace(stockMinimoText) && !int.TryParse(stockMinimoText, out stockMinimo))
                {
                    MessageBox.Show("Stock mínimo debe ser un número entero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal? precioVenta = null;
                if (!string.IsNullOrWhiteSpace(precioVentaText))
                {
                    if (decimal.TryParse(precioVentaText.Replace(',', '.'), out decimal pv))
                    {
                        precioVenta = pv;
                    }
                }

                int previousStock = productoEditando.StockActual;

                productoEditando.Nombre = nombre;
                productoEditando.Categoria = categoria;
                productoEditando.Descripcion = descripcion;
                productoEditando.PrecioCompra = precioCompra;
                productoEditando.PrecioVenta = precioVenta;
                productoEditando.UnidadMedida = unidadMedida;
                productoEditando.FechaCaducidad = dpFechaCaducidad.SelectedDate;
                productoEditando.Estado = cmbEstado.SelectedItem.ToString();
                productoEditando.StockActual = stockActual;
                productoEditando.StockMinimo = stockMinimo;

                bool resultado = await _productoService.ActualizarAsync(productoEditando);

                if (resultado)
                {
                
                    int delta = productoEditando.StockActual - previousStock;
                    if (delta != 0)
                    {
                        string tipo = delta > 0 ? "entrada" : "salida";
                        decimal cantidadMov = Math.Abs(delta);
                        await _inventarioService.RegistrarMovimientoLogAsync(productoEditando.IdProducto, tipo, cantidadMov, "Ajuste manual de stock", "Sistema");
                    }

                    await CargarProductosAsync();
                    CambiarModoCreacion();
                    LimpiarCampos();
                    MessageBox.Show("Producto actualizado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al actualizar el producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarCampos_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
            CambiarModoCreacion();
        }

        private void LimpiarCampos()
        {
            ((TextBox)this.FindName("txtNombre")).Text = "";
            ((TextBox)this.FindName("txtCategoria")).Text = "";
            ((TextBox)this.FindName("txtPrecioCompra")).Text = "";
            ((TextBox)this.FindName("txtPrecioVenta")).Text = "";
            ((TextBox)this.FindName("txtUnidadMedida")).Text = "";
            ((TextBox)this.FindName("txtDescripcion")).Text = "";
            ((TextBox)this.FindName("txtStockActual")).Text = "";
            ((TextBox)this.FindName("txtStockMinimo")).Text = "";
            ((DatePicker)this.FindName("dpFechaCaducidad")).SelectedDate = null;
            ((ComboBox)this.FindName("cmbEstado")).SelectedIndex = 0;
        }

        private void EditarProducto_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Producto producto = (Producto)button.Tag;
            productoEditando = producto;

            
            ((TextBox)this.FindName("txtNombre")).Text = producto.Nombre;
            ((TextBox)this.FindName("txtCategoria")).Text = producto.Categoria;
            ((TextBox)this.FindName("txtPrecioCompra")).Text = producto.PrecioCompra.ToString("F2");
            ((TextBox)this.FindName("txtPrecioVenta")).Text = producto.PrecioVenta.HasValue ? producto.PrecioVenta.Value.ToString("F2") : "";
            ((TextBox)this.FindName("txtUnidadMedida")).Text = producto.UnidadMedida;
            ((TextBox)this.FindName("txtDescripcion")).Text = producto.Descripcion;
            ((TextBox)this.FindName("txtStockActual")).Text = producto.StockActual.ToString();
            ((TextBox)this.FindName("txtStockMinimo")).Text = producto.StockMinimo.ToString();
            ((DatePicker)this.FindName("dpFechaCaducidad")).SelectedDate = producto.FechaCaducidad;

            ComboBox cmbEstado = (ComboBox)this.FindName("cmbEstado");
            for (int i = 0; i < cmbEstado.Items.Count; i++)
            {
                if (cmbEstado.Items[i].ToString() == producto.Estado)
                {
                    cmbEstado.SelectedIndex = i;
                    break;
                }
            }

            CambiarModoEdicion();
        }

        private async void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Producto producto = (Producto)button.Tag;

            var result = MessageBox.Show($"¿Está seguro de eliminar el producto '{producto.Nombre}'?\n\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool resultado = await _productoService.EliminarAsync(producto.IdProducto);

                    if (resultado)
                    {
                        await CargarProductosAsync();
                        MessageBox.Show("Producto eliminado exitosamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CambiarModoEdicion()
        {
            var lblTitulo = (TextBlock)this.FindName("lblTituloForm");
            var borderActualizar = (Border)this.FindName("borderActualizar");
            var borderGuardar = (Border)this.FindName("borderGuardar");

            lblTitulo.Text = "✏️ Editar Producto";
            borderActualizar.Visibility = Visibility.Visible;
            borderGuardar.Visibility = Visibility.Collapsed;
        }

        private void CambiarModoCreacion()
        {
            var lblTitulo = (TextBlock)this.FindName("lblTituloForm");
            var borderActualizar = (Border)this.FindName("borderActualizar");
            var borderGuardar = (Border)this.FindName("borderGuardar");

            lblTitulo.Text = "🆕 Nuevo Producto";
            borderActualizar.Visibility = Visibility.Collapsed;
            borderGuardar.Visibility = Visibility.Visible;
            productoEditando = null;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "🔍 Buscar producto...")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "🔍 Buscar producto...";
                textBox.Foreground = Brushes.Gray;
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string searchText = textBox.Text;

            if (searchText == "🔍 Buscar producto..." || string.IsNullOrWhiteSpace(searchText))
            {
                await CargarProductosAsync();
                return;
            }

            try
            {
                var productosFiltrados = await _productoService.BuscarAsync(searchText);
                productos = productosFiltrados;
                RefreshProductosList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en búsqueda: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AsignarServicios_Click(object sender, RoutedEventArgs e)
        {
            if (productoEditando == null)
            {
                MessageBox.Show("Seleccione primero un producto para configurar servicios.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(productoEditando.Categoria))
            {
                MessageBox.Show("El producto no tiene categoría. Defina la categoría antes de asignarlo a servicios.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var servicios = await _servicioService.ObtenerTodosAsync();

                var window = new Window
                {
                    Title = $"Administrar servicios para {productoEditando.Nombre}",
                    Width = 500,
                    Height = 500,
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                StackPanel panel = new StackPanel { Margin = new Thickness(15) };
                TextBlock info = new TextBlock { Text = $"Categoría del producto: {productoEditando.Categoria}", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 10) };
                panel.Children.Add(info);

                var checkBoxes = new List<CheckBox>();
                foreach (var s in servicios)
                {
                    var cb = new CheckBox
                    {
                        Content = s.Nombre,
                        Tag = s,
                        IsChecked = s.CategoriasPermitidas.Contains(productoEditando.Categoria),
                        Margin = new Thickness(0, 0, 0, 6)
                    };
                    checkBoxes.Add(cb);
                    panel.Children.Add(cb);
                }

                Button btnSave = new Button { Content = "Guardar", Width = 100, Height = 35, Margin = new Thickness(0, 10, 0, 0), HorizontalAlignment = HorizontalAlignment.Center };
                btnSave.Click += async (s, ev) =>
                {
                    try
                    {
                        foreach (var cb in checkBoxes)
                        {
                            var serv = (Servicio)cb.Tag;
                            bool isChecked = cb.IsChecked == true;
                            bool currently = serv.CategoriasPermitidas.Contains(productoEditando.Categoria);

                            if (isChecked && !currently)
                            {
                                await _servicioService.AgregarCategoriaAlServicioAsync(serv.IdServicio, productoEditando.Categoria);
                            }
                            else if (!isChecked && currently)
                            {
                                await _servicioService.EliminarCategoriaDelServicioAsync(serv.IdServicio, productoEditando.Categoria);
                            }
                        }

                        MessageBox.Show("Configuración guardada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        window.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                panel.Children.Add(btnSave);
                window.Content = new ScrollViewer { Content = panel };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void VerServiciosUso_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Producto producto = (Producto)button.Tag;

            try
            {
                var servicios = await _servicioService.ObtenerServiciosPorProductoAsync(producto.IdProducto);

                if (servicios == null || servicios.Count == 0)
                {
                    MessageBox.Show("No hay servicios que usen este producto.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var window = new Window
                {
                    Title = $"Servicios que usan {producto.Nombre}",
                    Width = 400,
                    Height = 400,
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                StackPanel panel = new StackPanel { Margin = new Thickness(15) };
                foreach (var s in servicios)
                {
                    panel.Children.Add(new TextBlock { Text = s.Nombre, FontSize = 14, FontWeight = FontWeights.Medium, Margin = new Thickness(0, 0, 0, 6) });
                }

                window.Content = new ScrollViewer { Content = panel };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
