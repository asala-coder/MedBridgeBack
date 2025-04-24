namespace MedBridge.Models.GoogLe_signIn
{
    public class UserProfileRequest
    {
        public string GoogleToken { get; set; }
        public long Phone { get; set; }
        public string MedicalSpecialist { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
    }
}
