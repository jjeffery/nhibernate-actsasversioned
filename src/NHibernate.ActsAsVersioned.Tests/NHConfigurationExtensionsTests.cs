using NHibernate.Cfg;
using Xunit;
using Xunit.Abstractions;

namespace NHibernate.ActsAsVersioned
{
    public class NHConfigurationExtensionsTests
    {
        public readonly ITestOutputHelper Output;

        public NHConfigurationExtensionsTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void MappingDocuments()
        {
            var configuration = TestConfiguration.Configuration;

             var mappings = configuration.Mappings();

             foreach (var mapping in mappings)
             {
                 Output.WriteLine(mapping.ToString());
             }
        }
    }
}
