﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HackerNews.Models;
using HackerNews.ViewModels.Base;
using Xamarin.Forms;

namespace HackerNews.ViewModels
{
	internal class NewsViewModelBadAsyncAwaitPractices : BaseViewModel
    {
        bool isListRefreshing;
        ICommand? refreshCommand;
        List<StoryModel> topStoryList = new List<StoryModel>();

        public NewsViewModelBadAsyncAwaitPractices()
        {
            //ToDo Refactor
            ExecuteRefreshCommand();
        }

        public event EventHandler<string>? ErrorOcurred;

        //ToDo Refactor
        public ICommand RefreshCommand => refreshCommand ??= new Command(async () => await ExecuteRefreshCommand());

        public List<StoryModel> TopStoryList
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
            //ToDo Refactor
            SetIsRefreshing(true).Wait();

            try
            {
                TopStoryList = await GetTopStories(20);
            }
            finally
            {
                //ToDo Refactor
                SetIsRefreshing(false).Wait();
            }
        }

        async Task<List<StoryModel>> GetTopStories(int numberOfStories)
        {
            List<StoryModel> topStoryList = new List<StoryModel>();

            //ToDo Refactor
            var topStoryIds = await GetTopStoryIDs();

            if (topStoryIds != null)
            {
                for (int i = 0; i < numberOfStories; i++)
                {
                    //ToDo Refactor
                    var story = await GetStory(topStoryIds[i]);
                    topStoryList.Add(story);
                }
            }

            return topStoryList.Where(x => x != null).OrderByDescending(x => x.Score).ToList();
        }

        //ToDo Refactor
        async Task<StoryModel> GetStory(string storyId)
        {
            return await GetDataObjectFromApi<StoryModel>($"https://hacker-news.firebaseio.com/v0/item/{storyId}.json?print=pretty");
        }

        //ToDo Refactor
        async Task<List<string>?> GetTopStoryIDs()
        {
            try
            {
                return await GetDataObjectFromApi<List<string>>("https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }

        async Task<bool> SetIsRefreshing(bool isRefreshing)
        {
            IsListRefreshing = isRefreshing;
            await Task.Delay(100);

            return isRefreshing;
        }

        void OnErrorOccurred(string message) => ErrorOcurred?.Invoke(this, message);
    }
}
