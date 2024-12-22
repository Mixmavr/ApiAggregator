using RestSharp;
using System.Threading;
using System.Threading.Tasks;

public interface IRestClientWrapper
{
    Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default);
}
