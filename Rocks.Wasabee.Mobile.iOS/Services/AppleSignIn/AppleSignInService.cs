using System;
using System.Threading.Tasks;
using Rocks.Wasabee.Mobile.Core.Apple.Models;
using Rocks.Wasabee.Mobile.Core.Services;
using AuthenticationServices;
using Foundation;
using UIKit;

namespace Rocks.Wasabee.Mobile.iOS.Services.AppleSignIn
{
    public class AppleSignInService : IAppleSignInService
    {
		AuthManager authManager;

        public async Task<AppleAccount> SignInAsync()
        {
            AppleAccount appleAccount = default;

			var provider = new ASAuthorizationAppleIdProvider();
			var req = provider.CreateRequest();

			authManager = new AuthManager(UIApplication.SharedApplication.KeyWindow);

			req.RequestedScopes = new[] { ASAuthorizationScope.FullName, ASAuthorizationScope.Email };
			var controller = new ASAuthorizationController(new[] { req });

			controller.Delegate = authManager;
			controller.PresentationContextProvider = authManager;

			controller.PerformRequests();

			try
			{
                var creds = await authManager.Credentials;

                if (creds == null)
                    return null;

                appleAccount = new AppleAccount();
                appleAccount.IdToken = JwtToken.Decode(new NSString(creds.IdentityToken, NSStringEncoding.UTF8).ToString());
                appleAccount.Email = creds.Email;
                appleAccount.UserId = creds.User;
                appleAccount.Name = NSPersonNameComponentsFormatter.GetLocalizedString(creds.FullName, NSPersonNameComponentsFormatterStyle.Default, NSPersonNameComponentsFormatterOptions.Phonetic);
                appleAccount.RealUserStatus = creds.RealUserStatus.ToString();

                return appleAccount;
            }
			catch (Exception)
			{
				return appleAccount;
			}
        }

        public bool Callback(string url) => true;
    }

	class AuthManager : NSObject, IASAuthorizationControllerDelegate, IASAuthorizationControllerPresentationContextProviding
	{
		public Task<ASAuthorizationAppleIdCredential> Credentials
			=> tcsCredential?.Task;

		TaskCompletionSource<ASAuthorizationAppleIdCredential> tcsCredential;

		UIWindow presentingAnchor;

		public AuthManager(UIWindow presentingWindow)
		{
			tcsCredential = new TaskCompletionSource<ASAuthorizationAppleIdCredential>();
			presentingAnchor = presentingWindow;
		}

		public UIWindow GetPresentationAnchor(ASAuthorizationController controller)
			=> presentingAnchor;

		[Export("authorizationController:didCompleteWithAuthorization:")]
		public void DidComplete(ASAuthorizationController controller, ASAuthorization authorization)
		{
			var creds = authorization.GetCredential<ASAuthorizationAppleIdCredential>();
			tcsCredential?.TrySetResult(creds);
		}

		[Export("authorizationController:didCompleteWithError:")]
		public void DidComplete(ASAuthorizationController controller, NSError error)
			=> tcsCredential?.TrySetException(new Exception(error.LocalizedDescription));
	}
}