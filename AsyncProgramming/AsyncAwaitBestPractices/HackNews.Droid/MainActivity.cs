﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using HackerNews.Services;

namespace HackerNews.Droid
{
    [Activity(Label = "HackerNews.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState); 

            LoadApplication(new App());
        }

        [Preserve, Java.Interop.Export(nameof(GetStoriesAsBase64String))]
        public string GetStoriesAsBase64String() => BackdoorMethodServices.GetStoriesAsBase64String();
    }
}
