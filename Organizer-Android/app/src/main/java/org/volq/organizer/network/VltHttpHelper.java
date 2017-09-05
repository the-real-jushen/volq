package org.volq.organizer.network;

import android.content.Context;
import android.os.Build;

import com.hippo.network.HttpHelper;

import org.volq.organizer.account.VltAccount;
import org.volq.organizer.account.VltAccountStore;

import java.net.HttpURLConnection;
import java.net.URL;

public class VltHttpHelper extends HttpHelper {
    private Context mContext;

    public VltHttpHelper(Context context) {
        mContext = context;
    }

    @Override
    protected void onBeforeConnect(HttpURLConnection conn) {
        super.onBeforeConnect(conn);
        conn.setRequestProperty("Client", "Volunteer-Android");
        conn.setRequestProperty("Android-Brand", Build.BRAND);
        conn.setRequestProperty("Android-Manufacturer", Build.MANUFACTURER);
        conn.setRequestProperty("Android-Model", Build.MODEL);
        conn.setRequestProperty("Android-Product", Build.PRODUCT);
        conn.setRequestProperty("Android-Device", Build.DEVICE);
        conn.setRequestProperty("Android-SDK", Integer.toString(Build.VERSION.SDK_INT));
    }

    @Override
    protected String getCookie(URL url) {
        VltAccount account = VltAccountStore.getInstance(mContext).getCurAccount();
        if (account != null && account.userId != null && account.token != null) {
            StringBuilder sb = new StringBuilder();
            sb.append("userid=").append(account.userId)
                    .append("; token=").append(account.token);
            return sb.toString();
        } else {
            return null;
        }
    }

    @Override
    protected void storeCookie(URL url, String value) {
        // Empty
    }

}
