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
    public partial class EmpleadosWindow : Window
    {
        private readonly EmpleadoService _empleadoService;
        private List<Empleado> empleados = new List<Empleado>();
        private Empleado empleadoEditando = null;

        public EmpleadosWindow()
        {
            InitializeComponent();
            _empleadoService = new EmpleadoService();
            CreateModernEmpleadosUI();
            _ = CargarEmpleadosAsync();
        }

        private void CreateModernEmpleadosUI()
        {
          
            this.Title = "Bella Vista - Gestión de Empleados";
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
                Text = "👨‍💼",
                FontSize = 32,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock titleText = new TextBlock
            {
                Text = "Gestión de Empleados",
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

            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(400) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            
            CreateFormPanel(contentGrid);

           
            CreateEmpleadosList(contentGrid);

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
                Text = "🆕 Nuevo Empleado",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                Margin = new Thickness(0, 0, 0, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            this.RegisterName("lblTituloForm", formTitle);
            formPanel.Children.Add(formTitle);

    
            CreateFormField(formPanel, "Nombre(s)", "txtNombre");
            CreateFormField(formPanel, "Apellido Paterno", "txtPaterno");
            CreateFormField(formPanel, "Apellido Materno", "txtMaterno");
            CreateFormField(formPanel, "Especialidad", "txtEspecialidad");
            CreateFormField(formPanel, "Horario Disponible", "txtHorarioDisponible", true);

          
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
                    Height = 100,
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
                Content = "✏️ Actualizar Empleado",
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
                Content = "💾 Guardar Empleado",
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

           
            guardarButton.Click += GuardarEmpleado_Click;
            limpiarButton.Click += LimpiarCampos_Click;
            actualizarButton.Click += ActualizarEmpleado_Click;

        
            this.RegisterName("btnActualizar", actualizarButton);
            this.RegisterName("borderActualizar", actualizarBorder);
            this.RegisterName("btnGuardar", guardarButton);
            this.RegisterName("borderGuardar", guardarBorder);

            buttonPanel.Children.Add(actualizarBorder);
            buttonPanel.Children.Add(guardarBorder);
            buttonPanel.Children.Add(limpiarBorder);
            parent.Children.Add(buttonPanel);
        }

        private void CreateEmpleadosList(Grid contentGrid)
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

           
            CreateEmpleadosDataGrid(listGrid);

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
                Text = "📋 Lista de Empleados",
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
                Text = "🔍 Buscar empleado...",
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

        private void CreateEmpleadosDataGrid(Grid listGrid)
        {
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 10, 0, 0)
            };

            StackPanel empleadosPanel = new StackPanel
            {
                Name = "empleadosPanel"
            };

            this.RegisterName("empleadosPanel", empleadosPanel);
            scrollViewer.Content = empleadosPanel;

            Grid.SetRow(scrollViewer, 1);
            listGrid.Children.Add(scrollViewer);
        }

        private void RefreshEmpleadosList()
        {
            StackPanel empleadosPanel = (StackPanel)this.FindName("empleadosPanel");
            empleadosPanel.Children.Clear();

            foreach (var empleado in empleados)
            {
                CreateEmpleadoCard(empleadosPanel, empleado);
            }
        }

        private void CreateEmpleadoCard(StackPanel parent, Empleado empleado)
        {
            Border cardBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30, 102, 126, 234)),
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
                Text = $"👨‍💼 {empleado.Nombre} {empleado.Paterno} {empleado.Materno}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock especialidadText = new TextBlock
            {
                Text = $"⭐ {empleado.Especialidad}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(155, 89, 182)),
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 3)
            };

            TextBlock horarioText = new TextBlock
            {
                Text = $"🕒 {empleado.HorarioDisponible}",
                FontSize = 14,
                Foreground = Brushes.Gray
            };

            infoPanel.Children.Add(nombreText);
            infoPanel.Children.Add(especialidadText);
            infoPanel.Children.Add(horarioText);

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
                Tag = empleado
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
                Tag = empleado
            };

            deleteBorder.Child = deleteButton;

            editButton.Click += EditarEmpleado_Click;
            deleteButton.Click += EliminarEmpleado_Click;

            buttonPanel.Children.Add(editBorder);
            buttonPanel.Children.Add(deleteBorder);

            Grid.SetColumn(infoPanel, 0);
            Grid.SetColumn(buttonPanel, 1);
            cardGrid.Children.Add(infoPanel);
            cardGrid.Children.Add(buttonPanel);

            cardBorder.Child = cardGrid;
            parent.Children.Add(cardBorder);
        }

        private async Task CargarEmpleadosAsync()
        {
            try
            {
                empleados = await _empleadoService.ObtenerTodosAsync();
                RefreshEmpleadosList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar empleados: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

      
        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void GuardarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nombre = ((TextBox)this.FindName("txtNombre")).Text.Trim();
                var paterno = ((TextBox)this.FindName("txtPaterno")).Text.Trim();
                var materno = ((TextBox)this.FindName("txtMaterno")).Text.Trim();
                var especialidad = ((TextBox)this.FindName("txtEspecialidad")).Text.Trim();
                var horarioDisponible = ((TextBox)this.FindName("txtHorarioDisponible")).Text.Trim();

                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(paterno) || string.IsNullOrWhiteSpace(materno))
                {
                    MessageBox.Show("El nombre y los apellidos son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nuevoEmpleado = new Empleado
                {
                    Nombre = nombre,
                    Paterno = paterno,
                    Materno = materno,
                    Especialidad = especialidad,
                    HorarioDisponible = horarioDisponible
                };

                bool resultado = await _empleadoService.AgregarAsync(nuevoEmpleado);

                if (resultado)
                {
                    await CargarEmpleadosAsync();
                    LimpiarCampos();
                    MessageBox.Show("Empleado guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al guardar el empleado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ActualizarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (empleadoEditando == null) return;

                var nombre = ((TextBox)this.FindName("txtNombre")).Text.Trim();
                var paterno = ((TextBox)this.FindName("txtPaterno")).Text.Trim();
                var materno = ((TextBox)this.FindName("txtMaterno")).Text.Trim();
                var especialidad = ((TextBox)this.FindName("txtEspecialidad")).Text.Trim();
                var horarioDisponible = ((TextBox)this.FindName("txtHorarioDisponible")).Text.Trim();

            
                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(paterno) || string.IsNullOrWhiteSpace(materno))
                {
                    MessageBox.Show("El nombre y los apellidos son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                empleadoEditando.Nombre = nombre;
                empleadoEditando.Paterno = paterno;
                empleadoEditando.Materno = materno;
                empleadoEditando.Especialidad = especialidad;
                empleadoEditando.HorarioDisponible = horarioDisponible;

                bool resultado = await _empleadoService.ActualizarAsync(empleadoEditando);

                if (resultado)
                {
                    await CargarEmpleadosAsync();
                    CambiarModoCreacion();
                    LimpiarCampos();
                    MessageBox.Show("Empleado actualizado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al actualizar el empleado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            ((TextBox)this.FindName("txtPaterno")).Text = "";
            ((TextBox)this.FindName("txtMaterno")).Text = "";
            ((TextBox)this.FindName("txtEspecialidad")).Text = "";
            ((TextBox)this.FindName("txtHorarioDisponible")).Text = "";
        }

        private void EditarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Empleado empleado = (Empleado)button.Tag;
            empleadoEditando = empleado;

         
            ((TextBox)this.FindName("txtNombre")).Text = empleado.Nombre;
            ((TextBox)this.FindName("txtPaterno")).Text = empleado.Paterno;
            ((TextBox)this.FindName("txtMaterno")).Text = empleado.Materno;
            ((TextBox)this.FindName("txtEspecialidad")).Text = empleado.Especialidad;
            ((TextBox)this.FindName("txtHorarioDisponible")).Text = empleado.HorarioDisponible;

            CambiarModoEdicion();
        }

        private async void EliminarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Empleado empleado = (Empleado)button.Tag;

            var result = MessageBox.Show($"¿Está seguro de eliminar al empleado '{empleado.Nombre} {empleado.Paterno} {empleado.Materno}'?\n\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool resultado = await _empleadoService.EliminarAsync(empleado.IdEmpleado);

                    if (resultado)
                    {
                        await CargarEmpleadosAsync();
                        MessageBox.Show("Empleado eliminado exitosamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
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

            lblTitulo.Text = "✏️ Editar Empleado";
            borderActualizar.Visibility = Visibility.Visible;
            borderGuardar.Visibility = Visibility.Collapsed;
        }

        private void CambiarModoCreacion()
        {
            var lblTitulo = (TextBlock)this.FindName("lblTituloForm");
            var borderActualizar = (Border)this.FindName("borderActualizar");
            var borderGuardar = (Border)this.FindName("borderGuardar");

            lblTitulo.Text = "🆕 Nuevo Empleado";
            borderActualizar.Visibility = Visibility.Collapsed;
            borderGuardar.Visibility = Visibility.Visible;
            empleadoEditando = null;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "🔍 Buscar empleado...")
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
                textBox.Text = "🔍 Buscar empleado...";
                textBox.Foreground = Brushes.Gray;
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string searchText = textBox.Text;

            if (searchText == "🔍 Buscar empleado..." || string.IsNullOrWhiteSpace(searchText))
            {
                await CargarEmpleadosAsync();
                return;
            }

            try
            {
                var empleadosFiltrados = await _empleadoService.BuscarAsync(searchText);
                empleados = empleadosFiltrados;
                RefreshEmpleadosList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en búsqueda: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
