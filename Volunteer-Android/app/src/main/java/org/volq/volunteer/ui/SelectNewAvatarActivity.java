package org.volq.volunteer.ui;

import android.app.Activity;
import android.app.ProgressDialog;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.provider.MediaStore;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.Toast;

import com.squareup.picasso.Picasso;

import org.volq.volunteer.R;
import org.volq.volunteer.account.VltAccount;
import org.volq.volunteer.account.VltAccountStore;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.User;
import org.volq.volunteer.widget.UserPlane;

public class SelectNewAvatarActivity extends AbsActionBarActivity implements View.OnClickListener,
        VltClient.OnUploadAvatarListener {

    private static final int SELECT_AVATAT_CODE = 233;

    private User mUser;
    private String mImagePath;

    private ImageView mAvatar;
    private Button mButtonSelect;
    private Button mButtonConfirm;

    private ProgressDialog mProgressDialog;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        VltAccount account = VltAccountStore.getInstance(this).getCurAccount();
        if (account != null) {
            mUser = account.user;
        }
        if (mUser == null) {
            errorToFinish(getString(R.string.mesg_current_account_invaild));
            return;
        }

        setContentView(R.layout.activity_select_avatar);

        mAvatar = (ImageView) findViewById(R.id.avatar);
        mButtonSelect = (Button) findViewById(R.id.select);
        mButtonConfirm = (Button) findViewById(R.id.confirm);

        Picasso.with(this).load(mUser.avatar).into(mAvatar);
        mButtonSelect.setOnClickListener(this);
        mButtonConfirm.setOnClickListener(this);
    }

    @Override
    public void onClick(View v) {
        if (v == mButtonSelect) {
            Intent intent = new Intent(Intent.ACTION_PICK, MediaStore.Images.Media.EXTERNAL_CONTENT_URI);
            startActivityForResult(intent, SELECT_AVATAT_CODE);
        } else if (v == mButtonConfirm) {
            if (mImagePath != null) {
                mProgressDialog = ProgressDialog.show(this, "请稍后", "正在上传头像");
                VltClient.getInstance(this).uploadAvatar(mImagePath, this);
            } else {
                Toast.makeText(this, "未选择新头像", Toast.LENGTH_SHORT).show(); // TODO
            }
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == SELECT_AVATAT_CODE && resultCode == RESULT_OK) {
            Uri selectedImage = data.getData();
            String[] filePathColumn = { MediaStore.Images.Media.DATA };

            Cursor cursor = getContentResolver().query(selectedImage,
                    filePathColumn, null, null, null);
            cursor.moveToFirst();

            int columnIndex = cursor.getColumnIndex(filePathColumn[0]);
            String imagePath = cursor.getString(columnIndex);
            cursor.close();

            mImagePath = imagePath;
            Picasso.with(this).load("file:" + mImagePath).into(mAvatar);
        }
    }

    @Override
    public void onSuccess() {
        // Update current user info
        final VltAccount account = VltAccountStore.getInstance(this).getCurAccount();
        if (account == null) {
            closeDialog();
            Toast.makeText(this, "上传成功，刷新失败", Toast.LENGTH_SHORT).show();
        } else {
            VltClient.getInstance(this).getUserWithInfo(account.userId, new VltClient.OnGetUserWithInfoListener() {
                @Override
                public void onSuccess(User user) {
                    account.user = user;
                    UserPlane.allSetUser();
                    closeDialog();
                    Toast.makeText(SelectNewAvatarActivity.this, "上传成功", Toast.LENGTH_SHORT).show();

                    Intent intent = new Intent();
                    intent.putExtra(UserStatusActivity.KEY_USER, user);
                    setResult(Activity.RESULT_OK, intent);
                    finish();
                }

                @Override
                public void onFailure(Exception e) {
                    closeDialog();
                    Toast.makeText(SelectNewAvatarActivity.this, "上传成功，刷新失败", Toast.LENGTH_SHORT).show();
                }
            });
        }
    }

    @Override
    public void onFailure(Exception e) {
        e.printStackTrace();
        closeDialog();
        Toast.makeText(this, "上传失败", Toast.LENGTH_SHORT).show();
    }

    private void closeDialog() {
        if (mProgressDialog != null) {
            mProgressDialog.dismiss();
            mProgressDialog = null;
        }
    }
}
