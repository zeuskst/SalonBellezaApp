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
    public partial class CitasWindow : Window
    {
        private readonly CitaService _citaService;
        private List<Cita> citas = new List<Cita>();
        private Cita citaEditando = null;

        private List<ClienteSimple> clientes = new List<ClienteSimple>();
        private List<ServicioSimple> servicios = new List<ServicioSimple>();
        private List<EmpleadoSimple> empleados = new List<EmpleadoSimple>();

        public CitasWindow()
        {
            InitializeComponent();
            _citaService = new CitaService();
            CreateModernCitasUI();
            _ = CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                clientes = await _citaService.ObtenerClientesAsync();
                servicios = await _citaService.ObtenerServiciosAsync();
                empleados = await _citaService.ObtenerEmpleadosAsync();

                await CargarCitasAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateModernCitasUI()
        {
            this.Title = "Bella Vista - Gestión de Citas";
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
                Text = "📅",
                FontSize = 32,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock titleText = new TextBlock
            {
                Text = "Gestión de Citas",
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
            CreateCitasList(contentGrid);

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
                Text = "🆕 Nueva Cita",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                Margin = new Thickness(0, 0, 0, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            this.RegisterName("lblTituloForm", formTitle);
            formPanel.Children.Add(formTitle);

            CreateComboBoxField(formPanel, "Cliente", "cmbCliente");
            CreateListBoxServiciosField(formPanel, "Servicios (Selección múltiple)", "lstServicios");
            CreateComboBoxField(formPanel, "Empleado", "cmbEmpleado");
            CreateDatePickerField(formPanel, "Fecha", "dpFecha");
            CreateTimePickerField(formPanel, "Hora", "txtHora");
            CreateComboBoxField(formPanel, "Estado", "cmbEstado");

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

            if (name == "cmbEstado")
            {
                comboBox.Items.Add("programada");
                comboBox.Items.Add("cancelada");
                comboBox.Items.Add("completada");
                comboBox.SelectedIndex = 0;
            }

            inputBorder.Child = comboBox;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, comboBox);
        }

        private void CreateListBoxServiciosField(StackPanel parent, string label, string name)
        {
            TextBlock labelText = new TextBlock
            {
                Text = label,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
            };

            TextBlock helpText = new TextBlock
            {
                Text = "Mantén Ctrl presionado para seleccionar varios",
                FontSize = 11,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 5)
            };

            Border inputBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 102, 126, 234)),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };

            ListBox listBox = new ListBox
            {
                Name = name,
                Height = 150,
                Padding = new Thickness(10),
                FontSize = 14,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                SelectionMode = SelectionMode.Multiple
            };

            inputBorder.Child = listBox;

            parent.Children.Add(labelText);
            parent.Children.Add(helpText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, listBox);
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
                SelectedDate = DateTime.Today
            };

            inputBorder.Child = datePicker;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, datePicker);
        }

        private void CreateTimePickerField(StackPanel parent, string label, string name)
        {
            TextBlock labelText = new TextBlock
            {
                Text = label + " (HH:MM formato 24hrs)",
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
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = "09:00"
            };

            inputBorder.Child = textBox;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

            this.RegisterName(name, textBox);
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
                Content = "✏️ Actualizar Cita",
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
                Content = "💾 Guardar Cita",
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

            guardarButton.Click += GuardarCita_Click;
            limpiarButton.Click += LimpiarCampos_Click;
            actualizarButton.Click += ActualizarCita_Click;

            this.RegisterName("btnActualizar", actualizarButton);
            this.RegisterName("borderActualizar", actualizarBorder);
            this.RegisterName("btnGuardar", guardarButton);
            this.RegisterName("borderGuardar", guardarBorder);

            buttonPanel.Children.Add(actualizarBorder);
            buttonPanel.Children.Add(guardarBorder);
            buttonPanel.Children.Add(limpiarBorder);
            parent.Children.Add(buttonPanel);
        }

        private void CreateCitasList(Grid contentGrid)
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
            CreateCitasDataGrid(listGrid);

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
                Text = "📋 Lista de Citas",
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
                Text = "🔍 Buscar cita...",
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

        private void CreateCitasDataGrid(Grid listGrid)
        {
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 10, 0, 0)
            };

            StackPanel citasPanel = new StackPanel
            {
                Name = "citasPanel"
            };

            this.RegisterName("citasPanel", citasPanel);
            scrollViewer.Content = citasPanel;

            Grid.SetRow(scrollViewer, 1);
            listGrid.Children.Add(scrollViewer);
        }

        private void RefreshCitasList()
        {
            StackPanel citasPanel = (StackPanel)this.FindName("citasPanel");
            citasPanel.Children.Clear();

            foreach (var cita in citas)
            {
                CreateCitaCard(citasPanel, cita);
            }
        }

        private void CreateCitaCard(StackPanel parent, Cita cita)
        {
            Color estadoColor = cita.Estado switch
            {
                "programada" => Color.FromRgb(52, 152, 219),
                "completada" => Color.FromRgb(46, 204, 113),
                "cancelada" => Color.FromRgb(231, 76, 60),
                _ => Color.FromRgb(149, 165, 166)
            };

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

            TextBlock clienteText = new TextBlock
            {
                Text = $"👤 {cita.NombreCliente}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock serviciosText = new TextBlock
            {
                Text = $"✂️ {cita.NombreServicios}",
                FontSize = 14,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 3),
                TextWrapping = TextWrapping.Wrap
            };

            TextBlock empleadoText = new TextBlock
            {
                Text = $"👨‍💼 {cita.NombreEmpleado}",
                FontSize = 14,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 3)
            };

            TextBlock fechaText = new TextBlock
            {
                Text = $"📅 {cita.Fecha:dd/MM/yyyy} - 🕐 {cita.HoraInicio.ToString(@"hh\:mm")}",
                FontSize = 14,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 3)
            };

            TextBlock estadoText = new TextBlock
            {
                Text = $"● {cita.Estado.ToUpper()}",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(estadoColor)
            };

            infoPanel.Children.Add(clienteText);
            infoPanel.Children.Add(serviciosText);
            infoPanel.Children.Add(empleadoText);
            infoPanel.Children.Add(fechaText);
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
                Tag = cita
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
                Tag = cita
            };

            deleteBorder.Child = deleteButton;

            editButton.Click += EditarCita_Click;
            deleteButton.Click += EliminarCita_Click;

            buttonPanel.Children.Add(editBorder);
            buttonPanel.Children.Add(deleteBorder);

            Grid.SetColumn(infoPanel, 0);
            Grid.SetColumn(buttonPanel, 1);
            cardGrid.Children.Add(infoPanel);
            cardGrid.Children.Add(buttonPanel);

            cardBorder.Child = cardGrid;
            parent.Children.Add(cardBorder);
        }

        private async Task CargarCitasAsync()
        {
            try
            {
                citas = await _citaService.ObtenerTodosAsync();
                RefreshCitasList();
                CargarComboBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar citas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarComboBoxes()
        {
            ComboBox cmbCliente = (ComboBox)this.FindName("cmbCliente");
            ListBox lstServicios = (ListBox)this.FindName("lstServicios");
            ComboBox cmbEmpleado = (ComboBox)this.FindName("cmbEmpleado");

            cmbCliente.ItemsSource = clientes;
            cmbCliente.DisplayMemberPath = "NombreCompleto";
            cmbCliente.SelectedValuePath = "IdCliente";

            lstServicios.ItemsSource = servicios;
            lstServicios.DisplayMemberPath = "Nombre";
            lstServicios.SelectedValuePath = "IdServicio";

            cmbEmpleado.ItemsSource = empleados;
            cmbEmpleado.DisplayMemberPath = "NombreCompleto";
            cmbEmpleado.SelectedValuePath = "IdEmpleado";
        }

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void GuardarCita_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBox cmbCliente = (ComboBox)this.FindName("cmbCliente");
                ListBox lstServicios = (ListBox)this.FindName("lstServicios");
                ComboBox cmbEmpleado = (ComboBox)this.FindName("cmbEmpleado");
                DatePicker dpFecha = (DatePicker)this.FindName("dpFecha");
                TextBox txtHora = (TextBox)this.FindName("txtHora");
                ComboBox cmbEstado = (ComboBox)this.FindName("cmbEstado");

                
                if (cmbCliente.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un cliente.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (lstServicios.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Debe seleccionar al menos un servicio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbEmpleado.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un empleado.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!dpFecha.SelectedDate.HasValue)
                {
                    MessageBox.Show("Debe seleccionar una fecha.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!TimeSpan.TryParse(txtHora.Text, out TimeSpan hora))
                {
                    MessageBox.Show("La hora debe estar en formato HH:MM (ej: 09:30)", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                List<int> idsServicios = new List<int>();
                foreach (ServicioSimple servicio in lstServicios.SelectedItems)
                {
                    idsServicios.Add(servicio.IdServicio);
                }

                var nuevaCita = new Cita
                {
                    IdCliente = (int)cmbCliente.SelectedValue,
                    IdEmpleado = (int)cmbEmpleado.SelectedValue,
                    Fecha = dpFecha.SelectedDate.Value,
                    HoraInicio = hora,
                    Estado = cmbEstado.SelectedItem.ToString()
                };

                int idCita = await _citaService.AgregarAsync(nuevaCita, idsServicios);

                if (idCita > 0)
                {
                    await CargarCitasAsync();
                    LimpiarCampos();
                    MessageBox.Show("Cita guardada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al guardar la cita.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ActualizarCita_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (citaEditando == null) return;

                ComboBox cmbCliente = (ComboBox)this.FindName("cmbCliente");
                ListBox lstServicios = (ListBox)this.FindName("lstServicios");
                ComboBox cmbEmpleado = (ComboBox)this.FindName("cmbEmpleado");
                DatePicker dpFecha = (DatePicker)this.FindName("dpFecha");
                TextBox txtHora = (TextBox)this.FindName("txtHora");
                ComboBox cmbEstado = (ComboBox)this.FindName("cmbEstado");

               
                if (cmbCliente.SelectedValue == null || lstServicios.SelectedItems.Count == 0 ||
                    cmbEmpleado.SelectedValue == null || !dpFecha.SelectedDate.HasValue)
                {
                    MessageBox.Show("Todos los campos son obligatorios y debe seleccionar al menos un servicio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!TimeSpan.TryParse(txtHora.Text, out TimeSpan hora))
                {
                    MessageBox.Show("La hora debe estar en formato HH:MM (ej: 09:30)", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

               
                List<int> idsServicios = new List<int>();
                foreach (ServicioSimple servicio in lstServicios.SelectedItems)
                {
                    idsServicios.Add(servicio.IdServicio);
                }

                citaEditando.IdCliente = (int)cmbCliente.SelectedValue;
                citaEditando.IdEmpleado = (int)cmbEmpleado.SelectedValue;
                citaEditando.Fecha = dpFecha.SelectedDate.Value;
                citaEditando.HoraInicio = hora;
                citaEditando.Estado = cmbEstado.SelectedItem.ToString();

                bool resultado = await _citaService.ActualizarAsync(citaEditando, idsServicios);

                if (resultado)
                {
                    await CargarCitasAsync();
                    CambiarModoCreacion();
                    LimpiarCampos();
                    MessageBox.Show("Cita actualizada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al actualizar la cita.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            ComboBox cmbCliente = (ComboBox)this.FindName("cmbCliente");
            ListBox lstServicios = (ListBox)this.FindName("lstServicios");
            ComboBox cmbEmpleado = (ComboBox)this.FindName("cmbEmpleado");
            DatePicker dpFecha = (DatePicker)this.FindName("dpFecha");
            TextBox txtHora = (TextBox)this.FindName("txtHora");
            ComboBox cmbEstado = (ComboBox)this.FindName("cmbEstado");

            cmbCliente.SelectedIndex = -1;
            lstServicios.SelectedItems.Clear();
            cmbEmpleado.SelectedIndex = -1;
            dpFecha.SelectedDate = DateTime.Today;
            txtHora.Text = "09:00";
            cmbEstado.SelectedIndex = 0;
        }

        private void EditarCita_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Cita cita = (Cita)button.Tag;
            citaEditando = cita;

            
            ComboBox cmbCliente = (ComboBox)this.FindName("cmbCliente");
            ListBox lstServicios = (ListBox)this.FindName("lstServicios");
            ComboBox cmbEmpleado = (ComboBox)this.FindName("cmbEmpleado");
            DatePicker dpFecha = (DatePicker)this.FindName("dpFecha");
            TextBox txtHora = (TextBox)this.FindName("txtHora");
            ComboBox cmbEstado = (ComboBox)this.FindName("cmbEstado");

            cmbCliente.SelectedValue = cita.IdCliente;
            cmbEmpleado.SelectedValue = cita.IdEmpleado;
            dpFecha.SelectedDate = cita.Fecha;
            txtHora.Text = cita.HoraInicio.ToString(@"hh\:mm");

         
            lstServicios.SelectedItems.Clear();
            foreach (var servicio in cita.Servicios)
            {
                foreach (ServicioSimple item in lstServicios.Items)
                {
                    if (item.IdServicio == servicio.IdServicio)
                    {
                        lstServicios.SelectedItems.Add(item);
                        break;
                    }
                }
            }

            
            for (int i = 0; i < cmbEstado.Items.Count; i++)
            {
                if (cmbEstado.Items[i].ToString() == cita.Estado)
                {
                    cmbEstado.SelectedIndex = i;
                    break;
                }
            }

            CambiarModoEdicion();
        }

        private async void EliminarCita_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Cita cita = (Cita)button.Tag;

            var result = MessageBox.Show($"¿Está seguro de eliminar la cita de '{cita.NombreCliente}' del {cita.Fecha:dd/MM/yyyy}?\n\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool resultado = await _citaService.EliminarAsync(cita.IdCita);

                    if (resultado)
                    {
                        await CargarCitasAsync();
                        MessageBox.Show("Cita eliminada exitosamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
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

            lblTitulo.Text = "✏️ Editar Cita";
            borderActualizar.Visibility = Visibility.Visible;
            borderGuardar.Visibility = Visibility.Collapsed;
        }

        private void CambiarModoCreacion()
        {
            var lblTitulo = (TextBlock)this.FindName("lblTituloForm");
            var borderActualizar = (Border)this.FindName("borderActualizar");
            var borderGuardar = (Border)this.FindName("borderGuardar");

            lblTitulo.Text = "🆕 Nueva Cita";
            borderActualizar.Visibility = Visibility.Collapsed;
            borderGuardar.Visibility = Visibility.Visible;
            citaEditando = null;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "🔍 Buscar cita...")
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
                textBox.Text = "🔍 Buscar cita...";
                textBox.Foreground = Brushes.Gray;
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string searchText = textBox.Text;

            if (searchText == "🔍 Buscar cita..." || string.IsNullOrWhiteSpace(searchText))
            {
                await CargarCitasAsync();
                return;
            }

            try
            {
                var citasFiltradas = await _citaService.BuscarAsync(searchText);
                citas = citasFiltradas;
                RefreshCitasList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en búsqueda: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
