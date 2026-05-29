namespace SV22T1020274.Shop.Models;

public class CartDisplayLine
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public string? Photo { get; set; }
}
