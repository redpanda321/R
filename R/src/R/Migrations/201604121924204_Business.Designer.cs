// <auto-generated />
namespace R.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode("EntityFramework.Migrations", "6.1.3-40302")]
    public sealed partial class Business : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(Business));
        
        string IMigrationMetadata.Id
        {
            get { return "201604121924204_Business"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return "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"; }
        }
    }
}
