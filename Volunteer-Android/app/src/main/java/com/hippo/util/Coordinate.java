package com.hippo.util;

import android.os.Parcel;
import android.os.Parcelable;

public class Coordinate implements Parcelable {
    public double lng;
    public double lat;

    private Coordinate() {
        // Empty
    }

    public Coordinate(double lng, double lat) {
        this.lng = lng;
        this.lat = lat;
    }

    public Coordinate(Coordinate c) {
        lng = c.lng;
        lat = c.lat;
    }

    public static final Creator<Coordinate> CREATOR =
            new Creator<Coordinate>() {
                @Override
                public Coordinate createFromParcel(Parcel source) {
                    Coordinate p = new Coordinate();
                    p.lng = source.readDouble();
                    p.lat = source.readDouble();
                    return p;
                }

                @Override
                public Coordinate[] newArray(int size) {
                    return new Coordinate[size];
                }
            };

    @Override
    public String toString() {
        return "lng = " + lng + ", lat = " + lat;
    }

    @Override
    public int describeContents() {
        return 0;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {
        dest.writeDouble(lng);
        dest.writeDouble(lat);
    }
}
