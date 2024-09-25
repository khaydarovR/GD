using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace GD.Api.DB.Models
{
    public class GDUser : IdentityUser<Guid>
    {
        public string Address { get; set; } = "";

        public double Balance { get; set; }

        public double PosLati { get; set; }
        public double PosLong { get; set; }
    }

    public class Order
    {
        public Guid Id { get; set; }
        public double TargetPosLati { get; set; }
        public double TargetPosLong { get; set; }
        public string ToAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartDeliveryAt { get; set; }
        public DateTime? OrderClosedAt { get; set; }
        public Guid ClientId { get; set; }
        public GDUser Client { get; set; }
        
        public Product Product { get; set; }
        public Guid ProductId { get; set; }
        public int Amount { get; set; }
        
        /// <summary>
        /// Наличка, Карта, Онлайн
        /// </summary>
        public string PayMethod { get; set; }
        public double TotalPrice { get; set; }
        public string Status { get; set; }
        public Guid? CourierId { get; set; }
    }
}