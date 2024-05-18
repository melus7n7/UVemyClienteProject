using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UVemyCliente.DTO
{
    public class EtiquetaDTO
    {
        [JsonPropertyName("idEtiqueta")]
        public int IdEtiqueta { get; set; }
        [JsonPropertyName("nombre")]
        public required string Nombre { get; set; }
    }
}
