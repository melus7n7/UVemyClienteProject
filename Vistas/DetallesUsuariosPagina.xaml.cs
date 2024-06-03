using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    /// Interaction logic for DetallesUsuariosPagina.xaml
    /// </summary>
    public partial class DetallesUsuariosPagina : Page
    {
        private ObservableCollection<UsuarioDetalles> _usuarios = new ObservableCollection<UsuarioDetalles>();
        private int _paginaActual = 1;

        public DetallesUsuariosPagina()
        {
            InitializeComponent();
            txtBlockTitulo.Text = "Usuarios registrados dentro del sistema UVemy";
            _ = CargarUsuariosAsync(_paginaActual);
        }

        private async Task CargarUsuariosAsync(int pagina)
        {
            DeshabilitarBotones();

            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, $"usuarios/{pagina}");

            if (respuestaHttp.IsSuccessStatusCode)
            {
                var json = await respuestaHttp.Content.ReadAsStringAsync();
                var usuarios = JsonConvert.DeserializeObject<List<UsuarioDetalles>>(json);

                if (usuarios != null)
                {
                    _usuarios.Clear();
                    foreach (var usuario in usuarios.Where(u => u.EsAdministrador == 0))
                    {                        
                        if (usuario.Imagen != null)
                        {
                            byte[] imageData = usuario.Imagen.Data;
                            BitmapImage bitmap = new BitmapImage();
                            using (MemoryStream stream = new MemoryStream(imageData))
                            {
                                bitmap.BeginInit();
                                bitmap.StreamSource = stream;
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                            }
                            usuario.ImagenUsuario = bitmap;
                        }
                        
                        _usuarios.Add(usuario);
                    }
                }
                lstBoxUsuarios.ItemsSource = _usuarios;
                BtnPrevia.IsEnabled = _paginaActual > 1; 
                BtnSiguiente.IsEnabled = usuarios.Count == 6;
                HabilitarBotones();
            }
            else
            {
                ErrorMensaje errorMensaje = new ErrorMensaje("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();
                NavigationService.GoBack();
            }
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        public class UsuarioDetalles
        {
            [JsonPropertyName("idUsuario")]
            public int? Id { get; set; }
            [JsonPropertyName("nombres")]
            public string? Nombres { get; set; }
            [JsonPropertyName("apellidos")]
            public string? Apellidos { get; set; }
            [JsonPropertyName("imagen")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public ImagenUsuario Imagen { get; set; }
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public BitmapImage ImagenUsuario { get; set; }
            [JsonPropertyName("correoElectronico")]
            public string? CorreoElectronico { get; set; }
            [JsonPropertyName("esAdministrador")]
            public int? EsAdministrador { get; set; }
        }

        public class ImagenUsuario
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("data")]
            public byte[] Data { get; set; }
        }

        private async void ClicPrevia(object sender, RoutedEventArgs e)
        {
            if (_paginaActual > 1)
            {
                _paginaActual--;
                string textoBusqueda = txtBlockNombres.Text.ToLower();
                if (string.IsNullOrEmpty(textoBusqueda))
                {
                    await CargarUsuariosAsync(_paginaActual);
                }
                else
                {
                    await CargarUsuariosBusquedaAsync(_paginaActual);
                }
            }
        }

        private async void ClicSiguiente(object sender, RoutedEventArgs e)
        {
            _paginaActual++;
            string textoBusqueda = txtBlockNombres.Text.ToLower();
            if (string.IsNullOrEmpty(textoBusqueda))
            {
                await CargarUsuariosAsync(_paginaActual);
            }
            else
            {
                await CargarUsuariosBusquedaAsync(_paginaActual);
            }
        }

        private async void ClicBuscar(object sender, RoutedEventArgs e)
        {
            string textoBusqueda = txtBlockNombres.Text.ToLower();
            if (string.IsNullOrEmpty(textoBusqueda))
            {
                await CargarUsuariosAsync(_paginaActual);
            }
            else
            {
                await CargarUsuariosBusquedaAsync(_paginaActual);
            }
        }

        private async Task CargarUsuariosBusquedaAsync(int pagina)
        {
            DeshabilitarBotones();

            string textoBusqueda = txtBlockNombres.Text.ToLower();
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, $"usuarios/buscar/{pagina}?busqueda={textoBusqueda}");

            if (respuestaHttp.IsSuccessStatusCode)
            {
                var json = await respuestaHttp.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<ResponseData>(json);

                if (responseData != null)
                {
                    _usuarios.Clear();

                    foreach (var usuario in responseData.Data.Where(u => u.EsAdministrador == 0))
                    {
                        if (usuario.Imagen != null)
                        {
                            byte[] imageData = usuario.Imagen.Data;
                            BitmapImage bitmap = new BitmapImage();
                            using (MemoryStream stream = new MemoryStream(imageData))
                            {
                                bitmap.BeginInit();
                                bitmap.StreamSource = stream;
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                            }
                            usuario.ImagenUsuario = bitmap;
                        }

                        _usuarios.Add(usuario);
                    }
                }
                lstBoxUsuarios.ItemsSource = _usuarios;
                BtnPrevia.IsEnabled = responseData.CurrentPage > 1;
                BtnSiguiente.IsEnabled = responseData.CurrentPage < responseData.TotalPages;
                HabilitarBotones();
            }
            else
            {
                var errorContent = await respuestaHttp.Content.ReadAsStringAsync();
                ErrorMensaje errorMensaje = new ErrorMensaje($"Error. No se pudo conectar con el servidor. Detalles: {errorContent}");
                errorMensaje.Show();
                NavigationService.GoBack();
            }
        }

        public class ResponseData
        {
            [JsonProperty("data")]
            public List<UsuarioDetalles> Data { get; set; }

            [JsonProperty("total")]
            public int Total { get; set; }

            [JsonProperty("totalPages")]
            public int TotalPages { get; set; }

            [JsonProperty("currentPage")]
            public int CurrentPage { get; set; }
        }

        private void DeshabilitarBotones()
        {
            BtnPrevia.IsEnabled = false;
            BtnSiguiente.IsEnabled = false;
            BtnRegresar.IsEnabled = false;
            BtnBuscar.IsEnabled = false;
        }

        private void HabilitarBotones()
        {
            BtnPrevia.IsEnabled = true;
            BtnSiguiente.IsEnabled = true;
            BtnRegresar.IsEnabled = true;
            BtnBuscar.IsEnabled = true;
        }

    }
}
