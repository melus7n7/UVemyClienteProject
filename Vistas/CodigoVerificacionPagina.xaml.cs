using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Lógica de interacción para CodigoVerificacionPagina.xaml
    /// </summary>
    public partial class CodigoVerificacionPagina : Page
    {
        private UsuarioDTO _usuario;

        public CodigoVerificacionPagina(UsuarioDTO usuario)
        {
            InitializeComponent();
            _usuario = usuario;
        }

        private void ClicConfirmar(object sender, RoutedEventArgs e)
        {
            if (ValidarCampo())
            {
                _usuario.CodigoVerificacion = txtCodigoVerificacion.Text;
                _ = VerificarCodigoAsync();
            }
        }

        private bool ValidarCampo()
        {
            txtBlockMensajeError.Text = string.Empty;
            bool esValido = true;

            if (string.IsNullOrEmpty(txtCodigoVerificacion.Text) || txtCodigoVerificacion.Text.Length < 4)
            {
                txtBlockMensajeError.Text = "El código de verificación debe tener 4 dígitos.";
                esValido = false;
            }
            
            return esValido;
        }

        private async Task VerificarCodigoAsync()
        {
            var json = JsonSerializer.Serialize(_usuario);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, "autenticacion/registro", contenido);
            if (respuestaHttp.IsSuccessStatusCode)
            {
                ExitoMensaje exitoMensaje = new();
                exitoMensaje.Show();

                InicioSesionPagina inicioSesionPagina = new();
                this.NavigationService?.Navigate(inicioSesionPagina);
                this.NavigationService?.RemoveBackEntry();
                SingletonUsuario.JWT = string.Empty;
            }
            else
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                UsuarioDTO? errorJson = JsonSerializer.Deserialize<UsuarioDTO>(jsonString);

                string[] detalles = errorJson?.Detalles.ToArray() ?? ["Error desconocido"];
                string detallesConcatenados = string.Join(", ", detalles);
                txtBlockMensajeError.Text = detallesConcatenados + ". Verifique la información e intentélo de nuevo más tarde";
            }
        }

        private void VerificarEntradaNumérica(object sender, TextCompositionEventArgs e)
        {
            if (!EsNumerico(e.Text) || e.Text.Contains(" "))
            {
                e.Handled = true;
            }
        }

        private static bool EsNumerico(string texto)
        {
            return int.TryParse(texto, out _);
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            InicioSesionPagina inicioSesionPagina = new();
            this.NavigationService?.Navigate(inicioSesionPagina);
        }
    }
}
