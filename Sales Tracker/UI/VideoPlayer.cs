using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Loads a YouTube video into the specified WebBrowser control and a loading message while the video is loading.
    /// </summary>
    internal class VideoPlayer
    {
        // Properties
        private static Label loadingLabel;
        private static Panel loadingPanel;
        private static Timer failureTimer;

        // Loading panel
        /// <summary>
        /// Loads a YouTube video into the specified WebBrowser control and shows a loading message while the video is loading.
        /// </summary>
        public static void LoadVideo(WebBrowser webBrowser, string url)
        {
            // https://stackoverflow.com/questions/73795000/how-do-i-display-a-youtube-video-in-webviewer#73795057

            // Create and display the loading panel
            Panel loadingPanel = CreateLoadingPanel(webBrowser);
            webBrowser.Parent.Controls.Add(loadingPanel);
            loadingPanel.BringToFront();

            SetLabelText("Loading video...");
            StartFailureTimer();

            // Split the URL to get the video ID
            string videoId;
            if (url.Contains('='))
            {
                videoId = url.Split('=')[1];
            }
            else
            {
                throw new ArgumentException("Invalid YouTube URL format");
            }

            string html = "<html style='width: 100%; height: 100%; margin: 0; padding: 0;'><head>";
            html += "<meta content='IE=Edge' http-equiv='X-UA-Compatible'/>";
            html += "</head><body style='width: 100%; height: 100%; margin: 0; padding: 0;'>";
            html += $"<iframe id='video' src='https://www.youtube.com/embed/{videoId}' style=\"padding: 0px; width: 100%; height: 100%; border: none; display: block;\"></iframe>";
            html += "</body></html>";

            // Set the HTML content to the webBrowser
            webBrowser.DocumentText = html;

            webBrowser.DocumentCompleted += (_, _) =>
            {
                StopFailureTimer();
                webBrowser.Parent.Controls.Remove(loadingPanel);
            };

            //  SetLabelText("Failed to load video. Please check your internet connection");
        }
        /// <summary>
        /// Creates a loading panel with a loading label centered inside.
        /// </summary>
        private static Panel CreateLoadingPanel(WebBrowser webBrowser)
        {
            if (loadingPanel != null) { return loadingPanel; }

            // Create the loading panel
            loadingPanel = new()
            {
                Size = webBrowser.Size,
                Location = webBrowser.Location,
                BackColor = CustomColors.MainBackground,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Create the loading label
            loadingLabel = new()
            {
                ForeColor = CustomColors.Text,
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.None
            };
            loadingPanel.Controls.Add(loadingLabel);

            return loadingPanel;
        }
        private static void SetLabelText(string text)
        {
            loadingLabel.Text = text;

            // Center the label in the panel
            loadingLabel.Location = new Point(
                (loadingPanel.Width - loadingLabel.Width) / 2,
                (loadingPanel.Height - loadingLabel.Height) / 2
            );
        }

        // Time out
        private static void StartFailureTimer()
        {
            failureTimer = new Timer { Interval = 5000 };  // 5 second timeout for failure
            failureTimer.Tick += (s, e) =>
            {
                StopFailureTimer();
                SetLabelText("Video failed to load. Please check your internet connection");
            };
            failureTimer.Start();
        }
        private static void StopFailureTimer()
        {
            if (failureTimer != null)
            {
                failureTimer.Stop();
                failureTimer.Dispose();
                failureTimer = null;
            }
        }
    }
}