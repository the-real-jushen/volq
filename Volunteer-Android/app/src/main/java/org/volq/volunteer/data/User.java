package org.volq.volunteer.data;

import android.os.Parcel;
import android.os.Parcelable;
import android.support.annotation.NonNull;
import android.support.annotation.Nullable;

import com.hippo.util.ArrayUtils;
import com.hippo.util.Coordinate;
import com.hippo.util.ParcelUtils;

public class User implements Parcelable {

    public String id;
    public String name;
    public String avatar;
    public int role;

    public int sex;
    public boolean isEmailVerified;
    public String phoneNumber;

    public String email;
    public String description;
    public @NonNull String[] affiliation;
    public String location;
    public @Nullable Coordinate coordinate;


    public static final Creator<User> CREATOR = new Creator<User>() {
        public User createFromParcel(Parcel source) {
            return new User(source);
        }

        public User[] newArray(int size) {
            return new User[size];
        }
    };

    public User() {
        // Empty
    }


    public User(Parcel source) {
        id = source.readString();
        name = source.readString();
        avatar = source.readString();
        role = source.readInt();
        email = source.readString();
        description = source.readString();
        affiliation = ParcelUtils.readStringArray(source);
        location = source.readString();
        coordinate = source.readParcelable(Coordinate.class.getClassLoader());
    }

    public User(User user) {
        id = user.id;
        name = user.name;
        avatar = user.avatar;
        role = user.role;
        email = user.email;
        description = user.description;
        affiliation = ArrayUtils.copyOf(user.affiliation);
        location = user.location;
        if (user.coordinate == null) {
            coordinate = null;
        } else {
            coordinate = new Coordinate(user.coordinate);
        }
    }

    @Override
    public int describeContents() {
        return 0;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {
        dest.writeString(id);
        dest.writeString(name);
        dest.writeString(avatar);
        dest.writeInt(role);
        dest.writeString(email);
        dest.writeString(description);
        ParcelUtils.writeStringArray(dest, affiliation);
        dest.writeString(location);
        dest.writeParcelable(coordinate, 0);
    }

}
