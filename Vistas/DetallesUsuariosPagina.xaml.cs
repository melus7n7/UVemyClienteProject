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

        public DetallesUsuariosPagina()
        {
            InitializeComponent();
            _ = CargarUsuariosAsync();
        }

        private async Task CargarUsuariosAsync()
        {
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, "usuarios/1");

            if (respuestaHttp.IsSuccessStatusCode)
            {
                var json = await respuestaHttp.Content.ReadAsStringAsync();
                var usuarios = JsonConvert.DeserializeObject<List<UsuarioDetalles>>(json);

                if (usuarios != null)
                {
                    foreach (var usuario in usuarios)
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
            }
            else
            {
                ErrorMensaje errorMensaje = new ErrorMensaje("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();
            }
        }

        private BitmapImage ConvertirImagen(byte[] imagenBytes)
        {
            using (var stream = new MemoryStream(imagenBytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ClicGenerarDocumento(object sender, RoutedEventArgs e)
        {

        }

        private void ClicDescargar(object sender, RoutedEventArgs e)
        {

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
        }

        public class ImagenUsuario
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("data")]
            public byte[] Data { get; set; }
        }
    }
}
