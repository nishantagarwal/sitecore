using System;
using Yahoo.Yui.Compressor;
using Sitecore.Data.Items;
using System.IO;
using Sitecore.Publishing.Pipelines.PublishItem;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using System.Text;

namespace MediaLibraryMinifier
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
                    var item = context.PublishHelper.GetTargetItem(context.ItemId);

                    // Check if item is published and it is a media item
                    if (item != null && item.Paths.IsMediaItem)
                    {
                        MediaItem mediaItem = new MediaItem(item);
                        string fileExtension = mediaItem.Extension.ToLower();
                        if (fileExtension != "css" && fileExtension != "js")
                        {
                            return;
                        }

                        using (Stream mediaStream = mediaItem.GetMediaStream())
                        {
                            using (StreamReader sr = new StreamReader(mediaStream))
                            {
                                string originalString = sr.ReadToEnd();
                                string newString = String.Empty;

                                if (fileExtension == "css")
                                {
                                    newString = new CssCompressor().Compress(originalString);
                                }
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
                                    using (new EditContext((Item)mediaItem, SecurityCheck.Disable))
                                    {
                                        Sitecore.Resources.Media.Media media = MediaManager.GetMedia(mediaItem);
                                        media.SetStream(new MediaStream(stream, fileExtension, mediaItem));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}