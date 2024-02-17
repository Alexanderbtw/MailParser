using System.Text;

namespace MailParser.Domain
{
    public class LeadInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal AmountOfDebt { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var prop in typeof(LeadInfo).GetProperties())
            {
                sb.Append($"{prop.Name}: {prop.GetValue(this)}\n");
            }
            return sb.ToString();
        }
    }
}
