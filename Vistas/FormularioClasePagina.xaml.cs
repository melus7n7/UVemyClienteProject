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

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Lógica de interacción para FormularioClasePagina.xaml
    /// </summary>
    public partial class FormularioClase : Page
    {
        private List<DocumentoDTO> _documentosClase;
        private DocumentoDTO _videoDocumento;
        //private byte[] _videoClase;
        public FormularioClase()
        {
            InitializeComponent();
            _documentosClase = new List<DocumentoDTO>();
            _videoDocumento = null;
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
            BrushConverter brush = new BrushConverter();
            if (string.IsNullOrWhiteSpace(txtBlockNombreClase.Text))
            {
                esValido = false;
                txtBlockNombreClase.Background = (SolidColorBrush) brush.ConvertFrom("#f19090");
            }

            if (string.IsNullOrWhiteSpace(txtBlockDescripcion.Text))
            {
                esValido = false;
                txtBlockDescripcion.Background = (SolidColorBrush)brush.ConvertFrom("#f19090");
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
                ErrorMensaje error = new ErrorMensaje();
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
            DocumentoDTO videoNuevo = new DocumentoDTO
            {
                Archivo = _videoDocumento.Archivo,
                Nombre = _videoDocumento.Nombre,
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
                try
                {
                    archivo = File.ReadAllBytes(ventanaArchivo.FileName);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex);
                    MostrarMensajeErrorArchivo();
                }

                if (archivo != null)
                {
                    DocumentoDTO documento = new DocumentoDTO 
                    { 
                        Archivo = archivo, Nombre = ventanaArchivo.SafeFileName
                    };

                    _documentosClase.Add(documento);
                    lstBoxDocumentos.Items.Add(documento);
                    lstBoxDocumentos.Items.Refresh();
                }

            }
        }

        private void MostrarMensajeErrorArchivo()
        {
            ErrorMensaje error = new ErrorMensaje();
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
                try
                {
                    videoArchivo = File.ReadAllBytes(ventanaArchivo.FileName);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex);
                    MostrarMensajeErrorArchivo();
                }

                if (videoArchivo != null)
                {
                    _= MostrarVideoAsync(videoArchivo, ventanaArchivo.SafeFileName);
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

                _videoDocumento = new DocumentoDTO
                {
                    Archivo = videoBytes,
                    Nombre = nombreVideo
                };
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex);
                ErrorMensaje error = new ErrorMensaje();
                error.Show();
            }
        }

        private void ClicEliminarVideo(object sender, RoutedEventArgs e)
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

    }
}
