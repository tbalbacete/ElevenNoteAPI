using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ElevenNote.Models.Token
{
    public class TokenRequest
    {
        [Required]
        public string Username {get; set;}
        
        [Required]
        public string Password {get; set;}
    }
}