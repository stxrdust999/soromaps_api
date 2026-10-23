namespace soromaps_api.Tests.Extensions;

using FluentAssertions;
using soromaps_api.Extensions;

/**
 * Suíte de testes unitários para a geração e sanitização de slugs em títulos e nomes urbanos.
 */
public sealed class SlugHelperTests
{
    [Theory]
    [InlineData("Padaria Real Campolim", "padaria-real-campolim")]
    [InlineData("Parque das Águas & Jardim Botânico", "parque-das-aguas-jardim-botanico")]
    [InlineData("São Bento do Sapucaí!", "sao-bento-do-sapucai")]
    [InlineData("Avenida Dom Aguirre --- Trânsito", "avenida-dom-aguirre-transito")]
    public void GenerateSlug_ShouldNormalizeDiacriticsAndHyphens(string input, string expected)
    {
        var result = SlugHelper.GenerateSlug(input);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateSlug_WhenInputIsEmptyOrWhitespace_ShouldReturnEmptyString(string? input)
    {
        var result = SlugHelper.GenerateSlug(input!);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateSlug_WhenResultExceedsSixtyCharacters_ShouldTruncateToSixtyWithoutTrailingHyphen()
    {
        var longInput = "Este é um nome extremamente longo para um estabelecimento ou localidade histórica de Sorocaba com muitos detalhes";

        var result = SlugHelper.GenerateSlug(longInput);

        result.Length.Should().BeLessOrEqualTo(60);
        result.Should().NotEndWith("-");
    }
}
