package org.volq.volunteer.data;

import android.content.Context;
import android.os.Parcel;
import android.os.Parcelable;

import com.hippo.util.MathUtils;

import org.volq.volunteer.R;

public class VolunteersRecord implements Parcelable {

    public static final int VOLUNTEER_STATUS_SIGNED_IN = 0;
    public static final int VOLUNTEER_STATUS_UNSIGNED_IN = 1;
    public static final int VOLUNTEER_STATUS_CHECKED_IN = 2;
    public static final int VOLUNTEER_STATUS_NOT_PARTICIPATE_IN = 3;
    public static final int VOLUNTEER_STATUS_QUIT = 4;
    public static final int VOLUNTEER_STATUS_COMPLETE = 5;
    public static final int VOLUNTEER_STATUS_ERROR = 6;
    public static final int VOLUNTEER_STATUS_KICKED_OUT = 7;
    
    private static final int[] VOLUNTEER_STATUS_STRINGS = {
        R.string.volunteer_status_signed_in,
        R.string.volunteer_status_unsigned_in,
        R.string.volunteer_status_checked_in,
        R.string.volunteer_status_not_participate_in,
        R.string.volunteer_status_quit,
        R.string.volunteer_status_complete,
        R.string.volunteer_status_error,
        R.string.volunteer_status_kicked_out
    };

    public String volunteerId;
    public String volunteerName;
    public int volunteerSex;
    public String volunteerEmail;
    public String volunteerPhoneNumber;
    public int volunteerStatus;

    @Override
    public int describeContents() {
        return 0;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {

    }

    public static boolean canOperate(int status) {
        return status < VOLUNTEER_STATUS_QUIT;
    }
    
    public static String getVolunteerStatusString(Context context, int status) {
        int resId = VOLUNTEER_STATUS_STRINGS[MathUtils.clamp(status, 0, VOLUNTEER_STATUS_KICKED_OUT)];
        return context.getString(resId);
    }
}
