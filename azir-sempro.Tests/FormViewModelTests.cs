using azir_sempro.Models;

namespace azir_sempro.Tests;

public class FormViewModelTests
{
    [Fact]
    public void NyModell_HarGronnFargeSomStandard()
    {
        var model = new FormViewModel();
        Assert.Equal("gronn", model.Farge);
    }

    [Fact]
    public void NyModell_HarStatusNy()
    {
        var model = new FormViewModel();
        Assert.Equal("ny", model.Status);
    }

    [Fact]
    public void NyModell_HarTommeTekstfelter()
    {
        var model = new FormViewModel();
        Assert.Equal("", model.Tittel);
        Assert.Equal("", model.PunkterJson);
    }
}