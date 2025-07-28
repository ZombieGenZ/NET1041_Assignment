using Newtonsoft.Json;

namespace Assignment.Utilities
{
    public static class GeocodeService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<(dynamic delivery, dynamic receiving)> GetLocationDataDynamicAsync(
            double deliveryLatitude,
            double deliveryLongitude,
            double receivingLatitude,
            double receivingLongitude)
        {
            string deliveryUrl = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={deliveryLatitude}&lon={deliveryLongitude}";
            string receivingUrl = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={receivingLatitude}&lon={receivingLongitude}";

            Task<HttpResponseMessage> deliveryTask = _httpClient.GetAsync(deliveryUrl);
            Task<HttpResponseMessage> receivingTask = _httpClient.GetAsync(receivingUrl);

            HttpResponseMessage[] responses = await Task.WhenAll(deliveryTask, receivingTask);

            string deliveryJson = await responses[0].Content.ReadAsStringAsync();
            string receivingJson = await responses[1].Content.ReadAsStringAsync();

            dynamic delivery = JsonConvert.DeserializeObject(deliveryJson);
            dynamic receiving = JsonConvert.DeserializeObject(receivingJson);

            return (delivery, receiving);
        }
    }
}