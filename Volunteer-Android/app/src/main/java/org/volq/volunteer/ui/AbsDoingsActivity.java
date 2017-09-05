package org.volq.volunteer.ui;

import android.content.Intent;
import android.content.res.Resources;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.support.v4.app.FragmentManager;
import android.support.v4.app.FragmentPagerAdapter;
import android.support.v7.widget.StaggeredGridLayoutManager;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;

import com.hippo.util.UiUtils;
import com.hippo.widget.ContentLoadLayout;
import com.hippo.widget.recyclerview.EasyRecyclerView;
import com.hippo.widget.recyclerview.MarginItemDecoration;
import com.squareup.picasso.Picasso;

import org.volq.volunteer.R;
import org.volq.volunteer.app.LazyFragment;
import org.volq.volunteer.cardview.DoingHolder;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.Doing;
import org.volq.volunteer.util.VltUtils;

public abstract class AbsDoingsActivity extends ViewPagerActivity {
    private static final String KEY_STATE = "state";

    public static final int POSITION_PREPARING = 0;
    public static final int POSITION_ONGOING = 1;
    public static final int POSITION_CLOSED = 2;
    public static final int POSITION_ALL = 3;
    public static final int POSITION_SUM = 4;

    private DoingsAdapter mAdapter;

    public abstract int getDrawerListPosition();

    protected abstract void getData(int index, int state, ContentLoadLayout.ContentHelper helper);

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        mAdapter = new DoingsAdapter(getSupportFragmentManager());
        setViewPageAdapter(mAdapter);

        setDrawerListActivatedPosition(getDrawerListPosition());
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        if (enableSearchAction()) {
            MenuInflater inflater = getMenuInflater();
            inflater.inflate(R.menu.search_fake, menu);
        }
        return true;
    }

    protected abstract boolean enableSearchAction();

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
                c = getString(R.string.title_closed);
                break;
            case POSITION_ALL:
                c = getString(R.string.title_all);
                break;
        }
        return c;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
        case R.id.action_search:
            Intent intent = new Intent(this, SearchActivity.class);
            startActivity(intent);
            return true;
        default:
            return super.onOptionsItemSelected(item);
        }
    }

    public class DoingsAdapter extends FragmentPagerAdapter {
        public DoingsAdapter(FragmentManager fm) {
            super(fm);
        }

        @Override
        public Fragment getItem(int i) {
            Fragment fragment;
            switch (i) {
            default:
            case POSITION_PREPARING:
                fragment = new DoingsFragment();
                break;
            case POSITION_ONGOING:
                fragment = new DoingsFragment();
                break;
            case POSITION_CLOSED:
                fragment = new DoingsFragment();
                break;
            case POSITION_ALL:
                fragment = new DoingsFragment();
                break;
            }
            Bundle args = new Bundle();
            args.putInt(KEY_STATE, i);
            fragment.setArguments(args);

            return fragment;
        }

        @Override
        public int getCount() {
            return POSITION_SUM;
        }

        @Override
        public CharSequence getPageTitle(int position) {
            return AbsDoingsActivity.this.getPageTitle(position);
        }
    }

    public static class DoingsFragment extends LazyFragment {

        private AbsDoingsActivity mActivity;
        private Resources mResources;

        private ContentLoadLayout mContentLoadLayout;

        private DoingsHelper mContentHelper;
        private StaggeredGridLayoutManager mLayoutManager;
        private MarginItemDecoration mItemDecoration;

        private int mState;

        @Override
        public View onCreateViewFirst(LayoutInflater inflater,
                ViewGroup container, Bundle savedInstanceState) {
            mState = getArguments().getInt(KEY_STATE);

            mActivity = (AbsDoingsActivity) getActivity();
            mResources = mActivity.getResources();

            mContentLoadLayout = new ContentLoadLayout(mActivity);
            View rootView = mContentLoadLayout;

            mContentHelper = new DoingsHelper();
            mLayoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.VERTICAL);
            mItemDecoration = new MarginItemDecoration(UiUtils.dp2pix(8)); // TODO
            mContentLoadLayout.setContentHelper(mContentHelper);
            mContentLoadLayout.setLayoutManager(mLayoutManager);
            mContentLoadLayout.addItemDecoration(mItemDecoration);
            mContentLoadLayout.setOnItemClickListener(new EasyRecyclerView.OnItemClickListener() {
                @Override
                public void onItemClick(EasyRecyclerView parent, View view, int position, long id) {
                    Intent intent = new Intent(getActivity(), DoingActivity.class);
                    intent.putExtra(DoingActivity.KEY_ID, mContentHelper.getData(position).Id);
                    getActivity().startActivity(intent);
                }
            });

            mContentHelper.refresh();

            return rootView;
        }

        private class DoingsHelper extends ContentLoadLayout.ContentHelper<Doing, DoingHolder> {

            @Override
            public void getContent(int index) {
                mActivity.getData(index, mState, this);
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
                Picasso.with(mActivity).load(d.Cover).error(R.drawable.ic_default_doing).into(holder.thumb);
                holder.title.setText(d.Name);
                holder.state.setText(VltUtils.getDoingStatusString(mActivity, d.Status));
                holder.time.setText(VltClient.getInstance(mActivity).formatDateToHuman(d.StartTime));
                holder.browse.setText(Integer.toString(d.VolunteerViewedTime));
                holder.favorite.setText(Integer.toString(d.VolunteerFavoritedTime));
                holder.join.setText(Integer.toString(d.HasSignedInVolunteerNumber));

                if (d.hasViewed) {
                    holder.browse.setCompoundDrawablesWithIntrinsicBounds(
                            mResources.getDrawable(R.drawable.ic_small_visibility_color),
                            null,
                            null,
                            null
                    );
                    holder.browse.setTextColor(mResources.getColor(R.color.material_pink_500));
                } else {
                    holder.browse.setCompoundDrawablesWithIntrinsicBounds(
                            mResources.getDrawable(R.drawable.ic_small_visibility),
                            null,
                            null,
                            null
                    );
                    holder.browse.setTextColor(mResources.getColor(R.color.primary_text_light));
                }

                if (d.hasFavorited) {
                    holder.favorite.setCompoundDrawablesWithIntrinsicBounds(
                            mResources.getDrawable(R.drawable.ic_small_favorite_color),
                            null,
                            null,
                            null
                    );
                    holder.favorite.setTextColor(mResources.getColor(R.color.material_pink_500));
                } else {
                    holder.favorite.setCompoundDrawablesWithIntrinsicBounds(
                            mResources.getDrawable(R.drawable.ic_small_favorite),
                            null,
                            null,
                            null
                    );
                    holder.favorite.setTextColor(mResources.getColor(R.color.primary_text_light));
                }

                if (d.hasSignined) {
                    holder.join.setCompoundDrawablesWithIntrinsicBounds(
                            mResources.getDrawable(R.drawable.ic_small_games_color),
                            null,
                            null,
                            null
                    );
                    holder.join.setTextColor(mResources.getColor(R.color.material_pink_500));
                } else {
                    holder.join.setCompoundDrawablesWithIntrinsicBounds(
                            mResources.getDrawable(R.drawable.ic_small_games),
                            null,
                            null,
                            null
                    );
                    holder.join.setTextColor(mResources.getColor(R.color.primary_text_light));
                }
            }
        }
    }
}
