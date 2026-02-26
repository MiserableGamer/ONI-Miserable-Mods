// Steam Workshop upload helper for ONI mods. Uses Steamworks.NET 12.0.0 (game's steam_api64.dll).
// Must call SteamAPI.RunCallbacks() in a loop while waiting for async results.

using System;
using System.IO;
using System.Threading;
using Steamworks;

namespace SteamWorkshopUploader
{
    internal static class Program
    {
        private const uint AppIdOxygenNotIncluded = 457140;
        private const int PublishTimeoutMs = 120000; // 2 minutes
        private const int CallbackSleepMs = 50;

        private static int Main(string[] args)
        {
            try
            {
                return Run(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("EXCEPTION: " + ex.GetType().FullName + ": " + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static int Run(string[] args)
        {
            if (!ParseArgs(args, out string contentFolder, out string previewFile, out string title, out string description,
                out string changeNote, out string visibility, out ulong? publishedFileId))
            {
                PrintUsage();
                return 1;
            }

            if (!Directory.Exists(contentFolder))
            {
                Console.Error.WriteLine("ERROR: Content folder not found: " + contentFolder);
                return 1;
            }
            if (string.IsNullOrEmpty(previewFile) || !File.Exists(previewFile))
            {
                Console.Error.WriteLine("ERROR: Preview file not found: " + previewFile);
                return 1;
            }

            if (!SteamAPI.Init())
            {
                Console.Error.WriteLine("ERROR: SteamAPI.Init() failed. Is Steam running and logged in?");
                return 1;
            }

            try
            {
                var appId = new AppId_t(AppIdOxygenNotIncluded);
                bool isUpdate = publishedFileId.HasValue;
                PublishedFileId_t? finalId = null;
                bool success = false;

                if (isUpdate)
                {
                    success = SubmitUpdate(appId, publishedFileId.Value, contentFolder, previewFile, title, description, changeNote, out finalId);
                }
                else
                {
                    success = CreateAndSubmitNew(appId, contentFolder, previewFile, title, description, changeNote, visibility, out finalId);
                }

                if (success && finalId.HasValue)
                {
                    Console.WriteLine("SUCCESS PublishedFileId=" + finalId.Value.m_PublishedFileId);
                    return 0;
                }
            }
            finally
            {
                SteamAPI.Shutdown();
            }

            Console.Error.WriteLine("ERROR: Upload failed.");
            return 1;
        }

        private static bool CreateAndSubmitNew(AppId_t appId, string contentFolder, string previewFile, string title,
            string description, string changeNote, string visibility, out PublishedFileId_t? outPublishedId)
        {
            outPublishedId = null;
            CreateItemResult_t? createResult = null;
            var callResult = CallResult<CreateItemResult_t>.Create((r, failure) => { createResult = r; });

            SteamAPICall_t createCall = SteamUGC.CreateItem(appId, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
            if (createCall == SteamAPICall_t.Invalid)
            {
                Console.Error.WriteLine("ERROR: CreateItem returned invalid handle.");
                return false;
            }
            callResult.Set(createCall, (r, failure) => { createResult = r; });

            DateTime deadline = DateTime.UtcNow.AddMilliseconds(PublishTimeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                SteamAPI.RunCallbacks();
                if (createResult.HasValue)
                {
                    if (createResult.Value.m_eResult != EResult.k_EResultOK)
                    {
                        Console.Error.WriteLine("ERROR: CreateItem failed: " + createResult.Value.m_eResult);
                        return false;
                    }
                    outPublishedId = createResult.Value.m_nPublishedFileId;
                    break;
                }
                Thread.Sleep(CallbackSleepMs);
            }

            if (!outPublishedId.HasValue)
            {
                Console.Error.WriteLine("Publish timed out.");
                return false;
            }

            return SubmitUpdate(appId, outPublishedId.Value.m_PublishedFileId, contentFolder, previewFile, title, description, changeNote, out _);
        }

        private static bool SubmitUpdate(AppId_t appId, ulong publishedFileId, string contentFolder, string previewFile,
            string title, string description, string changeNote, out PublishedFileId_t? outPublishedId)
        {
            outPublishedId = new PublishedFileId_t(publishedFileId);
            SubmitItemUpdateResult_t? submitResult = null;
            var callResult = CallResult<SubmitItemUpdateResult_t>.Create((r, failure) => { submitResult = r; });

            UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(appId, new PublishedFileId_t(publishedFileId));
            if (handle == UGCUpdateHandle_t.Invalid)
            {
                Console.Error.WriteLine("ERROR: StartItemUpdate failed.");
                return false;
            }

            if (!SteamUGC.SetItemContent(handle, contentFolder))
            {
                Console.Error.WriteLine("ERROR: SetItemContent failed.");
                return false;
            }
            if (!SteamUGC.SetItemPreview(handle, previewFile))
            {
                Console.Error.WriteLine("ERROR: SetItemPreview failed.");
                return false;
            }
            if (!SteamUGC.SetItemTitle(handle, title))
            {
                Console.Error.WriteLine("ERROR: SetItemTitle failed.");
                return false;
            }
            if (!SteamUGC.SetItemDescription(handle, description ?? ""))
            {
                Console.Error.WriteLine("ERROR: SetItemDescription failed.");
                return false;
            }

            SteamAPICall_t submitCall = SteamUGC.SubmitItemUpdate(handle, changeNote ?? "");
            if (submitCall == SteamAPICall_t.Invalid)
            {
                Console.Error.WriteLine("ERROR: SubmitItemUpdate returned invalid handle.");
                return false;
            }
            callResult.Set(submitCall, (r, failure) => { submitResult = r; });

            DateTime deadline = DateTime.UtcNow.AddMilliseconds(PublishTimeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                SteamAPI.RunCallbacks();
                if (submitResult.HasValue)
                {
                    if (submitResult.Value.m_eResult != EResult.k_EResultOK)
                    {
                        Console.Error.WriteLine("ERROR: SubmitItemUpdate failed: " + submitResult.Value.m_eResult);
                        return false;
                    }
                    return true;
                }
                Thread.Sleep(CallbackSleepMs);
            }

            Console.Error.WriteLine("Publish timed out.");
            return false;
        }

        private static bool ParseArgs(string[] args, out string contentFolder, out string previewFile, out string title,
            out string description, out string changeNote, out string visibility, out ulong? publishedFileId)
        {
            contentFolder = previewFile = title = description = changeNote = visibility = null;
            publishedFileId = null;

            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                if (i + 1 >= args.Length) return false;
                string value = args[i + 1];
                switch (a)
                {
                    case "-ContentFolder": contentFolder = value; break;
                    case "-PreviewFile": previewFile = value; break;
                    case "-Title": title = value; break;
                    case "-Description": description = value; break;
                    case "-ChangeNote": changeNote = value; break;
                    case "-Visibility": visibility = value; break;
                    case "-PublishedFileId":
                        if (ulong.TryParse(value, out ulong id))
                            publishedFileId = id;
                        break;
                }
                i++;
            }

            return !string.IsNullOrEmpty(contentFolder) && !string.IsNullOrEmpty(previewFile) && !string.IsNullOrEmpty(title);
        }

        private static void PrintUsage()
        {
            Console.Error.WriteLine("Usage: SteamWorkshopUploader -ContentFolder <path> -PreviewFile <path> -Title <title> -Description <text> -ChangeNote <text> -Visibility <public|unlisted> [-PublishedFileId <id>]");
        }
    }
}
