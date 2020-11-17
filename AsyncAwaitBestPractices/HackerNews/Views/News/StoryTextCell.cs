﻿using System;
using HackerNews.Constants;
using HackerNews.Models;
using Xamarin.Forms;

namespace HackerNews.Views.News
{
    public class StoryTextCell : TextCell
    {
        public StoryTextCell()
        {
            TextColor = ColorConstants.TextCellTextColor;
            DetailColor = ColorConstants.TextCellDetailColor;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var story = (StoryModel)BindingContext;

            Text = story.Title;
            Detail = $"{story.Score} Points by {story.Author} {GetAgeOfStory(story.CreatedAtDateTimeOffset)} ago";
        }

        string GetAgeOfStory(DateTimeOffset storyCreatedAt)
        {
            var timespanSinceStoryCreated = DateTimeOffset.UtcNow - storyCreatedAt;

            if (timespanSinceStoryCreated < TimeSpan.FromHours(1))
                return $"{Math.Ceiling(timespanSinceStoryCreated.TotalMinutes)} minutes";

            if (timespanSinceStoryCreated >= TimeSpan.FromHours(1) && timespanSinceStoryCreated < TimeSpan.FromHours(2))
                return $"{Math.Floor(timespanSinceStoryCreated.TotalHours)} hour";

            return $"{Math.Floor(timespanSinceStoryCreated.TotalHours)} hours";
        }
    }
}
