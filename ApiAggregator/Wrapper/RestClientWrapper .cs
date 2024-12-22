using RestSharp;
using System.Threading;
using System.Threading.Tasks;

public class RestClientWrapper : IRestClientWrapper
{
    private readonly RestClient _restClient;

    public RestClientWrapper(string baseUrl)
    {
        _restClient = new RestClient(baseUrl);
    }

    public Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
    {
        return _restClient.ExecuteAsync(request, cancellationToken);
    }
}
