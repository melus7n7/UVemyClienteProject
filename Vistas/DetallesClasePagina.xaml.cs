using Grpc.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
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
    /// Lógica de interacción para DetallesClasePagina.xaml
    /// </summary>
    public partial class DetallesClase : Page
    {
        private ClaseDTO _clase;
        private int _idClase;
        private bool _esProfesor;
        private string _tempArchivoPath = "";
        private CursoDTO _curso = new CursoDTO() { };

        public DetallesClase(CursoDTO curso, int idClase, bool esProfesor = true)
        {
            InitializeComponent();
            _idClase = idClase;
            _curso = curso;
            _esProfesor = esProfesor;
            _ = RecuperarDatosClaseAsync(idClase);
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DetallesCurso(_curso));
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
                    if (_esProfesor)
                    {
                        btnModificarClase.Visibility = Visibility.Visible;
                    }

                    _clase = claseRecuperada;
                    txtBlockNombreClase.Text = _clase.Nombre;
                    txtBlockDescripcionClase.Text = _clase.Descripcion;
                    await RecuperarDocumentosAsync();
                    await RecuperarComentariosAsync();

                    await RecuperarVideoClaseAsync();

                    
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

        private async Task RecuperarComentariosAsync()
        {
            string url = $"comentarios/{_idClase}";
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, url);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                grdPrincipal.IsEnabled = false;
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudieron recuperar los comentarios, inténtelo más tarde");
                error.Show();
            }
            else if (respuestaHttp.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                ErrorMensaje error = new("Error al conectar al servidor");
                error.Show();
            }
            else
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                List<ComentarioDTO>? comentariosRecuperados = JsonSerializer.Deserialize<List<ComentarioDTO>>(jsonString);

                if(comentariosRecuperados != null)
                {
                    foreach(var comentario in comentariosRecuperados)
                    {
                        ComentarioPrincipalUserControl comentarioPrincipal = new(comentario, _idClase);

                        lstViewComentarios.Items.Add(comentarioPrincipal);
                    }
                }
            }
        }

        private async Task RecuperarVideoClaseAsync()
        {
            if (_clase.VideoId == null)
            {
                ErrorMensaje error = new ErrorMensaje("La clase no tiene un video, agregue una para que se muestre a otros la clase");
                error.Show();
                return;
            }

            _tempArchivoPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp_video_file.mp4");

            try
            {
                MemoryStream streamVideo = await VideoGrpc.DescargarVideoStreamAsync((int)_clase.VideoId);
                using (FileStream fileStream = new(_tempArchivoPath, FileMode.Create, FileAccess.Write))
                {
                    streamVideo.Position = 0;
                    await streamVideo.CopyToAsync(fileStream);
                }

                //Para poder modificar en el formulario
                _clase.Video = new DocumentoDTO 
                { 
                    IdDocumento = (int)_clase.VideoId, Archivo = streamVideo.ToArray(), Nombre = "TempUvemy"
                };
            }
            catch (RpcException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                grdPrincipal.IsEnabled = false;
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo recuperar el video de la clase, inténtelo más tarde");
                error.Show();
            }

            mdElementVideo.Source = new Uri(_tempArchivoPath);
            mdElementVideo.LoadedBehavior = MediaState.Manual;
            mdElementVideo.UnloadedBehavior = MediaState.Manual;
            mdElementVideo.Stop();

            btnReproducir.Visibility = Visibility.Visible;
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

        private void ClicReproducir(object sender, RoutedEventArgs e)
        {
            mdElementVideo.Play();
            btnReproducir.Visibility = Visibility.Collapsed;
            btnPausar.Visibility = Visibility.Visible;
        }

        private void ClicPausar(object sender, RoutedEventArgs e)
        {
            mdElementVideo.Stop();
            btnReproducir.Visibility = Visibility.Visible;
            btnPausar.Visibility = Visibility.Collapsed;
        }

        private async void ClicEnviarComentario(object sender, RoutedEventArgs e)
        {
            string descripcionComentario = txtBoxComentario.Text;

            if (string.IsNullOrEmpty(descripcionComentario))
            {
                ErrorMensaje errorMensaje = new("Escriba el contenido del comentario para poder enviarlo");
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
                txtBoxComentario.Text = "";

                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                var respuesta = JsonSerializer.Deserialize<ComentarioDTO>(jsonString);
                int idComentario = respuesta.IdComentario;

                ComentarioPrincipalUserControl comentarioUserControl = new(new ComentarioDTO
                {
                    IdComentario = idComentario,
                    IdClase = _idClase,
                    NombreUsuario = SingletonUsuario.Nombres + " " + SingletonUsuario.Apellidos,
                    Descripcion = descripcionComentario,
                    Respuestas = []
                },
                _idClase);

                lstViewComentarios.Items.Add(comentarioUserControl);
            }
        }

        private void CerrarPagina(object sender, RoutedEventArgs e)
        {
            if (File.Exists(_tempArchivoPath))
            {
                File.Delete(_tempArchivoPath);
            }
            mdElementVideo.Stop();
        }
    }
}
