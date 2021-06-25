using System;
using System.Collections.Generic;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.DevTools.config_specific.import_export
{
    public delegate void PushSnapshotScreenForImport(string screenId);
    public static class ImportExportUtils{
        public static readonly string devToolsSnapshotKey = "devToolsSnapshot";
        public static readonly string activeScreenIdKey = "activeScreenId";
    }
    
    public class ImportController
    {
        // public ImportController(
        //     this._notifications,
        // this._pushSnapshotScreenForImport
        // );
        
        public readonly PushSnapshotScreenForImport _pushSnapshotScreenForImport;
        
        // public readonly NotificationService _notifications;
        public static readonly int repeatImportTimeBufferMs = 500;
        DateTime previousImportTime;

        public ImportController(NotificationsState of, Action<string> pushSnapshotScreenForImport)
        {
            this._pushSnapshotScreenForImport = _pushSnapshotScreenForImport;
        }

        public void importData(Dictionary<string, object> json) {
            var now = DateTime.Now;
            if (previousImportTime != null &&
                (now.Millisecond - previousImportTime.Millisecond)
                .abs() <
                repeatImportTimeBufferMs) {
                return;
            }
            previousImportTime = now;

            var isDevToolsSnapshot = json[ImportExportUtils.devToolsSnapshotKey];
            if (isDevToolsSnapshot == null) {
               Debug.Log("isDevToolsSnapshot == null");
                return;
            }
            
            var activeScreenId = json[ImportExportUtils.activeScreenIdKey];
            Globals.offlineDataJson = json;
            // _notifications.push(attemptingToImportMessage(activeScreenId));
            // _pushSnapshotScreenForImport(activeScreenId);
        }
        
    }
}