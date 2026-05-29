using SV22T1020274.Domain.Partner;


namespace SV22T1020274.Admin.Models
{
    public class OrderShippingFormModel
    {
        public int OrderID { get; set; }
        public List<Shipper> Shippers { get; set; } = new();
    }
}
