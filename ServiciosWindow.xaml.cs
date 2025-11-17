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
    public partial class ServiciosWindow : Window
    {
        private readonly ServicioService _servicioService;
        private readonly ProductoService _productoService;
        private List<Servicio> servicios = new List<Servicio>();
        private Servicio servicioEditando = null;
        private List<Producto> productosDisponibles = new List<Producto>();

        public ServiciosWindow()
        {
            InitializeComponent();
            _servicioService = new ServicioService();
            _productoService = new ProductoService();
            CreateModernServiciosUI();
            _ = CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            await CargarServiciosAsync();
            await CargarProductosAsync();
        }

        private async Task CargarProductosAsync()
        {
            try
            {
                productosDisponibles = await _productoService.ObtenerTodosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateModernServiciosUI()
        {
            this.Title = "Bella Vista - Gestión de Servicios";
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
                Text = "✂️",
                FontSize = 32,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock titleText = new TextBlock
            {
                Text = "Gestión de Servicios",
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
            CreateServiciosList(contentGrid);

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
                Text = "🆕 Nuevo Servicio",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                Margin = new Thickness(0, 0, 0, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            this.RegisterName("lblTituloForm", formTitle);
            formPanel.Children.Add(formTitle);

            CreateFormField(formPanel, "Nombre del Servicio", "txtNombre");
            CreateFormFieldNumeric(formPanel, "Precio ($)", "txtPrecio");
            CreateFormFieldNumeric(formPanel, "Duración (minutos)", "txtDuracion");
            CreateFormField(formPanel, "Descripción", "txtDescripcion", true);

            TextBlock productosTitle = new TextBlock
            {
                Text = "📦 Productos Utilizados",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                Margin = new Thickness(0, 20, 0, 10)
            };
            formPanel.Children.Add(productosTitle);

            StackPanel productosPanel = new StackPanel
            {
                Name = "productosPanel",
                Margin = new Thickness(0, 0, 0, 15)
            };
            this.RegisterName("productosPanel", productosPanel);
            formPanel.Children.Add(productosPanel);

            Border agregarProductoBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromRgb(155, 89, 182)),
                Margin = new Thickness(0, 0, 0, 20)
            };

            Button agregarProductoButton = new Button
            {
                Name = "btnAgregarProducto",
                Content = "➕ Agregar Producto",
                Height = 45,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            agregarProductoButton.Click += AgregarProducto_Click;
            agregarProductoBorder.Child = agregarProductoButton;
            formPanel.Children.Add(agregarProductoBorder);

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
                    Height = 120,
                    Padding = new Thickness(15),
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
                    Height = 50,
                    Padding = new Thickness(15),
                    FontSize = 14,
                    BorderThickness = new Thickness(0),
                    Background = Brushes.Transparent
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
                Height = 60,
                Padding = new Thickness(15),
                FontSize = 14,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent
            };

            textBox.PreviewTextInput += NumericTextBox_PreviewTextInput;

            inputBorder.Child = textBox;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, textBox);
        }

        private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            e.Handled = !IsTextNumeric(e.Text, textBox.Text);
        }

        private bool IsTextNumeric(string text, string currentText)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',')
                    return false;
            }
            return true;
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
                Content = "✏️ Actualizar Servicio",
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
                Content = "💾 Guardar Servicio",
                Height = 50,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            guardarBorder.Child = guardarButton;

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

            guardarButton.Click += GuardarServicio_Click;
            limpiarButton.Click += LimpiarCampos_Click;
            actualizarButton.Click += ActualizarServicio_Click;

            this.RegisterName("btnActualizar", actualizarButton);
            this.RegisterName("borderActualizar", actualizarBorder);
            this.RegisterName("btnGuardar", guardarButton);
            this.RegisterName("borderGuardar", guardarBorder);

            buttonPanel.Children.Add(actualizarBorder);
            buttonPanel.Children.Add(guardarBorder);
            buttonPanel.Children.Add(limpiarBorder);
            parent.Children.Add(buttonPanel);
        }

        private void CreateServiciosList(Grid contentGrid)
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
            CreateServiciosDataGrid(listGrid);

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
                Text = "📋 Lista de Servicios",
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
                Text = "🔍 Buscar servicio...",
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

        private void CreateServiciosDataGrid(Grid listGrid)
        {
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 10, 0, 0)
            };

            StackPanel serviciosPanel = new StackPanel
            {
                Name = "serviciosPanel"
            };

            this.RegisterName("serviciosPanel", serviciosPanel);
            scrollViewer.Content = serviciosPanel;

            Grid.SetRow(scrollViewer, 1);
            listGrid.Children.Add(scrollViewer);
        }

        private void RefreshServiciosList()
        {
            StackPanel serviciosPanel = (StackPanel)this.FindName("serviciosPanel");
            serviciosPanel.Children.Clear();

            foreach (var servicio in servicios)
            {
                CreateServicioCard(serviciosPanel, servicio);
            }
        }

        private void CreateServicioCard(StackPanel parent, Servicio servicio)
        {
            Border cardBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30, 102, 126, 234)),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(20, 5, 20, 5),
                Padding = new Thickness(20, 15, 20, 15)
            };

            Grid cardGrid = new Grid();
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel infoPanel = new StackPanel();

            TextBlock nombreText = new TextBlock
            {
                Text = $"✂️ {servicio.Nombre}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock precioText = new TextBlock
            {
                Text = $"💰 ${servicio.Precio:F2}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 3)
            };

            TextBlock duracionText = new TextBlock
            {
                Text = $"⏱️ {servicio.Duracion} minutos",
                FontSize = 14,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 10)
            };

            infoPanel.Children.Add(nombreText);
            infoPanel.Children.Add(precioText);
            infoPanel.Children.Add(duracionText);

            if (servicio.Productos.Count > 0)
            {
                TextBlock productosLabel = new TextBlock
                {
                    Text = "📦 Productos:",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(155, 89, 182)),
                    Margin = new Thickness(0, 0, 0, 5)
                };
                infoPanel.Children.Add(productosLabel);

                foreach (var prod in servicio.Productos)
                {
                    TextBlock prodText = new TextBlock
                    {
                        Text = $"  • {prod.NombreProducto} ({prod.Cantidad})",
                        FontSize = 11,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(0, 0, 0, 2)
                    };
                    infoPanel.Children.Add(prodText);
                }
            }

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top
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
                Tag = servicio
            };

            editBorder.Child = editButton;

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
                Tag = servicio
            };

            deleteBorder.Child = deleteButton;

            editButton.Click += EditarServicio_Click;
            deleteButton.Click += EliminarServicio_Click;

            buttonPanel.Children.Add(editBorder);
            buttonPanel.Children.Add(deleteBorder);

            Grid.SetRow(infoPanel, 0);
            Grid.SetRow(buttonPanel, 0);
            Grid.SetColumn(infoPanel, 0);
            Grid.SetColumn(buttonPanel, 1);
            cardGrid.Children.Add(infoPanel);
            cardGrid.Children.Add(buttonPanel);

            cardBorder.Child = cardGrid;
            parent.Children.Add(cardBorder);
        }

        private async Task CargarServiciosAsync()
        {
            try
            {
                servicios = await _servicioService.ObtenerTodosAsync();
                RefreshServiciosList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar servicios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void GuardarServicio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nombre = ((TextBox)this.FindName("txtNombre")).Text.Trim();
                var precioText = ((TextBox)this.FindName("txtPrecio")).Text.Trim();
                var duracionText = ((TextBox)this.FindName("txtDuracion")).Text.Trim();
                var descripcion = ((TextBox)this.FindName("txtDescripcion")).Text.Trim();

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    MessageBox.Show("El nombre del servicio es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(precioText))
                {
                    MessageBox.Show("El precio es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(duracionText))
                {
                    MessageBox.Show("La duración es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(precioText.Replace(',', '.'), out decimal precio) || precio <= 0)
                {
                    MessageBox.Show("El precio debe ser un número válido mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(duracionText, out int duracion) || duracion <= 0)
                {
                    MessageBox.Show("La duración debe ser un número válido mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nuevoServicio = new Servicio
                {
                    Nombre = nombre,
                    Precio = precio,
                    Duracion = duracion,
                    Descripcion = descripcion
                };

                bool resultado = await _servicioService.AgregarAsync(nuevoServicio);

                if (resultado)
                {
                    await CargarServiciosAsync();
                    LimpiarCampos();
                    MessageBox.Show("Servicio guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al guardar el servicio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ActualizarServicio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (servicioEditando == null) return;

                var nombre = ((TextBox)this.FindName("txtNombre")).Text.Trim();
                var precioText = ((TextBox)this.FindName("txtPrecio")).Text.Trim();
                var duracionText = ((TextBox)this.FindName("txtDuracion")).Text.Trim();
                var descripcion = ((TextBox)this.FindName("txtDescripcion")).Text.Trim();

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    MessageBox.Show("El nombre del servicio es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(precioText))
                {
                    MessageBox.Show("El precio es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(duracionText))
                {
                    MessageBox.Show("La duración es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(precioText.Replace(',', '.'), out decimal precio) || precio <= 0)
                {
                    MessageBox.Show("El precio debe ser un número válido mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(duracionText, out int duracion) || duracion <= 0)
                {
                    MessageBox.Show("La duración debe ser un número válido mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                servicioEditando.Nombre = nombre;
                servicioEditando.Precio = precio;
                servicioEditando.Duracion = duracion;
                servicioEditando.Descripcion = descripcion;

                bool resultado = await _servicioService.ActualizarAsync(servicioEditando);

                if (resultado)
                {
                    await CargarServiciosAsync();
                    CambiarModoCreacion();
                    LimpiarCampos();
                    MessageBox.Show("Servicio actualizado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al actualizar el servicio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            ((TextBox)this.FindName("txtPrecio")).Text = "";
            ((TextBox)this.FindName("txtDuracion")).Text = "";
            ((TextBox)this.FindName("txtDescripcion")).Text = "";

            StackPanel productosPanel = (StackPanel)this.FindName("productosPanel");
            productosPanel.Children.Clear();
        }

        private void EditarServicio_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Servicio servicio = (Servicio)button.Tag;
            servicioEditando = servicio;

            ((TextBox)this.FindName("txtNombre")).Text = servicio.Nombre;
            ((TextBox)this.FindName("txtPrecio")).Text = servicio.Precio.ToString("F2");
            ((TextBox)this.FindName("txtDuracion")).Text = servicio.Duracion.ToString();
            ((TextBox)this.FindName("txtDescripcion")).Text = servicio.Descripcion;

            RefreshProductosPanel(servicio);
            CambiarModoEdicion();
        }

        private void RefreshProductosPanel(Servicio servicio)
        {
            StackPanel productosPanel = (StackPanel)this.FindName("productosPanel");
            productosPanel.Children.Clear();

            foreach (var prod in servicio.Productos)
            {
                Border productoBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(50, 155, 89, 182)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                Grid productoGrid = new Grid();
                productoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                productoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                StackPanel productoInfo = new StackPanel();

                TextBlock productoNombre = new TextBlock
                {
                    Text = prod.NombreProducto,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(155, 89, 182))
                };

                TextBlock productoCantidad = new TextBlock
                {
                    Text = $"Cantidad: {prod.Cantidad}",
                    FontSize = 11,
                    Foreground = Brushes.Gray
                };

                productoInfo.Children.Add(productoNombre);
                productoInfo.Children.Add(productoCantidad);

                Button eliminarButton = new Button
                {
                    Content = "✕",
                    Width = 25,
                    Height = 25,
                    Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontSize = 12,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = prod
                };

                eliminarButton.Click += EliminarProductoDelServicio_Click;

                Grid.SetColumn(productoInfo, 0);
                Grid.SetColumn(eliminarButton, 1);
                productoGrid.Children.Add(productoInfo);
                productoGrid.Children.Add(eliminarButton);

                productoBorder.Child = productoGrid;
                productosPanel.Children.Add(productoBorder);
            }
        }

        private async void AgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (servicioEditando == null)
            {
                MessageBox.Show("Debe seleccionar un servicio para agregar productos.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (servicioEditando.CategoriasPermitidas.Count == 0)
            {
                MessageBox.Show($"El servicio '{servicioEditando.Nombre}' no tiene categorías permitidas configuradas.\n\nConfigure las categorías desde el panel de administración.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var availableProducts = productosDisponibles
                .Where(p => p.StockActual > 0 && servicioEditando.CategoriasPermitidas.Contains(p.Categoria))
                .ToList();

            if (availableProducts.Count == 0)
            {
                MessageBox.Show("No hay productos con stock disponible en las categorías permitidas para este servicio.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var window = new Window
            {
                Title = "Agregar Producto al Servicio",
                Width = 450,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            StackPanel mainPanel = new StackPanel { Margin = new Thickness(20) };

            TextBlock lblServicio = new TextBlock 
            { 
                Text = $"Servicio: {servicioEditando.Nombre}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                Margin = new Thickness(0, 0, 0, 15)
            };

            TextBlock lblProducto = new TextBlock { Text = "Seleccionar Producto:", Margin = new Thickness(0, 0, 0, 10) };
            ComboBox cmbProductos = new ComboBox
            {
                Height = 35,
                Margin = new Thickness(0, 0, 0, 8),
                ItemsSource = availableProducts,
                DisplayMemberPath = "Nombre",
                SelectedValuePath = "IdProducto"
            };

            TextBlock lblCategoria = new TextBlock { Text = "Categoría: -", Margin = new Thickness(0, 0, 0, 10), FontSize = 11, Foreground = Brushes.Gray };
            TextBlock lblStock = new TextBlock { Text = "Stock disponible: -", Margin = new Thickness(0, 0, 0, 10), FontSize = 11, Foreground = Brushes.Gray };

            TextBlock lblCantidad = new TextBlock { Text = "Cantidad:", Margin = new Thickness(0, 0, 0, 10) };
            TextBox txtCantidad = new TextBox
            {
                Height = 35,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 20)
            };

            cmbProductos.SelectionChanged += (s, ev) =>
            {
                if (cmbProductos.SelectedItem is Producto selProd)
                {
                    lblCategoria.Text = $"Categoría: {selProd.Categoria}";
                    lblStock.Text = $"Stock disponible: {selProd.StockActual}";
                }
                else
                {
                    lblCategoria.Text = "Categoría: -";
                    lblStock.Text = "Stock disponible: -";
                }
            };

            StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

            Button btnAgregar = new Button
            {
                Content = "✅ Agregar",
                Width = 100,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 10, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            Button btnCancelar = new Button
            {
                Content = "❌ Cancelar",
                Width = 100,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            btnAgregar.Click += async (s, e) =>
            {
                if (cmbProductos.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar un producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtCantidad.Text, out decimal cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("Ingrese una cantidad válida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedProduct = (Producto)cmbProductos.SelectedItem;
                if (cantidad > selectedProduct.StockActual)
                {
                    MessageBox.Show($"No hay suficiente stock. Stock disponible: {selectedProduct.StockActual}", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    int idProducto = (int)cmbProductos.SelectedValue;
                    bool resultado = await _servicioService.AgregarProductoAlServicioAsync(servicioEditando.IdServicio, idProducto, cantidad);

                    if (resultado)
                    {
                        await CargarServiciosAsync();
                        var servicioActualizado = servicios.FirstOrDefault(s => s.IdServicio == servicioEditando.IdServicio);
                        if (servicioActualizado != null)
                        {
                            servicioEditando = servicioActualizado;
                            RefreshProductosPanel(servicioEditando);
                        }
                        window.Close();
                        MessageBox.Show($"Producto agregado al servicio y movimiento registrado automáticamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            btnCancelar.Click += (s, e) => window.Close();

            buttonPanel.Children.Add(btnAgregar);
            buttonPanel.Children.Add(btnCancelar);

            mainPanel.Children.Add(lblServicio);
            mainPanel.Children.Add(lblProducto);
            mainPanel.Children.Add(cmbProductos);
            mainPanel.Children.Add(lblCategoria);
            mainPanel.Children.Add(lblStock);
            mainPanel.Children.Add(lblCantidad);
            mainPanel.Children.Add(txtCantidad);
            mainPanel.Children.Add(buttonPanel);

            window.Content = mainPanel;
            window.ShowDialog();
        }

        private async void EliminarProductoDelServicio_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ServicioProducto servicioProducto = (ServicioProducto)button.Tag;

            var result = MessageBox.Show($"¿Eliminar {servicioProducto.NombreProducto} del servicio?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool resultado = await _servicioService.EliminarProductoDelServicioAsync(servicioProducto.IdServicioProducto);

                    if (resultado)
                    {
                        await CargarServiciosAsync();
                        var servicioActualizado = servicios.FirstOrDefault(s => s.IdServicio == servicioEditando.IdServicio);
                        if (servicioActualizado != null)
                        {
                            servicioEditando = servicioActualizado;
                            RefreshProductosPanel(servicioEditando);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EliminarServicio_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Servicio servicio = (Servicio)button.Tag;

            var result = MessageBox.Show($"¿Está seguro de eliminar el servicio '{servicio.Nombre}'?\n\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool resultado = await _servicioService.EliminarAsync(servicio.IdServicio);

                    if (resultado)
                    {
                        await CargarServiciosAsync();
                        MessageBox.Show("Servicio eliminado exitosamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
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

            lblTitulo.Text = "✏️ Editar Servicio";
            borderActualizar.Visibility = Visibility.Visible;
            borderGuardar.Visibility = Visibility.Collapsed;
        }

        private void CambiarModoCreacion()
        {
            var lblTitulo = (TextBlock)this.FindName("lblTituloForm");
            var borderActualizar = (Border)this.FindName("borderActualizar");
            var borderGuardar = (Border)this.FindName("borderGuardar");

            lblTitulo.Text = "🆕 Nuevo Servicio";
            borderActualizar.Visibility = Visibility.Collapsed;
            borderGuardar.Visibility = Visibility.Visible;
            servicioEditando = null;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "🔍 Buscar servicio...")
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
                textBox.Text = "🔍 Buscar servicio...";
                textBox.Foreground = Brushes.Gray;
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string searchText = textBox.Text;

            if (searchText == "🔍 Buscar servicio..." || string.IsNullOrWhiteSpace(searchText))
            {
                await CargarServiciosAsync();
                return;
            }

            try
            {
                var serviciosFiltrados = await _servicioService.BuscarAsync(searchText);
                servicios = serviciosFiltrados;
                RefreshServiciosList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en búsqueda: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
