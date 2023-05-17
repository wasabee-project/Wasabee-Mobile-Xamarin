using System;
using System.Threading.Tasks;
using Rocks.Wasabee.Mobile.Core.Apple.Models;
using Rocks.Wasabee.Mobile.Core.Services;

namespace Rocks.Wasabee.Mobile.Droid.Services.AppleSignIn
{
    public class AppleSignInService : IAppleSignInService
    {
        public bool Callback(string url)
        {
            throw new NotImplementedException();
        }

        public Task<AppleAccount> SignInAsync()
        {
            throw new NotImplementedException();
        }
    }
}

