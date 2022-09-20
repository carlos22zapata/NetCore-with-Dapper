using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ARIS_Net.Models
{
    public class ImportModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Enable { get; set; }
        public string Site { get; set; }
        public List<JsonModel> JsonModel { get; set; }
    }

    public class JsonModel
    {
        public string fieldFile { get; set; }
        public string fieldBD { get; set; }
    }
}
