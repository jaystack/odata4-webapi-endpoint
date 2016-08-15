using Inheritance;
using JayData.Test.CommonItems.Entities;
using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;


namespace Inheritance {
    public class GenericArticle
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        public bool Deleted { get; set; }
    }

    public class InternalArticle : GenericArticle
    {
        public string InternalTitle { get; set; }
        public string InternalBody { get; set; }
        public DateTimeOffset ValidTill { get; set; }
    }

    public class PublicArticle : GenericArticle
    {
        public string Lead { get; set; }
        public DateTimeOffset PublishDate { get; set; }
    }

    public static class Init {
        private static List<GenericArticle> _data = null;

        public static List<GenericArticle> GetData()
        {
            if(_data == null){
                var list = new List<GenericArticle>(){};
                for (var i = 0; i < 10; i++) {
                    if (i % 2 == 0)
                    {
                        list.Add(new InternalArticle() { Id = i, Title = "T" + i, Body = "B" + i, Deleted = i % 3 == 0, InternalBody = "IB" + i, InternalTitle = "IT" + i, ValidTill = DateTime.Now.AddDays(i) });
                    }
                    else {
                        list.Add(new PublicArticle() { Id = i, Title = "T" + i, Body = "B" + i, Deleted = i % 3 == 0, Lead = "L" + i, PublishDate = DateTime.Now.AddDays(i) }); 
                    }
                }

                _data = list;
            }

            return _data;
        }
    }
}


namespace WebApi_2_2_OData_4.Controllers
{



    public class GenericArticlesController : ODataController
    {
        [EnableQuery(MaxExpansionDepth = 10)]
        public IQueryable<GenericArticle> Get()
        {
            return Inheritance.Init.GetData().AsQueryable();
        }

        [EnableQuery]
        public SingleResult<GenericArticle> Get([FromODataUri] int key)
        {
            IQueryable<GenericArticle> result = Inheritance.Init.GetData().AsQueryable().Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

        public IHttpActionResult Post(GenericArticle Article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Inheritance.Init.GetData().Add(Article);
            return Created(Article);
        }

        public IHttpActionResult Patch([FromODataUri] int key, Delta<GenericArticle> Article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = Inheritance.Init.GetData().FirstOrDefault(p => p.Id == key);
            if (entity == null)
            {
                return NotFound();
            }
            Article.Patch(entity);
            return Updated(entity);
        }
        
        
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            var list = Inheritance.Init.GetData();
            var Article = list.FirstOrDefault(p => p.Id == key);
            if (Article == null)
            {
                return NotFound();
            }

            list.Remove(Article);

            return StatusCode(HttpStatusCode.NoContent);
        }



        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
