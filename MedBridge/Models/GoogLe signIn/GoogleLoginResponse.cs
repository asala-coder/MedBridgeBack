namespace MedBridge.Models.GoogLe_signIn
{
    public class GoogleLoginResponse
    {
        public string Status { get; set; } // "new_user" or "existing_user"
        public string Email { get; set; }
        public string Token { get; set; } // لو مستخدم قديم فقط
    }
}

