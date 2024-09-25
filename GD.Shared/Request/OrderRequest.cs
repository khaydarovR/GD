namespace GD.Shared.Request;

public class OrderRequest
{
    public double TargetPosLati { get; set; }
    public double TargetPosLong { get; set; }
    public string ToAddress { get; set; }
    public Guid ProductId { get; set; }
    public int Amount { get; set; }
    /// <summary>
    /// Наличка, Карта, Онлайн
    /// </summary>
    public string PayMethod { get; set; }
}
