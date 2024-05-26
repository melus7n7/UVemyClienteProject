using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UVemyCliente.DTO
{
    public class UsuarioEtiquetasDTO
    {
        [JsonPropertyName("idUsuario")]
        public int IdUsuario { get; set; }
        [JsonPropertyName("idsEtiqueta")]
        public List<int> IdsEtiqueta { get; set; }
    }
}
