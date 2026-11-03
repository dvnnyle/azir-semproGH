using System.Text.Json;

namespace azir_sempro.Tests;

public class FormXssTests
{
    [Fact]
    public void SerializedMapJson_CannotCloseInlineScriptAndRoundTrips()
    {
        const string submittedJson = """[{"lat":58.1,"lng":8.0,"extra":"</script><script>alert(1)</script>"}]""";

        var scriptStringLiteral = JsonSerializer.Serialize(submittedJson);

        Assert.False(scriptStringLiteral.Contains("</script>", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(submittedJson, JsonSerializer.Deserialize<string>(scriptStringLiteral));
    }
}
