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

package org.volq.organizer.account;

import android.os.Parcel;
import android.os.Parcelable;

import org.volq.organizer.data.User;

public class VltAccount implements Parcelable {

    public String email;
    public String password;

    public String status;
    public String token;
    public String userId;
    public String role;
    public String name;

    public User user;

    @Override
    public boolean equals(Object obj) {
        if (obj instanceof VltAccount) {
            VltAccount account = (VltAccount) obj;
            if (email != null) {
                if (email.equals(account.email)) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

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
