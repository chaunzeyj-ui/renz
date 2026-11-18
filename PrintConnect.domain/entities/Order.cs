using System;
using System.Collections.Generic;

namespace PrintConnect.domain.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        // "Pending", "Paid", "Delivered"
        public string Status { get; set; } = "Pending";

        // Navigation property
        public Customer? Customer { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}