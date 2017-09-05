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
import android.support.v4.view.MenuItemCompat;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.support.v7.widget.SearchView;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.TextView;

import com.hippo.util.ArrayUtils;
import com.hippo.util.UiUtils;
import com.hippo.widget.AutoWrapLayout;
import com.hippo.widget.recyclerview.EasyRecyclerView;

import org.volq.volunteer.R;
import org.volq.volunteer.SimpleSuggestion;
import org.volq.volunteer.SimpleSuggestionProvider;
import org.volq.volunteer.client.VltClient;

import java.util.ArrayList;
import java.util.List;

public class SearchActivity extends AbsActionBarActivity
        implements View.OnFocusChangeListener, SearchView.OnQueryTextListener,
        VltClient.OnGetHotTagsListener {

    private SimpleSuggestion mSimpleSuggestion;

    private EasyRecyclerView mEasyRecyclerView;

    private SearchAdapter mAdapter;
    private LinearLayoutManager mLayoutManager;

    private List<String> mTagArray;
    private List<String> mSearchItemArray;

    @Override
    public void onFocusChange(View v, boolean hasFocus) {
        finish();
    }

    @Override
    public boolean onQueryTextChange(String arg0) {
        return false;
    }

    @Override
    public boolean onQueryTextSubmit(String arg0) {
        mSimpleSuggestion.saveRecentQuery(arg0, null);
        mSearchItemArray.clear();
        ArrayUtils.addAll(mSearchItemArray, mSimpleSuggestion.getQueries());
        mAdapter.notifyDataSetChanged();

        startResultActivity(arg0);

        return true;
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE);
        mSimpleSuggestion = new SimpleSuggestion(this,
                SimpleSuggestionProvider.AUTHORITY, SimpleSuggestionProvider.MODE);

        mEasyRecyclerView = new EasyRecyclerView(this);
        setContentView(mEasyRecyclerView);

        mAdapter = new SearchAdapter();
        mLayoutManager = new LinearLayoutManager(this, LinearLayoutManager.VERTICAL, false);
        //mEasyRecyclerView.setHasFixedSize(true);
        mEasyRecyclerView.setPadding(UiUtils.dp2pix(8), UiUtils.dp2pix(8), UiUtils.dp2pix(8), UiUtils.dp2pix(8)); // TODO
        mEasyRecyclerView.setAdapter(mAdapter);
        mEasyRecyclerView.setLayoutManager(mLayoutManager);

        mTagArray = new ArrayList<String>();
        mSearchItemArray = new ArrayList<String>();

        ArrayUtils.addAll(mSearchItemArray, mSimpleSuggestion.getQueries());

        VltClient.getInstance(this).getHotTags(this);

        mEasyRecyclerView.setOnItemClickListener(new EasyRecyclerView.OnItemClickListener() {
            @Override
            public void onItemClick(EasyRecyclerView parent, View view, int position, long id) {
                if (position != 0) {
                    startResultActivity((String) ((TextView) view).getText());
                }
            }
        });
    }

    private void startResultActivity(String key) {
        Intent intent = new Intent(SearchActivity.this, SearchDoingActivity.class);
        intent.putExtra(SearchDoingActivity.KEY_KEYWORD, key);
        startActivity(intent);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.search, menu);
        MenuItem searchItem = menu.findItem(R.id.action_search);
        SearchView searchView = (SearchView) MenuItemCompat.getActionView(searchItem);
        searchView.setIconifiedByDefault(false);
        searchView.setOnQueryTextListener(this);
        return true;
    }

    @Override
    public void onSuccess(String[] tags) {
        mTagArray.clear();
        ArrayUtils.addAll(mTagArray, tags);
        mAdapter.notifyDataSetChanged();
    }

    @Override
    public void onFailure(Exception e) {
        // TODO
    }

    private static class SimpleHolder extends RecyclerView.ViewHolder {
        public SimpleHolder(View itemView) {
            super(itemView);
        }
    }


    private class SearchAdapter extends EasyRecyclerView.Adapter<RecyclerView.ViewHolder> {
        private static final int TYPE_TAG = 0;
        private static final int TYPE_SEARCH = 1;

        @Override
        public int getItemCount() {
            return mSearchItemArray.size() + 1;
        }

        @Override
        public final int getItemViewType(int position) {
            if (position == 0)
                return TYPE_TAG;
            else
                return TYPE_SEARCH;
        }

        @Override
        public RecyclerView.ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
            if (viewType == TYPE_TAG) {
                AutoWrapLayout awLayout = new AutoWrapLayout(SearchActivity.this);
                return new SimpleHolder(awLayout);
            } else {
                TextView tv = (TextView) getLayoutInflater().inflate(R.layout.search_item, parent, false);
                return new SimpleHolder(tv);
            }
        }

        @Override
        public void onBindViewHolder(RecyclerView.ViewHolder holder, int position) {
            super.onBindViewHolder(holder, position);

            if (holder.getItemViewType() == TYPE_TAG) {
                AutoWrapLayout awLayout = (AutoWrapLayout) holder.itemView;
                awLayout.removeAllViews();
                int count = mTagArray.size();
                for (int i = 0; i < count; i++) {
                    getLayoutInflater().inflate(R.layout.tag_item, awLayout);
                    View view = awLayout.getChildAt(i);
                    ((TextView) view).setText(mTagArray.get(i));
                    final int index = i;
                    view.setOnClickListener(new View.OnClickListener() {
                        @Override
                        public void onClick(View v) {
                            startResultActivity(mTagArray.get(index));
                        }
                    });
                }


            } else {
                TextView tv = (TextView) holder.itemView;
                tv.setText(mSearchItemArray.get(position - 1));
            }
        }
    }
}
