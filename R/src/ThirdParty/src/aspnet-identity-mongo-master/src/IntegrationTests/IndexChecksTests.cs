﻿namespace IntegrationTests
{
	using System.Linq;
	using AspNet.Identity.MongoDB;
	using NUnit.Framework;

	[TestFixture]
	public class IndexChecksTests : UserIntegrationTestsBase
	{
		[Test]
		public void EnsureUniqueIndexOnUserName_NoIndexOnUserName_AddsUniqueIndexOnUserName()
		{
			var userCollectionName = "userindextest";
			Database.DropCollection(userCollectionName);
			var usersNewApi = DatabaseNewApi.GetCollection<IdentityUser>(userCollectionName);

			IndexChecks.EnsureUniqueIndexOnUserName(usersNewApi);

			var users = Database.GetCollection(userCollectionName);
			var index = users.GetIndexes()
				.Where(i => i.IsUnique)
				.Where(i => i.Key.Count() == 1)
				.First(i => i.Key.Contains("UserName"));
			Expect(index.Key.Count(), Is.EqualTo(1));
		}

		[Test]
		public void EnsureEmailUniqueIndex_NoIndexOnEmail_AddsUniqueIndexOnEmail()
		{
			var userCollectionName = "userindextest";
			Database.DropCollection(userCollectionName);
			var usersNewApi = DatabaseNewApi.GetCollection<IdentityUser>(userCollectionName);

			IndexChecks.EnsureUniqueIndexOnEmail(usersNewApi);

			var users = Database.GetCollection(userCollectionName);
			var index = users.GetIndexes()
				.Where(i => i.IsUnique)
				.Where(i => i.Key.Count() == 1)
				.First(i => i.Key.Contains("Email"));
			Expect(index.Key.Count(), Is.EqualTo(1));
		}

		[Test]
		public void EnsureUniqueIndexOnRoleName_NoIndexOnRoleName_AddsUniqueIndexOnRoleName()
		{
			var roleCollectionName = "roleindextest";
			Database.DropCollection(roleCollectionName);
			var rolesNewApi = DatabaseNewApi.GetCollection<IdentityRole>(roleCollectionName);

			IndexChecks.EnsureUniqueIndexOnRoleName(rolesNewApi);

			var roles = Database.GetCollection(roleCollectionName);
			var index = roles.GetIndexes()
				.Where(i => i.IsUnique)
				.Where(i => i.Key.Count() == 1)
				.First(i => i.Key.Contains("Name"));
			Expect(index.Key.Count(), Is.EqualTo(1));
		}
	}
}