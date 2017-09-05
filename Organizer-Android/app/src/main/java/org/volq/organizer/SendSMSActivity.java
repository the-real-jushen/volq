package org.volq.organizer;

import android.content.Intent;
import android.os.Bundle;

import com.hippo.util.Log;

import org.volq.organizer.account.VltAccount;
import org.volq.organizer.account.VltAccountStore;
import org.volq.organizer.client.VltClient;
import org.volq.organizer.data.ActivitySendSMS;

public class SendSMSActivity extends AbsActionBarActivity {

    public static final String KEY_ACTIVITY_VALIDATE_ID = "action_validate_id";

    private String mActionValidateId;
    
    private VltClient mClient;
    private VltAccountStore mAccountStore;
    private VltAccount mAccount;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        mClient = VltClient.getInstance(this);
        mAccountStore = VltAccountStore.getInstance(this);

        Intent intent = getIntent();
        mActionValidateId = intent.getStringExtra(KEY_ACTIVITY_VALIDATE_ID);
        if (mActionValidateId == null) {
            errorToFinish("无效的参数");
            finish();
            return;
        }

        // Check current account
        mAccount = mAccountStore.getCurAccount();
        if (mAccount == null) {
            errorToFinish("未登录账户");
            finish();
            return;
        }


    }
}
