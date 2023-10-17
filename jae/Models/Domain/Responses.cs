using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;


namespace jae.Models.Domain
{
    public class Responses
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime? Response { get; set; }
        public string? Name { get; set; }
        public string? Course { get; set; }
        public string? School { get; set; }
        public string? Number { get; set; }
        public string? wfhft { get; set; }
        public DateTime? Datestart { get; set; }
        public string? Renderhrs { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Resume { get; set; } 

        public string? Status { get; set; }
    }
}