using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            if (codigoRespuesta >= 500)
            {
                ErrorMensaje error = new ErrorMensaje();
                error.Show();
            }
            else
            {
                ExitoMensaje exito = new ExitoMensaje();
                exito.Show();
            }
            //grpc
            try
            {
                using var channel = GrpcChannel.ForAddress("http://localhost:3001");
                VideoService.VideoServiceClient stub = new VideoService.VideoServiceClient(channel);
                EnviarVideoPeticion peticion = new EnviarVideoPeticion { Datos = "" };
                var respuesta = stub.enviarVideoDesdeCliente(peticion);
                Debug.WriteLine("");
                Debug.WriteLine(respuesta.Respuesta);
                Debug.WriteLine("");
                ExitoMensaje exito = new ExitoMensaje();
                exito.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("");

                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                Debug.WriteLine("");

            }
        }
    }
}
