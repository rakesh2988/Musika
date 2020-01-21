using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Models
{
    public class Parameters
    {
        public string Channel { get; set; }
        public string Store { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVC { get; set; }
        public string PosInputMode { get; set; }
        public string TrxType { get; set; }
        public string Amount { get; set; }
        public string Itbis { get; set; }
        public string CurrencyPosCode { get; set; }
        public string Payments { get; set; }
        public string Plan { get; set; }
        public string AcquirerRefData { get; set; }
        public string RRN { get; set; }
        public string CustomerServicePhone { get; set; }
        public string OrderNumber { get; set; }
        public string ECommerceUrl { get; set; }
        public string CustomOrderId { get; set; }
        public string OriginalDate { get; set; }
        public string OriginalTrxTicketNr { get; set; }
        public string AuthorizationCode { get; set; }
        public string ResponseCode { get; set; }
        public string AzulOrderId { get; set; }
        public string pemPath { get; set; }
        public string keyPath { get; set; }
    }
  
}