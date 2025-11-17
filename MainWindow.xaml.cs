using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Reflection;

namespace SalonBellezaApp
{
    public partial class MainWindow : Window
    {
        private CitaService _citaService;
        private ClienteService _clienteService;
        private ProductoService _productoService;
        private PagoService _pagoService;

        // UI helpers
        private DispatcherTimer _clockTimer;
        private TextBlock _dateTextBlock;

        public MainWindow()
        {
            InitializeComponent();
            _citaService = new CitaService();
            _clienteService = new ClienteService();
            _productoService = new ProductoService();
            _pagoService = new PagoService();
            CreateModernUI();
            _ = CargarEstadisticasAsync();
        }

        private void CreateModernUI()
        {
            this.Title = "Bella Vista - Sistema de Gestión";
            this.Width = 1200;
            this.Height = 800;
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

            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            CreateSidebar(mainGrid);
            CreateMainContent(mainGrid);
        }

        private void CreateSidebar(Grid mainGrid)
        {
            Border sidebarBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(240, 255, 255, 255)),
                CornerRadius = new CornerRadius(0, 20, 20, 0),
                Margin = new Thickness(0, 20, 0, 20)
            };

            DropShadowEffect shadowEffect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 320,
                ShadowDepth = 5,
                Opacity = 0.3,
                BlurRadius = 20
            };
            sidebarBorder.Effect = shadowEffect;

            StackPanel sidebarPanel = new StackPanel();
            sidebarBorder.Child = sidebarPanel;

            StackPanel logoPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20, 30, 20, 40),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock logoIcon = new TextBlock
            {
                Text = "💇‍♀️",
                FontSize = 32,
                Margin = new Thickness(0, 0, 10, 0)
            };

            TextBlock logoText = new TextBlock
            {
                Text = "Bella Vista",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234))
            };

            logoPanel.Children.Add(logoIcon);
            logoPanel.Children.Add(logoText);
            sidebarPanel.Children.Add(logoPanel);

            string[] menuItems = { "👥 Clientes", "📅 Citas", "✂️ Servicios",
                                   "👨‍💼 Empleados", "💰 Pagos", "📦 Productos","📊 Inventario", "📈 Reportes" };

            foreach (string item in menuItems)
            {
                Button menuButton = CreateMenuButton(item);
                sidebarPanel.Children.Add(menuButton);
            }

            Grid.SetColumn(sidebarBorder, 0);
            mainGrid.Children.Add(sidebarBorder);
        }

        private Button CreateMenuButton(string text)
        {
            Button button = new Button
            {
                Content = text,
                Height = 60,
                Margin = new Thickness(20, 5, 20, 5),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Medium,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(20, 0, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            Style buttonStyle = new Style(typeof(Button));

            Trigger mouseEnterTrigger = new Trigger
            {
                Property = Button.IsMouseOverProperty,
                Value = true
            };
            mouseEnterTrigger.Setters.Add(new Setter(Button.BackgroundProperty,
                new SolidColorBrush(Color.FromArgb(100, 102, 126, 234))));
            mouseEnterTrigger.Setters.Add(new Setter(Button.ForegroundProperty,
                new SolidColorBrush(Color.FromRgb(102, 126, 234))));

            buttonStyle.Triggers.Add(mouseEnterTrigger);
            button.Style = buttonStyle;

            button.Click += MenuButton_Click;
            return button;
        }

        private void CreateMainContent(Grid mainGrid)
        {
            Border mainContentBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(250, 255, 255, 255)),
                CornerRadius = new CornerRadius(20, 0, 0, 20),
                Margin = new Thickness(20, 20, 20, 20)
            };

            DropShadowEffect contentShadow = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 140,
                ShadowDepth = 5,
                Opacity = 0.2,
                BlurRadius = 15
            };
            mainContentBorder.Effect = contentShadow;

            Grid contentGrid = new Grid();
            mainContentBorder.Child = contentGrid;

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            CreateContentHeader(contentGrid);
            CreateDashboard(contentGrid);

            Grid.SetColumn(mainContentBorder, 1);
            mainGrid.Children.Add(mainContentBorder);
        }

        private void CreateContentHeader(Grid contentGrid)
        {
            Border headerBorder = new Border
            {
                Background = new LinearGradientBrush(Color.FromRgb(102, 126, 234),
                                                   Color.FromRgb(118, 75, 162), 0),
                CornerRadius = new CornerRadius(20, 0, 0, 0)
            };

            Grid headerGrid = new Grid { Margin = new Thickness(30, 0, 30, 0) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Left: welcome
            TextBlock welcomeText = new TextBlock
            {
                Text = "Bienvenido al Sistema de Gestión",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(welcomeText, 0);
            headerGrid.Children.Add(welcomeText);

            // Middle: date/time (live)
            _dateTextBlock = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromArgb(230, 255, 255, 255)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 20, 0)
            };
            UpdateClockText();
            Grid.SetColumn(_dateTextBlock, 1);
            headerGrid.Children.Add(_dateTextBlock);

            // Right: user + actions
            StackPanel rightPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            TextBlock userText = new TextBlock
            {
                Text = Environment.UserName,
                FontSize = 14,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            Button btnChangePwd = new Button
            {
                Content = "Cambiar Contraseña",
                Height = 32,
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(10, 0, 10, 0),
                Background = Brushes.White,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            btnChangePwd.Click += BtnChangePassword_Click;

            Button btnLogout = new Button
            {
                Content = "Cerrar Sesión",
                Height = 32,
                Padding = new Thickness(10, 0, 10, 0),
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand
            };
            btnLogout.Click += BtnLogout_Click;

            rightPanel.Children.Add(userText);
            rightPanel.Children.Add(btnChangePwd);
            rightPanel.Children.Add(btnLogout);
            Grid.SetColumn(rightPanel, 2);
            headerGrid.Children.Add(rightPanel);

            headerBorder.Child = headerGrid;
            Grid.SetRow(headerBorder, 0);
            contentGrid.Children.Add(headerBorder);

            // Setup clock timer
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => UpdateClockText();
            _clockTimer.Start();
        }

        private void UpdateClockText()
        {
            if (_dateTextBlock != null)
            {
                _dateTextBlock.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("¿Desea cerrar sesión?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            try
            {
                var login = new LoginWindow();
                login.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir la ventana de login: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.Close();
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Window
            {
                Title = "Cambiar Contraseña",
                Width = 420,
                Height = 360,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow
            };

            var panel = new StackPanel { Margin = new Thickness(20) };
            panel.Children.Add(new TextBlock { Text = "Usuario:", FontWeight = FontWeights.Medium });
            var txtUser = new TextBox { Text = Environment.UserName, IsReadOnly = true, Margin = new Thickness(0, 5, 0, 10) };
            panel.Children.Add(txtUser);

            panel.Children.Add(new TextBlock { Text = "Contraseña Actual:", FontWeight = FontWeights.Medium });
            var pwdCurrent = new PasswordBox { Margin = new Thickness(0, 5, 0, 10) };
            panel.Children.Add(pwdCurrent);

            panel.Children.Add(new TextBlock { Text = "Nueva Contraseña:", FontWeight = FontWeights.Medium });
            var pwdNew = new PasswordBox { Margin = new Thickness(0, 5, 0, 10) };
            panel.Children.Add(pwdNew);

            panel.Children.Add(new TextBlock { Text = "Confirmar Nueva Contraseña:", FontWeight = FontWeights.Medium });
            var pwdConfirm = new PasswordBox { Margin = new Thickness(0, 5, 0, 20) };
            panel.Children.Add(pwdConfirm);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnOk = new Button { Content = "Guardar", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var btnCancel = new Button { Content = "Cancelar", Width = 100 };
            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);
            panel.Children.Add(btnPanel);

            btnCancel.Click += (s, ev) => dlg.Close();
            btnOk.Click += (s, ev) =>
            {
                string user = txtUser.Text.Trim();
                string current = pwdCurrent.Password ?? string.Empty;
                string nw = pwdNew.Password ?? string.Empty;
                string conf = pwdConfirm.Password ?? string.Empty;

                if (string.IsNullOrWhiteSpace(current))
                {
                    MessageBox.Show(dlg, "Ingrese la contraseña actual.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(nw))
                {
                    MessageBox.Show(dlg, "Ingrese la nueva contraseña.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (nw != conf)
                {
                    MessageBox.Show(dlg, "La nueva contraseña y la confirmación no coinciden.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool ok = AuthService.VerifyUserPassword(user, current);
                AuthService.LogAttempt(user, ok, ok ? "ChangePwdCurrentVerified" : "ChangePwdCurrentFailed");
                if (!ok)
                {
                    MessageBox.Show(dlg, "La contraseña actual es incorrecta.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    AuthService.AddOrUpdateUserPassword(user, nw);
                    AuthService.LogAttempt(user, true, "PasswordChanged");
                    MessageBox.Show(dlg, "Contraseña cambiada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    dlg.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(dlg, $"Error al cambiar contraseña: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            dlg.Content = panel;
            dlg.ShowDialog();
        }

        private void CreateDashboard(Grid contentGrid)
        {
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(30, 20, 30, 30)
            };

            Grid dashboardGrid = new Grid();
            scrollViewer.Content = dashboardGrid;

            dashboardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            dashboardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            CreateStatsCards(dashboardGrid);
            CreateRecentAppointments(dashboardGrid);

            Grid.SetRow(scrollViewer, 1);
            contentGrid.Children.Add(scrollViewer);
        }

        private void CreateStatsCards(Grid dashboardGrid)
        {
            UniformGrid statsGrid = new UniformGrid
            {
                Columns = 4,
                Margin = new Thickness(0, 0, 0, 30)
            };

            var statsData = new[]
            {
                new { Title = "Citas Hoy", Name = "statsCitasHoy", Value = "0", Icon = "📅", Color = Color.FromRgb(52, 152, 219) },
                new { Title = "Clientes", Name = "statsClientes", Value = "0", Icon = "👥", Color = Color.FromRgb(155, 89, 182) },
                new { Title = "Ingresos Mes", Name = "statsIngresos", Value = "$0", Icon = "💰", Color = Color.FromRgb(46, 204, 113) },
                new { Title = "Productos", Name = "statsProductos", Value = "0", Icon = "📦", Color = Color.FromRgb(231, 76, 60) }
            };

            foreach (var stat in statsData)
            {
                Border cardBorder = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(15),
                    Margin = new Thickness(10),
                    Height = 120,
                    Name = stat.Name
                };

                DropShadowEffect cardShadow = new DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 270,
                    ShadowDepth = 3,
                    Opacity = 0.3,
                    BlurRadius = 10
                };
                cardBorder.Effect = cardShadow;

                Grid cardGrid = new Grid();
                cardBorder.Child = cardGrid;

                cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                StackPanel textPanel = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 0, 0)
                };

                TextBlock titleText = new TextBlock
                {
                    Text = stat.Title,
                    FontSize = 14,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                TextBlock valueText = new TextBlock
                {
                    Text = stat.Value,
                    FontSize = 28,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(stat.Color),
                    Name = "txtValor"
                };

                textPanel.Children.Add(titleText);
                textPanel.Children.Add(valueText);

                TextBlock iconText = new TextBlock
                {
                    Text = stat.Icon,
                    FontSize = 40,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 20, 0)
                };

                Grid.SetColumn(textPanel, 0);
                Grid.SetColumn(iconText, 1);
                cardGrid.Children.Add(textPanel);
                cardGrid.Children.Add(iconText);

                statsGrid.Children.Add(cardBorder);
                this.RegisterName(stat.Name, cardBorder);
            }

            Grid.SetRow(statsGrid, 0);
            dashboardGrid.Children.Add(statsGrid);
        }

        private void CreateRecentAppointments(Grid dashboardGrid)
        {
            Border appointmentsBorder = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(15),
                Margin = new Thickness(0, 10, 0, 0)
            };

            DropShadowEffect appointmentsShadow = new DropShadowEffect
            {
                Color = Colors.Gray,
                Direction = 270,
                ShadowDepth = 3,
                Opacity = 0.3,
                BlurRadius = 10
            };
            appointmentsBorder.Effect = appointmentsShadow;

            StackPanel appointmentsPanel = new StackPanel
            {
                Margin = new Thickness(25)
            };

            TextBlock appointmentsTitle = new TextBlock
            {
                Text = "Citas de Hoy",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
            };

            appointmentsPanel.Children.Add(appointmentsTitle);

            StackPanel citasPanel = new StackPanel
            {
                Name = "citasDelDiaPanel"
            };

            this.RegisterName("citasDelDiaPanel", citasPanel);
            appointmentsPanel.Children.Add(citasPanel);

            _ = CargarCitasDelDiaAsync();

            appointmentsBorder.Child = appointmentsPanel;
            Grid.SetRow(appointmentsBorder, 1);
            dashboardGrid.Children.Add(appointmentsBorder);
        }

        private async Task CargarCitasDelDiaAsync()
        {
            try
            {
                if (_citaService == null) return;
                var citas = await _citaService.ObtenerCitasDelDiaAsync();

                StackPanel citasPanel = (StackPanel)this.FindName("citasDelDiaPanel");
                if (citasPanel == null) return;

                citasPanel.Children.Clear();

                if (citas.Count == 0)
                {
                    TextBlock sinCitas = new TextBlock
                    {
                        Text = "No hay citas programadas para hoy",
                        FontSize = 14,
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 10, 0, 10)
                    };
                    citasPanel.Children.Add(sinCitas);
                    return;
                }

                foreach (var cita in citas)
                {
                    Border appointmentItem = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(50, 102, 126, 234)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(15),
                        Margin = new Thickness(0, 5, 0, 5)
                    };

                    Grid appointmentGrid = new Grid();
                    appointmentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                    appointmentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                    appointmentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    TextBlock clientText = new TextBlock
                    {
                        Text = $"👤 {cita.NombreCliente}",
                        FontWeight = FontWeights.Medium,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    TextBlock serviceText = new TextBlock
                    {
                        Text = $"✂️ {cita.NombreServicios}",
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    TextBlock timeText = new TextBlock
                    {
                        Text = $"🕐 {cita.HoraInicio.ToString(@"hh\:mm")}",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        FontWeight = FontWeights.Medium,
                        Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234))
                    };

                    Grid.SetColumn(clientText, 0);
                    Grid.SetColumn(serviceText, 1);
                    Grid.SetColumn(timeText, 2);

                    appointmentGrid.Children.Add(clientText);
                    appointmentGrid.Children.Add(serviceText);
                    appointmentGrid.Children.Add(timeText);

                    appointmentItem.Child = appointmentGrid;
                    citasPanel.Children.Add(appointmentItem);
                }

                ActualizarEstadistica("statsCitasHoy", citas.Count.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar citas: {ex.Message}");
            }
        }

        private async Task CargarEstadisticasAsync()
        {
            try
            {
                var citasHoy = await _citaService.ObtenerCitasDelDiaAsync();
                ActualizarEstadistica("statsCitasHoy", citasHoy.Count.ToString());

                var clientes = await _clienteService.ObtenerTodosAsync();
                ActualizarEstadistica("statsClientes", clientes.Count.ToString());

                var productos = await _productoService.ObtenerTodosAsync();
                ActualizarEstadistica("statsProductos", productos.Count.ToString());

                var pagosMes = await _pagoService.ObtenerPagosMesActualAsync();
                decimal totalIngresos = pagosMes.Sum(p => p.Monto);
                ActualizarEstadistica("statsIngresos", $"${totalIngresos:N2}");

                await CargarCitasDelDiaAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar estadísticas: {ex.Message}");
            }
        }

        private void ActualizarEstadistica(string nombreTarjeta, string valor)
        {
            try
            {
                Border tarjeta = (Border)this.FindName(nombreTarjeta);
                if (tarjeta != null && tarjeta.Child is Grid cardGrid)
                {
                    foreach (var child in cardGrid.Children)
                    {
                        if (child is StackPanel textPanel)
                        {
                            foreach (var textChild in textPanel.Children)
                            {
                                if (textChild is TextBlock tb && tb.Name == "txtValor")
                                {
                                    tb.Text = valor;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar estadística: {ex.Message}");
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            string buttonText = clickedButton.Content.ToString();

            switch (buttonText)
            {

                case "👥 Clientes":
                    ClientesWindow clientesWindow = new ClientesWindow();
                    clientesWindow.Closed += (s, args) => CargarEstadisticasAsync();
                    clientesWindow.ShowDialog();
                    break;

                case "✂️ Servicios":
                    ServiciosWindow serviciosWindow = new ServiciosWindow();
                    serviciosWindow.Closed += (s, args) => CargarEstadisticasAsync();
                    serviciosWindow.ShowDialog();
                    break;

                case "👨‍💼 Empleados":
                    EmpleadosWindow empleadosWindow = new EmpleadosWindow();
                    empleadosWindow.Closed += (s, args) => CargarEstadisticasAsync();
                    empleadosWindow.ShowDialog();
                    break;

                case "📅 Citas":
                    CitasWindow citasWindow = new CitasWindow();
                    citasWindow.Closed += (s, args) => CargarEstadisticasAsync();
                    citasWindow.ShowDialog();
                    break;

                case "💰 Pagos":
                    PagosWindow pagosWindow = new PagosWindow();
                    pagosWindow.Closed += (s, args) => CargarEstadisticasAsync();
                    pagosWindow.ShowDialog();
                    break;

                case "📦 Productos":
                    ProductosWindow productosWindow = new ProductosWindow();
                    productosWindow.Closed += (s, args) => CargarEstadisticasAsync();
                    productosWindow.ShowDialog();
                    break;

                case "📊 Inventario":
                    InventarioWindow inventarioWindow = new InventarioWindow();
                    inventarioWindow.Closed += (s, args) => CargarEstadisticasAsync();
                    inventarioWindow.ShowDialog();
                    break;

                case "📈 Reportes":
                    ReportesWindow reportesWindow = new ReportesWindow();
                    reportesWindow.Closed += (s, args) => CargarEstadisticasAsync();
                    reportesWindow.ShowDialog();
                    break;

                default:
                    MessageBox.Show($"Navegando a: {buttonText}", "Navegación", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }
    }
}