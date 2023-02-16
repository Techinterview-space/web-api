using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace TestUtils.Fakes;

public class InMemoryConfig
{
    private readonly IConfigurationRoot _config;

    public InMemoryConfig(IDictionary<string, string> dictionary)
    {
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(dictionary)
            .Build();
    }

    public IConfigurationRoot Value() => _config;
}