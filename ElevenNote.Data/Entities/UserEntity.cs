using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ElevenNote.Data.Entities
{
    public class UserEntity
    {
        [Key]
        public int Id {get; set;}

        [Required]
        [EmailAddress]
        public int Email {get; set;}

        [Required]
        public string Username {get; set;}
        
        [Required]
        public string Password {get; set;}

        public string FirstName {get; set;}
        public string LastName {get; set;}

        [Required]
        public DateTime DateCreated {get; set;}
    }
}