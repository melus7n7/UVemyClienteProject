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
    /// Lógica de interacción para DetallesCursoPagina.xaml
    /// </summary>
    public partial class DetallesCurso : Page
    {
        int _idCurso = 4;
        public DetallesCurso()
        {
            InitializeComponent();
        }

        private void ClicAgregarClase(object sender, RoutedEventArgs e)
        {
            FormularioClase formulario = new FormularioClase();
            NavigationService.Navigate(formulario);
        }

        private void ClicVerClase(object sender, RoutedEventArgs e)
        {
            DetallesClase detalles = new DetallesClase();
            NavigationService.Navigate(detalles);
        }

        private void ClicVerEstadisticasCurso(object sender, RoutedEventArgs e)
        {
            EstadisticasCurso estadisticas = new EstadisticasCurso(_idCurso);
            NavigationService.Navigate(estadisticas);
        }

        private void ClicInscribirseAlCurso(object sender, RoutedEventArgs e)
        {
            string message = "¿Desea inscribirse al curso?";
            string title = "Inscripción al curso";
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            MessageBoxResult result = MessageBox.Show(message, title, buttons);
            if (result == MessageBoxResult.Yes)
            {
                _ = InscribirseAlCursoAsync();
            }
        }

        private async Task InscribirseAlCursoAsync()
        {
            btnRegresar.IsEnabled = false;
            grdBackground.IsEnabled = false;

            UsuarioCursoDTO usuario = new UsuarioCursoDTO { IdCurso = _idCurso, IdUsuario = 4 };
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string url = "cursos/inscripcion/" + _idCurso;
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, url, content);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo inscribir, inténtelo más tarde");
                error.Show();
            }
            else
            {
                ExitoMensaje mensaje = new ExitoMensaje("Se ha inscribido al curso exitosamente");
                mensaje.Show();
            }

            btnRegresar.IsEnabled = true;
            grdBackground.IsEnabled = true;
        }

        private void ClicCalificarCurso(object sender, RoutedEventArgs e)
        {
            CalificacionCurso pagina = new CalificacionCurso(new CursoDTO { IdCurso = 1, Titulo = "si"});
            NavigationService.Navigate(pagina);
        }
    }
}
