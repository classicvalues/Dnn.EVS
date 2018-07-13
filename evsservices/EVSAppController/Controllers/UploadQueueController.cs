﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EVSAppController.Models;

namespace EVSAppController.Controllers
{
    public class UploadQueueController : PetaPocoBaseClass
    {
        //create

        public static void AddItemToQueue(UploadQueue queue)
        {
            EVSDatabase.Insert("UploadQueue", "UploadQueueID", true, queue);

            UpdateItemQueueStatus();
        }

        //read
        
        private static UploadQueue GetNextItemFromQueue()
        {
            try
            {
                return EVSDatabase.First<UploadQueue>("SELECT TOP 1 * FROM [dbo].[UploadQueue] WHERE [Processing] = 0 ORDER BY [UploadedDate] ASC");
            }
            catch //(Exception exc)
            {
                return null;
            }
        }

        public static List<UploadQueue> GetUploadQueueList()
        {
            return EVSDatabase.Fetch<UploadQueue>("SELECT * FROM [dbo].[UploadQueue]");
        }

        //update

        private static void LockItem(int uploadQueueId)
        {
            EVSDatabase.Execute("UPDATE [dbo].[UploadQueue] SET [Processing] = 1 WHERE [UploadQueueID] = @0", uploadQueueId);
        }

        //delete

        public static void RemoveItemFromQueue(UploadQueue queue)
        {
            EVSDatabase.Delete("UploadQueue", "UploadQueueID", queue);

            UpdateItemQueueStatus();
        }

        //business logic

        public static UploadQueue GetAndLockItemForProcessing()
        {
            var item = GetNextItemFromQueue();

            if (item != null)
            {
                LockItem(item.UploadQueueID);
            }

            return item;
        }

        public static void UpdateItemQueueStatus()
        {
            var queue = GetUploadQueueList();
            var processingItems = queue.Count(uploadQueue => uploadQueue.Processing);
            var totalItems = queue.Count;
            var itemCounter = 0;

            foreach (var uploadQueue in queue)
            {
                itemCounter++;
                var otheritems = (totalItems - processingItems);
                
                if (otheritems <= 0)
                    otheritems = 1;

                var currentProgress = ((itemCounter) / otheritems) * 100;
                var status = new Status
                    {
                        FileID = uploadQueue.FileID,
                        UserID = uploadQueue.UserKey,
                        Finished = false,
                        CurrentProgress = currentProgress
                    };

                if (currentProgress == 100 && !uploadQueue.Processing)
                {
                    status.CurrentMessage = "Your extension is next to begin processing.";
                    status.OverAllProgress = 32;
                }
                else if (uploadQueue.Processing)
                {
                    status.CurrentMessage = "Your extension has started processing.";
                    status.OverAllProgress = 35;
                }
                else
                {
                    status.CurrentMessage = "Your extension is currently behind " + (itemCounter - 1) + " other extension waiting to be processed.";
                    status.OverAllProgress = 31;
                }

                StatusController.AddStatus(status);
            }
        }
    }
}