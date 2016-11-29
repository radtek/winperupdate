﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace WinPerUpdateAdmin.Controllers.api
{
    public class ModuloController : ApiController
    {
        #region get

        [Route("api/Modulo/{idModulo:int}")]
        [HttpGet]
        public Object GetModulo(int idModulo)
        {
            try
            {
                var obj = ProcessMsg.Modulo.GetModulo(idModulo);
                if (obj == null)
                {
                    return Content(HttpStatusCode.BadRequest, (ProcessMsg.Model.VersionBo)null);
                }

                return obj;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message));
            }
        }

        [Route("api/Modulos")]
        [HttpGet]
        public Object Get()
        {
            try
            {
                var obj = ProcessMsg.Modulo.GetModulos(null);
                if (obj == null)
                {
                    return Content(HttpStatusCode.BadRequest, (ProcessMsg.Model.VersionBo)null);
                }

                return obj;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message));
            }
        }

        [Route("api/ModulosXlsx/{idUsuario:int}")]
        [HttpGet]
        public Object GetModulosXlsx(int idUsuario)
        {
            try
            {
                return ProcessMsg.Modulo.GetModulosXlsx(idUsuario);
            }
            catch(Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message));
            }
        }

        [Route("api/Modulos/Planilla")]
        [HttpGet]
        public Object GetPlanillaModulosXLSX()
        {
            try
            {
                string dirfmt = string.Format("{0}", HttpContext.Current.Server.MapPath("~/Fuentes/PlanillaModulos.xlsx"));

                Byte[] objByte = System.IO.File.ReadAllBytes(dirfmt);
                if (objByte == null)
                {
                    return Content(HttpStatusCode.BadRequest, (ByteArrayContent)null);
                }
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.Created);

                message.Content = new ByteArrayContent(objByte);
                message.Content.Headers.ContentLength = objByte.Length;
                message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                message.Content.Headers.Add("Content-Disposition", "attachment; filename=PlanillaModulos.xlsx");
                message.StatusCode = HttpStatusCode.OK;

                return message;

            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message));
            }
        }

        #endregion

        #region post
        [Route("api/Modulo")]
        [HttpPost]
        public Object PostModulo([FromBody] ProcessMsg.Model.ModuloBo modulo)
        {
            try
            {
                var moduloRes = ProcessMsg.Modulo.AddModulo(modulo);
                if (moduloRes != null)
                {
                    return Content(HttpStatusCode.OK, (ProcessMsg.Model.ModuloBo)moduloRes);
                }
                return Content(HttpStatusCode.BadRequest, (ProcessMsg.Model.ModuloBo)null);
            }
            catch(Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message));
            }
        }

        #endregion

        #region put
        [Route("api/Modulo/{idModulo:int}/Vigente")]
        [HttpPut]
        public Object SetVigente(int idModulo)
        {
            try
            {
                if (ProcessMsg.Modulo.SetVigente(idModulo) == 1)
                {
                    return Content(HttpStatusCode.OK, true);
                }
                return Content(HttpStatusCode.Created, false);
            }
            catch(Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message));
            }
        }

        [Route("api/Modulo/{idModulo:int}")]
        [HttpPut]
        public Object UpdModulo(int idModulo, [FromBody] ProcessMsg.Model.ModuloBo modulo)
        {
            try
            {
                modulo.idModulo = idModulo;
                if (ProcessMsg.Modulo.UpdModulo(modulo) == 1)
                {
                    return Content(HttpStatusCode.OK, true);
                }
                return Content(HttpStatusCode.Created, false);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message));
            }
        }
        #endregion

        #region delete
        [Route("api/Modulo/{idModulo:int}")]
        [HttpDelete]
        public Object DelModulo(int idModulo)
        {
            try
            {
                if (ProcessMsg.Modulo.DelModulo(idModulo) == 1)
                {
                    return Content(HttpStatusCode.OK, true);
                }
                return Content(HttpStatusCode.Created, false);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message));
            }
        }
        #endregion
    }
}
