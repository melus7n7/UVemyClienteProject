using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UVemyCliente.DTO
{
    public class EstadisticaCursoDTO
    {
        [JsonPropertyName("nombre")]
        public string NombreCurso { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        [JsonPropertyName("calificacionCurso")]
        public double? Calificacion { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        [JsonPropertyName("promedioComentarios")]
        public double? PromedioComentarios { get; set; }
        [JsonPropertyName("estudiantesInscritos")]
        public int EstudiantesInscritos { get; set; }
        [JsonPropertyName("etiquetasCoinciden")]
        public List<string> EtiquetasCoinciden { get; set; }
        [JsonPropertyName("estudiantes")]
        public List<string> EstudiantesCurso { get; set; }
        [JsonPropertyName("clases")]
        public List<ClaseEstadisticaDTO> ClasesEstadistcas { get; set; }
    }

    public class ClaseEstadisticaDTO
    {
        public int NumeroClase { get; set; }
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }
        [JsonPropertyName("cantidadComentarios")]
        public int CantidadComentarios { get; set; }
    }

}
