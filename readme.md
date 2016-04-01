##You can access our live OData v4 sandbox endpoint here:
    http://odatav4-demo.jaystack.com:9000/odata

##Generate your own context definition with jaysvcutil on nodejs
    npm install -g jaysvcutil
    jaysvcutil -m http://odatav4-demo.jaystack.com:9000/odata/$metadata -o context.js


#Features:

##Using custom data annotation
    - Define custom annotations - .NET server-side (Startup.cs)
        - Add annotation to entity properties
            var m = model as EdmModel;
            m.SetVocabularyAnnotation(
                new EdmAnnotation(model.EntityContainer.FindEntitySet("Categories").EntityType().FindProperty("Title"),
                    new EdmTerm("UI", "DisplayName", EdmPrimitiveTypeKind.String),
                    new EdmStringConstant("Category name")));
        - Add annotation to an entity
            m.SetVocabularyAnnotation(
                new EdmAnnotation(model.EntityContainer.FindEntitySet("Categories").EntityType(),
                    new EdmTerm("UI", "DisplayName", EdmPrimitiveTypeKind.String),
                    new EdmStringConstant("Categories")));

    - Generate dynamic data model with JayData (client-side) - using $data.initService()
        $data.initService('http://odatav4-demo.jaystack.com:9000/odata', function(ctx){
            ...
        })    
    
    - Retrieve annotations with JayData
        - Retrieve entity property annotation
            ctx.Categories.elementType.getMetadata('UI.DisplayName', 'Title')
            
        - Retrieve entity annotation
            ctx.Categories.elementType.getMetadata('UI.DisplayName')

    - Avaliable metadata functions on JayData types
        The property parameter is optional and can be used to retrieve metadata of a particular property. You can retrieve entity annonations without passing the property parameter.
    
        - type.hasMetadata(metadataKey [, property])
            returns: true / false
            
        - type.getMetadata(metadataKey [, property])
            returns: metadata value
        
        - type.getAllMetadata([property])
            returns: object hash with metadata values
            
        - type.setMetadata(metadataKey, metadataValue [, property])


##Arrow function support
    - JayData has improved predicates in Queryables. You can use arrow functions for example in filter conditions.
        - Old style
            ctx.Articles.filter(function (it) { return it.Id > 5 })
        - New style
            ctx.Articles.filter(it => it.Id > 5)
            
            If arrow function isn't supported by your targeted browsers (older IE) then you can use it in string
                ctx.Articles.filter('it => it.Id > 5')

##Advanced retrieval of related entites through $expand
    - You can use additional queryable methods on collections to filter/project navigation properties with arrays
        - Filtering array-valued navigation properties
            ctx.Categories.include('c => c.Articles.filter(a => a.Id > 5)')
            
        - Projecting array-valued navigation properties
            ctx.Categories.include('c => c.Articles.map(a => a.Title)')
            
        - Projecting and filtering array-valued navigation properties
            ctx.Categories.include('c => c.Articles.filter(a => a.Id > 5).map(a => a.Title)')
            
        - Expanding navigation properties of navigation properties
            - Basic retrieval: 
                ctx.Categories
                    .include('c => c.Articles.Tags')
                    
            - Retrieval of multiple navigation properties at the same level
                ctx.Categories
                    .include('c => c.Articles.Tags')
                    .include('c => c.Articles.Reviewer')
            
            - Expanding pre-filtered navigation properties - result: you will get a list of categories that has a prefiltered article list 
              that match the filter criteria; all article record will be loaded with Tags and Reviewer properties and its values
                
                ctx.Categories
                    .include('c => c.Articles.filter(a => a.Id > 5)')
                    .include('c => c.Articles.Tags')
                    .include('c => c.Articles.Reviewer')
                        
        - Available nested queryable methods:
            - filter
            - map
            - orderBy
            - orderByDescending
            - take
            - skip

##Deep insert
    - You can enable deep insert behavior through the following global configuration property in JayData
            $data.defaults.OData.enableDeepSave
        If true (default: false) then the provider will discover the deeply insertable entities to reduce the created request numbers
        
            var category = new ctx.Categories.elementType({ Title: 'Javascript' })
            var deepInsertArticle = new ctx.Articles.elementType({ Title: 'Deep insert', Lead: 'Lead', Body: 'body', Category: category })
            ctx.Articles.add(deepInsertArticle)
            ctx.saveChanges()  
            
        The result will be only one POST request.
        Technical depth: computed fields (such as auto-incremented IDs) of deep-inserted entities aren't returned by current version of ASP.NET WebAPI OData
        
##Function parameter resolution (not enabled by default yet)
    - There is an other usage of parameters in Queryable predicates
        - Old style
            ctx.Articles.filter(function(it){ return it.Title == this.p1 && it.Lead == this.p2 }, {p1: 'Title', p2: 'Lead'})
        - New style
            ctx.Articles.filter(function(it, p1, p2){ return it.Title == p1 && it.Lead == p2 }, {p1: 'Title', p2: 'Lead'})
            
            You can use function parameters in arrow functions, so it is equivalent with:
                ctx.Articles.filter((it, p1, p2) => it.Title == p1 && it.Lead == p2 }, {p1: 'Title', p2: 'Lead'})
            
            To enable this feature with the following global setting: $data.defaults.parameterResolutionCompatibility = false; (default: true) 

##Managing entity relationships
    - Creating a relationship between entities (http://www.asp.net/web-api/overview/odata-support-in-aspnet-web-api/odata-v4/entity-relations-in-odata-v4)
        - Create an action in your ODataController that sets references  (.NET server-side)
        
            [AcceptVerbs("POST", "PUT")]
            public async Task<IHttpActionResult> CreateRef([FromODataUri] int key, 
                string navigationProperty, [FromBody] Uri link)
            {
                var product = await db.Products.SingleOrDefaultAsync(p => p.Id == key);
                if (product == null)
                {
                    return NotFound();
                }
                switch (navigationProperty)
                {
                    case "Supplier":
                        // Note: The code for GetKeyFromUri is shown later in this topic.
                        var relatedKey = Helpers.GetKeyFromUri<int>(Request, link);
                        var supplier = await db.Suppliers.SingleOrDefaultAsync(f => f.Id == relatedKey);
                        if (supplier == null)
                        {
                            return NotFound();
                        }

                        product.Supplier = supplier;
                        break;

                    default:
                        return StatusCode(HttpStatusCode.NotImplemented);
                }
                await db.SaveChangesAsync();
                return StatusCode(HttpStatusCode.NoContent);
            }
            
        - How to set entity references in JayData
            There is a global configuration property
                $data.defaults.OData.withReferenceMethods
            If true (true by default) then OData provider provider will create an additional POST request what handled by this CreateRef action to set navigation property while calling saveChanges() method
            
                var product = products[0];
                ctx.Products.attach(product);
                product.Supplier = suppliers[0];
                ctx.saveChanges();
                
    - Removing a relationship between entities
        - Create an action in your ODataController for handle clear reference requests (.NET server-side)
        
            public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key, 
                string navigationProperty, [FromBody] Uri link)
            {
                var product = db.Products.SingleOrDefault(p => p.Id == key);
                if (product == null)
                {
                    return NotFound();
                }

                switch (navigationProperty)
                {
                    case "Supplier":
                        product.Supplier = null;
                        break;

                    default:
                        return StatusCode(HttpStatusCode.NotImplemented);
                }
                await db.SaveChangesAsync();

                return StatusCode(HttpStatusCode.NoContent);
            }
            
        - How to remove relationships between entities in JayData
            There is a configuration property
                $data.defaults.OData.withReferenceMethods
            If true (default: true) then the provider will create an additional DELETE request what handled by this DeleteRef action to clear navigation property while calling saveChanges() method
            
                var product = products[0];
                ctx.Products.attach(product);
                product.Supplier = null;
                ctx.saveChanges();
    
    - Generic server-side implementation (.NET):
        - We created a generic base controller with some reflection for handle navigation properties on entity types. You can check it here:
            https://github.com/jaystack/odata4-webapi-endpoint/blob/development/WebApi_2_2_OData_4/Controllers/NewsBaseODataController.cs#L29
    
##Enums:
    - Define custom enum type - .NET server-side
        
        public enum UserType
        {
            Admin = 0,
            Customer = 1,
            Guest = 2
        }
        
        public partial class User
        {
            ...
            public UserType UserType { get; set; }
            ...
        }
        
        
    - Publish custom enum type through OData $metadata service - .NET server-side
        
        var client = new ODataConventionModelBuilder();
        client.AddEnumType(typeof(UserType));
        
        
    - Define custom enum type in JayData data model - JavaScript 
        
        var UserType = $data.createEnum("JayData.Test.CommonItems.Entities.UserType", [
            { name: 'Admin', value: 0 },
            { name: 'Customer', value: 1 },
            { name: 'Guest', value: 2 }
        ])
        
        $data.Entity.extend('JayData.Test.CommonItems.Entities.User', {
            ...
            UserType: { type: 'JayData.Test.CommonItems.Entities.UserType', nullable: false },
            ...
        })
        
        
    - Use custom enum type in JayData - JavaScript
        
        user.UserType = UserType.Admin
        
        
##Property with collection of primitive type (not persisted by Entity Framework):
    - Define a collection of primitives .NET server-side
        
        public class TestItemGuid
        {
            ...
            public List<string> emails { get; set; }
            ...
        }
        
    
    - Provide service method that retrieves a collection of primitive .NET server-side
        
        public class TestTable2Controller : ODataController
        {
            NewsReaderContext db = new NewsReaderContext();
    
            [EnableQuery]
            public IQueryable<TestItemGuid> Get()
            {
                var list = new List<TestItemGuid>();
    
                ...
                
                var item = new TestItemGuid { Id = Guid.NewGuid() };
                
                ...
    
                item.emails.Add("a@example.com");
                item.emails.Add("b@example.com");
                item.emails.Add("c@example.com");
    
                list.Add(item);
    
                return list.AsQueryable();
            }	
        }
        
    
    - Define JayData data model with collection of primitives - JavaScript  
        
        $data.Entity.extend('JayData.Test.CommonItems.Entities.TestItemGuid', {
            ...
            emails: { type: 'Array', elementType: 'Edm.String' }
            ...
        })
        
        
    - Reference collection of primitives in JayData query results - JavaScript 
        
        ctx.TestTable2.toArray(function(items) {
            console.log(items[0].emails)
        })
        

##OpenType (non queryable):
    - Define custom OData v4 open type - .NET server-side
        
        public class TestItemGuid
        {
            public TestItemGuid()
            {
                openProperties = new Dictionary<string, object>();
            }
            ...
            public IDictionary<string, object> openProperties { get; set; }
            ...
        }
        
        
    - Publishing custom OData v4 open type throgh OData $metadata service - .NET server-side
        
        // var et = client.EntityType<TestItemGuid>();
        // et.HasDynamicProperties(x => x.openProperties);
        
        not needed, because conventional builder know from property type
    
    - Define custom open type in JayData - JavaScript
        
        $data.Entity.extend('JayData.Test.CommonItems.Entities.TestItemGuid', {
            ...
        }, {
            openType: { value: true }
        })
        
        
        If you not define your property at client side it JayData use defaults. Default OpenType property is 'dynamicProperties'.
        Or you can define your own property
        
        
        $data.Entity.extend('JayData.Test.CommonItems.Entities.TestItemGuid', {
            ...
            openProperties: { type: $data.Object }
            ...
        }, {
            openType: { value: 'openProperties' }
        })
        
        
    - Setting fields of open types
        - .NET server-side
            
            var item1 = new TestItemGuid { Id = Guid.NewGuid() };
            item1.openProperties.Add("t0", 1.0f);
            item1.openProperties.Add("t1", false);
            item1.openProperties.Add("t2", DateTime.Now);
            
        
        - JavaScript - using JayData
            
            item.openProperties.t1 = true
            
        
##Inheritance:
    - Define inherited type .NET server-side
        
        public abstract class MyTClass
        {
            [Key]
            public int Id { get; set; }
            public string Title { get; set; }
        }
    
        public partial class Category : MyTClass
        {
            ...
        }
        
        
    - Publish inherited types through OData $metadata service - .NET server-side
        
        var c = client.EntitySet<Category>("Categories");
        var cc = client.EntityType<Category>();
        cc.DerivesFrom<MyTClass>();
        
        
    - Definition of inherited class using JayData type system - JavaScript
        
        var base_MyTClass = $data.Entity.extend('JayData.Test.CommonItems.Entities.MyTClass', {
            Id: { type: 'Edm.Int32', nullable: false, required: true, key: true },
            Title: { type: 'Edm.String' }
        })
        
        base_MyTClass.extend('JayData.Test.CommonItems.Entities.Category', {
            ...
        })
        
        
    - Set inherited field of JayData entity - JavaScript
        
        category.Title = 'Sport'
        
        
##Action Bound to Entity
    - Define OData v4 action - .NET server-side
        
        [HttpPost]
        [Route("GetDisplayText")]
        public string GetDisplayText([FromODataUri] Guid key)
        {
            var item = db.TestTable2.Find(key)
            return string.Format("{0} - {1}", item.Id, "item");
        }
        
        
    - Publish OData v4 entity action through OData $metadata service - .NET server-side
        
        var entityAction = client.EntityType<TestItemGuid>().Action("GetDisplayText");
        entityAction.ReturnsCollection<string>();
        

    - Define entity action in JayData data model /context definition/ - JavaScript
        
        $data.Entity.extend('JayData.Test.CommonItems.Entities.TestItemGuid', {
            ...
            GetDisplayText: { type: $data.ServiceAction, namespace: "Default", returnType: 'Edm.String', params: [] }
        })
        
        var Context = $data.EntityContext.extend('Default.Container', {
            ...
        })
        
        
        You have to define 'namespace' member. The value of the namespace property is your ODataConventionModelBuilder namespace.
        By default it's value is "Default"
        
    - Consume OData v4 actions with JayData - JavaScript
        
        entity.GetDisplayText(function (displayText) { console.log(displayText) })
        
        
##Action bound to Entity Set
    - Define collection-level action .NET server-side
        
        [HttpPost]
        [Route("GetTitles")]
        public IQueryable<string> GetTitles(ODataActionParameters param)
        {
            int count = (int)param["count"];

            return db.TestTable2.Take(count).Select(s => s.s0);
        }
        
        
    - Publish action bound to entity set through $metadata service - .NET server-side
        
        var collectionAction = client.EntityType<TestItemGuid>().Collection.Action("GetTitles");
        collectionAction.Parameter<int>("count");
        collectionAction.Returns<string>();
        
    
    - Define action bound to entity set in JayData data model (context definiton) - JavaScript
        
        var Context = $data.EntityContext.extend('Default.Container', {
            ...
            TestTable2: { type: $data.EntitySet, elementType: 'JayData.Test.CommonItems.Entities.TestItemGuid',
                actions: {
                    GetTitles: { type: $data.ServiceAction, namespace: "Default", returnType: '$data.Queryable', elementType: 'Edm.String', params: [{ name: 'count', type: 'Edm.Int32' }] }
                }
            }
            ...
        })
        
        
        You have to define 'namespace' member. The value of the namespace property is your ODataConventionModelBuilder namespace.
        By default it's value is "Default"
        
    - Call GetTitles() action in JayData API - JavaScript 
        context.TestTable2.GetTitles(2).toArray(function (titles) { console.log(titles) })
        
##Controller actions (aka ServiceImport)
    - Define action at controller-level - .NET server-side
        
        [HttpPost]
        [Route("SAction2")]
        [EnableQuery]
        public IQueryable<Article> SAction2(ODataActionParameters param)
        {
            var count = (int)param["count"];
            return db.Articles.Take(count);
        }
        
        
    - Publishing conroller-level actions through OData $metadata service - .NET server-side
        You need to write some code for nonbinded actions to handle these requests.
        
        - 1) Create Controller for service actions (nonbindable actions)
            
            [RoutePrefix("odata")]
            public class NonBindableActionsController : ODataController
            {
                NewsReaderContext db = new NewsReaderContext();
        
                ...
                    
                [HttpPost]
                [Route("SAction2")]
                [EnableQuery]
                public IQueryable<Article> SAction2(ODataActionParameters param)
                {
                    var count = (int)param["count"];
                    return db.Articles.Take(count);
                }
                
                ...
            }
            
        
        - 2) Create Routing convention - .NET server-side
            
            public class ContainmentRoutingConvention : IODataRoutingConvention
            {
        
                public string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
                {
                    if (odataPath.PathTemplate == "~/unboundaction")
                    {
                        UnboundActionPathSegment actionSegment = odataPath.Segments.First() as UnboundActionPathSegment;
                        if (actionSegment != null)
                        {
                            IEdmActionImport action = actionSegment.Action;
        
                            if (actionMap.Contains(action.Name))
                            {
                                return action.Name;
                            }
                        }
                    }
                    else if (odataPath.PathTemplate == "~/unboundfunction")
                    {
                        UnboundFunctionPathSegment functionSegment = odataPath.Segments.First() as UnboundFunctionPathSegment;
                        if (functionSegment != null)
                        {
                            IEdmFunctionImport function = functionSegment.Function;
        
                            if (actionMap.Contains(function.Name))
                            {
                                return function.Name;
                            }
                        }
                    }
        
                    return null;
                }
        
                public string SelectController(ODataPath odataPath, HttpRequestMessage request)
                {
                    if (odataPath.PathTemplate == "~/unboundaction" || odataPath.PathTemplate == "~/unboundfunction")
                    {
                        return "NonBindableActions";
                    }
        
                    return null;
                }
            }
            
        
        - 3) Register your routing conventions in metadata - .NET server-side
            
            var client = new ODataConventionModelBuilder();
            ...
            var model = client.GetEdmModel();
            ...
            IList<IODataRoutingConvention> conventions = ODataRoutingConventions.CreateDefaultWithAttributeRouting(config, model);
            conventions.Insert(0, new ContainmentRoutingConvention());
            ...
            config.MapODataServiceRoute("odata", "odata", model, new DefaultODataPathHandler(), conventions);
            appBuilder.UseWebApi(config);
            
            
    - Define actions on controller-level in JayData context definition / data model - JavaScript
        
        var Context = $data.EntityContext.extend('Default.Container', {
            ...
            SAction2: { type: $data.ServiceAction, returnType: '$data.Queryable', elementType: 'JayData.Test.CommonItems.Entities.Article', EntitySet: 'Articles', params: [{ name: 'count', type: 'Edm.Int32' }] },
            ...
        })
        
        
        You don't have to define 'namespace' member right now, because it isn't supported in this scenario.
        
    - Using JayData API to invoke SAction2() action defined at controller-level - JavaScript
        
        context.SAction2(2).toArray(function (articles) { console.log(articles) })
        
        
##Bi-directional navigation properties (workaround)
    - How to publish 'Partner' attribute as navigation property in metadata
        - We have entities with relations
            
            public partial class Article : MyTClass
            {
                ...
                public virtual Category Category { get; set; }
                ...
            }
            
            public partial class Category : MyTClass
            {
                ...
                public virtual IList<Article> Articles { get; set; }
            }
            
    
        - 1) Define a function, which creates the new association between entities (we will use this after conventional model builder)
            
            public static void UpgradeBidirectionalNavigationProperties(Microsoft.OData.Edm.IEdmModel model, string setName, string partnerSetName, string elementTypeName, string partnerElementTypeName, string navigationName, string partnerName, EdmMultiplicity multipl, EdmMultiplicity partnerMultipl)
            {
                var fromSet = (EdmEntitySet)model.EntityContainer.FindEntitySet(setName);
                var partnerSet = (EdmEntitySet)model.EntityContainer.FindEntitySet(partnerSetName);
    
                var fromType = (EdmEntityType)model.FindDeclaredType(elementTypeName);
                var partnerType = (EdmEntityType)model.FindDeclaredType(partnerElementTypeName);
    
                if (fromType == null)
                    throw new Exception("fromType is null");
    
                if (partnerType == null)
                    throw new Exception("partnerType is null");
    
                var partsProperty = new EdmNavigationPropertyInfo();
                partsProperty.Name = navigationName;
                partsProperty.TargetMultiplicity = multipl;
                partsProperty.Target = partnerType;
                partsProperty.ContainsTarget = false;
                partsProperty.OnDelete = EdmOnDeleteAction.None;
    
                var partnerProperty = new EdmNavigationPropertyInfo();
                partnerProperty.Name = partnerName;
                partnerProperty.TargetMultiplicity = partnerMultipl;
                partnerProperty.Target = fromType;
                partnerProperty.ContainsTarget = false;
                partnerProperty.OnDelete = EdmOnDeleteAction.None;
    
                fromSet.AddNavigationTarget(fromType.AddBidirectionalNavigation(partsProperty, partnerProperty), partnerSet);
            }
            
            
        - 2) Ignore the navigation properties from convention builder
            
            client.EntityType<Article>().Ignore(a => a.Category);
            client.EntityType<Category>().Ignore(a => a.Articles);
            
            
        - 3) Create new navigations in model
            
            var model = client.GetEdmModel();
            UpgradeBidirectionalNavigationProperties(model, "Articles", "Categories", "JayData.Test.CommonItems.Entities.Article", "JayData.Test.CommonItems.Entities.Category", "Category", "Articles", EdmMultiplicity.ZeroOrOne, EdmMultiplicity.Many);
            
            
    - Comparison of uni-directional and bi-directional navigation properties in $metadata service
        - Original (uni-directional)
            
            <EntityType Name="Article" BaseType="JayData.Test.CommonItems.Entities.MyTClass">
                ...
                <NavigationProperty Name="Category" Type="JayData.Test.CommonItems.Entities.Category" />
                ...
            </EntityType>
            
        - Updated (bi-directional)
            
            <EntityType Name="Article" BaseType="JayData.Test.CommonItems.Entities.MyTClass">
                ...
                <NavigationProperty Name="Category" Type="JayData.Test.CommonItems.Entities.Category" Partner="Articles" />
                ...
            </EntityType>
            
