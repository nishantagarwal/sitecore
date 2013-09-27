using System;
using Yahoo.Yui.Compressor;
using Sitecore.Data.Items;
using System.IO;
using Sitecore.Publishing.Pipelines.PublishItem;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using System.Text;

namespace MediaItemsMinifier
{
    /// <summary>
    /// Class providing event handler for ItemProcessedEvent
    /// </summary>
    public class ItemProcessedEventHandler
    {
        /// <summary>
        /// Gets or set the list of comma separated databases.
        /// </summary>
        public string Databases { get; set; }

        /// <summary>
        /// OnItemProcessed event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnItemProcessed(object sender, EventArgs args)
        {
            var context = ((ItemProcessedEventArgs)args).Context;
            string targetDataBase = context.PublishOptions.TargetDatabase.Name.ToLower();
            Databases = Databases.ToLower();

            if (Databases.Contains(targetDataBase))
            {
                using (new Sitecore.SecurityModel.SecurityDisabler())
                {
                    // Get published item in target environment
                    var item = context.PublishHelper.GetTargetItem(context.ItemId);

                    // Check if item is published and it is a media item
                    if (item != null && item.Paths.IsMediaItem)
                    {
                        MediaItem mediaItem = new MediaItem(item);
                        string fileExtension = mediaItem.Extension.ToLower();

                        // Check if media item is css or js resource
                        if (fileExtension != "css" && fileExtension != "js")
                        {
                            return;
                        }

                        try
                        {
                            // Get Stream object of the media item
                            using (Stream mediaStream = mediaItem.GetMediaStream())
                            {
                                using (StreamReader sr = new StreamReader(mediaStream))
                                {
                                    // Read contents of the media items into a String object
                                    string originalString = sr.ReadToEnd();
                                    string newString = String.Empty;

                                    // If media item is CSS, then use CssCompressor for compression
                                    if (fileExtension == "css")
                                    {
                                        newString = new CssCompressor().Compress(originalString);
                                    }
                                    // If media item is JS, then use JavaScriptCompressor for compression
                                    else if (fileExtension == "js")
                                    {
                                        newString = new JavaScriptCompressor().Compress(originalString);
                                    }

                                    if (String.IsNullOrEmpty(newString))
                                    {
                                        return;
                                    }

                                    byte[] byteArray = Encoding.ASCII.GetBytes(newString);
                                    using (MemoryStream stream = new MemoryStream(byteArray))
                                    {
                                        // Edit mediaItem and upload minified version
                                        using (new EditContext((Item)mediaItem, SecurityCheck.Disable))
                                        {
                                            Sitecore.Resources.Media.Media media = MediaManager.GetMedia(mediaItem);
                                            media.SetStream(new MediaStream(stream, fileExtension, mediaItem));
                                        }
                                    }
                                }
                            }
                            Sitecore.Diagnostics.Log.Info(String.Format("Media Items Minfier: Published item - '{0}' minified successfully.", item.Paths.FullPath), this);
                        }
                        catch (Exception ex)
                        {
                            Sitecore.Diagnostics.Log.Error(String.Format("Media Items Minfier: Published item - '{0}' not minified. Error: ", item.Paths.FullPath), ex, this);
                        }
                    }
                }
            }
        }
    }
}
