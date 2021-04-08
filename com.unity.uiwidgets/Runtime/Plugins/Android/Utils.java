package com.unity.uiwidgets.plugin;
import android.view.View;
import android.view.WindowManager;

import com.unity3d.player.UnityPlayer;

public class Utils {
    public static final String TAG = "UIWidgets";

    public static void SetStatusBarState(final boolean show) {
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                if (show) {
                    UnityPlayer.currentActivity.getWindow().clearFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN);
                } else {
                    UnityPlayer.currentActivity.getWindow().addFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN);
                }
            }
        });
    }
}
