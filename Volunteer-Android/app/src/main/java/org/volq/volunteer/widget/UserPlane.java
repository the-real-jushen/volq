package org.volq.volunteer.widget;

import android.annotation.TargetApi;
import android.app.AlertDialog;
import android.app.Dialog;
import android.content.Context;
import android.content.DialogInterface;
import android.os.Build;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.RadioGroup;
import android.widget.RelativeLayout;
import android.widget.TextView;
import android.widget.Toast;

import com.hippo.util.TextUtils;
import com.hippo.util.UiUtils;
import com.hippo.util.ViewUtils;
import com.squareup.picasso.Picasso;

import org.volq.volunteer.R;
import org.volq.volunteer.account.VltAccount;
import org.volq.volunteer.account.VltAccountStore;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.User;
import org.volq.volunteer.data.ValidateImage;
import org.volq.volunteer.data.Volunteer;
import org.volq.volunteer.util.VltUtils;

import java.util.HashSet;
import java.util.Set;

import de.hdodenhof.circleimageview.CircleImageView;

// TODO Use broadcast to tell activity, login and logout
public class UserPlane extends RelativeLayout implements View.OnClickListener {
    private VltAccountStore mAccountStore;
    private VltClient mClient;

    private CircleImageView mUserAvatar;

    private Button mLogin;
    private Button mRegister;
    private TextView mUserName;
    private TextView mUserCategory;
    private TextView mUserLevel;
    private Button mLogout;

    private Dialog mLoginDialog;
    private TextView mError;
    private EditText mUsername;
    private EditText mPassword;

    private Dialog mRegisterDialog;
    private EditText mRegisterEmail;
    private EditText mRegisterPhone;
    private EditText mRegisterPassword;
    private EditText mRegisterName;
    private RadioGroup mGender;
    private RadioGroup mRole;
    private EditText mValidateText;
    private ImageView mValidateImage;

    private ValidateImage mValidate;

    private ProgressBar mProgressBar;

    private static final Set<UserPlane> mUserPlaneSet;

    static {
        mUserPlaneSet = new HashSet<UserPlane>();
    }

    public UserPlane(Context context) {
        super(context);
        init();
    }

    public UserPlane(Context context, AttributeSet attrs) {
        super(context, attrs);
        init();
    }

    public UserPlane(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        init();
    }

    @TargetApi(Build.VERSION_CODES.LOLLIPOP)
    public UserPlane(Context context, AttributeSet attrs, int defStyleAttr, int defStyleRes) {
        super(context, attrs, defStyleAttr, defStyleRes);
        init();
    }

    private VltClient.OnLoginWithInfoListener mLoginListener = new VltClient.OnLoginWithInfoListener() {
        @Override
        public void onSuccess(VltAccount account) {
            mAccountStore.addVltAccount(account);
            mAccountStore.switchAccount(account.userId);
            UserPlane.allSetUser();
        }

        @Override
        public void onFailure(Exception e) {
            UserPlane.allResetUser();
            try {
                mError.setText(e.getMessage());
                ViewUtils.setVisibility(mError, View.VISIBLE);
                mLoginDialog.show();
            } catch (Throwable t) {
                // If activity is closed, may throw BadTokenException
            }
        }
    };

    private VltClient.OnRegisterListener mRegisterListener = new VltClient.OnRegisterListener() {
        @Override
        public void onSuccess() {
            mUsername.setText(mRegisterEmail.getText().toString());
            mPassword.setText(mRegisterPassword.getText().toString());
            login(mUsername.getText().toString(),
                    mPassword.getText().toString());
            Toast.makeText(getContext(), "注册成功，开始登录", Toast.LENGTH_SHORT).show(); // TODO
        }

        @Override
        public void onFailure(Exception e) {
            Toast.makeText(getContext(), e.getMessage(), Toast.LENGTH_SHORT).show();

            mValidateText.setText(TextUtils.STRING_EMPTY);
            mValidateImage.setImageDrawable(null);
            getValidateImage();
            mRegisterDialog.show();
        }
    };

    private void login(String username, String password) {
        mClient.loginWithInfo(username, password, mLoginListener);
    }

    private void register(String email, String phone, String password,
            String name,String gender, String role, String validateId, String validateCode) {
        mClient.register(email, phone, password, name, gender, role,
                TextUtils.STRING_EMPTY, validateId, validateCode, mRegisterListener);
    }

    private void getValidateImage() {
        mClient.getValidateImage(new VltClient.OnGetValidateImageListener() {
            @Override
            public void onSuccess(ValidateImage validateImage) {
                mValidate = validateImage;
                mValidateImage.setImageBitmap(validateImage.image);
            }

            @Override
            public void onFailure(Exception e) {
                // TODO
            }
        });
    }


    private Dialog createLoginDialog() {
        LayoutInflater inflater = LayoutInflater.from(getContext());
        View view = inflater.inflate(R.layout.dialog_login, null);
        mError = (TextView) view.findViewById(R.id.text_error);
        mUsername = (EditText) view.findViewById(R.id.username);
        mPassword = (EditText) view.findViewById(R.id.password);
        return new AlertDialog.Builder(getContext()).setTitle(R.string.login)
                .setView(view).setPositiveButton(R.string.login, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        // TODO Should check input in empty
                        setInProgress();
                        login(mUsername.getText().toString(),
                                mPassword.getText().toString());
                    }
                }).setNegativeButton(android.R.string.cancel, null).create();
    }

    private AlertDialog createRegisterDialog() {
        LayoutInflater inflater = LayoutInflater.from(getContext());
        View view = inflater.inflate(R.layout.dialog_register, null);
        mRegisterEmail = (EditText) view.findViewById(R.id.email);
        mRegisterPhone = (EditText) view.findViewById(R.id.phone);
        mRegisterPassword = (EditText) view.findViewById(R.id.password);
        mRegisterName = (EditText) view.findViewById(R.id.name);
        mGender = (RadioGroup) view.findViewById(R.id.gender);
        mRole = (RadioGroup) view.findViewById(R.id.role);
        mValidateText = (EditText) view.findViewById(R.id.validateText);
        mValidateImage = (ImageView) view.findViewById(R.id.validateImage);
        final AlertDialog dialog = new AlertDialog.Builder(getContext()).setTitle(R.string.register)
                .setView(view).setPositiveButton(R.string.register, null)
                .setNegativeButton(android.R.string.cancel, null).create();

        dialog.setOnShowListener(new DialogInterface.OnShowListener() {
            @Override
            public void onShow(DialogInterface d) {
                dialog.getButton(AlertDialog.BUTTON_POSITIVE).setOnClickListener(
                        new OnClickListener() {
                            @Override
                            public void onClick(View v) {
                                if (mValidate == null) {
                                    return;
                                } else {
                                    setInProgress();
                                    register(mRegisterEmail.getText().toString(),
                                            mRegisterPhone.getText().toString(),
                                            mRegisterPassword.getText().toString(),
                                            mRegisterName.getText().toString(),
                                            getValueForPosition(mGender.indexOfChild(mGender.findViewById(mGender.getCheckedRadioButtonId()))),
                                            getValueForPosition(mRole.indexOfChild(mRole.findViewById(mRole.getCheckedRadioButtonId()))),
                                            mValidate.id,
                                            mValidateText.getText().toString());
                                    mValidate = null;
                                    dialog.dismiss();
                                }
                            }
                        }
                );
            }
        });

        return dialog;
    }

    private String getValueForPosition(int position) {
        return Character.toString((char) ('a' + position));
    }

    private void init() {
        mAccountStore = VltAccountStore.getInstance(getContext());
        mClient = VltClient.getInstance(getContext());
        mLoginDialog = createLoginDialog();
        mRegisterDialog = createRegisterDialog();
        setPadding(UiUtils.dp2pix(16), UiUtils.dp2pix(16),
                UiUtils.dp2pix(16), UiUtils.dp2pix(16));
        LayoutInflater.from(getContext()).inflate(R.layout.user_plane, this);

        mUserAvatar = (CircleImageView) findViewById(R.id.user_avatar);
        mLogin = (Button) findViewById(R.id.btn_login);
        mRegister = (Button) findViewById(R.id.btn_register);
        mProgressBar = (ProgressBar) findViewById(R.id.progressBar);
        mUserName = (TextView) findViewById(R.id.user_name);
        mUserCategory = (TextView) findViewById(R.id.user_category);
        mUserLevel = (TextView) findViewById(R.id.user_level);
        mLogout = (Button) findViewById(R.id.btn_logout);

        mLogin.setOnClickListener(this);
        mRegister.setOnClickListener(this);
        mLogout.setOnClickListener(this);

        // Set user if possible
        VltAccount account = mAccountStore.getCurAccount();
        if (account != null) {
            setUser(account.user);
        } else {
            resetUser();
        }
    }

    public void setDefaultAvatat() {
        Picasso.with(getContext()).load(R.drawable.ic_default_avatar).into(mUserAvatar);
    }

    @Override
    public void onClick(View v) {
        if (v == mLogin) {
            if (!mLoginDialog.isShowing()) {
                // Clear EditText
                mUsername.setText(TextUtils.STRING_EMPTY);
                mPassword.setText(TextUtils.STRING_EMPTY);
                ViewUtils.setVisibility(mError, View.GONE);
                mLoginDialog.show();
            }
        } else if (v == mRegister) {
            if (!mRegisterDialog.isShowing()) {
                mRegisterEmail.setText(TextUtils.STRING_EMPTY);
                mRegisterPhone.setText(TextUtils.STRING_EMPTY);
                mRegisterPassword.setText(TextUtils.STRING_EMPTY);
                mRegisterName.setText(TextUtils.STRING_EMPTY);
                mGender.check(-1);
                mRole.check(-1);
                mValidateText.setText(TextUtils.STRING_EMPTY);
                mValidateImage.setImageDrawable(null);
                mRegisterEmail.requestFocus();
                mRegisterDialog.show();

                getValidateImage();
            }
        } else if (v == mLogout) {
            mAccountStore.removeAllAccount();
            UserPlane.allResetUser();
        }
    }

    protected void onAttachedToWindow() {
        super.onAttachedToWindow();
        mUserPlaneSet.add(this);
    }

    protected void onDetachedFromWindow() {
        super.onDetachedFromWindow();
        mUserPlaneSet.remove(this);
    }

    public static void allResetUser() {
        for (UserPlane userPlane : mUserPlaneSet) {
            userPlane.resetUser();
        }
    }

    public static void allSetInProgress() {
        for (UserPlane userPlane : mUserPlaneSet) {
            userPlane.setInProgress();
        }
    }

    public static void allSetUser() {
        VltAccountStore accountStore = null;
        VltAccount account = null;

        for (UserPlane userPlane : mUserPlaneSet) {
            if (accountStore == null) {
                accountStore = VltAccountStore.getInstance(userPlane.getContext());
                account = accountStore.getCurAccount();
                if (account == null) {
                    break;
                }
            }
            userPlane.setUser(account.user);
        }
    }


    private void resetUser() {
        ViewUtils.setVisibility(mLogin, View.VISIBLE);
        ViewUtils.setVisibility(mRegister, View.VISIBLE);
        ViewUtils.setVisibility(mProgressBar, View.GONE);
        ViewUtils.setVisibility(mUserName, View.GONE);
        ViewUtils.setVisibility(mUserCategory, View.GONE);
        ViewUtils.setVisibility(mUserLevel, View.GONE);
        ViewUtils.setVisibility(mLogout, View.GONE);
        setDefaultAvatat();
    }

    private void setInProgress() {
        ViewUtils.setVisibility(mLogin, View.GONE);
        ViewUtils.setVisibility(mRegister, View.GONE);
        ViewUtils.setVisibility(mProgressBar, View.VISIBLE);
        ViewUtils.setVisibility(mUserName, View.GONE);
        ViewUtils.setVisibility(mUserCategory, View.GONE);
        ViewUtils.setVisibility(mUserLevel, View.GONE);
        ViewUtils.setVisibility(mLogout, View.GONE);
        setDefaultAvatat();
    }

    private void setUser(User user) {
        if (user == null) {
            return;
        }

        ViewUtils.setVisibility(mLogin, View.GONE);
        ViewUtils.setVisibility(mRegister, View.GONE);
        ViewUtils.setVisibility(mProgressBar, View.GONE);
        ViewUtils.setVisibility(mUserName, View.VISIBLE);
        ViewUtils.setVisibility(mUserCategory, View.VISIBLE);
        ViewUtils.setVisibility(mUserLevel, View.VISIBLE);
        ViewUtils.setVisibility(mLogout, View.VISIBLE);

        Picasso.with(getContext()).load(user.avatar).into(mUserAvatar);
        mUserName.setText(user.name);
        mUserCategory.setText(VltUtils.getRoleString(getContext(), user.role));

        if (user instanceof Volunteer) {
            Volunteer volunteer = (Volunteer) user;
            mUserLevel.setText(volunteer.levelName);
        }
    }

}
