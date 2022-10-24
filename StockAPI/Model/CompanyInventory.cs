using System.ComponentModel.DataAnnotations;

namespace StockAPI.Model
{
    public class CompanyInventory
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public long CompanyNit { get; set; }

        public Company? Company { get; set; }
        public int Quantity { get; set; }
    }
}
