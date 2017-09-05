package org.volq.organizer;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import org.volq.organizer.account.VltAccount;
import org.volq.organizer.account.VltAccountStore;
import org.volq.organizer.client.VltClient;


public class MainActivity extends AbsActionBarActivity implements View.OnClickListener {

    private VltClient mClient;
    private VltAccountStore mAccountStore;
    private VltAccount mAccount;
    
    private TextView mTextName;
    private Button mButton;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        mClient = VltClient.getInstance(this);
        mAccountStore = VltAccountStore.getInstance(this);

        // Check current account
        mAccount = mAccountStore.getCurAccount();
        if (mAccount == null) {
            errorToFinish("未登录账户");
            finish();
            return;
        }

        mTextName = (TextView) findViewById(R.id.name);
        mButton = (Button) findViewById(R.id.scan);

        mTextName.setText("你是 " + mAccount.name);
        mButton.setOnClickListener(this);
    }

    @Override
    public void onClick(View v) {
        toScanActivity();
    }

    private void toScanActivity() {
        Intent intent = new Intent(this, ScanActivity.class);
        startActivity(intent);
    }
}
