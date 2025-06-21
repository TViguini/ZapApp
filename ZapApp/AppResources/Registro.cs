using System;
using System.ComponentModel.DataAnnotations;

namespace ZapApp.AppResources
{
    public class Registro
    {
        [Key]
        public int Id { get; set; }

        public string CNS { get; set; }
        public string Nome { get; set; }
        public string Procedimento { get; set; }
        public string Local { get; set; }
        public DateTime Data_Agenda { get; set; }
        public string Telefone { get; set; }
        public bool Enviado { get; set; }
        public DateTime Data_Zap { get; set; }
    }
    public class Path_PDF
    {
        [Key]
        public int Id { get; set; }

        public string Path { get; set; }
    }
}
