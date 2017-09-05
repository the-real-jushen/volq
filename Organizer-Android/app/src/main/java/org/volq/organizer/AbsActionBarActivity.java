package org.volq.organizer;

import android.app.AlertDialog;
import android.content.DialogInterface;
import android.support.v7.app.ActionBarActivity;
import android.view.View;
import android.view.Window;

public abstract class AbsActionBarActivity extends ActionBarActivity {
    public boolean post(Runnable action) {
        Window w = getWindow();
        if (w != null) {
            View v = w.getDecorView();
            if (v != null) {
                return v.post(action);
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

    public boolean postDelayed(Runnable action, long delayMillis) {
        Window w = getWindow();
        if (w != null) {
            View v = w.getDecorView();
            if (v != null) {
                return v.postDelayed(action, delayMillis);
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

    public void errorToFinish(String emesg) {
        new AlertDialog.Builder(this).setTitle("Error") // TODO
                .setMessage(emesg)
                .setOnCancelListener(new DialogInterface.OnCancelListener() {
                    @Override
                    public void onCancel(DialogInterface dialog) {
                        finish();
                    }
                }).show();
    }

}
