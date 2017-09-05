package org.volq.volunteer.util;

import android.content.Context;

import org.volq.volunteer.R;
import org.volq.volunteer.client.VltClient;

public class VltUtils {

    public static final int ROLE_VOLUNTEER = 0;
    public static final int ROLE_ORGANIZER = 1;
    public static final int ROLE_ORGANIZATION = 2;
    public static final int ROLE_ANONYMOUS = 3;

    public static final String ROLE_STR_VOLUNTEER = "Volunteer";
    public static final String ROLE_STR_ORGANIZER = "Organizer";
    public static final String ROLE_STR_ORGANIZATION = "Organization";

    public static final int[] ROLE_STRING_ID = {
        R.string.role_volunteer,
        R.string.role_organizer,
        R.string.role_organization,
        R.string.role_anonymous
    };

    public static String getRoleString(Context context, int role) {
        if (role >= 0 && role < ROLE_STRING_ID.length) {
            return context.getString(ROLE_STRING_ID[role]);
        } else {
            return null;
        }
    }

    public static String buildDoingUrl(String id) {
        return VltClient.API_HEADER + "/views/activity.html?id=" + id;
    }

    public static String getDoingStatusString(Context context, int status) {
        int resId;
        switch (status) {
            case 0:
                resId = R.string.doing_status_draft;
                break;
            case 1:
                resId = R.string.doing_status_activate;
                break;
            case 2:
                resId = R.string.doing_status_signin;
                break;
            case 3:
                resId = R.string.doing_status_overflow;
                break;
            case 4:
                resId = R.string.doing_status_ready;
                break;
            case 5:
                resId = R.string.doing_status_chekcin;
                break;
            case 6:
                resId = R.string.doing_status_signin_chekcin;
                break;
            case 7:
                resId = R.string.doing_status_closed;
                break;
            default:
            case 8:
                resId = R.string.doing_status_cancelled;
                break;
        }
        return context.getString(resId);
    }

}
