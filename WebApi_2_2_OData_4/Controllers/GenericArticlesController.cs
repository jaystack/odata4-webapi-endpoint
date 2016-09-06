using Inheritance;
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


namespace Inheritance
{
    public class GenericArticle
    {
        public GenericArticle()
        {
            this.Authors = new List<User>();
        }

        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        public bool Deleted { get; set; }

        public User CreatedBy { get; set; }

        public IList<User> Authors { get; set; }
    }

    public class User
    {
        public User()
        {
            this.PublishedArticles = new List<PublicArticle>();
            this.CreatedArticles = new List<GenericArticle>();
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public IList<PublicArticle> PublishedArticles { get; set; }
        public IList<GenericArticle> CreatedArticles { get; set; }
        public PublicArticle RelatedPublicArticle { get; set; }
        public GenericArticle RelatedArticle { get; set; }
    }

    public class InternalArticle : GenericArticle
    {
        public string InternalTitle { get; set; }
        public string InternalBody { get; set; }
        public DateTimeOffset ValidTill { get; set; }
    }

    public class PublicArticle : GenericArticle
    {
        public PublicArticle()
        {
            this.RelatedAuthors = new List<User>();
        }

        public string Lead { get; set; }
        public DateTimeOffset PublishDate { get; set; }

        public User PublishedBy { get; set; }
        public IList<User> RelatedAuthors { get; set; }
    }

    public static class Init
    {
        private static List<GenericArticle> _articles = null;
        private static List<User> _users = null;

        public static List<GenericArticle> GenericArticles
        {
            get
            {
                if (_articles == null)
                {
                    BuildData();
                }
                return _articles;
            }
        }

        public static List<User> Users
        {
            get
            {
                if (_users == null)
                {
                    BuildData();
                }
                return _users;
            }
        }

        private static void BuildData()
        {
            var articles = GetArticleData();
            var users = GetUserData();
            Random r = new Random(DateTime.Now.Millisecond);

            foreach (GenericArticle a in articles)
            {
                var selectedUser = users[r.Next(users.Count)];
                a.CreatedBy = selectedUser;
                selectedUser.CreatedArticles.Add(a);

                if (a is PublicArticle) {
                    var pa = a as PublicArticle;
                    selectedUser = users[r.Next(users.Count)];
                    pa.PublishedBy = selectedUser;
                    selectedUser.PublishedArticles.Add(pa);
                }
            }

            var publicArticles = articles.Where(a => a is PublicArticle).ToList();
            foreach (User u in users)
            {
                var selectedArticle = articles[r.Next(articles.Count)];
                u.RelatedArticle = selectedArticle;
                selectedArticle.Authors.Add(u);

                
                var selectedPublicArticle = publicArticles[r.Next(publicArticles.Count)];
                var pa = selectedPublicArticle as PublicArticle;
                u.RelatedPublicArticle = pa;
                pa.RelatedAuthors.Add(u);
            }

            _articles = articles;
            _users = users;
        }

        private static List<User> GetUserData()
        {
            var list = new List<User>() { };
            for (var i = 0; i < 20; i++)
            {
                list.Add(new User() { Id = i, Name = "InheritanceUser" + i, Email = "user" + i + "@example.com" });
            }
            return list;
        }

        private static List<GenericArticle> GetArticleData()
        {
            var list = new List<GenericArticle>() { };
            for (var i = 0; i < 10; i++)
            {
                if (i % 2 == 0)
                {
                    list.Add(new InternalArticle() { Id = i, Title = "T" + i, Body = "B" + i, Deleted = i % 3 == 0, InternalBody = "IB" + i, InternalTitle = "IT" + i, ValidTill = DateTime.Now.AddDays(i) });
                }
                else
                {
                    list.Add(new PublicArticle() { Id = i, Title = "T" + i, Body = "B" + i, Deleted = i % 3 == 0, Lead = "L" + i, PublishDate = DateTime.Now.AddDays(i) });
                }
            }
            return list;
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
            return Inheritance.Init.GenericArticles.AsQueryable();
        }

        [EnableQuery]
        public SingleResult<GenericArticle> Get([FromODataUri] int key)
        {
            IQueryable<GenericArticle> result = Inheritance.Init.GenericArticles.AsQueryable().Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

        public IHttpActionResult Post(GenericArticle Article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Inheritance.Init.GenericArticles.Add(Article);
            return Created(Article);
        }

        public IHttpActionResult Patch([FromODataUri] int key, Delta<GenericArticle> Article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = Inheritance.Init.GenericArticles.FirstOrDefault(p => p.Id == key);
            if (entity == null)
            {
                return NotFound();
            }
            Article.Patch(entity);
            return Updated(entity);
        }


        public IHttpActionResult Delete([FromODataUri] int key)
        {
            var list = Inheritance.Init.GenericArticles;
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

    public class InheritanceUsersController : ODataController
    {
        [EnableQuery(MaxExpansionDepth = 10)]
        public IQueryable<User> Get()
        {
            return Inheritance.Init.Users.AsQueryable();
        }

        [EnableQuery]
        public SingleResult<User> Get([FromODataUri] int key)
        {
            IQueryable<User> result = Inheritance.Init.Users.AsQueryable().Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

        public IHttpActionResult Post(User User)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Inheritance.Init.Users.Add(User);
            return Created(User);
        }

        public IHttpActionResult Patch([FromODataUri] int key, Delta<User> User)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = Inheritance.Init.Users.FirstOrDefault(p => p.Id == key);
            if (entity == null)
            {
                return NotFound();
            }
            User.Patch(entity);
            return Updated(entity);
        }


        public IHttpActionResult Delete([FromODataUri] int key)
        {
            var list = Inheritance.Init.Users;
            var User = list.FirstOrDefault(p => p.Id == key);
            if (User == null)
            {
                return NotFound();
            }

            list.Remove(User);

            return StatusCode(HttpStatusCode.NoContent);
        }



        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
