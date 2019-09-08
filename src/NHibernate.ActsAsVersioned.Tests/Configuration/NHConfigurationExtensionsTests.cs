using System;
using System.Collections.Generic;
using System.Text;
using NHibernate.Cfg;
using Xunit;
using Xunit.Abstractions;

namespace NHibernate.ActsAsVersioned.Configuration
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
