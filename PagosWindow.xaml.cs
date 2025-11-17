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
    public partial class PagosWindow : Window
    {
        private readonly PagoService _pagoService;
        private List<Pago> pagos = new List<Pago>();
        private Pago pagoEditando = null;

        private List<CitaParaPago> citasSinPagar = new List<CitaParaPago>();

        public PagosWindow()
        {
            InitializeComponent();
            _pagoService = new PagoService();
            CreateModernPagosUI();
            _ = CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                citasSinPagar = await _pagoService.ObtenerCitasSinPagarAsync();
                await CargarPagosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateModernPagosUI()
        {
            this.Title = "Bella Vista - Gestión de Pagos";
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
                Text = "💰",
                FontSize = 32,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock titleText = new TextBlock
            {
                Text = "Gestión de Pagos",
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
            CreatePagosList(contentGrid);

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
                Text = "🆕 Registrar Pago",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                Margin = new Thickness(0, 0, 0, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            this.RegisterName("lblTituloForm", formTitle);
            formPanel.Children.Add(formTitle);

            CreateComboBoxField(formPanel, "Cita a Pagar", "cmbCita");
            CreateInfoField(formPanel, "Detalles de Servicios", "txtDetalles");
            CreateMontoField(formPanel, "Monto Total", "txtMonto");
            CreateComboBoxField(formPanel, "Método de Pago", "cmbMetodoPago");
            CreateDatePickerField(formPanel, "Fecha de Pago", "dpFechaPago");

            CreateFormButtons(formPanel);

            formBorder.Child = formScroll;
            Grid.SetColumn(formBorder, 0);
            contentGrid.Children.Add(formBorder);
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

            if (name == "cmbMetodoPago")
            {
                comboBox.Items.Add("efectivo");
                comboBox.Items.Add("tarjeta");
                comboBox.Items.Add("transferencia");
                comboBox.SelectedIndex = 0;
            }
            else if (name == "cmbCita")
            {
                comboBox.SelectionChanged += CmbCita_SelectionChanged;
            }

            inputBorder.Child = comboBox;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, comboBox);
        }

        private void CreateInfoField(StackPanel parent, string label, string name)
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
                Background = new SolidColorBrush(Color.FromArgb(240,240, 240, 240)),
                Margin = new Thickness(0, 0, 0, 20)
            };

            TextBox textBox = new TextBox
            {
                Name = name,
                Height = 100,
                Padding = new Thickness(15, 10, 15, 10),
                FontSize = 13,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            inputBorder.Child = textBox;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, textBox);
        }

        private void CreateMontoField(StackPanel parent, string label, string name)
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
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 46, 204, 113)),
                Background = new SolidColorBrush(Color.FromArgb(230, 255, 255, 255)),
                Margin = new Thickness(0, 0, 0, 20)
            };

            TextBox textBox = new TextBox
            {
                Name = name,
                Height = 50,
                Padding = new Thickness(15, 10, 15, 10),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Foreground = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                IsReadOnly = true,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            inputBorder.Child = textBox;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, textBox);
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
                Background = Brushes.Transparent,
                SelectedDate = DateTime.Now
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
                Content = "✏️ Actualizar Pago",
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
                Content = "💾 Registrar Pago",
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

            guardarButton.Click += GuardarPago_Click;
            limpiarButton.Click += LimpiarCampos_Click;
            actualizarButton.Click += ActualizarPago_Click;

            this.RegisterName("btnActualizar", actualizarButton);
            this.RegisterName("borderActualizar", actualizarBorder);
            this.RegisterName("btnGuardar", guardarButton);
            this.RegisterName("borderGuardar", guardarBorder);

            buttonPanel.Children.Add(actualizarBorder);
            buttonPanel.Children.Add(guardarBorder);
            buttonPanel.Children.Add(limpiarBorder);
            parent.Children.Add(buttonPanel);
        }

        private async void CmbCita_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem != null)
            {
                CitaParaPago cita = (CitaParaPago)comboBox.SelectedItem;

                try
                {
                    
                    decimal monto = await _pagoService.CalcularMontoCitaAsync(cita.IdCita);
                    TextBox txtMonto = (TextBox)this.FindName("txtMonto");
                    txtMonto.Text = $"${monto:F2}";

                    
                    string detalles = await _pagoService.ObtenerDetallesCitaAsync(cita.IdCita);
                    TextBox txtDetalles = (TextBox)this.FindName("txtDetalles");
                    txtDetalles.Text = detalles;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar información: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CreatePagosList(Grid contentGrid)
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
            CreatePagosDataGrid(listGrid);

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
                Text = "📋 Historial de Pagos",
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
                Text = "🔍 Buscar pago...",
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

        private void CreatePagosDataGrid(Grid listGrid)
        {
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 10, 0, 0)
            };

            StackPanel pagosPanel = new StackPanel
            {
                Name = "pagosPanel"
            };

            this.RegisterName("pagosPanel", pagosPanel);
            scrollViewer.Content = pagosPanel;

            Grid.SetRow(scrollViewer, 1);
            listGrid.Children.Add(scrollViewer);
        }

        private void RefreshPagosList()
        {
            StackPanel pagosPanel = (StackPanel)this.FindName("pagosPanel");
            pagosPanel.Children.Clear();

            foreach (var pago in pagos)
            {
                CreatePagoCard(pagosPanel, pago);
            }
        }

        private void CreatePagoCard(StackPanel parent, Pago pago)
        {
            Color metodColor = pago.MetodoPago switch
            {
                "efectivo" => Color.FromRgb(46, 204, 113),
                "tarjeta" => Color.FromRgb(52, 152, 219),
                "transferencia" => Color.FromRgb(155, 89, 182),
                _ => Color.FromRgb(149, 165, 166)
            };

            Border cardBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30, metodColor.R, metodColor.G, metodColor.B)),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(20, 5, 20, 5),
                Padding = new Thickness(20, 15, 20, 15)
            };

            Grid cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel infoPanel = new StackPanel();

            TextBlock clienteText = new TextBlock
            {
                Text = $"👤 {pago.NombreCliente}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock montoText = new TextBlock
            {
                Text = $"💰 ${pago.Monto:F2}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock metodoText = new TextBlock
            {
                Text = $"💳 {pago.MetodoPago.ToUpper()}",
                FontSize = 14,
                Foreground = new SolidColorBrush(metodColor),
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 3)
            };

            TextBlock fechaText = new TextBlock
            {
                Text = $"📅 Pago: {pago.FechaPago:dd/MM/yyyy HH:mm} | Cita: {pago.FechaCita:dd/MM/yyyy}",
                FontSize = 13,
                Foreground = Brushes.Gray
            };

            infoPanel.Children.Add(clienteText);
            infoPanel.Children.Add(montoText);
            infoPanel.Children.Add(metodoText);
            infoPanel.Children.Add(fechaText);

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
                Tag = pago
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
                Tag = pago
            };

            deleteBorder.Child = deleteButton;

            editButton.Click += EditarPago_Click;
            deleteButton.Click += EliminarPago_Click;

            buttonPanel.Children.Add(editBorder);
            buttonPanel.Children.Add(deleteBorder);

            Grid.SetColumn(infoPanel, 0);
            Grid.SetColumn(buttonPanel, 1);
            cardGrid.Children.Add(infoPanel);
            cardGrid.Children.Add(buttonPanel);

            cardBorder.Child = cardGrid;
            parent.Children.Add(cardBorder);
        }

        private async Task CargarPagosAsync()
        {
            try
            {
                pagos = await _pagoService.ObtenerTodosAsync();
                RefreshPagosList();
                CargarComboBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar pagos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarComboBoxes()
        {
            ComboBox cmbCita = (ComboBox)this.FindName("cmbCita");

            cmbCita.ItemsSource = citasSinPagar;
            cmbCita.DisplayMemberPath = "DisplayText";
            cmbCita.SelectedValuePath = "IdCita";
        }

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void GuardarPago_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBox cmbCita = (ComboBox)this.FindName("cmbCita");
                TextBox txtMonto = (TextBox)this.FindName("txtMonto");
                ComboBox cmbMetodoPago = (ComboBox)this.FindName("cmbMetodoPago");
                DatePicker dpFechaPago = (DatePicker)this.FindName("dpFechaPago");

                
                if (cmbCita.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar una cita.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtMonto.Text) || txtMonto.Text == "$0.00")
                {
                    MessageBox.Show("El monto no puede estar vacío.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!dpFechaPago.SelectedDate.HasValue)
                {
                    MessageBox.Show("Debe seleccionar una fecha de pago.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                
                string montoStr = txtMonto.Text.Replace("$", "").Trim();
                if (!decimal.TryParse(montoStr, out decimal monto))
                {
                    MessageBox.Show("El monto no es válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nuevoPago = new Pago
                {
                    IdCita = (int)cmbCita.SelectedValue,
                    Monto = monto,
                    MetodoPago = cmbMetodoPago.SelectedItem.ToString(),
                    FechaPago = dpFechaPago.SelectedDate.Value
                };

                bool resultado = await _pagoService.AgregarAsync(nuevoPago);

                if (resultado)
                {
                    
                    citasSinPagar = await _pagoService.ObtenerCitasSinPagarAsync();
                    await CargarPagosAsync();
                    LimpiarCampos();
                    MessageBox.Show("Pago registrado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al registrar el pago.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ActualizarPago_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pagoEditando == null) return;

                ComboBox cmbMetodoPago = (ComboBox)this.FindName("cmbMetodoPago");
                DatePicker dpFechaPago = (DatePicker)this.FindName("dpFechaPago");

                if (!dpFechaPago.SelectedDate.HasValue)
                {
                    MessageBox.Show("Debe seleccionar una fecha de pago.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                pagoEditando.MetodoPago = cmbMetodoPago.SelectedItem.ToString();
                pagoEditando.FechaPago = dpFechaPago.SelectedDate.Value;

                bool resultado = await _pagoService.ActualizarAsync(pagoEditando);

                if (resultado)
                {
                    await CargarPagosAsync();
                    CambiarModoCreacion();
                    LimpiarCampos();
                    MessageBox.Show("Pago actualizado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al actualizar el pago.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            ComboBox cmbCita = (ComboBox)this.FindName("cmbCita");
            TextBox txtMonto = (TextBox)this.FindName("txtMonto");
            TextBox txtDetalles = (TextBox)this.FindName("txtDetalles");
            ComboBox cmbMetodoPago = (ComboBox)this.FindName("cmbMetodoPago");
            DatePicker dpFechaPago = (DatePicker)this.FindName("dpFechaPago");

            cmbCita.SelectedIndex = -1;
            txtMonto.Text = "";
            txtDetalles.Text = "";
            cmbMetodoPago.SelectedIndex = 0;
            dpFechaPago.SelectedDate = DateTime.Now;
        }

        private void EditarPago_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Pago pago = (Pago)button.Tag;
            pagoEditando = pago;

            
            TextBox txtMonto = (TextBox)this.FindName("txtMonto");
            ComboBox cmbMetodoPago = (ComboBox)this.FindName("cmbMetodoPago");
            DatePicker dpFechaPago = (DatePicker)this.FindName("dpFechaPago");
            ComboBox cmbCita = (ComboBox)this.FindName("cmbCita");

            
            cmbCita.IsEnabled = false;
            txtMonto.Text = $"${pago.Monto:F2}";
            dpFechaPago.SelectedDate = pago.FechaPago;

            
            for (int i = 0; i < cmbMetodoPago.Items.Count; i++)
            {
                if (cmbMetodoPago.Items[i].ToString() == pago.MetodoPago)
                {
                    cmbMetodoPago.SelectedIndex = i;
                    break;
                }
            }

            CambiarModoEdicion();
        }

        private async void EliminarPago_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Pago pago = (Pago)button.Tag;

            var result = MessageBox.Show($"¿Está seguro de eliminar el pago de ${pago.Monto:F2} del cliente '{pago.NombreCliente}'?\n\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool resultado = await _pagoService.EliminarAsync(pago.IdPago);

                    if (resultado)
                    {
                        citasSinPagar = await _pagoService.ObtenerCitasSinPagarAsync();
                        await CargarPagosAsync();
                        MessageBox.Show("Pago eliminado exitosamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
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

            lblTitulo.Text = "✏️ Editar Pago";
            borderActualizar.Visibility = Visibility.Visible;
            borderGuardar.Visibility = Visibility.Collapsed;
        }

        private void CambiarModoCreacion()
        {
            var lblTitulo = (TextBlock)this.FindName("lblTituloForm");
            var borderActualizar = (Border)this.FindName("borderActualizar");
            var borderGuardar = (Border)this.FindName("borderGuardar");
            ComboBox cmbCita = (ComboBox)this.FindName("cmbCita");

            lblTitulo.Text = "🆕 Registrar Pago";
            borderActualizar.Visibility = Visibility.Collapsed;
            borderGuardar.Visibility = Visibility.Visible;
            cmbCita.IsEnabled = true;
            pagoEditando = null;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "🔍 Buscar pago...")
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
                textBox.Text = "🔍 Buscar pago...";
                textBox.Foreground = Brushes.Gray;
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string searchText = textBox.Text;

            if (searchText == "🔍 Buscar pago..." || string.IsNullOrWhiteSpace(searchText))
            {
                await CargarPagosAsync();
                return;
            }

            try
            {
                var pagosFiltrados = await _pagoService.BuscarAsync(searchText);
                pagos = pagosFiltrados;
                RefreshPagosList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en búsqueda: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
