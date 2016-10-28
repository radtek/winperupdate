﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace WinperUpdateDAO
{
    public class CnaTareas:SpDao
    {
        /// <summary>
        /// Obtiene las tareas de un cliente y un perfil que esten en estado 0 (Aun no se ha ejecutado)
        /// </summary>
        /// <param name="idClientes"></param>
        /// <param name="CodPrf"></param>
        /// <returns></returns>
        public SqlDataReader Execute(int idClientes, int CodPrf)
        {
            SpName = @"SELECT t.idTareas, t.idClientes as CltTarea, a.*
                             ,t.CodPrf, t.Estado, t.Modulo, t.idVersion
                             ,t.NameFile, t.Error
                                             FROM Tareas t INNER JOIN Ambientes a
                                             ON t.idAmbientes = a.idAmbientes
                                             WHERE t.idClientes = @idClientes
                                             AND t.CodPrf = @CodPrf
                                             AND t.Estado = 0";
            try
            {
                ParmsDictionary.Add("@idClientes", idClientes);
                ParmsDictionary.Add("@CodPrf", CodPrf);
                return Connector.ExecuteQuery(SpName,ParmsDictionary);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Error al ejecutar {0}: {1}", "Excute", ex.Message);
                throw new Exception(msg, ex);
            }
        }
    }
}
