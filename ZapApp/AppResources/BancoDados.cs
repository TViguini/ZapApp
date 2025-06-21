using System;
using System.Collections.Generic;
using System.Linq;

namespace ZapApp.AppResources
{
    public class BancoDados
    {
        public void SalvarRegistro(string cns, string nome, string procedimento, string local, DateTime data_agenda, string telefone, bool enviado, DateTime data_zap)
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated(); // Cria o banco se não existir

            var registro = new Registro
            {
                CNS = cns,
                Nome = nome,
                Procedimento = procedimento,
                Local = local,
                Data_Agenda = data_agenda,
                Telefone = telefone,
                Enviado = enviado,
                Data_Zap = data_zap
            };

            db.Registros.Add(registro);
            db.SaveChanges();
        }

        public List<Registro> ConsultarTodos()
        {
            using var db = new AppDbContext();
            return db.Registros.OrderBy(r => r.Data_Agenda).ToList();
        }

        public List<Registro> ConsultarPorTelefone(string telefone)
        {
            using var db = new AppDbContext();
            return db.Registros
                     .Where(r => r.Telefone == telefone)
                     .OrderByDescending(r => r.Data_Agenda)
                     .ToList();
        }

        public List<Registro> ConsultarNaoEnviados()
        {
            using var db = new AppDbContext();
            return db.Registros.Where(r => !r.Enviado).ToList();
        }

        public void MarcarComoEnviado(int id)
        {
            using var db = new AppDbContext();
            var registro = db.Registros.Find(id);
            if (registro != null)
            {
                registro.Enviado = true;
                db.SaveChanges();
            }
        }

        public void SalvarPathPDF(string pathPDF)
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            // Tenta buscar o registro com Id = 1
            var registroExistente = db.Path_PDF.FirstOrDefault(p => p.Id == 1);

            if (registroExistente != null)
            {
                // Atualiza o valor
                registroExistente.Path = pathPDF;
                db.Path_PDF.Update(registroExistente);
            }
            else
            {
                // Cria novo com Id = 1
                var novoRegistro = new Path_PDF
                {
                    Id = 1,
                    Path = pathPDF
                };
                db.Path_PDF.Add(novoRegistro);
            }
            db.SaveChanges();
        }

    }
}
