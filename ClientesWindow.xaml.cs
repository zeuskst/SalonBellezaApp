using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SalonBellezaApp
{
    public partial class ClientesWindow : Window
    {
        
        private readonly ClienteService _clienteService;
        private List<Cliente> clientes = new List<Cliente>();
        private Cliente clienteEditando = null;

        public ClientesWindow()
        {
            InitializeComponent();
            _clienteService = new ClienteService();
            CreateModernClientesUI();
            _ = CargarClientesAsync();
        }

        private void CreateModernClientesUI()
        {
        
            this.Title = "Bella Vista - Gestión de Clientes";
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
                Text = "👥",
                FontSize = 32,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock titleText = new TextBlock
            {
                Text = "Gestión de Clientes",
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

            
            CreateClientesList(contentGrid);

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
                Text = "🆕 Nuevo Cliente",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                Margin = new Thickness(0, 0, 0, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            this.RegisterName("lblTituloForm", formTitle);
            formPanel.Children.Add(formTitle);

           
            CreateFormField(formPanel, "Nombre", "txtNombre");
            CreateFormField(formPanel, "Apellido Paterno", "txtPaterno");  
            CreateFormField(formPanel, "Apellido Materno", "txtMaterno");
            CreateFormField(formPanel, "Teléfono", "txtTelefono");
            CreateFormField(formPanel, "Correo Electrónico", "txtCorreo");
            CreateFormField(formPanel, "Preferencias", "txtPreferencias", true);
            CreateFormField(formPanel, "Alergias", "txtAlergias", true);
            CreateFormField(formPanel, "Comentarios", "txtComentarios", true);

           
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
                Margin = new Thickness(0, 0, 0, 25)
            };

            TextBox textBox;

            if (isMultiline)
            {
                textBox = new TextBox
                {
                    Name = name,
                    Height = 90,
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
                    Height = 55,
                    Padding = new Thickness(15),
                    FontSize = 14,
                    BorderThickness = new Thickness(0),
                    Background = Brushes.Transparent
                };

                
                if (name == "txtTelefono")
                {
                    textBox.MaxLength = 10;
                    textBox.PreviewTextInput += TxtTelefono_PreviewTextInput;
                    DataObject.AddPastingHandler(textBox, TxtTelefono_Paste);
                }
            }

            inputBorder.Child = textBox;

            parent.Children.Add(labelText);
            parent.Children.Add(inputBorder);

        
            this.RegisterName(name, textBox);
        }

        private void TxtTelefono_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            
            if (!e.Text.All(char.IsDigit))
            {
                e.Handled = true;
                return;
            }

            
            if (sender is TextBox tb)
            {
                int selectedLength = tb.SelectionLength;
                int currentLength = tb.Text.Length - selectedLength;
                if (currentLength + e.Text.Length > 10)
                {
                    e.Handled = true;
                }
            }
        }

        private void TxtTelefono_Paste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();
                return;
            }

            var pastedText = e.SourceDataObject.GetData(DataFormats.Text) as string ?? string.Empty;
            // Extract digits
            var digits = new string(pastedText.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digits))
            {
                e.CancelCommand();
                return;
            }

            if (sender is TextBox tb)
            {
                int spaceLeft = 10 - (tb.Text.Length - tb.SelectionLength);
                if (digits.Length > spaceLeft)
                    digits = digits.Substring(0, spaceLeft);

                // Insert digits at selection
                e.CancelCommand();
                tb.SelectedText = digits;
            }
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
                Content = "✏️ Actualizar Cliente",
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
                Content = "💾 Guardar Cliente",
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

        
            guardarButton.Click += GuardarCliente_Click;
            limpiarButton.Click += LimpiarCampos_Click;
            actualizarButton.Click += ActualizarCliente_Click;

            
            this.RegisterName("btnActualizar", actualizarButton);
            this.RegisterName("borderActualizar", actualizarBorder);
            this.RegisterName("btnGuardar", guardarButton);
            this.RegisterName("borderGuardar", guardarBorder);

            buttonPanel.Children.Add(actualizarBorder);
            buttonPanel.Children.Add(guardarBorder);
            buttonPanel.Children.Add(limpiarBorder);
            parent.Children.Add(buttonPanel);
        }

        private void CreateClientesList(Grid contentGrid)
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

           
            CreateClientesDataGrid(listGrid);

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
                Text = "📋 Lista de Clientes",
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
                Text = "🔍 Buscar cliente...",
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

        private void CreateClientesDataGrid(Grid listGrid)
        {
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 10, 0, 0)
            };

            StackPanel clientesPanel = new StackPanel
            {
                Name = "clientesPanel"
            };

            this.RegisterName("clientesPanel", clientesPanel);
            scrollViewer.Content = clientesPanel;

            Grid.SetRow(scrollViewer, 1);
            listGrid.Children.Add(scrollViewer);
        }

        private void RefreshClientesList()
        {
            StackPanel clientesPanel = (StackPanel)this.FindName("clientesPanel");
            clientesPanel.Children.Clear();

            foreach (var cliente in clientes)
            {
                CreateClienteCard(clientesPanel, cliente);
            }
        }

        private void CreateClienteCard(StackPanel parent, Cliente cliente)
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
                Text = $"👤 {cliente.Nombre} {cliente.Paterno} {cliente.Materno}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock telefonoText = new TextBlock
            {
                Text = $"📞 {cliente.Telefono}",
                FontSize = 14,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 3)
            };

            TextBlock correoText = new TextBlock
            {
                Text = $"📧 {cliente.Correo}",
                FontSize = 14,
                Foreground = Brushes.Gray
            };

            infoPanel.Children.Add(nombreText);
            infoPanel.Children.Add(telefonoText);
            infoPanel.Children.Add(correoText);

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
                Tag = cliente
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
                Tag = cliente
            };

            deleteBorder.Child = deleteButton;

            editButton.Click += EditarCliente_Click;
            deleteButton.Click += EliminarCliente_Click;

            buttonPanel.Children.Add(editBorder);
            buttonPanel.Children.Add(deleteBorder);

            Grid.SetColumn(infoPanel, 0);
            Grid.SetColumn(buttonPanel, 1);
            cardGrid.Children.Add(infoPanel);
            cardGrid.Children.Add(buttonPanel);

            cardBorder.Child = cardGrid;
            parent.Children.Add(cardBorder);
        }

        private async Task CargarClientesAsync()
        {
            try
            {
                clientes = await _clienteService.ObtenerTodosAsync();
                RefreshClientesList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

      
        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void GuardarCliente_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nombre = ((TextBox)this.FindName("txtNombre")).Text;
                var paterno = ((TextBox)this.FindName("txtPaterno")).Text;  
                var materno = ((TextBox)this.FindName("txtMaterno")).Text;  
                var telefono = ((TextBox)this.FindName("txtTelefono")).Text;
                var correo = ((TextBox)this.FindName("txtCorreo")).Text;
                var preferencias = ((TextBox)this.FindName("txtPreferencias")).Text;
                var alergias = ((TextBox)this.FindName("txtAlergias")).Text;
                var comentarios = ((TextBox)this.FindName("txtComentarios")).Text;

                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(telefono) || string.IsNullOrEmpty(paterno) || string.IsNullOrEmpty(materno))
                {
                    MessageBox.Show("El nombre, apellidos y teléfono son campos obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                
                var telefonoDigits = new string(telefono.Where(char.IsDigit).ToArray());
                if (telefonoDigits.Length != 10)
                {
                    MessageBox.Show("El número telefónico debe contener exactamente 10 dígitos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nuevoCliente = new Cliente
                {
                    Nombre = nombre,
                    Paterno = paterno,
                    Materno = materno,
                    Telefono = telefonoDigits,
                    Correo = correo,
                    Preferencias = preferencias,
                    Alergias = alergias,
                    Comentarios = comentarios
                };

               
                bool resultado = await _clienteService.AgregarAsync(nuevoCliente);

                if (resultado)
                {
                    await CargarClientesAsync(); 
                    LimpiarCampos();
                    MessageBox.Show("Cliente guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al guardar el cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ActualizarCliente_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (clienteEditando == null) return;

               
                var nombre = ((TextBox)this.FindName("txtNombre")).Text;
                var paterno = ((TextBox)this.FindName("txtPaterno")).Text;
                var materno = ((TextBox)this.FindName("txtMaterno")).Text;
                var telefono = ((TextBox)this.FindName("txtTelefono")).Text;

                
                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(paterno) ||
                    string.IsNullOrWhiteSpace(materno) || string.IsNullOrWhiteSpace(telefono))
                {
                    MessageBox.Show("El nombre, apellidos y teléfono son campos obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validar teléfono (exactamente 10 dígitos)
                var telefonoDigits = new string(telefono.Where(char.IsDigit).ToArray());
                if (telefonoDigits.Length != 10)
                {
                    MessageBox.Show("El número telefónico debe contener exactamente 10 dígitos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                clienteEditando.Nombre = nombre;
                clienteEditando.Paterno = paterno;
                clienteEditando.Materno = materno;
                clienteEditando.Telefono = telefonoDigits;
                clienteEditando.Correo = ((TextBox)this.FindName("txtCorreo")).Text;
                clienteEditando.Preferencias = ((TextBox)this.FindName("txtPreferencias")).Text;
                clienteEditando.Alergias = ((TextBox)this.FindName("txtAlergias")).Text;
                clienteEditando.Comentarios = ((TextBox)this.FindName("txtComentarios")).Text;

                
                bool resultado = await _clienteService.ActualizarAsync(clienteEditando);
                if (resultado)
                {
                    await CargarClientesAsync();
                    CambiarModoCreacion();
                    LimpiarCampos();
                    MessageBox.Show("Cliente actualizado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error al actualizar el cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            ((TextBox)this.FindName("txtTelefono")).Text = "";
            ((TextBox)this.FindName("txtCorreo")).Text = "";
            ((TextBox)this.FindName("txtPreferencias")).Text = "";
            ((TextBox)this.FindName("txtAlergias")).Text = "";
            ((TextBox)this.FindName("txtComentarios")).Text = "";
        }

        private void EditarCliente_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Cliente cliente = (Cliente)button.Tag;
            clienteEditando = cliente;

         
            ((TextBox)this.FindName("txtNombre")).Text = cliente.Nombre;
            ((TextBox)this.FindName("txtPaterno")).Text = cliente.Paterno;  
            ((TextBox)this.FindName("txtMaterno")).Text = cliente.Materno;
            ((TextBox)this.FindName("txtTelefono")).Text = cliente.Telefono;
            ((TextBox)this.FindName("txtCorreo")).Text = cliente.Correo;
            ((TextBox)this.FindName("txtPreferencias")).Text = cliente.Preferencias;
            ((TextBox)this.FindName("txtAlergias")).Text = cliente.Alergias;
            ((TextBox)this.FindName("txtComentarios")).Text = cliente.Comentarios;

            CambiarModoEdicion();
        }

        private async void EliminarCliente_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Cliente cliente = (Cliente)button.Tag;

            var result = MessageBox.Show($"¿Está seguro de eliminar al cliente {cliente.Nombre}?",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                
                bool resultado = await _clienteService.EliminarAsync(cliente.IdCliente);

                if (resultado)
                {
                    await CargarClientesAsync();
                    MessageBox.Show("Cliente eliminado exitosamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void CambiarModoEdicion()
        {
            var lblTitulo = (TextBlock)this.FindName("lblTituloForm");
            var borderActualizar = (Border)this.FindName("borderActualizar");
            var borderGuardar = (Border)this.FindName("borderGuardar");

            lblTitulo.Text = "✏️ Editar Cliente";
            borderActualizar.Visibility = Visibility.Visible;
            borderGuardar.Visibility = Visibility.Collapsed;
        }

        private void CambiarModoCreacion()
        {
            var lblTitulo = (TextBlock)this.FindName("lblTituloForm");
            var borderActualizar = (Border)this.FindName("borderActualizar");
            var borderGuardar = (Border)this.FindName("borderGuardar");

            lblTitulo.Text = "🆕 Nuevo Cliente";
            borderActualizar.Visibility = Visibility.Collapsed;
            borderGuardar.Visibility = Visibility.Visible;
            clienteEditando = null;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "🔍 Buscar cliente...")
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
                textBox.Text = "🔍 Buscar cliente...";
                textBox.Foreground = Brushes.Gray;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string searchText = textBox.Text.ToLower();

            if (searchText == "🔍 buscar cliente..." || string.IsNullOrWhiteSpace(searchText))
            {
                RefreshClientesList();
                return;
            }

            var filteredClientes = clientes.Where(c =>
                c.Nombre.ToLower().Contains(searchText) ||
                c.Telefono.Contains(searchText) ||
                c.Correo.ToLower().Contains(searchText)
            ).ToList();

            StackPanel clientesPanel = (StackPanel)this.FindName("clientesPanel");
            clientesPanel.Children.Clear();

            foreach (var cliente in filteredClientes)
            {
                CreateClienteCard(clientesPanel, cliente);
            }
        }
    }
}


