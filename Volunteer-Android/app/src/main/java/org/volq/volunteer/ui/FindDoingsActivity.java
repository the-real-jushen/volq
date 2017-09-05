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

package org.volq.volunteer.ui;

import android.os.Bundle;

import com.hippo.util.TextUtils;
import com.hippo.widget.ContentLoadLayout;

import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.Doing;

public class FindDoingsActivity extends AbsDoingsActivity {

    private VltClient mClient;

    public FindDoingsActivity() {
        mClient = VltClient.getInstance(this);
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    public int getDrawerListPosition() {
        return DrawerActivity.POSITION_FIND_DOINGS;
    }

    protected boolean enableSearchAction() {
        return true;
    }

    private String getStageByState(int state) {
        switch (state) {
            case POSITION_PREPARING:
                return VltClient.DOING_STAGE_ABOUT_TO_START;
            case POSITION_ONGOING:
                return VltClient.DOING_STAGE_RUNNING;
            case POSITION_CLOSED:
                return VltClient.DOING_STAGE_FINISH;
            default:
            case POSITION_ALL:
                return VltClient.DOING_STAGE_ALL;
        }
    }

    @Override
    protected void getData(final int index, final int state, final ContentLoadLayout.ContentHelper helper) {
        mClient.doings(TextUtils.STRING_EMPTY, getStageByState(state), index, new VltClient.OnDoingsListener() {
            @Override
            public void onSuccess(Doing[] doings) {
                helper.onGetContentSuccess(index, doings, doings.length <= 0);
            }

            @Override
            public void onFailure(Exception e) {
                helper.onGetContentFailure(index, e);
            }
        });
    }


}
