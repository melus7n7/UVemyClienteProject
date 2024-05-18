using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using UVemyCliente.Conexion;
using UVemyCliente.DTO;
using UVemyCliente.Utilidades;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Text;

namespace UVemyCliente.Vistas
{
    public partial class SeleccionEtiquetasPagina : Page
    {
        private ObservableCollection<EtiquetaDTO> _etiquetas = [];
        private UsuarioDTO _usuario;

        public SeleccionEtiquetasPagina(UsuarioDTO usuario)
        {
            InitializeComponent();
            _usuario = usuario;
        }

        private void CargarPagina(object sender, RoutedEventArgs e)
        {
            _ = CargarEtiquetasAsync();
        }

        private async Task CargarEtiquetasAsync()
        {
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestSinAutenticacionAsync(HttpMethod.Get, "etiquetas");
            if (respuestaHttp.IsSuccessStatusCode)
            {
                var json = await respuestaHttp.Content.ReadAsStringAsync();
                _etiquetas = JsonSerializer.Deserialize<ObservableCollection<EtiquetaDTO>>(json) ?? [];
                itmControlEtiquetas.ItemsSource = _etiquetas;
            }
            else
            {
                ErrorMensaje errorMensaje = new("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();

                //TODO: Regresar a menú principal o a FormularioUsuarioPagina
            }
        }

        private void SeleccionarEtiqueta(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && toggleButton.DataContext is EtiquetaDTO etiqueta)
            {
                _usuario.IdsEtiqueta ??= [];
                _usuario.IdsEtiqueta.Add(etiqueta.IdEtiqueta);
            }
        }

        private void DeseleccionarEtiqueta(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && toggleButton.DataContext is EtiquetaDTO etiqueta)
            {
                _usuario.IdsEtiqueta?.Remove(etiqueta.IdEtiqueta);
            }
        }

        private void ClicConfirmar(object sender, RoutedEventArgs e)
        {
            if (_usuario.IdsEtiqueta?.Count > 0)
            {
                _ = SolicitarCodigoVerificacion();
            }
            else
            {
                ErrorMensaje errorMensaje = new("Debes seleccionar al menos una etiqueta");
                errorMensaje.Show();
            }
        }

        private async Task SolicitarCodigoVerificacion()
        {
            var json = JsonSerializer.Serialize(_usuario);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestSinAutenticacionAsync(HttpMethod.Post, "autenticacion/verificacion", contenido);
            if (respuestaHttp.IsSuccessStatusCode)
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                if (JsonSerializer.Deserialize<UsuarioDTO>(jsonString) != null)
                {
                    SingletonUsuario.JWT = JsonSerializer.Deserialize<UsuarioDTO>(jsonString).Token;
                    CodigoVerificacionPagina codigoVerificacionPagina = new(_usuario);
                    this.NavigationService.Navigate(codigoVerificacionPagina);
                }
            }
            else
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                UsuarioDTO? errorJson = JsonSerializer.Deserialize<UsuarioDTO>(jsonString);

                string[] detalles = errorJson?.Detalles.ToArray() ?? ["Error desconocido"];
                string detallesConcatenados = string.Join(", ", detalles);
                ErrorMensaje errorMensaje = new(detallesConcatenados + ". Verifique la información e intentélo de nuevo más tarde");
                errorMensaje.Show();
            }
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}
