package org.volq.volunteer.ui;

import android.content.Intent;
import android.os.Bundle;
import android.support.v7.widget.StaggeredGridLayoutManager;
import android.view.View;
import android.view.ViewGroup;

import com.hippo.util.TextUtils;
import com.hippo.util.UiUtils;
import com.hippo.widget.ContentLoadLayout;
import com.hippo.widget.recyclerview.EasyRecyclerView;
import com.hippo.widget.recyclerview.MarginItemDecoration;
import com.squareup.picasso.Picasso;

import org.volq.volunteer.R;
import org.volq.volunteer.account.VltAccountStore;
import org.volq.volunteer.cardview.DoingHolder;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.Doing;
import org.volq.volunteer.util.VltUtils;

public class FavoriteDoingsActivity extends DrawerActivity implements
        VltAccountStore.OnChangeAccountListener {

    private VltClient mClient;

    private ContentLoadLayout mContentLoadLayout;

    private DoingsHelper mContentHelper;
    private StaggeredGridLayoutManager mLayoutManager;
    private MarginItemDecoration mItemDecoration;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        mClient = VltClient.getInstance(this);

        mContentLoadLayout = new ContentLoadLayout(this);
        setCustomView(mContentLoadLayout);

        mContentHelper = new DoingsHelper();
        mLayoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.VERTICAL);
        mItemDecoration = new MarginItemDecoration(UiUtils.dp2pix(8)); // TODO
        mContentLoadLayout.setContentHelper(mContentHelper);
        mContentLoadLayout.setLayoutManager(mLayoutManager);
        mContentLoadLayout.addItemDecoration(mItemDecoration);
        mContentLoadLayout.setOnItemClickListener(new EasyRecyclerView.OnItemClickListener() {
            @Override
            public void onItemClick(EasyRecyclerView parent, View view, int position, long id) {
                Intent intent = new Intent(FavoriteDoingsActivity.this, DoingActivity.class);
                intent.putExtra(DoingActivity.KEY_ID, mContentHelper.getData(position).Id);
                startActivity(intent);
            }
        });

        mContentHelper.refresh();

        VltAccountStore.getInstance(this).addOnChangeAccountListener(this);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();

        VltAccountStore.getInstance(this).removeOnChangeAccountListener(this);
    }

    @Override
    public void onAddAccount() {
        // Empty
    }

    @Override
    public void onRemoveAccount() {
        finish();
    }

    private class DoingsHelper extends ContentLoadLayout.ContentHelper<Doing, DoingHolder> {
        @Override
        public void getContent(final int index) {
            mClient.getFavoriteDoings(TextUtils.STRING_EMPTY, VltClient.DOING_STAGE_ALL, index, new VltClient.OnGetFavoriteDoingsListener() {
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
            return DoingHolder.createViewHolder(getLayoutInflater(), parent);
        }

        @Override
        public void onBindViewHolder(DoingHolder holder, int position) {
            super.onBindViewHolder(holder, position);

            Doing d = getData(position);
            Picasso.with(FavoriteDoingsActivity.this).load(d.Cover).into(holder.thumb);
            holder.title.setText(d.Name);
            holder.state.setText(VltUtils.getDoingStatusString(FavoriteDoingsActivity.this, d.Status));
            holder.state.setTextColor(FavoriteDoingsActivity.this.getResources().getColor(R.color.material_blue_500));
            holder.time.setText(VltClient.getInstance(FavoriteDoingsActivity.this).formatDateToHuman(d.StartTime));
            holder.browse.setText(Integer.toString(d.VolunteerViewedTime));
            holder.favorite.setText(Integer.toString(d.VolunteerFavoritedTime));
            holder.join.setText(Integer.toString(d.HasSignedInVolunteerNumber));
        }
    }
}
