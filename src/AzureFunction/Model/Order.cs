using System.Text.Json.Serialization;

namespace AzureFunction.Model
{
    public class Order
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("customer-name")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("total-amount")]
        public decimal TotalAmount { get; set; }
    }
}
