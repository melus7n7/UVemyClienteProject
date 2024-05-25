using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UVemyCliente.DTO
{
    public class CursoDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("idCurso")]
        public int? IdCurso { get; set; }
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; }
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }
        [JsonPropertyName("objetivos")]
        public string Objetivos { get; set; }
        [JsonPropertyName("requisitos")]
        public string Requisitos { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("idUsuario")]
        public int? IdUsuario { get; set; }
        [JsonPropertyName("etiquetas")]
        public List<int> Etiquetas { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("idDocumento")]
        public int? idDocumento { get; set; }
        [JsonPropertyName("file")]
        public byte[] Archivo { get; set; }
    }
}
