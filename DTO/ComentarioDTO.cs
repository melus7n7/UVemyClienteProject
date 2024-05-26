using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UVemyCliente.DTO
{
    public class ComentarioDTO
    {
        [JsonPropertyName("idComentario")]
        public int IdComentario { get; set; }
        [JsonPropertyName("idClase")]
        public int IdClase { get; set; }
        [JsonPropertyName("nombreUsuario")]
        public string NombreUsuario { get; set; }
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }
        [JsonPropertyName("fecha")]
        public DateTime Fecha { get; set; }
        [JsonPropertyName("respuestas")]
        public List<ComentarioDTO> Respuestas { get; set; }
    }
}
