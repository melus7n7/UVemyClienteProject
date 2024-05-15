using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using UVemyCliente.Servicios;
using UVemyCliente.Utilidades;

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Lógica de interacción para FormularioClasePagina.xaml
    /// </summary>
    public partial class FormularioClasePagina : Page
    {
        public FormularioClasePagina()
        {
            InitializeComponent();
        }
        private void ClicGuardarClase(object sender, RoutedEventArgs e)
        {
            bool esValido = ValidarInformacion();

            if (!esValido)
            {
                MostrarCamposNoValidos();
            }
            else
            {
                _ = GuardarClaseAsync();
            }
        }

        private bool ValidarInformacion()
        {
            return true;
        }

        private void MostrarCamposNoValidos()
        {

        }

        private async Task GuardarClaseAsync()
        {
            ClaseDTO clase = new ClaseDTO
            {
                Nombre = "Clase Prueba",
                Descripcion = "Descripcion",
                IdCurso = 1
            };

            var json = JsonSerializer.Serialize(clase);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, "clases", content);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;
            

            if (codigoRespuesta >= 400)
            {
                ErrorMensaje error = new ErrorMensaje();
                error.Show();
            }
            else
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                ClaseDTO? claseNueva = JsonSerializer.Deserialize<ClaseDTO>(jsonString);
                if (claseNueva != null)
                {
                    await GuardarVideoAsync(claseNueva.Id);
                }
            }            
        }

        private async Task GuardarVideoAsync(int idClase)
        {
            byte[] video =File.ReadAllBytes(@"C:\Users\sulem\Downloads\VideoIntroKirbyDreamLand3.mp4");
            DocumentoDTO videoNuevo = new DocumentoDTO
            {
                Archivo = video,
                Nombre = "nombreNuevo.mp4",
                IdClase = idClase
            };
            int respuesta = await VideoGrpc.EnviarVideoDeClaseAsync(videoNuevo);

            if (respuesta >= 400)
            {
                ErrorMensaje error = new ErrorMensaje();
                error.Show();
            }
            else
            {
                ExitoMensaje exito = new ExitoMensaje();
                exito.Show();
            }
            
        }
    }
}
