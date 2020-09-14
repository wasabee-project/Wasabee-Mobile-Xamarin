using System.Collections.Generic;
using Android.OS;
using Firebase.Analytics;
using MvvmCross.Platforms.Android;
using Rocks.Wasabee.Mobile.Core.Infra.Firebase;

namespace Rocks.Wasabee.Mobile.Droid.Infra.Firebase
{
	public class FirebaseAnalyticsService : IFirebaseAnalyticsService
	{
		private readonly IMvxAndroidCurrentTopActivity _currentTopActivity;

		public FirebaseAnalyticsService(IMvxAndroidCurrentTopActivity currentTopActivity)
		{
			_currentTopActivity = currentTopActivity;
		}

		public void LogEvent(string eventId)
		{
			LogEvent(eventId, null);
		}

		public void LogEvent(string eventId, string paramName, string value)
		{
			LogEvent(eventId, new Dictionary<string, string>
			{
				{paramName, value}
			});
		}

		public void SetUserId(string userId)
		{
#if DEBUG
			var fireBaseAnalytics = FirebaseAnalytics.GetInstance(_currentTopActivity.Activity.ApplicationContext);

			fireBaseAnalytics.SetUserId(userId);
#endif
		}

		public void LogEvent(string eventId, IDictionary<string, string> parameters)
		{
#if DEBUG
			var fireBaseAnalytics = FirebaseAnalytics.GetInstance(_currentTopActivity.Activity.ApplicationContext);

			if (parameters == null)
			{
				fireBaseAnalytics.LogEvent(eventId, null);
				return;
			}

			var bundle = new Bundle();

			foreach (var item in parameters)
			{
				bundle.PutString(item.Key, item.Value);
			}

			fireBaseAnalytics.LogEvent(eventId, bundle);
#endif
		}
	}
}