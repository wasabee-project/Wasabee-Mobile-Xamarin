using System;
using System.Threading.Tasks;
using Rocks.Wasabee.Mobile.Core.Apple.Models;

namespace Rocks.Wasabee.Mobile.Core.Services
{
	public interface IAppleSignInService
    {
        bool Callback(string url);
        Task<AppleAccount> SignInAsync();
    }
}