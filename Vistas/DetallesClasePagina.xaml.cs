using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
    /// Lógica de interacción para DetallesClasePagina.xaml
    /// </summary>
    public partial class DetallesClase : Page
    {
        private ClaseDTO _clase;
        private DetallesCurso _paginaDetallesCurso;

        public DetallesClase(int idClase)
        {
            InitializeComponent();
            _ = RecuperarDatosClaseAsync(idClase);
            _paginaDetallesCurso = new DetallesCurso();
        }

        public DetallesClase(int idClase, DetallesCurso paginaDetallesCurso)
        {
            InitializeComponent();
            _ = RecuperarDatosClaseAsync(idClase);
            _paginaDetallesCurso = paginaDetallesCurso;
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_paginaDetallesCurso);
        }

        private async Task RecuperarDatosClaseAsync(int idClase)
        {
            grdBackground.IsEnabled = false;
            string url = "clases/" + idClase;
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, url);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                grdPrincipal.IsEnabled = false;
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo recuperar la clase, inténtelo más tarde");
                error.Show();
            }
            else
            {
                ClaseDTO claseRecuperada = null;
                try
                {
                    var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                    claseRecuperada = JsonSerializer.Deserialize<ClaseDTO>(jsonString);
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                if (claseRecuperada != null)
                {
                    _clase = claseRecuperada;
                    txtBlockNombreClase.Text = _clase.Nombre;
                    txtBlockDescripcionClase.Text = _clase.Descripcion;
                    await RecuperarDocumentosAsync();
                    
                    //Recuperar video, asignar como DocumentoDTO a _clase.Video para formulario, comentarios
                }
            }
            grdBackground.IsEnabled = true;
        }

        private async Task RecuperarDocumentosAsync()
        {
            List<int> listaId = _clase.DocumentosId;
            List<DocumentoDTO> documentosRecuperados = new List<DocumentoDTO>();

            if (listaId == null || listaId.Count == 0)
            {
                ErrorMensaje error = new ErrorMensaje("La clase no tiene documentos asociados");
                error.Show();
                return;
            }

            for (int i = 0; i < listaId.Count; i++)
            {
                string url = "documentos/clase/" + listaId[i];
                HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, url);
                int codigoRespuesta = (int)respuestaHttp.StatusCode;

                if (codigoRespuesta >= 400)
                {
                    grdPrincipal.IsEnabled = false;
                    Debug.WriteLine(codigoRespuesta);
                    ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudieron recuperar los documentos, inténtelo más tarde");
                    error.Show();
                    break;
                }
                else
                {
                    DocumentoDTO documento = new DocumentoDTO();
                    byte[] archivo = null;
                    try
                    {
                        archivo = await respuestaHttp.Content.ReadAsByteArrayAsync();
                    }
                    catch (SystemException ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    if (archivo != null)
                    {
                        documento.IdDocumento = listaId[i];
                        documento.Archivo = archivo;
                        documento.Nombre = ObtenerNombreDesdeHeader(respuestaHttp.Content.Headers);
                        documentosRecuperados.Add(documento);
                    }
                }

            }

            _clase.Documentos = documentosRecuperados;
            lstBoxDocumentos.ItemsSource = documentosRecuperados;
            
        }

        private string ObtenerNombreDesdeHeader(HttpContentHeaders headersHttp)
        {
            if (headersHttp.ContentDisposition != null && headersHttp.ContentDisposition.FileName != null)
            {
                return System.IO.Path.GetFileNameWithoutExtension(headersHttp.ContentDisposition.FileName);
            }
            string header = headersHttp.ToString();

            string nombre = "Documento";
            if (header != null && header.Contains("filename="))
            {
                int posicionNombre = header.IndexOf("=");
                nombre = header.Substring(posicionNombre + 1);
                int fin = nombre.IndexOf("\n");

                if (fin < nombre.Length)
                {
                    nombre = nombre.Substring(0, fin);
                    nombre = System.IO.Path.GetFileNameWithoutExtension(nombre);
                }
            }
            return nombre;
        }

        private void ClicModificarClase(object sender, RoutedEventArgs e)
        {
            FormularioClase formulario = new FormularioClase(_clase);
            NavigationService.Navigate(formulario);
        }

        private void ClicDescargarDocumento(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            DocumentoDTO documento = btn.DataContext as DocumentoDTO;
            if (documento != null && documento.Archivo != null)
            {
                _= DescargarAsync(documento);
            }
        }

        private async Task DescargarAsync(DocumentoDTO documento)
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                string ruta = dialog.FolderName + "/" + documento.Nombre + ".pdf";

                try
                {
                    File.WriteAllBytes(ruta, documento.Archivo);

                    ExitoMensaje mensaje = new ExitoMensaje("Se ha descargado el documento exitosamente");
                    mensaje.Show();
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    ErrorMensaje error = new ErrorMensaje("Ocurrió un error al guardar el documento");
                    error.Show();
                }
            }
        }
    }
}
