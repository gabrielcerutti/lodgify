using Grpc.Net.Client;
using Lodgify.Api.Application.Commands;
using ProtoDefinitions;
using System.Net.Http;

namespace Lodgify.Api.Infrastructure
{
    public interface IMoviesApi
    {
        Task<showListResponse> GetAllAsync();
        Task<showResponse> GetByIdAsync(string id);
    }

    public class MoviesApi : IMoviesApi
    {        
        private readonly ProtoDefinitions.MoviesApi.MoviesApiClient client;
        private readonly GrpcChannel channel;
        private readonly ICache _cache;
        private readonly ILogger<CreateShowtimeCommandHandler> _logger;
        private readonly string API_KEY = "should_come_from_key_vault";

        public MoviesApi(ICache cache, ILogger<CreateShowtimeCommandHandler> logger, IConfiguration configuration)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            API_KEY = configuration["MoviesApi:ApiKey"];
            if (string.IsNullOrEmpty(API_KEY) ) throw new ArgumentNullException(nameof(API_KEY), "MoviesApi key not found");
            
            var address = configuration["MoviesApi:BaseUrl"];
            if (string.IsNullOrEmpty(address)) throw new ArgumentNullException(nameof(address), "MoviesApi address not found");

            // Create a channel to the gRPC server
            channel = CreateAuthenticatedChannel(address);

            // Create a client for the gRPC service
            client = new ProtoDefinitions.MoviesApi.MoviesApiClient(channel);
        }

        public async Task<showListResponse> GetAllAsync()
        {
            var all = await client.GetAllAsync(new Empty());
            all.Data.TryUnpack<showListResponse>(out var data);
            return data;
        }

        public async Task<showResponse> GetByIdAsync(string id)
        {
            var response = await client.GetByIdAsync(new IdRequest { Id = id });
            if (response.Success)
            {
                _logger.LogInformation("----- Fetching movie from GRPC service - Movie: {@Id}", id);
                response.Data.TryUnpack<showResponse>(out var data);
                _cache.Set(id, data, TimeSpan.FromMinutes(5));                
                return data;
            } else
            {
                _logger.LogInformation("----- GRPC service failed, trying from cache..");
                var cached = _cache.Get<showResponse>(id);
                if (cached != null)
                {
                    _logger.LogInformation("----- Movie found in cache}");
                    return cached;
                }
                else
                {
                    _logger.LogError("----- Movie {@Id} Not found", id);
                    throw new Exception($"MovieId {id} Not found");
                }
            }
        }

        #region Private methods
        private GrpcChannel CreateAuthenticatedChannel(string address)
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(API_KEY))
                {
                    metadata.Add("X-Apikey", $"{API_KEY}");
                }
                return Task.CompletedTask;
            });

            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
                HttpHandler = httpHandler
            });
            return channel;
        }
        #endregion
    }
}