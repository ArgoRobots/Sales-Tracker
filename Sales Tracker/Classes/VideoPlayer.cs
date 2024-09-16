namespace Sales_Tracker.Classes
{
    internal class VideoPlayer
    {
        /// <summary>
        /// Loads a YouTube video into the specified WebBrowser control and a loading message while the video is loading.
        /// </summary>
        public static void LoadVideo(WebBrowser webViewer, string url)
        {
            // https://stackoverflow.com/questions/73795000/how-do-i-display-a-youtube-video-in-webviewer#73795057

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

            // Convert CustomColors.mainBackground to a valid HTML color string
            string backgroundColor = $"#{CustomColors.mainBackground.R:X2}{CustomColors.mainBackground.G:X2}{CustomColors.mainBackground.B:X2}";

            // Start building the HTML string
            string html = "<html style='width: 100%; height: 100%; margin: 0; padding: 0;'>";
            html += "<head>";
            html += "<meta content='IE=Edge' http-equiv='X-UA-Compatible'/>";
            html += "</head>";

            // Begin body with the custom background color
            html += $"<body style='width: 100%; height: 100%; margin: 0; padding: 0; background-color: {backgroundColor};'>";

            // Placeholder with custom font for the loading message
            html += "<div id='placeholder' style='width: 100%; height: 100%; display: flex; align-items: center; justify-content: center;'>";
            html += "<p style='color: white; font-family: \"Segoe UI\"; font-size: 11pt;'>Loading Video...</p>";
            html += "</div>";

            // Iframe (hidden initially)
            html += $"<iframe id='video' src='https://www.youtube.com/embed/{videoId}' ";
            html += "style='padding: 0px; width: 100%; height: 100%; border: none; display: none;'></iframe>";

            // JavaScript to show iframe after loading
            html += "<script>";
            html += "var iframe = document.getElementById('video');";
            html += "iframe.onload = function() {";
            html += "document.getElementById('placeholder').style.display = 'none';";
            html += "iframe.style.display = 'block';";
            html += "};";
            html += "</script>";

            // Close body and html
            html += "</body></html>";

            // Set the HTML content of the WebBrowser control
            webViewer.DocumentText = html;
        }
    }
}