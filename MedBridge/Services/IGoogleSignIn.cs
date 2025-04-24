﻿using MedBridge.Models.GoogLe_signIn;

namespace MedBridge.Services
{
    public interface IGoogleSignIn
    {
        Task<GoogleLoginResponse> SignInWithGoogle(string googleToken); // دخول عبر جوجل
        Task<bool> CompleteProfile(UserProfileRequest request); // إتمام البيانات

    }
}
