using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Lógica de interacción para ComentarioPrincipalUserControl.xaml
    /// </summary>
    public partial class ComentarioPrincipalUserControl : UserControl
    {
        private int _idClase;
        private ComentarioDTO _comentarioPrincipal;

        public ComentarioPrincipalUserControl(ComentarioDTO comentario, int idClase)
        {
            InitializeComponent();

            _idClase = idClase;
            _comentarioPrincipal = comentario;

            txtBlockNombreUsuarioPrincipal.Text = comentario.NombreUsuario;
            txtBlockComentarioPrincipal.Text = comentario.Descripcion;

            if(comentario.Respuestas.Count > 0 )
            {
                lstBoxRespuestas.ItemsSource = comentario.Respuestas;
            }
            else
            {
                lstBoxRespuestas.Visibility = Visibility.Collapsed;
                txtBlockRespuestasTitulo.Visibility = Visibility.Collapsed;
            }
        }

        private void ClicMostrarComentario(object sender, RoutedEventArgs e)
        {
            brdComentarioNuevo.Visibility = Visibility.Visible;
            btnResponder.Visibility = Visibility.Collapsed;
        }

        private void ClicEnviarComentario(object sender, RoutedEventArgs e)
        {
            string descripcionComentario = txtBoxComentarioRespuesta.Text;

            if (string.IsNullOrEmpty(descripcionComentario))
            {

               ErrorMensaje errorMensaje = new("Escriba el contenido de la respuesta para poder enviarla");
               errorMensaje.Show();
            }
            else
            {
                _ = EnviarComentarioAsync(descripcionComentario);
            }
        }

        private async Task EnviarComentarioAsync(string descripcionComentario)
        {
            var comentarioNuevo = new
            {
                idClase = _idClase,
                idUsuario = SingletonUsuario.IdUsuario,
                descripcion = descripcionComentario
            };

            string json = JsonSerializer.Serialize(comentarioNuevo);
            HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
            string url = "comentarios";
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, url, contenido);

            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje errorMensaje = new("Ocurrió un error y no se pudo enviar el comentario, inténtelo más tarde");
                errorMensaje.Show();
            }
            else if (respuestaHttp.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                ErrorMensaje error = new("Error al conectar al servidor");
                error.Show();
            }
            else
            {
                txtBoxComentarioRespuesta.Text = "";

                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                var respuesta = JsonSerializer.Deserialize<ComentarioDTO>(jsonString);
                int idComentario = respuesta.IdComentario;

                ComentarioDTO comentarioNuevoDTO = new()
                {
                    IdComentario = idComentario,
                    IdClase = _idClase,
                    NombreUsuario = SingletonUsuario.Nombres + " " + SingletonUsuario.Apellidos,
                    Descripcion = descripcionComentario,
                    Respuestas = []
                };

                _comentarioPrincipal.Respuestas.Add(comentarioNuevoDTO);
                lstBoxRespuestas.ItemsSource = _comentarioPrincipal.Respuestas;

                lstBoxRespuestas.Visibility = Visibility.Visible;
                txtBlockRespuestasTitulo.Visibility = Visibility.Visible;
                brdComentarioNuevo.Visibility = Visibility.Collapsed;
                btnResponder.Visibility = Visibility.Visible;
            }
        }
    }
}
