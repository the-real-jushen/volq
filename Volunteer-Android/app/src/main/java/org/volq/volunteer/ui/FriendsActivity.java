package org.volq.volunteer.ui;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Color;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.support.v4.app.FragmentManager;
import android.support.v4.app.FragmentPagerAdapter;
import android.support.v4.view.MenuItemCompat;
import android.support.v4.view.ViewPager;
import android.support.v7.widget.RecyclerView;
import android.support.v7.widget.SearchView;
import android.support.v7.widget.StaggeredGridLayoutManager;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Toast;

import com.hippo.util.UiUtils;
import com.hippo.util.ViewUtils;
import com.hippo.widget.ContentLoadLayout;
import com.hippo.widget.recyclerview.EasyRecyclerView;
import com.hippo.widget.recyclerview.MarginItemDecoration;
import com.squareup.picasso.Picasso;

import org.volq.volunteer.R;
import org.volq.volunteer.account.VltAccount;
import org.volq.volunteer.account.VltAccountStore;
import org.volq.volunteer.app.LazyFragment;
import org.volq.volunteer.cardview.FriendApplyHolder;
import org.volq.volunteer.cardview.FriendRankingHolder;
import org.volq.volunteer.cardview.SeparatorTextHolder;
import org.volq.volunteer.cardview.SimpleHolder;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.ApplyFromMe;
import org.volq.volunteer.data.ApplyToMe;
import org.volq.volunteer.data.Friend;
import org.volq.volunteer.data.User;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.util.ArrayList;
import java.util.List;

public class FriendsActivity extends ViewPagerActivity
        implements ViewPager.OnPageChangeListener,
        SearchView.OnQueryTextListener, View.OnFocusChangeListener,
        VltAccountStore.OnChangeAccountListener {
    private static final int POSITION_MY_FRIENDS = 0;
    private static final int POSITION_FIND_FRIENDS = 1;
    private static final int POSITION_FRIEND_RANKING = 2;
    private static final int POSITION_FRIEND_APPLY = 3;
    private static final int POSITION_SUM = 4;

    private FriendsPagerAdapter mAdapter;

    private OnQueryTextSubmitListener mOnQueryTextSubmitListener;

    private MenuItem mSearchItem;

    @Override
    public void onPageScrollStateChanged(int arg0) {
        // Empty
    }

    @Override
    public void onPageScrolled(int arg0, float arg1, int arg2) {
        // Empty
    }

    @Override
    public void onPageSelected(int position) {
        supportInvalidateOptionsMenu();
    }

    @Override
    public boolean onQueryTextChange(String arg0) {
        return false;
    }

    @Override
    public boolean onQueryTextSubmit(String arg0) {
        if (mOnQueryTextSubmitListener != null)
            mOnQueryTextSubmitListener.onQueryTextSubmit(arg0);
        if (mSearchItem != null)
            MenuItemCompat.collapseActionView(mSearchItem);
        return true;
    }

    @Override
    public void onFocusChange(View v, boolean hasFocus) {
        if (!hasFocus && mSearchItem != null)
            MenuItemCompat.collapseActionView(mSearchItem);
    }

    @Override
    public void onAddAccount() {
        // Empty
    }

    @Override
    public void onRemoveAccount() {
        finish();
    }

    public void setOnQueryTextSubmitListener(OnQueryTextSubmitListener l) {
        mOnQueryTextSubmitListener = l;
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        VltAccount account = VltAccountStore.getInstance(this).getCurAccount();
        if (account == null) {
            errorToFinish(getString(R.string.mesg_current_account_invaild));
            return;
        }

        mAdapter = new FriendsPagerAdapter(getSupportFragmentManager());
        setViewPageAdapter(mAdapter);
        setOnPageChangeListener(this);
        setDrawerListActivatedPosition(DrawerActivity.POSITION_FRIENDS);

        VltAccountStore.getInstance(this).addOnChangeAccountListener(this);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();

        VltAccountStore.getInstance(this).removeOnChangeAccountListener(this);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        if (getCurrentItem() == POSITION_FIND_FRIENDS) {
            MenuInflater inflater = getMenuInflater();
            inflater.inflate(R.menu.search, menu);
            mSearchItem = menu.findItem(R.id.action_search);
            MenuItemCompat.setShowAsAction(mSearchItem, MenuItemCompat.SHOW_AS_ACTION_IF_ROOM
                    | MenuItemCompat.SHOW_AS_ACTION_COLLAPSE_ACTION_VIEW);
            SearchView searchView = (SearchView) MenuItemCompat.getActionView(mSearchItem);
            searchView.setOnQueryTextFocusChangeListener(this);
            searchView.setOnQueryTextListener(this);
            return true;
        } else {
            return super.onCreateOptionsMenu(menu);
        }
    }

    public class FriendsPagerAdapter extends FragmentPagerAdapter {
        public FriendsPagerAdapter(FragmentManager fm) {
            super(fm);
        }

        @Override
        public Fragment getItem(int i) {
            Fragment fragment;
            switch (i) {
            default:
            case POSITION_MY_FRIENDS:
                fragment = new MyFriendsFragment();
                break;
            case POSITION_FIND_FRIENDS:
                fragment = new FindFriendsFragment();
                break;
            case POSITION_FRIEND_RANKING:
                fragment = new FriendRankingFragment();
                break;
            case POSITION_FRIEND_APPLY:
                fragment = new FriendApplyFragment();
                break;
            }
            return fragment;
        }

        @Override
        public int getCount() {
            return POSITION_SUM;
        }

        @Override
        public CharSequence getPageTitle(int position) {
            CharSequence c;
            switch (position) {
            default:
            case POSITION_MY_FRIENDS:
                c = getString(R.string.my_friends);
                break;
            case POSITION_FIND_FRIENDS:
                c = getString(R.string.find_friends);
                break;
            case POSITION_FRIEND_RANKING:
                c = getString(R.string.friends_ranking);
                break;
            case POSITION_FRIEND_APPLY:
                c = getString(R.string.friend_apply);
                break;
            }
            return c;
        }
    }

    public static class MyFriendsFragment extends LazyFragment {
        private Activity mActivity;
        private VltClient mClient;

        private ContentLoadLayout mContentLoadLayout;

        private MyFriendHelper mMyFriendHelper;
        private StaggeredGridLayoutManager mLayoutManager;
        private MarginItemDecoration mItemDecoration;

        @Override
        public View onCreateViewFirst(LayoutInflater inflater,
                ViewGroup container, Bundle savedInstanceState) {
            mActivity = getActivity();
            mClient = VltClient.getInstance(mActivity);

            mContentLoadLayout = new ContentLoadLayout(mActivity);

            mMyFriendHelper = new MyFriendHelper();
            mLayoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.VERTICAL);
            mItemDecoration = new MarginItemDecoration(UiUtils.dp2pix(8)); // TODO
            mContentLoadLayout.setContentHelper(mMyFriendHelper);
            mContentLoadLayout.setLayoutManager(mLayoutManager);
            mContentLoadLayout.addItemDecoration(mItemDecoration);
            mContentLoadLayout.setOnItemClickListener(new EasyRecyclerView.OnItemClickListener() {
                @Override
                public void onItemClick(EasyRecyclerView parent, View view,
                        int position, long id) {
                    Intent intent = new Intent(mActivity, UserStatusActivity.class);
                    intent.putExtra(UserStatusActivity.KEY_USER_ID,
                            mMyFriendHelper.getData(position).id);
                    startActivity(intent);
                }
            });

            mMyFriendHelper.refresh();

            return mContentLoadLayout;
        }

        private class MyFriendHelper extends ContentLoadLayout.ContentHelper<Friend, SimpleHolder> {
            @Override
            public void getContent(final int index) {
                mClient.getMyFriends(new VltClient.OnGetMyFriendsListener() {
                    @Override
                    public void onSuccess(Friend[] friends) {
                        onGetContentSuccess(index, friends, true);
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
            public SimpleHolder onCreateViewHolder(ViewGroup parent, int viewType) {
                return SimpleHolder.createViewHolder(mActivity.getLayoutInflater(), parent);
            }

            @Override
            public void onBindViewHolder(SimpleHolder holder, int position) {
                super.onBindViewHolder(holder, position);

                Friend f = getData(position);
                Picasso.with(mActivity).load(f.avatar).into(holder.avatar);
                holder.owner.setText(f.name);
                holder.subtitle.setText(Integer.toString(f.level));
                holder.message.setText(f.description);
                ViewUtils.setVisibility(holder.thumb, View.GONE);
            }
        }
    }

    public static class FindFriendsFragment extends LazyFragment
            implements OnQueryTextSubmitListener {
        private static final int STATE_RECOMMEND = 0;
        private static final int STATE_SEARCH = 1;

        private static final int TYPE_SEPARATOR = 0;
        private static final int TYPE_FRIEND = 1;

        private Activity mActivity;
        private VltClient mClient;

        private ContentLoadLayout mContentLoadLayout;

        private FindFriendsHelper mContentHelper;
        private StaggeredGridLayoutManager mLayoutManager;
        private MarginItemDecoration mItemDecoration;

        private int mState = STATE_RECOMMEND;
        private String mSearchStr;

        @Override
        public View onCreateViewFirst(LayoutInflater inflater,
                ViewGroup container, Bundle savedInstanceState) {
            mActivity = getActivity();
            mClient = VltClient.getInstance(mActivity);
            ((FriendsActivity) mActivity).setOnQueryTextSubmitListener(this);

            mContentLoadLayout = new ContentLoadLayout(mActivity);

            mContentHelper = new FindFriendsHelper();
            mLayoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.VERTICAL);
            mItemDecoration = new MarginItemDecoration(UiUtils.dp2pix(8)); // TODO
            mContentLoadLayout.setContentHelper(mContentHelper);
            mContentLoadLayout.setLayoutManager(mLayoutManager);
            mContentLoadLayout.addItemDecoration(mItemDecoration);
            mContentLoadLayout.setOnItemClickListener(new EasyRecyclerView.OnItemClickListener() {
                @Override
                public void onItemClick(EasyRecyclerView parent, View view, int position, long id) {
                    Friend friend = mContentHelper.getData(position - 1);
                    Intent intent = new Intent(mActivity, UserStatusActivity.class);
                    intent.putExtra(UserStatusActivity.KEY_USER_ID, friend.id);
                    startActivity(intent);
                }
            });

            mContentHelper.refresh();

            return mContentLoadLayout;
        }

        @Override
        public void onQueryTextSubmit(String str) {
            // Switch recommend to search
            mState = STATE_SEARCH;
            try {
                mSearchStr = URLEncoder.encode(str, "UTF-8");
            } catch (UnsupportedEncodingException e) {
                // Empyt
            }

            if (!mContentLoadLayout.isRefresh()) {
                mContentHelper.refresh();
            }
        }

        private class FindFriendsHelper extends ContentLoadLayout.ContentHelper<Friend, RecyclerView.ViewHolder> {
            @Override
            public void getContent(final int index) {
                if (mState == STATE_RECOMMEND) {
                    mClient.getRecommendFriends(new VltClient.OnGetRecommendFriendsListener() {
                        @Override
                        public void onSuccess(Friend[] friends) {
                            onGetContentSuccess(index, friends, true);
                        }

                        @Override
                        public void onFailure(Exception e) {
                            onGetContentFailure(index, e);
                        }
                    });
                } else {
                    mClient.getSearchNotMyFriend(mSearchStr, new VltClient.OnSearchNotMyFriendListener() {
                        @Override
                        public void onSuccess(Friend[] friends) {
                            onGetContentSuccess(index, friends, true);
                        }

                        @Override
                        public void onFailure(Exception e) {
                            onGetContentFailure(index, e);
                        }
                    });
                }
            }

            @Override
            public int getItemCount() {
                return getDataSize() + 1;
            }

            @Override
            public int getItemViewType(int position) {
                if (position == 0)
                    return TYPE_SEPARATOR;
                else
                    return TYPE_FRIEND;
            }

            @Override
            public RecyclerView.ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
                if (viewType == TYPE_SEPARATOR) {
                    return SeparatorTextHolder.createViewHolder(mActivity.getLayoutInflater(), parent);
                } else {
                    return SimpleHolder.createViewHolder(mActivity.getLayoutInflater(), parent);
                }
            }

            @Override
            public void onBindViewHolder(RecyclerView.ViewHolder holder, int position) {
                super.onBindViewHolder(holder, position);

                if (holder.getItemViewType() == TYPE_SEPARATOR) {
                    SeparatorTextHolder stHolder = (SeparatorTextHolder) holder;
                    if (mState == STATE_RECOMMEND) {
                        stHolder.separatorTitle.setText("推荐好友");
                        stHolder.separator.setBackgroundColor(Color.RED);
                    } else {
                        stHolder.separatorTitle.setText("搜索");
                        stHolder.separator.setBackgroundColor(Color.DKGRAY);
                    }
                } else {
                    SimpleHolder sHolder = (SimpleHolder) holder;
                    Friend f = getData(position - 1);
                    Picasso.with(mActivity).load(f.avatar).into(sHolder.avatar);
                    sHolder.owner.setText(f.name);
                    // sHolder.subtitle.setText(Integer.toString(f.level));
                    sHolder.message.setText(f.description);
                    ViewUtils.setVisibility(sHolder.thumb, View.GONE);
                }
            }
        }
    }

    public static class FriendRankingFragment extends LazyFragment {
        private Activity mActivity;
        private VltClient mClient;

        private ContentLoadLayout mContentLoadLayout;

        private FriendRankingHelper mContentHelper;
        private StaggeredGridLayoutManager mLayoutManager;

        private int mStartRank;
        private int mMyPosition;

        @Override
        public View onCreateViewFirst(LayoutInflater inflater,
                ViewGroup container, Bundle savedInstanceState) {
            mActivity = getActivity();
            mClient = VltClient.getInstance(mActivity);

            mContentLoadLayout = new ContentLoadLayout(mActivity);

            mContentHelper = new FriendRankingHelper();
            mLayoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.VERTICAL);
            mContentLoadLayout.setContentHelper(mContentHelper);
            mContentLoadLayout.setLayoutManager(mLayoutManager);
            mContentLoadLayout.setOnItemClickListener(new EasyRecyclerView.OnItemClickListener() {
                @Override
                public void onItemClick(EasyRecyclerView parent, View view, int position, long id) {
                    Friend friend = mContentHelper.getData(position);
                    Intent intent = new Intent(mActivity, UserStatusActivity.class);
                    intent.putExtra(UserStatusActivity.KEY_USER_ID, friend.id);
                    startActivity(intent);
                }
            });

            mContentHelper.refresh();

            return mContentLoadLayout;
        }

        private class FriendRankingHelper extends ContentLoadLayout.ContentHelper<Friend, FriendRankingHolder> {
            @Override
            public void getContent(int index) {
                mClient.myNearbyFriendsRank("point", new VltClient.OnMyNearbyFriendsRankListener() {
                    @Override
                    public void onSuccess(Friend[] friends, int startRank, int myPosition) {
                        mStartRank = startRank;
                        mMyPosition = myPosition;
                        onGetContentSuccess(0, friends, true);
                    }

                    @Override
                    public void onFailure(Exception e) {
                        onGetContentFailure(0, e);
                    }
                });
            }

            @Override
            public int getItemCount() {
                return getDataSize();
            }

            @Override
            public FriendRankingHolder onCreateViewHolder(ViewGroup parent, int viewType) {
                return FriendRankingHolder.createViewHolder(mActivity.getLayoutInflater(), parent);
            }

            @Override
            public void onBindViewHolder(final FriendRankingHolder holder, int position) {
                super.onBindViewHolder(holder, position);

                final Friend f = getData(position);
                if (f.avatar == null) {
                    Picasso.with(mActivity).load(R.drawable.ic_default_avatar).into(holder.avatar);
                    mClient.getUser(f.id, new VltClient.OnGetUserListener() {
                        @Override
                        public void onSuccess(User user) {
                            f.avatar = user.avatar;
                            Picasso.with(mActivity).load(f.avatar).into(holder.avatar);
                        }

                        @Override
                        public void onFailure(Exception e) {
                            // Empty
                        }
                    });
                } else {
                    Picasso.with(mActivity).load(f.avatar).placeholder(R.drawable.ic_default_avatar).into(holder.avatar);
                }

                holder.ranking.setText(Integer.toString(mStartRank + position));
                holder.name.setText(f.name);
                holder.point.setText(Integer.toString(f.point) + " pt"); // TODO
                ViewGroup.LayoutParams lp = holder.avatar.getLayoutParams();
                if (position == mMyPosition) {
                    lp.width = UiUtils.dp2pix(64); // TODO
                    lp.height = UiUtils.dp2pix(64);
                } else {
                    lp.width = UiUtils.dp2pix(48);
                    lp.height = UiUtils.dp2pix(48);
                }
            }
        }
    }

    public static class FriendApplyFragment extends LazyFragment {
        private static final int TYPE_SEPARATOR = 0;
        private static final int TYPE_FRIEND = 1;

        private Activity mActivity;
        private VltClient mClient;

        private ContentLoadLayout mContentLoadLayout;

        private FriendApplyHelper mContentHelper;
        private StaggeredGridLayoutManager mLayoutManager;

        private List<ApplyToMe> mApplyToMeList;
        private List<ApplyFromMe> mApplyFromMeList;

        private int mSeparatorPositionTo = -1;
        private int mSeparatorPositionFrom = -1;

        @Override
        public View onCreateViewFirst(LayoutInflater inflater,
                ViewGroup container, Bundle savedInstanceState) {
            mActivity = getActivity();
            mClient = VltClient.getInstance(mActivity);
            mApplyToMeList = new ArrayList<>();
            mApplyFromMeList = new ArrayList<>();

            mContentLoadLayout = new ContentLoadLayout(mActivity);

            mContentHelper = new FriendApplyHelper();
            mLayoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.VERTICAL);
            mContentLoadLayout.setContentHelper(mContentHelper);
            mContentLoadLayout.setLayoutManager(mLayoutManager);
            mContentLoadLayout.setOnItemClickListener(new EasyRecyclerView.OnItemClickListener() {
                @Override
                public void onItemClick(EasyRecyclerView parent, View view, int position, long id) {
                    int toSize = mApplyToMeList.size();
                    int fromSize = mApplyFromMeList.size();

                    if (mSeparatorPositionTo != -1 && position > mSeparatorPositionTo
                            && position <= mSeparatorPositionTo + toSize) {
                        // Check is in to mApplyToMeList
                        ApplyToMe applyToMe = mApplyToMeList.get(position - mSeparatorPositionTo - 1);
                        Intent intent = new Intent(mActivity, UserStatusActivity.class);
                        intent.putExtra(UserStatusActivity.KEY_USER_ID, applyToMe.fromId);
                        startActivity(intent);
                    } else if (mSeparatorPositionFrom != -1 && position > mSeparatorPositionFrom
                            && position <= mSeparatorPositionFrom + fromSize) {
                        // Check is in to mApplyFromMeList
                        ApplyFromMe applyFromMe = mApplyFromMeList.get(position - mSeparatorPositionFrom - 1);
                        Intent intent = new Intent(mActivity, UserStatusActivity.class);
                        intent.putExtra(UserStatusActivity.KEY_USER_ID, applyFromMe.toId);
                        startActivity(intent);
                    }
                }
            });

            mContentHelper.refresh();

            return mContentLoadLayout;
        }

        private class FriendApplyHelper extends ContentLoadLayout.ContentHelper<Object, RecyclerView.ViewHolder> {

            @Override
            public void getContent(int index) {
                mClient.friendApply(new VltClient.OnFriendApplyListener() {
                    @Override
                    public void onSuccess(ApplyFromMe[] applyFromMes, ApplyToMe[] applyToMes) {
                        mApplyToMeList.clear();
                        mApplyFromMeList.clear();

                        int length;

                        length = applyFromMes.length;
                        for (int i = 0; i < length; i++) {
                            ApplyFromMe applyFromMe = applyFromMes[i];
                            mApplyFromMeList.add(applyFromMe);
                        }

                        length = applyToMes.length;
                        for (int i = 0; i < length; i++) {
                            ApplyToMe applyToMe = applyToMes[i];
                            //if (!applyToMe.hasHandled) {
                                mApplyToMeList.add(applyToMe);
                            //}
                        }

                        if (mApplyFromMeList.size() == 0 && mApplyToMeList.size() == 0) {
                            onGetContentSuccess(0, new Object[0], true);
                        } else {
                            onGetContentSuccess(0, new Object[1], true);
                        }
                    }

                    @Override
                    public void onFailure(Exception e) {
                        mApplyToMeList.clear();
                        mApplyFromMeList.clear();

                        onGetContentFailure(0, e);
                    }
                });
            }

            @Override
            public int getItemCount() {
                int toSize = mApplyToMeList.size();
                int fromSize = mApplyFromMeList.size();

                if (toSize == 0)
                    mSeparatorPositionTo = -1;
                else
                    mSeparatorPositionTo = 0;
                if (fromSize == 0)
                    mSeparatorPositionFrom = -1;
                else
                    mSeparatorPositionFrom = mSeparatorPositionTo + toSize + 1;
                return (toSize == 0 ? 0 : 1) + toSize
                        + (fromSize == 0 ? 0 : 1) + fromSize;
            }

            @Override
            public int getItemViewType(int position) {
                if (position == mSeparatorPositionTo || position == mSeparatorPositionFrom)
                    return TYPE_SEPARATOR;
                else
                    return TYPE_FRIEND;
            }

            @Override
            public RecyclerView.ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
                if (viewType == TYPE_SEPARATOR) {
                    return SeparatorTextHolder.createViewHolder(mActivity.getLayoutInflater(), parent);
                } else {
                    return FriendApplyHolder.createViewHolder(mActivity.getLayoutInflater(), parent);
                }
            }

            @Override
            public void onBindViewHolder(RecyclerView.ViewHolder holder, final int position) {
                super.onBindViewHolder(holder, position);

                if (holder.getItemViewType() == TYPE_SEPARATOR) {
                    SeparatorTextHolder stHolder = (SeparatorTextHolder) holder;
                    if (position == mSeparatorPositionTo) {
                        stHolder.separatorTitle.setText("待处理的请求"); // TODO
                        stHolder.separator.setBackgroundColor(Color.RED);
                    } else {
                        stHolder.separatorTitle.setText("我的请求"); // TODO
                        stHolder.separator.setBackgroundColor(Color.DKGRAY);
                    }
                } else {
                    final FriendApplyHolder faHolder = (FriendApplyHolder) holder;
                    int toSize = mApplyToMeList.size();
                    int fromSize = mApplyFromMeList.size();

                    if (mSeparatorPositionTo != -1 && position > mSeparatorPositionTo
                            && position <= mSeparatorPositionTo + toSize) {
                        // Check is in to mApplyToMeList
                        final ApplyToMe applyToMe = mApplyToMeList.get(position - mSeparatorPositionTo - 1);
                        Picasso.with(mActivity).load(applyToMe.avatar).into(faHolder.avatar);
                        faHolder.name.setText(applyToMe.name);
                        faHolder.affiliation.setText(applyToMe.comment); // TODO
                        faHolder.message.setText(applyToMe.comment);
                        View.OnClickListener listener = new View.OnClickListener() {
                            @Override
                            public void onClick(View v) {
                                boolean isAccept = false;
                                String comment = "我不想做你的好朋友";
                                if (v == faHolder.accept) {
                                    isAccept = true;
                                    comment = "让我们做好朋友吧";
                                }
                                mClient.respondFriendApply(applyToMe.fromId, isAccept, comment, new VltClient.OnRespondFriendApplyListener() {
                                    @Override
                                    public void onSuccess() {
                                        mApplyToMeList.remove(applyToMe);
                                        notifyDataSetChanged();
                                        Toast.makeText(mActivity, "操作大成功", Toast.LENGTH_SHORT).show();
                                    }

                                    @Override
                                    public void onFailure(Exception e) {
                                        Toast.makeText(mActivity, "操作大失败", Toast.LENGTH_SHORT).show();
                                    }
                                });
                            }
                        };

                        faHolder.accept.setOnClickListener(listener);
                        faHolder.refuse.setOnClickListener(listener);
                        ViewUtils.setVisibility(faHolder.accept, View.VISIBLE);
                        ViewUtils.setVisibility(faHolder.refuse, View.VISIBLE);
                        ViewUtils.setVisibility(faHolder.pending, View.GONE);

                    } else if (mSeparatorPositionFrom != -1 && position > mSeparatorPositionFrom
                            && position <= mSeparatorPositionFrom + fromSize) {
                        // Check is in to mApplyFromMeList
                        ApplyFromMe applyFromMe = mApplyFromMeList.get(position - mSeparatorPositionFrom - 1);
                        Picasso.with(mActivity).load(R.drawable.ic_default_avatar).into(faHolder.avatar);
                        faHolder.name.setText(applyFromMe.name);
                        faHolder.affiliation.setText("hhhhh"); // TODO
                        faHolder.message.setText(applyFromMe.comment);
                        ViewUtils.setVisibility(faHolder.accept, View.GONE);
                        ViewUtils.setVisibility(faHolder.refuse, View.GONE);
                        ViewUtils.setVisibility(faHolder.pending, View.VISIBLE);
                    }
                }
            }
        }
    }

    public static interface OnQueryTextSubmitListener {
        public void onQueryTextSubmit(String str);
    }
}
