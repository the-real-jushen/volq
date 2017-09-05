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

import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.content.Context;
import android.content.Intent;
import android.content.res.Resources;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.support.v4.app.FragmentManager;
import android.support.v4.app.FragmentPagerAdapter;
import android.support.v7.widget.StaggeredGridLayoutManager;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import com.hippo.util.Coordinate;
import com.hippo.util.Log;
import com.hippo.util.MathUtils;
import com.hippo.util.TextUtils;
import com.hippo.util.UiUtils;
import com.hippo.util.Utils;
import com.hippo.util.ViewUtils;
import com.hippo.widget.ContentLoadLayout;
import com.hippo.widget.FloatingActionButton;
import com.hippo.widget.recyclerview.EasyRecyclerView;
import com.hippo.widget.recyclerview.MarginItemDecoration;
import com.squareup.picasso.Picasso;

import org.volq.volunteer.R;
import org.volq.volunteer.account.VltAccount;
import org.volq.volunteer.account.VltAccountStore;
import org.volq.volunteer.app.LazyFragment;
import org.volq.volunteer.cardview.BadgeHolder;
import org.volq.volunteer.cardview.DoingHolder;
import org.volq.volunteer.cardview.StatusHolder;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.Badge;
import org.volq.volunteer.data.Doing;
import org.volq.volunteer.data.User;
import org.volq.volunteer.data.Volunteer;
import org.volq.volunteer.util.VltUtils;
import org.volq.volunteer.widget.PentagonParameterView;

public class UserStatusActivity extends ViewPagerActivity implements
        VltAccountStore.OnChangeAccountListener {
    public static final String ACTION_ACCOUNT_STATUS = "org.volq.volunteer.ui.UserStatusActivity.ACCOUNT_STATUS";

    public static final String KEY_USER = "user";
    public static final String KEY_USER_ID = "user_id";

    private static final int POSITION_INFO = 0;
    private static final int POSITION_BADGES = 1;
    private static final int POSITION_DOINGS = 2;
    private static final int POSITION_SUM = 3;

    private User mAccountUser;
    private String mUserId;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        Intent intent = getIntent();
        String action = intent.getAction();
        if (ACTION_ACCOUNT_STATUS.equals(action)) {
            VltAccount account = VltAccountStore.getInstance(this).getCurAccount();
            if (account == null || account.user == null) {
                errorToFinish(getString(R.string.mesg_current_account_invaild));
                return;
            } else {
                mAccountUser = account.user;
            }
        } else {
            mAccountUser = null;
            mUserId = intent.getStringExtra(KEY_USER_ID);
            if (mUserId == null) {
                errorToFinish(getString(R.string.mesg_invaild_parameters));
                return;
            }
        }

        UserStatusAdapter adapter = new UserStatusAdapter(getSupportFragmentManager());
        setViewPageAdapter(adapter);

        if (mAccountUser != null) {
            setDrawerListActivatedPosition(DrawerActivity.POSITION_MY_STATUS);

            VltAccountStore.getInstance(this).addOnChangeAccountListener(this);
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();

        if (mAccountUser != null) {
            VltAccountStore.getInstance(this).removeOnChangeAccountListener(this);
        }
    }

    @Override
    public void onAddAccount() {
        // Empty
    }

    @Override
    public void onRemoveAccount() {
        if (mAccountUser != null) {
            finish();
        }
    }

    private InfoFragment mInfoFragment;

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {

        Log.d("TAG", "resultCode = " + resultCode);

        if (resultCode == Activity.RESULT_OK) {
            mAccountUser = data.getParcelableExtra(UserStatusActivity.KEY_USER);
            mInfoFragment.setUser(mAccountUser);
        }
    }

    public class UserStatusAdapter extends FragmentPagerAdapter {
        public UserStatusAdapter(FragmentManager fm) {
            super(fm);
        }

        @Override
        public Fragment getItem(int i) {
            Fragment fragment;
            switch (i) {
                default:
                case POSITION_INFO: {
                    Bundle args = new Bundle();
                    if (mAccountUser != null) {
                        args.putParcelable(KEY_USER, mAccountUser);
                    } else {
                        args.putString(KEY_USER_ID, mUserId);
                    }
                    mInfoFragment = new InfoFragment();
                    fragment = mInfoFragment;
                    fragment.setArguments(args);
                    break;
                }
                case POSITION_BADGES: {
                    Bundle args = new Bundle();
                    if (mAccountUser != null) {
                        args.putString(KEY_USER_ID, mAccountUser.id);
                    } else {
                        args.putString(KEY_USER_ID, mUserId);
                    }
                    fragment = new BadgesFragment();
                    fragment.setArguments(args);
                    break;
                }
                case POSITION_DOINGS: {
                    Bundle args = new Bundle();
                    if (mAccountUser != null) {
                        args.putString(KEY_USER_ID, mAccountUser.id);
                    } else {
                        args.putString(KEY_USER_ID, mUserId);
                    }
                    fragment = new DoingsFragment();
                    fragment.setArguments(args);
                    break;
                }
            }
            return fragment;
        }

        @Override
        public int getCount() {
            return mAccountUser != null ? 2 : 3;
        }

        @Override
        public CharSequence getPageTitle(int position) {
            CharSequence c;
            switch (position) {
            default:
            case POSITION_INFO:
                c = getString(R.string.title_info);
                break;
            case POSITION_BADGES:
                c = getString(R.string.title_badges);
                break;
            case POSITION_DOINGS:
                c = getString(R.string.title_doings);
                break;
            }
            return c;
        }
    }

    public static class InfoFragment extends LazyFragment implements
            View.OnClickListener {
        private static final int POSITION_VOLUNTEER_AVATAR = 0;
        private static final int POSITION_VOLUNTEER_BASE_INFO = 1;
        private static final int POSITION_VOLUNTEER_POSITION = 2;
        private static final int POSITION_VOLUNTEER_PROPERTY = 3;
        private static final int POSITION_VOLUNTEER_GROWING = 4;
        private static final int CARD_COUNT_VOLUNTEER = 5;

        private VltClient mClient;
        private UserStatusActivity mActivity;
        private Resources mResources;
        private LayoutInflater mInflater;

        private ContentLoadLayout mContentLoadLayout;
        private FloatingActionButton mFab;

        private VolunteerStatusContentHelper mContentHelper;
        private StaggeredGridLayoutManager mLayoutManager;
        private MarginItemDecoration mItemDecoration;

        private User mUser;
        private String mUserId;

        private boolean mIsMyself;

        @Override
        public View onCreateViewFirst(LayoutInflater inflater,
                ViewGroup container, Bundle savedInstanceState) {
            mClient = VltClient.getInstance(mActivity);
            mActivity = (UserStatusActivity) getActivity();
            mResources = mActivity.getResources();
            mInflater = mActivity.getLayoutInflater();

            Bundle args = getArguments();
            mUser = args.getParcelable(KEY_USER);
            mUserId = args.getString(KEY_USER_ID);

            View rootView = inflater.inflate(R.layout.fragment_user_info, container, false);
            mContentLoadLayout = (ContentLoadLayout) rootView.findViewById(R.id.content_load_layout);
            mFab = (FloatingActionButton) rootView.findViewById(R.id.fab_add_friend);

            mContentHelper = new VolunteerStatusContentHelper();
            mLayoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.VERTICAL);
            mItemDecoration = new MarginItemDecoration(UiUtils.dp2pix(8)); // TODO
            mContentLoadLayout.setContentHelper(mContentHelper);
            mContentLoadLayout.setLayoutManager(mLayoutManager);
            mContentLoadLayout.addItemDecoration(mItemDecoration);
            mContentLoadLayout.setHeaderEnable(false);
            mContentLoadLayout.setFooterEnable(false);

            if (mUser == null) {
                mContentHelper.refresh();
            } else {
                mUserId = mUser.id;
                mContentHelper.onGetContentSuccess(0, new User[]{mUser}, true);
            }

            mIsMyself = mUser != null;

            ViewUtils.setVisibility(mFab, View.GONE);

            return rootView;
        }

        public void setUser(User user) {
            mUser = user;
            mUserId = user.id;
            mContentHelper.onGetContentSuccess(0, new User[]{mUser}, true);
        }

        public void checkIsMyFriend() {
            mClient.isMyFriend(mUserId, new VltClient.OnIsMyFriendHelperListener() {
                @Override
                public void onSuccess(boolean isMyFriend) {
                    if (!isMyFriend) {
                        ViewUtils.setVisibility(mFab, View.VISIBLE);
                        mFab.setOnClickListener(InfoFragment.this);
                    }
                }

                @Override
                public void onFailure(Exception e) {
                    Toast.makeText(mActivity, e.getMessage(), Toast.LENGTH_SHORT).show(); // TODO
                }
            });
        }

        public boolean isMyself(String id) {
            VltAccount account = VltAccountStore.getInstance(mActivity).getCurAccount();
            if (account == null) {
                return false;
            } else if (account.userId.equals(id)) {
                return true;
            } else {
                return false;
            }
        }

        @Override
        public void onClick(View v) {
            if (v == mFab) {
                mClient.applyFriend(mUserId, "来做我的好朋友吧", new VltClient.OnApplyFriendListener() {
                    @Override
                    public void onSuccess() {
                        Toast.makeText(mActivity, "成功发出好友申请", Toast.LENGTH_SHORT).show(); // TODO
                    }

                    @Override
                    public void onFailure(Exception e) {
                        Toast.makeText(mActivity, "未成功发出好友申请", Toast.LENGTH_SHORT).show(); // TODO
                    }
                });
            }
        }

        private class VolunteerStatusContentHelper extends ContentLoadLayout.ContentHelper<User, StatusHolder> {
            @Override
            public void getContent(int index) {
                mClient.getUserWithInfo(mUserId, new VltClient.OnGetUserWithInfoListener() {
                    @Override
                    public void onSuccess(User user) {
                        mUser = user;
                        onGetContentSuccess(0, new User[]{user}, true);
                        if (!isMyself(mUser.id)) {
                            checkIsMyFriend();
                        }
                    }

                    @Override
                    public void onFailure(Exception e) {
                        onGetContentFailure(0, e);
                    }
                });
            }

            @Override
            public int getItemCount() {
                if (getDataSize() == 0)
                    return 0;

                switch (mUser.role) {
                    case VltUtils.ROLE_VOLUNTEER:
                        return CARD_COUNT_VOLUNTEER;
                    case VltUtils.ROLE_ORGANIZER:
                        return 0; // TODO
                    case VltUtils.ROLE_ORGANIZATION:
                        return 0; // TODO
                    case VltUtils.ROLE_ANONYMOUS:
                    default:
                        return 0; // TODO
                }
            }

            @Override
            public StatusHolder onCreateViewHolder(ViewGroup parent, int viewType) {
                return StatusHolder.createViewHolder(mInflater, parent);
            }

            @Override
            public void onBindViewHolder(StatusHolder holder, int position) {
                super.onBindViewHolder(holder, position);

                switch (mUser.role) {
                    case VltUtils.ROLE_VOLUNTEER:
                        bindVolunteerViewHolder(holder, mInflater, position);
                        break;
                    case VltUtils.ROLE_ORGANIZER:
                        bindOrganizerViewHolder(holder, mInflater, position);
                        break;
                    case VltUtils.ROLE_ORGANIZATION:
                        bindOrganizationViewHolder(holder, mInflater, position);
                        break;
                    case VltUtils.ROLE_ANONYMOUS:
                    default:
                        bindAnonyousViewHolder(holder, mInflater, position);
                        break;
                }
            }


            private void bindVolunteerViewHolder(StatusHolder statusViewHolder,
                                                 LayoutInflater inflater, int position) {
                Context context = inflater.getContext();
                Resources resources = context.getResources();
                final Volunteer volunteer = (Volunteer) mUser;
                FrameLayout custom = statusViewHolder.custom;
                View view;
                switch (position) {
                    case POSITION_VOLUNTEER_AVATAR:
                        statusViewHolder.header.setBackgroundColor(
                                mResources.getColor(R.color.material_green_500));
                        statusViewHolder.title.setText("头像"); // TODO
                        ViewUtils.setVisibility(statusViewHolder.edit, mIsMyself ? View.VISIBLE : View.GONE);
                        statusViewHolder.edit.setOnClickListener(new View.OnClickListener() {
                            @Override
                            public void onClick(View v) {
                                Intent intent = new Intent(mActivity, SelectNewAvatarActivity.class);
                                mActivity.startActivityForResult(intent, 0);
                            }
                        });
                        custom.removeAllViews();

                        ImageView ivv = new ImageView(mActivity);
                        Picasso.with(mActivity).load(volunteer.avatar).into(ivv);
                        custom.addView(ivv);
                        break;

                    case POSITION_VOLUNTEER_BASE_INFO:
                        statusViewHolder.header.setBackgroundColor(
                                mResources.getColor(R.color.material_amber_500));
                        statusViewHolder.title.setText(R.string.base_info_title);
                        ViewUtils.setVisibility(statusViewHolder.edit, View.GONE);
                        custom.removeAllViews();

                        view = inflater.inflate(R.layout.base_info, custom);
                        TextView name = (TextView) view.findViewById(R.id.name);
                        TextView email = (TextView) view.findViewById(R.id.email);
                        TextView role = (TextView) view.findViewById(R.id.role);
                        TextView affiliation = (TextView) view.findViewById(R.id.affiliation);
                        TextView description = (TextView) view.findViewById(R.id.description);
                        name.setText(volunteer.name);
                        email.setText(volunteer.email);
                        role.setText(VltUtils.getRoleString(mActivity, volunteer.role));
                        affiliation.setText(Utils.join(volunteer.affiliation,
                                inflater.getContext().getString(R.string.join_separator)));
                        description.setText(volunteer.description);

                        View editEmail = view.findViewById(R.id.email_edit);
                        View editAffiliation = view.findViewById(R.id.affiliation_edit);
                        View editDescription = view.findViewById(R.id.description_edit);
                        if (mIsMyself) {
                            ViewUtils.setVisibility(editEmail, View.VISIBLE);
                            ViewUtils.setVisibility(editAffiliation, View.VISIBLE);
                            ViewUtils.setVisibility(editDescription, View.VISIBLE);
                            editEmail.setOnClickListener(new View.OnClickListener() {
                                @Override
                                public void onClick(View v) {
                                    Intent intent = new Intent(mActivity, UpdateInfoActivity.class);
                                    intent.putExtra(UpdateInfoActivity.KEY_TYPE, UpdateInfoActivity.TYPE_EMAIL);
                                    intent.putExtra(UpdateInfoActivity.KEY_OLD_VALUE, volunteer.email);
                                    mActivity.startActivityForResult(intent, 0);
                                }
                            });
                            editAffiliation.setOnClickListener(new View.OnClickListener() {
                                @Override
                                public void onClick(View v) {
                                    Intent intent = new Intent(mActivity, UpdateInfoActivity.class);
                                    intent.putExtra(UpdateInfoActivity.KEY_TYPE, UpdateInfoActivity.TYPE_AFFILIATION);
                                    intent.putExtra(UpdateInfoActivity.KEY_OLD_VALUE, volunteer.affiliation);
                                    mActivity.startActivityForResult(intent, 0);
                                }
                            });
                            editDescription.setOnClickListener(new View.OnClickListener() {
                                @Override
                                public void onClick(View v) {
                                    Intent intent = new Intent(mActivity, UpdateInfoActivity.class);
                                    intent.putExtra(UpdateInfoActivity.KEY_TYPE, UpdateInfoActivity.TYPE_DESCRIPTION);
                                    intent.putExtra(UpdateInfoActivity.KEY_OLD_VALUE, volunteer.description);
                                    mActivity.startActivityForResult(intent, 0);
                                }
                            });
                        } else {
                            ViewUtils.setVisibility(editEmail, View.GONE);
                            ViewUtils.setVisibility(editAffiliation, View.GONE);
                            ViewUtils.setVisibility(editDescription, View.GONE);
                        }

                        break;

                    case POSITION_VOLUNTEER_POSITION:
                        statusViewHolder.header.setBackgroundColor(
                                mResources.getColor(R.color.material_purple_500));
                        statusViewHolder.title.setText(R.string.location);
                        ViewUtils.setVisibility(statusViewHolder.edit, View.GONE);
                        custom.removeAllViews();

                        final Coordinate coo = volunteer.coordinate;
                        final String location = volunteer.location;
                        String showLoaction;
                        view = inflater.inflate(R.layout.status_location, statusViewHolder.custom);
                        TextView tv = (TextView) view.findViewById(R.id.location_str);
                        ImageView iv = (ImageView) view.findViewById(R.id.action_map);

                        if (location != null) {
                            showLoaction = location;
                        } else if (coo != null) {
                            showLoaction = coo.toString();
                        } else {
                            showLoaction = getString(R.string.location_none);
                        }
                        tv.setText(showLoaction);

                        final String finalShowLoaction = showLoaction;
                        if (coo != null) {
                            iv.setOnClickListener(new View.OnClickListener() {
                                @Override
                                public void onClick(View v) {
                                    Intent intent = new Intent(mActivity, MapActivity.class);
                                    intent.putExtra(MapActivity.KEY_LNG, coo.lng);
                                    intent.putExtra(MapActivity.KEY_LAT, coo.lat);
                                    intent.putExtra(MapActivity.KEY_LOCATION_STR, finalShowLoaction);
                                    startActivity(intent);
                                }
                            });
                        } else {
                            ViewUtils.setVisibility(iv, View.GONE);
                        }

                        break;
                    case POSITION_VOLUNTEER_PROPERTY:
                        statusViewHolder.header.setBackgroundColor(
                                mResources.getColor(R.color.material_indigo_500));
                        statusViewHolder.title.setText(R.string.property_title);
                        ViewUtils.setVisibility(statusViewHolder.edit, View.GONE);
                        custom.removeAllViews();

                        PentagonParameterView pentagon = new PentagonParameterView(context);
                        pentagon.setParameterText(
                                getString(R.string.pentagon_strength),
                                getString(R.string.pentagon_intelligence),
                                getString(R.string.pentagon_endurance),
                                getString(R.string.pentagon_compassion),
                                getString(R.string.pentagon_sacrifice));
                        pentagon.setStrokeColor(resources.getColor(R.color.material_grey_700));
                        pentagon.setFillColor(resources.getColor(R.color.material_pink_200));
                        pentagon.setFillStrokeColor(resources.getColor(R.color.material_pink_500));
                        int maxProperty = MathUtils.max(volunteer.strength, volunteer.intelligence,
                                volunteer.endurance, volunteer.compassion, volunteer.sacrifice);
                        if (maxProperty == 0) {
                            pentagon.setParameters(0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
                        } else {
                            pentagon.setParameters((float) volunteer.strength / (float) maxProperty,
                                    (float) volunteer.intelligence / (float) maxProperty,
                                    (float) volunteer.endurance / (float) maxProperty,
                                    (float) volunteer.compassion / (float) maxProperty,
                                    (float) volunteer.sacrifice / (float) maxProperty);
                        }
                        FrameLayout.LayoutParams lp = new FrameLayout.LayoutParams(
                                ViewGroup.LayoutParams.MATCH_PARENT,
                                ViewGroup.LayoutParams.WRAP_CONTENT);
                        custom.addView(pentagon, lp);
                        break;
                    case POSITION_VOLUNTEER_GROWING:
                        statusViewHolder.header.setBackgroundColor(
                                mResources.getColor(R.color.material_blue_500));
                        statusViewHolder.title.setText(R.string.growing_title);
                        ViewUtils.setVisibility(statusViewHolder.edit, View.GONE);
                        custom.removeAllViews();

                        view = inflater.inflate(R.layout.growing, custom);
                        TextView level = (TextView) view.findViewById(R.id.level);
                        ImageView levelImage = (ImageView) view.findViewById(R.id.level_image);
                        TextView point = (TextView) view.findViewById(R.id.point);
                        TextView signedInActivityNumber = (TextView) view.findViewById(R.id.signed_in_activity_number);
                        TextView completeRate = (TextView) view.findViewById(R.id.complete_rate);
                        level.setText(Integer.toString(volunteer.level));
                        Picasso.with(mActivity).load(volunteer.levelPicture).into(levelImage);
                        point.setText(Integer.toString(volunteer.point));
                        signedInActivityNumber.setText(Integer.toString(volunteer.signedInActivityNumber));
                        completeRate.setText(Integer.toString((int) (volunteer.completeRate * 100)) + "%");
                        break;
                }
            }

            private void bindOrganizerViewHolder(StatusHolder statusViewHolder,
                                                 LayoutInflater inflater, int position) {
                // TODO
            }

            private void bindOrganizationViewHolder(StatusHolder statusViewHolder,
                                                    LayoutInflater inflater, int position) {
                // TODO
            }

            private void bindAnonyousViewHolder(StatusHolder statusViewHolder,
                                                LayoutInflater inflater, int position) {
                // TODO
            }
        }
    }

    public static class BadgesFragment extends LazyFragment {
        private Activity mActivity;
        private LayoutInflater mInflater;
        private VltClient mClient;

        private ContentLoadLayout mContentLoadLayout;

        private BadgesHelper mContentHelper;
        private StaggeredGridLayoutManager mLayoutManager;
        private MarginItemDecoration mItemDecoration;

        private String mUserId;

        @SuppressLint("InflateParams")
        private Dialog createBadgeDialog(String name) {
            View view = mInflater.inflate(R.layout.dialog_badge_detail, null);
            final ProgressBar progressBar = (ProgressBar) view.findViewById(R.id.progressBar);
            final View badgeDetail = view.findViewById(R.id.badge_detail);
            final TextView title = (TextView) view.findViewById(R.id.title);
            final TextView unlockTime = (TextView) view.findViewById(R.id.unlock_time);
            final TextView detail = (TextView) view.findViewById(R.id.detail);
            final TextView conditions = (TextView) view.findViewById(R.id.conditions);
            final ImageView badgeImage = (ImageView) view.findViewById(R.id.badge);

            final Dialog d = new AlertDialog.Builder(mActivity).setView(view).create();

            mClient.getBadgeDetail(mUserId, name, new VltClient.OnGetBadgeDetailListener() {
                @Override
                public void onSuccess(Badge badge) {
                    ViewUtils.setVisibility(progressBar, View.GONE);
                    ViewUtils.setVisibility(badgeDetail, View.VISIBLE);

                    title.setText(badge.name);
                    unlockTime.setText(mClient.formatDateToHuman(badge.grantedTime));
                    detail.setText(badge.description);
                    conditions.setText(Utils.join(badge.requirementDescription, "\n"));
                    Picasso.with(mActivity).load(badge.picture).error(R.drawable.default_badge).into(badgeImage);
                }

                @Override
                public void onFailure(Exception e) {
                    d.dismiss();
                    Toast.makeText(mActivity, e.getMessage(), Toast.LENGTH_SHORT).show();
                }
            });

            return d;
        }

        @Override
        public View onCreateViewFirst(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
            Bundle args = getArguments();
            mUserId = args.getString(KEY_USER_ID);

            mActivity = getActivity();
            mInflater = mActivity.getLayoutInflater();
            mClient = VltClient.getInstance(mActivity);

            mContentLoadLayout = new ContentLoadLayout(mActivity);

            mContentHelper = new BadgesHelper();
            mLayoutManager = new StaggeredGridLayoutManager(3, StaggeredGridLayoutManager.VERTICAL);
            mItemDecoration = new MarginItemDecoration(UiUtils.dp2pix(8));
            mContentLoadLayout.setContentHelper(mContentHelper);
            mContentLoadLayout.setLayoutManager(mLayoutManager);
            mContentLoadLayout.addItemDecoration(mItemDecoration);
            mContentLoadLayout.setOnItemClickListener(new EasyRecyclerView.OnItemClickListener() {
                @Override
                public void onItemClick(EasyRecyclerView parent, View view, int position, long id) {
                    createBadgeDialog(mContentHelper.getData(position).name).show();
                }
            });

            mContentHelper.refresh();

            return mContentLoadLayout;
        }

        private class BadgesHelper extends ContentLoadLayout.ContentHelper<Badge, BadgeHolder> {
            @Override
            public void getContent(final int index) {
                mClient.getUserBadges(mUserId, index, new VltClient.OnGetUserBadgesListener() {
                    @Override
                    public void onSuccess(Badge[] badges) {
                        onGetContentSuccess(index, badges, badges.length <= 0);
                    }

                    @Override
                    public void onFailure(Exception e) {
                        onGetContentFailure(index, e);
                    }
                });
            }

            @Override
            public BadgeHolder onCreateViewHolder(ViewGroup parent, int viewType) {
                return BadgeHolder.createViewHolder(mInflater, parent);
            }

            @Override
            public int getItemCount() {
                return getDataSize();
            }

            @Override
            public void onBindViewHolder(BadgeHolder holder, int position) {
                super.onBindViewHolder(holder, position);

                Badge badge = getData(position);
                holder.name.setText(badge.name);
                Picasso.with(mActivity).load(badge.picture).error(R.drawable.default_badge).into(holder.picture);
            }
        }
    }

    public static class DoingsFragment extends LazyFragment {

        private Activity mActivity;
        private LayoutInflater mInflater;
        private VltClient mClient;

        private ContentLoadLayout mContentLoadLayout;

        private DoingsHelper mContentHelper;
        private StaggeredGridLayoutManager mLayoutManager;
        private MarginItemDecoration mItemDecoration;

        private String mUserId;

        @Override
        public View onCreateViewFirst(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
            Bundle args = getArguments();
            mUserId = args.getString(KEY_USER_ID);

            mActivity = getActivity();
            mInflater = mActivity.getLayoutInflater();
            mClient = VltClient.getInstance(mActivity);

            mContentLoadLayout = new ContentLoadLayout(mActivity);

            mContentHelper = new DoingsHelper();
            mLayoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.VERTICAL);
            mItemDecoration = new MarginItemDecoration(UiUtils.dp2pix(8)); // TODO
            mContentLoadLayout.setContentHelper(mContentHelper);
            mContentLoadLayout.setLayoutManager(mLayoutManager);
            mContentLoadLayout.addItemDecoration(mItemDecoration);
            mContentLoadLayout.setOnItemClickListener(new EasyRecyclerView.OnItemClickListener() {
                @Override
                public void onItemClick(EasyRecyclerView parent, View view, int position, long id) {
                    Intent intent = new Intent(mActivity, DoingActivity.class);
                    intent.putExtra(DoingActivity.KEY_ID, mContentHelper.getData(position).Id);
                    startActivity(intent);
                }
            });

            mContentHelper.refresh();

            return mContentLoadLayout;
        }

        private class DoingsHelper extends ContentLoadLayout.ContentHelper<Doing, DoingHolder> {
            @Override
            public void getContent(final int index) {
                mClient.myDoings(mUserId, TextUtils.STRING_EMPTY, VltClient.DOING_STAGE_ALL, index, new VltClient.OnMyDoingsListener() {
                    @Override
                    public void onSuccess(Doing[] doings) {
                        onGetContentSuccess(index, doings, doings.length <= 0);
                    }

                    @Override
                    public void onFailure(Exception e) {
                        onGetContentFailure(index, e);
                    }
                });
            }

            @Override
            public int getItemCount() {
                return getDataSize();
            }

            @Override
            public DoingHolder onCreateViewHolder(ViewGroup parent, int viewType) {
                return DoingHolder.createViewHolder(mActivity.getLayoutInflater(), parent);
            }

            @Override
            public void onBindViewHolder(DoingHolder holder, int position) {
                super.onBindViewHolder(holder, position);

                Doing d = getData(position);
                Picasso.with(mActivity).load(d.Cover).into(holder.thumb);
                holder.title.setText(d.Name);
                holder.state.setText(VltUtils.getDoingStatusString(mActivity, d.Status));
                holder.state.setTextColor(mActivity.getResources().getColor(R.color.material_blue_500));
                holder.time.setText(VltClient.getInstance(mActivity).formatDateToHuman(d.StartTime));
                holder.browse.setText(Integer.toString(d.VolunteerViewedTime));
                holder.favorite.setText(Integer.toString(d.VolunteerFavoritedTime));
                holder.join.setText(Integer.toString(d.HasSignedInVolunteerNumber));
            }
        }
    }

}
