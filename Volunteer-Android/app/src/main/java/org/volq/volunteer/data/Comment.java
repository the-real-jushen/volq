package org.volq.volunteer.data;

import android.os.Parcel;
import android.os.Parcelable;

import org.volq.volunteer.client.VltClient;

import java.util.Date;

public class Comment extends FastJSONObject implements Parcelable {

    public String Id;
    public String UserId;
    public String Avatar;
    public String UserName;
    public int UserState;
    public String Content;
    public Date Time;
    public int Position;

    public String getId() {
        return Id;
    }

    public void setId(String id) {
        Id = id;
    }

    public String getUserId() {
        return UserId;
    }

    public void setUserId(String userId) {
        UserId = userId;
    }

    public String getAvatar() {
        return Avatar;
    }

    public void setAvatar(String avatar) {
        Avatar = VltClient.rightUrl(avatar);
    }

    public String getUserName() {
        return UserName;
    }

    public void setUserName(String userName) {
        UserName = userName;
    }

    public int getUserState() {
        return UserState;
    }

    public void setUserState(int userState) {
        UserState = userState;
    }

    public String getContent() {
        return Content;
    }

    public void setContent(String content) {
        Content = content;
    }

    public Date getTime() {
        return Time;
    }

    public void setTime(Date time) {
        Time = time;
    }

    public int getPosition() {
        return Position;
    }

    public void setPosition(int position) {
        Position = position;
    }

    @Override
    public int describeContents() {
        return 0;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {
        dest.writeString(this.Id);
        dest.writeString(this.UserId);
        dest.writeString(this.Avatar);
        dest.writeString(this.UserName);
        dest.writeInt(this.UserState);
        dest.writeString(this.Content);
        dest.writeLong(Time != null ? Time.getTime() : -1);
        dest.writeInt(this.Position);
    }

    public Comment() {
    }

    private Comment(Parcel in) {
        this.Id = in.readString();
        this.UserId = in.readString();
        this.Avatar = in.readString();
        this.UserName = in.readString();
        this.UserState = in.readInt();
        this.Content = in.readString();
        long tmpTime = in.readLong();
        this.Time = tmpTime == -1 ? null : new Date(tmpTime);
        this.Position = in.readInt();
    }

    public static final Creator<Comment> CREATOR = new Creator<Comment>() {
        public Comment createFromParcel(Parcel source) {
            return new Comment(source);
        }

        public Comment[] newArray(int size) {
            return new Comment[size];
        }
    };
}
