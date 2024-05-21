using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.Win32;
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
using UVemyCliente.Conexion;
using UVemyCliente.DTO;
using UVemyCliente.Servicios;
using UVemyCliente.Utilidades;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Lógica de interacción para FormularioClasePagina.xaml
    /// </summary>
    public partial class FormularioClase : Page
    {
        private ClaseDTO _claseActual;
        private List<DocumentoDTO> _documentosClase;
        private DocumentoDTO _videoDocumento;
        public FormularioClase()
        {
            InitializeComponent();
            PrepararMediaElement();
            _documentosClase = new List<DocumentoDTO>();
            _videoDocumento = null;

        }
        public FormularioClase(ClaseDTO clase)
        {
            InitializeComponent();
            PrepararMediaElement();
            MostrarClaseActual(clase);
        }

        private void MostrarClaseActual(ClaseDTO clase)
        {
            _claseActual = clase;
            _documentosClase = clase.Documentos;
            _videoDocumento = clase.Video;
            txtBlockNombreClase.Text = clase.Nombre;
            txtBlockDescripcion.Text = clase.Descripcion;

            if (_documentosClase == null)
            {
                _documentosClase = new List<DocumentoDTO>();
                ErrorMensaje error = new ErrorMensaje("No se pudieron recuperar los documentos de la clase");
                error.Show();
            }
            else
            {
                _documentosClase.ForEach(documento => lstBoxDocumentos.Items.Add(documento));
            }

            if (_videoDocumento == null)
            {
                ErrorMensaje error = new ErrorMensaje("No se pudo recuperar el video de la clase");
                error.Show();
            }
            else
            {
                //Mostrar el video guardado
            }

            btnEliminarClase.Visibility = Visibility.Visible;
            btnGuardar.Click -= ClicGuardarClase;
            btnGuardar.Click += ClicActualizarClase;
        }

        private void PrepararMediaElement()
        {
            mdElementVideo.LoadedBehavior = MediaState.Manual;
            mdElementVideo.UnloadedBehavior = MediaState.Manual;
            mdElementVideo.Stretch = Stretch.Uniform;
        }

        private void ClicGuardarClase(object sender, RoutedEventArgs e)
        {
            bool esValido = ValidarInformacion();

            if (esValido)
            {
                _ = GuardarClaseAsync();
            }
        }

        private bool ValidarInformacion()
        {
            ResetearCampos();
            bool esValido = true;
            string razones = "";
            BrushConverter brush = new BrushConverter();
            if (string.IsNullOrWhiteSpace(txtBlockNombreClase.Text) || txtBlockNombreClase.Text.Length > 150)
            {
                esValido = false;
                txtBlockNombreClase.Background = (SolidColorBrush) brush.ConvertFrom("#f19090");
                razones += "El nombre es obligatorio y debe ser menor a 150 caracteres";
            }

            if (string.IsNullOrWhiteSpace(txtBlockDescripcion.Text) || txtBlockDescripcion.Text.Length > 660)
            {
                esValido = false;
                txtBlockDescripcion.Background = (SolidColorBrush)brush.ConvertFrom("#f19090");
                string problema = "La descripción es obligatoria debe ser menor a 660 caracteres";
                razones = (razones.Length > 0 ) ? razones + "; " + problema : problema;

            }

            if (_documentosClase.Count == 0)
            {
                esValido = false;
                lstBoxDocumentos.Background = (SolidColorBrush)brush.ConvertFrom("#f19090");
            }

            if (_videoDocumento == null)
            {
                esValido = false;
                brdVideo.Background = (SolidColorBrush)brush.ConvertFrom("#f19090");
            }

            if (!esValido)
            {
                ErrorMensaje error = new ErrorMensaje("Campos no válidos: " + razones);
                error.Show();
            }

            return esValido;
        }
        

        private void ResetearCampos()
        {
            BrushConverter brush = new BrushConverter();
            txtBlockNombreClase.Background = (SolidColorBrush)brush.ConvertFrom("#D9E1E4");
            txtBlockDescripcion.Background = (SolidColorBrush)brush.ConvertFrom("#D9E1E4");
            lstBoxDocumentos.Background = (SolidColorBrush)brush.ConvertFrom("#D9E1E4");
            brdVideo.Background = (SolidColorBrush)brush.ConvertFrom("#D9E1E4");
        }


        private async Task GuardarClaseAsync()
        {
            grdPrincipal.IsEnabled = false;
            ClicPausar(null, null);

            ClaseDTO clase = new ClaseDTO
            {
                Nombre = txtBlockNombreClase.Text,
                Descripcion = txtBlockDescripcion.Text,
                IdCurso = 1 //To-Do
            };

            var json = JsonSerializer.Serialize(clase);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, "clases", content);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;
            

            if (codigoRespuesta >= 400)
            {
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo guardar la clase, inténtelo más tarde");
                error.Show();
            }
            else
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                ClaseDTO? claseNueva = JsonSerializer.Deserialize<ClaseDTO>(jsonString);
                if (claseNueva != null)
                {
                    await GuardarDocumentosAsync(claseNueva.Id);
                }
            }

            grdPrincipal.IsEnabled = true;
        }

        private async Task GuardarDocumentosAsync(int idClase)
        {
            foreach (DocumentoDTO documento in _documentosClase)
            {
                var contenido = new MultipartFormDataContent
                {
                    { new StringContent(documento.Nombre), "nombre" },
                    { new StringContent(idClase.ToString()), "idClase" }
                };

                var contenidoDocumento = new ByteArrayContent(documento.Archivo);
                contenidoDocumento.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                contenido.Add(contenidoDocumento, "file", documento.Nombre + ".pdf");

                HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, "documentos/clase", contenido);
                int codigoRespuesta = (int)respuestaHttp.StatusCode;    

                if (codigoRespuesta >= 400)
                {
                    Debug.WriteLine(codigoRespuesta);
                    ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo guardar el documento, revise la clase en la lista de clases");
                    error.Show();
                    RedirigirListaClases();
                    return;
                }
            }

            await GuardarVideoAsync(idClase);
        }

        private async Task GuardarVideoAsync(int idClase)
        {
            DocumentoDTO videoNuevo = new DocumentoDTO
            {
                IdDocumento = 0,
                Archivo = _videoDocumento.Archivo,
                Nombre = _videoDocumento.Nombre,
                IdClase = idClase
            };
            int respuesta = await VideoGrpc.EnviarVideoDeClaseAsync(videoNuevo);

            if (respuesta >= 400)
            {
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo guardar el video de la clase, revise la clase en la lista");
                error.Show();
            }
            else
            {
                ExitoMensaje exito = new ExitoMensaje("Se ha guardado la clase y el video correctamente");
                exito.Show();
            }

            RedirigirListaClases();
            
        }

        private void RedirigirListaClases()
        {
            DetallesCurso curso = new DetallesCurso();
            NavigationService.Navigate(curso);
        }

        private void ClicAgregarDocumento(object sender, RoutedEventArgs e)
        {
            AdjuntarDocumento();
        }

        private void AdjuntarDocumento()
        {
            OpenFileDialog ventanaArchivo = new OpenFileDialog();
            ventanaArchivo.Title = "Seleccione el archivo a adjuntar para la clase";
            ventanaArchivo.CheckFileExists = true;
            ventanaArchivo.CheckPathExists = true;
            ventanaArchivo.Filter = "Solo archivos PDF (*.pdf)|*.pdf";
            bool respuesta = (bool)ventanaArchivo.ShowDialog();

            if (respuesta)
            {
                byte[] archivo = null;
                string nombre = Path.GetFileNameWithoutExtension(ventanaArchivo.FileName);

                try
                {
                    archivo = File.ReadAllBytes(ventanaArchivo.FileName);
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex);
                    MostrarMensajeErrorArchivo();
                }

                if (archivo != null)
                {
                    if (ArchivoNoSuperaTamanio(archivo, TamanioDocumentos.TAMANIO_MAXIMO_DOCUMENTOS_KB, nombre))
                    {
                        DocumentoDTO documento = new DocumentoDTO
                        {
                            Archivo = archivo,
                            Nombre = nombre
                        };

                        _documentosClase.Add(documento);
                        lstBoxDocumentos.Items.Add(documento);
                        lstBoxDocumentos.Items.Refresh();
                    }

                }

            }
        }
        private bool ArchivoNoSuperaTamanio(byte[] archivo, float limiteMB, string nombre)
        {
            bool documentoExcedeTamanio = true;
            bool nombreExcedeTamano = true;
            if (archivo != null)
            {
                float tamanioArchivo = (archivo.Length / 1024.0F);
                if (tamanioArchivo < limiteMB)
                {
                    documentoExcedeTamanio = false;
                }
                else
                {
                    float mb = limiteMB / 1000;
                    ErrorMensaje error = new ErrorMensaje("El tamaño del archivo supera el límite de " + mb + "MB");
                    error.Show();
                }
            }

            if (nombre.Length > 35)
            {
                ErrorMensaje error = new ErrorMensaje("El nombre supera el límite de 35 caracteres");
                error.Show();
            }
            else
            {
                nombreExcedeTamano = false;
            }

            return !documentoExcedeTamanio && !nombreExcedeTamano;
        }

        private void MostrarMensajeErrorArchivo()
        {
            ErrorMensaje error = new ErrorMensaje("Ocurrió un error al adjuntar el archivo, verifique el archivo");
            error.Show();
        }

        private void ClicEliminarDocumento(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            DocumentoDTO documento = (DocumentoDTO)btn.DataContext;

            if (documento != null)
            {
                _documentosClase.Remove(documento);
                lstBoxDocumentos.Items.Remove(documento);
                lstBoxDocumentos.Items.Refresh();
            }
        }

        private void ClicAgregarVideo(object sender, RoutedEventArgs e)
        {
            AdjuntarVideo();
        }

        private void AdjuntarVideo()
        {
            OpenFileDialog ventanaArchivo = new OpenFileDialog();
            ventanaArchivo.Title = "Seleccione el video para adjuntar a la clase";
            ventanaArchivo.CheckFileExists = true;
            ventanaArchivo.CheckPathExists = true;
            ventanaArchivo.Filter = "Solo archivos PDF (*.mp4)|*.mp4";
            bool respuesta = (bool)ventanaArchivo.ShowDialog();

            if (respuesta)
            {
                byte[] videoArchivo = null;
                string nombre = Path.GetFileNameWithoutExtension(ventanaArchivo.FileName);
                try
                {
                    videoArchivo = File.ReadAllBytes(ventanaArchivo.FileName);
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex);
                    MostrarMensajeErrorArchivo();
                }

                if (videoArchivo != null)
                {
                    if (ArchivoNoSuperaTamanio(videoArchivo, TamanioDocumentos.TAMANIO_MAXIMO_VIDEOS_KB, nombre))
                    {
                        _ = MostrarVideoAsync(videoArchivo, nombre);
                    }
                }

            }
        }


        private async Task MostrarVideoAsync(byte[] videoBytes, string nombreVideo)
        {
            try
            {
                string tempDirectory = Path.GetTempPath();
                string tempFilePath = Path.Combine(tempDirectory, "tempVideoUVemy.mp4");
                await File.WriteAllBytesAsync(tempFilePath, videoBytes);

                mdElementVideo.Source = new Uri(tempFilePath);

                btnReproducir.Visibility = Visibility.Visible;
                brdVideoBackground.Visibility = Visibility.Visible;
                btnAgregarVideo.IsEnabled = false;
                btnEliminarVideo.IsEnabled = true;

                if (_videoDocumento == null)
                {
                    _videoDocumento = new DocumentoDTO
                    {
                        Archivo = videoBytes,
                        Nombre = nombreVideo
                    };
                }
                else
                {
                    _videoDocumento.Archivo = videoBytes;
                    _videoDocumento.Nombre = nombreVideo;
                }

                
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error al mostrar el video, inténtelo de nuevo");
                error.Show();
            }
        }

        private void ClicEliminarVideo(object sender, RoutedEventArgs e)
        {
            EliminarVideo();          
        }

        private void EliminarVideo()
        {
            _videoDocumento = null;
            mdElementVideo.Stop();
            mdElementVideo.Source = null;
            string tempDirectory = Path.GetTempPath();
            string tempFilePath = Path.Combine(tempDirectory, "tempVideoUVemy.mp4");
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            btnAgregarVideo.IsEnabled = true;
            btnEliminarVideo.IsEnabled = false;
            btnReproducir.Visibility = Visibility.Collapsed;
            btnPausar.Visibility = Visibility.Collapsed;
            brdVideoBackground.Visibility = Visibility.Collapsed;
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

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
            EliminarVideo();
        }

        private void ClicEliminarClase(object sender, RoutedEventArgs e)
        {
            string message = "¿Desea eliminar la clase?";
            string title = "ELiminación de clase";
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            MessageBoxResult result = MessageBox.Show(message, title, buttons);
            if (result == MessageBoxResult.Yes)
            {
                _ = EliminarClaseAsync();
            }
        }

        private async Task EliminarClaseAsync()
        {
            grdPrincipal.IsEnabled = false;

            string url = "clases/" + _claseActual.Id;
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Delete, url);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo eliminar la clase, inténtelo más tarde");
                error.Show();
            }
            else
            {
                ExitoMensaje mensaje = new ExitoMensaje("Se ha eliminado la clase exitosamente");
                mensaje.Show();

                DetallesCurso curso = new DetallesCurso();
                NavigationService.Navigate(curso);
                EliminarVideo();
            }

            grdPrincipal.IsEnabled = true;
        }

        private void ClicActualizarClase(object sender, RoutedEventArgs e)
        {
            bool esValido = ValidarInformacion();

            if (esValido)
            {
                _ = ActualizarClaseAsync();
            }
        }

        private async Task ActualizarClaseAsync()
        {
            grdPrincipal.IsEnabled = false;
            ClicPausar(null, null);

            ClaseDTO claseActualizada = new ClaseDTO
            {
                Id = _claseActual.Id,
                Nombre = txtBlockNombreClase.Text,
                Descripcion = txtBlockDescripcion.Text,
                IdCurso = 1 //To-Do
            };

            string url = "clases/" + _claseActual.Id;
            var json = JsonSerializer.Serialize(claseActualizada);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Put, url, content);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;


            if (codigoRespuesta >= 400)
            {
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo actualizar la clase, inténtelo más tarde");
                error.Show();
            }
            else
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                ClaseDTO? claseNueva = JsonSerializer.Deserialize<ClaseDTO>(jsonString);
                if (claseNueva != null)
                {
                    await ActualizarDocumentosAsync();
                }
            }

            grdPrincipal.IsEnabled = true;
        }

        private async Task ActualizarDocumentosAsync()
        {
            string url = "documentos/clase";

            List<int> documentosEliminados = _claseActual.DocumentosId;

            for (int i = 0; i < _documentosClase.Count; i++)
            {
                if (_documentosClase[i].IdDocumento == 0)
                {
                    HttpResponseMessage respuestaHttp;
                    MultipartFormDataContent contenido = new MultipartFormDataContent
                    {
                        { new StringContent(_documentosClase[i].Nombre), "nombre" }
                    };

                    var contenidoDocumento = new ByteArrayContent(_documentosClase[i].Archivo);
                    contenidoDocumento.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                    contenido.Add(contenidoDocumento, "file", _documentosClase[i].Nombre + ".pdf");
                    contenido.Add(new StringContent(_claseActual.Id.ToString()), "idClase");
                    respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, url, contenido);

                    int codigoRespuesta = (int)respuestaHttp.StatusCode;

                    if (codigoRespuesta >= 400)
                    {
                        Debug.WriteLine(respuestaHttp.RequestMessage);
                        Debug.WriteLine(codigoRespuesta);

                        ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo actualizar los documentos, revise la clase en la lista de clases");
                        error.Show();
                        RedirigirDetallesClase();
                        return;
                    }
                }
                else
                {
                    documentosEliminados.Remove(_documentosClase[i].IdDocumento);
                }
            }

            for (int i = 0; i < documentosEliminados.Count; i++)
            {
                HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Delete, url + "/" + documentosEliminados[i]);

                int codigoRespuesta = (int)respuestaHttp.StatusCode;

                if (codigoRespuesta >= 400)
                {
                    Debug.WriteLine(codigoRespuesta);
                    ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo actualizar los documentos, revise la clase en la lista de clases");
                    error.Show();
                    RedirigirDetallesClase();
                    return;
                }
            }

            await ActualizarVideoAsync();

        }

        private async Task ActualizarVideoAsync()
        {
            DocumentoDTO videoNuevo = new DocumentoDTO
            {
                IdDocumento = _videoDocumento.IdDocumento,
                Archivo = _videoDocumento.Archivo,
                Nombre = _videoDocumento.Nombre,
                IdClase = _claseActual.Id
            };
            int respuesta = await VideoGrpc.EnviarVideoDeClaseAsync(videoNuevo);

            if (respuesta >= 400)
            {
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo actualizar el video de la clase, revise la clase en la lista");
                error.Show();
            }
            else
            {
                ExitoMensaje exito = new ExitoMensaje("Se ha actualizado la clase y el video correctamente");
                exito.Show();
            }

            RedirigirDetallesClase();
        }

        private void RedirigirDetallesClase()
        {
            DetallesClase clase = new DetallesClase(_claseActual.Id);
            NavigationService.Navigate(clase);
            EliminarVideo();
        }
    }
}
