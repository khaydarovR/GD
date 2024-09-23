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
        public DateTime At { get; set; }
        public string PayMethod { get; set; }
        public double TotalPrice { get; set; }
        public string Status { get; set; }
        [ForeignKey(nameof(GDUser.Id))]
        public Guid? CourierId { get; set; }
        public List<OrderItem> Basket { get; set; }
    }

    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Amount { get; set; }
    }
}