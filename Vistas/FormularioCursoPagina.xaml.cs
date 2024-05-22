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
        private string _rutaImagen;
        private CursoDTO _curso;
        
        public FormularioCursoPagina()
        {
            InitializeComponent();
            btnGuardarCurso.Visibility = Visibility.Visible;
            btnEliminarCurso.Visibility = Visibility.Hidden;
            btnModificarCurso.Visibility = Visibility.Hidden;
        }
        
        public FormularioCursoPagina(CursoDTO curso, List<EtiquetaDTO> etiquetas)
        {
            
            foreach (EtiquetaDTO etiqueta in etiquetas)
            {
                _listNombreEtiquetas.Add(etiqueta.Nombre);
                _listIdEtiquetas.Add(etiqueta.IdEtiqueta);
            }
            
            _curso = new CursoDTO()
            {
                IdCurso = curso.IdCurso,
                Descripcion = curso.Descripcion,
                Objetivos = curso.Objetivos,
                Requisitos = curso.Requisitos,
                Titulo = curso.Titulo
            };
            InitializeComponent();
            CargarCurso();
            btnEliminarCurso.Visibility = Visibility.Visible;
            btnModificarCurso.Visibility = Visibility.Visible;
            btnGuardarCurso.Visibility = Visibility.Hidden;
        }

        private void CargarCurso()
        {
            txtBoxTitulo.Text = _curso.Titulo;
            txtBoxDescripcion.Text = _curso.Descripcion;
            txtBoxObjetivos.Text = _curso.Objetivos;
            txtBoxRequisitos.Text = _curso.Requisitos;
            //CargarImagen(imageBytes);  //Cargar documento todo to do
            CargarTemasInteres(_listIdEtiquetas, _listNombreEtiquetas);
        }

        private void CargarImagen(byte[] arrayImagen)
        {
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
            _arrayImagen = null;
        }

        private void ClicGuardarCurso(object sender, RoutedEventArgs e)
        {
            bool sonCamposValidos = ValidarCampos();
            if (sonCamposValidos)
            {
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
            string razones = "";

            if (String.IsNullOrWhiteSpace(tituloCurso))
            {
                sonCamposValidos = false;
                txtBoxTitulo.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampoErroneos");
                razones += "Nombre checklist";
            }
            else
            {
                txtBoxTitulo.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampo");
            }
            if (String.IsNullOrWhiteSpace(descripcionCurso))
            {
                sonCamposValidos = false;
                txtBoxDescripcion.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampoErroneos");
                razones = (razones.Length > 0) ? razones + ", descripción" : "Descripción";
            }
            else
            {
                txtBoxDescripcion.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampo");
            }
            if (String.IsNullOrWhiteSpace(requisitosCurso))
            {
                sonCamposValidos = false;
                txtBoxRequisitos.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampoErroneos");
                razones = (razones.Length > 0) ? razones + ", requisitos curso" : "Requisitos curso";
            }
            else
            {
                txtBoxRequisitos.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampo");
            }
            if (String.IsNullOrWhiteSpace(objetivosCurso))
            {
                sonCamposValidos = false;
                txtBoxObjetivos.Style = (Style)FindResource("estiloTxtBoxFormularioCursoCampoErroneos");
                razones = (razones.Length > 0) ? razones + ", objetivos curso" : "Objetivos curso";
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
                razones = (razones.Length > 0) ? razones + ", etiquetas" : "Etiquetas";
            }
            else
            {
                lstBoxEtiquetas.Style = (Style)FindResource("estiloLstBoxTemasInteres");
            }
            if (_arrayImagen == null || _arrayImagen.Length <= 0)
            {
                sonCamposValidos = false;
                brdMiniatura.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F19090"));
                razones = (razones.Length > 0) ? razones + ", imagen curso" : "Imagen curso";
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
            CursoDTO curso = new CursoDTO
            {
                IdCurso = null,
                Titulo = tituloCurso,
                Descripcion = descripcionCurso,
                Requisitos = requisitosCurso,
                Objetivos = objetivosCurso,
                Etiquetas = _listIdEtiquetas,
                IdUsuario = 1//To DO TODO SingletonUsuario.IdUsuario
            };
            var json = JsonSerializer.Serialize(curso);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, "cursos", content);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo guardar el curso, inténtelo más tarde");
                error.Show();
            }
            else
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                CursoDTO? cursoNuevo = JsonSerializer.Deserialize<CursoDTO>(jsonString);
                Debug.WriteLine("correcto "+ cursoNuevo.IdCurso+" "+ cursoNuevo.Titulo + " " + cursoNuevo.Descripcion + " " + cursoNuevo.Objetivos);
            }
        }

        private async Task EliminarCursoAsync()
        {
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Delete, "cursos/"+_curso.IdCurso);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                Debug.WriteLine("Incorrecto");
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo eliminar el curso, inténtelo más tarde");
                error.Show();
            }
            else
            {
                Debug.WriteLine(codigoRespuesta);
                Debug.WriteLine("Correcto");
            }
        }


        private void ClicAñadirTemas(object sender, RoutedEventArgs e)
        {
            //SI ES MODIFICAR ENTONCES CURSO TIENE IDCURSO SINO ENTONCES NO LA PASO
            //string tituloCurso = txtBoxTitulo.Text;
            //string descripcionCurso = txtBoxDescripcion.Text;
            //string requisitosCurso = txtBoxRequisitos.Text;
            //string objetivosCurso = txtBoxObjetivos.Text;
            //CursoDTO curso = new CursoDTO
            //{
            //    IdCurso = _curso.IdCurso,
            //    Titulo = tituloCurso,
            //    Descripcion = descripcionCurso,
            //    Requisitos = requisitosCurso,
            //    Objetivos = objetivosCurso,
            //    Etiquetas = _listIdEtiquetas,
            //    IdUsuario = 1   //To DO TODO SingletonUsuario.IdUsuario
            //};
            //SeleccionEtiquetasPagina pagina = new SeleccionEtiquetasPagina(_listIdEtiquetas, _listNombreEtiquetas, curso);
            //this.NavigationService.Navigate(pagina);
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {

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
            _ = EliminarCursoAsync();
        }

        private void ClicModificarCurso(object sender, RoutedEventArgs e)
        {
            bool sonCamposValidos = ValidarCampos();
            if (sonCamposValidos)
            {
               _ = ModificarCursoAsync();
            }
        }

        private async Task ModificarCursoAsync()
        {
            string tituloCurso = txtBoxTitulo.Text;
            string descripcionCurso = txtBoxDescripcion.Text;
            string requisitosCurso = txtBoxRequisitos.Text;
            string objetivosCurso = txtBoxObjetivos.Text;
            CursoDTO curso = new CursoDTO
            {
                IdCurso = _curso.IdCurso,
                Titulo = tituloCurso,
                Descripcion = descripcionCurso,
                Requisitos = requisitosCurso,
                Objetivos = objetivosCurso,
                Etiquetas = _listIdEtiquetas,
                idDocumento = 85
            };
            var json = JsonSerializer.Serialize(curso);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Put, "cursos/"+curso.IdCurso, content);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

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
                Debug.WriteLine(codigoRespuesta);
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                CursoDTO? cursoNuevo = JsonSerializer.Deserialize<CursoDTO>(jsonString);
                Debug.WriteLine("correcto " + cursoNuevo.IdCurso + " " + cursoNuevo.Titulo + " " + cursoNuevo.Descripcion + " " + cursoNuevo.Objetivos);
            }
        }
    }
}
