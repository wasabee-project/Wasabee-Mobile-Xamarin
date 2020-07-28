using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.Infra.Firebase
{
	public interface IFirebaseAnalyticsService
	{
		void LogEvent(string eventId);
		void LogEvent(string eventId, string paramName, string value);
		void SetUserId(string userId);
		void LogEvent(string eventId, IDictionary<string, string> parameters);
	}
}