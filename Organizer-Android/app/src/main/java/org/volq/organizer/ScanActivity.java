package org.volq.organizer;

import android.app.AlertDialog;
import android.app.ProgressDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Bitmap;
import android.net.Uri;
import android.os.Bundle;

import com.google.zxing.Result;
import com.hippo.scan.CaptureActivity;
import com.hippo.util.Log;
import com.hippo.util.Utils;

import org.volq.organizer.client.VltClient;
import org.volq.organizer.data.ActivitySendSMS;

import java.util.Map;

public class ScanActivity extends CaptureActivity {

    private VltClient mClient;
    private ProgressDialog mProgressDialog;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        mClient = VltClient.getInstance(this);
    }

    protected void onDecodeFinished(Result rawResult, Bitmap barcode, float scaleFactor) {
        String str = rawResult.getText();
        if (str.startsWith("activitysendsms,")) {
            String actionValidateId = str.substring("activitysendsms,".length());
            Log.d("actionValidateId = " + actionValidateId);
            getSendSMSInfo(actionValidateId);
        } else {
            new AlertDialog.Builder(this).setMessage(rawResult.getText()).setOnCancelListener(new DialogInterface.OnCancelListener() {
                @Override
                public void onCancel(DialogInterface dialog) {
                    resetDecodeThread();
                }
            }).show();
        }
    }

    private void getSendSMSInfo(String actionValidateId) {
        mProgressDialog = ProgressDialog.show(this, "请稍后", "正在获取信息");
        mClient.activitySendSMS(actionValidateId, new VltClient.OnSendActivitySMSListener() {
            @Override
            public void onSuccess(ActivitySendSMS activitySendSMS) {
                dismissProgressDialog();

                Map<String, String> map = activitySendSMS.getVolunteerNameAndPhoneNumber();
                int size = map.size();
                String[] phones = new String[size];
                int i = 0;
                for (String key : map.keySet()) {
                    phones[i] = map.get(key);
                    i++;
                }

                setSMS(activitySendSMS.getContent(), phones);

                finish();
            }

            @Override
            public void onFailure(Exception e) {
                ongetSendSMSInfoFailure();

            }
        });
    }

    private void ongetSendSMSInfoFailure() {
        dismissProgressDialog();
        new AlertDialog.Builder(this).setTitle("错误").setMessage("获取短信信息错误").setOnCancelListener(new DialogInterface.OnCancelListener() {
            @Override
            public void onCancel(DialogInterface dialog) {
                resetDecodeThread();
            }
        }).show();
    }

    private void dismissProgressDialog() {
        if (mProgressDialog != null) {
            mProgressDialog.dismiss();
            mProgressDialog = null;
        }
    }

    private void setSMS(String sms, String[] phones) {
        Intent smsIntent = new Intent(Intent.ACTION_SENDTO, Uri.parse("smsto:" + Utils.join(phones, ";")));
        smsIntent.putExtra("sms_body", sms);
        startActivity(smsIntent);
    }

}
