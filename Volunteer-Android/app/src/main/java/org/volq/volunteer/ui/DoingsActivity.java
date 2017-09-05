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

import android.content.Intent;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;

import com.hippo.util.TextUtils;
import com.hippo.widget.ContentLoadLayout;

import org.volq.volunteer.R;
import org.volq.volunteer.account.VltAccount;
import org.volq.volunteer.account.VltAccountStore;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.Doing;

public class DoingsActivity extends AbsDoingsActivity implements
        VltAccountStore.OnChangeAccountListener {

    private VltClient mClient;

    public DoingsActivity() {
        mClient = VltClient.getInstance(this);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        VltAccount account = VltAccountStore.getInstance(this).getCurAccount();
        if (account == null || account.user == null) {
            errorToFinish(getString(R.string.mesg_current_account_invaild));
            return;
        }

        VltAccountStore.getInstance(this).addOnChangeAccountListener(this);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();

        VltAccountStore.getInstance(this).removeOnChangeAccountListener(this);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.my_doings, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.action_favorite:
                Intent intent = new Intent(DoingsActivity.this, FavoriteDoingsActivity.class);
                startActivity(intent);
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }
    }

    @Override
    public int getDrawerListPosition() {
        return DoingsActivity.POSITION_DOINGS;
    }

    protected boolean enableSearchAction() {
        return false;
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

    protected CharSequence getPageTitle(int position) {
        CharSequence c;
        switch (position) {
            default:
            case POSITION_PREPARING:
                c = getString(R.string.title_preparing);
                break;
            case POSITION_ONGOING:
                c = getString(R.string.title_ongoing);
                break;
            case POSITION_CLOSED:
                c = getString(R.string.title_i_finished);
                break;
            case POSITION_ALL:
                c = getString(R.string.title_all);
                break;
        }
        return c;
    }

    @Override
    protected void getData(final int index, final int state, final ContentLoadLayout.ContentHelper helper) {
        VltAccount account = VltAccountStore.getInstance(this).getCurAccount();
        if (account == null) {
            helper.onGetContentFailure(index, new Exception("未登录账号")); // TODO
        } else {
            mClient.myDoings(account.userId, TextUtils.STRING_EMPTY, getStageByState(state), index, new VltClient.OnMyDoingsListener() {
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

    @Override
    public void onAddAccount() {
        // Empty
    }

    @Override
    public void onRemoveAccount() {
        finish();
    }
}
