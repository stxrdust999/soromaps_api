namespace soromaps_api.Services.Interfaces;

/**
 * Contrato para o serviço de mineração e importação de locais urbanos a partir do OpenStreetMap (Overpass API).
 */
public interface IOsmPlaceScraperService
{
    /**
     * Minera pontos de interesse em Sorocaba via Overpass API, realiza deduplicação com a base existente e persiste novos locais.
     */
    Task<int> ScrapeAndImportAsync(bool approveImmediately = true, CancellationToken cancellationToken = default);
}
