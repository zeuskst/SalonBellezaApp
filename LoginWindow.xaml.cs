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

namespace SalonBellezaApp
{
    /// <summary>
    /// Lógica de interacción para LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            CreateModernLoginUI();
        }

        private void CreateModernLoginUI()
        {
            // Match MainWindow size and state
            this.Title = "Bella Vista - Login";
            this.Width = 1200;
            this.Height = 800;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowState = WindowState.Maximized;

            LinearGradientBrush backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(1, 1);
            backgroundBrush.GradientStops.Add(new GradientStop(Color.FromRgb(102, 126, 234), 0));
            backgroundBrush.GradientStops.Add(new GradientStop(Color.FromRgb(118, 75, 162), 1));
            this.Background = backgroundBrush;

            Grid mainGrid = new Grid();
            this.Content = mainGrid;

            // Main container
            Border containerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(250, 255, 255, 255)),
                CornerRadius = new CornerRadius(20),
                Margin = new Thickness(60),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            DropShadowEffect containerShadow = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 5,
                Opacity = 0.3,
                BlurRadius = 20
            };
            containerBorder.Effect = containerShadow;

            StackPanel containerPanel = new StackPanel
            {
                Margin = new Thickness(40),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Logo and title
            StackPanel logoPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 30),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock logoIcon = new TextBlock
            {
                Text = "\uD83D\uDC87\u200D\u2640\uFE0F",
                FontSize = 48,
                Margin = new Thickness(0, 0, 15, 0)
            };

            TextBlock logoText = new TextBlock
            {
                Text = "Bella Vista",
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                VerticalAlignment = VerticalAlignment.Center
            };

            logoPanel.Children.Add(logoIcon);
            logoPanel.Children.Add(logoText);
            containerPanel.Children.Add(logoPanel);

            // Subtitle
            TextBlock subtitleText = new TextBlock
            {
                Text = "Sistema de Gestión",
                FontSize = 14,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 40)
            };
            containerPanel.Children.Add(subtitleText);

            // Usuario label
            TextBlock lblUsuario = new TextBlock
            {
                Text = "Usuario:",
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
            };
            containerPanel.Children.Add(lblUsuario);

            // Usuario TextBox
            Border usuarioBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 102, 126, 234)),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };

            TextBox txtUsuario = new TextBox
            {
                Name = "txtUsuario",
                Height = 50,
                Padding = new Thickness(15),
                FontSize = 14,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Text = Environment.UserName
            };

            usuarioBorder.Child = txtUsuario;
            containerPanel.Children.Add(usuarioBorder);
            this.RegisterName("txtUsuario", txtUsuario);

            // Contraseña label
            TextBlock lblContraseña = new TextBlock
            {
                Text = "Contraseña:",
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
            };
            containerPanel.Children.Add(lblContraseña);

            // Contraseña PasswordBox
            Border contraseñaBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 102, 126, 234)),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 30)
            };

            PasswordBox pwdContraseña = new PasswordBox
            {
                Name = "pwdContraseña",
                Height = 50,
                Padding = new Thickness(15),
                FontSize = 14,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent
            };

            contraseñaBorder.Child = pwdContraseña;
            containerPanel.Children.Add(contraseñaBorder);
            this.RegisterName("pwdContraseña", pwdContraseña);

            // Botón Login
            Border loginBorder = new Border
            {
                CornerRadius = new CornerRadius(25),
                Background = new LinearGradientBrush(Color.FromRgb(102, 126, 234), Color.FromRgb(118, 75, 162), 0),
                Margin = new Thickness(0, 0, 0, 15)
            };

            Button btnLogin = new Button
            {
                Content = "Iniciar Sesión",
                Height = 50,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand
            };

            btnLogin.Click += BtnLogin_Click;
            loginBorder.Child = btnLogin;
            containerPanel.Children.Add(loginBorder);

            // Botón Salir + Cambiar contraseña panel
            StackPanel bottomButtons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

            Border changeBorder = new Border
            {
                CornerRadius = new CornerRadius(25),
                Background = new SolidColorBrush(Color.FromRgb(155, 89, 182)),
                Margin = new Thickness(0, 0, 10, 0)
            };

            Button btnChange = new Button
            {
                Content = "Cambiar Contraseña",
                Height = 45,
                Width = 200,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Cursor = Cursors.Hand
            };
            btnChange.Click += BtnChangePassword_Click;
            changeBorder.Child = btnChange;

            Border exitBorder = new Border
            {
                CornerRadius = new CornerRadius(25),
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60))
            };

            Button btnSalir = new Button
            {
                Content = "Salir",
                Height = 45,
                Width = 100,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Cursor = Cursors.Hand
            };

            btnSalir.Click += (s, e) => this.Close();
            exitBorder.Child = btnSalir;

            bottomButtons.Children.Add(changeBorder);
            bottomButtons.Children.Add(exitBorder);
            containerPanel.Children.Add(bottomButtons);

            // Info text
            TextBlock infoText = new TextBlock
            {
                Text = "Usa tu usuario del sistema y contraseña: admin (se guarda de forma segura)",
                FontSize = 11,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 30, 0, 0),
                FontStyle = FontStyles.Italic
            };
            containerPanel.Children.Add(infoText);

            containerBorder.Child = containerPanel;
            mainGrid.Children.Add(containerBorder);
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Dialog window to change password for current user
            var dlg = new Window
            {
                Title = "Cambiar Contraseña",
                Width = 420,
                Height = 320,
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

                // Verify current
                bool ok = AuthService.VerifyUserPassword(user, current);
                AuthService.LogAttempt(user, ok, ok ? "ChangePwdCurrentVerified" : "ChangePwdCurrentFailed");
                if (!ok)
                {
                    MessageBox.Show(dlg, "La contraseña actual es incorrecta.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Update password
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

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox txtUsuario = (TextBox)this.FindName("txtUsuario");
                PasswordBox pwdContraseña = (PasswordBox)this.FindName("pwdContraseña");

                string usuario = txtUsuario?.Text?.Trim() ?? string.Empty;
                string contraseña = pwdContraseña?.Password ?? string.Empty;

                if (string.IsNullOrWhiteSpace(usuario))
                {
                    MessageBox.Show("Por favor ingrese un usuario.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(contraseña))
                {
                    MessageBox.Show("Por favor ingrese una contraseña.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check lockout
                if (AuthService.IsLockedOut(usuario, out TimeSpan remaining))
                {
                    MessageBox.Show($"La cuenta está bloqueada. Intente nuevamente en {remaining.Minutes} minutos {remaining.Seconds} segundos.", "Bloqueado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    AuthService.LogAttempt(usuario, false, "LockedOut");
                    return;
                }

                bool ok = AuthService.VerifyUserPassword(usuario, contraseña);
                AuthService.LogAttempt(usuario, ok, ok ? "Success" : "InvalidCredentials");

                if (ok)
                {
                    AuthService.ResetFailedAttempts(usuario);

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();

                    this.Close();
                }
                else
                {
                    bool lockedNow = AuthService.IncreaseFailedAttempt(usuario);
                    if (lockedNow)
                    {
                        MessageBox.Show($"Demasiados intentos fallidos. La cuenta ha sido bloqueada por {15} minutos.", "Cuenta Bloqueada", MessageBoxButton.OK, MessageBoxImage.Error);
                        AuthService.LogAttempt(usuario, false, "LockedNow");
                    }
                    else
                    {
                        MessageBox.Show("Usuario o contraseña incorrectos.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    pwdContraseña.Clear();
                    pwdContraseña.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar sesión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
