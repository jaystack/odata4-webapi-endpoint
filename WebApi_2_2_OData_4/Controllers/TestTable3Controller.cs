using JayData.Test.CommonItems.Entities;
using JayData.Test.WebApi_2_2_OData_4.Model;
using Microsoft.OData.Edm.Library;
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

namespace WebApi_2_2_OData_4.Controllers
{
    public class TestTable3Controller : NewsBaseODataController<TestItem3>
    {
        public static List<TestItem3> items = new List<TestItem3>();
        static TestTable3Controller()
        {
            Reset();
        }
        public static void Reset()
        {
            items = new List<TestItem3>();
            items.Add(new TestItem3
            {
                Id = Guid.NewGuid(),
                b0 = true,
                i0 = 1234,
                s0 = "1st row",
            });
            var item1 = new TestItem3 { Id = Guid.NewGuid(), b0 = false, i0 = 6234, s0 = "2nd row" };
            item1.Locations = new List<Location>()
            {
                new Location { City = "City 1", Zip = 1117, Country = "Country 1", Address = "Test data1" },
                new Location { City = "City 2", Zip = 1117, Country = "Country 2", Address = "Test data2" },
                new Location { City = "City 3", Zip = 1117, Country = "Country 3", Address = "Test data3" },
                new Location { City = "City 4", Zip = 1117, Country = "Country 4", Address = "Test data4" },
            };

            items.Add(item1);
            var item = new TestItem3 { Id = Guid.NewGuid()/*, Entrance = GeographyPoint.Create(64.1, 142.1)*/, b0 = null, i0 = 62341, s0 = "3rd row" };
            //item.openProperties.Add("t5", UserType.Customer);


            item.Locations = new List<Location>()
            {
                new Location { City = "xxxCity 1", Zip = 1117, Country = "Country 1", Address = "Test data1" },
                new Location { City = "xxxCity 2", Zip = 1117, Country = "Country 2", Address = "Test data2" },
                new Location { City = "xxxCity 3", Zip = 1117, Country = "Country 3", Address = "Test data3" },
                new Location { City = "xxxCity 4", Zip = 1117, Country = "Country 4", Address = "Test data4" },
            };

            items.Add(item);
        }

        [EnableQuery]
        public IQueryable<TestItem3> Get()
        {
            return items.AsQueryable();
        }

        [EnableQuery]
        public SingleResult<TestItem3> Get([FromODataUri] Guid key)
        {
            IQueryable<TestItem3> result = items.Where(p => p.Id == key).AsQueryable();
            return SingleResult.Create(result);
        }

        public async Task<IHttpActionResult> Post(TestItem3 TestGuid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            items.Add(TestGuid);
            //await db.SaveChangesAsync();
            return Created(TestGuid);
        }

        public IHttpActionResult Patch([FromODataUri] Guid key, Delta<TestItem3> TestGuid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = items.Find(i => i.Id == key);
            if (entity == null)
            {
                return NotFound();
            }
            TestGuid.Patch(entity);
            try
            {
                //await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(entity);
        }

        public IHttpActionResult Put([FromODataUri] Guid key, TestItem3 update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != update.Id)
            {
                return BadRequest();
            }
            //db.Entry(update).State = EntityState.Modified;
            try
            {
                //await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(update);
        }
        
        public IHttpActionResult Delete([FromODataUri] Guid key)
        {
            var TestGuid = items.Find(i => i.Id == key);
            if (TestGuid == null)
            {
                return NotFound();
            }
            items.Remove(TestGuid);
            //await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private bool ProductExists(Guid key)
        {
            return items.Any(p => p.Id == key);
        }

        [HttpPost]
        [Route("Clear")]
        public int Clear(ODataActionParameters param)
        {
            Reset();
            return items.Count();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
