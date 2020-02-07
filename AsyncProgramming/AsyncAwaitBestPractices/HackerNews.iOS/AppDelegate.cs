﻿using UIKit;
using Foundation;
using HackerNews.Services;

namespace HackerNews.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            global::Xamarin.Forms.Forms.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        [Preserve, Export("getStoriesAsBase64String:")]
        public NSString GetStoriesAsBase64String(NSString noValue) =>
            new NSString(BackdoorMethodServices.GetStoriesAsBase64String());
    }
}
