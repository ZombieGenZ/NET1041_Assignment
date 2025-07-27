using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class EvaluateBase
    {
        public double Star { get; set; }
        public string Content { get; set; }
        public long ProductId { get; set; }
    }
}
