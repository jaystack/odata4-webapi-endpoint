$data.Entity.extend('JayData.Test.CommonItems.Entities.User', {
	Id: { type: 'Edm.Int32', nullable: false, required: true, key: true },
	LoginName: { type: 'Edm.String' },
	Email: { type: 'Edm.String' },
	UserType: { type: 'JayData.Test.CommonItems.Entities.UserType', nullable: false },
	AuthoredArticles: { type: 'Array', elementType: 'JayData.Test.CommonItems.Entities.Article', inverseProperty: "Author" },
	ReviewedArticles: { type: 'Array', elementType: 'JayData.Test.CommonItems.Entities.Article', inverseProperty: "Reviewer" },
	Profile: { type: 'JayData.Test.CommonItems.Entities.UserProfile' }
})

var UserType = $data.createEnum("JayData.Test.CommonItems.Entities.UserType", [
	{ name: 'Admin', value: 0 },
	{ name: 'Customer', value: 1 },
	{ name: 'Guest', value: 2 }
])

var base_MyTClass = $data.Entity.extend('JayData.Test.CommonItems.Entities.MyTClass', {
	Id: { type: 'Edm.Int32', nullable: false, required: true, key: true },
	Title: { type: 'Edm.String' }
}, {
	openType: { value: true }
})

base_MyTClass.extend('JayData.Test.CommonItems.Entities.Article',{
	RowVersion: { type: 'Edm.Binary' },
	Lead: { type: 'Edm.String' },
	Body: { type: 'Edm.String' },
	CreateDate: { type: 'Edm.DateTimeOffset' },
	Thumbnail_LowRes: { type: 'Edm.Binary' },
	Thumbnail_HighRes: { type: 'Edm.Binary' },
	Category: { type: 'JayData.Test.CommonItems.Entities.Category', inverseProperty: "Articles" },
	Tags: { type: 'Array', elementType: 'JayData.Test.CommonItems.Entities.TagConnection', inverseProperty: "Article" },
	Author: { type: 'JayData.Test.CommonItems.Entities.User', inverseProperty: "AuthoredArticles" },
	Reviewer: { type: 'JayData.Test.CommonItems.Entities.User', inverseProperty: "ReviewedArticles" }
})

$data.Entity.extend('JayData.Test.CommonItems.Entities.UserProfile', {
	Id: { type: 'Edm.Int32', nullable: false, required: true, key: true },
	FullName: { type: 'Edm.String' },
	Bio: { type: 'Edm.Binary' },
	Avatar: { type: 'Edm.String' },
	Birthday: { type: 'Edm.DateTimeOffset' },
	Location: { type: 'JayData.Test.CommonItems.Entities.Location' },
	User: { type: 'JayData.Test.CommonItems.Entities.User', required: true, nullable: false, inverseProperty: "Profile" }
})

base_MyTClass.extend('JayData.Test.CommonItems.Entities.Category', {
	RowVersion: { type: 'Edm.Binary' },
	Subtitle: { type: 'Edm.String' },
	Description: { type: 'Edm.String' },
	Articles: { type: 'Array', elementType: 'JayData.Test.CommonItems.Entities.Article', inverseProperty: "Category" }
})

$data.Entity.extend('JayData.Test.CommonItems.Entities.Tag', {
	Id: { type: 'Edm.Int32', nullable: false, required: true, key: true },
	Title: { type: 'Edm.String' },
	Articles: { type: 'Array', elementType: 'JayData.Test.CommonItems.Entities.TagConnection', inverseProperty: "Tag" },
})

$data.Entity.extend('JayData.Test.CommonItems.Entities.TestItem', {
	Id: { type: 'Edm.Int32', nullable: false, required: true, key: true },
	i0: { type: 'Edm.Int32' },
	b0: { type: 'Edm.Boolean' },
	s0: { type: 'Edm.String' },
	blob: { type: 'Array', elementType: 'Edm.Byte' },
	n0: { type: 'Edm.Double' },
	d0: { type: 'Edm.DateTimeOffset' },
	g0: { type: 'Edm.Guid' },
	l0: { type: 'Edm.Int64' },
	de0: { type: 'Edm.Decimal', nullable: false, required: true },
	b1: { type: 'Edm.Byte' }
})

$data.Entity.extend('JayData.Test.CommonItems.Entities.TagConnection', {
	Id: { type: 'Edm.Int32', nullable: false, required: true, key: true },
	Article: { type: 'JayData.Test.CommonItems.Entities.Article', inverseProperty: "Tags" },
	Tag: { type: 'JayData.Test.CommonItems.Entities.Tag', inverseProperty: "Articles" }
})

$data.Entity.extend('JayData.Test.CommonItems.Entities.TestItemGuid', {
	Id: { type: 'Edm.Guid', nullable: false, required: true, key: true },
	i0: { type: 'Edm.Int32' },
	b0: { type: 'Edm.Boolean' },
	s0: { type: 'Edm.String' },
	time: { type: 'Edm.TimeOfDay', nullable: false },
	date: { type: 'Edm.Date', nullable: false },
	t: { type: 'Edm.DateTimeOffset', nullable: false },
	dur: { type: 'Edm.Duration', nullable: false },
	dtOffset: { type: 'Edm.DateTimeOffset', nullable: false },
	lng: { type: 'Edm.Int64', nullable: false },
	dec: { type: 'Edm.Decimal', nullable: false },
	flt: { type: 'Edm.Single', nullable: false },
	emails: { type: 'Array', elementType: 'Edm.String' },
	Group: { type: 'JayData.Test.CommonItems.Entities.TestItemGroup', inverseProperty: "Items" },
	GetDisplayText: { type: $data.ServiceAction, namespace: "Default", returnType: 'Edm.String', params: [] }
}, {
	openType: { value: true }
})

$data.Entity.extend('JayData.Test.CommonItems.Entities.TestItemGroup', {
	Id: { type: 'Edm.Int32', nullable: false, required: true, key: true },
	Name: { type: 'Edm.String' },
	Items: { type: 'Array', elementType: 'JayData.Test.CommonItems.Entities.TestItemGuid', inverseProperty: "Group" },
})

$data.Entity.extend('JayData.Test.CommonItems.Entities.TestItemType', {
	Id: { type: 'Edm.Int32', nullable: false, required: true, key: true },
	blob: { type: 'Edm.Binary' },
	b0: { type: 'Edm.Boolean' },
	b1: { type: 'Edm.Byte' },
	d0: { type: 'Edm.DateTimeOffset' },
	de0: { type: 'Edm.Decimal', nullable: false, required: true },
	n0: { type: 'Edm.Double' },
	si0: { type: 'Edm.Single' },
	g0: { type: 'Edm.Guid' },
	i16: { type: 'Edm.Int16' },
	i0: { type: 'Edm.Int32' },
	i64: { type: 'Edm.Int64' },
	s0: { type: 'Edm.String' }
})

$data.Entity.extend('JayData.Test.CommonItems.Entities.Location', {
	Address: { type: 'Edm.String' },
	City: { type: 'Edm.String' },
	Zip: { type: 'Edm.Int32', nullable: false, required: true },
	Country: { type: 'Edm.String' }
})


var Context = $data.EntityContext.extend('Default.Container', {
	Users: { type: $data.EntitySet, elementType: 'JayData.Test.CommonItems.Entities.User' },
	Articles: { type: $data.EntitySet, elementType: 'JayData.Test.CommonItems.Entities.Article' },
	UserProfiles: { type: $data.EntitySet, elementType: 'JayData.Test.CommonItems.Entities.UserProfile' },
	Categories: { type: $data.EntitySet, elementType: 'JayData.Test.CommonItems.Entities.Category' },
	Tags: { type: $data.EntitySet, elementType: 'JayData.Test.CommonItems.Entities.Tag' },
	TestTable: { type: $data.EntitySet, elementType: 'JayData.Test.CommonItems.Entities.TestItem' },
	TagConnections: { type: $data.EntitySet, elementType: 'JayData.Test.CommonItems.Entities.TagConnection' },
	TestTable2: { type: $data.EntitySet, elementType: 'JayData.Test.CommonItems.Entities.TestItemGuid',
		actions: {
			GetTitles: { type: $data.ServiceAction, namespace: "Default", returnType: '$data.Queryable', elementType: 'Edm.String', params: [{ name: 'count', type: 'Edm.Int32' }] }
		}
	},
	TestItemGroups: { type: $data.EntitySet, elementType: 'JayData.Test.CommonItems.Entities.TestItemGroup' },
	TestItemTypes: { type: $data.EntitySet, elementType: 'JayData.Test.CommonItems.Entities.TestItemType' },
	SAction1: { type: $data.ServiceAction, returnType: 'Edm.String', params: [{ name: 'number', type: 'Edm.Int32' }] },
	SAction2: { type: $data.ServiceAction, returnType: '$data.Queryable', elementType: 'JayData.Test.CommonItems.Entities.Article', EntitySet: 'Articles', params: [{ name: 'count', type: 'Edm.Int32' }] },
	SFunction1: { type: $data.ServiceAction, returnType: 'Edm.String', params: [{ name: 'number', type: 'Edm.Int32' }] }
})

var ctx = new Context('http://localhost:9000/odata')