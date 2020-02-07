#if DEBUG
using HackerNews.Pages;
using HackerNews.ViewModels;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace HackerNews.Services
{
	public static class BackdoorMethodServices
	{
		public static string GetStoriesAsBase64String()
		{
			var storyList = GetViewModel().TopStoryList;
			return JsonConvert.SerializeObject(storyList);
		}

		static NewsPage GetNewsPage()
		{
			var mainPage = (NavigationPage)Application.Current.MainPage;
			return (NewsPage)mainPage.RootPage;
		}

		static NewsViewModelGoodAsyncAwaitPractices GetViewModel() => (NewsViewModelGoodAsyncAwaitPractices)GetNewsPage().BindingContext;
	}
}
#endif