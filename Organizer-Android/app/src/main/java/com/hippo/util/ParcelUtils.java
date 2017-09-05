package com.hippo.util;

import android.os.Parcel;

public final class ParcelUtils {

    public static final String[] readStringArray(Parcel source) {
        int length = source.readInt();
        String[] result = new String[length];
        for (int i = 0; i < length; i++) {
            result[i] = source.readString();
        }
        return result;
    }

    public static final void writeStringArray(Parcel dest, String[] strings) {
        int length = strings == null ? 0 : strings.length;
        dest.writeInt(length);
        for (int i = 0; i < length; i++) {
            dest.writeString(strings[i]);
        }
    }

}
