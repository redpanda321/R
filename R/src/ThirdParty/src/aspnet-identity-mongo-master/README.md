AspNet.Identity.Mongo
=====================

A mongodb provider for the new ASP.NET Identity framework. My aim is to ensure this project is well tested and configurable.

## Usage

	var client = new MongoClient("mongodb://localhost:27017");
	var database = client.GetDatabase("mydb");
	var users = database.GetCollection<IdentityUser>("users");

	var store = new UserStore<IdentityUser>(users);
	var manager = new UserManager<IdentityUser>(store);

	// if you want roles too:
	var roles = database.GetCollection<IdentityRole>("roles");
	var roleStore = new RoleStore<IdentityRole>(roles);

	// at some point in application startup it would be good to ensure unique indexes on user and role names exist:

	IndexChecks.EnsureUniqueIndexOnUserName(users);
	IndexChecks.EnsureUniqueIndexOnEmail(users);

	IndexChecks.EnsureUniqueIndexOnRoleName(roles);

OR

a sample [aspnet-identity-mongo-sample](https://github.com/g0t4/aspnet-identity-mongo-sample) based on [Microsoft ASP.NET Identity Samples](http://www.nuget.org/packages/Microsoft.AspNet.Identity.Samples).

## Installation

via nuget:

	Install-Package AspNet.Identity.MongoDB

## Building and Testing

I'm using the albacore project with rake.

To build:

	rake msbuild
	
To test:

	rake tests
	rake integration_tests

To package:
	
	rake package

## Documentation

I'm writing about my design decisions on my blog:

- [Building a mongodb provider for the new ASP.NET Identity framework - Part 1](http://devblog.wesmcclure.com/posts/building-a-mongodb-provider-for-the-new-asp.net-identity-framework-part-1)
- [Building a mongodb provider for the new ASP.NET Identity framework - Part 2 RoleStore And Sample](http://devblog.wesmcclure.com/posts/building-a-mongodb-provider-for-the-new-asp.net-identity-framework-part-2-rolestore-and-sample)