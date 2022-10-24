using System.ComponentModel;

namespace StockAPI.Model
{
    public class User
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [DefaultValue(0)]
        public int Admin { get; set; }
    }
}
