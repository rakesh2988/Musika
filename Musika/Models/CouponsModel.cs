using System;

namespace Musika.Models
{
    public class CouponsModel
    {
        public int Id { get; set; }

        public string EventName { get; set; }
        public string CouponCode { get; set; }
        public decimal Discount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CreateOn { get; set; }
        public int CreatedBy { get; set; }
        public int Status { get; set; }
        public EventReponse MessageResponse { get; set; }
        public string TicketCategory { get; set; }
        public int TicketCategoryID { get; set; }
        public int EventID { get; set; }
    }

    public class CouponsUpdateModel
    {
        public int Id { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public string CouponCode { get; set; }
        public string Discount { get; set; }
        public string ExpiryDate { get; set; }
        public string CreateOn { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
        public EventReponse MessageResponse { get; set; }
        public int TicketCategoryID { get; set; }
        public string TicketCategory { get; set; }
    }
}