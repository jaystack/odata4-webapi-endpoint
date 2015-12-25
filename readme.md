Full context definition /data model/ available here: .\WebApi_2_2_OData_4\client\JayDataContext.js


Extra features:

	Enums:
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
			
			
	Property with collection of primitive type (not persisted by Entity Framework):
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
			
	
	OpenType (non queryable):
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
			
			
			If you not define your property at client side it JayData use defaults. Default OpenType property is 'Dynamics'.
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
				
			
	Inheritance:
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
			
			
	Action Bound to Entity
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
			
			
	Action bound to Entity Set
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
			
	Controller actions (aka ServiceImport)
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
			
			
	Bi-directional navigation properties (workaround)
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
				
