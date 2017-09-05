/*
 * Copyright (C) 2015 Hippo Seven
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package org.volq.volunteer.ui;

import android.app.AlertDialog;
import android.app.ProgressDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.util.Log;

import com.google.zxing.Result;
import com.hippo.scan.CaptureActivity;
import org.volq.volunteer.client.VltClient;

public class ScanActivity extends CaptureActivity {
    public static final String TAG = ScanActivity.class.getSimpleName();

    public static final String ACTION_VOLUNTEER_ACTION = "org.volq.volunteer.ui.ScanActivity.VOLUNTEER_ACTION";

    private VltClient mClient;
    private ProgressDialog mProgressDialog;
    private String mAction;

    private VltClient.OnVolunteerActionListener mListener = new VltClient.OnVolunteerActionListener() {
        @Override
        public void onSuccess() {
            mProgressDialog.dismiss();
            mProgressDialog = null;

            new AlertDialog.Builder(ScanActivity.this).setTitle("Congratulation")
                    .setMessage("Action OK").setOnCancelListener(new DialogInterface.OnCancelListener() {
                @Override
                public void onCancel(DialogInterface dialog) {
                    ScanActivity.this.finish();
                }
            }).show();
        }

        @Override
        public void onFailure(Exception e) {
            mProgressDialog.dismiss();
            mProgressDialog = null;
            e.printStackTrace();

            new AlertDialog.Builder(ScanActivity.this).setTitle("ERROR")
                    .setMessage(e.getMessage())
                    .setOnCancelListener(new DialogInterface.OnCancelListener() {
                        @Override
                        public void onCancel(DialogInterface dialog) {
                            resetDecodeThread();
                        }
                    }).show();
        }
    };

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        mAction = getIntent().getAction();
        mClient = VltClient.getInstance(this);
    }

    protected void onDecodeFinished(Result rawResult, Bitmap barcode, float scaleFactor) {
        if (ACTION_VOLUNTEER_ACTION.equals(mAction)) {
            barcode.recycle();
            barcode = null;

            String str = rawResult.getText();
            Log.d(TAG, "QR code: " + str);

            Action action = Action.readAction(str);
            if (action != null) {
                int index = str.indexOf(",");
                if (index == -1) {
                    new AlertDialog.Builder(this).setMessage(str).setOnCancelListener(new DialogInterface.OnCancelListener() {
                        @Override
                        public void onCancel(DialogInterface dialog) {
                            resetDecodeThread();
                        }
                    }).show();
                } else {
                    String id = str.substring(index + 1);
                    mProgressDialog = ProgressDialog.show(this, "Pleasr wait", "Acting");
                    mClient.volunteerAction(id, mListener);
                }

            } else if (str.startsWith("http://www.volq.org/views/activity.html?id=")) {
                String id = str.substring("http://www.volq.org/views/activity.html?id=".length());
                Intent intent = new Intent(this, DoingActivity.class);
                intent.putExtra(DoingActivity.KEY_ID, id);
                startActivity(intent);
                finish();
            } else {
                new AlertDialog.Builder(this).setMessage(str).setOnCancelListener(new DialogInterface.OnCancelListener() {
                    @Override
                    public void onCancel(DialogInterface dialog) {
                        resetDecodeThread();
                    }
                }).show();
            }
        }
    }

    private static class Action {
        private String mName;
        private String mId;

        private Action(String name, String id) {
            mName = name;
            mId = id;
        }

        public static Action readAction(String raw) {
            int index = raw.indexOf(',');
            if (index == -1) {
                return null;
            } else {
                String actionName = raw.substring(0, index);
                // TODO
                return new Action(raw.substring(0, index), raw.substring(index + 1));
            }
        }
    }

}
