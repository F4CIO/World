using CraftSynth.BuildingBlocks.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace CraftSynth.BuildingBlocks.IO.Http
{
    public partial class ServerResponse
    {     
        public bool IsSuccess { get; set; }
        public System.Net.HttpStatusCode HttpStatusCode { get; set; }
        public string Message { get; set; }
        public int? LogId { get; set; }
        public string LogDetailsUri { get; set; }
        public string Tag { get; set; }
        public object Data { get; set; }

        /// <summary>
        /// Needed for JavaScriptSerializer deserialize.
        /// </summary>
        public ServerResponse()
        {

        }

        public ServerResponse(bool isSuccess, string message = null, int? logId = null, object data = null, HttpStatusCode? httpStatusCode = null, string tag = null)
        {
            this.IsSuccess = isSuccess;
            this.HttpStatusCode = httpStatusCode??(isSuccess? HttpStatusCode.OK:HttpStatusCode.InternalServerError);
            this.Message = message;
            this.LogId = logId;
            //if (this.LogId != null)
            //{
            //    this.LogDetailsUri = Services.HelperForUriMvc.BuildUriToAction(System.Web.HttpContext.Current, MVC.Manage.ActionNames.LogDetails, MVC.Manage.Name, false, new KeyValuePair<string, string>("id", this.LogId.ToString()));
            //}
            this.Tag = tag;
            this.Data = data;            
        }

        public override string ToString()
        {
            string r = $"IsSuccess={this.IsSuccess}, Message='{this.Message.ToNonNullString("null")}', LogId={this.LogId.ToNonNullString("null")}, LogDetailsUri={this.LogDetailsUri.ToNonNullString("null")}";
            return r;
        }

        public virtual string ToJson()
        {
            string r = JsonSerializer.Serialize(this);
            return r;
        }
    }
}