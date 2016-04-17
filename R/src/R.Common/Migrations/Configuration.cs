using System.Data.Entity.Migrations;
using R.Models;

namespace R.Migrations
{
	public class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
	{
        public Configuration() {


            AutomaticMigrationsEnabled = true;
        }
	}
}
