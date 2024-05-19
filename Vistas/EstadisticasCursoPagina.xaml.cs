using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
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
using System.Xml.Linq;
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
        private HttpContent _contenidoReporte;
        private int _idCurso;
        
        public EstadisticasCurso(int idCurso)
        {
            InitializeComponent();
            _idCurso = idCurso;
            _ = RecuperarEstadisticasAsync(idCurso);
        }
        private async Task RecuperarEstadisticasAsync(int idCurso)
        {
            grdBackground.IsEnabled = false;
            string url = "cursos/estadisticas/" + idCurso;
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, url);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                Debug.WriteLine(codigoRespuesta);
                btnGenerarDocumento.IsEnabled = false;
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo recuperar las estadísticas, inténtelo más tarde");
                error.Show();
            }
            else
            {
                EstadisticaCursoDTO estadisticas = null;
                try
                {
                    var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                    estadisticas = JsonSerializer.Deserialize<EstadisticaCursoDTO>(jsonString);
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                

                if (estadisticas != null)
                {
                    _estadisticas = estadisticas;
                    MostrarDatos();
                }
                
            }
            grdBackground.IsEnabled = true;
        }

        private void MostrarDatos()
        {
            txtBlockNombreCurso.Text = _estadisticas.NombreCurso;
            if (_estadisticas.Calificacion != null)
            {
                txtBlockCalificacionTotal.Text = _estadisticas.Calificacion.ToString();
            }
            if (_estadisticas.PromedioComentarios != null)
            {
                txtBlockPromedioComentarios.Text = _estadisticas.PromedioComentarios.ToString();
            }
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

            if (_estadisticas.ClasesEstadistcas != null && _estadisticas.ClasesEstadistcas.Count > 0)
            {
                for(int i=0; i<_estadisticas.ClasesEstadistcas.Count; i++)
                {
                    _estadisticas.ClasesEstadistcas[i].NumeroClase = i + 1;
                }
                lstBoxClases.ItemsSource = _estadisticas.ClasesEstadistcas;
            }
            else
            {
                brdNoHayClases.Visibility = Visibility.Visible;
            }

        }

        private void ClicGenerarDocumento(object sender, RoutedEventArgs e)
        {
            _= ObtenerDocumentoAsync();
        }

        private async Task ObtenerDocumentoAsync()
        {
            grdBackground.IsEnabled = false;
            string url = "cursos/reporte/" + _idCurso;
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
                _contenidoReporte = respuestaHttp.Content;
                if (respuestaHttp.Content.Headers != null && respuestaHttp.Content.Headers.ContentDisposition != null)
                {
                    txtBlockNombreReporte.Text = respuestaHttp.Content.Headers.ContentDisposition.FileName;
                    brdDescargaDocumento.Visibility = Visibility.Visible;
                    btnGenerarDocumento.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ErrorMensaje error = new ErrorMensaje("Ocurrió un error al mostrar la descarga del reporte, inténtelo más tarde");
                    error.Show();
                }
            }
            grdBackground.IsEnabled = true;
        }
        private void ClicDescargar(object sender, RoutedEventArgs e)
        {
            _ = DescargarAsync(_contenidoReporte.Headers.ContentDisposition.FileName);
        }

        private async Task DescargarAsync(string nombreArchivo)
        {
            if (_contenidoReporte != null)
            {
                OpenFolderDialog dialog = new OpenFolderDialog();
                if (dialog.ShowDialog() == true)
                {
                    string ruta = dialog.FolderName + "/" + nombreArchivo;

                    try
                    {
                        using (Stream contentStream = await _contenidoReporte.ReadAsStreamAsync(),
                            fileStream = new FileStream(ruta, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            await contentStream.CopyToAsync(fileStream);
                        }

                        ExitoMensaje mensaje = new ExitoMensaje("Se ha descargado el documento exitosamente");
                        mensaje.Show();
                    }
                    catch (SystemException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        ErrorMensaje error = new ErrorMensaje("Ocurrió un error al guardar el reporte");
                        error.Show();
                    }
                }                
            }
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        
    }
}
