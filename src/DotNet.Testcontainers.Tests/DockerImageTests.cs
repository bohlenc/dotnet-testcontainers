namespace DotNet.Testcontainers.Tests
{
  using System.Net;
  using DotNet.Testcontainers.Builder;
  using DotNet.Testcontainers.Clients;
  using DotNet.Testcontainers.Images;
  using Xunit;
  using static LanguageExt.Prelude;

  public class DockerImageTests
  {
    [Theory]
    [ClassData(typeof(DockerImageTestDataNameParser))]
    public void Test_DockerImageNameParser_WithValidImageNames_NoException(IDockerImage expected, string fullName)
    {
      // Given
      var dockerImage = new TestcontainersImage();

      // When
      dockerImage.Image = fullName;

      // Then
      Assert.Equal(expected.Repository, dockerImage.Repository);
      Assert.Equal(expected.Name, dockerImage.Name);
      Assert.Equal(expected.Tag, dockerImage.Tag);
    }

    [Fact]
    public void Test_DockerImageExist_WithInValidImage_NoException()
    {
      Assert.False(TestcontainersClient.Instance.ExistImageById(string.Empty));
    }

    [Fact]
    public void Test_DockerContainerExist_WithInValidImage_NoException()
    {
      Assert.False(TestcontainersClient.Instance.ExistContainerById(string.Empty));
    }

    [Fact]
    public void Test_DockerContainerStartStop_WithValidImage_NoException()
    {
      // Given
      var dockerImage = "alpine";

      // When
      var testcontainersBuilder = new TestcontainersBuilder()
        .WithImage(dockerImage);

      // Then
      using (var testcontainers = testcontainersBuilder.Build())
      {
        testcontainers.Start();
      }
    }

    [Fact]
    public void Test_DockerContainerPortBindings_WithValidImage_NoException()
    {
      // Given
      var http = Tuple(80, 80);
      var https = Tuple(443, 80);

      // When
      var nginx = new TestcontainersBuilder()
        .WithImage("nginx");

      // Then
      List(http, https).Iter(port =>
      {
        using (var testcontainers = port.Map(nginx.WithPortBinding).Build())
        {
          testcontainers.Start();

          var request = WebRequest.Create($"http://localhost:{port.Item1}");

          var response = (HttpWebResponse)request.GetResponse();

          var isAvailable = Optional(response).Match(
            Some: value => value.StatusCode == HttpStatusCode.OK,
            None: () => false);

          Assert.True(isAvailable, $"nginx port {port.Item1} is not available.");
        }
      });
    }

    [Fact]
    public void Test_DockerContainerName_WithoutName_NoException()
    {
      // Given
      // When
      var testcontainersBuilder = new TestcontainersBuilder()
        .WithImage("alpine");

      // Then
      using (var testcontainers = testcontainersBuilder.Build())
      {
        testcontainers.Start();
        Assert.NotEmpty(testcontainers.Name);
      }
    }

    [Fact]
    public void Test_DockerContainerName_WithName_NoException()
    {
      // Given
      var name = "foo";

      // When
      var testcontainersBuilder = new TestcontainersBuilder()
        .WithImage("alpine")
        .WithName(name);

      // Then
      using (var testcontainers = testcontainersBuilder.Build())
      {
        testcontainers.Start();
        Assert.Equal(name, testcontainers.Name);
      }
    }
  }
}
