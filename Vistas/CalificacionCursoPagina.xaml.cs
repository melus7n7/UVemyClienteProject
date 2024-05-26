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
    /// Lógica de interacción para CalificacionCursoPagina.xaml
    /// </summary>
    public partial class CalificacionCurso : Page
    {
        private CursoDTO _curso;
        public CalificacionCurso(CursoDTO curso)
        {
            InitializeComponent();
            _curso = curso;
           _ = ObtenerCalificacionPreviaUsuarioAsync();
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private async Task ObtenerCalificacionPreviaUsuarioAsync()
        {
            grdBackground.IsEnabled = false;

            string url = "cursos/calificacion/" + _curso.IdCurso;
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, url);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                grdPrincipal.IsEnabled = false;
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo recuperar la calificación, inténtelo más tarde");
                error.Show();
            }
            else
            {
                UsuarioCursoDTO usuarioCalificacion = null;
                try
                {
                    var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                    usuarioCalificacion = JsonSerializer.Deserialize<UsuarioCursoDTO>(jsonString);
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                if (usuarioCalificacion == null || usuarioCalificacion.Calificacion == null || usuarioCalificacion.Calificacion == 0)
                {
                    txtBlockCalificacion.Text = "10";
                    txtBlockCalificacionPrevia.Visibility = Visibility.Visible;
                }
                else
                {
                    BrushConverter brush = new BrushConverter();
                    txtBlockCalificacion.Text = usuarioCalificacion.Calificacion.ToString();
                    txtBlockCalificacion.Foreground = (SolidColorBrush)brush.ConvertFrom("#000000");
                }
            }

            grdBackground.IsEnabled = true;
        }

        private void ClicAumentar(object sender, RoutedEventArgs e)
        {
            txtBlockCalificacion.Foreground = (SolidColorBrush) new BrushConverter().ConvertFrom("#6d6d6d");

            int numero = Int32.Parse(txtBlockCalificacion.Text);
            numero = numero >= 10 ? numero : numero + 1;

            txtBlockCalificacion.Text = numero.ToString();
        }

        private void ClicDisminuir(object sender, RoutedEventArgs e)
        {
            txtBlockCalificacion.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#6d6d6d");

            int numero = Int32.Parse(txtBlockCalificacion.Text);
            numero = numero <= 1 ? numero : numero - 1;

            txtBlockCalificacion.Text = numero.ToString();
        }

        private void ClicGuardarCalificacion(object sender, RoutedEventArgs e)
        {
            _ = GuardarCalificacionAsync();
        }

        private async Task GuardarCalificacionAsync()
        {
            grdBackground.IsEnabled = false;

            int calificacionNueva = Int32.Parse(txtBlockCalificacion.Text);
            UsuarioCursoDTO usuario = new UsuarioCursoDTO { IdCurso = (int)_curso.IdCurso, IdUsuario = 3, Calificacion = calificacionNueva };
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string url = "cursos/calificacion/" + _curso.IdCurso;
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, url, content);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo asignar la calificación, inténtelo más tarde");
                error.Show();
            }
            else
            {
                txtBlockCalificacionPrevia.Visibility = Visibility.Collapsed;

                BrushConverter brush = new BrushConverter();
                txtBlockCalificacion.Text = calificacionNueva.ToString();
                txtBlockCalificacion.Foreground = (SolidColorBrush)brush.ConvertFrom("#000000");

                ExitoMensaje exito = new ExitoMensaje("Se asignó la calificación que le dio al curso");
                exito.Show();
            }

            grdBackground.IsEnabled = true;
        }
    }
}
