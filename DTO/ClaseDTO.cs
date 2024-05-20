using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UVemyCliente.DTO
{
    public class ClaseDTO
    {
        [JsonPropertyName("idClase")]
        public int Id { get; set; }
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }
        [JsonPropertyName("idCurso")]
        public int IdCurso { get; set; }
        [JsonPropertyName("documentos")]
        public List<DocumentoDTO> Documentos { get; set; }
        [JsonPropertyName("documentosId")]
        public List<int> DocumentosId { get; set; }
        [JsonPropertyName("videoId")]
        public int VideoId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DocumentoDTO Video { get; set; }

    }
}
