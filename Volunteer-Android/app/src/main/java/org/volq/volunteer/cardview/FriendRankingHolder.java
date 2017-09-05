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

package org.volq.volunteer.cardview;

import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import org.volq.volunteer.R;

import de.hdodenhof.circleimageview.CircleImageView;

public class FriendRankingHolder extends RecyclerView.ViewHolder {

    public TextView ranking;
    public CircleImageView avatar;
    public TextView name;
    public TextView point;

    public FriendRankingHolder(View itemView) {
        super(itemView);

        ranking = (TextView) itemView.findViewById(R.id.ranking);
        avatar = (CircleImageView) itemView.findViewById(R.id.avatar);
        name = (TextView) itemView.findViewById(R.id.name);
        point = (TextView) itemView.findViewById(R.id.point);
    }

    public static FriendRankingHolder createViewHolder(LayoutInflater inflater, ViewGroup parent) {
        return new FriendRankingHolder(inflater.inflate(R.layout.friend_ranking_card_view, parent, false));
    }

}
