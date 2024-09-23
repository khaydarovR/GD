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

        public List<Feedback> Feedbacks { get;set;}
    }

    public class Feedback
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public int Starts { get; set; }
        
        public Guid ClientId { get; set; }
        public GDUser Client { get; set; }
        
        public Guid Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}