package org.volq.volunteer.data;

import android.os.Parcel;
import android.os.Parcelable;

import java.util.Date;

public class Badge implements Parcelable {

    public String name;
    public String picture;
    public String description;
    public Date grantedTime;
    public String[] requirementDescription;


    @Override
    public int describeContents() {
        return 0;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {

    }
}
