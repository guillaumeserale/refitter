using FluentAssertions;
using Refitter.Core;
using Refitter.Tests.Build;
using Xunit;

namespace Refitter.Tests.Examples;

public class MultipleInterfacesByEndpointTests
{
    private const string OpenApiSpec = @"
openapi: '3.0.0'
paths:
  /foo/{id}:
    get:
      tags:
      - 'Foo'
      operationId: 'Get foo details'
      description: 'Get the details of the specified foo'
      parameters:
        - in: 'path'
          name: 'id'
          description: 'Foo ID'
          required: true
          schema:
            type: 'string'
      responses:
        '200':
          description: 'successful operation'
  /foo:
    get:
      tags:
      - 'Foo'
      operationId: 'Get all foos'
      description: 'Get all foos'      
      responses:
        '200':
          description: 'successful operation'
  /bar:
    get:
      tags:
      - 'Bar'
      operationId: 'Get all bars'
      description: 'Get all bars'      
      responses:
        '200':
          description: 'successful operation'
  /bar/{id}:
    get:
      tags:
      - 'Bar'
      operationId: 'Get bar details'
      description: 'Get the details of the specified bar'   
      responses:
        '200':
          description: 'successful operation'
";

    [Fact]
    public async Task Can_Generate_Code()
    {
        string generateCode = await GenerateCode();
        generateCode.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Generates_IGetAllFooEndpoint()
    {
        string generateCode = await GenerateCode();
        generateCode.Should().Contain("interface IGetAllFoosEndpoint");
    }

    [Fact]
    public async Task Generates_IGetFooDetailsEndpoint()
    {
        string generateCode = await GenerateCode();
        generateCode.Should().Contain("interface IGetFooDetailsEndpoint");
    }

    [Fact]
    public async Task Generates_IGetAllBarEndpoint()
    {
        string generateCode = await GenerateCode();
        generateCode.Should().Contain("interface IGetAllBarsEndpoint");
    }

    [Fact]
    public async Task Generates_IGetBarDetailsEndpoint()
    {
        string generateCode = await GenerateCode();
        generateCode.Should().Contain("interface IGetBarDetailsEndpoint");
    }

    [Fact]
    public async Task Can_Build_Generated_Code()
    {
        string generateCode = await GenerateCode();
        BuildHelper
            .BuildCSharp(generateCode)
            .Should()
            .BeTrue();
    }

    private static async Task<string> GenerateCode()
    {
        var swaggerFile = await CreateSwaggerFile(OpenApiSpec);
        var settings = new RefitGeneratorSettings
        {
            OpenApiPath = swaggerFile,
            MultipleInterfaces = MultipleInterfaces.ByEndpoint
        };

        var sut = await RefitGenerator.CreateAsync(settings);
        var generateCode = sut.Generate();
        return generateCode;
    }

    private static async Task<string> CreateSwaggerFile(string contents)
    {
        var filename = $"{Guid.NewGuid()}.yml";
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folder);
        var swaggerFile = Path.Combine(folder, filename);
        await File.WriteAllTextAsync(swaggerFile, contents);
        return swaggerFile;
    }
}
