/*
 * Copyright (C) 2015 Hippo Seven
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package org.volq.volunteer.data;

import android.os.Parcel;

import com.hippo.util.Coordinate;

public class Volunteer extends User implements android.os.Parcelable {
    public int level;
    public String levelName;
    public String levelPicture;
    public int point;
    public int pointsToNextLevel;

    // Five
    public int strength;
    public int intelligence;
    public int endurance;
    public int compassion;
    public int sacrifice;

    public int signedInActivityNumber;
    public double completeRate;

    public Volunteer() {
        // Empty
    }

    public Volunteer(User user) {
        super(user);
    }

    @Override
    public int describeContents() {
        return 0;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {
        super.writeToParcel(dest, flags);
        dest.writeInt(this.level);
        dest.writeString(this.levelName);
        dest.writeString(this.levelPicture);
        dest.writeInt(this.point);
        dest.writeInt(this.pointsToNextLevel);
        dest.writeInt(this.strength);
        dest.writeInt(this.intelligence);
        dest.writeInt(this.endurance);
        dest.writeInt(this.compassion);
        dest.writeInt(this.sacrifice);
        dest.writeInt(this.signedInActivityNumber);
        dest.writeDouble(this.completeRate);
        dest.writeString(this.id);
        dest.writeString(this.name);
        dest.writeString(this.avatar);
        dest.writeInt(this.role);
        dest.writeInt(this.sex);
        dest.writeByte(isEmailVerified ? (byte) 1 : (byte) 0);
        dest.writeString(this.phoneNumber);
        dest.writeString(this.email);
        dest.writeString(this.description);
        dest.writeStringArray(this.affiliation);
        dest.writeString(this.location);
        dest.writeParcelable(this.coordinate, 0);
    }

    private Volunteer(Parcel in) {
        super(in);
        this.level = in.readInt();
        this.levelName = in.readString();
        this.levelPicture = in.readString();
        this.point = in.readInt();
        this.pointsToNextLevel = in.readInt();
        this.strength = in.readInt();
        this.intelligence = in.readInt();
        this.endurance = in.readInt();
        this.compassion = in.readInt();
        this.sacrifice = in.readInt();
        this.signedInActivityNumber = in.readInt();
        this.completeRate = in.readDouble();
        this.id = in.readString();
        this.name = in.readString();
        this.avatar = in.readString();
        this.role = in.readInt();
        this.sex = in.readInt();
        this.isEmailVerified = in.readByte() != 0;
        this.phoneNumber = in.readString();
        this.email = in.readString();
        this.description = in.readString();
        this.affiliation = in.createStringArray();
        this.location = in.readString();
        this.coordinate = in.readParcelable(Coordinate.class.getClassLoader());
    }

    public static final Creator<Volunteer> CREATOR = new Creator<Volunteer>() {
        public Volunteer createFromParcel(Parcel source) {
            return new Volunteer(source);
        }

        public Volunteer[] newArray(int size) {
            return new Volunteer[size];
        }
    };
}
