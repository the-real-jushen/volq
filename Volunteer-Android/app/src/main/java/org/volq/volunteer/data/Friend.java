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
import android.os.Parcelable;

public class Friend implements Parcelable {

    // Friend
    public String id;
    public String name;
    public String avatar;
    public String description;
    public int level;

    // My friend ranking
    // public String id;
    public String email;
    // public String name;
    public String[] affiliation;
    public int point;
    public int activityCount;
    public int badgeCount;

    @Override
    public int describeContents() {
        // TODO Auto-generated method stub
        return 0;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {
        // TODO Auto-generated method stub

    }

}
