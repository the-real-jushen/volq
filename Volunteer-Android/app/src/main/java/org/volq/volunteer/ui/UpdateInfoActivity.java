package org.volq.volunteer.ui;

import android.app.Activity;
import android.app.ProgressDialog;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import org.volq.volunteer.R;
import org.volq.volunteer.account.VltAccount;
import org.volq.volunteer.account.VltAccountStore;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.User;
import org.volq.volunteer.widget.UserPlane;

public class UpdateInfoActivity extends AbsActionBarActivity implements View.OnClickListener,
        VltClient.OnUpdateInfoListener {

    public static final int TYPE_EMAIL = 0;
    public static final int TYPE_PHONE = 1;
    public static final int TYPE_AFFILIATION = 2;
    public static final int TYPE_DESCRIPTION = 3;

    public static final String KEY_TYPE = "item_name";
    public static final String KEY_OLD_VALUE = "old_value";

    private int mType;
    private String mItemName;
    private String mOldValue;

    private TextView mTextItemName;
    private EditText mEditValue;
    private Button mButtonCancel;
    private Button mButtonConfirm;

    private ProgressDialog mProgressDialog;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_update_info);

        Intent intent = getIntent();
        mType = intent.getIntExtra(KEY_TYPE, -1);
        mOldValue = intent.getStringExtra(KEY_OLD_VALUE);

        if (mType == -1) {
            finish();
            return;
        }

        mTextItemName = (TextView) findViewById(R.id.item_name);
        mEditValue = (EditText) findViewById(R.id.new_item);
        mButtonCancel = (Button) findViewById(R.id.cancel);
        mButtonConfirm = (Button) findViewById(R.id.confirm);

        mItemName = findItemName(mType);

        mTextItemName.setText(mItemName);
        mEditValue.setHint("New value"); // TODO
        mEditValue.setText(mOldValue);
        mButtonCancel.setOnClickListener(this);
        mButtonConfirm.setOnClickListener(this);
    }

    private String findItemName(int type) {
        int id;
        switch (type) {
            default:
            case TYPE_EMAIL:
                id = R.string.email;
                break;
            case TYPE_PHONE:
                id = R.string.phone;
                break;
            case TYPE_AFFILIATION:
                id = R.string.affiliation;
                break;
            case TYPE_DESCRIPTION:
                id = R.string.description;
                break;
        }
        return getString(id);
    }


    @Override
    public void onClick(View v) {
        if (v == mButtonCancel) {
            finish();
        } else if (v == mButtonConfirm) {
            mProgressDialog = ProgressDialog.show(this, "请稍后", "正在发送请求");
            VltClient.getInstance(this).updateInfo(mType, mEditValue.getText().toString(), this);
        }
    }

    @Override
    public void onSuccess() {
        // Update current user info
        final VltAccount account = VltAccountStore.getInstance(this).getCurAccount();
        if (account == null) {
            closeDialog();
            Toast.makeText(this, "操作成功，刷新失败", Toast.LENGTH_SHORT).show();
        } else {
            VltClient.getInstance(this).getUserWithInfo(account.userId, new VltClient.OnGetUserWithInfoListener() {
                @Override
                public void onSuccess(User user) {
                    account.user = user;
                    UserPlane.allSetUser();
                    closeDialog();
                    Toast.makeText(UpdateInfoActivity.this, "上传成功", Toast.LENGTH_SHORT).show();

                    Intent intent = new Intent();
                    intent.putExtra(UserStatusActivity.KEY_USER, user);
                    setResult(Activity.RESULT_OK, intent);
                    finish();
                }

                @Override
                public void onFailure(Exception e) {
                    closeDialog();
                    Toast.makeText(UpdateInfoActivity.this, "上传成功，刷新失败", Toast.LENGTH_SHORT).show();
                }
            });
        }
    }

    @Override
    public void onFailure(Exception e) {
        closeDialog();
        Toast.makeText(this, "操作失败", Toast.LENGTH_SHORT).show();
    }

    private void closeDialog() {
        if (mProgressDialog != null) {
            mProgressDialog.dismiss();
            mProgressDialog = null;
        }
    }
}
