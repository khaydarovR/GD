namespace GD.Api.DB.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageValue { get; set; }
        public double Price { get; set; }
        public string Tags { get; set; }
        public int Amount { get; set; }

    }
}