using System.Net;
using System.Text.Json;
using cloud_dictionary.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace cloud_dictionary
{
    public class DictionaryFunctions
    {
        private readonly ILogger _logger;
        private readonly DictionaryRepository _dictionaryRepository;

        public DictionaryFunctions(ILoggerFactory loggerFactory, DictionaryRepository dictionaryRepository)
        {
            _logger = loggerFactory.CreateLogger<DictionaryFunctions>();
            _dictionaryRepository = dictionaryRepository;
        }

        [Function("GetDefinitions")]
        public async Task<HttpResponseData> GetDefinitions(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            var definitions = await _dictionaryRepository.GetDefinitionsAsync(null, null);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            response.WriteString(JsonSerializer.Serialize(definitions));

            return response;
        }

        [Function("GetWords")]
        public async Task<HttpResponseData> GetWords(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            var words = await _dictionaryRepository.GetWordsAsync(null, null);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            response.WriteString(JsonSerializer.Serialize(words));

            return response;
        }

        [Function("GetRandomDefinition")]
        public async Task<HttpResponseData> GetRandomDefinition(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            var definition = await _dictionaryRepository.GetRandomDefinitionAsync();
            response.WriteString(JsonSerializer.Serialize(definition));

            return response;

        }

        [Function("CreateDefinition")]
        public async Task<HttpResponseData> CreateDefinition(
       [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req, string word, string content, string learn_more_url, string tag, string abbreviation, string author_name, string author_link)
        {
            var response = req.CreateResponse(HttpStatusCode.Created);
            var definition = new Definition(word, content, author_name, author_link, learn_more_url, tag, abbreviation);
            await _dictionaryRepository.AddDefinitionAsync(definition);
            response.WriteString(JsonSerializer.Serialize(definition));
            return response;

        }
        [Function("UpdateDefinition")]
        public async Task<HttpResponseData> UpdateDefinition(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = "{definition_id}")] HttpRequestData req, string definition_id,
           string word, string content, string learn_more_url, string tag, string abbreviation, string author_name, string author_link)
        {
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                var existingItem = await _dictionaryRepository.GetDefinitionAsync(definition_id);
                if (existingItem == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }
                existingItem.Abbreviation = abbreviation;
                existingItem.Author.Name = author_name;
                existingItem.Author.Link = author_link;
                existingItem.Content = content;
                existingItem.LearnMoreUrl = learn_more_url;
                existingItem.Tag = tag;
                existingItem.Word = word;
                await _dictionaryRepository.UpdateListItem(existingItem);
                return response;
            }

        }

    }
}
