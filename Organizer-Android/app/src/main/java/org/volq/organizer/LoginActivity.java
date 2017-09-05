package org.volq.organizer;

import android.app.ProgressDialog;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;

import com.hippo.widget.FloatLabelEditText;

import org.volq.organizer.account.VltAccount;
import org.volq.organizer.account.VltAccountStore;
import org.volq.organizer.client.VltClient;

public class LoginActivity extends AbsActionBarActivity implements View.OnClickListener{

    private VltClient mClient;
    private VltAccountStore mAccountStore;
    
    private FloatLabelEditText mTextEmail;
    private FloatLabelEditText mTextPassword;
    private Button mButtonConfirm;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        mClient = VltClient.getInstance(this);
        mAccountStore = VltAccountStore.getInstance(this);

        mTextEmail = (FloatLabelEditText) findViewById(R.id.email);
        mTextPassword = (FloatLabelEditText) findViewById(R.id.password);
        mButtonConfirm = (Button) findViewById(R.id.confirm);

        mButtonConfirm.setOnClickListener(this);

        // Try to get
        VltAccount account = mAccountStore.getStoreAccount();
        if (account != null) {
            ProgressDialog pd = new ProgressDialog(this);
            pd.setTitle("请稍后"); // TODO
            pd.setMessage("正在登录"); // TODO
            login(account.email, account.password, pd);
        }
    }
    
    private void login(String email, String password, final ProgressDialog dialog) {
        dialog.show();
        mClient.login(email, password, new VltClient.OnLoginListener() {
            @Override
            public void onSuccess(VltAccount account) {
                if (dialog != null) {
                    dialog.dismiss();
                }

                mAccountStore.removeAllAccount();
                mAccountStore.addVltAccount(account);

                Toast.makeText(LoginActivity.this, account.name + " 登录成功", Toast.LENGTH_SHORT).show();

                toMainActivity();
            }

            @Override
            public void onFailure(Exception e) {
                if (dialog != null) {
                    dialog.dismiss();
                }

                Toast.makeText(LoginActivity.this, e.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    @Override
    public void onClick(View v) {
        ProgressDialog pd = new ProgressDialog(this);
        pd.setTitle("请稍后"); // TODO
        pd.setMessage("正在登录"); // TODO
        login(mTextEmail.getText().toString(), mTextPassword.getText().toString(), pd);
    }

    private void toMainActivity() {
        finish();
        Intent intent = new Intent(this, MainActivity.class);
        startActivity(intent);
    }
}
