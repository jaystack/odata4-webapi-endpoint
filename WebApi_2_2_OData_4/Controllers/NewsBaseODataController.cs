using JayData.Test.CommonItems.Entities;
using JayData.Test.WebApi_2_2_OData_4.Model;
using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;

using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using System.Net.Http;
using System.Web.Http.Routing;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Reflection;

namespace WebApi_2_2_OData_4.Controllers
{
    public class NewsBaseODataController<TElement> : ODataController where TElement : class
    {
        protected NewsReaderContext db = new NewsReaderContext();

        #region References
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
            string navigationProperty, [FromBody] Uri link)
        {
            var target = await db.Set<TElement>().FindAsync(key);
            if (target == null)
            {
                return NotFound();
            }

            var prop = target.GetType().GetProperty(navigationProperty);
            if (prop != null)
            {
                prop.SetValue(target, await GetValueUri(Request, link, prop));
            }

            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("DELETE")]
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
            string navigationProperty, [FromBody] Uri link)
        {
            var target = await db.Set<TElement>().FindAsync(key);
            if (target == null)
            {
                return NotFound();
            }

            var prop = target.GetType().GetProperty(navigationProperty);
            if (prop != null)
            {
                var values = prop.GetValue(target);
                prop.SetValue(target, null);
            }

            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        //[AcceptVerbs("DELETE")]
        //public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key, [FromODataUri] string relatedKey,
        //    string navigationProperty, [FromBody] Uri link)
        //{
        //    var target = await db.Set<TElement>().FindAsync(key);
        //    if (target == null)
        //    {
        //        return NotFound();
        //    }

        //    var prop = target.GetType().GetProperty(navigationProperty);
        //    if (prop != null)
        //    {
        //        var values = prop.GetValue(target);
        //        if (!prop.PropertyType.IsClass)
        //        {

        //        }

        //        prop.SetValue(target, null);
        //    }

        //    await db.SaveChangesAsync();
        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        public async Task<object> GetValueUri(HttpRequestMessage request, Uri uri, PropertyInfo prop)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var urlHelper = request.GetUrlHelper() ?? new UrlHelper(request);

            string serviceRoot = urlHelper.CreateODataLink(
                request.ODataProperties().RouteName,
                request.ODataProperties().PathHandler, new List<ODataPathSegment>());
            var odataPath = request.ODataProperties().PathHandler.Parse(
                request.ODataProperties().Model,
                serviceRoot, uri.LocalPath);


            var setSegment = odataPath.Segments.OfType<EntitySetPathSegment>().FirstOrDefault();
            if (setSegment == null)
            {
                throw new InvalidOperationException("The link does not contain an entityset.");
            }

            var keySegment = odataPath.Segments.OfType<KeyValuePathSegment>().FirstOrDefault();
            if (keySegment == null)
            {
                throw new InvalidOperationException("The link does not contain a key.");
            }

            var key = ODataUriUtils.ConvertFromUriLiteral(keySegment.Value, ODataVersion.V4);

            var element = await db.Set(prop.PropertyType).FindAsync(key);

            return element;
        }
        #endregion


        //protected async Task BindNavigation(TElement entity) {

        //    StringBuilder sb = new StringBuilder();
        //    byte[] buffer = new byte[8000];
        //    int read = 0;

        //    read = Request.GetOwinContext().Request.Body.Read(buffer, 0, buffer.Length);
        //    while (read > 0)
        //    {
        //        sb.Append(Encoding.UTF8.GetString(buffer));
        //        buffer = new byte[8000];
        //        read = Request.GetOwinContext().Request.Body.Read(buffer, 0, buffer.Length);
        //    }

        //    string requestFromPost = sb.ToString();


        //    //System.IO.StreamReader reader = new System.IO.StreamReader(Request.GetOwinContext().Request.Body);
        //    //reader.BaseStream.Position = 0;
        //    //string requestFromPost = reader.ReadToEnd();


            
        //    //var formData2 = await Request.GetOwinContext().Request.ReadFormAsync() as IEnumerable<KeyValuePair<string, string[]>>;
        //    var a = 22;
        
        //}






        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
