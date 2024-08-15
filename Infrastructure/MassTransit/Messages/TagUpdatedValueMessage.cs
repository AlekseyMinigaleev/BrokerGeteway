
namespace Infrastructure.MassTransit.Messages
{
    public class TagUpdatedValueMessage
    {
        public int Id { get; set; }

        public string Value { get; set; }
    }
}
