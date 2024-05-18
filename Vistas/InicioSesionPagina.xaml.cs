using System;
using System.Collections;
using System.Collections.Generic;
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
            UsuarioDTO credenciales = new()
            {
                CorreoElectronico = _correoElectronico,
                Contrasena = _contrasena
            };

            var json = JsonSerializer.Serialize(credenciales);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestSinAutenticacionAsync(HttpMethod.Post, "autenticacion", contenido);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta == (int) HttpStatusCode.OK)
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                if (JsonSerializer.Deserialize<UsuarioDTO>(jsonString) != null)
                {
                    SingletonUsuario.IdUsuario = (int)JsonSerializer.Deserialize<UsuarioDTO>(jsonString).Id;
                    SingletonUsuario.Nombres = JsonSerializer.Deserialize<UsuarioDTO>(jsonString).Nombres;
                    SingletonUsuario.Apellidos = JsonSerializer.Deserialize<UsuarioDTO>(jsonString).Apellidos;
                    SingletonUsuario.CorreoElectronico = JsonSerializer.Deserialize<UsuarioDTO>(jsonString).CorreoElectronico;
                    SingletonUsuario.JWT = JsonSerializer.Deserialize<UsuarioDTO>(jsonString).Token;

                    ExitoMensaje exitoMensaje = new();
                    exitoMensaje.Show();
                }
            }
            else
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                UsuarioDTO? errorJson = JsonSerializer.Deserialize<UsuarioDTO>(jsonString);

                string[] detalles = errorJson?.Detalles.ToArray() ?? ["Error desconocido"];
                string detallesConcatenados = string.Join(", ", detalles);
                txtBlockMensajeError.Text = detallesConcatenados;
            }
        }

        private void ClicRegistrate(object sender, RoutedEventArgs e)
        {
            FormularioUsuarioPagina formularioUsuarioPagina = new();
            NavigationService.Navigate(formularioUsuarioPagina);
        }

        private void CargarPagina(object sender, RoutedEventArgs e)
        {
            txtBoxCorreoElectronico.Text = string.Empty;
            pwdBoxContrasena.Password = string.Empty;
        }
    }
}
