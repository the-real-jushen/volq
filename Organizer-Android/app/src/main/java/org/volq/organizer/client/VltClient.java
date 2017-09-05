package org.volq.organizer.client;

import android.content.Context;
import android.os.AsyncTask;

import com.alibaba.fastjson.JSON;
import com.hippo.network.ResponseCodeException;
import com.hippo.network.UrlBuilder;
import com.hippo.util.Log;
import com.hippo.util.Utils;

import org.json.JSONException;
import org.json.JSONObject;
import org.volq.organizer.account.VltAccount;
import org.volq.organizer.account.VltAccountStore;
import org.volq.organizer.data.ActivitySendSMS;
import org.volq.organizer.network.VltHttpHelper;

import java.util.HashMap;
import java.util.Iterator;

public final class VltClient {

    public static final String API_HEADER = "http://www.volq.org";

    private static final String API_LOGIN = "/api/user/login";
    private static final String API_ACTIVITY_SEND_SMS_QRCODE = "/api/organizer/activitysendsmsqrcode";

    private static final String API_KEY_ID = "id";

    private Context mContext;
    private VltAccountStore mAccountStore;

    private static VltClient sInstance;

    public static VltClient getInstance(Context context) {
        if (sInstance == null) {
            sInstance = new VltClient(context);
        }
        return sInstance;
    }

    private VltClient(Context context) {
        mContext = context;
        mAccountStore = VltAccountStore.getInstance(context);
    }

    private void doBgJob(BgJobHelper bjh) {
        Utils.execute(false, new AsyncTask<Object, Void, BgJobHelper>() {
            @Override
            protected BgJobHelper doInBackground(Object... params) {
                BgJobHelper bjh = (BgJobHelper) params[0];
                bjh.doInBackground();
                return bjh;
            }

            @Override
            protected void onPostExecute(BgJobHelper bjh) {
                bjh.onPostExecute();
            }
        }, bjh);
    }

    private interface BgJobHelper {
        public void doInBackground();

        public void onPostExecute();
    }

    public static interface VltClientListener {
        public void onFailure(Exception e);
    }

    private abstract class SimpleBgJobHelper implements BgJobHelper {

        private VltClientListener mListener;
        private Exception mException;

        public SimpleBgJobHelper(VltClientListener listener) {
            mListener = listener;
        }

        @Override
        public void doInBackground() {
            try {
                doBgJob();
            } catch (Exception e) {
                mException = e;
            }
        }

        @Override
        public void onPostExecute() {
            if (mListener != null) {
                if (mException == null) {
                    doSuccessCallback();
                } else {
                    mListener.onFailure(mException);
                }
            }
        }

        public abstract void doBgJob() throws Exception;

        public abstract void doSuccessCallback();
    }

    private void checkHttpError(VltHttpHelper vhh, String body) throws ResponseCodeException {
        final int responseCode = vhh.getResponseCode();

        if (responseCode >= 400) {
            if (body.startsWith("{")) {
                try {
                    JSONObject jo = new JSONObject(body);
                    throw new ResponseCodeException(responseCode, jo.getString("Message"));
                } catch (JSONException e) {
                    throw new ResponseCodeException(responseCode);
                }

            } else if (body.length() > 0 && !body.startsWith("<")) {
                throw new ResponseCodeException(responseCode, body);
            } else {
                throw new ResponseCodeException(responseCode);
            }
        }
    }

    // Login

    public VltAccount doLogin(String email, String password)
            throws Exception {
        JSONObject jo = new JSONObject();
        jo.put("email", email);
        jo.put("password", password);
        VltHttpHelper hh = new VltHttpHelper(mContext);
        String result = hh.postJson(API_HEADER + API_LOGIN, jo);

        checkHttpError(hh, result);

        JSONObject reJo = new JSONObject(result);

        String status = reJo.getString("status");
        if ("OK".equals(status)) {
            VltAccount account = new VltAccount();
            account.email = email;
            account.password = password;
            account.name = reJo.getString("name");
            account.role = reJo.getString("role");
            account.userId = reJo.getString("userId");
            account.token = reJo.getString("token");
            return account;
        } else if ("ERROR".equals(status)) {
            throw new VltException(reJo.getString("message"));
        } else {
            throw new VltException("Unknown status: " + status);
        }
    }

    public abstract static class OnLoginListener implements VltClientListener {
        public abstract void onSuccess(VltAccount account);
    }

    private class LoginHelper extends SimpleBgJobHelper {

        private String mEmail;
        private String mPassword;
        private OnLoginListener mListener;
        private VltAccount mAccount;

        public LoginHelper(String email, String password, OnLoginListener listener) {
            super(listener);
            mEmail = email;
            mPassword = password;
            mListener = listener;
        }

        @Override
        public void doBgJob() throws Exception {
            mAccount = doLogin(mEmail, mPassword);
        }

        @Override
        public void doSuccessCallback() {
            mListener.onSuccess(mAccount);
        }
    }

    public void login(String email, String password, OnLoginListener l) {
        doBgJob(new LoginHelper(email, password, l));
    }

    // doSendActivitySMS

    private ActivitySendSMS parseActivitySendSMS(String str) throws JSONException {
        JSONObject jo = new JSONObject(str);
        ActivitySendSMS ass = new ActivitySendSMS();
        ass.setActivityId(jo.getString("activityId"));
        ass.setContent(jo.getString("content"));

        JSONObject vnp = jo.getJSONObject("volunteerNameAndPhoneNumber");
        HashMap<String, String> map = new HashMap<>();
        Iterator<String> iter = vnp.keys();
        while (iter.hasNext()) {
            String key = iter.next();
            map.put(key, jo.getString(key));
        }
        ass.setVolunteerNameAndPhoneNumber(map);

        return ass;
    }

    private ActivitySendSMS doActivitySendSMS(String id)
            throws Exception {
        UrlBuilder ub = new UrlBuilder(API_HEADER + API_ACTIVITY_SEND_SMS_QRCODE);
        ub.addQuery(API_KEY_ID, id);

        VltHttpHelper hh = new VltHttpHelper(mContext);
        String body = hh.get(ub.build());

        checkHttpError(hh, body);

        Log.d(body);

        return JSON.parseObject(body, ActivitySendSMS.class);
    }

    public abstract static class OnSendActivitySMSListener implements VltClientListener {
        public abstract void onSuccess(ActivitySendSMS activitySendSMS);
    }

    private class ActivitySendSMSHelper extends SimpleBgJobHelper {

        private String mId;
        private OnSendActivitySMSListener mListener;
        private ActivitySendSMS mActivitySendSMS;

        public ActivitySendSMSHelper(String id, OnSendActivitySMSListener listener) {
            super(listener);
            mId = id;
            mListener = listener;
        }

        @Override
        public void doBgJob() throws Exception {
            mActivitySendSMS = doActivitySendSMS(mId);
        }

        @Override
        public void doSuccessCallback() {
            mListener.onSuccess(mActivitySendSMS);
        }
    }

    public void activitySendSMS(String id, OnSendActivitySMSListener l) {
        doBgJob(new ActivitySendSMSHelper(id, l));
    }


    public class VltException extends Exception {
        private static final long serialVersionUID = 1L;

        public VltException() {
            super("VltException");
        }

        public VltException(String detailMessage) {
            super(detailMessage);
        }
    }
}
