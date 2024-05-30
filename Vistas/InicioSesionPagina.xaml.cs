using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UVemyCliente.Conexion;
using UVemyCliente.DTO;
using UVemyCliente.Utilidades;

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Lógica de interacción para InicioSesionPagina.xaml
    /// </summary>
    public partial class InicioSesionPagina : Page
    {
        private string _correoElectronico = string.Empty;
        private string _contrasena = string.Empty;

        public InicioSesionPagina()
        {
            InitializeComponent();
        }

        private void ClicIniciarSesion(object sender, RoutedEventArgs e)
        {
            _correoElectronico = txtBoxCorreoElectronico.Text.Trim();
            _contrasena = pwdBoxContrasena.Password.Trim();

            if (ValidarCampos())
            {
                _ = IniciarSesionAsync();
            }
        }

        private bool ValidarCampos()
        {
            bool sonValidos = true;
            txtBlockMensajeError.Text = string.Empty;

            if (string.IsNullOrEmpty(_correoElectronico))
            {
                txtBlockMensajeError.Text = "Correo electrónico requerido";
                sonValidos = false;
            }
            else if (!CredencialesValidador.EsCorreoValido(_correoElectronico))
            {
                txtBlockMensajeError.Text = "Correo electrónico inválido";
                sonValidos = false;
            }

            if (string.IsNullOrEmpty(_contrasena))
            {
                txtBlockMensajeError.Text += string.IsNullOrEmpty(txtBlockMensajeError.Text) ? "Contraseña requerida" : "\nContraseña requerida";
                sonValidos = false;
            }

            return sonValidos;
        }


        private async Task IniciarSesionAsync()
        {
            var credenciales = new
            {
                correoElectronico = _correoElectronico,
                contrasena = _contrasena
            };

            var json = JsonSerializer.Serialize(credenciales);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestSinAutenticacionAsync(HttpMethod.Post, "autenticacion", contenido);
            Debug.Write(respuestaHttp);
            Debug.Write(respuestaHttp.IsSuccessStatusCode);
            if (respuestaHttp.IsSuccessStatusCode)
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();

                using JsonDocument document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;

                UsuarioDTO usuario = new()
                {
                    Id = root.GetProperty("idUsuario").GetInt32(),
                    Nombres = root.GetProperty("nombres").GetString(),
                    Apellidos = root.GetProperty("apellidos").GetString(),
                    CorreoElectronico = root.GetProperty("correoElectronico").GetString(),
                    Token = root.GetProperty("jwt").GetString()
                };

                SingletonUsuario.IdUsuario = usuario.Id ?? 0;
                SingletonUsuario.Nombres = usuario.Nombres;
                SingletonUsuario.Apellidos = usuario.Apellidos;
                SingletonUsuario.CorreoElectronico = usuario.CorreoElectronico;
                SingletonUsuario.JWT = usuario.Token;

                var idsEtiquetaJson = root.GetProperty("idsEtiqueta");
                List<int> idsEtiqueta = [];
                foreach (var idEtiquetaJson in idsEtiquetaJson.EnumerateArray())
                {
                    idsEtiqueta.Add(idEtiquetaJson.GetInt32());
                }
                SingletonUsuario.IdsEtiqueta = [.. idsEtiqueta];

                ExitoMensaje exitoMensaje = new("Bienvenido " + SingletonUsuario.Nombres + " " + SingletonUsuario.Apellidos);
                exitoMensaje.Show();

                MenuPrincipalPagina menuPrincipalPagina = new();
                this.NavigationService.Navigate(menuPrincipalPagina);
            }
            else if (respuestaHttp.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                ErrorMensaje error = new ErrorMensaje("Error al conectar al servidor");
                error.Show();
                
            }
            else
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                var errorJson = JsonSerializer.Deserialize<UsuarioDTO>(jsonString);

                string[] detalles = errorJson?.Detalles?.ToArray() ?? ["Error desconocido"];
                string detallesConcatenados = string.Join(", ", detalles);
                txtBlockMensajeError.Text = detallesConcatenados;
            }
        }

        private void ClicRegistrate(object sender, RoutedEventArgs e)
        {
            FormularioUsuarioPagina formularioUsuarioPagina = new();
            this.NavigationService.Navigate(formularioUsuarioPagina);
        }

        private void CargarPagina(object sender, RoutedEventArgs e)
        {
            txtBoxCorreoElectronico.Text = string.Empty;
            pwdBoxContrasena.Password = string.Empty;
        }
    }
}
