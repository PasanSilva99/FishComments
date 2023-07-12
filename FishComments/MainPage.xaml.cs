using ColorCode.Compilation.Languages;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.System;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FishComments
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
       private DispatcherTimer timer;

        private List<String> commentsCache;
        public MainPage()
        {
            this.InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private async void Timer_Tick(object sender, object e)
        {
            await GetComments();
        }

        private async void WebView2_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            
            //await WebView2.CoreWebView2.ExecuteScriptAsync("document.getElementById('comments').scrollTo(0, document.getElementById('comments').scrollHeight)");
            await GetComments();
            if(timer.IsEnabled) timer.Stop();
            else timer.Start();

            WebView2.KeyDown += WebView2_KeyDown;
        }

        private async void WebView2_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            await GetComments();
        }

        private async void CoreWebView2_NavigationCompleted(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            await GetComments();
        }

        private async Task GetComments()
        {
            try
            {
                await WebView2.CoreWebView2.ExecuteScriptAsync("document.getElementById('related').style = \'display:none\'");
                await WebView2.CoreWebView2.ExecuteScriptAsync("document.getElementById('bottom-row').style = \'display:none\'");
                var contentElement = await WebView2.CoreWebView2.ExecuteScriptAsync(@"
    var commentsElement = document.getElementById('comments');
    var contentElement = commentsElement.querySelector('#contents');
    var commentThreadRenderers = contentElement.querySelectorAll('yt-formatted-string');

    var commentThreadHtmlArray = Array.from(commentThreadRenderers).map(function(commentThreadRenderer) {
        return commentThreadRenderer.innerHTML;
    });

    commentThreadHtmlArray.join('\n');");

                var html1 = contentElement.Replace("\\u003C", "<");

                string filteredhead = Regex.Replace(html1, "<a.*?</a>", string.Empty);
                string filtered = Regex.Replace(filteredhead, "<.*?>", string.Empty);


                var comments = filtered.Substring(1, filtered.Length - 2).Split("\\n", StringSplitOptions.RemoveEmptyEntries);

                commentsList.ItemsSource = new List<string>(comments);

                commentsCache = new List<String>(comments);

                Number_of_comments.Text = $"{comments.Length} Comments";
            }
            catch { }
        }

        private async void WebView2_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            await GetComments();
        }

        private void url_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (timer.IsEnabled) timer.Stop();
            try
            {
                WebView2.Source = new Uri(url.Text);
            }
            catch { }
        }

        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled) timer.Stop();
            try
            {
                WebView2.Source = new Uri(url.Text);
            }
            catch { }
            
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (timer.IsEnabled) timer.Stop();
                // Create a File Save Picker instance
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("Text File", new List<string>() { ".csv" });
                savePicker.SuggestedFileName = $"Comments {WebView2.CoreWebView2.DocumentTitle}";

                // Show the File Save Picker dialog
                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Convert the comments list to a string
                    List<string> comments = commentsCache; // Replace with your actual comments list
                    string commentsText = string.Join(Environment.NewLine, comments);

                    // Write the comments to the selected file
                    await FileIO.WriteTextAsync(file, commentsText);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
