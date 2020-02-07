using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using HackerNews.Constants;
using HackerNews.Models;
using HackerNews.ViewModels.Base;

namespace HackerNews.ViewModels
{
	internal class NewsViewModelGoodAsyncAwaitPractices : BaseViewModel
    {
        readonly WeakEventManager<string> errorOcurredEventManager = new WeakEventManager<string>();

        bool isListRefreshing;
        IAsyncCommand? refreshCommand;
        IReadOnlyList<StoryModel> topStoryList = new List<StoryModel>().ToList();

        public NewsViewModelGoodAsyncAwaitPractices()
        {
            ExecuteRefreshCommand().SafeFireAndForget(onException:  ex => Debug.WriteLine(ex));
        }

        public event EventHandler<string> ErrorOcurred
        {
            add => errorOcurredEventManager.AddEventHandler(value);
            remove => errorOcurredEventManager.RemoveEventHandler(value);
        }

        public IAsyncCommand RefreshCommand => refreshCommand ??= new AsyncCommand(ExecuteRefreshCommand);

        public IReadOnlyList<StoryModel> TopStoryList
        {
            get => topStoryList;
            set => SetProperty(ref topStoryList, value);
        }

        public bool IsListRefreshing
        {
            get => isListRefreshing;
            set => SetProperty(ref isListRefreshing, value);
        }

        async Task ExecuteRefreshCommand()
        {
            IsListRefreshing = true;

            try
            {
                TopStoryList = await GetTopStories(StoriesConstants.NumberOfStories).ConfigureAwait(false);
            }
            finally
            {
                IsListRefreshing = false;
            }
        }

        async Task<List<StoryModel>> GetTopStories(int numberOfStories)
        {
            var topStoryIds = await GetTopStoryIDs().ConfigureAwait(false);

            var getTopStoryTaskList = new List<Task<StoryModel>>();
            for (var i = 0; i < Math.Min(topStoryIds.Count, numberOfStories); i++)
            {
                getTopStoryTaskList.Add(GetStory(topStoryIds[i]));
            }

            var topStoriesArray = await Task.WhenAll(getTopStoryTaskList).ConfigureAwait(false);

            return topStoriesArray.Where(x => x != null).OrderByDescending(x => x.Score).ToList();
        }

        Task<StoryModel> GetStory(string storyId) => GetDataObjectFromApi<StoryModel>($"https://hacker-news.firebaseio.com/v0/item/{storyId}.json?print=pretty");

        async ValueTask<List<string>> GetTopStoryIDs()
        {
            if (TopStoryList.Any())
                return TopStoryList.Select(x => x.Id.ToString()).ToList();

            try
            {
                return await GetDataObjectFromApi<List<string>>("https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                OnErrorOccurred(e.Message);
                return Enumerable.Empty<string>().ToList();
            }
        }

        void OnErrorOccurred(string message) => errorOcurredEventManager.HandleEvent(this, message, nameof(ErrorOcurred));
    }
}
