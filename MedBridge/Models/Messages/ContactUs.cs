using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedBridge.Models.Messages
{
    public class ContactUs
    {

        [Key]
        public int MessageId { get; set; }

        public String Message { get; set; }


        public int UserId { get; set; }
        public User ? User { get; set; }

    }
}