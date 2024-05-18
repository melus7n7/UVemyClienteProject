using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
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
    /// Lógica de interacción para EstadisticasCursoPagina.xaml
    /// </summary>
    public partial class EstadisticasCurso : Page
    {
        private EstadisticaCursoDTO _estadisticas;
        public EstadisticasCurso()
        {
            InitializeComponent();
            
            _ = RecuperarEstadisticasAsync(1);
        }
        private async Task RecuperarEstadisticasAsync(int idCurso)
        {
            string url = "cursos/estadisticas/" + idCurso;
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, url);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo recuperar las estadísticas, inténtelo más tarde");
                error.Show();
            }
            else
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                EstadisticaCursoDTO? estadisticas = JsonSerializer.Deserialize<EstadisticaCursoDTO>(jsonString);

                if (estadisticas != null)
                {
                    _estadisticas = estadisticas;
                    MostrarDatos();
                }
                
            }

        }

        private void MostrarDatos()
        {
            txtBlockNombreCurso.Text = _estadisticas.NombreCurso;
            txtBlockCalificacionTotal.Text = _estadisticas.Calificacion.ToString();
            txtBlockPromedioComentarios.Text = _estadisticas.PromedioComentarios.ToString();
            txtBlockEstudiantesTotales.Text = _estadisticas.EstudiantesInscritos.ToString();

            if (_estadisticas.EtiquetasCoinciden != null)
            {
                string etiquetas = "";
                for (int i = 0; i < _estadisticas.EtiquetasCoinciden.Count; i++)
                {
                    etiquetas = (i == 0) ? _estadisticas.EtiquetasCoinciden[i] : etiquetas + ", " + _estadisticas.EtiquetasCoinciden[i];
                }
                txtBlockEtiquetas.Text = etiquetas;
            }

            if (_estadisticas.EstudiantesCurso != null)
            {
                lstBoxEstudiantes.ItemsSource = _estadisticas.EstudiantesCurso;
            }

            if (_estadisticas.ClasesEstadistcas != null)
            {
                for(int i=0; i<_estadisticas.ClasesEstadistcas.Count; i++)
                {
                    _estadisticas.ClasesEstadistcas[i].NumeroClase = i + 1;
                }
                lstBoxClases.ItemsSource = _estadisticas.ClasesEstadistcas;
            }

        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
