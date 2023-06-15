
using System;
/// <summary>
/// This is an empty interface which is used just to tag the callback handlers
/// </summary>
namespace GameModules
{
	public interface ICallbackHandler : IDisposable
	{
		void Listen();
		void Ignore();
	}
}
