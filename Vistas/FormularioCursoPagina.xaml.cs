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
using System.Windows.Shapes;
using UVemyCliente.Conexion;
using UVemyCliente.DTO;
using UVemyCliente.Utilidades;

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Lógica de interacción para FormularioCursoPagina.xaml
    /// </summary>
    public partial class FormularioCursoPagina : Page
    {

        private List<string> _listNombreEtiquetas = new List<string>() { };
        private List<int> _listIdEtiquetas = new List<int>();
        private byte[] _arrayImagen = Array.Empty<byte>();
        private CursoDTO _curso;
        private bool _esCrearCurso = false;
        private DocumentoDTO _documento = new DocumentoDTO();

        public FormularioCursoPagina()
        {
            InitializeComponent();
            _esCrearCurso = true; 
            MostrarBotones();
        }

        public FormularioCursoPagina(CursoDTO curso, List<EtiquetaDTO> etiquetas, DocumentoDTO documento, bool esCrearCurso)
        {
            _esCrearCurso = esCrearCurso;
            foreach (EtiquetaDTO etiqueta in etiquetas)
            {
                _listNombreEtiquetas.Add(etiqueta.Nombre);
                _listIdEtiquetas.Add(etiqueta.IdEtiqueta);
            }
            _documento = documento;
            Debug.WriteLine(_documento.IdDocumento);
            _curso = new CursoDTO()
            {
                Descripcion = curso.Descripcion,
                Objetivos = curso.Objetivos,
                Requisitos = curso.Requisitos,
                Titulo = curso.Titulo
            };
            if (curso.IdCurso != null)
            {
                _curso.IdCurso = curso.IdCurso;
            }
            InitializeComponent();
            CargarCurso();
            MostrarBotones();

        }

        private void MostrarBotones()
        {
            if (_esCrearCurso)
            {
                btnGuardarCurso.Visibility = Visibility.Visible;
                btnEliminarCurso.Visibility = Visibility.Hidden;
                btnModificarCurso.Visibility = Visibility.Hidden;
            }
            else
            {
                btnEliminarCurso.Visibility = Visibility.Visible;
                btnModificarCurso.Visibility = Visibility.Visible;
                btnGuardarCurso.Visibility = Visibility.Hidden;
            }
        }

        private void CargarCurso()
        {
            txtBoxTitulo.Text = _curso.Titulo;
            txtBoxDescripcion.Text = _curso.Descripcion;
            txtBoxObjetivos.Text = _curso.Objetivos;
            txtBoxRequisitos.Text = _curso.Requisitos;
            if (_documento.Archivo != null)
            {
                CargarImagen(_documento.Archivo);
            }
            CargarTemasInteres(_listIdEtiquetas, _listNombreEtiquetas);
        }

        private void CargarImagen(byte[] arrayImagen)
        {
            _documento.Archivo = arrayImagen;
            _arrayImagen = arrayImagen;

            BitmapImage imagen = new BitmapImage();
            using (MemoryStream memoryStream = new MemoryStream(_arrayImagen))
            {
                memoryStream.Position = 0;
                imagen.BeginInit();
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.StreamSource = memoryStream;
                imagen.EndInit();
            }
            imgMiniatura.Source = imagen;
        }

        private void ClicEliminarMiniatura(object sender, RoutedEventArgs e)
        {
            imgMiniatura.Source = null;
            _arrayImagen = Array.Empty<byte>();
        }

        private void ClicGuardarCurso(object sender, RoutedEventArgs e)
        {
            bool sonCamposValidos = ValidarCampos();
            if (sonCamposValidos)
            {
                btnGuardarCurso.IsEnabled = false;
                _ = GuardarCursoAsync();
            }
        }

        private bool ValidarCampos()
        {
            bool sonCamposValidos = true;
            bool sonEtiquetasValidas = true;
            string tituloCurso = txtBoxTitulo.Text;
            string descripcionCurso = txtBoxDescripcion.Text;
            string requisitosCurso = txtBoxRequisitos.Text;
            string objetivosCurso = txtBoxObjetivos.Text;
            string problema = "";
            string razones = "";

            if (String.IsNullOrWhiteSpace(tituloCurso) || tituloCurso.Length >= 150)
            {
                sonCamposValidos = false;
                txtBoxTitulo.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampoErroneos");
                razones += "El titulo es obligatorio y debe ser menor a 150 caracteres";

            }
            else
            {
                txtBoxTitulo.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampo");
            }
            if (String.IsNullOrWhiteSpace(descripcionCurso) || descripcionCurso.Length >= 660)
            {
                sonCamposValidos = false;
                txtBoxDescripcion.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampoErroneos");
                problema = "La descripcion es obligatoria y debe ser menor a 660 caracteres";
                razones = (razones.Length > 0) ? razones + "; " + problema : problema;
            }
            else
            {
                txtBoxDescripcion.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampo");
            }
            if (String.IsNullOrWhiteSpace(requisitosCurso) || requisitosCurso.Length >= 300)
            {
                sonCamposValidos = false;
                txtBoxRequisitos.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampoErroneos");
                problema = "Los requisitos son obligatorios y deben ser menor a 300 caracteres";
                razones = (razones.Length > 0) ? razones + "; " + problema : problema;
            }
            else
            {
                txtBoxRequisitos.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampo");
            }
            if (String.IsNullOrWhiteSpace(objetivosCurso) || objetivosCurso.Length >= 600)
            {
                sonCamposValidos = false;
                txtBoxObjetivos.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampoErroneos");
                problema = "Los objetivos son obligatorios y deben ser menor a 600 caracteres";
                razones = (razones.Length > 0) ? razones + "; " + problema : problema;

            }
            else
            {
                txtBoxObjetivos.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampo");
            }
            if (_listIdEtiquetas.Count <= 0)
            {
                sonCamposValidos = false;
                sonEtiquetasValidas = false;
                lstBoxEtiquetas.Style = (Style)FindResource("estiloLstBoxTemasInteresErroneo");
                problema = "Las etiquetas son obligatoras";
                razones = (razones.Length > 0) ? razones + "; " + problema : problema;
            }
            else
            {
                lstBoxEtiquetas.Style = (Style)FindResource("estiloLstBoxTemasInteres");
            }
            if (_arrayImagen == null || _arrayImagen.Length <= 0)
            {
                sonCamposValidos = false;
                brdMiniatura.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F19090"));
                problema = "La minuatura del curso es obligatoria";
                razones = (razones.Length > 0) ? razones + "; " + problema : problema;
            }
            else
            {
                brdMiniatura.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9E1E4"));
            }
            if (!sonCamposValidos)
            {
                ErrorMensaje error =new ErrorMensaje("Campos no válidos: " + razones);
                error.Show();
                if (!sonEtiquetasValidas)
                {
                    ErrorMensaje error2 = new ErrorMensaje("El curso debe tener por lo menos un tema de interes");
                    error2.Show();
                }
            }

            return sonCamposValidos;
        }

        private async Task GuardarCursoAsync()
        {
            string tituloCurso = txtBoxTitulo.Text;
            string descripcionCurso = txtBoxDescripcion.Text;
            string requisitosCurso = txtBoxRequisitos.Text;
            string objetivosCurso = txtBoxObjetivos.Text;

            var content = new MultipartFormDataContent 
            {
                { new StringContent(tituloCurso), "titulo" },
                { new StringContent(descripcionCurso), "descripcion" },
                { new StringContent(requisitosCurso), "requisitos" },
                { new StringContent(objetivosCurso), "objetivos" },
            };
            
            for (int i = 0; i < _listIdEtiquetas.Count; i++)
            {
                content.Add(new StringContent(_listIdEtiquetas[i].ToString()), "etiquetas[" + i + "]"); 
            }
            var contenidoDocumento = new ByteArrayContent(_documento.Archivo);
            contenidoDocumento.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Add(contenidoDocumento,"file","fileNuevo.png");
            LimpiarCampos();
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, "cursos", content);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;
            Debug.WriteLine(codigoRespuesta);
            if (codigoRespuesta >= 400)
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                Debug.WriteLine(jsonString);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo guardar el curso, inténtelo más tarde");
                error.Show();
            }
            else
            {
                ExitoMensaje exito = new ExitoMensaje("Se creo el curso exitosamente");
                exito.Show();
            }
            btnGuardarCurso.IsEnabled = true;
        }

        private void LimpiarCampos()
        {
            txtBoxTitulo.Text = "";
            txtBoxDescripcion.Text = "";
            txtBoxRequisitos.Text = "";
            txtBoxObjetivos.Text = "";
            lstBoxEtiquetas.Items.Clear();
            _listIdEtiquetas = new List<int>();
            _listNombreEtiquetas = new List<string>();
            imgMiniatura.Source = null;
            _arrayImagen = Array.Empty<byte>();
            _documento = new DocumentoDTO();
            _curso = new CursoDTO();
        }

        private async Task EliminarCursoAsync()
        {
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Delete, "cursos/"+_curso.IdCurso);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;
            LimpiarCampos();
            Debug.WriteLine(codigoRespuesta);
            if (codigoRespuesta >= 400)
            {
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo eliminar el curso, inténtelo más tarde");
                error.Show();
            }
            else
            {
                ExitoMensaje exito = new ExitoMensaje("Se elimino el curso exitosamente");
                exito.Show();
                ListaCursosPagina lista = new ListaCursosPagina();
                this.NavigationService.Navigate(lista);
            }
            btnEliminarCurso.IsEnabled = true;
        }


        private void ClicAñadirTemas(object sender, RoutedEventArgs e)
        {
            string tituloCurso = txtBoxTitulo.Text;
            string descripcionCurso = txtBoxDescripcion.Text;
            string requisitosCurso = txtBoxRequisitos.Text;
            string objetivosCurso = txtBoxObjetivos.Text;
            CursoDTO curso = new CursoDTO
            {
                Titulo = tituloCurso,
                Descripcion = descripcionCurso,
                Requisitos = requisitosCurso,
                Objetivos = objetivosCurso,
                Etiquetas = _listIdEtiquetas,
            };
            if (_curso != null)
            {
                curso.IdCurso = _curso.IdCurso;
            };
            SeleccionEtiquetasPagina pagina = new SeleccionEtiquetasPagina(_listIdEtiquetas, _listNombreEtiquetas, curso, _documento, _esCrearCurso);
            this.NavigationService.Navigate(pagina);
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            ListaCursosPagina listaCursos = new ListaCursosPagina();
            this.NavigationService.Navigate(listaCursos);
        }

        private bool ValidarTamaño(byte[] archivo, float limiteMB, string nombre)
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

        private void MouseLeftButtonDownMiniatura(object sender, MouseButtonEventArgs e)
        {
            try
            {
                OpenFileDialog ventanaArchivo = new OpenFileDialog();
                ventanaArchivo.Title = "Seleccione una imagen como miniatura";
                ventanaArchivo.CheckFileExists = true;
                ventanaArchivo.CheckPathExists = true;
                ventanaArchivo.Filter = "Solo archivos PNG (*.png)|*.png";
                bool respuesta = (bool)ventanaArchivo.ShowDialog();

                if (respuesta)
                {
                    byte[] archivoProvisional = null;
                    string nombre = System.IO.Path.GetFileNameWithoutExtension(ventanaArchivo.FileName);
                    
                    archivoProvisional = File.ReadAllBytes(ventanaArchivo.FileName);
                    

                    if (archivoProvisional != null && ValidarTamaño(archivoProvisional, TamanioDocumentos.TAMANIO_MAXIMO_DOCUMENTOS_KB, nombre))
                    {
                        CargarImagen(archivoProvisional);
                    }

                }
            }
            catch (ArgumentException ex)
            {
                ErrorMensaje error = new ErrorMensaje("Se ha proporcionado un argumento invalido");
                error.Show();
            }
            catch (OutOfMemoryException ex)
            {
                ErrorMensaje error = new ErrorMensaje("Se ha agotado la memoria");
                error.Show();
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorMensaje error = new ErrorMensaje("Error al acceder a la imagen");
                error.Show();
            }
            catch (IOException ex)
            {
                ErrorMensaje error = new ErrorMensaje("Error al acceder a la imagen");
                error.Show();
            }

        }

        public void CargarTemasInteres(List<int> listIdEtiquetas, List<string> listNombreEtiquetas)
        {
            stcPanelTemasInteres.Children.Clear();
            foreach (string temaInteres in listNombreEtiquetas)
            {
                AgregarMensaje(temaInteres);
            }
            _listIdEtiquetas = listIdEtiquetas;
            _listNombreEtiquetas = listNombreEtiquetas;
        }

        private void AgregarMensaje(string nombreEtiqueta)
        {
            Label temasInteres = new Label();
            temasInteres.Style = (Style)FindResource("estiloLabelTemaInteres");
            temasInteres.Content = nombreEtiqueta;
            stcPanelTemasInteres.Children.Add(temasInteres);
        }

        private void ClicEliminarCurso(object sender, RoutedEventArgs e)
        {
            btnEliminarCurso.IsEnabled = false;
            _ = EliminarCursoAsync();
        }

        private void ClicModificarCurso(object sender, RoutedEventArgs e)
        {
            bool sonCamposValidos = ValidarCampos();
            if (sonCamposValidos)
            {
                btnModificarCurso.IsEnabled = false;
                _ = ModificarCursoAsync();
            }
        }

        private async Task ModificarCursoAsync()
        {

            string tituloCurso = txtBoxTitulo.Text;
            string descripcionCurso = txtBoxDescripcion.Text;
            string requisitosCurso = txtBoxRequisitos.Text;
            string objetivosCurso = txtBoxObjetivos.Text;
            var content = new MultipartFormDataContent
            {
                { new StringContent(_curso.IdCurso.ToString()), "idCurso" },
                { new StringContent(tituloCurso), "titulo" },
                { new StringContent(descripcionCurso), "descripcion" },
                { new StringContent(requisitosCurso), "requisitos" },
                { new StringContent(objetivosCurso), "objetivos" },
                { new StringContent(_documento.IdDocumento.ToString()), "idDocumento" }
            };
            for (int i = 0; i < _listIdEtiquetas.Count; i++)
            {
                content.Add(new StringContent(_listIdEtiquetas[i].ToString()), "etiquetas[" + i + "]");
            }
            var contenidoDocumento = new ByteArrayContent(_documento.Archivo);
            contenidoDocumento.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Add(contenidoDocumento, "file", "fileNuevo.png");

            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Put, "cursos/"+_curso.IdCurso, content);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;
            Debug.WriteLine(codigoRespuesta);
            if (codigoRespuesta >= 400)
            {
                Debug.WriteLine(codigoRespuesta);
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo guardar el curso, inténtelo más tarde");
                error.Show();
                Debug.WriteLine(jsonString);
            }
            else
            {
                ExitoMensaje exito = new ExitoMensaje("Se actualizo el curso exitosamente");
                exito.Show();
            }
            btnModificarCurso.IsEnabled = true;
        }
    }
}
