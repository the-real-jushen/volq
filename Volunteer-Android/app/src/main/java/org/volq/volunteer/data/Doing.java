package org.volq.volunteer.data;

import android.os.Parcel;
import android.os.Parcelable;

import com.hippo.util.Coordinate;

import java.util.Date;

public class Doing implements Parcelable {

    /*
    0="草案"
1="激活"
2="可以Signin"
3="注册人数达到上限"
4="Ready" // 即将开始
5="可以CheckIn"
6="可以Signin或CheckIn"
7="已结束"
8="已取消"

     */


    public String Id;
    public String Name;
    public String OrganizationName;

    public int Point;
    public int Status;

    public Date OpenSignInTime;
    public Date StartTime;
    public Date FinishTime;

    public String Location;
    public Coordinate Coordinate;

    public String Cover;
    public String[] Tags;

    public int HasSignedInVolunteerNumber;
    public int VolunteerViewedTime;
    public int VolunteerFavoritedTime;

    public boolean hasViewed;
    public boolean hasFavorited;
    public boolean hasSignined;

    /* Add for detail */
    public String OrganizerId;
    public String OrganizationId;
    public String OrganizationAvatar;

    public String Abstract;

    public Date ActivateTime;
    public Date CloseSignInTime;

    public int MostVolunteers;
    public int LeastVolunteers;

    public String Procedure;

    public String[] Photos;
    public String[] Videos;

    public int HexagramPropertyTotalPointLimit;
    public int Strength;
    public int Intelligence;
    public int Endurance;
    public int Compassion;
    public int Sacrifice;

    public String Requirement;

    public VolunteersRecord VolunteersRecord;


/*



    /*
            "BadgeLimit": {
        "MustGranted": [],
        "CantGranted": []
    },
*/
    /*
            "HexagramPropertyTotalPointLimit": 50,
            "HexagramProperty": {
        "Strength": 2,
                "Intelligence": 1,
                "Endurance": 1,
                "Compassion": 1,
                "Sacrifice": 1
    },
            "Requirement": "<p>Test<br></p>",
            "VolunteerViewedTime": 0,
            "VolunteerFavoritedTime": 0,
            "HasSignedInVolunteerNumber": 0,
            "Rating": 0,
            "VolunteersRecord": [],
            "MyRecord": null,
            "hasFavorited": false,
            "hasSignined": false,
            "hasViewed": false,
            "myRate": 0,
            "ratedNumber": 0
*/








/*
    public String id;
    public String name;
    public String organizationName;
    public int point;
    public int status;
    public Date openSignInTime;
    public Date startTime;
    public Date finishTime;
    public String location;
    public Coordinate coordinate;
    public String photo;
    public String tags[];
    public int hasSignedInVolunteerNumber;
    public int volunteerViewedTime;
    public int volunteerFavoritedTime;
    public boolean hasViewed;
    public boolean hasFavorited;
    public boolean hasSignined;

    // public String id;
    public String organizerId;
    public String organizationId;
    // public String organizationName;
    // public String name;
    public String summary; // Abstract
    public Date activateTime;
    // public Date openSignInTime;
    public Date closeSignInTime;
    // public Date startTime;
    // public Date finishTime;

    // public int point;
    public int mostVolunteers;
    public int leastVolunteers;
    // public String location;
    // public Coordinate coordinate;
    public String procedure;
    public String[] photos;
    public String[] vidoes;
    // public int status;
    // public String[] tags;

    // Badage limit

    public int hexagramPropertyTotalPointLimit;
    public int strength;
    public int intelligence;
    public int endurance;
    public int compassion;
    public int sacrifice;
    public String requirement;
    // public int volunteerViewedTime;
    // public int volunteerFavoritedTime;
    // public int hasSignedInVolunteerNumber;
    // VolunteersRecord
    // public boolean hasFavorited;
    // public boolean hasSignined;
    // public boolean hasViewed;

    public @Nullable
    VolunteersRecord volunteersRecord;

    @Override
    public int describeContents() {
        return 0;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {

    }
    */


    @Override
    public int describeContents() {
        return 0;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {
        dest.writeString(this.Id);
        dest.writeString(this.Name);
        dest.writeString(this.OrganizationName);
        dest.writeInt(this.Point);
        dest.writeInt(this.Status);
        dest.writeLong(OpenSignInTime != null ? OpenSignInTime.getTime() : -1);
        dest.writeLong(StartTime != null ? StartTime.getTime() : -1);
        dest.writeLong(FinishTime != null ? FinishTime.getTime() : -1);
        dest.writeString(this.Location);
        dest.writeParcelable(this.Coordinate, 0);
        dest.writeString(this.Cover);
        dest.writeStringArray(this.Tags);
        dest.writeInt(this.HasSignedInVolunteerNumber);
        dest.writeInt(this.VolunteerViewedTime);
        dest.writeInt(this.VolunteerFavoritedTime);
        dest.writeByte(hasViewed ? (byte) 1 : (byte) 0);
        dest.writeByte(hasFavorited ? (byte) 1 : (byte) 0);
        dest.writeByte(hasSignined ? (byte) 1 : (byte) 0);
        dest.writeString(this.OrganizerId);
        dest.writeString(this.OrganizationId);
        dest.writeString(this.OrganizationAvatar);
        dest.writeString(this.Abstract);
        dest.writeLong(ActivateTime != null ? ActivateTime.getTime() : -1);
        dest.writeLong(CloseSignInTime != null ? CloseSignInTime.getTime() : -1);
        dest.writeInt(this.MostVolunteers);
        dest.writeInt(this.LeastVolunteers);
        dest.writeString(this.Procedure);
        dest.writeStringArray(this.Photos);
        dest.writeStringArray(this.Videos);
        dest.writeInt(this.HexagramPropertyTotalPointLimit);
        dest.writeInt(this.Strength);
        dest.writeInt(this.Intelligence);
        dest.writeInt(this.Endurance);
        dest.writeInt(this.Compassion);
        dest.writeInt(this.Sacrifice);
        dest.writeString(this.Requirement);
        dest.writeParcelable(this.VolunteersRecord, 0);
    }

    public Doing() {
    }

    private Doing(Parcel in) {
        this.Id = in.readString();
        this.Name = in.readString();
        this.OrganizationName = in.readString();
        this.Point = in.readInt();
        this.Status = in.readInt();
        long tmpOpenSignInTime = in.readLong();
        this.OpenSignInTime = tmpOpenSignInTime == -1 ? null : new Date(tmpOpenSignInTime);
        long tmpStartTime = in.readLong();
        this.StartTime = tmpStartTime == -1 ? null : new Date(tmpStartTime);
        long tmpFinishTime = in.readLong();
        this.FinishTime = tmpFinishTime == -1 ? null : new Date(tmpFinishTime);
        this.Location = in.readString();
        this.Coordinate = in.readParcelable(com.hippo.util.Coordinate.class.getClassLoader());
        this.Cover = in.readString();
        this.Tags = in.createStringArray();
        this.HasSignedInVolunteerNumber = in.readInt();
        this.VolunteerViewedTime = in.readInt();
        this.VolunteerFavoritedTime = in.readInt();
        this.hasViewed = in.readByte() != 0;
        this.hasFavorited = in.readByte() != 0;
        this.hasSignined = in.readByte() != 0;
        this.OrganizerId = in.readString();
        this.OrganizationId = in.readString();
        this.OrganizationAvatar = in.readString();
        this.Abstract = in.readString();
        long tmpActivateTime = in.readLong();
        this.ActivateTime = tmpActivateTime == -1 ? null : new Date(tmpActivateTime);
        long tmpCloseSignInTime = in.readLong();
        this.CloseSignInTime = tmpCloseSignInTime == -1 ? null : new Date(tmpCloseSignInTime);
        this.MostVolunteers = in.readInt();
        this.LeastVolunteers = in.readInt();
        this.Procedure = in.readString();
        this.Photos = in.createStringArray();
        this.Videos = in.createStringArray();
        this.HexagramPropertyTotalPointLimit = in.readInt();
        this.Strength = in.readInt();
        this.Intelligence = in.readInt();
        this.Endurance = in.readInt();
        this.Compassion = in.readInt();
        this.Sacrifice = in.readInt();
        this.Requirement = in.readString();
        this.VolunteersRecord = in.readParcelable(org.volq.volunteer.data.VolunteersRecord.class.getClassLoader());
    }

    public static final Creator<Doing> CREATOR = new Creator<Doing>() {
        public Doing createFromParcel(Parcel source) {
            return new Doing(source);
        }

        public Doing[] newArray(int size) {
            return new Doing[size];
        }
    };
}
